namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class SetOp : RelOp
{
	private readonly VarMap[] m_varMap;

	private readonly VarVec m_outputVars;

	internal override int Arity => 2;

	internal VarMap[] VarMap => m_varMap;

	internal VarVec Outputs => m_outputVars;

	internal SetOp(OpType opType, VarVec outputs, VarMap left, VarMap right)
		: this(opType)
	{
		m_varMap = new VarMap[2];
		m_varMap[0] = left;
		m_varMap[1] = right;
		m_outputVars = outputs;
	}

	protected SetOp(OpType opType)
		: base(opType)
	{
	}
}
