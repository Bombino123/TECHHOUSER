namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal abstract class Node
{
	private ErrorContext _errCtx = new ErrorContext();

	internal ErrorContext ErrCtx
	{
		get
		{
			return _errCtx;
		}
		set
		{
			_errCtx = value;
		}
	}

	internal Node()
	{
	}

	internal Node(string commandText, int inputPosition)
	{
		_errCtx.CommandText = commandText;
		_errCtx.InputPosition = inputPosition;
	}
}
