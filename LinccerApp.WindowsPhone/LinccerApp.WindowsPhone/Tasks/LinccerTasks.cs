using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using LinccerApi.WindowsPhone;
using System.Device.Location;
using System.Linq;
using System.IO;

namespace LinccerApp.WindowsPhone.Tasks
{
	public class LinccerTasks
	{
		private ClientConfig _config;
		private Linccer _linccer;
		private const int TIMEOUT = 60;

		public LinccerTasks()
		{
			this._config = new ClientConfig("Windows Phone Hoccer Demo");
			this._config.UseProductionServers(); // enables communication to real Hoccer Clients (iOS & Android)

			this._linccer = new Linccer();
			this._linccer.Config = this._config;

			
		}

		public void UpdateLocation(GeoCoordinate location, LinccerContentCallback callback)
		{
			// set geo position (must be changed to work on other locations than Molkenmarkt 2 in Berlin, Germany)
			//41.38690, 2.16532
			//41.386843, 2.165176
			//this._linccer.Gps = new LocationInfo { Latitude = location.Latitude, Longitude = location.Longitude, Accuracy = 1000 };
			//41.383164, 2.131466

			this._linccer.Gps = new LocationInfo { Latitude = 41.386843, Longitude = 2.165176, Accuracy = 1000 };
			this._linccer.SubmitEnvironment(callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <param name="mode">one-to-many / one-to-one</param>
		/// <param name="callback"></param>
		private void SendText(string content, SendMode mode, LinccerContentCallback callback)
		{
			// create a plain message
			Hoc hoc = new Hoc();
			hoc.DataList.Add(
				new HocData { Content = content, Type = "text/plain" }
			);

			// share it 1:1, in the Hoccer mobile App, you need to perform a drag in
			// gesture to receive the message  (one-to-many is throw/catch)
			var stringMode = SendModeString.ConvertSendModeToString(mode);
			this._linccer.Share(stringMode, hoc, callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">one-to-many / one-to-one</param>
		/// <param name="callback"></param>
		private void SendData(byte[] data, SendMode mode, LinccerContentCallback callback)
		{

			// inialize filecache for temporary up- and downloading large files
			var cache = new FileCache();
			cache.Config = this._config;

			cache.Store(data, TIMEOUT, (uri) =>
			{
				Hoc hoc = new Hoc();
				hoc.DataList.Add(
					new HocData { Uri = uri }
				);

				var stringMode = SendModeString.ConvertSendModeToString(mode);
				// share it 1:1, in the Hoccer mobile App, you need to perform a drag in
				// gesture to receive the message  (one-to-many is throw/catch)
				this._linccer.Share(stringMode, hoc, callback);
			});
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <param name="mode">one-to-many / one-to-one</param>
		/// <param name="callback"></param>
		public void SendTextToOne(string content, LinccerContentCallback callback)
		{
			SendText(content, SendMode.OneToOne, callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">one-to-many / one-to-one</param>
		/// <param name="callback"></param>
		public void SendDataToOne(byte[] data, LinccerContentCallback callback)
		{
			SendData(data, SendMode.OneToOne, callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="content"></param>
		/// <param name="mode">one-to-many / one-to-one</param>
		/// <param name="callback"></param>
		public void SendTextToMany(string content,  LinccerContentCallback callback)
		{
			SendText(content, SendMode.OneToMany, callback);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="mode">one-to-many / one-to-one</param>
		/// <param name="callback"></param>
		public void SendDataToMany(byte[] data, LinccerContentCallback callback)
		{
			SendData(data, SendMode.OneToMany, callback);
		}

		private void Receive(SendMode mode, LinccerContentCallback contentCallback, FileCacheGetCallback fileCallback)
		{
			var stringMode = SendModeString.ConvertSendModeToString(mode);
			// receive 1:1, in the Hoccer mobile App, you need to perform a drag out
			// gesture to send something to this client (one-to-many is throw/catch)
			this._linccer.Receive<Hoc>(stringMode, (hoc) =>
			{
				if (hoc == null)
				{
					contentCallback(null);
				}
				else
				{
					if(hoc.DataList.Any(x => x.Type == "text/plain"))
					{
						contentCallback(String.Join(",", hoc.DataList.Where(x => x.Type == "text/plain").Select(x => x.Content)));
						return;
					}

					var data = hoc.DataList.FirstOrDefault(x => x.Uri != string.Empty);

					//// inialize filecache for temporary up- and downloading large files
					var cache = new FileCache();
					cache.Config = this._config;
					cache.Fetch(data.Uri, fileCallback);
				}
			});
		}

		public void ReceiveFromMany( LinccerContentCallback contentCallback, FileCacheGetCallback fileCallback)
		{
			Receive(SendMode.OneToMany, contentCallback, fileCallback);
		}

		public void ReceiveFromOne( LinccerContentCallback contentCallback, FileCacheGetCallback fileCallback)
		{
			Receive(SendMode.OneToOne, contentCallback, fileCallback);
		}

	}
}
