using System;

namespace dnlib.DotNet.Writer;

public readonly struct ContentId
{
	public readonly Guid Guid;

	public readonly uint Timestamp;

	public ContentId(Guid guid, uint timestamp)
	{
		Guid = guid;
		Timestamp = timestamp;
	}
}
