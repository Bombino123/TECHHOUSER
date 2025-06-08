using System;
using System.Threading;
using NAudio.Wave.Asio;

namespace NAudio.Wave;

public class AsioOut : IWavePlayer, IDisposable
{
	private AsioDriverExt driver;

	private IWaveProvider sourceStream;

	private PlaybackState playbackState;

	private int nbSamples;

	private byte[] waveBuffer;

	private AsioSampleConvertor.SampleConvertor convertor;

	private string driverName;

	private readonly SynchronizationContext syncContext;

	private bool isInitialized;

	public int PlaybackLatency
	{
		get
		{
			driver.Driver.GetLatencies(out var _, out var outputLatency);
			return outputLatency;
		}
	}

	public bool AutoStop { get; set; }

	public bool HasReachedEnd { get; private set; }

	public PlaybackState PlaybackState => playbackState;

	public string DriverName => driverName;

	public int NumberOfOutputChannels { get; private set; }

	public int NumberOfInputChannels { get; private set; }

	public int DriverInputChannelCount => driver.Capabilities.NbInputChannels;

	public int DriverOutputChannelCount => driver.Capabilities.NbOutputChannels;

	public int FramesPerBuffer
	{
		get
		{
			if (!isInitialized)
			{
				throw new Exception("Not initialized yet. Call this after calling Init");
			}
			return nbSamples;
		}
	}

	public int ChannelOffset { get; set; }

	public int InputChannelOffset { get; set; }

	[Obsolete("this function will be removed in a future NAudio as ASIO does not support setting the volume on the device")]
	public float Volume
	{
		get
		{
			return 1f;
		}
		set
		{
			if (value != 1f)
			{
				throw new InvalidOperationException("AsioOut does not support setting the device volume");
			}
		}
	}

	public WaveFormat OutputWaveFormat { get; private set; }

	public event EventHandler<StoppedEventArgs> PlaybackStopped;

	public event EventHandler<AsioAudioAvailableEventArgs> AudioAvailable;

	public event EventHandler DriverResetRequest;

	public AsioOut()
		: this(0)
	{
	}

	public AsioOut(string driverName)
	{
		syncContext = SynchronizationContext.Current;
		InitFromName(driverName);
	}

	public AsioOut(int driverIndex)
	{
		syncContext = SynchronizationContext.Current;
		string[] driverNames = GetDriverNames();
		if (driverNames.Length == 0)
		{
			throw new ArgumentException("There is no ASIO Driver installed on your system");
		}
		if (driverIndex < 0 || driverIndex > driverNames.Length)
		{
			throw new ArgumentException($"Invalid device number. Must be in the range [0,{driverNames.Length}]");
		}
		InitFromName(driverNames[driverIndex]);
	}

	~AsioOut()
	{
		Dispose();
	}

