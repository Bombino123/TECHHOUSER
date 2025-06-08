using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.PlanCompiler;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class Command
{
	private readonly Dictionary<string, ParameterVar> m_parameterMap;

	private readonly List<Var> m_vars;

	private readonly List<Table> m_tables;

	private readonly MetadataWorkspace m_metadataWorkspace;

	private readonly TypeUsage m_boolType;

	private readonly TypeUsage m_intType;

	private readonly TypeUsage m_stringType;

	private readonly ConstantPredicateOp m_trueOp;

	private readonly ConstantPredicateOp m_falseOp;

	private readonly NodeInfoVisitor m_nodeInfoVisitor;

	private readonly KeyPullup m_keyPullupVisitor;

	private int m_nextNodeId;

	private int m_nextBranchDiscriminatorValue = 1000;

	private bool m_disableVarVecEnumCaching;

	private readonly Stack<VarVec.VarVecEnumerator> m_freeVarVecEnumerators;

	private readonly Stack<VarVec> m_freeVarVecs;

	private readonly HashSet<RelProperty> m_referencedRelProperties;

	internal virtual MetadataWorkspace MetadataWorkspace => m_metadataWorkspace;

	internal virtual Node Root { get; set; }

	internal virtual int NextBranchDiscriminatorValue => m_nextBranchDiscriminatorValue++;

	internal virtual int NextNodeId => m_nextNodeId;

	internal virtual TypeUsage BooleanType => m_boolType;

	internal virtual TypeUsage IntegerType => m_intType;

	internal virtual TypeUsage StringType => m_stringType;

	internal virtual IEnumerable<Var> Vars => m_vars.Where((Var v) => v.VarType != VarType.NotValid);

	internal virtual HashSet<RelProperty> ReferencedRelProperties => m_referencedRelProperties;

	internal Command(MetadataWorkspace metadataWorkspace)
	{
		m_parameterMap = new Dictionary<string, ParameterVar>();
		m_vars = new List<Var>();
		m_tables = new List<Table>();
		m_metadataWorkspace = metadataWorkspace;
		if (!TryGetPrimitiveType(PrimitiveTypeKind.Boolean, out m_boolType))
		{
			throw new ProviderIncompatibleException(Strings.Cqt_General_NoProviderBooleanType);
		}
		if (!TryGetPrimitiveType(PrimitiveTypeKind.Int32, out m_intType))
		{
			throw new ProviderIncompatibleException(Strings.Cqt_General_NoProviderIntegerType);
		}
		if (!TryGetPrimitiveType(PrimitiveTypeKind.String, out m_stringType))
		{
			throw new ProviderIncompatibleException(Strings.Cqt_General_NoProviderStringType);
		}
		m_trueOp = new ConstantPredicateOp(m_boolType, value: true);
		m_falseOp = new ConstantPredicateOp(m_boolType, value: false);
		m_nodeInfoVisitor = new NodeInfoVisitor(this);
		m_keyPullupVisitor = new KeyPullup(this);
		m_freeVarVecEnumerators = new Stack<VarVec.VarVecEnumerator>();
		m_freeVarVecs = new Stack<VarVec>();
		m_referencedRelProperties = new HashSet<RelProperty>();
	}

	internal Command()
	{
	}

	internal virtual void DisableVarVecEnumCaching()
	{
		m_disableVarVecEnumCaching = true;
	}

	private static bool TryGetPrimitiveType(PrimitiveTypeKind modelType, out TypeUsage type)
	{
		type = null;
		if (modelType == PrimitiveTypeKind.String)
		{
			type = TypeUsage.CreateStringTypeUsage(MetadataWorkspace.GetModelPrimitiveType(modelType), isUnicode: false, isFixedLength: false);
		}
		else
		{
			type = MetadataWorkspace.GetCanonicalModelTypeUsage(modelType);
		}
		return type != null;
	}

	internal virtual VarVec CreateVarVec()
	{
		VarVec varVec;
		if (m_freeVarVecs.Count == 0)
		{
			varVec = new VarVec(this);
		}
		else
		{
			varVec = m_freeVarVecs.Pop();
			varVec.Clear();
		}
		return varVec;
	}

	internal virtual VarVec CreateVarVec(Var v)
	{
		VarVec varVec = CreateVarVec();
		varVec.Set(v);
		return varVec;
	}

	internal virtual VarVec CreateVarVec(IEnumerable<Var> v)
	{
		VarVec varVec = CreateVarVec();
		varVec.InitFrom(v);
		return varVec;
	}

	internal virtual VarVec CreateVarVec(VarVec v)
	{
		VarVec varVec = CreateVarVec();
		varVec.InitFrom(v);
		return varVec;
	}

	internal virtual void ReleaseVarVec(VarVec vec)
	{
		m_freeVarVecs.Push(vec);
	}

	internal virtual VarVec.VarVecEnumerator GetVarVecEnumerator(VarVec vec)
	{
		VarVec.VarVecEnumerator varVecEnumerator;
		if (m_disableVarVecEnumCaching || m_freeVarVecEnumerators.Count == 0)
		{
			varVecEnumerator = new VarVec.VarVecEnumerator(vec);
		}
		else
		{
			varVecEnumerator = m_freeVarVecEnumerators.Pop();
			varVecEnumerator.Init(vec);
		}
		return varVecEnumerator;
	}

	internal virtual void ReleaseVarVecEnumerator(VarVec.VarVecEnumerator enumerator)
	{
		if (!m_disableVarVecEnumCaching)
		{
			m_freeVarVecEnumerators.Push(enumerator);
		}
	}

	internal static VarList CreateVarList()
	{
		return new VarList();
	}

	internal static VarList CreateVarList(IEnumerable<Var> vars)
	{
		return new VarList(vars);
	}

	private int NewTableId()
	{
		return m_tables.Count;
	}

	internal static TableMD CreateTableDefinition(TypeUsage elementType)
	{
		return new TableMD(elementType, null);
	}

	internal static TableMD CreateTableDefinition(EntitySetBase extent)
	{
		return new TableMD(TypeUsage.Create(extent.ElementType), extent);
	}

	internal virtual TableMD CreateFlatTableDefinition(RowType type)
	{
		return CreateFlatTableDefinition(type.Properties, new List<EdmMember>(), null);
	}

	internal virtual TableMD CreateFlatTableDefinition(IEnumerable<EdmProperty> properties, IEnumerable<EdmMember> keyMembers, EntitySetBase entitySet)
	{
		return new TableMD(properties, keyMembers, entitySet);
	}

	internal virtual Table CreateTableInstance(TableMD tableMetadata)
	{
		Table table = new Table(this, tableMetadata, NewTableId());
		m_tables.Add(table);
		return table;
	}

	internal virtual Var GetVar(int id)
	{
		return m_vars[id];
	}

	internal virtual ParameterVar GetParameter(string paramName)
	{
		return m_parameterMap[paramName];
	}

	private int NewVarId()
	{
		return m_vars.Count;
	}

	internal virtual ParameterVar CreateParameterVar(string parameterName, TypeUsage parameterType)
	{
		if (m_parameterMap.ContainsKey(parameterName))
		{
			throw new ArgumentException(Strings.DuplicateParameterName(parameterName));
		}
		ParameterVar parameterVar = new ParameterVar(NewVarId(), parameterType, parameterName);
		m_vars.Add(parameterVar);
		m_parameterMap[parameterName] = parameterVar;
		return parameterVar;
	}

	private ParameterVar ReplaceParameterVar(ParameterVar oldVar, Func<TypeUsage, TypeUsage> generateReplacementType)
	{
		ParameterVar parameterVar = new ParameterVar(NewVarId(), generateReplacementType(oldVar.Type), oldVar.ParameterName);
		m_parameterMap[oldVar.ParameterName] = parameterVar;
		m_vars.Add(parameterVar);
		return parameterVar;
	}

	internal virtual ParameterVar ReplaceEnumParameterVar(ParameterVar oldVar)
	{
		return ReplaceParameterVar(oldVar, (TypeUsage t) => TypeHelpers.CreateEnumUnderlyingTypeUsage(t));
	}

	internal virtual ParameterVar ReplaceStrongSpatialParameterVar(ParameterVar oldVar)
	{
		return ReplaceParameterVar(oldVar, (TypeUsage t) => TypeHelpers.CreateSpatialUnionTypeUsage(t));
	}

	internal virtual ColumnVar CreateColumnVar(Table table, ColumnMD columnMD)
	{
		ColumnVar columnVar = new ColumnVar(NewVarId(), table, columnMD);
		table.Columns.Add(columnVar);
		m_vars.Add(columnVar);
		return columnVar;
	}

	internal virtual ComputedVar CreateComputedVar(TypeUsage type)
	{
		ComputedVar computedVar = new ComputedVar(NewVarId(), type);
		m_vars.Add(computedVar);
		return computedVar;
	}

	internal virtual SetOpVar CreateSetOpVar(TypeUsage type)
	{
		SetOpVar setOpVar = new SetOpVar(NewVarId(), type);
		m_vars.Add(setOpVar);
		return setOpVar;
	}

	internal virtual Node CreateNode(Op op)
	{
		return CreateNode(op, new List<Node>());
	}

	internal virtual Node CreateNode(Op op, Node arg1)
	{
		List<Node> list = new List<Node>();
		list.Add(arg1);
		return CreateNode(op, list);
	}

	internal virtual Node CreateNode(Op op, Node arg1, Node arg2)
	{
		List<Node> list = new List<Node>();
		list.Add(arg1);
		list.Add(arg2);
		return CreateNode(op, list);
	}

	internal virtual Node CreateNode(Op op, Node arg1, Node arg2, Node arg3)
	{
		List<Node> list = new List<Node>();
		list.Add(arg1);
		list.Add(arg2);
		list.Add(arg3);
		return CreateNode(op, list);
	}

	internal virtual Node CreateNode(Op op, IList<Node> args)
	{
		return new Node(m_nextNodeId++, op, new List<Node>(args));
	}

	internal virtual Node CreateNode(Op op, List<Node> args)
	{
		return new Node(m_nextNodeId++, op, args);
	}

	internal virtual ConstantBaseOp CreateConstantOp(TypeUsage type, object value)
	{
		if (value == null)
		{
			return new NullOp(type);
		}
		if (TypeSemantics.IsBooleanType(type))
		{
			return new InternalConstantOp(type, value);
		}
		return new ConstantOp(type, value);
	}

	internal virtual InternalConstantOp CreateInternalConstantOp(TypeUsage type, object value)
	{
		return new InternalConstantOp(type, value);
	}

	internal virtual NullSentinelOp CreateNullSentinelOp()
	{
		return new NullSentinelOp(IntegerType, 1);
	}

	internal virtual NullOp CreateNullOp(TypeUsage type)
	{
		return new NullOp(type);
	}

	internal virtual ConstantPredicateOp CreateConstantPredicateOp(bool value)
	{
		if (!value)
		{
			return m_falseOp;
		}
		return m_trueOp;
	}

	internal virtual ConstantPredicateOp CreateTrueOp()
	{
		return m_trueOp;
	}

	internal virtual ConstantPredicateOp CreateFalseOp()
	{
		return m_falseOp;
	}

	internal virtual FunctionOp CreateFunctionOp(EdmFunction function)
	{
		return new FunctionOp(function);
	}

	internal virtual TreatOp CreateTreatOp(TypeUsage type)
	{
		return new TreatOp(type, isFake: false);
	}

	internal virtual TreatOp CreateFakeTreatOp(TypeUsage type)
	{
		return new TreatOp(type, isFake: true);
	}

	internal virtual IsOfOp CreateIsOfOp(TypeUsage isOfType)
	{
		return new IsOfOp(isOfType, isOfOnly: false, m_boolType);
	}

	internal virtual IsOfOp CreateIsOfOnlyOp(TypeUsage isOfType)
	{
		return new IsOfOp(isOfType, isOfOnly: true, m_boolType);
	}

	internal virtual CastOp CreateCastOp(TypeUsage type)
	{
		return new CastOp(type);
	}

	internal virtual SoftCastOp CreateSoftCastOp(TypeUsage type)
	{
		return new SoftCastOp(type);
	}

	internal virtual ComparisonOp CreateComparisonOp(OpType opType, bool useDatabaseNullSemantics = false)
	{
		return new ComparisonOp(opType, BooleanType)
		{
			UseDatabaseNullSemantics = useDatabaseNullSemantics
		};
	}

	internal virtual LikeOp CreateLikeOp()
	{
		return new LikeOp(BooleanType);
	}

	internal virtual ConditionalOp CreateConditionalOp(OpType opType)
	{
		return new ConditionalOp(opType, BooleanType);
	}

	internal virtual CaseOp CreateCaseOp(TypeUsage type)
	{
		return new CaseOp(type);
	}

	internal virtual AggregateOp CreateAggregateOp(EdmFunction aggFunc, bool distinctAgg)
	{
		return new AggregateOp(aggFunc, distinctAgg);
	}

	internal virtual NewInstanceOp CreateNewInstanceOp(TypeUsage type)
	{
		return new NewInstanceOp(type);
	}

	internal virtual NewEntityOp CreateScopedNewEntityOp(TypeUsage type, List<RelProperty> relProperties, EntitySet entitySet)
	{
		return new NewEntityOp(type, relProperties, scoped: true, entitySet);
	}

	internal virtual NewEntityOp CreateNewEntityOp(TypeUsage type, List<RelProperty> relProperties)
	{
		return new NewEntityOp(type, relProperties, scoped: false, null);
	}

	internal virtual DiscriminatedNewEntityOp CreateDiscriminatedNewEntityOp(TypeUsage type, ExplicitDiscriminatorMap discriminatorMap, EntitySet entitySet, List<RelProperty> relProperties)
	{
		return new DiscriminatedNewEntityOp(type, discriminatorMap, entitySet, relProperties);
	}

	internal virtual NewMultisetOp CreateNewMultisetOp(TypeUsage type)
	{
		return new NewMultisetOp(type);
	}

	internal virtual NewRecordOp CreateNewRecordOp(TypeUsage type)
	{
		return new NewRecordOp(type);
	}

	internal virtual NewRecordOp CreateNewRecordOp(RowType type)
	{
		return new NewRecordOp(TypeUsage.Create(type));
	}

	internal virtual NewRecordOp CreateNewRecordOp(TypeUsage type, List<EdmProperty> fields)
	{
		return new NewRecordOp(type, fields);
	}

	internal virtual VarRefOp CreateVarRefOp(Var v)
	{
		return new VarRefOp(v);
	}

	internal virtual ArithmeticOp CreateArithmeticOp(OpType opType, TypeUsage type)
	{
		return new ArithmeticOp(opType, type);
	}

	internal PropertyOp CreatePropertyOp(EdmMember prop)
	{
		if (prop is NavigationProperty navigationProperty)
		{
			RelProperty relProperty = new RelProperty(navigationProperty.RelationshipType, navigationProperty.FromEndMember, navigationProperty.ToEndMember);
			AddRelPropertyReference(relProperty);
			RelProperty relProperty2 = new RelProperty(navigationProperty.RelationshipType, navigationProperty.ToEndMember, navigationProperty.FromEndMember);
			AddRelPropertyReference(relProperty2);
		}
		return new PropertyOp(Helper.GetModelTypeUsage(prop), prop);
	}

	internal RelPropertyOp CreateRelPropertyOp(RelProperty prop)
	{
		AddRelPropertyReference(prop);
		return new RelPropertyOp(prop.ToEnd.TypeUsage, prop);
	}

	internal virtual RefOp CreateRefOp(EntitySet entitySet, TypeUsage type)
	{
		return new RefOp(entitySet, type);
	}

	internal ExistsOp CreateExistsOp()
	{
		return new ExistsOp(BooleanType);
	}

	internal virtual ElementOp CreateElementOp(TypeUsage type)
	{
		return new ElementOp(type);
	}

	internal virtual GetEntityRefOp CreateGetEntityRefOp(TypeUsage type)
	{
		return new GetEntityRefOp(type);
	}

	internal virtual GetRefKeyOp CreateGetRefKeyOp(TypeUsage type)
	{
		return new GetRefKeyOp(type);
	}

	internal virtual CollectOp CreateCollectOp(TypeUsage type)
	{
		return new CollectOp(type);
	}

	internal virtual DerefOp CreateDerefOp(TypeUsage type)
	{
		return new DerefOp(type);
	}

	internal NavigateOp CreateNavigateOp(TypeUsage type, RelProperty relProperty)
	{
		AddRelPropertyReference(relProperty);
		return new NavigateOp(type, relProperty);
	}

	internal virtual VarDefListOp CreateVarDefListOp()
	{
		return VarDefListOp.Instance;
	}

	internal virtual VarDefOp CreateVarDefOp(Var v)
	{
		return new VarDefOp(v);
	}

	internal Node CreateVarDefNode(Node definingExpr, out Var computedVar)
	{
		ScalarOp scalarOp = definingExpr.Op as ScalarOp;
		computedVar = CreateComputedVar(scalarOp.Type);
		VarDefOp op = CreateVarDefOp(computedVar);
		return CreateNode(op, definingExpr);
	}

	internal Node CreateVarDefListNode(Node definingExpr, out Var computedVar)
	{
		Node arg = CreateVarDefNode(definingExpr, out computedVar);
		VarDefListOp op = CreateVarDefListOp();
		return CreateNode(op, arg);
	}

	internal ScanTableOp CreateScanTableOp(TableMD tableMetadata)
	{
		Table table = CreateTableInstance(tableMetadata);
		return CreateScanTableOp(table);
	}

	internal virtual ScanTableOp CreateScanTableOp(Table table)
	{
		return new ScanTableOp(table);
	}

	internal virtual ScanViewOp CreateScanViewOp(Table table)
	{
		return new ScanViewOp(table);
	}

	internal virtual ScanViewOp CreateScanViewOp(TableMD tableMetadata)
	{
		Table table = CreateTableInstance(tableMetadata);
		return CreateScanViewOp(table);
	}

	internal virtual UnnestOp CreateUnnestOp(Var v)
	{
		Table t = CreateTableInstance(CreateTableDefinition(TypeHelpers.GetEdmType<CollectionType>(v.Type).TypeUsage));
		return CreateUnnestOp(v, t);
	}

	internal virtual UnnestOp CreateUnnestOp(Var v, Table t)
	{
		return new UnnestOp(v, t);
	}

	internal virtual FilterOp CreateFilterOp()
	{
		return FilterOp.Instance;
	}

	internal virtual ProjectOp CreateProjectOp(VarVec vars)
	{
		return new ProjectOp(vars);
	}

	internal virtual ProjectOp CreateProjectOp(Var v)
	{
		VarVec varVec = CreateVarVec();
		varVec.Set(v);
		return new ProjectOp(varVec);
	}

	internal virtual InnerJoinOp CreateInnerJoinOp()
	{
		return InnerJoinOp.Instance;
	}

	internal virtual LeftOuterJoinOp CreateLeftOuterJoinOp()
	{
		return LeftOuterJoinOp.Instance;
	}

	internal virtual FullOuterJoinOp CreateFullOuterJoinOp()
	{
		return FullOuterJoinOp.Instance;
	}

	internal virtual CrossJoinOp CreateCrossJoinOp()
	{
		return CrossJoinOp.Instance;
	}

	internal virtual CrossApplyOp CreateCrossApplyOp()
	{
		return CrossApplyOp.Instance;
	}

	internal virtual OuterApplyOp CreateOuterApplyOp()
	{
		return OuterApplyOp.Instance;
	}

	internal static SortKey CreateSortKey(Var v, bool asc, string collation)
	{
		return new SortKey(v, asc, collation);
	}

	internal static SortKey CreateSortKey(Var v, bool asc)
	{
		return new SortKey(v, asc, "");
	}

	internal static SortKey CreateSortKey(Var v)
	{
		return new SortKey(v, asc: true, "");
	}

	internal virtual SortOp CreateSortOp(List<SortKey> sortKeys)
	{
		return new SortOp(sortKeys);
	}

	internal virtual ConstrainedSortOp CreateConstrainedSortOp(List<SortKey> sortKeys)
	{
		return new ConstrainedSortOp(sortKeys, withTies: false);
	}

	internal virtual ConstrainedSortOp CreateConstrainedSortOp(List<SortKey> sortKeys, bool withTies)
	{
		return new ConstrainedSortOp(sortKeys, withTies);
	}

	internal virtual GroupByOp CreateGroupByOp(VarVec gbyKeys, VarVec outputs)
	{
		return new GroupByOp(gbyKeys, outputs);
	}

	internal virtual GroupByIntoOp CreateGroupByIntoOp(VarVec gbyKeys, VarVec inputs, VarVec outputs)
	{
		return new GroupByIntoOp(gbyKeys, inputs, outputs);
	}

	internal virtual DistinctOp CreateDistinctOp(VarVec keyVars)
	{
		return new DistinctOp(keyVars);
	}

	internal virtual DistinctOp CreateDistinctOp(Var keyVar)
	{
		return new DistinctOp(CreateVarVec(keyVar));
	}

	internal virtual UnionAllOp CreateUnionAllOp(VarMap leftMap, VarMap rightMap)
	{
		return CreateUnionAllOp(leftMap, rightMap, null);
	}

	internal virtual UnionAllOp CreateUnionAllOp(VarMap leftMap, VarMap rightMap, Var branchDiscriminator)
	{
		VarVec varVec = CreateVarVec();
		foreach (Var key in leftMap.Keys)
		{
			varVec.Set(key);
		}
		return new UnionAllOp(varVec, leftMap, rightMap, branchDiscriminator);
	}

	internal virtual IntersectOp CreateIntersectOp(VarMap leftMap, VarMap rightMap)
	{
		VarVec varVec = CreateVarVec();
		foreach (Var key in leftMap.Keys)
		{
			varVec.Set(key);
		}
		return new IntersectOp(varVec, leftMap, rightMap);
	}

	internal virtual ExceptOp CreateExceptOp(VarMap leftMap, VarMap rightMap)
	{
		VarVec varVec = CreateVarVec();
		foreach (Var key in leftMap.Keys)
		{
			varVec.Set(key);
		}
		return new ExceptOp(varVec, leftMap, rightMap);
	}

	internal virtual SingleRowOp CreateSingleRowOp()
	{
		return SingleRowOp.Instance;
	}

	internal virtual SingleRowTableOp CreateSingleRowTableOp()
	{
		return SingleRowTableOp.Instance;
	}

	internal virtual PhysicalProjectOp CreatePhysicalProjectOp(VarList outputVars, SimpleCollectionColumnMap columnMap)
	{
		return new PhysicalProjectOp(outputVars, columnMap);
	}

	internal virtual PhysicalProjectOp CreatePhysicalProjectOp(Var outputVar)
	{
		VarList varList = CreateVarList();
		varList.Add(outputVar);
		VarRefColumnMap varRefColumnMap = new VarRefColumnMap(outputVar);
		SimpleCollectionColumnMap columnMap = new SimpleCollectionColumnMap(TypeUtils.CreateCollectionType(varRefColumnMap.Type), null, varRefColumnMap, new SimpleColumnMap[0], new SimpleColumnMap[0]);
		return CreatePhysicalProjectOp(varList, columnMap);
	}

	internal static CollectionInfo CreateCollectionInfo(Var collectionVar, ColumnMap columnMap, VarList flattenedElementVars, VarVec keys, List<SortKey> sortKeys, object discriminatorValue)
	{
		return new CollectionInfo(collectionVar, columnMap, flattenedElementVars, keys, sortKeys, discriminatorValue);
	}

	internal virtual SingleStreamNestOp CreateSingleStreamNestOp(VarVec keys, List<SortKey> prefixSortKeys, List<SortKey> postfixSortKeys, VarVec outputVars, List<CollectionInfo> collectionInfoList, Var discriminatorVar)
	{
		return new SingleStreamNestOp(keys, prefixSortKeys, postfixSortKeys, outputVars, collectionInfoList, discriminatorVar);
	}

	internal virtual MultiStreamNestOp CreateMultiStreamNestOp(List<SortKey> prefixSortKeys, VarVec outputVars, List<CollectionInfo> collectionInfoList)
	{
		return new MultiStreamNestOp(prefixSortKeys, outputVars, collectionInfoList);
	}

	internal virtual NodeInfo GetNodeInfo(Node n)
	{
		return n.GetNodeInfo(this);
	}

	internal virtual ExtendedNodeInfo GetExtendedNodeInfo(Node n)
	{
		return n.GetExtendedNodeInfo(this);
	}

	internal virtual void RecomputeNodeInfo(Node n)
	{
		m_nodeInfoVisitor.RecomputeNodeInfo(n);
	}

	internal virtual KeyVec PullupKeys(Node n)
	{
		return m_keyPullupVisitor.GetKeys(n);
	}

	internal static bool EqualTypes(TypeUsage x, TypeUsage y)
	{
		return TypeUsageEqualityComparer.Instance.Equals(x, y);
	}

	internal static bool EqualTypes(EdmType x, EdmType y)
	{
		return TypeUsageEqualityComparer.Equals(x, y);
	}

	internal virtual void BuildUnionAllLadder(IList<Node> inputNodes, IList<Var> inputVars, out Node resultNode, out IList<Var> resultVars)
	{
		if (inputNodes.Count == 0)
		{
			resultNode = null;
			resultVars = null;
			return;
		}
		int num = inputVars.Count / inputNodes.Count;
		if (inputNodes.Count == 1)
		{
			resultNode = inputNodes[0];
			resultVars = inputVars;
			return;
		}
		List<Var> list = new List<Var>();
		Node node = inputNodes[0];
		for (int i = 0; i < num; i++)
		{
			list.Add(inputVars[i]);
		}
		for (int j = 1; j < inputNodes.Count; j++)
		{
			VarMap varMap = new VarMap();
			VarMap varMap2 = new VarMap();
			List<Var> list2 = new List<Var>();
			for (int k = 0; k < num; k++)
			{
				SetOpVar setOpVar = CreateSetOpVar(list[k].Type);
				list2.Add(setOpVar);
				varMap.Add(setOpVar, list[k]);
				varMap2.Add(setOpVar, inputVars[j * num + k]);
			}
			Op op = CreateUnionAllOp(varMap, varMap2);
			node = CreateNode(op, node, inputNodes[j]);
			list = list2;
		}
		resultNode = node;
		resultVars = list;
	}

	internal virtual void BuildUnionAllLadder(IList<Node> inputNodes, IList<Var> inputVars, out Node resultNode, out Var resultVar)
	{
		BuildUnionAllLadder(inputNodes, inputVars, out resultNode, out IList<Var> resultVars);
		if (resultVars != null && resultVars.Count > 0)
		{
			resultVar = resultVars[0];
		}
		else
		{
			resultVar = null;
		}
	}

	internal virtual Node BuildProject(Node inputNode, IEnumerable<Var> inputVars, IEnumerable<Node> computedExpressions)
	{
		VarDefListOp op = CreateVarDefListOp();
		Node node = CreateNode(op);
		VarVec varVec = CreateVarVec(inputVars);
		foreach (Node computedExpression in computedExpressions)
		{
			Var v = CreateComputedVar(computedExpression.Op.Type);
			varVec.Set(v);
			VarDefOp op2 = CreateVarDefOp(v);
			Node item = CreateNode(op2, computedExpression);
			node.Children.Add(item);
		}
		return CreateNode(CreateProjectOp(varVec), inputNode, node);
	}

	internal virtual Node BuildProject(Node input, Node computedExpression, out Var projectVar)
	{
		Node node = BuildProject(input, new Var[0], new Node[1] { computedExpression });
		projectVar = ((ProjectOp)node.Op).Outputs.First;
		return node;
	}

	internal virtual void BuildOfTypeTree(Node inputNode, Var inputVar, TypeUsage desiredType, bool includeSubtypes, out Node resultNode, out Var resultVar)
	{
		Op op = (includeSubtypes ? CreateIsOfOp(desiredType) : CreateIsOfOnlyOp(desiredType));
		Node arg = CreateNode(op, CreateNode(CreateVarRefOp(inputVar)));
		Node inputNode2 = CreateNode(CreateFilterOp(), inputNode, arg);
		resultNode = BuildFakeTreatProject(inputNode2, inputVar, desiredType, out resultVar);
	}

	internal virtual Node BuildFakeTreatProject(Node inputNode, Var inputVar, TypeUsage desiredType, out Var resultVar)
	{
		Node computedExpression = CreateNode(CreateFakeTreatOp(desiredType), CreateNode(CreateVarRefOp(inputVar)));
		return BuildProject(inputNode, computedExpression, out resultVar);
	}

	internal Node BuildComparison(OpType opType, Node arg0, Node arg1, bool useDatabaseNullSemantics = false)
	{
		if (!EqualTypes(arg0.Op.Type, arg1.Op.Type))
		{
			TypeUsage commonTypeUsage = TypeHelpers.GetCommonTypeUsage(arg0.Op.Type, arg1.Op.Type);
			if (!EqualTypes(commonTypeUsage, arg0.Op.Type))
			{
				arg0 = CreateNode(CreateSoftCastOp(commonTypeUsage), arg0);
			}
			if (!EqualTypes(commonTypeUsage, arg1.Op.Type))
			{
				arg1 = CreateNode(CreateSoftCastOp(commonTypeUsage), arg1);
			}
		}
		return CreateNode(CreateComparisonOp(opType, useDatabaseNullSemantics), arg0, arg1);
	}

	internal virtual Node BuildCollect(Node relOpNode, Var relOpVar)
	{
		Node arg = CreateNode(CreatePhysicalProjectOp(relOpVar), relOpNode);
		TypeUsage type = TypeHelpers.CreateCollectionTypeUsage(relOpVar.Type);
		return CreateNode(CreateCollectOp(type), arg);
	}

	private void AddRelPropertyReference(RelProperty relProperty)
	{
		if (relProperty.ToEnd.RelationshipMultiplicity != RelationshipMultiplicity.Many && !m_referencedRelProperties.Contains(relProperty))
		{
			m_referencedRelProperties.Add(relProperty);
		}
	}

	internal virtual bool IsRelPropertyReferenced(RelProperty relProperty)
	{
		return m_referencedRelProperties.Contains(relProperty);
	}
}
