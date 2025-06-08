using System;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

[ComVisible(true)]
public class LoadException : Exception
{
	private Exception _outerException;

	public Exception OuterException => _outerException;

	public override string Message
	{
		get
		{
			if (_outerException == null)
			{
				return base.Message;
			}
			return $"{base.Message} {_outerException.Message}.";
		}
	}

	public LoadException(string message, Exception innerException, Exception outerException)
		: base(message, innerException)
	{
		_outerException = outerException;
	}
}
