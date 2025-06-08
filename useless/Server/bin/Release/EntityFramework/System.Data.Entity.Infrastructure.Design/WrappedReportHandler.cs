namespace System.Data.Entity.Infrastructure.Design;

internal class WrappedReportHandler : IReportHandler
{
	private readonly IReportHandler _handler;

	public WrappedReportHandler(object handler)
	{
		if (handler != null)
		{
			HandlerBase handlerBase = (handler as HandlerBase) ?? new ForwardingProxy<HandlerBase>(handler).GetTransparentProxy();
			_handler = (handler as IReportHandler) ?? (handlerBase.ImplementsContract(typeof(IReportHandler).FullName) ? new ForwardingProxy<IReportHandler>(handler).GetTransparentProxy() : null);
		}
	}

	public void OnError(string message)
	{
		_handler?.OnError(message);
	}

	public void OnInformation(string message)
	{
		_handler?.OnInformation(message);
	}

	public void OnVerbose(string message)
	{
		_handler?.OnVerbose(message);
	}

	public void OnWarning(string message)
	{
		_handler?.OnWarning(message);
	}
}
