using System;
using System.Threading;

namespace NAudio.Wave;

public class WaveOutEvent : IWavePlayer, IDisposable, IWavePosition
{
	private readonly object waveOutLock;

	private readonly SynchronizationContext syncContext;

	private IntPtr hWaveOut;

	private WaveOutBuffer[] buffers;

	private IWaveProvider waveStream;

	private volatile PlaybackState playbackState;

	private AutoResetEvent callbackEvent;

	public int DesiredLatency { get; set; }

	public int NumberOfBuffers { get; set; }

	public int DeviceNumber { get; set; } = -1;


	public WaveFormat OutputWaveFormat => waveStream.WaveFormat;

	public PlaybackState PlaybackState => playbackState;

	public float Volume
	{
		get
		{
			return WaveOutUtils.GetWaveOutVolume(hWaveOut, waveOutLock);
		}
		set
		{
			WaveOutUtils.SetWaveOutVolume(value, hWaveOut, waveOutLock);
		}
	}

	public event EventHandler<StoppedEventArgs> PlaybackStopped;

	public WaveOutEvent()
	{
		syncContext = SynchronizationContext.Current;
		if (syncContext != null && (syncContext.GetType().Name == "LegacyAspNetSynchronizationContext" || syncContext.GetType().Name == "AspNetSynchronizationContext"))
		{
			syncContext = null;
		}
		DesiredLatency = 300;
		NumberOfBuffers = 2;
		waveOutLock = new object();
	}

	public void Init(IWaveProvider waveProvider)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if ((int)playbackState != 0)
		{
			throw new InvalidOperationException("Can't re-initialize during playback");
		}
		if (hWaveOut != IntPtr.Zero)
		{
			DisposeBuffers();
			CloseWaveOut();
		}
		callbackEvent = new AutoResetEvent(initialState: false);
		waveStream = waveProvider;
		int bufferSize = waveProvider.WaveFormat.ConvertLatencyToByteSize((DesiredLatency + NumberOfBuffers - 1) / NumberOfBuffers);
		MmResult val;
		lock (waveOutLock)
		{
			val = WaveInterop.waveOutOpenWindow(out hWaveOut, (IntPtr)DeviceNumber, waveStream.WaveFormat, callbackEvent.SafeWaitHandle.DangerousGetHandle(), IntPtr.Zero, WaveInterop.WaveInOutOpenFlags.CallbackEvent);
		}
		MmException.Try(val, "waveOutOpen");
		buffers = new WaveOutBuffer[NumberOfBuffers];
		playbackState = (PlaybackState)0;
		for (int i = 0; i < NumberOfBuffers; i++)
		{
			buffers[i] = new WaveOutBuffer(hWaveOut, bufferSize, waveStream, waveOutLock);
		}
	}

	public void Play()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (buffers == null || waveStream == null)
		{
			throw new InvalidOperationException("Must call Init first");
		}
		if ((int)playbackState == 0)
		{
			playbackState = (PlaybackState)1;
			callbackEvent.Set();
			ThreadPool.QueueUserWorkItem(delegate
			{
				PlaybackThread();
			}, null);
		}
		else if ((int)playbackState == 2)
		{
			Resume();
			callbackEvent.Set();
		}
	}

	private void PlaybackThread()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Exception e = null;
		try
		{
			DoPlayback();
		}
		catch (Exception ex)
		{
			e = ex;
		}
		finally
		{
			playbackState = (PlaybackState)0;
			RaisePlaybackStoppedEvent(e);
		}
	}

	private void DoPlayback()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		while ((int)playbackState != 0)
		{
			if (!callbackEvent.WaitOne(DesiredLatency))
			{
				_ = playbackState;
				_ = 1;
			}
			if ((int)playbackState != 1)
			{
				continue;
			}
			int num = 0;
			WaveOutBuffer[] array = buffers;
			foreach (WaveOutBuffer waveOutBuffer in array)
			{
				if (waveOutBuffer.InQueue || waveOutBuffer.OnDone())
				{
					num++;
				}
			}
			if (num == 0)
			{
				playbackState = (PlaybackState)0;
				callbackEvent.Set();
			}
		}
	}

	public void Pause()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if ((int)playbackState == 1)
		{
			playbackState = (PlaybackState)2;
			MmResult val;
			lock (waveOutLock)
			{
				val = WaveInterop.waveOutPause(hWaveOut);
			}
			if ((int)val != 0)
			{
				throw new MmException(val, "waveOutPause");
			}
		}
	}

	private void Resume()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)playbackState == 2)
		{
			MmResult val;
			lock (waveOutLock)
			{
				val = WaveInterop.waveOutRestart(hWaveOut);
			}
			if ((int)val != 0)
			{
				throw new MmException(val, "waveOutRestart");
			}
			playbackState = (PlaybackState)1;
		}
	}

	public void Stop()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if ((int)playbackState != 0)
		{
			playbackState = (PlaybackState)0;
			MmResult val;
			lock (waveOutLock)
			{
				val = WaveInterop.waveOutReset(hWaveOut);
			}
			if ((int)val != 0)
			{
				throw new MmException(val, "waveOutReset");
			}
			callbackEvent.Set();
		}
	}

	public long GetPosition()
	{
		return WaveOutUtils.GetPositionBytes(hWaveOut, waveOutLock);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true);
	}

	protected void Dispose(bool disposing)
	{
		Stop();
		if (disposing)
		{
			DisposeBuffers();
		}
		CloseWaveOut();
	}

	private void CloseWaveOut()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (callbackEvent != null)
		{
			callbackEvent.Close();
			callbackEvent = null;
		}
		lock (waveOutLock)
		{
			if (hWaveOut != IntPtr.Zero)
			{
				WaveInterop.waveOutClose(hWaveOut);
				hWaveOut = IntPtr.Zero;
			}
		}
	}

	private void DisposeBuffers()
	{
		if (buffers != null)
		{
			WaveOutBuffer[] array = buffers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Dispose();
			}
			buffers = null;
		}
	}

	~WaveOutEvent()
	{
		Dispose(disposing: false);
	}

	private void RaisePlaybackStoppedEvent(Exception e)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		EventHandler<StoppedEventArgs> handler = this.PlaybackStopped;
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
}
