namespace System.Data.Entity.Infrastructure.Design;

public class ReportHandler : HandlerBase, IReportHandler
{
	private readonly Action<string> _errorHandler;

	private readonly Action<string> _warningHandler;

	private readonly Action<string> _informationHandler;

	private readonly Action<string> _verboseHandler;

	public ReportHandler(Action<string> errorHandler, Action<string> warningHandler, Action<string> informationHandler, Action<string> verboseHandler)
	{
		_errorHandler = errorHandler;
		_warningHandler = warningHandler;
		_informationHandler = informationHandler;
		_verboseHandler = verboseHandler;
	}

	public virtual void OnError(string message)
	{
		_errorHandler?.Invoke(message);
	}

	public virtual void OnWarning(string message)
	{
		_warningHandler?.Invoke(message);
	}

	public virtual void OnInformation(string message)
	{
		_informationHandler?.Invoke(message);
	}

	public virtual void OnVerbose(string message)
	{
		_verboseHandler?.Invoke(message);
	}
}
