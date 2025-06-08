using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class ConstantBaseOp : ScalarOp
{
	private readonly object m_value;

	internal virtual object Value => m_value;

	internal override int Arity => 0;

	protected ConstantBaseOp(OpType opType, TypeUsage type, object value)
		: base(opType, type)
	{
		m_value = value;
	}

	protected ConstantBaseOp(OpType opType)
		: base(opType)
	{
	}

	internal override bool IsEquivalent(Op other)
	{
		if (other is ConstantBaseOp constantBaseOp && base.OpType == other.OpType && constantBaseOp.Type.EdmEquals(Type))
		{
			if (constantBaseOp.Value != null || Value != null)
			{
				return constantBaseOp.Value.Equals(Value);
			}
			return true;
		}
		return false;
	}
}
