using System;
using System.Diagnostics;
using dnlib.IO;

namespace dnlib.DotNet.MD;

[DebuggerDisplay("{dataReader.Length} {streamHeader.Name}")]
public abstract class DotNetStream : IFileSection, IDisposable
{
	protected DataReader dataReader;

	private StreamHeader streamHeader;

	private DataReaderFactory mdReaderFactory;

	private uint metadataBaseOffset;

	public FileOffset StartOffset => (FileOffset)dataReader.StartOffset;

	public FileOffset EndOffset => (FileOffset)dataReader.EndOffset;

	public uint StreamLength => dataReader.Length;

	public StreamHeader StreamHeader => streamHeader;

	public string Name
	{
		get
		{
			if (streamHeader != null)
			{
				return streamHeader.Name;
			}
			return string.Empty;
		}
	}

	public DataReader CreateReader()
	{
		return dataReader;
	}

	protected DotNetStream()
	{
		streamHeader = null;
		dataReader = default(DataReader);
	}

	protected DotNetStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
	{
		this.mdReaderFactory = mdReaderFactory;
		mdReaderFactory.DataReaderInvalidated += DataReaderFactory_DataReaderInvalidated;
		this.mdReaderFactory = mdReaderFactory;
		this.metadataBaseOffset = metadataBaseOffset;
		this.streamHeader = streamHeader;
		RecreateReader(mdReaderFactory, metadataBaseOffset, streamHeader, notifyThisClass: false);
	}

	private void DataReaderFactory_DataReaderInvalidated(object sender, EventArgs e)
	{
		RecreateReader(mdReaderFactory, metadataBaseOffset, streamHeader, notifyThisClass: true);
	}

	private void RecreateReader(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader, bool notifyThisClass)
	{
		if (mdReaderFactory == null || streamHeader == null)
		{
			dataReader = default(DataReader);
		}
		else
		{
			dataReader = mdReaderFactory.CreateReader(metadataBaseOffset + streamHeader.Offset, streamHeader.StreamSize);
		}
		if (notifyThisClass)
		{
			OnReaderRecreated();
		}
	}

	protected virtual void OnReaderRecreated()
	{
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			DataReaderFactory dataReaderFactory = mdReaderFactory;
			if (dataReaderFactory != null)
			{
				dataReaderFactory.DataReaderInvalidated -= DataReaderFactory_DataReaderInvalidated;
			}
			streamHeader = null;
			mdReaderFactory = null;
		}
	}

	public virtual bool IsValidIndex(uint index)
	{
		return IsValidOffset(index);
	}

	public bool IsValidOffset(uint offset)
	{
		if (offset != 0)
		{
			return offset < dataReader.Length;
		}
		return true;
	}

	public bool IsValidOffset(uint offset, int size)
	{
		if (size == 0)
		{
			return IsValidOffset(offset);
		}
		if (size > 0)
		{
			return (ulong)((long)offset + (long)(uint)size) <= (ulong)dataReader.Length;
		}
		return false;
	}
}
