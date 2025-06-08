using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public readonly struct ModuleWriterProgressEventArgs
{
	public ModuleWriterBase Writer { get; }

	public double Progress { get; }

	public ModuleWriterProgressEventArgs(ModuleWriterBase writer, double progress)
	{
		if (progress < 0.0 || progress > 1.0)
		{
			throw new ArgumentOutOfRangeException("progress");
		}
		Writer = writer ?? throw new ArgumentNullException("writer");
		Progress = progress;
	}
}
