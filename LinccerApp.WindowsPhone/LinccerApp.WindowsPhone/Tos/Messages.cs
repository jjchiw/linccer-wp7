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
using System.Collections.ObjectModel;

namespace LinccerApp.WindowsPhone.Tos
{
	public enum MessageSide
	{
		Me,
		World
	}

	/// <summary>
	/// An SMS message
	/// </summary>
	public class Message
	{
		public string Text { get; set; }

		public DateTime Timestamp { get; set; }

		public MessageSide Side { get; set; }
	}

	/// <summary>
	/// A collection of messages
	/// </summary>
	public class MessageCollection : ObservableCollection<Message>
	{
	}
}
