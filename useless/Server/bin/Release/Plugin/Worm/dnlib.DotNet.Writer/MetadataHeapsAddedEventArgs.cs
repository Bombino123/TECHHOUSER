using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public readonly struct MetadataHeapsAddedEventArgs
{
	public Metadata Metadata { get; }

	public List<IHeap> Heaps { get; }

	public MetadataHeapsAddedEventArgs(Metadata metadata, List<IHeap> heaps)
	{
		Metadata = metadata ?? throw new ArgumentNullException("metadata");
		Heaps = heaps ?? throw new ArgumentNullException("heaps");
	}
}
