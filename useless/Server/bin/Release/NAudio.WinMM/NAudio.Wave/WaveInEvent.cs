using System;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Mixer;

namespace NAudio.Wave;

public class WaveInEvent : IWaveIn, IDisposable
{
	private readonly AutoResetEvent callbackEvent;

	private readonly SynchronizationContext syncContext;

	private IntPtr waveInHandle;

	private volatile CaptureState captureState;

	private WaveInBuffer[] buffers;

	public static int DeviceCount => WaveInterop.waveInGetNumDevs();

	public int BufferMilliseconds { get; set; }

	public int NumberOfBuffers { get; set; }

	public int DeviceNumber { get; set; }

	public WaveFormat WaveFormat { get; set; }

	public event EventHandler<WaveInEventArgs> DataAvailable;

	public event EventHandler<StoppedEventArgs> RecordingStopped;

	public WaveInEvent()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		callbackEvent = new AutoResetEvent(initialState: false);
		syncContext = SynchronizationContext.Current;
		DeviceNumber = 0;
		WaveFormat = new WaveFormat(8000, 16, 1);
		BufferMilliseconds = 100;
		NumberOfBuffers = 3;
		captureState = (CaptureState)0;
	}

	public static WaveInCapabilities GetCapabilities(int devNumber)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		WaveInCapabilities waveInCaps = default(WaveInCapabilities);
		int waveInCapsSize = Marshal.SizeOf(waveInCaps);
		MmException.Try(WaveInterop.waveInGetDevCaps((IntPtr)devNumber, out waveInCaps, waveInCapsSize), "waveInGetDevCaps");
		return waveInCaps;
	}

	private void CreateBuffers()
	{
		int num = BufferMilliseconds * WaveFormat.AverageBytesPerSecond / 1000;
		if (num % WaveFormat.BlockAlign != 0)
		{
			num -= num % WaveFormat.BlockAlign;
		}
		buffers = new WaveInBuffer[NumberOfBuffers];
		for (int i = 0; i < buffers.Length; i++)
		{
			buffers[i] = new WaveInBuffer(waveInHandle, num);
		}
	}

	private void OpenWaveInDevice()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		CloseWaveInDevice();
		MmException.Try(WaveInterop.waveInOpenWindow(out waveInHandle, (IntPtr)DeviceNumber, WaveFormat, callbackEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackEvent), "waveInOpen");
		CreateBuffers();
	}

	public void StartRecording()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if ((int)captureState != 0)
		{
			throw new InvalidOperationException("Already recording");
		}
		OpenWaveInDevice();
		MmException.Try(WaveInterop.waveInStart(waveInHandle), "waveInStart");
		captureState = (CaptureState)1;
		ThreadPool.QueueUserWorkItem(delegate
		{
			RecordThread();
		}, null);
	}

	private void RecordThread()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Exception e = null;
		try
		{
			DoRecording();
		}
		catch (Exception ex)
		{
			e = ex;
		}
		finally
		{
			captureState = (CaptureState)0;
			RaiseRecordingStoppedEvent(e);
		}
	}

	private void DoRecording()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Invalid comparison between Unknown and I4
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		captureState = (CaptureState)2;
		WaveInBuffer[] array = buffers;
		foreach (WaveInBuffer waveInBuffer in array)
		{
			if (!waveInBuffer.InQueue)
			{
				waveInBuffer.Reuse();
			}
		}
		while ((int)captureState == 2)
		{
			if (!callbackEvent.WaitOne())
			{
				continue;
			}
			array = buffers;
			foreach (WaveInBuffer waveInBuffer2 in array)
			{
				if (waveInBuffer2.Done)
				{
					if (waveInBuffer2.BytesRecorded > 0)
					{
						this.DataAvailable?.Invoke(this, new WaveInEventArgs(waveInBuffer2.Data, waveInBuffer2.BytesRecorded));
					}
					if ((int)captureState == 2)
					{
						waveInBuffer2.Reuse();
					}
				}
			}
		}
	}

	private void RaiseRecordingStoppedEvent(Exception e)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		EventHandler<StoppedEventArgs> handler = this.RecordingStopped;
		if (handler == null)
		{
			return;
		}
		if (syncContext == null)
		{
			handler(this, new StoppedEventArgs(e));
			return;
		}
		syncContext.Post(delegate
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			handler(this, new StoppedEventArgs(e));
		}, null);
	}

	public void StopRecording()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((int)captureState != 0)
		{
			captureState = (CaptureState)3;
			MmException.Try(WaveInterop.waveInStop(waveInHandle), "waveInStop");
			MmException.Try(WaveInterop.waveInReset(waveInHandle), "waveInReset");
			callbackEvent.Set();
		}
	}

	public long GetPosition()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		MmTime mmTime = default(MmTime);
		mmTime.wType = 4u;
		MmException.Try(WaveInterop.waveInGetPosition(waveInHandle, out mmTime, Marshal.SizeOf(mmTime)), "waveInGetPosition");
		if (mmTime.wType != 4)
		{
			throw new Exception($"waveInGetPosition: wType -> Expected {4}, Received {mmTime.wType}");
		}
		return mmTime.cb;
	}

	protected virtual void Dispose(bool disposing)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (disposing)
		{
			if ((int)captureState != 0)
			{
				StopRecording();
			}
			CloseWaveInDevice();
		}
	}

	private void CloseWaveInDevice()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		WaveInterop.waveInReset(waveInHandle);
		if (buffers != null)
		{
			for (int i = 0; i < buffers.Length; i++)
			{
				buffers[i].Dispose();
			}
			buffers = null;
		}
		WaveInterop.waveInClose(waveInHandle);
		waveInHandle = IntPtr.Zero;
	}

	public MixerLine GetMixerLine()
	{
		if (waveInHandle != IntPtr.Zero)
		{
			return new MixerLine(waveInHandle, 0, MixerFlags.WaveInHandle);
		}
		return new MixerLine((IntPtr)DeviceNumber, 0, MixerFlags.WaveIn);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
