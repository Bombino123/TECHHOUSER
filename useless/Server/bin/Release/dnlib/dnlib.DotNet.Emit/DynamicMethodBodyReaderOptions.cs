using System;

namespace dnlib.DotNet.Emit;

[Flags]
public enum DynamicMethodBodyReaderOptions
{
	None = 0,
	UnknownDeclaringType = 1
}
