using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Plugin.Helper;

namespace Plugin.Handler;

internal class HandlerSoundPlayer
{
	public static WaveOutEvent waveOut;

	public static BufferedWaveProvider bufferedWaveProvider;

	public static SMBPitchShiftingSampleProvider SMB;

	public static int sampleRate = 48000;

	public static float tone = 1f;

	public static G722ChatCodec g722;

	public static void Start()
	{
		g722 = new G722ChatCodec();
		bufferedWaveProvider = new BufferedWaveProvider(g722.RecordFormat);
		SMB = new SMBPitchShiftingSampleProvider(bufferedWaveProvider.ToSampleProvider());
		SMB.PitchFactor = tone;
		waveOut = new WaveOutEvent
		{
			DesiredLatency = 150,
			NumberOfBuffers = 3
		};
		waveOut.Init(new SampleToWaveProvider16(SMB));
		waveOut.Play();
	}

	public static void Buffer(byte[] e)
	{
		if (waveOut != null)
		{
			byte[] array = g722.Decode(e, 0, e.Length);
			bufferedWaveProvider.AddSamples(array, 0, array.Count());
		}
	}

	public static void Stop()
	{
		if (waveOut != null)
		{
			waveOut.Dispose();
		}
		if (g722 != null)
		{
			g722.Dispose();
		}
	}
}
