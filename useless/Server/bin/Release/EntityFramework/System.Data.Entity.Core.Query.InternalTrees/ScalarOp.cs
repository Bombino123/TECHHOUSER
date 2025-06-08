using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class ScalarOp : Op
{
	private TypeUsage m_type;

	internal override bool IsScalarOp => true;

	internal override TypeUsage Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	internal virtual bool IsAggregateOp => false;

	internal ScalarOp(OpType opType, TypeUsage type)
		: this(opType)
	{
		m_type = type;
	}

	protected ScalarOp(OpType opType)
		: base(opType)
	{
	}

	internal override bool IsEquivalent(Op other)
	{
		if (other.OpType == base.OpType)
		{
			return TypeSemantics.IsStructurallyEqual(Type, other.Type);
		}
		return false;
	}
}
