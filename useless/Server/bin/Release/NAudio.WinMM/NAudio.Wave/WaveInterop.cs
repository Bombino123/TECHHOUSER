using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave;

public class WaveInterop
{
	[Flags]
	public enum WaveInOutOpenFlags
	{
		CallbackNull = 0,
		CallbackFunction = 0x30000,
		CallbackEvent = 0x50000,
		CallbackWindow = 0x10000,
		CallbackThread = 0x20000
	}

	public enum WaveMessage
	{
		WaveInOpen = 958,
		WaveInClose = 959,
		WaveInData = 960,
		WaveOutClose = 956,
		WaveOutDone = 957,
		WaveOutOpen = 955
	}

	public delegate void WaveCallback(IntPtr hWaveOut, WaveMessage message, IntPtr dwInstance, WaveHeader wavhdr, IntPtr dwReserved);

	[DllImport("winmm.dll")]
	public static extern int mmioStringToFOURCC([MarshalAs(UnmanagedType.LPStr)] string s, int flags);

	[DllImport("winmm.dll")]
	public static extern int waveOutGetNumDevs();

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutPrepareHeader(IntPtr hWaveOut, WaveHeader lpWaveOutHdr, int uSize);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutUnprepareHeader(IntPtr hWaveOut, WaveHeader lpWaveOutHdr, int uSize);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutWrite(IntPtr hWaveOut, WaveHeader lpWaveOutHdr, int uSize);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutOpen(out IntPtr hWaveOut, IntPtr uDeviceID, WaveFormat lpFormat, WaveCallback dwCallback, IntPtr dwInstance, WaveInOutOpenFlags dwFlags);

	[DllImport("winmm.dll", EntryPoint = "waveOutOpen")]
	public static extern MmResult waveOutOpenWindow(out IntPtr hWaveOut, IntPtr uDeviceID, WaveFormat lpFormat, IntPtr callbackWindowHandle, IntPtr dwInstance, WaveInOutOpenFlags dwFlags);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutReset(IntPtr hWaveOut);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutClose(IntPtr hWaveOut);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutPause(IntPtr hWaveOut);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutRestart(IntPtr hWaveOut);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutGetPosition(IntPtr hWaveOut, ref MmTime mmTime, int uSize);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutSetVolume(IntPtr hWaveOut, int dwVolume);

	[DllImport("winmm.dll")]
	public static extern MmResult waveOutGetVolume(IntPtr hWaveOut, out int dwVolume);

	[DllImport("winmm.dll", CharSet = CharSet.Auto)]
	public static extern MmResult waveOutGetDevCaps(IntPtr deviceID, out WaveOutCapabilities waveOutCaps, int waveOutCapsSize);

	[DllImport("winmm.dll")]
	public static extern int waveInGetNumDevs();

	[DllImport("winmm.dll", CharSet = CharSet.Auto)]
	public static extern MmResult waveInGetDevCaps(IntPtr deviceID, out WaveInCapabilities waveInCaps, int waveInCapsSize);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInAddBuffer(IntPtr hWaveIn, WaveHeader pwh, int cbwh);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInClose(IntPtr hWaveIn);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInOpen(out IntPtr hWaveIn, IntPtr uDeviceID, WaveFormat lpFormat, WaveCallback dwCallback, IntPtr dwInstance, WaveInOutOpenFlags dwFlags);

	[DllImport("winmm.dll", EntryPoint = "waveInOpen")]
	public static extern MmResult waveInOpenWindow(out IntPtr hWaveIn, IntPtr uDeviceID, WaveFormat lpFormat, IntPtr callbackWindowHandle, IntPtr dwInstance, WaveInOutOpenFlags dwFlags);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInPrepareHeader(IntPtr hWaveIn, WaveHeader lpWaveInHdr, int uSize);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInUnprepareHeader(IntPtr hWaveIn, WaveHeader lpWaveInHdr, int uSize);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInReset(IntPtr hWaveIn);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInStart(IntPtr hWaveIn);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInStop(IntPtr hWaveIn);

	[DllImport("winmm.dll")]
	public static extern MmResult waveInGetPosition(IntPtr hWaveIn, out MmTime mmTime, int uSize);
}
