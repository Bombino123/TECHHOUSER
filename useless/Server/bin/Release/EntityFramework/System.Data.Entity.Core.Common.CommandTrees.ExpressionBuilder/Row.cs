using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

public sealed class Row
{
	private readonly ReadOnlyCollection<KeyValuePair<string, DbExpression>> arguments;

	public Row(KeyValuePair<string, DbExpression> columnValue, params KeyValuePair<string, DbExpression>[] columnValues)
	{
		arguments = new ReadOnlyCollection<KeyValuePair<string, DbExpression>>(Helpers.Prepend(columnValues, columnValue));
	}

	public DbNewInstanceExpression ToExpression()
	{
		return DbExpressionBuilder.NewRow(arguments);
	}

	public static implicit operator DbExpression(Row row)
	{
		Check.NotNull(row, "row");
		return row.ToExpression();
	}
}
