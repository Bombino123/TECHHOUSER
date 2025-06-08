using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class NominalTypeEliminator : BasicOpVisitorOfNode
{
	internal enum OperationKind
	{
		Equality,
		IsNull,
		GetIdentity,
		GetKeys,
		All
	}

	private readonly Dictionary<Var, PropertyRefList> m_varPropertyMap;

	private readonly Dictionary<Node, PropertyRefList> m_nodePropertyMap;

	private readonly VarInfoMap m_varInfoMap;

	private readonly PlanCompiler m_compilerState;

	private readonly StructuredTypeInfo m_typeInfo;

	private readonly Dictionary<EdmFunction, EdmProperty[]> m_tvfResultKeys;

	private readonly Dictionary<TypeUsage, TypeUsage> m_typeToNewTypeMap;

	private const string PrefixMatchCharacter = "%";

	private Command m_command => m_compilerState.Command;

	private TypeUsage DefaultTypeIdType => m_command.StringType;

	private NominalTypeEliminator(PlanCompiler compilerState, StructuredTypeInfo typeInfo, Dictionary<Var, PropertyRefList> varPropertyMap, Dictionary<Node, PropertyRefList> nodePropertyMap, Dictionary<EdmFunction, EdmProperty[]> tvfResultKeys)
	{
		m_compilerState = compilerState;
		m_typeInfo = typeInfo;
		m_varPropertyMap = varPropertyMap;
		m_nodePropertyMap = nodePropertyMap;
		m_varInfoMap = new VarInfoMap();
		m_tvfResultKeys = tvfResultKeys;
		m_typeToNewTypeMap = new Dictionary<TypeUsage, TypeUsage>(TypeUsageEqualityComparer.Instance);
	}

	internal static void Process(PlanCompiler compilerState, StructuredTypeInfo structuredTypeInfo, Dictionary<EdmFunction, EdmProperty[]> tvfResultKeys)
	{
		PropertyPushdownHelper.Process(compilerState.Command, out var varPropertyRefs, out var nodePropertyRefs);
		new NominalTypeEliminator(compilerState, structuredTypeInfo, varPropertyRefs, nodePropertyRefs, tvfResultKeys).Process();
	}

	private void Process()
	{
		ParameterVar[] array = (from v in m_command.Vars.OfType<ParameterVar>()
			where TypeSemantics.IsEnumerationType(v.Type) || TypeSemantics.IsStrongSpatialType(v.Type)
			select v).ToArray();
		foreach (ParameterVar parameterVar in array)
		{
			ParameterVar newVar = (TypeSemantics.IsEnumerationType(parameterVar.Type) ? m_command.ReplaceEnumParameterVar(parameterVar) : m_command.ReplaceStrongSpatialParameterVar(parameterVar));
			m_varInfoMap.CreatePrimitiveTypeVarInfo(parameterVar, newVar);
		}
		Node root = m_command.Root;
		PlanCompiler.Assert(root.Op.OpType == OpType.PhysicalProject, "root node is not PhysicalProjectOp?");
		root.Op.Accept(this, root);
	}

	private TypeUsage GetNewType(TypeUsage type)
	{
		if (m_typeToNewTypeMap.TryGetValue(type, out var value))
		{
			return value;
		}
		value = (TypeHelpers.TryGetEdmType<CollectionType>(type, out var type2) ? TypeUtils.CreateCollectionType(GetNewType(type2.TypeUsage)) : (TypeUtils.IsStructuredType(type) ? m_typeInfo.GetTypeInfo(type).FlattenedTypeUsage : (TypeSemantics.IsEnumerationType(type) ? TypeHelpers.CreateEnumUnderlyingTypeUsage(type) : ((!TypeSemantics.IsStrongSpatialType(type)) ? type : TypeHelpers.CreateSpatialUnionTypeUsage(type)))));
		m_typeToNewTypeMap[type] = value;
		return value;
	}

	private Node BuildAccessor(Node input, EdmProperty property)
	{
		Op op = input.Op;
		if (op is NewRecordOp newRecordOp)
		{
			if (newRecordOp.GetFieldPosition(property, out var fieldPosition))
			{
				return Copy(input.Children[fieldPosition]);
			}
			return null;
		}
		if (op.OpType == OpType.Null)
		{
			return null;
		}
		PropertyOp op2 = m_command.CreatePropertyOp(property);
		return m_command.CreateNode(op2, Copy(input));
	}

	private Node BuildAccessorWithNulls(Node input, EdmProperty property)
	{
		Node node = BuildAccessor(input, property);
		if (node == null)
		{
			node = CreateNullConstantNode(Helper.GetModelTypeUsage(property));
		}
		return node;
	}

	private Node BuildTypeIdAccessor(Node input, TypeInfo typeInfo)
	{
		if (typeInfo.HasTypeIdProperty)
		{
			return BuildAccessorWithNulls(input, typeInfo.TypeIdProperty);
		}
		return CreateTypeIdConstant(typeInfo);
	}

	private Node BuildSoftCast(Node node, TypeUsage targetType)
	{
		PlanCompiler.Assert(node.Op.IsScalarOp, "Attempting SoftCast around non-ScalarOp?");
		if (Command.EqualTypes(node.Op.Type, targetType))
		{
			return node;
		}
		while (node.Op.OpType == OpType.SoftCast)
		{
			node = node.Child0;
		}
		return m_command.CreateNode(m_command.CreateSoftCastOp(targetType), node);
	}

	private Node Copy(Node n)
	{
		return OpCopier.Copy(m_command, n);
	}

	private Node CreateNullConstantNode(TypeUsage type)
	{
		return m_command.CreateNode(m_command.CreateNullOp(type));
	}

	private Node CreateNullSentinelConstant()
	{
		NullSentinelOp op = m_command.CreateNullSentinelOp();
		return m_command.CreateNode(op);
	}

	private Node CreateTypeIdConstant(TypeInfo typeInfo)
	{
		object typeId = typeInfo.TypeId;
		TypeUsage type = ((typeInfo.RootType.DiscriminatorMap == null) ? DefaultTypeIdType : Helper.GetModelTypeUsage(typeInfo.RootType.DiscriminatorMap.DiscriminatorProperty));
		InternalConstantOp op = m_command.CreateInternalConstantOp(type, typeId);
		return m_command.CreateNode(op);
	}

	private Node CreateTypeIdConstantForPrefixMatch(TypeInfo typeInfo)
	{
		string value = typeInfo.TypeId?.ToString() + "%";
		InternalConstantOp op = m_command.CreateInternalConstantOp(DefaultTypeIdType, value);
		return m_command.CreateNode(op);
	}

	private IEnumerable<PropertyRef> GetPropertyRefsForComparisonAndIsNull(TypeInfo typeInfo, OperationKind opKind)
	{
		PlanCompiler.Assert(opKind == OperationKind.IsNull || opKind == OperationKind.Equality, "Unexpected opKind: " + opKind.ToString() + "; Can only handle IsNull and Equality");
		TypeUsage type = typeInfo.Type;
		RowType type2 = null;
		if (TypeHelpers.TryGetEdmType<RowType>(type, out type2))
		{
			if (opKind == OperationKind.IsNull && typeInfo.HasNullSentinelProperty)
			{
				yield return NullSentinelPropertyRef.Instance;
				yield break;
			}
			foreach (EdmProperty i in type2.Properties)
			{
				if (!TypeUtils.IsStructuredType(Helper.GetModelTypeUsage(i)))
				{
					yield return new SimplePropertyRef(i);
					continue;
				}
				TypeInfo typeInfo2 = m_typeInfo.GetTypeInfo(Helper.GetModelTypeUsage(i));
				foreach (PropertyRef propertyRef in GetPropertyRefs(typeInfo2, opKind))
				{
					yield return propertyRef.CreateNestedPropertyRef(i);
				}
			}
			yield break;
		}
		EntityType type3 = null;
		if (TypeHelpers.TryGetEdmType<EntityType>(type, out type3))
		{
			if (opKind == OperationKind.Equality || (opKind == OperationKind.IsNull && !typeInfo.HasTypeIdProperty))
			{
				foreach (PropertyRef identityPropertyRef in typeInfo.GetIdentityPropertyRefs())
				{
					yield return identityPropertyRef;
				}
			}
			else
			{
				yield return TypeIdPropertyRef.Instance;
			}
			yield break;
		}
		ComplexType type4 = null;
		if (TypeHelpers.TryGetEdmType<ComplexType>(type, out type4))
		{
			PlanCompiler.Assert(opKind == OperationKind.IsNull, "complex types not equality-comparable");
			PlanCompiler.Assert(typeInfo.HasNullSentinelProperty, "complex type with no null sentinel property: can't handle isNull");
			yield return NullSentinelPropertyRef.Instance;
			yield break;
		}
		RefType type5 = null;
		if (TypeHelpers.TryGetEdmType<RefType>(type, out type5))
		{
			foreach (PropertyRef allPropertyRef in typeInfo.GetAllPropertyRefs())
			{
				yield return allPropertyRef;
			}
		}
		else
		{
			PlanCompiler.Assert(condition: false, "Unknown type");
		}
	}

	private IEnumerable<PropertyRef> GetPropertyRefs(TypeInfo typeInfo, OperationKind opKind)
	{
		PlanCompiler.Assert(opKind != OperationKind.All, "unexpected attempt to GetPropertyRefs(...,OperationKind.All)");
		return opKind switch
		{
			OperationKind.GetKeys => typeInfo.GetKeyPropertyRefs(), 
			OperationKind.GetIdentity => typeInfo.GetIdentityPropertyRefs(), 
			_ => GetPropertyRefsForComparisonAndIsNull(typeInfo, opKind), 
		};
	}

	private IEnumerable<EdmProperty> GetProperties(TypeInfo typeInfo, OperationKind opKind)
	{
		if (opKind == OperationKind.All)
		{
			foreach (EdmProperty allProperty in typeInfo.GetAllProperties())
			{
				yield return allProperty;
			}
			yield break;
		}
		foreach (PropertyRef propertyRef in GetPropertyRefs(typeInfo, opKind))
		{
			yield return typeInfo.GetNewProperty(propertyRef);
		}
	}

	private void GetPropertyValues(TypeInfo typeInfo, OperationKind opKind, Node input, bool ignoreMissingProperties, out List<EdmProperty> properties, out List<Node> values)
	{
		values = new List<Node>();
		properties = new List<EdmProperty>();
		foreach (EdmProperty property in GetProperties(typeInfo, opKind))
		{
			KeyValuePair<EdmProperty, Node> propertyValue = GetPropertyValue(input, property, ignoreMissingProperties);
			if (propertyValue.Value != null)
			{
				properties.Add(propertyValue.Key);
				values.Add(propertyValue.Value);
			}
		}
	}

	private KeyValuePair<EdmProperty, Node> GetPropertyValue(Node input, EdmProperty property, bool ignoreMissingProperties)
	{
		Node node = null;
		node = (ignoreMissingProperties ? BuildAccessor(input, property) : BuildAccessorWithNulls(input, property));
		return new KeyValuePair<EdmProperty, Node>(property, node);
	}

	private List<System.Data.Entity.Core.Query.InternalTrees.SortKey> HandleSortKeys(List<System.Data.Entity.Core.Query.InternalTrees.SortKey> keys)
	{
		List<System.Data.Entity.Core.Query.InternalTrees.SortKey> list = new List<System.Data.Entity.Core.Query.InternalTrees.SortKey>();
		bool flag = false;
		foreach (System.Data.Entity.Core.Query.InternalTrees.SortKey key in keys)
		{
			if (!m_varInfoMap.TryGetVarInfo(key.Var, out var varInfo))
			{
				list.Add(key);
				continue;
			}
			if (varInfo is StructuredVarInfo { NewVarsIncludeNullSentinelVar: not false })
			{
				m_compilerState.HasSortingOnNullSentinels = true;
			}
			foreach (Var newVar in varInfo.NewVars)
			{
				System.Data.Entity.Core.Query.InternalTrees.SortKey item = Command.CreateSortKey(newVar, key.AscendingSort, key.Collation);
				list.Add(item);
			}
			flag = true;
		}
		if (!flag)
		{
			return keys;
		}
		return list;
	}

	private Node CreateTVFProjection(Node unnestNode, List<Var> unnestOpTableColumns, TypeInfo unnestOpTableTypeInfo, out List<Var> newVars)
	{
		RowType rowType = unnestOpTableTypeInfo.Type.EdmType as RowType;
		PlanCompiler.Assert(rowType != null, "Unexpected TVF return type (must be row): " + unnestOpTableTypeInfo.Type);
		List<Var> list = new List<Var>();
		List<Node> list2 = new List<Node>();
		PropertyRef[] array = unnestOpTableTypeInfo.PropertyRefList.ToArray();
		Dictionary<EdmProperty, PropertyRef> dictionary = new Dictionary<EdmProperty, PropertyRef>();
		PropertyRef[] array2 = array;
		foreach (PropertyRef propertyRef in array2)
		{
			dictionary.Add(unnestOpTableTypeInfo.GetNewProperty(propertyRef), propertyRef);
		}
		foreach (EdmProperty property in unnestOpTableTypeInfo.FlattenedType.Properties)
		{
			PropertyRef propertyRef2 = dictionary[property];
			Var computedVar = null;
			if (propertyRef2 is SimplePropertyRef simplePropertyRef)
			{
				int num = rowType.Members.IndexOf(simplePropertyRef.Property);
				PlanCompiler.Assert(num >= 0, "Can't find a column in the TVF result type");
				list2.Add(m_command.CreateVarDefNode(m_command.CreateNode(m_command.CreateVarRefOp(unnestOpTableColumns[num])), out computedVar));
			}
			else if (propertyRef2 is NullSentinelPropertyRef)
			{
				list2.Add(m_command.CreateVarDefNode(CreateNullSentinelConstant(), out computedVar));
			}
			PlanCompiler.Assert(computedVar != null, "TVFs returning a collection of rows with non-primitive properties are not supported");
			list.Add(computedVar);
		}
		newVars = list;
		return m_command.CreateNode(m_command.CreateProjectOp(m_command.CreateVarVec(list)), unnestNode, m_command.CreateNode(m_command.CreateVarDefListOp(), list2));
	}

	public override Node Visit(VarDefListOp op, Node n)
	{
		VisitChildren(n);
		List<Node> list = new List<Node>();
		foreach (Node child in n.Children)
		{
			PlanCompiler.Assert(child.Op is VarDefOp, "VarDefOp expected");
			VarDefOp varDefOp = (VarDefOp)child.Op;
			if (TypeUtils.IsStructuredType(varDefOp.Var.Type) || TypeUtils.IsCollectionType(varDefOp.Var.Type))
			{
				FlattenComputedVar((ComputedVar)varDefOp.Var, child, out var newNodes, out var _);
				foreach (Node item in newNodes)
				{
					list.Add(item);
				}
			}
			else if (TypeSemantics.IsEnumerationType(varDefOp.Var.Type) || TypeSemantics.IsStrongSpatialType(varDefOp.Var.Type))
			{
				list.Add(FlattenEnumOrStrongSpatialVar(varDefOp, child.Child0));
			}
			else
			{
				list.Add(child);
			}
		}
		return m_command.CreateNode(n.Op, list);
	}

	private void FlattenComputedVar(ComputedVar v, Node node, out List<Node> newNodes, out TypeUsage newType)
	{
		newNodes = new List<Node>();
		Node child = node.Child0;
		newType = null;
		if (TypeUtils.IsCollectionType(v.Type))
		{
			PlanCompiler.Assert(child.Op.OpType != OpType.Function, "Flattening of TVF output is not allowed.");
			newType = GetNewType(v.Type);
			Var computedVar;
			Node item = m_command.CreateVarDefNode(child, out computedVar);
			newNodes.Add(item);
			m_varInfoMap.CreateCollectionVarInfo(v, computedVar);
			return;
		}
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(v.Type);
		PropertyRefList propertyRefList = m_varPropertyMap[v];
		List<Var> list = new List<Var>();
		List<EdmProperty> list2 = new List<EdmProperty>();
		newNodes = new List<Node>();
		bool flag = false;
		foreach (PropertyRef propertyRef in typeInfo.PropertyRefList)
		{
			if (!propertyRefList.Contains(propertyRef))
			{
				continue;
			}
			EdmProperty newProperty = typeInfo.GetNewProperty(propertyRef);
			Node node2 = null;
			if (propertyRefList.AllProperties)
			{
				node2 = BuildAccessorWithNulls(child, newProperty);
			}
			else
			{
				node2 = BuildAccessor(child, newProperty);
				if (node2 == null)
				{
					continue;
				}
			}
			list2.Add(newProperty);
			Var computedVar2;
			Node item2 = m_command.CreateVarDefNode(node2, out computedVar2);
			newNodes.Add(item2);
			list.Add(computedVar2);
			if (!flag && IsNullSentinelPropertyRef(propertyRef))
			{
				flag = true;
			}
		}
		m_varInfoMap.CreateStructuredVarInfo(v, typeInfo.FlattenedType, list, list2, flag);
	}

	private static bool IsNullSentinelPropertyRef(PropertyRef propertyRef)
	{
		if (propertyRef is NullSentinelPropertyRef)
		{
			return true;
		}
		if (!(propertyRef is NestedPropertyRef nestedPropertyRef))
		{
			return false;
		}
		return nestedPropertyRef.OuterProperty is NullSentinelPropertyRef;
	}

	private Node FlattenEnumOrStrongSpatialVar(VarDefOp varDefOp, Node node)
	{
		Var computedVar;
		Node result = m_command.CreateVarDefNode(node, out computedVar);
		m_varInfoMap.CreatePrimitiveTypeVarInfo(varDefOp.Var, computedVar);
		return result;
	}

	public override Node Visit(PhysicalProjectOp op, Node n)
	{
		VisitChildren(n);
		VarList outputVars = FlattenVarList(op.Outputs);
		SimpleCollectionColumnMap columnMap = ExpandColumnMap(op.ColumnMap);
		PhysicalProjectOp op2 = m_command.CreatePhysicalProjectOp(outputVars, columnMap);
		n.Op = op2;
		return n;
	}

	private SimpleCollectionColumnMap ExpandColumnMap(SimpleCollectionColumnMap columnMap)
	{
		VarRefColumnMap varRefColumnMap = columnMap.Element as VarRefColumnMap;
		PlanCompiler.Assert(varRefColumnMap != null, "Encountered a SimpleCollectionColumnMap element that is not VarRefColumnMap when expanding a column map in NominalTypeEliminator.");
		if (!m_varInfoMap.TryGetVarInfo(varRefColumnMap.Var, out var varInfo))
		{
			return columnMap;
		}
		if (TypeUtils.IsStructuredType(varRefColumnMap.Var.Type))
		{
			TypeInfo typeInfo = m_typeInfo.GetTypeInfo(varRefColumnMap.Var.Type);
			PlanCompiler.Assert(typeInfo.RootType.FlattenedType.Properties.Count == varInfo.NewVars.Count, "Var count mismatch; Expected " + typeInfo.RootType.FlattenedType.Properties.Count + "; got " + varInfo.NewVars.Count + " instead.");
		}
		ColumnMap columnMap2 = new ColumnMapProcessor(varRefColumnMap, varInfo, m_typeInfo).ExpandColumnMap();
		return new SimpleCollectionColumnMap(TypeUtils.CreateCollectionType(columnMap2.Type), columnMap2.Name, columnMap2, columnMap.Keys, columnMap.ForeignKeys);
	}

	private IEnumerable<Var> FlattenVars(IEnumerable<Var> vars)
	{
		foreach (Var var in vars)
		{
			if (!m_varInfoMap.TryGetVarInfo(var, out var varInfo))
			{
				yield return var;
				continue;
			}
			foreach (Var newVar in varInfo.NewVars)
			{
				yield return newVar;
			}
		}
	}

	private VarVec FlattenVarSet(VarVec varSet)
	{
		return m_command.CreateVarVec(FlattenVars(varSet));
	}

	private VarList FlattenVarList(VarList varList)
	{
		return Command.CreateVarList(FlattenVars(varList));
	}

	public override Node Visit(DistinctOp op, Node n)
	{
		VisitChildren(n);
		VarVec keyVars = FlattenVarSet(op.Keys);
		n.Op = m_command.CreateDistinctOp(keyVars);
		return n;
	}

	public override Node Visit(GroupByOp op, Node n)
	{
		VisitChildren(n);
		VarVec varVec = FlattenVarSet(op.Keys);
		VarVec varVec2 = FlattenVarSet(op.Outputs);
		if (varVec != op.Keys || varVec2 != op.Outputs)
		{
			n.Op = m_command.CreateGroupByOp(varVec, varVec2);
		}
		return n;
	}

	public override Node Visit(GroupByIntoOp op, Node n)
	{
		VisitChildren(n);
		VarVec varVec = FlattenVarSet(op.Keys);
		VarVec varVec2 = FlattenVarSet(op.Inputs);
		VarVec varVec3 = FlattenVarSet(op.Outputs);
		if (varVec != op.Keys || varVec2 != op.Inputs || varVec3 != op.Outputs)
		{
			n.Op = m_command.CreateGroupByIntoOp(varVec, varVec2, varVec3);
		}
		return n;
	}

	public override Node Visit(ProjectOp op, Node n)
	{
		VisitChildren(n);
		VarVec varVec = FlattenVarSet(op.Outputs);
		if (op.Outputs != varVec)
		{
			if (varVec.IsEmpty)
			{
				return n.Child0;
			}
			n.Op = m_command.CreateProjectOp(varVec);
		}
		return n;
	}

	public override Node Visit(ScanTableOp op, Node n)
	{
		Var var = op.Table.Columns[0];
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(var.Type);
		RowType flattenedType = typeInfo.FlattenedType;
		List<EdmProperty> list = new List<EdmProperty>();
		List<EdmMember> list2 = new List<EdmMember>();
		HashSet<string> hashSet = new HashSet<string>();
		foreach (EdmProperty allStructuralMember in TypeHelpers.GetAllStructuralMembers(var.Type.EdmType))
		{
			hashSet.Add(allStructuralMember.Name);
		}
		foreach (EdmProperty property in flattenedType.Properties)
		{
			if (hashSet.Contains(property.Name))
			{
				list.Add(property);
			}
		}
		foreach (PropertyRef keyPropertyRef in typeInfo.GetKeyPropertyRefs())
		{
			EdmProperty newProperty = typeInfo.GetNewProperty(keyPropertyRef);
			list2.Add(newProperty);
		}
		TableMD tableMetadata = m_command.CreateFlatTableDefinition(list, list2, op.Table.TableMetadata.Extent);
		Table table = m_command.CreateTableInstance(tableMetadata);
		m_varInfoMap.CreateStructuredVarInfo(var, flattenedType, table.Columns, list);
		n.Op = m_command.CreateScanTableOp(table);
		return n;
	}

	internal static Var GetSingletonVar(Node n)
	{
		switch (n.Op.OpType)
		{
		case OpType.Project:
		{
			ProjectOp projectOp = (ProjectOp)n.Op;
			if (projectOp.Outputs.Count != 1)
			{
				return null;
			}
			return projectOp.Outputs.First;
		}
		case OpType.ScanTable:
		{
			ScanTableOp scanTableOp = (ScanTableOp)n.Op;
			if (scanTableOp.Table.Columns.Count != 1)
			{
				return null;
			}
			return scanTableOp.Table.Columns[0];
		}
		case OpType.Filter:
		case OpType.Sort:
		case OpType.ConstrainedSort:
		case OpType.SingleRow:
			return GetSingletonVar(n.Child0);
		case OpType.UnionAll:
		case OpType.Intersect:
		case OpType.Except:
		{
			SetOp setOp = (SetOp)n.Op;
			if (setOp.Outputs.Count != 1)
			{
				return null;
			}
			return setOp.Outputs.First;
		}
		case OpType.Unnest:
		{
			UnnestOp unnestOp = (UnnestOp)n.Op;
			if (unnestOp.Table.Columns.Count != 1)
			{
				return null;
			}
			return unnestOp.Table.Columns[0];
		}
		case OpType.Distinct:
		{
			DistinctOp distinctOp = (DistinctOp)n.Op;
			if (distinctOp.Keys.Count != 1)
			{
				return null;
			}
			return distinctOp.Keys.First;
		}
		default:
			return null;
		}
	}

	public override Node Visit(ScanViewOp op, Node n)
	{
		Var singletonVar = GetSingletonVar(n.Child0);
		PlanCompiler.Assert(singletonVar != null, "cannot identify Var for the input node to the ScanViewOp");
		PlanCompiler.Assert(op.Table.Columns.Count == 1, "table for scanViewOp has more than on column?");
		Var var = op.Table.Columns[0];
		Node result = VisitNode(n.Child0);
		if (!m_varInfoMap.TryGetVarInfo(singletonVar, out var varInfo))
		{
			PlanCompiler.Assert(condition: false, "didn't find inputVar for scanViewOp?");
		}
		StructuredVarInfo structuredVarInfo = (StructuredVarInfo)varInfo;
		m_typeInfo.GetTypeInfo(var.Type);
		m_varInfoMap.CreateStructuredVarInfo(var, structuredVarInfo.NewType, structuredVarInfo.NewVars, structuredVarInfo.Fields);
		return result;
	}

	public override Node Visit(SortOp op, Node n)
	{
		VisitChildren(n);
		List<System.Data.Entity.Core.Query.InternalTrees.SortKey> list = HandleSortKeys(op.Keys);
		if (list != op.Keys)
		{
			n.Op = m_command.CreateSortOp(list);
		}
		return n;
	}

	public override Node Visit(UnnestOp op, Node n)
	{
		VisitChildren(n);
		Var var = null;
		EdmFunction edmFunction = null;
		if (n.HasChild0)
		{
			Node child = n.Child0;
			if (child.Op is VarDefOp varDefOp && TypeUtils.IsCollectionType(varDefOp.Var.Type))
			{
				ComputedVar computedVar = (ComputedVar)varDefOp.Var;
				if (child.HasChild0 && child.Child0.Op.OpType == OpType.Function)
				{
					var = computedVar;
					edmFunction = ((FunctionOp)child.Child0.Op).Function;
				}
				else
				{
					List<Node> newNodes = new List<Node>();
					FlattenComputedVar(computedVar, child, out newNodes, out var _);
					PlanCompiler.Assert(newNodes.Count == 1, "Flattening unnest var produced more than one Var.");
					n.Child0 = newNodes[0];
				}
			}
		}
		if (edmFunction != null)
		{
			PlanCompiler.Assert(var != null, "newUnnestVar must be initialized in the TVF case.");
		}
		else
		{
			if (!m_varInfoMap.TryGetVarInfo(op.Var, out var varInfo) || varInfo.Kind != VarInfoKind.CollectionVarInfo)
			{
				throw new InvalidOperationException(Strings.ADP_InternalProviderError(1006));
			}
			var = ((CollectionVarInfo)varInfo).NewVar;
		}
		Var var2 = op.Table.Columns[0];
		if (!TypeUtils.IsStructuredType(var2.Type))
		{
			PlanCompiler.Assert(edmFunction == null, "TVFs returning a collection of values of a non-structured type are not supported");
			if (TypeSemantics.IsEnumerationType(var2.Type) || TypeSemantics.IsStrongSpatialType(var2.Type))
			{
				UnnestOp unnestOp = m_command.CreateUnnestOp(var);
				m_varInfoMap.CreatePrimitiveTypeVarInfo(var2, unnestOp.Table.Columns[0]);
				n.Op = unnestOp;
			}
			else
			{
				n.Op = m_command.CreateUnnestOp(var, op.Table);
			}
		}
		else
		{
			TypeInfo typeInfo = m_typeInfo.GetTypeInfo(var2.Type);
			TableMD tableMetadata;
			if (edmFunction != null)
			{
				RowType tvfReturnType = TypeHelpers.GetTvfReturnType(edmFunction);
				PlanCompiler.Assert(Command.EqualTypes(tvfReturnType, var2.Type.EdmType), "Unexpected TVF return type (row type is expected).");
				tableMetadata = m_command.CreateFlatTableDefinition(tvfReturnType.Properties, GetTvfResultKeys(edmFunction), null);
			}
			else
			{
				tableMetadata = m_command.CreateFlatTableDefinition(typeInfo.FlattenedType);
			}
			Table table = m_command.CreateTableInstance(tableMetadata);
			n.Op = m_command.CreateUnnestOp(var, table);
			List<Var> newVars;
			if (edmFunction != null)
			{
				n = CreateTVFProjection(n, table.Columns, typeInfo, out newVars);
			}
			else
			{
				newVars = table.Columns;
			}
			m_varInfoMap.CreateStructuredVarInfo(var2, typeInfo.FlattenedType, newVars, typeInfo.FlattenedType.Properties.ToList());
		}
		return n;
	}

	private IEnumerable<EdmProperty> GetTvfResultKeys(EdmFunction tvf)
	{
		if (m_tvfResultKeys.TryGetValue(tvf, out var value))
		{
			return value;
		}
		return Enumerable.Empty<EdmProperty>();
	}

	protected override Node VisitSetOp(SetOp op, Node n)
	{
		VisitChildren(n);
		for (int i = 0; i < op.VarMap.Length; i++)
		{
			op.VarMap[i] = FlattenVarMap(op.VarMap[i], out var newComputedVars);
			if (newComputedVars != null)
			{
				n.Children[i] = FixupSetOpChild(n.Children[i], op.VarMap[i], newComputedVars);
			}
		}
		op.Outputs.Clear();
		foreach (Var key in op.VarMap[0].Keys)
		{
			op.Outputs.Set(key);
		}
		return n;
	}

	private Node FixupSetOpChild(Node setOpChild, VarMap varMap, List<ComputedVar> newComputedVars)
	{
		PlanCompiler.Assert(setOpChild != null, "null setOpChild?");
		PlanCompiler.Assert(varMap != null, "null varMap?");
		PlanCompiler.Assert(newComputedVars != null, "null newComputedVars?");
		VarVec varVec = m_command.CreateVarVec();
		foreach (KeyValuePair<Var, Var> item2 in varMap)
		{
			varVec.Set(item2.Value);
		}
		List<Node> list = new List<Node>();
		foreach (ComputedVar newComputedVar in newComputedVars)
		{
			VarDefOp op = m_command.CreateVarDefOp(newComputedVar);
			Node item = m_command.CreateNode(op, CreateNullConstantNode(newComputedVar.Type));
			list.Add(item);
		}
		Node arg = m_command.CreateNode(m_command.CreateVarDefListOp(), list);
		ProjectOp op2 = m_command.CreateProjectOp(varVec);
		return m_command.CreateNode(op2, setOpChild, arg);
	}

	private VarMap FlattenVarMap(VarMap varMap, out List<ComputedVar> newComputedVars)
	{
		newComputedVars = null;
		VarMap varMap2 = new VarMap();
		foreach (KeyValuePair<Var, Var> item in varMap)
		{
			if (!m_varInfoMap.TryGetVarInfo(item.Value, out var varInfo))
			{
				varMap2.Add(item.Key, item.Value);
				continue;
			}
			if (!m_varInfoMap.TryGetVarInfo(item.Key, out var varInfo2))
			{
				varInfo2 = FlattenSetOpVar((SetOpVar)item.Key);
			}
			if (varInfo2.Kind == VarInfoKind.CollectionVarInfo)
			{
				varMap2.Add(((CollectionVarInfo)varInfo2).NewVar, ((CollectionVarInfo)varInfo).NewVar);
				continue;
			}
			if (varInfo2.Kind == VarInfoKind.PrimitiveTypeVarInfo)
			{
				varMap2.Add(((PrimitiveTypeVarInfo)varInfo2).NewVar, ((PrimitiveTypeVarInfo)varInfo).NewVar);
				continue;
			}
			StructuredVarInfo structuredVarInfo = (StructuredVarInfo)varInfo2;
			StructuredVarInfo structuredVarInfo2 = (StructuredVarInfo)varInfo;
			foreach (EdmProperty field in structuredVarInfo.Fields)
			{
				PlanCompiler.Assert(structuredVarInfo.TryGetVar(field, out var v), "Could not find VarInfo for prop " + field.Name);
				if (!structuredVarInfo2.TryGetVar(field, out var v2))
				{
					v2 = m_command.CreateComputedVar(v.Type);
					if (newComputedVars == null)
					{
						newComputedVars = new List<ComputedVar>();
					}
					newComputedVars.Add((ComputedVar)v2);
				}
				varMap2.Add(v, v2);
			}
		}
		return varMap2;
	}

	private VarInfo FlattenSetOpVar(SetOpVar v)
	{
		if (TypeUtils.IsCollectionType(v.Type))
		{
			TypeUsage newType = GetNewType(v.Type);
			Var newVar = m_command.CreateSetOpVar(newType);
			return m_varInfoMap.CreateCollectionVarInfo(v, newVar);
		}
		if (TypeSemantics.IsEnumerationType(v.Type) || TypeSemantics.IsStrongSpatialType(v.Type))
		{
			TypeUsage newType2 = GetNewType(v.Type);
			Var newVar2 = m_command.CreateSetOpVar(newType2);
			return m_varInfoMap.CreatePrimitiveTypeVarInfo(v, newVar2);
		}
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(v.Type);
		PropertyRefList propertyRefList = m_varPropertyMap[v];
		List<Var> list = new List<Var>();
		List<EdmProperty> list2 = new List<EdmProperty>();
		bool flag = false;
		foreach (PropertyRef propertyRef in typeInfo.PropertyRefList)
		{
			if (propertyRefList.Contains(propertyRef))
			{
				EdmProperty newProperty = typeInfo.GetNewProperty(propertyRef);
				list2.Add(newProperty);
				SetOpVar item = m_command.CreateSetOpVar(Helper.GetModelTypeUsage(newProperty));
				list.Add(item);
				if (!flag && IsNullSentinelPropertyRef(propertyRef))
				{
					flag = true;
				}
			}
		}
		return m_varInfoMap.CreateStructuredVarInfo(v, typeInfo.FlattenedType, list, list2, flag);
	}

	public override Node Visit(SoftCastOp op, Node n)
	{
		TypeUsage type = n.Child0.Op.Type;
		TypeUsage type2 = op.Type;
		VisitChildren(n);
		TypeUsage newType = GetNewType(type2);
		if (TypeSemantics.IsRowType(type2))
		{
			PlanCompiler.Assert(n.Child0.Op.OpType == OpType.NewRecord, "Expected a record constructor here. Found " + n.Child0.Op.OpType.ToString() + " instead");
			TypeInfo typeInfo = m_typeInfo.GetTypeInfo(type);
			TypeInfo typeInfo2 = m_typeInfo.GetTypeInfo(op.Type);
			NewRecordOp newRecordOp = m_command.CreateNewRecordOp(newType);
			List<Node> list = new List<Node>();
			IEnumerator<EdmProperty> enumerator = newRecordOp.Properties.GetEnumerator();
			int num = newRecordOp.Properties.Count;
			enumerator.MoveNext();
			IEnumerator<Node> enumerator2 = n.Child0.Children.GetEnumerator();
			int num2 = n.Child0.Children.Count;
			enumerator2.MoveNext();
			while (num2 < num)
			{
				PlanCompiler.Assert(typeInfo2.HasNullSentinelProperty && !typeInfo.HasNullSentinelProperty, "NullSentinelProperty mismatch on input?");
				list.Add(CreateNullSentinelConstant());
				enumerator.MoveNext();
				num--;
			}
			while (num2 > num)
			{
				PlanCompiler.Assert(!typeInfo2.HasNullSentinelProperty && typeInfo.HasNullSentinelProperty, "NullSentinelProperty mismatch on output?");
				enumerator2.MoveNext();
				num2--;
			}
			do
			{
				EdmProperty current = enumerator.Current;
				Node item = BuildSoftCast(enumerator2.Current, Helper.GetModelTypeUsage(current));
				list.Add(item);
				enumerator.MoveNext();
			}
			while (enumerator2.MoveNext());
			return m_command.CreateNode(newRecordOp, list);
		}
		if (TypeSemantics.IsCollectionType(type2))
		{
			return BuildSoftCast(n.Child0, newType);
		}
		if (TypeSemantics.IsPrimitiveType(type2))
		{
			return n;
		}
		PlanCompiler.Assert(TypeSemantics.IsNominalType(type2) || TypeSemantics.IsReferenceType(type2), "Gasp! Not a nominal type or even a reference type");
		PlanCompiler.Assert(Command.EqualTypes(newType, n.Child0.Op.Type), "Types are not equal");
		return n.Child0;
	}

	public override Node Visit(CastOp op, Node n)
	{
		VisitChildren(n);
		if (TypeSemantics.IsEnumerationType(op.Type))
		{
			PlanCompiler.Assert(TypeSemantics.IsPrimitiveType(n.Child0.Op.Type), "Primitive type expected.");
			PrimitiveType underlyingEdmTypeForEnumType = Helper.GetUnderlyingEdmTypeForEnumType(op.Type.EdmType);
			return RewriteAsCastToUnderlyingType(underlyingEdmTypeForEnumType, op, n);
		}
		if (TypeSemantics.IsSpatialType(op.Type))
		{
			PlanCompiler.Assert(TypeSemantics.IsPrimitiveType(n.Child0.Op.Type, PrimitiveTypeKind.Geography) || TypeSemantics.IsPrimitiveType(n.Child0.Op.Type, PrimitiveTypeKind.Geometry), "Union spatial type expected.");
			PrimitiveType spatialNormalizedPrimitiveType = Helper.GetSpatialNormalizedPrimitiveType(op.Type.EdmType);
			return RewriteAsCastToUnderlyingType(spatialNormalizedPrimitiveType, op, n);
		}
		return n;
	}

	private Node RewriteAsCastToUnderlyingType(PrimitiveType underlyingType, CastOp op, Node n)
	{
		if (underlyingType.PrimitiveTypeKind == ((PrimitiveType)n.Child0.Op.Type.EdmType).PrimitiveTypeKind)
		{
			return n.Child0;
		}
		return m_command.CreateNode(m_command.CreateCastOp(TypeUsage.Create(underlyingType, op.Type.Facets)), n.Child0);
	}

	public override Node Visit(ConstantOp op, Node n)
	{
		PlanCompiler.Assert(n.Children.Count == 0, "Constant operations don't have children.");
		PlanCompiler.Assert(op.Value != null, "Value must not be null");
		if (TypeSemantics.IsEnumerationType(op.Type))
		{
			object value = (op.Value.GetType().IsEnum() ? Convert.ChangeType(op.Value, op.Value.GetType().GetEnumUnderlyingType(), CultureInfo.InvariantCulture) : op.Value);
			return m_command.CreateNode(m_command.CreateConstantOp(TypeHelpers.CreateEnumUnderlyingTypeUsage(op.Type), value));
		}
		if (TypeSemantics.IsStrongSpatialType(op.Type))
		{
			op.Type = TypeHelpers.CreateSpatialUnionTypeUsage(op.Type);
		}
		return n;
	}

	public override Node Visit(CaseOp op, Node n)
	{
		bool thenClauseIsNull;
		bool num = PlanCompilerUtil.IsRowTypeCaseOpWithNullability(op, n, out thenClauseIsNull);
		VisitChildren(n);
		if (num && TryRewriteCaseOp(n, thenClauseIsNull, out var rewrittenNode))
		{
			return rewrittenNode;
		}
		if (TypeUtils.IsCollectionType(op.Type) || TypeSemantics.IsEnumerationType(op.Type) || TypeSemantics.IsStrongSpatialType(op.Type))
		{
			TypeUsage newType = GetNewType(op.Type);
			n.Op = m_command.CreateCaseOp(newType);
			return n;
		}
		if (TypeUtils.IsStructuredType(op.Type))
		{
			PropertyRefList desiredProperties = m_nodePropertyMap[n];
			return FlattenCaseOp(n, m_typeInfo.GetTypeInfo(op.Type), desiredProperties);
		}
		return n;
	}

	private bool TryRewriteCaseOp(Node n, bool thenClauseIsNull, out Node rewrittenNode)
	{
		rewrittenNode = n;
		if (!m_typeInfo.GetTypeInfo(n.Op.Type).HasNullSentinelProperty)
		{
			return false;
		}
		Node node = (thenClauseIsNull ? n.Child2 : n.Child1);
		if (node.Op.OpType != OpType.NewRecord)
		{
			return false;
		}
		Node child = node.Child0;
		TypeUsage integerType = m_command.IntegerType;
		PlanCompiler.Assert(child.Op.Type.EdmEquals(integerType), "Column that is expected to be a null sentinel is not of Integer type.");
		CaseOp op = m_command.CreateCaseOp(integerType);
		List<Node> list = new List<Node>(3);
		list.Add(n.Child0);
		Node node2 = m_command.CreateNode(m_command.CreateNullOp(integerType));
		Node item = (thenClauseIsNull ? node2 : child);
		Node item2 = (thenClauseIsNull ? child : node2);
		list.Add(item);
		list.Add(item2);
		node.Child0 = m_command.CreateNode(op, list);
		rewrittenNode = node;
		return true;
	}

	private Node FlattenCaseOp(Node n, TypeInfo typeInfo, PropertyRefList desiredProperties)
	{
		List<EdmProperty> list = new List<EdmProperty>();
		List<Node> list2 = new List<Node>();
		foreach (PropertyRef propertyRef in typeInfo.PropertyRefList)
		{
			if (desiredProperties.Contains(propertyRef))
			{
				EdmProperty newProperty = typeInfo.GetNewProperty(propertyRef);
				List<Node> list3 = new List<Node>();
				int num;
				for (num = 0; num < n.Children.Count - 1; num++)
				{
					Node item = Copy(n.Children[num]);
					list3.Add(item);
					num++;
					Node item2 = BuildAccessorWithNulls(n.Children[num], newProperty);
					list3.Add(item2);
				}
				Node item3 = BuildAccessorWithNulls(n.Children[n.Children.Count - 1], newProperty);
				list3.Add(item3);
				Node item4 = m_command.CreateNode(m_command.CreateCaseOp(Helper.GetModelTypeUsage(newProperty)), list3);
				list.Add(newProperty);
				list2.Add(item4);
			}
		}
		NewRecordOp op = m_command.CreateNewRecordOp(typeInfo.FlattenedTypeUsage, list);
		return m_command.CreateNode(op, list2);
	}

	public override Node Visit(CollectOp op, Node n)
	{
		VisitChildren(n);
		n.Op = m_command.CreateCollectOp(GetNewType(op.Type));
		return n;
	}

	public override Node Visit(ComparisonOp op, Node n)
	{
		TypeUsage type = n.Child0.Op.Type;
		TypeUsage type2 = n.Child1.Op.Type;
		if (!TypeUtils.IsStructuredType(type))
		{
			return VisitScalarOpDefault(op, n);
		}
		VisitChildren(n);
		PlanCompiler.Assert(!TypeSemantics.IsComplexType(type) && !TypeSemantics.IsComplexType(type2), "complex type?");
		PlanCompiler.Assert(op.OpType == OpType.EQ || op.OpType == OpType.NE, "non-equality comparison of structured types?");
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(type);
		TypeInfo typeInfo2 = m_typeInfo.GetTypeInfo(type2);
		GetPropertyValues(typeInfo, OperationKind.Equality, n.Child0, ignoreMissingProperties: false, out var properties, out var values);
		GetPropertyValues(typeInfo2, OperationKind.Equality, n.Child1, ignoreMissingProperties: false, out var properties2, out var values2);
		PlanCompiler.Assert(properties.Count == properties2.Count && values.Count == values2.Count, "different shaped structured types?");
		Node node = null;
		for (int i = 0; i < values.Count; i++)
		{
			ComparisonOp op2 = m_command.CreateComparisonOp(op.OpType, op.UseDatabaseNullSemantics);
			Node node2 = m_command.CreateNode(op2, values[i], values2[i]);
			node = ((node != null) ? m_command.CreateNode(m_command.CreateConditionalOp(OpType.And), node, node2) : node2);
		}
		return node;
	}

	public override Node Visit(ConditionalOp op, Node n)
	{
		if (op.OpType != OpType.IsNull)
		{
			return VisitScalarOpDefault(op, n);
		}
		TypeUsage type = n.Child0.Op.Type;
		if (!TypeUtils.IsStructuredType(type))
		{
			return VisitScalarOpDefault(op, n);
		}
		VisitChildren(n);
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(type);
		List<EdmProperty> properties = null;
		List<Node> values = null;
		GetPropertyValues(typeInfo, OperationKind.IsNull, n.Child0, ignoreMissingProperties: false, out properties, out values);
		PlanCompiler.Assert(properties.Count == values.Count && properties.Count > 0, "No properties returned from GetPropertyValues(IsNull)?");
		Node node = null;
		foreach (Node item in values)
		{
			Node node2 = m_command.CreateNode(m_command.CreateConditionalOp(OpType.IsNull), item);
			node = ((node != null) ? m_command.CreateNode(m_command.CreateConditionalOp(OpType.And), node, node2) : node2);
		}
		return node;
	}

	public override Node Visit(ConstrainedSortOp op, Node n)
	{
		VisitChildren(n);
		List<System.Data.Entity.Core.Query.InternalTrees.SortKey> list = HandleSortKeys(op.Keys);
		if (list != op.Keys)
		{
			n.Op = m_command.CreateConstrainedSortOp(list, op.WithTies);
		}
		return n;
	}

	public override Node Visit(GetEntityRefOp op, Node n)
	{
		return FlattenGetKeyOp(op, n);
	}

	public override Node Visit(GetRefKeyOp op, Node n)
	{
		return FlattenGetKeyOp(op, n);
	}

	private Node FlattenGetKeyOp(ScalarOp op, Node n)
	{
		PlanCompiler.Assert(op.OpType == OpType.GetEntityRef || op.OpType == OpType.GetRefKey, "Expecting GetEntityRef or GetRefKey ops");
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(n.Child0.Op.Type);
		TypeInfo typeInfo2 = m_typeInfo.GetTypeInfo(op.Type);
		VisitChildren(n);
		List<EdmProperty> properties;
		List<Node> values;
		if (op.OpType == OpType.GetRefKey)
		{
			GetPropertyValues(typeInfo, OperationKind.GetKeys, n.Child0, ignoreMissingProperties: false, out properties, out values);
		}
		else
		{
			PlanCompiler.Assert(op.OpType == OpType.GetEntityRef, "Expected OpType.GetEntityRef: Found " + op.OpType);
			GetPropertyValues(typeInfo, OperationKind.GetIdentity, n.Child0, ignoreMissingProperties: false, out properties, out values);
		}
		if (typeInfo2.HasNullSentinelProperty && !typeInfo.HasNullSentinelProperty)
		{
			values.Insert(0, CreateNullSentinelConstant());
		}
		List<EdmProperty> list = new List<EdmProperty>(typeInfo2.FlattenedType.Properties);
		PlanCompiler.Assert(values.Count == list.Count, "fieldTypes.Count mismatch?");
		NewRecordOp op2 = m_command.CreateNewRecordOp(typeInfo2.FlattenedTypeUsage, list);
		return m_command.CreateNode(op2, values);
	}

	private Node VisitPropertyOp(Op op, Node n, PropertyRef propertyRef, bool throwIfMissing)
	{
		PlanCompiler.Assert(op.OpType == OpType.Property || op.OpType == OpType.RelProperty, "Unexpected optype: " + op.OpType);
		TypeUsage type = n.Child0.Op.Type;
		TypeUsage type2 = op.Type;
		VisitChildren(n);
		Node node = null;
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(type);
		if (TypeUtils.IsStructuredType(type2))
		{
			TypeInfo typeInfo2 = m_typeInfo.GetTypeInfo(type2);
			List<EdmProperty> list = new List<EdmProperty>();
			List<Node> list2 = new List<Node>();
			PropertyRefList propertyRefList = m_nodePropertyMap[n];
			foreach (PropertyRef propertyRef3 in typeInfo2.PropertyRefList)
			{
				if (!propertyRefList.Contains(propertyRef3))
				{
					continue;
				}
				PropertyRef propertyRef2 = propertyRef3.CreateNestedPropertyRef(propertyRef);
				if (typeInfo.TryGetNewProperty(propertyRef2, throwIfMissing, out var newProperty))
				{
					EdmProperty newProperty2 = typeInfo2.GetNewProperty(propertyRef3);
					Node node2 = BuildAccessor(n.Child0, newProperty);
					if (node2 != null)
					{
						list.Add(newProperty2);
						list2.Add(node2);
					}
				}
			}
			Op op2 = m_command.CreateNewRecordOp(typeInfo2.FlattenedTypeUsage, list);
			return m_command.CreateNode(op2, list2);
		}
		EdmProperty newProperty3 = typeInfo.GetNewProperty(propertyRef);
		return BuildAccessorWithNulls(n.Child0, newProperty3);
	}

	public override Node Visit(PropertyOp op, Node n)
	{
		return VisitPropertyOp(op, n, new SimplePropertyRef(op.PropertyInfo), throwIfMissing: true);
	}

	public override Node Visit(RelPropertyOp op, Node n)
	{
		return VisitPropertyOp(op, n, new RelPropertyRef(op.PropertyInfo), throwIfMissing: false);
	}

	public override Node Visit(RefOp op, Node n)
	{
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(n.Child0.Op.Type);
		TypeInfo typeInfo2 = m_typeInfo.GetTypeInfo(op.Type);
		VisitChildren(n);
		GetPropertyValues(typeInfo, OperationKind.All, n.Child0, ignoreMissingProperties: false, out var properties, out var values);
		List<EdmProperty> list = new List<EdmProperty>(typeInfo2.FlattenedType.Properties);
		if (typeInfo2.HasEntitySetIdProperty)
		{
			PlanCompiler.Assert(list[0] == typeInfo2.EntitySetIdProperty, "OutputField0 must be the entitySetId property");
			if (typeInfo.HasNullSentinelProperty && !typeInfo2.HasNullSentinelProperty)
			{
				PlanCompiler.Assert(list.Count == properties.Count, "Mismatched field count: Expected " + properties.Count + "; Got " + list.Count);
				RemoveNullSentinel(typeInfo, properties, values);
			}
			else
			{
				PlanCompiler.Assert(list.Count == properties.Count + 1, "Mismatched field count: Expected " + (properties.Count + 1) + "; Got " + list.Count);
			}
			int entitySetId = m_typeInfo.GetEntitySetId(op.EntitySet);
			values.Insert(0, m_command.CreateNode(m_command.CreateInternalConstantOp(Helper.GetModelTypeUsage(typeInfo2.EntitySetIdProperty), entitySetId)));
		}
		else
		{
			if (typeInfo.HasNullSentinelProperty && !typeInfo2.HasNullSentinelProperty)
			{
				RemoveNullSentinel(typeInfo, properties, values);
			}
			PlanCompiler.Assert(list.Count == properties.Count, "Mismatched field count: Expected " + properties.Count + "; Got " + list.Count);
		}
		NewRecordOp op2 = m_command.CreateNewRecordOp(typeInfo2.FlattenedTypeUsage, list);
		return m_command.CreateNode(op2, values);
	}

	private static void RemoveNullSentinel(TypeInfo inputTypeInfo, List<EdmProperty> inputFields, List<Node> inputFieldValues)
	{
		PlanCompiler.Assert(inputFields[0] == inputTypeInfo.NullSentinelProperty, "InputField0 must be the null sentinel property");
		inputFields.RemoveAt(0);
		inputFieldValues.RemoveAt(0);
	}

	public override Node Visit(VarRefOp op, Node n)
	{
		if (!m_varInfoMap.TryGetVarInfo(op.Var, out var varInfo))
		{
			PlanCompiler.Assert(!TypeUtils.IsStructuredType(op.Type), "No varInfo for a structured type var: Id = " + op.Var.Id + " Type = " + op.Type);
			return n;
		}
		if (varInfo.Kind == VarInfoKind.CollectionVarInfo)
		{
			n.Op = m_command.CreateVarRefOp(((CollectionVarInfo)varInfo).NewVar);
			return n;
		}
		if (varInfo.Kind == VarInfoKind.PrimitiveTypeVarInfo)
		{
			n.Op = m_command.CreateVarRefOp(((PrimitiveTypeVarInfo)varInfo).NewVar);
			return n;
		}
		StructuredVarInfo structuredVarInfo = (StructuredVarInfo)varInfo;
		NewRecordOp op2 = m_command.CreateNewRecordOp(structuredVarInfo.NewTypeUsage, structuredVarInfo.Fields);
		List<Node> list = new List<Node>();
		foreach (Var newVar in varInfo.NewVars)
		{
			VarRefOp op3 = m_command.CreateVarRefOp(newVar);
			list.Add(m_command.CreateNode(op3));
		}
		return m_command.CreateNode(op2, list);
	}

	public override Node Visit(NewEntityOp op, Node n)
	{
		return FlattenConstructor(op, n);
	}

	public override Node Visit(NewInstanceOp op, Node n)
	{
		return FlattenConstructor(op, n);
	}

	public override Node Visit(DiscriminatedNewEntityOp op, Node n)
	{
		return FlattenConstructor(op, n);
	}

	private Node NormalizeTypeDiscriminatorValues(DiscriminatedNewEntityOp op, Node discriminator)
	{
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.Type);
		CaseOp op2 = m_command.CreateCaseOp(typeInfo.RootType.TypeIdProperty.TypeUsage);
		List<Node> list = new List<Node>(op.DiscriminatorMap.TypeMap.Count * 2 - 1);
		for (int i = 0; i < op.DiscriminatorMap.TypeMap.Count; i++)
		{
			object key = op.DiscriminatorMap.TypeMap[i].Key;
			EntityType value = op.DiscriminatorMap.TypeMap[i].Value;
			TypeInfo typeInfo2 = m_typeInfo.GetTypeInfo(TypeUsage.Create(value));
			Node item = CreateTypeIdConstant(typeInfo2);
			if (i == op.DiscriminatorMap.TypeMap.Count - 1)
			{
				list.Add(item);
				continue;
			}
			ConstantBaseOp op3 = m_command.CreateConstantOp(Helper.GetModelTypeUsage(op.DiscriminatorMap.DiscriminatorProperty.TypeUsage), key);
			Node arg = m_command.CreateNode(op3);
			ComparisonOp op4 = m_command.CreateComparisonOp(OpType.EQ);
			Node item2 = m_command.CreateNode(op4, discriminator, arg);
			list.Add(item2);
			list.Add(item);
		}
		discriminator = m_command.CreateNode(op2, list);
		return discriminator;
	}

	public override Node Visit(NewRecordOp op, Node n)
	{
		return FlattenConstructor(op, n);
	}

	private Node GetEntitySetIdExpr(EdmProperty entitySetIdProperty, NewEntityBaseOp op)
	{
		EntitySet entitySet = op.EntitySet;
		if (entitySet != null)
		{
			int entitySetId = m_typeInfo.GetEntitySetId(entitySet);
			InternalConstantOp op2 = m_command.CreateInternalConstantOp(Helper.GetModelTypeUsage(entitySetIdProperty), entitySetId);
			return m_command.CreateNode(op2);
		}
		return CreateNullConstantNode(Helper.GetModelTypeUsage(entitySetIdProperty));
	}

	private Node FlattenConstructor(ScalarOp op, Node n)
	{
		PlanCompiler.Assert(op.OpType == OpType.NewInstance || op.OpType == OpType.NewRecord || op.OpType == OpType.DiscriminatedNewEntity || op.OpType == OpType.NewEntity, "unexpected op: " + op.OpType.ToString() + "?");
		VisitChildren(n);
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.Type);
		RowType flattenedType = typeInfo.FlattenedType;
		NewEntityBaseOp newEntityBaseOp = op as NewEntityBaseOp;
		IEnumerable enumerable = null;
		DiscriminatedNewEntityOp discriminatedNewEntityOp = null;
		if (op.OpType == OpType.NewRecord)
		{
			enumerable = ((NewRecordOp)op).Properties;
		}
		else if (op.OpType == OpType.DiscriminatedNewEntity)
		{
			discriminatedNewEntityOp = (DiscriminatedNewEntityOp)op;
			enumerable = discriminatedNewEntityOp.DiscriminatorMap.Properties;
		}
		else
		{
			enumerable = TypeHelpers.GetAllStructuralMembers(op.Type);
		}
		List<EdmProperty> list = new List<EdmProperty>();
		List<Node> list2 = new List<Node>();
		if (typeInfo.HasTypeIdProperty)
		{
			list.Add(typeInfo.TypeIdProperty);
			if (discriminatedNewEntityOp == null)
			{
				list2.Add(CreateTypeIdConstant(typeInfo));
			}
			else
			{
				Node node = n.Children[0];
				if (typeInfo.RootType.DiscriminatorMap == null)
				{
					node = NormalizeTypeDiscriminatorValues(discriminatedNewEntityOp, node);
				}
				list2.Add(node);
			}
		}
		if (typeInfo.HasEntitySetIdProperty)
		{
			list.Add(typeInfo.EntitySetIdProperty);
			PlanCompiler.Assert(newEntityBaseOp != null, "unexpected optype:" + op.OpType);
			Node entitySetIdExpr = GetEntitySetIdExpr(typeInfo.EntitySetIdProperty, newEntityBaseOp);
			list2.Add(entitySetIdExpr);
		}
		if (typeInfo.HasNullSentinelProperty)
		{
			list.Add(typeInfo.NullSentinelProperty);
			list2.Add(CreateNullSentinelConstant());
		}
		int num = ((discriminatedNewEntityOp != null) ? 1 : 0);
		foreach (EdmMember item in enumerable)
		{
			Node node2 = n.Children[num];
			if (TypeUtils.IsStructuredType(Helper.GetModelTypeUsage(item)))
			{
				RowType flattenedType2 = m_typeInfo.GetTypeInfo(Helper.GetModelTypeUsage(item)).FlattenedType;
				int num2 = typeInfo.RootType.GetNestedStructureOffset(new SimplePropertyRef(item));
				foreach (EdmProperty property in flattenedType2.Properties)
				{
					Node node3 = BuildAccessor(node2, property);
					if (node3 != null)
					{
						list.Add(flattenedType.Properties[num2]);
						list2.Add(node3);
					}
					num2++;
				}
			}
			else
			{
				PropertyRef propertyRef = new SimplePropertyRef(item);
				EdmProperty newProperty = typeInfo.GetNewProperty(propertyRef);
				list.Add(newProperty);
				list2.Add(node2);
			}
			num++;
		}
		if (newEntityBaseOp != null)
		{
			foreach (RelProperty relationshipProperty in newEntityBaseOp.RelationshipProperties)
			{
				Node input = n.Children[num];
				RowType flattenedType3 = m_typeInfo.GetTypeInfo(relationshipProperty.ToEnd.TypeUsage).FlattenedType;
				int num3 = typeInfo.RootType.GetNestedStructureOffset(new RelPropertyRef(relationshipProperty));
				foreach (EdmProperty property2 in flattenedType3.Properties)
				{
					Node node4 = BuildAccessor(input, property2);
					if (node4 != null)
					{
						list.Add(flattenedType.Properties[num3]);
						list2.Add(node4);
					}
					num3++;
				}
				num++;
			}
		}
		NewRecordOp op2 = m_command.CreateNewRecordOp(typeInfo.FlattenedTypeUsage, list);
		return m_command.CreateNode(op2, list2);
	}

	public override Node Visit(NullOp op, Node n)
	{
		if (!TypeUtils.IsStructuredType(op.Type))
		{
			if (TypeSemantics.IsEnumerationType(op.Type))
			{
				op.Type = TypeHelpers.CreateEnumUnderlyingTypeUsage(op.Type);
			}
			else if (TypeSemantics.IsStrongSpatialType(op.Type))
			{
				op.Type = TypeHelpers.CreateSpatialUnionTypeUsage(op.Type);
			}
			return n;
		}
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.Type);
		List<EdmProperty> list = new List<EdmProperty>();
		List<Node> list2 = new List<Node>();
		if (typeInfo.HasTypeIdProperty)
		{
			list.Add(typeInfo.TypeIdProperty);
			TypeUsage modelTypeUsage = Helper.GetModelTypeUsage(typeInfo.TypeIdProperty);
			list2.Add(CreateNullConstantNode(modelTypeUsage));
		}
		NewRecordOp op2 = new NewRecordOp(typeInfo.FlattenedTypeUsage, list);
		return m_command.CreateNode(op2, list2);
	}

	public override Node Visit(IsOfOp op, Node n)
	{
		VisitChildren(n);
		if (!TypeUtils.IsStructuredType(op.IsOfType))
		{
			return n;
		}
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.IsOfType);
		return CreateTypeComparisonOp(n.Child0, typeInfo, op.IsOfOnly);
	}

	public override Node Visit(TreatOp op, Node n)
	{
		VisitChildren(n);
		ScalarOp scalarOp = (ScalarOp)n.Child0.Op;
		if (op.IsFakeTreat || TypeSemantics.IsStructurallyEqual(scalarOp.Type, op.Type) || TypeSemantics.IsSubTypeOf(scalarOp.Type, op.Type))
		{
			return n.Child0;
		}
		if (!TypeUtils.IsStructuredType(op.Type))
		{
			return n;
		}
		TypeInfo typeInfo = m_typeInfo.GetTypeInfo(op.Type);
		Node arg = CreateTypeComparisonOp(n.Child0, typeInfo, isExact: false);
		CaseOp caseOp = m_command.CreateCaseOp(typeInfo.FlattenedTypeUsage);
		Node n2 = m_command.CreateNode(caseOp, arg, n.Child0, CreateNullConstantNode(caseOp.Type));
		PropertyRefList desiredProperties = m_nodePropertyMap[n];
		return FlattenCaseOp(n2, typeInfo, desiredProperties);
	}

	private Node CreateTypeComparisonOp(Node input, TypeInfo typeInfo, bool isExact)
	{
		Node node = BuildTypeIdAccessor(input, typeInfo);
		Node node2 = null;
		if (isExact)
		{
			return CreateTypeEqualsOp(typeInfo, node);
		}
		if (typeInfo.RootType.DiscriminatorMap != null)
		{
			return CreateDisjunctiveTypeComparisonOp(typeInfo, node);
		}
		Node arg = CreateTypeIdConstantForPrefixMatch(typeInfo);
		LikeOp op = m_command.CreateLikeOp();
		return m_command.CreateNode(op, node, arg, CreateNullConstantNode(DefaultTypeIdType));
	}

	private Node CreateDisjunctiveTypeComparisonOp(TypeInfo typeInfo, Node typeIdProperty)
	{
		PlanCompiler.Assert(typeInfo.RootType.DiscriminatorMap != null, "should be used only for DiscriminatorMap type checks");
		IEnumerable<TypeInfo> enumerable = from t in typeInfo.GetTypeHierarchy()
			where !t.Type.EdmType.Abstract
			select t;
		Node node = null;
		foreach (TypeInfo item in enumerable)
		{
			Node node2 = CreateTypeEqualsOp(item, typeIdProperty);
			node = ((node != null) ? m_command.CreateNode(m_command.CreateConditionalOp(OpType.Or), node, node2) : node2);
		}
		if (node == null)
		{
			node = m_command.CreateNode(m_command.CreateFalseOp());
		}
		return node;
	}

	private Node CreateTypeEqualsOp(TypeInfo typeInfo, Node typeIdProperty)
	{
		Node arg = CreateTypeIdConstant(typeInfo);
		ComparisonOp op = m_command.CreateComparisonOp(OpType.EQ);
		return m_command.CreateNode(op, typeIdProperty, arg);
	}
}
