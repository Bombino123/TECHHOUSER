using System;

namespace dnlib.DotNet.Writer;

public readonly struct ModuleWriterEventArgs
{
	public ModuleWriterBase Writer { get; }

	public ModuleWriterEvent Event { get; }

	public ModuleWriterEventArgs(ModuleWriterBase writer, ModuleWriterEvent @event)
	{
		Writer = writer ?? throw new ArgumentNullException("writer");
		Event = @event;
	}
}
