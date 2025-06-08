using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public enum Severity
{
	Critical = 1,
	Error,
	Warning,
	Information,
	Verbose,
	Debug,
	Trace
}
