using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public enum HeapType : uint
{
	Strings,
	Guid,
	Blob,
	US
}
