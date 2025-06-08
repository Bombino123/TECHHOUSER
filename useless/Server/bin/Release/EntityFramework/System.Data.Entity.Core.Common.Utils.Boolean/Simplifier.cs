using System.Collections.Generic;

namespace System.Data.Entity.Core.Common.Utils.Boolean;

internal class Simplifier<T_Identifier> : BasicVisitor<T_Identifier>
{
	internal static readonly Simplifier<T_Identifier> Instance = new Simplifier<T_Identifier>();

	protected Simplifier()
	{
	}

	internal override BoolExpr<T_Identifier> VisitNot(NotExpr<T_Identifier> expression)
	{
		BoolExpr<T_Identifier> boolExpr = expression.Child.Accept(this);
		return boolExpr.ExprType switch
		{
			ExprType.Not => ((NotExpr<T_Identifier>)boolExpr).Child, 
			ExprType.True => FalseExpr<T_Identifier>.Value, 
			ExprType.False => TrueExpr<T_Identifier>.Value, 
			_ => base.VisitNot(expression), 
		};
	}

	internal override BoolExpr<T_Identifier> VisitAnd(AndExpr<T_Identifier> expression)
	{
		return SimplifyTree(expression);
	}

	internal override BoolExpr<T_Identifier> VisitOr(OrExpr<T_Identifier> expression)
	{
		return SimplifyTree(expression);
	}

	private BoolExpr<T_Identifier> SimplifyTree(TreeExpr<T_Identifier> tree)
	{
		bool flag = tree.ExprType == ExprType.And;
		List<BoolExpr<T_Identifier>> list = new List<BoolExpr<T_Identifier>>(tree.Children.Count);
		foreach (BoolExpr<T_Identifier> child in tree.Children)
		{
			BoolExpr<T_Identifier> boolExpr = child.Accept(this);
			if (boolExpr.ExprType == tree.ExprType)
			{
				list.AddRange(((TreeExpr<T_Identifier>)boolExpr).Children);
			}
			else
			{
				list.Add(boolExpr);
			}
		}
		Dictionary<BoolExpr<T_Identifier>, bool> dictionary = new Dictionary<BoolExpr<T_Identifier>, bool>(tree.Children.Count);
		List<BoolExpr<T_Identifier>> list2 = new List<BoolExpr<T_Identifier>>(tree.Children.Count);
		foreach (BoolExpr<T_Identifier> item in list)
		{
			switch (item.ExprType)
			{
			case ExprType.Not:
				dictionary[((NotExpr<T_Identifier>)item).Child] = true;
				break;
			case ExprType.False:
				if (flag)
				{
					return FalseExpr<T_Identifier>.Value;
				}
				break;
			case ExprType.True:
				if (!flag)
				{
					return TrueExpr<T_Identifier>.Value;
				}
				break;
			default:
				list2.Add(item);
				break;
			}
		}
		List<BoolExpr<T_Identifier>> list3 = new List<BoolExpr<T_Identifier>>();
		foreach (BoolExpr<T_Identifier> item2 in list2)
		{
			if (dictionary.ContainsKey(item2))
			{
				if (flag)
				{
					return FalseExpr<T_Identifier>.Value;
				}
				return TrueExpr<T_Identifier>.Value;
			}
			list3.Add(item2);
		}
		foreach (BoolExpr<T_Identifier> key in dictionary.Keys)
		{
			list3.Add(key.MakeNegated());
		}
		if (list3.Count == 0)
		{
			if (flag)
			{
				return TrueExpr<T_Identifier>.Value;
			}
			return FalseExpr<T_Identifier>.Value;
		}
		if (1 == list3.Count)
		{
			return list3[0];
		}
		if (flag)
		{
			return new AndExpr<T_Identifier>(list3);
		}
		return new OrExpr<T_Identifier>(list3);
	}
}
