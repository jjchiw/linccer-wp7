using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Device.Location;
using System.Threading;
using LinccerApp.WindowsPhone.Tasks;
using NorthernLights;
using Microsoft.Phone.Tasks;
using TombstoneHelper;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using LinccerApp.WindowsPhone.Helpers;
using LinccerApi.WindowsPhone;
using LinccerApp.WindowsPhone.Controls;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.Media;

namespace LinccerApp.WindowsPhone
{
	public partial class MainPage : PhoneApplicationPage
	{
		GeoCoordinateWatcher watcher;
		LinccerTasks linccerTasks;
		bool trackingOn = false;
		LinccerContentCallback _contentCallback;
		LinccerContentCallback _contentDebugCallback;
		FileCacheGetCallback _fileContentCallback;

		private ProgressOverlay _progress;
		private ProgressOverlay Progress
		{
			get
			{
				if (_progress == null)
				{
					_progress = new ProgressOverlay();
				}
				return _progress;
			}
		}

		// Constructor
		public MainPage()
		{
			InitializeComponent();

			linccerTasks = new LinccerTasks();

			watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High); // using high accuracy;
			watcher.MovementThreshold = 10.0f; // meters of change before "PositionChanged"
			// wire up event handlers
			watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
			watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);

			// start up LocServ in bg; watcher_StatusChanged will be called when complete.
			new Thread(startLocServInBackground).Start();
			StatusTextBlock.Text = "Starting Location Service...";

			LoadAdminDebugHooks();

