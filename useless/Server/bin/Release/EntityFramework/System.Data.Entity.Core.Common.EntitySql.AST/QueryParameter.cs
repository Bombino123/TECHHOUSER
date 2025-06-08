using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class QueryParameter : Node
{
	private readonly string _name;

	internal string Name => _name;

	internal QueryParameter(string parameterName, string query, int inputPos)
		: base(query, inputPos)
	{
		_name = parameterName.Substring(1);
		if (_name.StartsWith("_", StringComparison.OrdinalIgnoreCase) || char.IsDigit(_name, 0))
		{
			ErrorContext errCtx = base.ErrCtx;
			string errorMessage = Strings.InvalidParameterFormat(_name);
			throw EntitySqlException.Create(errCtx, errorMessage, null);
		}
	}
}
