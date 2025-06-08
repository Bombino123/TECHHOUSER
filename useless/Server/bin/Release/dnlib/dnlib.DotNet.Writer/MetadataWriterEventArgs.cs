using System;

namespace dnlib.DotNet.Writer;

public readonly struct MetadataWriterEventArgs
{
	public Metadata Metadata { get; }

	public MetadataEvent Event { get; }

	public MetadataWriterEventArgs(Metadata metadata, MetadataEvent @event)
	{
		Metadata = metadata ?? throw new ArgumentNullException("metadata");
		Event = @event;
	}
}
