using System;

namespace AForge;

public class DeviceBusyException : Exception
{
	public DeviceBusyException(string message)
		: base(message)
	{
	}
}
