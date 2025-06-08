using System.Runtime.InteropServices;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public enum FlowControl
{
	Branch,
	Break,
	Call,
	Cond_Branch,
	Meta,
	Next,
	Phi,
	Return,
	Throw
}
