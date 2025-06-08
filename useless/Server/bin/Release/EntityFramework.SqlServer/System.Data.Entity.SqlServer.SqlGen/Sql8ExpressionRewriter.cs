using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.SqlServer.Utilities;
using System.Globalization;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class Sql8ExpressionRewriter : DbExpressionRebinder
{
	internal static DbQueryCommandTree Rewrite(DbQueryCommandTree originalTree)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		DbExpression val = ((DefaultExpressionVisitor)new Sql8ExpressionRewriter(((DbCommandTree)originalTree).MetadataWorkspace)).VisitExpression(originalTree.Query);
		return new DbQueryCommandTree(((DbCommandTree)originalTree).MetadataWorkspace, ((DbCommandTree)originalTree).DataSpace, val, false);
	}

	private Sql8ExpressionRewriter(MetadataWorkspace metadata)
		: base(metadata)
	{
	}

	public override DbExpression Visit(DbExceptExpression e)
	{
		Check.NotNull<DbExceptExpression>(e, "e");
		return TransformIntersectOrExcept(((DefaultExpressionVisitor)this).VisitExpression(((DbBinaryExpression)e).Left), ((DefaultExpressionVisitor)this).VisitExpression(((DbBinaryExpression)e).Right), (DbExpressionKind)14);
	}

	public override DbExpression Visit(DbIntersectExpression e)
	{
		Check.NotNull<DbIntersectExpression>(e, "e");
		return TransformIntersectOrExcept(((DefaultExpressionVisitor)this).VisitExpression(((DbBinaryExpression)e).Left), ((DefaultExpressionVisitor)this).VisitExpression(((DbBinaryExpression)e).Right), (DbExpressionKind)22);
	}

	public override DbExpression Visit(DbSkipExpression e)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		Check.NotNull<DbSkipExpression>(e, "e");
		DbExpression right = (DbExpression)(object)DbExpressionBuilder.Limit((DbExpression)(object)DbExpressionBuilder.Sort(((DefaultExpressionVisitor)this).VisitExpressionBinding(e.Input), (IEnumerable<DbSortClause>)((DefaultExpressionVisitor)this).VisitSortOrder(e.SortOrder)), ((DefaultExpressionVisitor)this).VisitExpression(e.Count));
		DbExpression left = ((DefaultExpressionVisitor)this).VisitExpression(e.Input.Expression);
		IList<DbSortClause> list = ((DefaultExpressionVisitor)this).VisitSortOrder(e.SortOrder);
		IList<DbPropertyExpression> list2 = new List<DbPropertyExpression>(e.SortOrder.Count);
		foreach (DbSortClause item in list)
		{
			if ((int)item.Expression.ExpressionKind == 46)
			{
				list2.Add((DbPropertyExpression)item.Expression);
			}
		}
		return (DbExpression)(object)DbExpressionBuilder.Sort(DbExpressionBuilder.BindAs(TransformIntersectOrExcept(left, right, (DbExpressionKind)51, list2, e.Input.VariableName), e.Input.VariableName), (IEnumerable<DbSortClause>)list);
	}

	private DbExpression TransformIntersectOrExcept(DbExpression left, DbExpression right, DbExpressionKind expressionKind)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return TransformIntersectOrExcept(left, right, expressionKind, null, null);
	}

	private DbExpression TransformIntersectOrExcept(DbExpression left, DbExpression right, DbExpressionKind expressionKind, IList<DbPropertyExpression> sortExpressionsOverLeft, string sortExpressionsBindingVariableName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		bool flag = (int)expressionKind == 14 || (int)expressionKind == 51;
		bool flag2 = (int)expressionKind == 14 || (int)expressionKind == 22;
		DbExpressionBinding val = DbExpressionBuilder.Bind(left);
		DbExpressionBinding val2 = DbExpressionBuilder.Bind(right);
		IList<DbPropertyExpression> list = new List<DbPropertyExpression>();
		IList<DbPropertyExpression> list2 = new List<DbPropertyExpression>();
		FlattenProperties((DbExpression)(object)val.Variable, list);
		FlattenProperties((DbExpression)(object)val2.Variable, list2);
		if ((int)expressionKind == 51 && RemoveNonSortProperties(list, list2, sortExpressionsOverLeft, val.VariableName, sortExpressionsBindingVariableName))
		{
			val2 = CapWithProject(val2, list2);
		}
		DbExpression val3 = null;
		for (int i = 0; i < list.Count; i++)
		{
			DbComparisonExpression obj = DbExpressionBuilder.Equal((DbExpression)(object)list[i], (DbExpression)(object)list2[i]);
			DbIsNullExpression obj2 = DbExpressionBuilder.IsNull((DbExpression)(object)list[i]);
			DbExpression val4 = (DbExpression)(object)DbExpressionBuilder.IsNull((DbExpression)(object)list2[i]);
			DbExpression val5 = (DbExpression)(object)DbExpressionBuilder.And((DbExpression)(object)obj2, val4);
			DbExpression val6 = (DbExpression)(object)DbExpressionBuilder.Or((DbExpression)(object)obj, val5);
			val3 = (DbExpression)((i != 0) ? ((object)DbExpressionBuilder.And(val3, val6)) : ((object)val6));
		}
		DbExpression val7 = (DbExpression)(object)DbExpressionBuilder.Any(val2, val3);
		DbExpression val8 = (DbExpression)((!flag) ? ((object)val7) : ((object)DbExpressionBuilder.Not(val7)));
		DbExpression val9 = (DbExpression)(object)DbExpressionBuilder.Filter(val, val8);
		if (flag2)
		{
			val9 = (DbExpression)(object)DbExpressionBuilder.Distinct(val9);
		}
		return val9;
	}

	private void FlattenProperties(DbExpression input, IList<DbPropertyExpression> flattenedProperties)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Invalid comparison between I4 and Unknown
		foreach (EdmProperty property in input.ResultType.GetProperties())
		{
			DbPropertyExpression val = DbExpressionBuilder.Property(input, property);
			if (26 == (int)((MetadataItem)((EdmMember)property).TypeUsage.EdmType).BuiltInTypeKind)
			{
				flattenedProperties.Add(val);
			}
			else
			{
				FlattenProperties((DbExpression)(object)val, flattenedProperties);
			}
		}
	}

	private static bool RemoveNonSortProperties(IList<DbPropertyExpression> list1, IList<DbPropertyExpression> list2, IList<DbPropertyExpression> sortList, string list1BindingVariableName, string sortExpressionsBindingVariableName)
	{
		bool result = false;
		for (int num = list1.Count - 1; num >= 0; num--)
		{
			if (!HasMatchInList(list1[num], sortList, list1BindingVariableName, sortExpressionsBindingVariableName))
			{
				list1.RemoveAt(num);
				list2.RemoveAt(num);
				result = true;
			}
		}
		return result;
	}

	private static bool HasMatchInList(DbPropertyExpression expr, IList<DbPropertyExpression> list, string exprBindingVariableName, string listExpressionsBindingVariableName)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (AreMatching(expr, list[i], exprBindingVariableName, listExpressionsBindingVariableName))
			{
				list.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	private static bool AreMatching(DbPropertyExpression expr1, DbPropertyExpression expr2, string expr1BindingVariableName, string expr2BindingVariableName)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Expected O, but got Unknown
		if (expr1.Property.Name != expr2.Property.Name)
		{
			return false;
		}
		if (expr1.Instance.ExpressionKind != expr2.Instance.ExpressionKind)
		{
			return false;
		}
		if ((int)expr1.Instance.ExpressionKind == 46)
		{
			return AreMatching((DbPropertyExpression)expr1.Instance, (DbPropertyExpression)expr2.Instance, expr1BindingVariableName, expr2BindingVariableName);
		}
		DbVariableReferenceExpression val = (DbVariableReferenceExpression)expr1.Instance;
		DbVariableReferenceExpression val2 = (DbVariableReferenceExpression)expr2.Instance;
		if (string.Equals(val.VariableName, expr1BindingVariableName, StringComparison.Ordinal))
		{
			return string.Equals(val2.VariableName, expr2BindingVariableName, StringComparison.Ordinal);
		}
		return false;
	}

	private static DbExpressionBinding CapWithProject(DbExpressionBinding inputBinding, IList<DbPropertyExpression> flattenedProperties)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		List<KeyValuePair<string, DbExpression>> list = new List<KeyValuePair<string, DbExpression>>(flattenedProperties.Count);
		Dictionary<string, int> dictionary = new Dictionary<string, int>(flattenedProperties.Count);
		foreach (DbPropertyExpression flattenedProperty in flattenedProperties)
		{
			string text = flattenedProperty.Property.Name;
			if (dictionary.TryGetValue(text, out var value))
			{
				string text2;
				do
				{
					value++;
					text2 = text + value.ToString(CultureInfo.InvariantCulture);
				}
				while (dictionary.ContainsKey(text2));
				dictionary[text] = value;
				text = text2;
			}
			dictionary[text] = 0;
			list.Add(new KeyValuePair<string, DbExpression>(text, (DbExpression)(object)flattenedProperty));
		}
		DbExpression val = (DbExpression)(object)DbExpressionBuilder.NewRow((IEnumerable<KeyValuePair<string, DbExpression>>)list);
		DbExpressionBinding val2 = DbExpressionBuilder.Bind((DbExpression)(object)DbExpressionBuilder.Project(inputBinding, val));
		flattenedProperties.Clear();
		RowType val3 = (RowType)val.ResultType.EdmType;
		foreach (KeyValuePair<string, DbExpression> item in list)
		{
			EdmProperty val4 = val3.Properties[item.Key];
			flattenedProperties.Add(DbExpressionBuilder.Property((DbExpression)(object)val2.Variable, val4));
		}
		return val2;
	}
}
