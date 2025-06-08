using System.Runtime.InteropServices;

namespace Plugin.Handler;

internal class HandleRotation
{
	public struct DEVMODE
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmDeviceName;

		public short dmSpecVersion;

		public short dmDriverVersion;

		public short dmSize;

		public short dmDriverExtra;

		public int dmFields;

		public int dmPositionX;

		public int dmPositionY;

		public int dmDisplayOrientation;

		public int dmDisplayFixedOutput;

		public short dmColor;

		public short dmDuplex;

		public short dmYResolution;

		public short dmTTOption;

		public short dmCollate;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string dmFormName;

		public short dmLogPixels;

		public short dmBitsPerPel;

		public int dmPelsWidth;

		public int dmPelsHeight;

		public int dmDisplayFlags;

		public int dmDisplayFrequency;

		public int dmICMMethod;

		public int dmICMIntent;

		public int dmMediaType;

		public int dmDitherType;

		public int dmReserved1;

		public int dmReserved2;

		public int dmPanningWidth;

		public int dmPanningHeight;
	}

	public class NativeMethods
	{
		public const int ENUM_CURRENT_SETTINGS = -1;

		public const int DMDO_DEFAULT = 0;

		public const int DMDO_90 = 1;

		public const int DMDO_180 = 2;

		public const int DMDO_270 = 3;

		public const int DISP_CHANGE_SUCCESSFUL = 0;

		public const int DISP_CHANGE_RESTART = 1;

		public const int DISP_CHANGE_FAILED = -1;

		[DllImport("user32.dll", CharSet = CharSet.Ansi)]
		public static extern int EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

		[DllImport("user32.dll", CharSet = CharSet.Ansi)]
		public static extern int ChangeDisplaySettings(ref DEVMODE lpDevMode, int dwFlags);
	}

	public static DEVMODE CreateDevmode()
	{
		DEVMODE dEVMODE = default(DEVMODE);
		dEVMODE.dmDeviceName = new string(new char[32]);
		dEVMODE.dmFormName = new string(new char[32]);
		dEVMODE.dmSize = (short)Marshal.SizeOf((object)dEVMODE);
		return dEVMODE;
	}

	public void Rotation(string Rot)
	{
		DEVMODE lpDevMode = default(DEVMODE);
		lpDevMode.dmDeviceName = new string(new char[32]);
		lpDevMode.dmFormName = new string(new char[32]);
		lpDevMode.dmSize = (short)Marshal.SizeOf((object)lpDevMode);
		if (NativeMethods.EnumDisplaySettings(null, -1, ref lpDevMode) == 0)
		{
			return;
		}
		if (lpDevMode.dmDisplayOrientation == 0 || lpDevMode.dmDisplayOrientation == 2)
		{
			if (Rot == "270" || Rot == "90")
			{
				int dmPelsHeight = lpDevMode.dmPelsHeight;
				lpDevMode.dmPelsHeight = lpDevMode.dmPelsWidth;
				lpDevMode.dmPelsWidth = dmPelsHeight;
			}
		}
		else if (Rot == "180" || Rot == "0")
		{
			int dmPelsHeight2 = lpDevMode.dmPelsHeight;
			lpDevMode.dmPelsHeight = lpDevMode.dmPelsWidth;
			lpDevMode.dmPelsWidth = dmPelsHeight2;
		}
		switch (Rot)
		{
		case "0":
			lpDevMode.dmDisplayOrientation = 0;
			break;
		case "180":
			lpDevMode.dmDisplayOrientation = 2;
			break;
		case "90":
			lpDevMode.dmDisplayOrientation = 1;
			break;
		case "270":
			lpDevMode.dmDisplayOrientation = 3;
			break;
		}
		NativeMethods.ChangeDisplaySettings(ref lpDevMode, 0);
	}
}
