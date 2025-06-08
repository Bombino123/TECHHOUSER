using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class OpCopierTrackingCollectionVars : OpCopier
{
	private readonly Dictionary<Var, Node> m_newCollectionVarDefinitions = new Dictionary<Var, Node>();

	private OpCopierTrackingCollectionVars(Command cmd)
		: base(cmd)
	{
	}

	internal static Node Copy(Command cmd, Node n, out VarMap varMap, out Dictionary<Var, Node> newCollectionVarDefinitions)
	{
		OpCopierTrackingCollectionVars opCopierTrackingCollectionVars = new OpCopierTrackingCollectionVars(cmd);
		Node result = opCopierTrackingCollectionVars.CopyNode(n);
		varMap = opCopierTrackingCollectionVars.m_varMap;
		newCollectionVarDefinitions = opCopierTrackingCollectionVars.m_newCollectionVarDefinitions;
		return result;
	}

	public override Node Visit(MultiStreamNestOp op, Node n)
	{
		Node node = base.Visit(op, n);
		MultiStreamNestOp multiStreamNestOp = (MultiStreamNestOp)node.Op;
		for (int i = 0; i < multiStreamNestOp.CollectionInfo.Count; i++)
		{
			m_newCollectionVarDefinitions.Add(multiStreamNestOp.CollectionInfo[i].CollectionVar, node.Children[i + 1]);
		}
		return node;
	}
}
