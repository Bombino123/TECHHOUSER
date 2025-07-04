using System;

namespace NAudio.Wave.SampleProviders;

public class SampleProviderEventArgs : EventArgs
{
	public ISampleProvider SampleProvider { get; private set; }

	public SampleProviderEventArgs(ISampleProvider sampleProvider)
	{
		SampleProvider = sampleProvider;
	}
}
