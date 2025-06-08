using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
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
