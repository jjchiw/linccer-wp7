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
using Newtonsoft.Json;

namespace LinccerApp.WindowsPhone.Tos
{
	public class UpdateLocationResponse
	{
		[JsonProperty("coordinates")]
		public Coordinates Coordinates { get; set; }

		[JsonProperty("wifi")]
		public Wifi Wifi { get; set; }
		
		[JsonProperty("quality")]
		public int Quality { get; set; }

		[JsonProperty("devices")]
		public int Devices { get; set; }
	}

	public class Coordinates
	{
		[JsonProperty("quality")]
		public int Quality { get; set; }

		[JsonProperty("info")]
		public string Info { get; set; }
	}

	public class Wifi
	{
		[JsonProperty("quality")]
		public int Quality { get; set; }

		[JsonProperty("info")]
		public string Info { get; set; }
	}
}
