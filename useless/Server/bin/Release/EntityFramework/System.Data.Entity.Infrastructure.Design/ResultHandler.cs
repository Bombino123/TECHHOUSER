namespace System.Data.Entity.Infrastructure.Design;

public class ResultHandler : HandlerBase, IResultHandler2, IResultHandler
{
	private bool _hasResult;

	private object _result;

	private string _errorType;

	private string _errorMessage;

	private string _errorStackTrace;

	public virtual bool HasResult => _hasResult;

	public virtual object Result => _result;

	public virtual string ErrorType => _errorType;

	public virtual string ErrorMessage => _errorMessage;

	public virtual string ErrorStackTrace => _errorStackTrace;

	public virtual void SetResult(object value)
	{
		_hasResult = true;
		_result = value;
	}

	public virtual void SetError(string type, string message, string stackTrace)
	{
		_errorType = type;
		_errorMessage = message;
		_errorStackTrace = stackTrace;
	}
}
