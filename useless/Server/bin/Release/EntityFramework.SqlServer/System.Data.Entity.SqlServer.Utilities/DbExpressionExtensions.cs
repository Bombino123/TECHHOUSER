using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;

namespace System.Data.Entity.SqlServer.Utilities;

internal static class DbExpressionExtensions
{
	public static IEnumerable<DbExpression> GetLeafNodes(this DbExpression root, DbExpressionKind kind, Func<DbExpression, IEnumerable<DbExpression>> getChildNodes)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Stack<DbExpression> nodes = new Stack<DbExpression>();
		nodes.Push(root);
		while (nodes.Count > 0)
		{
			DbExpression val = nodes.Pop();
			if (val.ExpressionKind != kind)
			{
				yield return val;
				continue;
			}
			foreach (DbExpression item in getChildNodes(val).Reverse())
			{
				nodes.Push(item);
			}
		}
	}
}
