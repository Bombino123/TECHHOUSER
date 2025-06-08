using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.Utils;

namespace NAudio.Wave;

public class WasapiOut : IWavePlayer, IDisposable, IWavePosition
{
	private AudioClient audioClient;

	private readonly MMDevice mmDevice;

	private readonly AudioClientShareMode shareMode;

	private AudioRenderClient renderClient;

	private IWaveProvider sourceProvider;

	private int latencyMilliseconds;

	private int bufferFrameCount;

	private int bytesPerFrame;

	private readonly bool isUsingEventSync;

	private EventWaitHandle frameEventWaitHandle;

	private byte[] readBuffer;

	private volatile PlaybackState playbackState;

	private Thread playThread;

	private readonly SynchronizationContext syncContext;

	private bool dmoResamplerNeeded;

	public WaveFormat OutputWaveFormat { get; private set; }

	public PlaybackState PlaybackState => playbackState;

	public float Volume
	{
		get
		{
			return mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
		}
		set
		{
			if (value < 0f)
			{
				throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
			}
			if (value > 1f)
			{
				throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
			}
			mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar = value;
		}
	}

	public AudioStreamVolume AudioStreamVolume
	{
		get
		{
			if (shareMode == AudioClientShareMode.Exclusive)
			{
				throw new InvalidOperationException("AudioStreamVolume is ONLY supported for shared audio streams.");
			}
			return audioClient.AudioStreamVolume;
		}
	}

	public event EventHandler<StoppedEventArgs> PlaybackStopped;

	public WasapiOut()
		: this(GetDefaultAudioEndpoint(), AudioClientShareMode.Shared, useEventSync: true, 200)
	{
	}

	public WasapiOut(AudioClientShareMode shareMode, int latency)
		: this(GetDefaultAudioEndpoint(), shareMode, useEventSync: true, latency)
	{
	}

	public WasapiOut(AudioClientShareMode shareMode, bool useEventSync, int latency)
		: this(GetDefaultAudioEndpoint(), shareMode, useEventSync, latency)
	{
	}

	public WasapiOut(MMDevice device, AudioClientShareMode shareMode, bool useEventSync, int latency)
	{
		audioClient = device.AudioClient;
		mmDevice = device;
		this.shareMode = shareMode;
		isUsingEventSync = useEventSync;
		latencyMilliseconds = latency;
		syncContext = SynchronizationContext.Current;
		OutputWaveFormat = audioClient.MixFormat;
	}

	private static MMDevice GetDefaultAudioEndpoint()
	{
		if (Environment.OSVersion.Version.Major < 6)
		{
			throw new NotSupportedException("WASAPI supported only on Windows Vista and above");
		}
		return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
	}

