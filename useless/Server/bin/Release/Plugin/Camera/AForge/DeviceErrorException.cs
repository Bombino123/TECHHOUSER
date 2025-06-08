using System;

namespace AForge;

public class DeviceErrorException : Exception
{
	public DeviceErrorException(string message)
		: base(message)
	{
	}
}
