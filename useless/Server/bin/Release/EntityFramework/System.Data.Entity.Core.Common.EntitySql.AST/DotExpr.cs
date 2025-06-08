namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class DotExpr : Node
{
	private readonly Node _leftExpr;

	private readonly Identifier _identifier;

	private bool? _isMultipartIdentifierComputed;

	private string[] _names;

	internal Node Left => _leftExpr;

	internal Identifier Identifier => _identifier;

	internal DotExpr(Node leftExpr, Identifier id)
	{
		_leftExpr = leftExpr;
		_identifier = id;
	}

	internal bool IsMultipartIdentifier(out string[] names)
	{
		if (_isMultipartIdentifierComputed.HasValue)
		{
			names = _names;
			return _isMultipartIdentifierComputed.Value;
		}
		_names = null;
		if (_leftExpr is Identifier identifier)
		{
			_names = new string[2] { identifier.Name, _identifier.Name };
		}
		if (_leftExpr is DotExpr dotExpr && dotExpr.IsMultipartIdentifier(out var names2))
		{
			_names = new string[names2.Length + 1];
			names2.CopyTo(_names, 0);
			_names[_names.Length - 1] = _identifier.Name;
		}
		_isMultipartIdentifierComputed = _names != null;
		names = _names;
		return _isMultipartIdentifierComputed.Value;
	}
}
