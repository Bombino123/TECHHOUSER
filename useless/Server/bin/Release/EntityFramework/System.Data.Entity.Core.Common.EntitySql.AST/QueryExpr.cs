namespace System.Data.Entity.Core.Common.EntitySql.AST;

internal sealed class QueryExpr : Node
{
	private readonly SelectClause _selectClause;

	private readonly FromClause _fromClause;

	private readonly Node _whereClause;

	private readonly GroupByClause _groupByClause;

	private readonly HavingClause _havingClause;

	private readonly OrderByClause _orderByClause;

	internal SelectClause SelectClause => _selectClause;

	internal FromClause FromClause => _fromClause;

	internal Node WhereClause => _whereClause;

	internal GroupByClause GroupByClause => _groupByClause;

	internal HavingClause HavingClause => _havingClause;

	internal OrderByClause OrderByClause => _orderByClause;

	internal bool HasMethodCall
	{
		get
		{
			if (!_selectClause.HasMethodCall && (_havingClause == null || !_havingClause.HasMethodCall))
			{
				if (_orderByClause != null)
				{
					return _orderByClause.HasMethodCall;
				}
				return false;
			}
			return true;
		}
	}

	internal QueryExpr(SelectClause selectClause, FromClause fromClause, Node whereClause, GroupByClause groupByClause, HavingClause havingClause, OrderByClause orderByClause)
	{
		_selectClause = selectClause;
		_fromClause = fromClause;
		_whereClause = whereClause;
		_groupByClause = groupByClause;
		_havingClause = havingClause;
		_orderByClause = orderByClause;
	}
}
