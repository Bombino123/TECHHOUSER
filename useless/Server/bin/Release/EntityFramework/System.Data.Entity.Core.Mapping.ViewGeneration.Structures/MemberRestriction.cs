using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal abstract class MemberRestriction : BoolLiteral
{
	private readonly MemberProjectedSlot m_restrictedMemberSlot;

	private readonly Domain m_domain;

	private readonly bool m_isComplete;

	internal bool IsComplete => m_isComplete;

	internal MemberProjectedSlot RestrictedMemberSlot => m_restrictedMemberSlot;

	internal Domain Domain => m_domain;

	protected MemberRestriction(MemberProjectedSlot slot, Constant value)
		: this(slot, new Constant[1] { value })
	{
	}

	protected MemberRestriction(MemberProjectedSlot slot, IEnumerable<Constant> values)
	{
		m_restrictedMemberSlot = slot;
		m_domain = new Domain(values, values);
	}

	protected MemberRestriction(MemberProjectedSlot slot, Domain domain)
	{
		m_restrictedMemberSlot = slot;
		m_domain = domain;
		m_isComplete = true;
	}

	protected MemberRestriction(MemberProjectedSlot slot, IEnumerable<Constant> values, IEnumerable<Constant> possibleValues)
		: this(slot, new Domain(values, possibleValues))
	{
	}

	internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> GetDomainBoolExpression(MemberDomainMap domainMap)
	{
		if (domainMap != null)
		{
			IEnumerable<Constant> domain = domainMap.GetDomain(m_restrictedMemberSlot.MemberPath);
			return BoolLiteral.MakeTermExpression(this, domain, m_domain.Values);
		}
		return BoolLiteral.MakeTermExpression(this, m_domain.AllPossibleValues, m_domain.Values);
	}

	internal abstract MemberRestriction CreateCompleteMemberRestriction(IEnumerable<Constant> possibleValues);

	internal override void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots)
	{
		MemberPath memberPath = RestrictedMemberSlot.MemberPath;
		int num = projectedSlotMap.IndexOf(memberPath);
		requiredSlots[num] = true;
	}

	protected override bool IsEqualTo(BoolLiteral right)
	{
		if (!(right is MemberRestriction memberRestriction))
		{
			return false;
		}
		if (this == memberRestriction)
		{
			return true;
		}
		if (!ProjectedSlot.EqualityComparer.Equals(m_restrictedMemberSlot, memberRestriction.m_restrictedMemberSlot))
		{
			return false;
		}
		return m_domain.IsEqualTo(memberRestriction.m_domain);
	}

	public override int GetHashCode()
	{
		return ProjectedSlot.EqualityComparer.GetHashCode(m_restrictedMemberSlot) ^ m_domain.GetHash();
	}

	protected override bool IsIdentifierEqualTo(BoolLiteral right)
	{
		if (!(right is MemberRestriction memberRestriction))
		{
			return false;
		}
		if (this == memberRestriction)
		{
			return true;
		}
		return ProjectedSlot.EqualityComparer.Equals(m_restrictedMemberSlot, memberRestriction.m_restrictedMemberSlot);
	}

	protected override int GetIdentifierHash()
	{
		return ProjectedSlot.EqualityComparer.GetHashCode(m_restrictedMemberSlot);
	}

	internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		return AsEsql(builder, blockAlias, skipIsNotNull);
	}

	internal override StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		builder.Append("NOT(");
		builder = AsUserString(builder, blockAlias, skipIsNotNull);
		builder.Append(")");
		return builder;
	}
}
