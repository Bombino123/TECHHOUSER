using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class Identifier : Node
{
	private readonly string _name;

	private readonly bool _isEscaped;

	internal string Name => _name;

	internal bool IsEscaped => _isEscaped;

	internal Identifier(string name, bool isEscaped, string query, int inputPos)
		: base(query, inputPos)
	{
		if (!isEscaped)
		{
			bool isIdentifierASCII = true;
			if (!CqlLexer.IsLetterOrDigitOrUnderscore(name, out isIdentifierASCII))
			{
				if (isIdentifierASCII)
				{
					ErrorContext errCtx = base.ErrCtx;
					string errorMessage = Strings.InvalidSimpleIdentifier(name);
					throw EntitySqlException.Create(errCtx, errorMessage, null);
				}
				ErrorContext errCtx2 = base.ErrCtx;
				string errorMessage2 = Strings.InvalidSimpleIdentifierNonASCII(name);
				throw EntitySqlException.Create(errCtx2, errorMessage2, null);
			}
		}
		_name = name;
		_isEscaped = isEscaped;
	}
}
