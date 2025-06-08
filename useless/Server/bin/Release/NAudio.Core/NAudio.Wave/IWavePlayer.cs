using System;

namespace NAudio.Wave;

public interface IWavePlayer : IDisposable
{
	float Volume { get; set; }

	PlaybackState PlaybackState { get; }

	WaveFormat OutputWaveFormat { get; }

	event EventHandler<StoppedEventArgs> PlaybackStopped;

	void Play();

	void Stop();

	void Pause();

	void Init(IWaveProvider waveProvider);
}