	public void Dispose()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (driver != null)
		{
			if ((int)playbackState != 0)
			{
				driver.Stop();
			}
			driver.ResetRequestCallback = null;
			driver.ReleaseDriver();
			driver = null;
		}
	}

	public static string[] GetDriverNames()
	{
		return AsioDriver.GetAsioDriverNames();
	}

	public static bool isSupported()
	{
		return GetDriverNames().Length != 0;
	}

	public bool IsSampleRateSupported(int sampleRate)
	{
		return driver.IsSampleRateSupported(sampleRate);
	}

	private void InitFromName(string driverName)
	{
		this.driverName = driverName;
		AsioDriver asioDriverByName = AsioDriver.GetAsioDriverByName(driverName);
		try
		{
			driver = new AsioDriverExt(asioDriverByName);
		}
		catch
		{
			ReleaseDriver(asioDriverByName);
			throw;
		}
		driver.ResetRequestCallback = OnDriverResetRequest;
		ChannelOffset = 0;
	}

	private void OnDriverResetRequest()
	{
		this.DriverResetRequest?.Invoke(this, EventArgs.Empty);
	}

	private void ReleaseDriver(AsioDriver driver)
	{
		driver.DisposeBuffers();
		driver.ReleaseComAsioDriver();
	}

	public void ShowControlPanel()
	{
		driver.ShowControlPanel();
	}

	public void Play()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)playbackState != 1)
		{
			playbackState = (PlaybackState)1;
			HasReachedEnd = false;
			driver.Start();
		}
	}

	public void Stop()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		playbackState = (PlaybackState)0;
		driver.Stop();
		HasReachedEnd = false;
		RaisePlaybackStopped(null);
	}

	public void Pause()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		playbackState = (PlaybackState)2;
		driver.Stop();
	}

	public void Init(IWaveProvider waveProvider)
	{
		InitRecordAndPlayback(waveProvider, 0, -1);
	}

	public void InitRecordAndPlayback(IWaveProvider waveProvider, int recordChannels, int recordOnlySampleRate)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		if (isInitialized)
		{
			throw new InvalidOperationException("Already initialised this instance of AsioOut - dispose and create a new one");
		}
		isInitialized = true;
		int num = ((waveProvider != null) ? waveProvider.WaveFormat.SampleRate : recordOnlySampleRate);
		if (waveProvider != null)
		{
			sourceStream = waveProvider;
			NumberOfOutputChannels = waveProvider.WaveFormat.Channels;
			AsioSampleType type = driver.Capabilities.OutputChannelInfos[0].type;
			convertor = AsioSampleConvertor.SelectSampleConvertor(waveProvider.WaveFormat, type);
			switch (type)
			{
			case AsioSampleType.Float32LSB:
				OutputWaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(waveProvider.WaveFormat.SampleRate, waveProvider.WaveFormat.Channels);
				break;
			case AsioSampleType.Int32LSB:
				OutputWaveFormat = new WaveFormat(waveProvider.WaveFormat.SampleRate, 32, waveProvider.WaveFormat.Channels);
				break;
			case AsioSampleType.Int16LSB:
				OutputWaveFormat = new WaveFormat(waveProvider.WaveFormat.SampleRate, 16, waveProvider.WaveFormat.Channels);
				break;
			case AsioSampleType.Int24LSB:
				OutputWaveFormat = new WaveFormat(waveProvider.WaveFormat.SampleRate, 24, waveProvider.WaveFormat.Channels);
				break;
			default:
				throw new NotSupportedException($"{type} not currently supported");
			}
		}
		else
		{
			NumberOfOutputChannels = 0;
		}
		if (!driver.IsSampleRateSupported(num))
		{
			throw new ArgumentException("SampleRate is not supported");
		}
		if (driver.Capabilities.SampleRate != (double)num)
		{
			driver.SetSampleRate(num);
		}
		driver.FillBufferCallback = driver_BufferUpdate;
		NumberOfInputChannels = recordChannels;
		nbSamples = driver.CreateBuffers(NumberOfOutputChannels, NumberOfInputChannels, useMaxBufferSize: false);
		driver.SetChannelOffset(ChannelOffset, InputChannelOffset);
		if (waveProvider != null)
		{
			waveBuffer = new byte[nbSamples * NumberOfOutputChannels * waveProvider.WaveFormat.BitsPerSample / 8];
		}
	}

	private unsafe void driver_BufferUpdate(IntPtr[] inputChannels, IntPtr[] outputChannels)
	{
		if (NumberOfInputChannels > 0)
		{
			EventHandler<AsioAudioAvailableEventArgs> audioAvailable = this.AudioAvailable;
			if (audioAvailable != null)
			{
				AsioAudioAvailableEventArgs asioAudioAvailableEventArgs = new AsioAudioAvailableEventArgs(inputChannels, outputChannels, nbSamples, driver.Capabilities.InputChannelInfos[0].type);
				audioAvailable(this, asioAudioAvailableEventArgs);
				if (asioAudioAvailableEventArgs.WrittenToOutputBuffers)
				{
					return;
				}
			}
		}
		if (NumberOfOutputChannels <= 0)
		{
			return;
		}
		int num = sourceStream.Read(waveBuffer, 0, waveBuffer.Length);
		if (num < waveBuffer.Length)
		{
			Array.Clear(waveBuffer, num, waveBuffer.Length - num);
		}
		fixed (byte* ptr = &waveBuffer[0])
		{
			void* value = ptr;
			convertor(new IntPtr(value), outputChannels, NumberOfOutputChannels, nbSamples);
		}
		if (num == 0)
		{
			if (AutoStop)
			{
				Stop();
			}
			HasReachedEnd = true;
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

	public string AsioInputChannelName(int channel)
	{
		if (channel <= DriverInputChannelCount)
		{
			return driver.Capabilities.InputChannelInfos[channel].name;
		}
		return "";
	}

	public string AsioOutputChannelName(int channel)
	{
		if (channel <= DriverOutputChannelCount)
		{
			return driver.Capabilities.OutputChannelInfos[channel].name;
		}
		return "";
	}
}