	private void PlayThread()
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Invalid comparison between Unknown and I4
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Invalid comparison between Unknown and I4
		ResamplerDmoStream resamplerDmoStream = null;
		IWaveProvider playbackProvider = sourceProvider;
		Exception e = null;
		try
		{
			if (dmoResamplerNeeded)
			{
				resamplerDmoStream = new ResamplerDmoStream(sourceProvider, OutputWaveFormat);
				playbackProvider = (IWaveProvider)(object)resamplerDmoStream;
			}
			bufferFrameCount = audioClient.BufferSize;
			bytesPerFrame = OutputWaveFormat.Channels * OutputWaveFormat.BitsPerSample / 8;
			readBuffer = BufferHelpers.Ensure(readBuffer, bufferFrameCount * bytesPerFrame);
			if (FillBuffer(playbackProvider, bufferFrameCount))
			{
				return;
			}
			WaitHandle[] waitHandles = new WaitHandle[1] { frameEventWaitHandle };
			audioClient.Start();
			while ((int)playbackState != 0)
			{
				if (isUsingEventSync)
				{
					WaitHandle.WaitAny(waitHandles, 3 * latencyMilliseconds, exitContext: false);
				}
				else
				{
					Thread.Sleep(latencyMilliseconds / 2);
				}
				if ((int)playbackState == 1)
				{
					int num = ((!isUsingEventSync) ? audioClient.CurrentPadding : ((shareMode == AudioClientShareMode.Shared) ? audioClient.CurrentPadding : 0));
					int num2 = bufferFrameCount - num;
					if (num2 > 10 && FillBuffer(playbackProvider, num2))
					{
						break;
					}
				}
			}
			if ((int)playbackState == 1)
			{
				Thread.Sleep(isUsingEventSync ? latencyMilliseconds : (latencyMilliseconds / 2));
			}
			audioClient.Stop();
			playbackState = (PlaybackState)0;
			audioClient.Reset();
		}
		catch (Exception ex)
		{
			e = ex;
		}
		finally
		{
			((Stream)(object)resamplerDmoStream)?.Dispose();
			RaisePlaybackStopped(e);
		}
	}

	private void RaisePlaybackStopped(Exception e)
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

	private unsafe bool FillBuffer(IWaveProvider playbackProvider, int frameCount)
	{
		int num = frameCount * bytesPerFrame;
		int num2 = playbackProvider.Read(readBuffer, 0, num);
		if (num2 == 0)
		{
			return true;
		}
		IntPtr buffer = renderClient.GetBuffer(frameCount);
		Marshal.Copy(readBuffer, 0, buffer, num2);
		if (isUsingEventSync && shareMode == AudioClientShareMode.Exclusive)
		{
			if (num2 < num)
			{
				byte* ptr = (byte*)(void*)buffer;
				while (num2 < num)
				{
					ptr[num2++] = 0;
				}
			}
			renderClient.ReleaseBuffer(frameCount, AudioClientBufferFlags.None);
		}
		else
		{
			int numFramesWritten = num2 / bytesPerFrame;
			renderClient.ReleaseBuffer(numFramesWritten, AudioClientBufferFlags.None);
		}
		return false;
	}

	private WaveFormat GetFallbackFormat()
	{
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		int sampleRate = audioClient.MixFormat.SampleRate;
		int channels = audioClient.MixFormat.Channels;
		List<int> list = new List<int> { OutputWaveFormat.SampleRate };
		if (!list.Contains(sampleRate))
		{
			list.Add(sampleRate);
		}
		if (!list.Contains(44100))
		{
			list.Add(44100);
		}
		if (!list.Contains(48000))
		{
			list.Add(48000);
		}
		List<int> list2 = new List<int> { OutputWaveFormat.Channels };
		if (!list2.Contains(channels))
		{
			list2.Add(channels);
		}
		if (!list2.Contains(2))
		{
			list2.Add(2);
		}
		List<int> list3 = new List<int> { OutputWaveFormat.BitsPerSample };
		if (!list3.Contains(32))
		{
			list3.Add(32);
		}
		if (!list3.Contains(24))
		{
			list3.Add(24);
		}
		if (!list3.Contains(16))
		{
			list3.Add(16);
		}
		foreach (int item in list)
		{
			foreach (int item2 in list2)
			{
				foreach (int item3 in list3)
				{
					WaveFormatExtensible val = new WaveFormatExtensible(item, item3, item2);
					if (audioClient.IsFormatSupported(shareMode, (WaveFormat)(object)val))
					{
						return (WaveFormat)(object)val;
					}
				}
			}
		}
		throw new NotSupportedException("Can't find a supported format to use");
	}

	public long GetPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		PlaybackState val = playbackState;
		if ((int)val != 0)
		{
			ulong position;
			if ((int)val == 1)
			{
				position = audioClient.AudioClockClient.AdjustedPosition;
			}
			else
			{
				audioClient.AudioClockClient.GetPosition(out position, out var _);
			}
			return (long)position * (long)OutputWaveFormat.AverageBytesPerSecond / (long)audioClient.AudioClockClient.Frequency;
		}
		return 0L;
	}

	public void Play()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if ((int)playbackState != 1)
		{
			if ((int)playbackState == 0)
			{
				playThread = new Thread(PlayThread)
				{
					IsBackground = true
				};
				playbackState = (PlaybackState)1;
				playThread.Start();
			}
			else
			{
				playbackState = (PlaybackState)1;
			}
		}
	}

	public void Stop()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)playbackState != 0)
		{
			playbackState = (PlaybackState)0;
			playThread.Join();
			playThread = null;
		}
	}

	public void Pause()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)playbackState == 1)
		{
			playbackState = (PlaybackState)2;
		}
	}

	public void Init(IWaveProvider waveProvider)
	{
		long num = (long)latencyMilliseconds * 10000L;
		OutputWaveFormat = waveProvider.WaveFormat;
		AudioClientStreamFlags audioClientStreamFlags = AudioClientStreamFlags.SrcDefaultQuality | AudioClientStreamFlags.AutoConvertPcm;
		sourceProvider = waveProvider;
		if (shareMode == AudioClientShareMode.Exclusive)
		{
			audioClientStreamFlags = AudioClientStreamFlags.None;
			if (!audioClient.IsFormatSupported(shareMode, OutputWaveFormat, out var closestMatchFormat))
			{
				if (closestMatchFormat == null)
				{
					OutputWaveFormat = GetFallbackFormat();
				}
				else
				{
					OutputWaveFormat = (WaveFormat)(object)closestMatchFormat;
				}
				try
				{
					ResamplerDmoStream resamplerDmoStream = new ResamplerDmoStream(waveProvider, OutputWaveFormat);
					try
					{
					}
					finally
					{
						((IDisposable)resamplerDmoStream)?.Dispose();
					}
				}
				catch (Exception)
				{
					OutputWaveFormat = GetFallbackFormat();
					ResamplerDmoStream resamplerDmoStream = new ResamplerDmoStream(waveProvider, OutputWaveFormat);
					try
					{
					}
					finally
					{
						((IDisposable)resamplerDmoStream)?.Dispose();
					}
				}
				dmoResamplerNeeded = true;
			}
			else
			{
				dmoResamplerNeeded = false;
			}
		}
		if (isUsingEventSync)
		{
			if (shareMode == AudioClientShareMode.Shared)
			{
				audioClient.Initialize(shareMode, AudioClientStreamFlags.EventCallback | audioClientStreamFlags, num, 0L, OutputWaveFormat, Guid.Empty);
				long streamLatency = audioClient.StreamLatency;
				if (streamLatency != 0L)
				{
					latencyMilliseconds = (int)(streamLatency / 10000);
				}
			}
			else
			{
				try
				{
					audioClient.Initialize(shareMode, AudioClientStreamFlags.EventCallback | audioClientStreamFlags, num, num, OutputWaveFormat, Guid.Empty);
				}
				catch (COMException ex2)
				{
					if (ex2.ErrorCode != -2004287463)
					{
						throw;
					}
					long num2 = (long)(10000000.0 / (double)OutputWaveFormat.SampleRate * (double)audioClient.BufferSize + 0.5);
					audioClient.Dispose();
					audioClient = mmDevice.AudioClient;
					audioClient.Initialize(shareMode, AudioClientStreamFlags.EventCallback | audioClientStreamFlags, num2, num2, OutputWaveFormat, Guid.Empty);
				}
			}
			frameEventWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);
			audioClient.SetEventHandle(frameEventWaitHandle.SafeWaitHandle.DangerousGetHandle());
		}
		else
		{
			audioClient.Initialize(shareMode, audioClientStreamFlags, num, 0L, OutputWaveFormat, Guid.Empty);
		}
		renderClient = audioClient.AudioRenderClient;
	}

	public void Dispose()
	{
		if (audioClient != null)
		{
			Stop();
			audioClient.Dispose();
			audioClient = null;
			renderClient = null;
		}
	}
}
