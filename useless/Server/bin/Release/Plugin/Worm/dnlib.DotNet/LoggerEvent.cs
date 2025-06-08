using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public enum LoggerEvent
{
	Error,
	Warning,
	Info,
	Verbose,
	VeryVerbose
}
