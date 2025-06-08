using System;

namespace dnlib.DotNet.Emit;

[Flags]
public enum ExceptionHandlerType
{
	Catch = 0,
	Filter = 1,
	Finally = 2,
	Fault = 4,
	Duplicated = 8
}
