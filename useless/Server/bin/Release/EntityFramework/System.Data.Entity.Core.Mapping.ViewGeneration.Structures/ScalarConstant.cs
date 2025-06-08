using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal sealed class ScalarConstant : Constant
{
	private readonly object m_scalar;

	internal object Value => m_scalar;

	internal ScalarConstant(object value)
	{
		m_scalar = value;
	}

	internal override bool IsNull()
	{
		return false;
	}

	internal override bool IsNotNull()
	{
		return false;
	}

	internal override bool IsUndefined()
	{
		return false;
	}

	internal override bool HasNotNull()
	{
		return false;
	}

	internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
	{
		TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(outputMember.LeafEdmMember);
		EdmType edmType = modelTypeUsage.EdmType;
		if (BuiltInTypeKind.PrimitiveType == edmType.BuiltInTypeKind)
		{
			switch (((PrimitiveType)edmType).PrimitiveTypeKind)
			{
			case PrimitiveTypeKind.Boolean:
			{
				bool flag = (bool)m_scalar;
				string value = StringUtil.FormatInvariant("{0}", flag);
				builder.Append(value);
				return builder;
			}
			case PrimitiveTypeKind.String:
			{
				if (!TypeHelpers.TryGetIsUnicode(modelTypeUsage, out var isUnicode))
				{
					isUnicode = true;
				}
				if (isUnicode)
				{
					builder.Append('N');
				}
				AppendEscapedScalar(builder);
				return builder;
			}
			}
		}
		else if (BuiltInTypeKind.EnumType == edmType.BuiltInTypeKind)
		{
			EnumMember enumMember = (EnumMember)m_scalar;
			builder.Append(enumMember.Name);
			return builder;
		}
		builder.Append("CAST(");
		AppendEscapedScalar(builder);
		builder.Append(" AS ");
		CqlWriter.AppendEscapedTypeName(builder, edmType);
		builder.Append(')');
		return builder;
	}

	private StringBuilder AppendEscapedScalar(StringBuilder builder)
	{
		string text = StringUtil.FormatInvariant("{0}", m_scalar);
		if (text.Contains("'"))
		{
			text = text.Replace("'", "''");
		}
		StringUtil.FormatStringBuilder(builder, "'{0}'", text);
		return builder;
	}

	internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
	{
		return Helper.GetModelTypeUsage(outputMember.LeafEdmMember).Constant(m_scalar);
	}

	protected override bool IsEqualTo(Constant right)
	{
		if (!(right is ScalarConstant scalarConstant))
		{
			return false;
		}
		return ByValueEqualityComparer.Default.Equals(m_scalar, scalarConstant.m_scalar);
	}

	public override int GetHashCode()
	{
		return m_scalar.GetHashCode();
	}

	internal override string ToUserString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		ToCompactString(stringBuilder);
		return stringBuilder.ToString();
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		if (m_scalar is EnumMember enumMember)
		{
			builder.Append(enumMember.Name);
			return;
		}
		builder.Append(StringUtil.FormatInvariant("'{0}'", m_scalar));
	}
}
