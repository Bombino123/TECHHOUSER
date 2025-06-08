using System;

namespace NAudio.Wave.Asio;

internal class AsioException : Exception
{
	private AsioError error;

	public AsioError Error
	{
		get
		{
			return error;
		}
		set
		{
			error = value;
			Data["ASIOError"] = error;
		}
	}

	public AsioException()
	{
	}

	public AsioException(string message)
		: base(message)
	{
	}

	public AsioException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public static string getErrorName(AsioError error)
	{
		return Enum.GetName(typeof(AsioError), error);
	}
}
