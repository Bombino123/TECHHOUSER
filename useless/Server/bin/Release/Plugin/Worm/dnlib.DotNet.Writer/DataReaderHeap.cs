using System;
using System.Runtime.InteropServices;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class DataReaderHeap : HeapBase
{
	private readonly DataReader heapReader;

	public override string Name { get; }

	internal DotNetStream OptionalOriginalStream { get; }

	public DataReaderHeap(DotNetStream stream)
	{
		OptionalOriginalStream = stream ?? throw new ArgumentNullException("stream");
		heapReader = stream.CreateReader();
		Name = stream.Name;
	}

	public DataReaderHeap(string name, DataReader heapReader)
	{
		this.heapReader = heapReader;
		this.heapReader.Position = 0u;
		Name = name ?? throw new ArgumentNullException("name");
	}

	public override uint GetRawLength()
	{
		return heapReader.Length;
	}

	protected override void WriteToImpl(DataWriter writer)
	{
		heapReader.CopyTo(writer);
	}
}
