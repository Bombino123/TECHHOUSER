using System.Collections.Generic;
using System.Data.Entity.Core.Query.InternalTrees;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class VarRemapper : BasicOpVisitor
{
	private readonly IDictionary<Var, Var> m_varMap;

	protected readonly Command m_command;

	internal VarRemapper(Command command)
		: this(command, new Dictionary<Var, Var>())
	{
	}

	internal VarRemapper(Command command, IDictionary<Var, Var> varMap)
	{
		m_command = command;
		m_varMap = varMap;
	}

	internal void AddMapping(Var oldVar, Var newVar)
	{
		m_varMap[oldVar] = newVar;
	}

	internal virtual void RemapNode(Node node)
	{
		if (m_varMap.Count != 0)
		{
			VisitNode(node);
		}
	}

	internal virtual void RemapSubtree(Node subTree)
	{
		if (m_varMap.Count == 0)
		{
			return;
		}
		foreach (Node child in subTree.Children)
		{
			RemapSubtree(child);
		}
		RemapNode(subTree);
		m_command.RecomputeNodeInfo(subTree);
	}

	internal VarList RemapVarList(VarList varList)
	{
		return Command.CreateVarList(MapVars(varList));
	}

	internal static VarList RemapVarList(Command command, IDictionary<Var, Var> varMap, VarList varList)
	{
		return new VarRemapper(command, varMap).RemapVarList(varList);
	}

	private Var Map(Var v)
	{
		Var value;
		while (m_varMap.TryGetValue(v, out value))
		{
			v = value;
		}
		return v;
	}

	private IEnumerable<Var> MapVars(IEnumerable<Var> vars)
	{
		foreach (Var var in vars)
		{
			yield return Map(var);
		}
	}

	private void Map(VarVec vec)
	{
		VarVec other = m_command.CreateVarVec(MapVars(vec));
		vec.InitFrom(other);
	}

	private void Map(VarList varList)
	{
		VarList collection = Command.CreateVarList(MapVars(varList));
		varList.Clear();
		varList.AddRange(collection);
	}

	private void Map(VarMap varMap)
	{
		VarMap varMap2 = new VarMap();
		foreach (KeyValuePair<Var, Var> item in varMap)
		{
			Var value = Map(item.Value);
			varMap2.Add(item.Key, value);
		}
		varMap.Clear();
		foreach (KeyValuePair<Var, Var> item2 in varMap2)
		{
			varMap.Add(item2.Key, item2.Value);
		}
	}

	private void Map(List<SortKey> sortKeys)
	{
		VarVec varVec = m_command.CreateVarVec();
		bool flag = false;
		foreach (SortKey sortKey in sortKeys)
		{
			sortKey.Var = Map(sortKey.Var);
			if (varVec.IsSet(sortKey.Var))
			{
				flag = true;
			}
			varVec.Set(sortKey.Var);
		}
		if (!flag)
		{
			return;
		}
		List<SortKey> list = new List<SortKey>(sortKeys);
		sortKeys.Clear();
		varVec.Clear();
		foreach (SortKey item in list)
		{
			if (!varVec.IsSet(item.Var))
			{
				sortKeys.Add(item);
			}
			varVec.Set(item.Var);
		}
	}

	protected override void VisitDefault(Node n)
	{
	}

	public override void Visit(VarRefOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		Var var = Map(op.Var);
		if (var != op.Var)
		{
			n.Op = m_command.CreateVarRefOp(var);
		}
	}

	protected override void VisitNestOp(NestBaseOp op, Node n)
	{
		throw new NotSupportedException();
	}

	public override void Visit(PhysicalProjectOp op, Node n)
	{
		VisitPhysicalOpDefault(op, n);
		Map(op.Outputs);
		SimpleCollectionColumnMap columnMap = (SimpleCollectionColumnMap)ColumnMapTranslator.Translate(op.ColumnMap, m_varMap);
		n.Op = m_command.CreatePhysicalProjectOp(op.Outputs, columnMap);
	}

	protected override void VisitGroupByOp(GroupByBaseOp op, Node n)
	{
		VisitRelOpDefault(op, n);
		Map(op.Outputs);
		Map(op.Keys);
	}

	public override void Visit(GroupByIntoOp op, Node n)
	{
		VisitGroupByOp(op, n);
		Map(op.Inputs);
	}

	public override void Visit(DistinctOp op, Node n)
	{
		VisitRelOpDefault(op, n);
		Map(op.Keys);
	}

	public override void Visit(ProjectOp op, Node n)
	{
		VisitRelOpDefault(op, n);
		Map(op.Outputs);
	}

	public override void Visit(UnnestOp op, Node n)
	{
		VisitRelOpDefault(op, n);
		Var var = Map(op.Var);
		if (var != op.Var)
		{
			n.Op = m_command.CreateUnnestOp(var, op.Table);
		}
	}

	protected override void VisitSetOp(SetOp op, Node n)
	{
		VisitRelOpDefault(op, n);
		Map(op.VarMap[0]);
		Map(op.VarMap[1]);
	}

	protected override void VisitSortOp(SortBaseOp op, Node n)
	{
		VisitRelOpDefault(op, n);
		Map(op.Keys);
	}
}
