using System;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class LogEntry : EventArgs
{
	public DateTime Time;

	public Severity Severity;

	public string Source;

	public string Message;

	public LogEntry(DateTime time, Severity severity, string source, string message)
	{
		Time = time;
		Severity = severity;
		Source = source;
		Message = message;
	}
}
