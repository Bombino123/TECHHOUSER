using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
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
