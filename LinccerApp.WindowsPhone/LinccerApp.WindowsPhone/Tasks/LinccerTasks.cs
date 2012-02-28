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
using LinccerApi;
using Linccerwp7.Tos;
using System.Device.Location;

namespace Linccerwp7.Tasks
{
	public class LinccerTasks
	{
		private ClientConfig _config;
		private Linccer _linccer;

		public LinccerTasks()
		{
			this._config = new ClientConfig("C# Hoccer Demo");
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
			this._linccer.Gps = new LocationInfo { Latitude = 41.386843, Longitude = 2.165176, Accuracy = 1000 };
			this._linccer.SubmitEnvironment(callback);
		}

		public void Send(string content, LinccerContentCallback callback)
		{
			// inialize filecache for temporary up- and downloading large files (not used jet)
			var cache = new FileCache();
			cache.Config = this._config;

			// create a plain message
			Hoc hoc = new Hoc();
			hoc.DataList.Add(
				new HocData { Content = content, Type = "text/plain" }
			);

			// share it 1:1, in the Hoccer mobile App, you need to perform a drag in
			// gesture to receive the message  (one-to-many is throw/catch)
			this._linccer.Share("one-to-one", hoc, callback);
		}

		public void Receive()
		{

			// inialize filecache for temporary up- and downloading large files (not used jet)
			var cache = new FileCache();
			cache.Config = this._config;

			// receive 1:1, in the Hoccer mobile App, you need to perform a drag out
				// gesture to send something to this client (one-to-many is throw/catch)
			this._linccer.Receive<Hoc>("one-to-one", (hoc) =>
			{
				if (hoc == null)
					System.Console.WriteLine("no sender found");
				else
					System.Console.WriteLine(hoc);
			});
		}

		
	}
}