			_contentCallback = new LinccerContentCallback(LinccerContentCallbackResponse);
			_contentDebugCallback = new LinccerContentCallback(LinccerContentDebugCallbackResponse);
			_fileContentCallback = new FileCacheGetCallback(FileCacheGetCallbackResponse);

		}

		private void LoadAdminDebugHooks()
		{
			ExceptionContainer exception = LittleWatson.GetPreviousException();

			if (exception != null)
			{
				Deployment.Current.Dispatcher.BeginInvoke(() =>
				{
					EmailComposeTask email = new EmailComposeTask();
					email.To = "jjchiw@gmail.com";
					email.Subject = "LinccerApp.WindowsPhone: auto-generated problem report";
					email.Body = exception.Message + System.Environment.NewLine + exception.StackTrace;
					email.Show();
				});
			}

			// Show graphics profiling information while debugging.
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// Display the current frame rate counters.
				Application.Current.Host.Settings.EnableFrameRateCounter = true;

				// Display the metro grid helper.
				MetroGridHelper.IsVisible = true;

				UpdateButton.Visibility = Visibility.Visible;
				StatusTextBlock.Visibility = Visibility.Visible;
				ResponseContentTextBlock.Visibility = Visibility.Visible;
				
			}
		}

		protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			this.SaveState(e);
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			this.RestoreState();
		}

		void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
		{
			switch (e.Status)
			{
				case GeoPositionStatus.Disabled:
					trackingOn = false;
					// The Location Service is disabled or unsupported.
					// Check to see if the user has disabled the Location Service.
					if (watcher.Permission == GeoPositionPermission.Denied)
					{
						// The user has disabled the Location Service on their device.
						StatusTextBlock.Text = "You have disabled Location Service.";
					}
					else
					{
						StatusTextBlock.Text = "Location Service is not functioning on this device.";
					}
					break;
				case GeoPositionStatus.Initializing:
					trackingOn = true;
					StatusTextBlock.Text = "Location Service is retrieving data...";
					// The Location Service is initializing.
					break;
				case GeoPositionStatus.NoData:
					// The Location Service is working, but it cannot get location data.
					StatusTextBlock.Text = "Location data is not available.";
					break;
				case GeoPositionStatus.Ready:
					trackingOn = true;
					// The Location Service is working and is receiving location data.
					StatusTextBlock.Text = "Location data is available.";
					break;
			}
		}

		void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
		{
			// update the Map if the user has asked to be tracked.
			if (trackingOn)
			{
				UpdateButton_Click(null, null);
			}
		}

		void startLocServInBackground()
		{
			watcher.TryStart(true, TimeSpan.FromMilliseconds(60000));
		}

		private void UpdateButton_Click(object sender, EventArgs e)
		{
			linccerTasks.UpdateLocation(watcher.Position.Location, (content) =>
			{
				Dispatcher.BeginInvoke(() => ResponseContentTextBlock.Text = content);
			});
		}

		private void PhotoIconButton_Click(object sender, EventArgs e)
		{
			ContentTextBox.Visibility = Visibility.Collapsed;
			var selectphoto = new PhotoChooserTask();
			selectphoto.Completed += new EventHandler<PhotoResult>(selectphoto_Completed);
			selectphoto.Show();
		}

		void selectphoto_Completed(object sender, PhotoResult e)
		{
			if (e.TaskResult == TaskResult.OK)
			{
				var imageUri = new Uri(e.OriginalFileName);
				ImageSend.Source = new BitmapImage(imageUri);
				ImageSend.Visibility = Visibility.Visible;
			}
		}

		private void MessageIconButton_Click(object sender, EventArgs e)
		{
			ImageSend.Visibility = Visibility.Collapsed;
			ContentTextBox.Visibility = Visibility.Visible;
		}

		private void SendToMany_Click(object sender, EventArgs e)
		{
			ShowProgressLayoutSend();

			if (ImageSend.Visibility == Visibility.Visible)
			{
				var bmp = ImageSend.Source as BitmapImage;

				var imageBytes = bmp.ConvertToBytes();

				linccerTasks.SendDataToMany(imageBytes, _contentDebugCallback);
			}
			else if (ContentTextBox.Visibility == Visibility.Visible)
			{
				linccerTasks.SendTextToMany(ContentTextBox.Text, _contentDebugCallback);
			}
		}

		private void SendToOne_Click(object sender, EventArgs e)
		{
			ShowProgressLayoutSend();
			
			if (ImageSend.Visibility == Visibility.Visible)
			{
				var bmp = ImageSend.Source as BitmapImage;

				var imageBytes = bmp.ConvertToBytes();

				linccerTasks.SendDataToOne(imageBytes, _contentDebugCallback);
			}
			else if (ContentTextBox.Visibility == Visibility.Visible)
			{
				linccerTasks.SendTextToOne(ContentTextBox.Text, _contentDebugCallback);
			}
		}

		private void ShowProgressLayoutSend()
		{
			Progress.SetTextBlockStatus("Sending...");
			Progress.Show();
		}

		private void ShowProgressLayoutDownloading()
		{
			Progress.SetTextBlockStatus("Downloading...");
			Progress.Show();
		}

		private void ReceiveToOne_Click(object sender, EventArgs e)
		{
			ShowProgressLayoutDownloading();
			linccerTasks.ReceiveFromOne(_contentCallback, _fileContentCallback);
		}

		private void ReceiveToMany_Click(object sender, EventArgs e)
		{
			ShowProgressLayoutDownloading();
			linccerTasks.ReceiveFromMany(_contentCallback, _fileContentCallback);
		}

		private void LinccerContentCallbackResponse(string content)
		{
			Dispatcher.BeginInvoke(() =>
			{
				if (content != null)
					ContentTextBox.Text = content;
				else
					MessageBox.Show("No sender found :(");

				Progress.Hide();
			});
		}

		private void LinccerContentDebugCallbackResponse(string content)
		{
			Dispatcher.BeginInvoke(() => 
				{
					ResponseContentTextBlock.Text = content;
					Progress.Hide();
				});
		}

		private void FileCacheGetCallbackResponse(byte[] data)
		{
			Dispatcher.BeginInvoke(() =>
			{
				try
				{
					ImageSend.Visibility = Visibility.Visible;
					ContentTextBox.Visibility = Visibility.Collapsed;
					
					BitmapImage bitmapImage = new BitmapImage();
					MemoryStream ms = new MemoryStream(data);
					bitmapImage.SetSource(ms);
					ImageSend.Source = bitmapImage;

					Progress.Hide();
				}
				catch (Exception e)
				{
					MessageBox.Show("Could not download the file");
				}
			});
		}

		private void SaveImage_Click(object sender, RoutedEventArgs e)
		{
			var bmp = ImageSend.Source as BitmapImage;
			MediaLibrary mediaLibrary = new MediaLibrary();
			mediaLibrary.SavePicture("linccer.jpg", bmp.ConvertToBytes());
		}

		private void AboutMenuItem_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
		}
	}
}