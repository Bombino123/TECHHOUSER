using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal sealed class FunctionOp : ScalarOp
{
	private readonly EdmFunction m_function;

	internal static readonly FunctionOp Pattern = new FunctionOp();

	internal EdmFunction Function => m_function;

	internal FunctionOp(EdmFunction function)
		: base(OpType.Function, function.ReturnParameter.TypeUsage)
	{
		m_function = function;
	}

	private FunctionOp()
		: base(OpType.Function)
	{
	}

	internal override bool IsEquivalent(Op other)
	{
		if (other is FunctionOp functionOp)
		{
			return functionOp.Function.EdmEquals(Function);
		}
		return false;
	}

	[DebuggerNonUserCode]
	internal override void Accept(BasicOpVisitor v, Node n)
	{
		v.Visit(this, n);
	}

	[DebuggerNonUserCode]
	internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
	{
		return v.Visit(this, n);
	}
}
