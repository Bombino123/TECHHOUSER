namespace NAudio.Wave;

public class WaveInProvider : IWaveProvider
{
	private readonly IWaveIn waveIn;

	private readonly BufferedWaveProvider bufferedWaveProvider;

	public WaveFormat WaveFormat => waveIn.WaveFormat;

	public WaveInProvider(IWaveIn waveIn)
	{
		this.waveIn = waveIn;
		waveIn.DataAvailable += OnDataAvailable;
		bufferedWaveProvider = new BufferedWaveProvider(WaveFormat);
	}

	private void OnDataAvailable(object sender, WaveInEventArgs e)
	{
		bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
	}

	public int Read(byte[] buffer, int offset, int count)
	{
		return bufferedWaveProvider.Read(buffer, offset, count);
	}
}
