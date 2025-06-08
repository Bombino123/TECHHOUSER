using System.Collections.Generic;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class FragmentQuery : ITileQuery
{
	private class FragmentQueryEqualityComparer : IEqualityComparer<FragmentQuery>
	{
		private readonly FragmentQueryProcessor _qp;

		internal FragmentQueryEqualityComparer(FragmentQueryProcessor qp)
		{
			_qp = qp;
		}

		public bool Equals(FragmentQuery x, FragmentQuery y)
		{
			if (!x.Attributes.SetEquals(y.Attributes))
			{
				return false;
			}
			return _qp.IsEquivalentTo(x, y);
		}

		public int GetHashCode(FragmentQuery q)
		{
			int num = 0;
			foreach (MemberPath attribute in q.Attributes)
			{
				num ^= MemberPath.EqualityComparer.GetHashCode(attribute);
			}
			int num2 = 0;
			int num3 = 0;
			foreach (MemberRestriction memberRestriction in q.Condition.MemberRestrictions)
			{
				num2 ^= MemberPath.EqualityComparer.GetHashCode(memberRestriction.RestrictedMemberSlot.MemberPath);
				foreach (Constant value in memberRestriction.Domain.Values)
				{
					num3 ^= Constant.EqualityComparer.GetHashCode(value);
				}
			}
			return num * 13 + num2 * 7 + num3;
		}
	}

	private readonly BoolExpression m_fromVariable;

	private readonly string m_label;

	private readonly HashSet<MemberPath> m_attributes;

	private readonly BoolExpression m_condition;

	public HashSet<MemberPath> Attributes => m_attributes;

	public BoolExpression Condition => m_condition;

	public BoolExpression FromVariable => m_fromVariable;

	public string Description
	{
		get
		{
			string text = m_label;
			if (text == null && m_fromVariable != null)
			{
				text = m_fromVariable.ToString();
			}
			return text;
		}
	}

	public static FragmentQuery Create(BoolExpression fromVariable, CellQuery cellQuery)
	{
		BoolExpression whereClause = cellQuery.WhereClause;
		whereClause = whereClause.MakeCopy();
		whereClause.ExpensiveSimplify();
		return new FragmentQuery(null, fromVariable, new HashSet<MemberPath>(cellQuery.GetProjectedMembers()), whereClause);
	}

	public static FragmentQuery Create(string label, RoleBoolean roleBoolean, CellQuery cellQuery)
	{
		BoolExpression boolExpression = cellQuery.WhereClause.Create(roleBoolean);
		boolExpression = BoolExpression.CreateAnd(boolExpression, cellQuery.WhereClause);
		boolExpression = boolExpression.MakeCopy();
		boolExpression.ExpensiveSimplify();
		return new FragmentQuery(label, null, new HashSet<MemberPath>(), boolExpression);
	}

	public static FragmentQuery Create(IEnumerable<MemberPath> attrs, BoolExpression whereClause)
	{
		return new FragmentQuery(null, null, attrs, whereClause);
	}

	public static FragmentQuery Create(BoolExpression whereClause)
	{
		return new FragmentQuery(null, null, new MemberPath[0], whereClause);
	}

	internal FragmentQuery(string label, BoolExpression fromVariable, IEnumerable<MemberPath> attrs, BoolExpression condition)
	{
		m_label = label;
		m_fromVariable = fromVariable;
		m_condition = condition;
		m_attributes = new HashSet<MemberPath>(attrs);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (MemberPath attribute in Attributes)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(',');
			}
			stringBuilder.Append(attribute);
		}
		if (Description != null && Description != stringBuilder.ToString())
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}: [{1} where {2}]", new object[3] { Description, stringBuilder, Condition });
		}
		return string.Format(CultureInfo.InvariantCulture, "[{0} where {1}]", new object[2] { stringBuilder, Condition });
	}

	internal static BoolExpression CreateMemberCondition(MemberPath path, Constant domainValue, MemberDomainMap domainMap)
	{
		if (domainValue is TypeConstant)
		{
			return BoolExpression.CreateLiteral(new TypeRestriction(new MemberProjectedSlot(path), new Domain(domainValue, domainMap.GetDomain(path))), domainMap);
		}
		return BoolExpression.CreateLiteral(new ScalarRestriction(new MemberProjectedSlot(path), new Domain(domainValue, domainMap.GetDomain(path))), domainMap);
	}

	internal static IEqualityComparer<FragmentQuery> GetEqualityComparer(FragmentQueryProcessor qp)
	{
		return new FragmentQueryEqualityComparer(qp);
	}
}
