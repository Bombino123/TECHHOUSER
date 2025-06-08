using System;

namespace NAudio;

public class MmException : Exception
{
	public MmResult Result { get; }

	public string Function { get; }

	public MmException(MmResult result, string function)
		: base(ErrorMessage(result, function))
	{
		Result = result;
		Function = function;
	}

	private static string ErrorMessage(MmResult result, string function)
	{
		return $"{result} calling {function}";
	}

	public static void Try(MmResult result, string function)
	{
		if (result != 0)
		{
			throw new MmException(result, function);
		}
	}
}
