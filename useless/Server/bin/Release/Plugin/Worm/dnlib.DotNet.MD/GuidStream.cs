using System;
using System.Runtime.InteropServices;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public sealed class GuidStream : HeapStream
{
	public GuidStream()
	{
	}

	public GuidStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
		: base(mdReaderFactory, metadataBaseOffset, streamHeader)
	{
	}

	public override bool IsValidIndex(uint index)
	{
		if (index != 0)
		{
			if (index <= 268435456)
			{
				return IsValidOffset((index - 1) * 16, 16);
			}
			return false;
		}
		return true;
	}

	public Guid? Read(uint index)
	{
		if (index == 0 || !IsValidIndex(index))
		{
			return null;
		}
		DataReader dataReader = base.dataReader;
		dataReader.Position = (index - 1) * 16;
		return dataReader.ReadGuid();
	}
}
