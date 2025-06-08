namespace System.Data.Entity.Infrastructure.Design;

internal class Reporter
{
	private readonly IReportHandler _handler;

	public Reporter(IReportHandler handler)
	{
		_handler = handler;
	}

	public void WriteError(string message)
	{
		_handler?.OnError(message);
	}

	public void WriteWarning(string message)
	{
		_handler?.OnWarning(message);
	}

	public void WriteInformation(string message)
	{
		_handler?.OnInformation(message);
	}

	public void WriteVerbose(string message)
	{
		_handler?.OnVerbose(message);
	}
}
