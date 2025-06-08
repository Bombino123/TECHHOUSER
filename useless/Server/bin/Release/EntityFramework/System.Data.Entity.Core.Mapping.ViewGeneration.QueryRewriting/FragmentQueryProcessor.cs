using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;

internal class FragmentQueryProcessor : TileQueryProcessor<FragmentQuery>
{
	private class AttributeSetComparator : IEqualityComparer<HashSet<MemberPath>>
	{
		public bool Equals(HashSet<MemberPath> x, HashSet<MemberPath> y)
		{
			return x.SetEquals(y);
		}

		public int GetHashCode(HashSet<MemberPath> attrs)
		{
			int num = 123;
			foreach (MemberPath attr in attrs)
			{
				num += MemberPath.EqualityComparer.GetHashCode(attr) * 7;
			}
			return num;
		}
	}

	private readonly FragmentQueryKBChaseSupport _kb;

	internal FragmentQueryKB KnowledgeBase => _kb;

	public FragmentQueryProcessor(FragmentQueryKBChaseSupport kb)
	{
		_kb = kb;
	}

	internal static FragmentQueryProcessor Merge(FragmentQueryProcessor qp1, FragmentQueryProcessor qp2)
	{
		FragmentQueryKBChaseSupport fragmentQueryKBChaseSupport = new FragmentQueryKBChaseSupport();
		fragmentQueryKBChaseSupport.AddKnowledgeBase(qp1.KnowledgeBase);
		fragmentQueryKBChaseSupport.AddKnowledgeBase(qp2.KnowledgeBase);
		return new FragmentQueryProcessor(fragmentQueryKBChaseSupport);
	}

	internal override FragmentQuery Union(FragmentQuery q1, FragmentQuery q2)
	{
		HashSet<MemberPath> hashSet = new HashSet<MemberPath>(q1.Attributes);
		hashSet.IntersectWith(q2.Attributes);
		BoolExpression whereClause = BoolExpression.CreateOr(q1.Condition, q2.Condition);
		return FragmentQuery.Create(hashSet, whereClause);
	}

	internal bool IsDisjointFrom(FragmentQuery q1, FragmentQuery q2)
	{
		return !IsSatisfiable(Intersect(q1, q2));
	}

	internal bool IsContainedIn(FragmentQuery q1, FragmentQuery q2)
	{
		return !IsSatisfiable(Difference(q1, q2));
	}

	internal bool IsEquivalentTo(FragmentQuery q1, FragmentQuery q2)
	{
		if (IsContainedIn(q1, q2))
		{
			return IsContainedIn(q2, q1);
		}
		return false;
	}

	internal override FragmentQuery Intersect(FragmentQuery q1, FragmentQuery q2)
	{
		HashSet<MemberPath> hashSet = new HashSet<MemberPath>(q1.Attributes);
		hashSet.IntersectWith(q2.Attributes);
		BoolExpression whereClause = BoolExpression.CreateAnd(q1.Condition, q2.Condition);
		return FragmentQuery.Create(hashSet, whereClause);
	}

	internal override FragmentQuery Difference(FragmentQuery qA, FragmentQuery qB)
	{
		return FragmentQuery.Create(qA.Attributes, BoolExpression.CreateAndNot(qA.Condition, qB.Condition));
	}

	internal override bool IsSatisfiable(FragmentQuery query)
	{
		return IsSatisfiable(query.Condition);
	}

	private bool IsSatisfiable(BoolExpression condition)
	{
		return _kb.IsSatisfiable(condition.Tree);
	}

	internal override FragmentQuery CreateDerivedViewBySelectingConstantAttributes(FragmentQuery view)
	{
		HashSet<MemberPath> hashSet = new HashSet<MemberPath>();
		foreach (DomainVariable<BoolLiteral, Constant> variable in view.Condition.Variables)
		{
			if (!(variable.Identifier is MemberRestriction memberRestriction))
			{
				continue;
			}
			MemberPath memberPath = memberRestriction.RestrictedMemberSlot.MemberPath;
			Domain domain = memberRestriction.Domain;
			if (view.Attributes.Contains(memberPath) || domain.AllPossibleValues.Any((Constant it) => it.HasNotNull()))
			{
				continue;
			}
			foreach (Constant value in domain.Values)
			{
				DomainConstraint<BoolLiteral, Constant> identifier = new DomainConstraint<BoolLiteral, Constant>(variable, new Set<Constant>(new Constant[1] { value }, Constant.EqualityComparer));
				BoolExpression condition = view.Condition.Create(new AndExpr<DomainConstraint<BoolLiteral, Constant>>(view.Condition.Tree, new NotExpr<DomainConstraint<BoolLiteral, Constant>>(new TermExpr<DomainConstraint<BoolLiteral, Constant>>(identifier))));
				if (!IsSatisfiable(condition))
				{
					hashSet.Add(memberPath);
				}
			}
		}
		if (hashSet.Count > 0)
		{
			hashSet.UnionWith(view.Attributes);
			return new FragmentQuery(string.Format(CultureInfo.InvariantCulture, "project({0})", new object[1] { view.Description }), view.FromVariable, hashSet, view.Condition);
		}
		return null;
	}

	public override string ToString()
	{
		return _kb.ToString();
	}
}
