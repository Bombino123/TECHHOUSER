using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
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
