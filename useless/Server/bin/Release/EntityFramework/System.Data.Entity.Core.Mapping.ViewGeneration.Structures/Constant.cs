using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal abstract class Constant : InternalBase
{
	private class CellConstantComparer : IEqualityComparer<Constant>
	{
		public bool Equals(Constant left, Constant right)
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

		public int GetHashCode(Constant key)
		{
			return key.GetHashCode();
		}
	}

	private sealed class NullConstant : Constant
	{
		internal static readonly Constant Instance = new NullConstant();

		private NullConstant()
		{
		}

		internal override bool IsNull()
		{
			return true;
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
			EdmType edmType = Helper.GetModelTypeUsage(outputMember.LeafEdmMember).EdmType;
			builder.Append("CAST(NULL AS ");
			CqlWriter.AppendEscapedTypeName(builder, edmType);
			builder.Append(')');
			return builder;
		}

		internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
		{
			return TypeUsage.Create(Helper.GetModelTypeUsage(outputMember.LeafEdmMember).EdmType).Null();
		}

		public override int GetHashCode()
		{
			return 0;
		}

		protected override bool IsEqualTo(Constant right)
		{
			return this == right;
		}

		internal override string ToUserString()
		{
			return Strings.ViewGen_Null;
		}

		internal override void ToCompactString(StringBuilder builder)
		{
			builder.Append("NULL");
		}
	}

	private sealed class UndefinedConstant : Constant
	{
		internal static readonly Constant Instance = new UndefinedConstant();

		private UndefinedConstant()
		{
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
			return true;
		}

		internal override bool HasNotNull()
		{
			return false;
		}

		internal override StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias)
		{
			throw new NotSupportedException();
		}

		internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
		{
			throw new NotSupportedException();
		}

		public override int GetHashCode()
		{
			return 0;
		}

		protected override bool IsEqualTo(Constant right)
		{
			return this == right;
		}

		internal override string ToUserString()
		{
			throw new NotSupportedException();
		}

		internal override void ToCompactString(StringBuilder builder)
		{
			builder.Append("?");
		}
	}

	private sealed class AllOtherConstantsConstant : Constant
	{
		internal static readonly Constant Instance = new AllOtherConstantsConstant();

		private AllOtherConstantsConstant()
		{
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
			throw new NotSupportedException();
		}

		internal override DbExpression AsCqt(DbExpression row, MemberPath outputMember)
		{
			throw new NotSupportedException();
		}

		public override int GetHashCode()
		{
			return 0;
		}

		protected override bool IsEqualTo(Constant right)
		{
			return this == right;
		}

		internal override string ToUserString()
		{
			throw new NotSupportedException();
		}

		internal override void ToCompactString(StringBuilder builder)
		{
			builder.Append("AllOtherConstants");
		}
	}

	internal static readonly IEqualityComparer<Constant> EqualityComparer = new CellConstantComparer();

	internal static readonly Constant Null = NullConstant.Instance;

	internal static readonly Constant NotNull = new NegatedConstant(new Constant[1] { NullConstant.Instance });

	internal static readonly Constant Undefined = UndefinedConstant.Instance;

	internal static readonly Constant AllOtherConstants = AllOtherConstantsConstant.Instance;

	internal abstract bool IsNull();

	internal abstract bool IsNotNull();

	internal abstract bool IsUndefined();

	internal abstract bool HasNotNull();

	internal abstract StringBuilder AsEsql(StringBuilder builder, MemberPath outputMember, string blockAlias);

	internal abstract DbExpression AsCqt(DbExpression row, MemberPath outputMember);

	public override bool Equals(object obj)
	{
		if (!(obj is Constant right))
		{
			return false;
		}
		return IsEqualTo(right);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	protected abstract bool IsEqualTo(Constant right);

	internal abstract string ToUserString();

	internal static void ConstantsToUserString(StringBuilder builder, Set<Constant> constants)
	{
		bool flag = true;
		foreach (Constant constant in constants)
		{
			if (!flag)
			{
				builder.Append(Strings.ViewGen_CommaBlank);
			}
			flag = false;
			string value = constant.ToUserString();
			builder.Append(value);
		}
	}
}
