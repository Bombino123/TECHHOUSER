using System;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public class UnsupportedInformationLevelException : Exception
{
	public UnsupportedInformationLevelException()
	{
	}

	public UnsupportedInformationLevelException(string message)
		: base(message)
	{
	}
}
