using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Resources;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class NegatedConstant : Constant
{
	private readonly Set<Constant> m_negatedDomain;

	internal IEnumerable<Constant> Elements => m_negatedDomain;

	internal NegatedConstant(IEnumerable<Constant> values)
	{
		m_negatedDomain = new Set<Constant>(values, Constant.EqualityComparer);
	}

	internal bool Contains(Constant constant)
	{
		return m_negatedDomain.Contains(constant);
	}

	internal override bool IsNull()
	{
		return false;
	}

	internal override bool IsNotNull()
	{
		if (this == Constant.NotNull)
		{
			return true;
		}
		if (m_negatedDomain.Count == 1)
		{
			return m_negatedDomain.Contains(Constant.Null);
		}
		return false;
	}

	internal override bool IsUndefined()
	{
		return false;
	}

	internal override bool HasNotNull()
	{
		return m_negatedDomain.Contains(Constant.Null);
	}

	public override int GetHashCode()
	{
		int num = 0;
		foreach (Constant item in m_negatedDomain)
		{
			num ^= Constant.EqualityComparer.GetHashCode(item);
		}
		return num;
	}

	protected override bool IsEqualTo(Constant right)
	{
		if (!(right is NegatedConstant negatedConstant))
		{
			return false;
		}
		return m_negatedDomain.SetEquals(negatedConstant.m_negatedDomain);
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
	{
		return null;
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		return null;
	}

	internal StringBuilder AsEsql(StringBuilder builder, string blockAlias, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull)
	{
		return ToStringHelper(builder, blockAlias, constants, outputMember, skipIsNotNull, userString: false);
	}

	internal DbExpression AsCqt(DbExpression row, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull)
	{
		DbExpression cqt = null;
		AsCql(delegate
		{
			cqt = DbExpressionBuilder.True;
		}, delegate
		{
			cqt = outputMember.AsCqt(row).IsNull().Not();
		}, delegate(Constant constant)
		{
			DbExpression dbExpression = outputMember.AsCqt(row).NotEqual(constant.AsCqt(row, outputMember));
			if (cqt != null)
			{
				cqt = cqt.And(dbExpression);
			}
			else
			{
				cqt = dbExpression;
			}
		}, constants, outputMember, skipIsNotNull);
		return cqt;
	}

	internal StringBuilder AsUserString(StringBuilder builder, string blockAlias, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull)
	{
		return ToStringHelper(builder, blockAlias, constants, outputMember, skipIsNotNull, userString: true);
	}

	private void AsCql(Action trueLiteral, Action varIsNotNull, Action<Constant> varNotEqualsTo, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull)
	{
		bool isNullable = outputMember.IsNullable;
		Set<Constant> set = new Set<Constant>(Elements, Constant.EqualityComparer);
		foreach (Constant constant in constants)
		{
			if (!constant.Equals(this))
			{
				set.Remove(constant);
			}
		}
		if (set.Count == 0)
		{
			trueLiteral();
			return;
		}
		bool flag = set.Contains(Constant.Null);
		set.Remove(Constant.Null);
		if (flag || (isNullable && !skipIsNotNull))
		{
			varIsNotNull();
		}
		foreach (Constant item in set)
		{
			varNotEqualsTo(item);
		}
	}

	private StringBuilder ToStringHelper(StringBuilder builder, string blockAlias, IEnumerable<Constant> constants, MemberPath outputMember, bool skipIsNotNull, bool userString)
	{
		bool anyAdded = false;
		AsCql(delegate
		{
			builder.Append("true");
		}, delegate
		{
			if (userString)
			{
				outputMember.ToCompactString(builder, blockAlias);
				builder.Append(" is not NULL");
			}
			else
			{
				outputMember.AsEsql(builder, blockAlias);
				builder.Append(" IS NOT NULL");
			}
			anyAdded = true;
		}, delegate(Constant constant)
		{
			if (anyAdded)
			{
				builder.Append(" AND ");
			}
			anyAdded = true;
			if (userString)
			{
				outputMember.ToCompactString(builder, blockAlias);
				builder.Append(" <>");
				constant.ToCompactString(builder);
			}
			else
			{
				outputMember.AsEsql(builder, blockAlias);
				builder.Append(" <>");
				constant.AsEsql(builder, outputMember, blockAlias);
			}
		}, constants, outputMember, skipIsNotNull);
		return builder;
	}

	internal override string ToUserString()
	{
		if (IsNotNull())
		{
			return Strings.ViewGen_NotNull;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (Constant item in m_negatedDomain)
		{
			if (m_negatedDomain.Count <= 1 || !item.IsNull())
			{
				if (!flag)
				{
					stringBuilder.Append(Strings.ViewGen_CommaBlank);
				}
				flag = false;
				stringBuilder.Append(item.ToUserString());
			}
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append(Strings.ViewGen_NegatedCellConstant(stringBuilder.ToString()));
		return stringBuilder2.ToString();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		if (IsNotNull())
		{
			builder.Append("NOT_NULL");
			return;
		}
		builder.Append("NOT(");
		StringUtil.ToCommaSeparatedStringSorted(builder, m_negatedDomain);
		builder.Append(")");
	}
}
