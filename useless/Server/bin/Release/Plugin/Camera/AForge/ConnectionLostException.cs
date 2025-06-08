using System;

namespace AForge;

public class ConnectionLostException : Exception
{
	public ConnectionLostException(string message)
		: base(message)
	{
	}
}
