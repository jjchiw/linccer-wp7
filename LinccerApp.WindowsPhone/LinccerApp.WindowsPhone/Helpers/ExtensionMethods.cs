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
using System.IO;
using System.Windows.Media.Imaging;

namespace LinccerApp.WindowsPhone.Helpers
{
	public static class ExtensionMethods
	{
		public static byte[] ConvertToBytes(this BitmapImage bitmapImage)
		{
			byte[] data = null;
			using (MemoryStream stream = new MemoryStream())
			{
				WriteableBitmap wBitmap = new WriteableBitmap(bitmapImage);
				wBitmap.SaveJpeg(stream, wBitmap.PixelWidth, wBitmap.PixelHeight, 0, 100);
				stream.Seek(0, SeekOrigin.Begin);
				data = stream.GetBuffer();
			}

			return data;
		}
	}
}
