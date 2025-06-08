using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal abstract class BoolLiteral : InternalBase
{
	private sealed class BoolLiteralComparer : IEqualityComparer<BoolLiteral>
	{
		public bool Equals(BoolLiteral left, BoolLiteral right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			return left.IsEqualTo(right);
		}

		public int GetHashCode(BoolLiteral literal)
		{
			return literal.GetHashCode();
		}
	}

	private sealed class IdentifierComparer : IEqualityComparer<BoolLiteral>
	{
		public bool Equals(BoolLiteral left, BoolLiteral right)
		{
			if (left == right)
			{
				return true;
			}
			if (left == null || right == null)
			{
				return false;
			}
			return left.IsIdentifierEqualTo(right);
		}

		public int GetHashCode(BoolLiteral literal)
		{
			return literal.GetIdentifierHash();
		}
	}

	internal static readonly IEqualityComparer<BoolLiteral> EqualityComparer = new BoolLiteralComparer();

	internal static readonly IEqualityComparer<BoolLiteral> EqualityIdentifierComparer = new IdentifierComparer();

	internal static TermExpr<DomainConstraint<BoolLiteral, Constant>> MakeTermExpression(BoolLiteral literal, IEnumerable<Constant> domain, IEnumerable<Constant> range)
	{
		Set<Constant> domain2 = new Set<Constant>(domain, Constant.EqualityComparer);
		Set<Constant> range2 = new Set<Constant>(range, Constant.EqualityComparer);
		return MakeTermExpression(literal, domain2, range2);
	}

	internal static TermExpr<DomainConstraint<BoolLiteral, Constant>> MakeTermExpression(BoolLiteral literal, Set<Constant> domain, Set<Constant> range)
	{
		domain.MakeReadOnly();
		range.MakeReadOnly();
		DomainConstraint<BoolLiteral, Constant> identifier = new DomainConstraint<BoolLiteral, Constant>(new DomainVariable<BoolLiteral, Constant>(literal, domain, EqualityIdentifierComparer), range);
		return new TermExpr<DomainConstraint<BoolLiteral, Constant>>(EqualityComparer<DomainConstraint<BoolLiteral, Constant>>.Default, identifier);
	}

	internal abstract BoolExpr<DomainConstraint<BoolLiteral, Constant>> FixRange(Set<Constant> range, MemberDomainMap memberDomainMap);

	internal abstract BoolExpr<DomainConstraint<BoolLiteral, Constant>> GetDomainBoolExpression(MemberDomainMap domainMap);

	internal abstract BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap);

	internal abstract void GetRequiredSlots(MemberProjectionIndex projectedSlotMap, bool[] requiredSlots);

	internal abstract StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull);

	internal abstract DbExpression AsCqt(DbExpression row, bool skipIsNotNull);

	internal abstract StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull);

	internal abstract StringBuilder AsNegatedUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull);

	protected virtual bool IsIdentifierEqualTo(BoolLiteral right)
	{
		return IsEqualTo(right);
	}

	protected abstract bool IsEqualTo(BoolLiteral right);

	protected virtual int GetIdentifierHash()
	{
		return GetHashCode();
	}
}
