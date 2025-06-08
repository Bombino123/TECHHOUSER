using System;

namespace AForge;

public class NotConnectedException : Exception
{
	public NotConnectedException(string message)
		: base(message)
	{
	}
}
