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

namespace LinccerApp.WindowsPhone
{
	public partial class MainPage : PhoneApplicationPage
	{
		GeoCoordinateWatcher watcher;
		LinccerTasks linccerTasks;
		bool trackingOn = false;

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

		}

		private void SendButton_Click(object sender, EventArgs e)
		{
			linccerTasks.Send(ContentTextBox.Text, (content) =>
				{
					Dispatcher.BeginInvoke(() => ResponseContentTextBlock.Text = content);
				});
		}

		private void ReceiveButton_Click(object sender, EventArgs e)
		{
			linccerTasks.Receive((content) =>
			{
				Dispatcher.BeginInvoke(() => ResponseContentTextBlock.Text = content);
			});
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
	}
}