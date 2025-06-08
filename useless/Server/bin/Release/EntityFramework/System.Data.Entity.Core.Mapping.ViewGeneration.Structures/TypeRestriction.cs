using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Common.Utils.Boolean;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class TypeRestriction : MemberRestriction
{
	internal TypeRestriction(MemberPath member, IEnumerable<EdmType> values)
		: base(new MemberProjectedSlot(member), CreateTypeConstants(values))
	{
	}

	internal TypeRestriction(MemberPath member, Constant value)
		: base(new MemberProjectedSlot(member), value)
	{
	}

	internal TypeRestriction(MemberProjectedSlot slot, Domain domain)
		: base(slot, domain)
	{
	}

	internal override BoolExpr<DomainConstraint<BoolLiteral, Constant>> FixRange(Set<Constant> range, MemberDomainMap memberDomainMap)
	{
		IEnumerable<Constant> domain = memberDomainMap.GetDomain(base.RestrictedMemberSlot.MemberPath);
		return new TypeRestriction(base.RestrictedMemberSlot, new Domain(range, domain)).GetDomainBoolExpression(memberDomainMap);
	}

	internal override BoolLiteral RemapBool(Dictionary<MemberPath, MemberPath> remap)
	{
		return new TypeRestriction(base.RestrictedMemberSlot.RemapSlot(remap), base.Domain);
	}

	internal override MemberRestriction CreateCompleteMemberRestriction(IEnumerable<Constant> possibleValues)
	{
		return new TypeRestriction(base.RestrictedMemberSlot, new Domain(base.Domain.Values, possibleValues));
	}

	internal override StringBuilder AsEsql(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		if (base.Domain.Count > 1)
		{
			builder.Append('(');
		}
		bool flag = true;
		foreach (Constant value in base.Domain.Values)
		{
			TypeConstant typeConstant = value as TypeConstant;
			if (!flag)
			{
				builder.Append(" OR ");
			}
			flag = false;
			if (Helper.IsRefType(base.RestrictedMemberSlot.MemberPath.EdmType))
			{
				builder.Append("Deref(");
				base.RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
				builder.Append(')');
			}
			else
			{
				base.RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
			}
			if (value.IsNull())
			{
				builder.Append(" IS NULL");
				continue;
			}
			builder.Append(" IS OF (ONLY ");
			CqlWriter.AppendEscapedTypeName(builder, typeConstant.EdmType);
			builder.Append(')');
		}
		if (base.Domain.Count > 1)
		{
			builder.Append(')');
		}
		return builder;
	}

	internal override DbExpression AsCqt(DbExpression row, bool skipIsNotNull)
	{
		DbExpression cqt = base.RestrictedMemberSlot.MemberPath.AsCqt(row);
		if (Helper.IsRefType(base.RestrictedMemberSlot.MemberPath.EdmType))
		{
			cqt = cqt.Deref();
		}
		if (base.Domain.Count == 1)
		{
			cqt = cqt.IsOfOnly(TypeUsage.Create(((TypeConstant)base.Domain.Values.Single()).EdmType));
		}
		else
		{
			List<DbExpression> nodes = base.Domain.Values.Select((Func<Constant, DbExpression>)((Constant t) => cqt.IsOfOnly(TypeUsage.Create(((TypeConstant)t).EdmType)))).ToList();
			cqt = Helpers.BuildBalancedTreeInPlace(nodes, (DbExpression prev, DbExpression next) => prev.Or(next));
		}
		return cqt;
	}

	internal override StringBuilder AsUserString(StringBuilder builder, string blockAlias, bool skipIsNotNull)
	{
		if (Helper.IsRefType(base.RestrictedMemberSlot.MemberPath.EdmType))
		{
			builder.Append("Deref(");
			base.RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
			builder.Append(')');
		}
		else
		{
			base.RestrictedMemberSlot.MemberPath.AsEsql(builder, blockAlias);
		}
		if (base.Domain.Count > 1)
		{
			builder.Append(" is a (");
		}
		else
		{
			builder.Append(" is type ");
		}
		bool flag = true;
		foreach (Constant value in base.Domain.Values)
		{
			TypeConstant typeConstant = value as TypeConstant;
			if (!flag)
			{
				builder.Append(" OR ");
			}
			if (value.IsNull())
			{
				builder.Append(" NULL");
			}
			else
			{
				CqlWriter.AppendEscapedTypeName(builder, typeConstant.EdmType);
			}
			flag = false;
		}
		if (base.Domain.Count > 1)
		{
			builder.Append(')');
		}
		return builder;
	}

	private static IEnumerable<Constant> CreateTypeConstants(IEnumerable<EdmType> types)
	{
		foreach (EdmType type in types)
		{
			if (type == null)
			{
				yield return Constant.Null;
			}
			else
			{
				yield return new TypeConstant(type);
			}
		}
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		builder.Append("type(");
		base.RestrictedMemberSlot.ToCompactString(builder);
		builder.Append(") IN (");
		StringUtil.ToCommaSeparatedStringSorted(builder, base.Domain.Values);
		builder.Append(")");
	}
}
