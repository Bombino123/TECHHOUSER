namespace System.Data.Entity.Infrastructure.Design;

internal class WrappedResultHandler : IResultHandler
{
	private readonly IResultHandler _handler;

	private readonly IResultHandler2 _handler2;

	public WrappedResultHandler(object handler)
	{
		HandlerBase handlerBase = (handler as HandlerBase) ?? new ForwardingProxy<HandlerBase>(handler).GetTransparentProxy();
		_handler = (handler as IResultHandler) ?? (handlerBase.ImplementsContract(typeof(IResultHandler).FullName) ? new ForwardingProxy<IResultHandler>(handler).GetTransparentProxy() : null);
		_handler2 = (handler as IResultHandler2) ?? (handlerBase.ImplementsContract(typeof(IResultHandler2).FullName) ? new ForwardingProxy<IResultHandler2>(handler).GetTransparentProxy() : null);
	}

	public void SetResult(object value)
	{
		if (_handler != null)
		{
			_handler.SetResult(value);
		}
	}

	public bool SetError(string type, string message, string stackTrace)
	{
		if (_handler2 == null)
		{
			return false;
		}
		_handler2.SetError(type, message, stackTrace);
		return true;
	}
}
