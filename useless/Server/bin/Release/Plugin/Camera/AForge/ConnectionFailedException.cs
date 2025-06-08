using System;

namespace AForge;

public class ConnectionFailedException : Exception
{
	public ConnectionFailedException(string message)
		: base(message)
	{
	}
}
