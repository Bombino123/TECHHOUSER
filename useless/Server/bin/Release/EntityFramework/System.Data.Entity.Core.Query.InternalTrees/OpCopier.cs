using System.Collections.Generic;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class OpCopier : BasicOpVisitorOfNode
{
	private readonly Command m_srcCmd;

	protected Command m_destCmd;

	protected VarMap m_varMap;

	internal static Node Copy(Command cmd, Node n)
	{
		VarMap varMap;
		return Copy(cmd, n, out varMap);
	}

	internal static Node Copy(Command cmd, Node node, VarList varList, out VarList newVarList)
	{
		VarMap varMap;
		Node result = Copy(cmd, node, out varMap);
		newVarList = Command.CreateVarList();
		foreach (Var var in varList)
		{
			Var item = varMap[var];
			newVarList.Add(item);
		}
		return result;
	}

	internal static Node Copy(Command cmd, Node n, out VarMap varMap)
	{
		OpCopier opCopier = new OpCopier(cmd);
		Node result = opCopier.CopyNode(n);
		varMap = opCopier.m_varMap;
		return result;
	}

	internal static List<SortKey> Copy(Command cmd, List<SortKey> sortKeys)
	{
		return new OpCopier(cmd).Copy(sortKeys);
	}

	protected OpCopier(Command cmd)
		: this(cmd, cmd)
	{
	}

	private OpCopier(Command destCommand, Command sourceCommand)
	{
		m_srcCmd = sourceCommand;
		m_destCmd = destCommand;
		m_varMap = new VarMap();
	}

	private Var GetMappedVar(Var v)
	{
		if (m_varMap.TryGetValue(v, out var value))
		{
			return value;
		}
		if (m_destCmd != m_srcCmd)
		{
			throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.UnknownVar, 6, null);
		}
		return v;
	}

	private void SetMappedVar(Var v, Var mappedVar)
	{
		m_varMap.Add(v, mappedVar);
	}

	private void MapTable(Table newTable, Table oldTable)
	{
		for (int i = 0; i < oldTable.Columns.Count; i++)
		{
			SetMappedVar(oldTable.Columns[i], newTable.Columns[i]);
		}
	}

	private IEnumerable<Var> MapVars(IEnumerable<Var> vars)
	{
		foreach (Var var in vars)
		{
			yield return GetMappedVar(var);
		}
	}

	private VarVec Copy(VarVec vars)
	{
		return m_destCmd.CreateVarVec(MapVars(vars));
	}

	private VarList Copy(VarList varList)
	{
		return Command.CreateVarList(MapVars(varList));
	}

	private SortKey Copy(SortKey sortKey)
	{
		return Command.CreateSortKey(GetMappedVar(sortKey.Var), sortKey.AscendingSort, sortKey.Collation);
	}

	private List<SortKey> Copy(List<SortKey> sortKeys)
	{
		List<SortKey> list = new List<SortKey>();
		foreach (SortKey sortKey in sortKeys)
		{
			list.Add(Copy(sortKey));
		}
		return list;
	}

	protected Node CopyNode(Node n)
	{
		return n.Op.Accept(this, n);
	}

	private List<Node> ProcessChildren(Node n)
	{
		List<Node> list = new List<Node>();
		foreach (Node child in n.Children)
		{
			list.Add(CopyNode(child));
		}
		return list;
	}

	private Node CopyDefault(Op op, Node original)
	{
		return m_destCmd.CreateNode(op, ProcessChildren(original));
	}

	public override Node Visit(Op op, Node n)
	{
		throw new NotSupportedException(Strings.Iqt_General_UnsupportedOp(op.GetType().FullName));
	}

	public override Node Visit(ConstantOp op, Node n)
	{
		ConstantBaseOp op2 = m_destCmd.CreateConstantOp(op.Type, op.Value);
		return m_destCmd.CreateNode(op2);
	}

	public override Node Visit(NullOp op, Node n)
	{
		return m_destCmd.CreateNode(m_destCmd.CreateNullOp(op.Type));
	}

	public override Node Visit(ConstantPredicateOp op, Node n)
	{
		return m_destCmd.CreateNode(m_destCmd.CreateConstantPredicateOp(op.Value));
	}

	public override Node Visit(InternalConstantOp op, Node n)
	{
		InternalConstantOp op2 = m_destCmd.CreateInternalConstantOp(op.Type, op.Value);
		return m_destCmd.CreateNode(op2);
	}

	public override Node Visit(NullSentinelOp op, Node n)
	{
		NullSentinelOp op2 = m_destCmd.CreateNullSentinelOp();
		return m_destCmd.CreateNode(op2);
	}

	public override Node Visit(FunctionOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateFunctionOp(op.Function), n);
	}

	public override Node Visit(PropertyOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreatePropertyOp(op.PropertyInfo), n);
	}

	public override Node Visit(RelPropertyOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateRelPropertyOp(op.PropertyInfo), n);
	}

	public override Node Visit(CaseOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateCaseOp(op.Type), n);
	}

	public override Node Visit(ComparisonOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateComparisonOp(op.OpType, op.UseDatabaseNullSemantics), n);
	}

	public override Node Visit(LikeOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateLikeOp(), n);
	}

	public override Node Visit(AggregateOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateAggregateOp(op.AggFunc, op.IsDistinctAggregate), n);
	}

	public override Node Visit(NewInstanceOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateNewInstanceOp(op.Type), n);
	}

	public override Node Visit(NewEntityOp op, Node n)
	{
		NewEntityOp op2 = ((!op.Scoped) ? m_destCmd.CreateNewEntityOp(op.Type, op.RelationshipProperties) : m_destCmd.CreateScopedNewEntityOp(op.Type, op.RelationshipProperties, op.EntitySet));
		return CopyDefault(op2, n);
	}

	public override Node Visit(DiscriminatedNewEntityOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateDiscriminatedNewEntityOp(op.Type, op.DiscriminatorMap, op.EntitySet, op.RelationshipProperties), n);
	}

	public override Node Visit(NewMultisetOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateNewMultisetOp(op.Type), n);
	}

	public override Node Visit(NewRecordOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateNewRecordOp(op.Type), n);
	}

	public override Node Visit(RefOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateRefOp(op.EntitySet, op.Type), n);
	}

	public override Node Visit(VarRefOp op, Node n)
	{
		if (!m_varMap.TryGetValue(op.Var, out var value))
		{
			value = op.Var;
		}
		return m_destCmd.CreateNode(m_destCmd.CreateVarRefOp(value));
	}

	public override Node Visit(ConditionalOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateConditionalOp(op.OpType), n);
	}

	public override Node Visit(ArithmeticOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateArithmeticOp(op.OpType, op.Type), n);
	}

	public override Node Visit(TreatOp op, Node n)
	{
		TreatOp op2 = (op.IsFakeTreat ? m_destCmd.CreateFakeTreatOp(op.Type) : m_destCmd.CreateTreatOp(op.Type));
		return CopyDefault(op2, n);
	}

	public override Node Visit(CastOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateCastOp(op.Type), n);
	}

	public override Node Visit(SoftCastOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateSoftCastOp(op.Type), n);
	}

	public override Node Visit(DerefOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateDerefOp(op.Type), n);
	}

	public override Node Visit(NavigateOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateNavigateOp(op.Type, op.RelProperty), n);
	}

	public override Node Visit(IsOfOp op, Node n)
	{
		if (op.IsOfOnly)
		{
			return CopyDefault(m_destCmd.CreateIsOfOnlyOp(op.IsOfType), n);
		}
		return CopyDefault(m_destCmd.CreateIsOfOp(op.IsOfType), n);
	}

	public override Node Visit(ExistsOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateExistsOp(), n);
	}

	public override Node Visit(ElementOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateElementOp(op.Type), n);
	}

	public override Node Visit(GetRefKeyOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateGetRefKeyOp(op.Type), n);
	}

	public override Node Visit(GetEntityRefOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateGetEntityRefOp(op.Type), n);
	}

	public override Node Visit(CollectOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateCollectOp(op.Type), n);
	}

	public override Node Visit(ScanTableOp op, Node n)
	{
		ScanTableOp scanTableOp = m_destCmd.CreateScanTableOp(op.Table.TableMetadata);
		MapTable(scanTableOp.Table, op.Table);
		return m_destCmd.CreateNode(scanTableOp);
	}

	public override Node Visit(ScanViewOp op, Node n)
	{
		ScanViewOp scanViewOp = m_destCmd.CreateScanViewOp(op.Table.TableMetadata);
		MapTable(scanViewOp.Table, op.Table);
		List<Node> args = ProcessChildren(n);
		return m_destCmd.CreateNode(scanViewOp, args);
	}

	public override Node Visit(UnnestOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		Var mappedVar = GetMappedVar(op.Var);
		Table t = m_destCmd.CreateTableInstance(op.Table.TableMetadata);
		UnnestOp unnestOp = m_destCmd.CreateUnnestOp(mappedVar, t);
		MapTable(unnestOp.Table, op.Table);
		return m_destCmd.CreateNode(unnestOp, args);
	}

	public override Node Visit(ProjectOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		VarVec vars = Copy(op.Outputs);
		ProjectOp op2 = m_destCmd.CreateProjectOp(vars);
		return m_destCmd.CreateNode(op2, args);
	}

	public override Node Visit(FilterOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateFilterOp(), n);
	}

	public override Node Visit(SortOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		List<SortKey> sortKeys = Copy(op.Keys);
		SortOp op2 = m_destCmd.CreateSortOp(sortKeys);
		return m_destCmd.CreateNode(op2, args);
	}

	public override Node Visit(ConstrainedSortOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		List<SortKey> sortKeys = Copy(op.Keys);
		ConstrainedSortOp op2 = m_destCmd.CreateConstrainedSortOp(sortKeys, op.WithTies);
		return m_destCmd.CreateNode(op2, args);
	}

	public override Node Visit(GroupByOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		GroupByOp op2 = m_destCmd.CreateGroupByOp(Copy(op.Keys), Copy(op.Outputs));
		return m_destCmd.CreateNode(op2, args);
	}

	public override Node Visit(GroupByIntoOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		GroupByIntoOp op2 = m_destCmd.CreateGroupByIntoOp(Copy(op.Keys), Copy(op.Inputs), Copy(op.Outputs));
		return m_destCmd.CreateNode(op2, args);
	}

	public override Node Visit(CrossJoinOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateCrossJoinOp(), n);
	}

	public override Node Visit(InnerJoinOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateInnerJoinOp(), n);
	}

	public override Node Visit(LeftOuterJoinOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateLeftOuterJoinOp(), n);
	}

	public override Node Visit(FullOuterJoinOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateFullOuterJoinOp(), n);
	}

	public override Node Visit(CrossApplyOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateCrossApplyOp(), n);
	}

	public override Node Visit(OuterApplyOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateOuterApplyOp(), n);
	}

	private Node CopySetOp(SetOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		VarMap varMap = new VarMap();
		VarMap varMap2 = new VarMap();
		foreach (KeyValuePair<Var, Var> item in op.VarMap[0])
		{
			Var var = m_destCmd.CreateSetOpVar(item.Key.Type);
			SetMappedVar(item.Key, var);
			varMap.Add(var, GetMappedVar(item.Value));
			varMap2.Add(var, GetMappedVar(op.VarMap[1][item.Key]));
		}
		SetOp op2 = null;
		switch (op.OpType)
		{
		case OpType.UnionAll:
		{
			Var var2 = ((UnionAllOp)op).BranchDiscriminator;
			if (var2 != null)
			{
				var2 = GetMappedVar(var2);
			}
			op2 = m_destCmd.CreateUnionAllOp(varMap, varMap2, var2);
			break;
		}
		case OpType.Intersect:
			op2 = m_destCmd.CreateIntersectOp(varMap, varMap2);
			break;
		case OpType.Except:
			op2 = m_destCmd.CreateExceptOp(varMap, varMap2);
			break;
		}
		return m_destCmd.CreateNode(op2, args);
	}

	public override Node Visit(UnionAllOp op, Node n)
	{
		return CopySetOp(op, n);
	}

	public override Node Visit(IntersectOp op, Node n)
	{
		return CopySetOp(op, n);
	}

	public override Node Visit(ExceptOp op, Node n)
	{
		return CopySetOp(op, n);
	}

	public override Node Visit(DistinctOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		VarVec keyVars = Copy(op.Keys);
		DistinctOp op2 = m_destCmd.CreateDistinctOp(keyVars);
		return m_destCmd.CreateNode(op2, args);
	}

	public override Node Visit(SingleRowOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateSingleRowOp(), n);
	}

	public override Node Visit(SingleRowTableOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateSingleRowTableOp(), n);
	}

	public override Node Visit(VarDefOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		Var var = m_destCmd.CreateComputedVar(op.Var.Type);
		SetMappedVar(op.Var, var);
		return m_destCmd.CreateNode(m_destCmd.CreateVarDefOp(var), args);
	}

	public override Node Visit(VarDefListOp op, Node n)
	{
		return CopyDefault(m_destCmd.CreateVarDefListOp(), n);
	}

	private ColumnMap Copy(ColumnMap columnMap)
	{
		return ColumnMapCopier.Copy(columnMap, m_varMap);
	}

	public override Node Visit(PhysicalProjectOp op, Node n)
	{
		List<Node> args = ProcessChildren(n);
		VarList outputVars = Copy(op.Outputs);
		SimpleCollectionColumnMap columnMap = Copy(op.ColumnMap) as SimpleCollectionColumnMap;
		PhysicalProjectOp op2 = m_destCmd.CreatePhysicalProjectOp(outputVars, columnMap);
		return m_destCmd.CreateNode(op2, args);
	}

	private Node VisitNestOp(Node n)
	{
		NestBaseOp nestBaseOp = n.Op as NestBaseOp;
		SingleStreamNestOp singleStreamNestOp = nestBaseOp as SingleStreamNestOp;
		List<Node> args = ProcessChildren(n);
		Var discriminatorVar = null;
		if (singleStreamNestOp != null)
		{
			discriminatorVar = GetMappedVar(singleStreamNestOp.Discriminator);
		}
		List<CollectionInfo> list = new List<CollectionInfo>();
		foreach (CollectionInfo item2 in nestBaseOp.CollectionInfo)
		{
			ColumnMap columnMap = Copy(item2.ColumnMap);
			Var var = m_destCmd.CreateComputedVar(item2.CollectionVar.Type);
			SetMappedVar(item2.CollectionVar, var);
			VarList flattenedElementVars = Copy(item2.FlattenedElementVars);
			VarVec keys = Copy(item2.Keys);
			List<SortKey> sortKeys = Copy(item2.SortKeys);
			CollectionInfo item = Command.CreateCollectionInfo(var, columnMap, flattenedElementVars, keys, sortKeys, item2.DiscriminatorValue);
			list.Add(item);
		}
		VarVec outputVars = Copy(nestBaseOp.Outputs);
		NestBaseOp nestBaseOp2 = null;
		List<SortKey> prefixSortKeys = Copy(nestBaseOp.PrefixSortKeys);
		if (singleStreamNestOp != null)
		{
			VarVec keys2 = Copy(singleStreamNestOp.Keys);
			List<SortKey> postfixSortKeys = Copy(singleStreamNestOp.PostfixSortKeys);
			nestBaseOp2 = m_destCmd.CreateSingleStreamNestOp(keys2, prefixSortKeys, postfixSortKeys, outputVars, list, discriminatorVar);
		}
		else
		{
			nestBaseOp2 = m_destCmd.CreateMultiStreamNestOp(prefixSortKeys, outputVars, list);
		}
		return m_destCmd.CreateNode(nestBaseOp2, args);
	}

	public override Node Visit(SingleStreamNestOp op, Node n)
	{
		return VisitNestOp(n);
	}

	public override Node Visit(MultiStreamNestOp op, Node n)
	{
		return VisitNestOp(n);
	}
}
