using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class ScalarRestriction : MemberRestriction
{
	internal ScalarRestriction(MemberPath member, Constant value)
		: base(new MemberProjectedSlot(member), value)
	{
	}

	internal ScalarRestriction(MemberPath member, IEnumerable<Constant> values, IEnumerable<Constant> possibleValues)
		: base(new MemberProjectedSlot(member), values, possibleValues)
	{
	}

	internal ScalarRestriction(MemberProjectedSlot slot, Domain domain)
		: base(slot, domain)
	{
	}

	internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> FixRange(Set<Constant> range, MemberDomainMap memberDomainMap)
	{
		IEnumerable<Constant> domain = memberDomainMap.GetDomain(base.RestrictedMemberSlot.MemberPath);
		return new ScalarRestriction(base.RestrictedMemberSlot, new Domain(range, domain)).GetDomainBoolExpression(memberDomainMap);
	}

	internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
	{
		return new ScalarRestriction(base.RestrictedMemberSlot.RemapSlot(remap), base.Domain);
	}

	internal override MemberRestriction CreateCompleteMemberRestriction(IEnumerable<Constant> possibleValues)
	{
		return new ScalarRestriction(base.RestrictedMemberSlot, new Domain(base.Domain.Values, possibleValues));
	}

	internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		return ToStringHelper(builder, blockAlias, skipIsNotNull, userString: false);
	}

	internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
	{
		DbExpression cqt = null;
		AsCql(delegate(NegatedConstant negated, IEnumerable<Constant> domainValues)
		{
			cqt = negated.AsCqt(row, domainValues, base.RestrictedMemberSlot.MemberPath, skipIsNotNull);
		}, delegate(Set<Constant> domainValues)
		{
			cqt = base.RestrictedMemberSlot.MemberPath.AsCqt(row);
			if (domainValues.Count == 1)
			{
				cqt = cqt.Equal(domainValues.Single().AsCqt(row, base.RestrictedMemberSlot.MemberPath));
			}
			else
			{
				List<DbExpression> nodes = ((IEnumerable<Constant>)domainValues).Select((Func<Constant, DbExpression>)((Constant c) => cqt.Equal(c.AsCqt(row, base.RestrictedMemberSlot.MemberPath)))).ToList();
				cqt = Helpers.BuildBalancedTreeInPlace(nodes, (DbExpression prev, DbExpression next) => prev.Or(next));
			}
		}, delegate
		{
			DbExpression dbExpression2 = base.RestrictedMemberSlot.MemberPath.AsCqt(row).IsNull().Not();
			cqt = ((cqt != null) ? cqt.And(dbExpression2) : dbExpression2);
		}, delegate
		{
			DbExpression dbExpression = base.RestrictedMemberSlot.MemberPath.AsCqt(row).IsNull();
			cqt = ((cqt != null) ? dbExpression.Or(cqt) : dbExpression);
		}, skipIsNotNull);
		return cqt;
	}

	internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		return ToStringHelper(builder, blockAlias, skipIsNotNull, userString: true);
	}

	private StringBuilder ToStringHelper(StringBuilder inputBuilder, string blockAlias, bool skipIsNotNull, bool userString)
	{
		StringBuilder builder = new StringBuilder();
		AsCql(delegate(NegatedConstant negated, IEnumerable<Constant> domainValues)
		{
			if (userString)
			{
				negated.AsUserString(builder, blockAlias, domainValues, base.RestrictedMemberSlot.MemberPath, skipIsNotNull);
			}
			else
			{
				negated.AsEsql(builder, blockAlias, domainValues, base.RestrictedMemberSlot.MemberPath, skipIsNotNull);
			}
		}, delegate(Set<Constant> domainValues)
		{
			base.RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
			if (domainValues.Count == 1)
			{
				builder.Append(" = ");
				if (userString)
				{
					domainValues.Single().ToCompactString(builder);
				}
				else
				{
					domainValues.Single().AsEsql(builder, base.RestrictedMemberSlot.MemberPath, blockAlias);
				}
			}
			else
			{
				builder.Append(" IN {");
				bool flag = true;
				foreach (Constant domainValue in domainValues)
				{
					if (!flag)
					{
						builder.Append(", ");
					}
					if (userString)
					{
						domainValue.ToCompactString(builder);
					}
					else
					{
						domainValue.AsEsql(builder, base.RestrictedMemberSlot.MemberPath, blockAlias);
					}
					flag = false;
				}
				builder.Append('}');
			}
		}, delegate
		{
			bool num2 = builder.Length == 0;
			builder.Insert(0, '(');
			if (!num2)
			{
				builder.Append(" AND ");
			}
			if (userString)
			{
				base.RestrictedMemberSlot.MemberPath.ToCompactString(builder, Strings.ViewGen_EntityInstanceToken);
				builder.Append(" is not NULL)");
			}
			else
			{
				base.RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
				builder.Append(" IS NOT NULL)");
			}
		}, delegate
		{
			bool num = builder.Length == 0;
			StringBuilder stringBuilder = new StringBuilder();
			if (!num)
			{
				stringBuilder.Append('(');
			}
			if (userString)
			{
				base.RestrictedMemberSlot.MemberPath.ToCompactString(stringBuilder, blockAlias);
				stringBuilder.Append(" is NULL");
			}
			else
			{
				base.RestrictedMemberSlot.MemberPath.AsEsql(stringBuilder, blockAlias);
				stringBuilder.Append(" IS NULL");
			}
			if (!num)
			{
				stringBuilder.Append(" OR ");
			}
			builder.Insert(0, stringBuilder.ToString());
			if (!num)
			{
				builder.Append(')');
			}
		}, skipIsNotNull);
		inputBuilder.Append((object?)builder);
		return inputBuilder;
	}

	private void AsCql(Action<NegatedConstant, IEnumerable<Constant>> negatedConstantAsCql, Action<Set<Constant>> varInDomain, Action varIsNotNull, Action varIsNull, bool skipIsNotNull)
	{
		NegatedConstant negatedConstant = (NegatedConstant)base.Domain.Values.FirstOrDefault((Constant c) => c is NegatedConstant);
		if (negatedConstant != null)
		{
			negatedConstantAsCql(negatedConstant, base.Domain.Values);
			return;
		}
		Set<Constant> set = new Set<Constant>(base.Domain.Values, Constant.EqualityComparer);
		bool flag = false;
		if (set.Contains(Constant.Null))
		{
			flag = true;
			set.Remove(Constant.Null);
		}
		if (set.Contains(Constant.Undefined))
		{
			flag = true;
			set.Remove(Constant.Undefined);
		}
		bool num = !skipIsNotNull && base.RestrictedMemberSlot.MemberPath.IsNullable;
		if (set.Count > 0)
		{
			varInDomain(set);
		}
		if (num)
		{
			varIsNotNull();
		}
		if (flag)
		{
			varIsNull();
		}
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		base.RestrictedMemberSlot.ToCompactString(builder);
		builder.Append(" IN (");
		StringUtil.ToCommaSeparatedStringSorted(builder, base.Domain.Values);
		builder.Append(")");
	}
}
