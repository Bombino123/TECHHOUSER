using System;
using System.Windows.Forms;

namespace NAudio.Wave;

public class WaveCallbackInfo
{
	private WaveWindow waveOutWindow;

	private WaveWindowNative waveOutWindowNative;

	public WaveCallbackStrategy Strategy { get; private set; }

	public IntPtr Handle { get; private set; }

	public static WaveCallbackInfo FunctionCallback()
	{
		return new WaveCallbackInfo(WaveCallbackStrategy.FunctionCallback, IntPtr.Zero);
	}

	public static WaveCallbackInfo NewWindow()
	{
		return new WaveCallbackInfo(WaveCallbackStrategy.NewWindow, IntPtr.Zero);
	}

	public static WaveCallbackInfo ExistingWindow(IntPtr handle)
	{
		if (handle == IntPtr.Zero)
		{
			throw new ArgumentException("Handle cannot be zero");
		}
		return new WaveCallbackInfo(WaveCallbackStrategy.ExistingWindow, handle);
	}

	private WaveCallbackInfo(WaveCallbackStrategy strategy, IntPtr handle)
	{
		Strategy = strategy;
		Handle = handle;
	}

	internal void Connect(WaveInterop.WaveCallback callback)
	{
		if (Strategy == WaveCallbackStrategy.NewWindow)
		{
			waveOutWindow = new WaveWindow(callback);
			((Control)waveOutWindow).CreateControl();
			Handle = ((Control)waveOutWindow).Handle;
		}
		else if (Strategy == WaveCallbackStrategy.ExistingWindow)
		{
			waveOutWindowNative = new WaveWindowNative(callback);
			((NativeWindow)waveOutWindowNative).AssignHandle(Handle);
		}
	}

	internal MmResult WaveOutOpen(out IntPtr waveOutHandle, int deviceNumber, WaveFormat waveFormat, WaveInterop.WaveCallback callback)
	{
		if (Strategy == WaveCallbackStrategy.FunctionCallback)
		{
			return WaveInterop.waveOutOpen(out waveOutHandle, (IntPtr)deviceNumber, waveFormat, callback, IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackFunction);
		}
		return WaveInterop.waveOutOpenWindow(out waveOutHandle, (IntPtr)deviceNumber, waveFormat, Handle, IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackWindow);
	}

	internal MmResult WaveInOpen(out IntPtr waveInHandle, int deviceNumber, WaveFormat waveFormat, WaveInterop.WaveCallback callback)
	{
		if (Strategy == WaveCallbackStrategy.FunctionCallback)
		{
			return WaveInterop.waveInOpen(out waveInHandle, (IntPtr)deviceNumber, waveFormat, callback, IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackFunction);
		}
		return WaveInterop.waveInOpenWindow(out waveInHandle, (IntPtr)deviceNumber, waveFormat, Handle, IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackWindow);
	}

	internal void Disconnect()
	{
		if (waveOutWindow != null)
		{
			((Form)waveOutWindow).Close();
			waveOutWindow = null;
		}
		if (waveOutWindowNative != null)
		{
			((NativeWindow)waveOutWindowNative).ReleaseHandle();
			waveOutWindowNative = null;
		}
	}
}
