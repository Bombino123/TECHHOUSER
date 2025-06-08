using System;

namespace dnlib.DotNet.Writer;

public readonly struct MetadataProgressEventArgs
{
	public Metadata Metadata { get; }

	public double Progress { get; }

	public MetadataProgressEventArgs(Metadata metadata, double progress)
	{
		if (progress < 0.0 || progress > 1.0)
		{
			throw new ArgumentOutOfRangeException("progress");
		}
		Metadata = metadata ?? throw new ArgumentNullException("metadata");
		Progress = progress;
	}
}
