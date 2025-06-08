using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb;

[Flags]
[ComVisible(true)]
public enum PdbLocalAttributes
{
	None = 0,
	DebuggerHidden = 1
}
