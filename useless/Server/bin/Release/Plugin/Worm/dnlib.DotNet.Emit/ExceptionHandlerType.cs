using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Emit;

[Flags]
[ComVisible(true)]
public enum ExceptionHandlerType
{
	Catch = 0,
	Filter = 1,
	Finally = 2,
	Fault = 4,
	Duplicated = 8
}
