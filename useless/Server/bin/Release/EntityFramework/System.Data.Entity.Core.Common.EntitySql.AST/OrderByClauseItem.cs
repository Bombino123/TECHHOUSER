namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class OrderByClauseItem : Node
{
	private readonly Node _orderExpr;

	private readonly OrderKind _orderKind;

	private readonly Identifier _optCollationIdentifier;

	internal Node OrderExpr => _orderExpr;

	internal OrderKind OrderKind => _orderKind;

	internal Identifier Collation => _optCollationIdentifier;

	internal OrderByClauseItem(Node orderExpr, OrderKind orderKind)
		: this(orderExpr, orderKind, null)
	{
	}

	internal OrderByClauseItem(Node orderExpr, OrderKind orderKind, Identifier optCollationIdentifier)
	{
		_orderExpr = orderExpr;
		_orderKind = orderKind;
		_optCollationIdentifier = optCollationIdentifier;
	}
}
