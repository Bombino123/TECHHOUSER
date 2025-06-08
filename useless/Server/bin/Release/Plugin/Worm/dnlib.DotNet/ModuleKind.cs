using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public enum ModuleKind
{
	Console,
	Windows,
	Dll,
	NetModule
}
