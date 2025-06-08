using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class Op
{
	private readonly OpType m_opType;

	internal const int ArityVarying = -1;

	internal OpType OpType => m_opType;

	internal virtual int Arity => -1;

	internal virtual bool IsScalarOp => false;

	internal virtual bool IsRulePatternOp => false;

	internal virtual bool IsRelOp => false;

	internal virtual bool IsAncillaryOp => false;

	internal virtual bool IsPhysicalOp => false;

	internal virtual TypeUsage Type
	{
		get
		{
			return null;
		}
		set
		{
			throw Error.NotSupported();
		}
	}

	internal Op(OpType opType)
	{
		m_opType = opType;
	}

	internal virtual bool IsEquivalent(Op other)
	{
		return false;
	}

	[DebuggerNonUserCode]
	internal virtual void Accept(BasicOpVisitor v, Node n)
	{
		v.Visit(this, n);
	}

	[DebuggerNonUserCode]
	internal virtual TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
	{
		return v.Visit(this, n);
	}
}
