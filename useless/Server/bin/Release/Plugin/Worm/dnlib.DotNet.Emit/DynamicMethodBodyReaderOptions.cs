using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Emit;

[Flags]
[ComVisible(true)]
public enum DynamicMethodBodyReaderOptions
{
	None = 0,
	UnknownDeclaringType = 1
}
