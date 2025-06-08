using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Mapping.ViewGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Data.Entity.Resources;
using System.Runtime.CompilerServices;

namespace System.Data.Entity.Core.Query.PlanCompiler;

internal class PreProcessor : SubqueryTrackingVisitor
{
	private class NavigationPropertyOpInfo
	{
		private Node _node;

		private readonly Node _root;

		private readonly Command _command;

		private readonly int _hashCode;

		public NavigationPropertyOpInfo(Node node, Node root, Command command)
		{
			_node = node;
			_root = root;
			_command = command;
			_hashCode = (((((_root != null) ? RuntimeHelpers.GetHashCode(_root) : 0) * 397) ^ RuntimeHelpers.GetHashCode(GetProperty(_node))) * 397) ^ _node.GetNodeInfo(_command).HashValue;
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public override bool Equals(object obj)
		{
			if (obj is NavigationPropertyOpInfo navigationPropertyOpInfo && _root != null && _root == navigationPropertyOpInfo._root && GetProperty(_node) == GetProperty(navigationPropertyOpInfo._node))
			{
				return _node.IsEquivalent(navigationPropertyOpInfo._node);
			}
			return false;
		}

		public void Seal()
		{
			_node = OpCopier.Copy(_command, _node);
		}

		private static EdmMember GetProperty(Node node)
		{
			return ((PropertyOp)node.Op).PropertyInfo;
		}
	}

	private readonly Stack<EntitySet> m_entityTypeScopes = new Stack<EntitySet>();

	private readonly HashSet<EntityContainer> m_referencedEntityContainers = new HashSet<EntityContainer>();

	private readonly HashSet<EntitySet> m_referencedEntitySets = new HashSet<EntitySet>();

	private readonly HashSet<TypeUsage> m_referencedTypes = new HashSet<TypeUsage>();

	private readonly HashSet<EntityType> m_freeFloatingEntityConstructorTypes = new HashSet<EntityType>();

	private readonly HashSet<string> m_typesNeedingNullSentinel = new HashSet<string>();

	private readonly Dictionary<EdmFunction, EdmProperty[]> m_tvfResultKeys = new Dictionary<EdmFunction, EdmProperty[]>();

	private readonly RelPropertyHelper m_relPropertyHelper;

	private bool m_suppressDiscriminatorMaps;

	private readonly Dictionary<EntitySetBase, DiscriminatorMapInfo> m_discriminatorMaps = new Dictionary<EntitySetBase, DiscriminatorMapInfo>();

	private readonly Dictionary<NavigationPropertyOpInfo, Node> _navigationPropertyOpRewrites = new Dictionary<NavigationPropertyOpInfo, Node>();

	private PreProcessor(PlanCompiler planCompilerState)
		: base(planCompilerState)
	{
		m_relPropertyHelper = new RelPropertyHelper(base.m_command.MetadataWorkspace, base.m_command.ReferencedRelProperties);
	}

	internal static void Process(PlanCompiler planCompilerState, out StructuredTypeInfo typeInfo, out Dictionary<EdmFunction, EdmProperty[]> tvfResultKeys)
	{
		PreProcessor preProcessor = new PreProcessor(planCompilerState);
		preProcessor.Process(out tvfResultKeys);
		StructuredTypeInfo.Process(planCompilerState.Command, preProcessor.m_referencedTypes, preProcessor.m_referencedEntitySets, preProcessor.m_freeFloatingEntityConstructorTypes, preProcessor.m_suppressDiscriminatorMaps ? null : preProcessor.m_discriminatorMaps, preProcessor.m_relPropertyHelper, preProcessor.m_typesNeedingNullSentinel, out typeInfo);
	}

	internal void Process(out Dictionary<EdmFunction, EdmProperty[]> tvfResultKeys)
	{
		base.m_command.Root = VisitNode(base.m_command.Root);
		foreach (Var var in base.m_command.Vars)
		{
			AddTypeReference(var.Type);
		}
		if (m_referencedTypes.Count > 0)
		{
			m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.NTE);
			((PhysicalProjectOp)base.m_command.Root.Op).ColumnMap.Accept(StructuredTypeNullabilityAnalyzer.Instance, m_typesNeedingNullSentinel);
		}
		tvfResultKeys = m_tvfResultKeys;
	}

	private void AddEntitySetReference(EntitySet entitySet)
	{
		m_referencedEntitySets.Add(entitySet);
		if (!m_referencedEntityContainers.Contains(entitySet.EntityContainer))
		{
			m_referencedEntityContainers.Add(entitySet.EntityContainer);
		}
	}

	private void AddTypeReference(TypeUsage type)
	{
		if (TypeUtils.IsStructuredType(type) || TypeUtils.IsCollectionType(type) || TypeUtils.IsEnumerationType(type))
		{
			m_referencedTypes.Add(type);
		}
	}

	private List<RelationshipSet> GetRelationshipSets(RelationshipType relType)
	{
		List<RelationshipSet> list = new List<RelationshipSet>();
		foreach (EntityContainer referencedEntityContainer in m_referencedEntityContainers)
		{
			foreach (EntitySetBase baseEntitySet in referencedEntityContainer.BaseEntitySets)
			{
				if (baseEntitySet is RelationshipSet relationshipSet && relationshipSet.ElementType.Equals(relType))
				{
					list.Add(relationshipSet);
				}
			}
		}
		return list;
	}

	private List<EntitySet> GetEntitySets(TypeUsage entityType)
	{
		List<EntitySet> list = new List<EntitySet>();
		foreach (EntityContainer referencedEntityContainer in m_referencedEntityContainers)
		{
			foreach (EntitySetBase baseEntitySet in referencedEntityContainer.BaseEntitySets)
			{
				if (baseEntitySet is EntitySet entitySet && (entitySet.ElementType.Equals(entityType.EdmType) || TypeSemantics.IsSubTypeOf(entityType.EdmType, entitySet.ElementType) || TypeSemantics.IsSubTypeOf(entitySet.ElementType, entityType.EdmType)))
				{
					list.Add(entitySet);
				}
			}
		}
		return list;
	}

	private Node ExpandView(ScanTableOp scanTableOp, ref IsOfOp typeFilter)
	{
		EntitySetBase extent = scanTableOp.Table.TableMetadata.Extent;
		PlanCompiler.Assert(extent != null, "The target of a ScanTableOp must reference an EntitySet to be used with ExpandView");
		PlanCompiler.Assert(extent.EntityContainer.DataSpace == DataSpace.CSpace, "Store entity sets cannot have Query Mapping Views and should not be used with ExpandView");
		if (typeFilter != null && !typeFilter.IsOfOnly && TypeSemantics.IsSubTypeOf(extent.ElementType, typeFilter.IsOfType.EdmType))
		{
			typeFilter = null;
		}
		GeneratedView generatedView = null;
		EntityTypeBase entityTypeBase = scanTableOp.Table.TableMetadata.Extent.ElementType;
		bool includeSubtypes = true;
		if (typeFilter != null)
		{
			entityTypeBase = (EntityTypeBase)typeFilter.IsOfType.EdmType;
			includeSubtypes = !typeFilter.IsOfOnly;
			if (base.m_command.MetadataWorkspace.TryGetGeneratedViewOfType(extent, entityTypeBase, includeSubtypes, out generatedView))
			{
				typeFilter = null;
			}
		}
		if (generatedView == null)
		{
			generatedView = base.m_command.MetadataWorkspace.GetGeneratedView(extent);
		}
		PlanCompiler.Assert(generatedView != null, Strings.ADP_NoQueryMappingView(extent.EntityContainer.Name, extent.Name));
		Node internalTree = generatedView.GetInternalTree(base.m_command);
		DetermineDiscriminatorMapUsage(internalTree, extent, entityTypeBase, includeSubtypes);
		ScanViewOp op = base.m_command.CreateScanViewOp(scanTableOp.Table);
		return base.m_command.CreateNode(op, internalTree);
	}

	private void DetermineDiscriminatorMapUsage(Node viewNode, EntitySetBase entitySet, EntityTypeBase rootEntityType, bool includeSubtypes)
	{
		ExplicitDiscriminatorMap discriminatorMap = null;
		if (viewNode.Op.OpType == OpType.Project && viewNode.Child1.Child0.Child0.Op is DiscriminatedNewEntityOp discriminatedNewEntityOp)
		{
			discriminatorMap = discriminatedNewEntityOp.DiscriminatorMap;
		}
		if (!m_discriminatorMaps.TryGetValue(entitySet, out var value))
		{
			if (rootEntityType == null)
			{
				rootEntityType = entitySet.ElementType;
				includeSubtypes = true;
			}
			value = new DiscriminatorMapInfo(rootEntityType, includeSubtypes, discriminatorMap);
			m_discriminatorMaps.Add(entitySet, value);
		}
		else
		{
			value.Merge(rootEntityType, includeSubtypes, discriminatorMap);
		}
	}

	private Node RewriteNavigateOp(Node navigateOpNode, NavigateOp navigateOp, out Var outputVar)
	{
		outputVar = null;
		if (!Helper.IsAssociationType(navigateOp.Relationship))
		{
			throw new NotSupportedException(Strings.Cqt_RelNav_NoCompositions);
		}
		if (navigateOpNode.Child0.Op.OpType == OpType.GetEntityRef && (navigateOp.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne || navigateOp.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.One))
		{
			PlanCompiler.Assert(base.m_command.IsRelPropertyReferenced(navigateOp.RelProperty), "Unreferenced rel property? " + navigateOp.RelProperty);
			Op op = base.m_command.CreateRelPropertyOp(navigateOp.RelProperty);
			return base.m_command.CreateNode(op, navigateOpNode.Child0.Child0);
		}
		List<RelationshipSet> relationshipSets = GetRelationshipSets(navigateOp.Relationship);
		if (relationshipSets.Count == 0)
		{
			if (navigateOp.ToEnd.RelationshipMultiplicity != RelationshipMultiplicity.Many)
			{
				return base.m_command.CreateNode(base.m_command.CreateNullOp(navigateOp.Type));
			}
			return base.m_command.CreateNode(base.m_command.CreateNewMultisetOp(navigateOp.Type));
		}
		List<Node> list = new List<Node>();
		List<Var> list2 = new List<Var>();
		foreach (RelationshipSet item3 in relationshipSets)
		{
			TableMD tableMetadata = Command.CreateTableDefinition(item3);
			ScanTableOp scanTableOp = base.m_command.CreateScanTableOp(tableMetadata);
			Node item = base.m_command.CreateNode(scanTableOp);
			Var item2 = scanTableOp.Table.Columns[0];
			list2.Add(item2);
			list.Add(item);
		}
		Node resultNode = null;
		base.m_command.BuildUnionAllLadder((IList<Node>)list, (IList<Var>)list2, out resultNode, out Var resultVar);
		Node computedExpression = base.m_command.CreateNode(base.m_command.CreatePropertyOp(navigateOp.ToEnd), base.m_command.CreateNode(base.m_command.CreateVarRefOp(resultVar)));
		Node arg = base.m_command.CreateNode(base.m_command.CreatePropertyOp(navigateOp.FromEnd), base.m_command.CreateNode(base.m_command.CreateVarRefOp(resultVar)));
		Node arg2 = base.m_command.BuildComparison(OpType.EQ, navigateOpNode.Child0, arg, useDatabaseNullSemantics: true);
		Node input = base.m_command.CreateNode(base.m_command.CreateFilterOp(), resultNode, arg2);
		Var projectVar;
		Node node = base.m_command.BuildProject(input, computedExpression, out projectVar);
		Node result;
		if (navigateOp.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
		{
			result = base.m_command.BuildCollect(node, projectVar);
		}
		else
		{
			result = node;
			outputVar = projectVar;
		}
		return result;
	}

	private Node BuildOfTypeTable(EntitySetBase entitySet, TypeUsage ofType, out Var resultVar)
	{
		TableMD tableMetadata = Command.CreateTableDefinition(entitySet);
		ScanTableOp scanTableOp = base.m_command.CreateScanTableOp(tableMetadata);
		Node node = base.m_command.CreateNode(scanTableOp);
		Var var = scanTableOp.Table.Columns[0];
		Node resultNode;
		if (ofType != null && !entitySet.ElementType.EdmEquals(ofType.EdmType))
		{
			base.m_command.BuildOfTypeTree(node, var, ofType, includeSubtypes: true, out resultNode, out resultVar);
		}
		else
		{
			resultNode = node;
			resultVar = var;
		}
		return resultNode;
	}

	private Node RewriteDerefOp(Node derefOpNode, DerefOp derefOp, out Var outputVar)
	{
		TypeUsage type = derefOp.Type;
		List<EntitySet> entitySets = GetEntitySets(type);
		if (entitySets.Count == 0)
		{
			outputVar = null;
			return base.m_command.CreateNode(base.m_command.CreateNullOp(type));
		}
		List<Node> list = new List<Node>();
		List<Var> list2 = new List<Var>();
		foreach (EntitySet item2 in entitySets)
		{
			Var resultVar;
			Node item = BuildOfTypeTable(item2, type, out resultVar);
			list.Add(item);
			list2.Add(resultVar);
		}
		base.m_command.BuildUnionAllLadder((IList<Node>)list, (IList<Var>)list2, out Node resultNode, out Var resultVar2);
		Node arg = base.m_command.CreateNode(base.m_command.CreateGetEntityRefOp(derefOpNode.Child0.Op.Type), base.m_command.CreateNode(base.m_command.CreateVarRefOp(resultVar2)));
		Node arg2 = base.m_command.BuildComparison(OpType.EQ, derefOpNode.Child0, arg, useDatabaseNullSemantics: true);
		Node result = base.m_command.CreateNode(base.m_command.CreateFilterOp(), resultNode, arg2);
		outputVar = resultVar2;
		return result;
	}

	private static EntitySetBase FindTargetEntitySet(RelationshipSet relationshipSet, RelationshipEndMember targetEnd)
	{
		EntitySetBase entitySetBase = null;
		AssociationSet obj = (AssociationSet)relationshipSet;
		entitySetBase = null;
		foreach (AssociationSetEnd associationSetEnd in obj.AssociationSetEnds)
		{
			if (associationSetEnd.CorrespondingAssociationEndMember.EdmEquals(targetEnd))
			{
				entitySetBase = associationSetEnd.EntitySet;
				break;
			}
		}
		PlanCompiler.Assert(entitySetBase != null, "Could not find entity set for relationship set " + relationshipSet?.ToString() + ";association end " + targetEnd);
		return entitySetBase;
	}

	private Node BuildJoinForNavProperty(RelationshipSet relSet, RelationshipEndMember end, out Var rsVar, out Var esVar)
	{
		EntitySetBase entitySet = FindTargetEntitySet(relSet, end);
		Node arg = BuildOfTypeTable(relSet, null, out rsVar);
		Node arg2 = BuildOfTypeTable(entitySet, TypeHelpers.GetElementTypeUsage(end.TypeUsage), out esVar);
		Node arg3 = base.m_command.BuildComparison(OpType.EQ, base.m_command.CreateNode(base.m_command.CreateGetEntityRefOp(end.TypeUsage), base.m_command.CreateNode(base.m_command.CreateVarRefOp(esVar))), base.m_command.CreateNode(base.m_command.CreatePropertyOp(end), base.m_command.CreateNode(base.m_command.CreateVarRefOp(rsVar))), useDatabaseNullSemantics: true);
		return base.m_command.CreateNode(base.m_command.CreateInnerJoinOp(), arg, arg2, arg3);
	}

	private Node RewriteManyToOneNavigationProperty(RelProperty relProperty, Node sourceEntityNode, TypeUsage resultType)
	{
		RelPropertyOp op = base.m_command.CreateRelPropertyOp(relProperty);
		Node arg = base.m_command.CreateNode(op, sourceEntityNode);
		DerefOp op2 = base.m_command.CreateDerefOp(resultType);
		return base.m_command.CreateNode(op2, arg);
	}

	private Node RewriteOneToManyNavigationProperty(RelProperty relProperty, List<RelationshipSet> relationshipSets, Node sourceRefNode)
	{
		Var outputVar;
		Node relOpNode = RewriteFromOneNavigationProperty(relProperty, relationshipSets, sourceRefNode, out outputVar);
		return base.m_command.BuildCollect(relOpNode, outputVar);
	}

	private Node RewriteOneToOneNavigationProperty(RelProperty relProperty, List<RelationshipSet> relationshipSets, Node sourceRefNode)
	{
		Node n = RewriteFromOneNavigationProperty(relProperty, relationshipSets, sourceRefNode, out var outputVar);
		n = VisitNode(n);
		return AddSubqueryToParentRelOp(outputVar, n);
	}

	private Node RewriteFromOneNavigationProperty(RelProperty relProperty, List<RelationshipSet> relationshipSets, Node sourceRefNode, out Var outputVar)
	{
		PlanCompiler.Assert(relationshipSets.Count > 0, "expected at least one relationship set here");
		PlanCompiler.Assert(relProperty.FromEnd.RelationshipMultiplicity != RelationshipMultiplicity.Many, "Expected source end multiplicity to be one. Found 'Many' instead " + relProperty);
		TypeUsage elementTypeUsage = TypeHelpers.GetElementTypeUsage(relProperty.ToEnd.TypeUsage);
		List<Node> list = new List<Node>(relationshipSets.Count);
		List<Var> list2 = new List<Var>(relationshipSets.Count);
		foreach (RelationshipSet relationshipSet in relationshipSets)
		{
			EntitySetBase entitySet = FindTargetEntitySet(relationshipSet, relProperty.ToEnd);
			Var resultVar;
			Node item = BuildOfTypeTable(entitySet, elementTypeUsage, out resultVar);
			list.Add(item);
			list2.Add(resultVar);
		}
		base.m_command.BuildUnionAllLadder((IList<Node>)list, (IList<Var>)list2, out Node resultNode, out outputVar);
		RelProperty relProperty2 = new RelProperty(relProperty.Relationship, relProperty.ToEnd, relProperty.FromEnd);
		PlanCompiler.Assert(base.m_command.IsRelPropertyReferenced(relProperty2), "Unreferenced rel property? " + relProperty2);
		Node arg = base.m_command.CreateNode(base.m_command.CreateRelPropertyOp(relProperty2), base.m_command.CreateNode(base.m_command.CreateVarRefOp(outputVar)));
		Node arg2 = base.m_command.BuildComparison(OpType.EQ, sourceRefNode, arg, useDatabaseNullSemantics: true);
		return base.m_command.CreateNode(base.m_command.CreateFilterOp(), resultNode, arg2);
	}

	private Node RewriteManyToManyNavigationProperty(RelProperty relProperty, List<RelationshipSet> relationshipSets, Node sourceRefNode)
	{
		PlanCompiler.Assert(relationshipSets.Count > 0, "expected at least one relationship set here");
		PlanCompiler.Assert(relProperty.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many && relProperty.FromEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many, "Expected target end multiplicity to be 'many'. Found " + relProperty?.ToString() + "; multiplicity = " + relProperty.ToEnd.RelationshipMultiplicity);
		List<Node> list = new List<Node>(relationshipSets.Count);
		List<Var> list2 = new List<Var>(relationshipSets.Count * 2);
		foreach (RelationshipSet relationshipSet in relationshipSets)
		{
			Var rsVar;
			Var esVar;
			Node item = BuildJoinForNavProperty(relationshipSet, relProperty.ToEnd, out rsVar, out esVar);
			list.Add(item);
			list2.Add(rsVar);
			list2.Add(esVar);
		}
		base.m_command.BuildUnionAllLadder((IList<Node>)list, (IList<Var>)list2, out Node resultNode, out IList<Var> resultVars);
		Node arg = base.m_command.CreateNode(base.m_command.CreatePropertyOp(relProperty.FromEnd), base.m_command.CreateNode(base.m_command.CreateVarRefOp(resultVars[0])));
		Node arg2 = base.m_command.BuildComparison(OpType.EQ, sourceRefNode, arg, useDatabaseNullSemantics: true);
		Node inputNode = base.m_command.CreateNode(base.m_command.CreateFilterOp(), resultNode, arg2);
		Node relOpNode = base.m_command.BuildProject(inputNode, new Var[1] { resultVars[1] }, new Node[0]);
		return base.m_command.BuildCollect(relOpNode, resultVars[1]);
	}

	private Node RewriteNavigationProperty(NavigationProperty navProperty, Node sourceEntityNode, TypeUsage resultType)
	{
		RelProperty relProperty = new RelProperty(navProperty.RelationshipType, navProperty.FromEndMember, navProperty.ToEndMember);
		PlanCompiler.Assert(base.m_command.IsRelPropertyReferenced(relProperty) || relProperty.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many, "Unreferenced rel property? " + relProperty);
		if (relProperty.FromEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many && relProperty.ToEnd.RelationshipMultiplicity != RelationshipMultiplicity.Many)
		{
			return RewriteManyToOneNavigationProperty(relProperty, sourceEntityNode, resultType);
		}
		List<RelationshipSet> relationshipSets = GetRelationshipSets(relProperty.Relationship);
		if (relationshipSets.Count == 0)
		{
			if (relProperty.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
			{
				return base.m_command.CreateNode(base.m_command.CreateNewMultisetOp(resultType));
			}
			return base.m_command.CreateNode(base.m_command.CreateNullOp(resultType));
		}
		Node sourceRefNode = base.m_command.CreateNode(base.m_command.CreateGetEntityRefOp(relProperty.FromEnd.TypeUsage), sourceEntityNode);
		if (relProperty.ToEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
		{
			if (relProperty.FromEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
			{
				return RewriteManyToManyNavigationProperty(relProperty, relationshipSets, sourceRefNode);
			}
			return RewriteOneToManyNavigationProperty(relProperty, relationshipSets, sourceRefNode);
		}
		return RewriteOneToOneNavigationProperty(relProperty, relationshipSets, sourceRefNode);
	}

	protected override Node VisitScalarOpDefault(ScalarOp op, Node n)
	{
		VisitChildren(n);
		AddTypeReference(op.Type);
		return n;
	}

	public override Node Visit(DerefOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		Node n2 = RewriteDerefOp(n, op, out var outputVar);
		n2 = VisitNode(n2);
		if (outputVar != null)
		{
			n2 = AddSubqueryToParentRelOp(outputVar, n2);
		}
		return n2;
	}

	public override Node Visit(ElementOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		Node child = n.Child0;
		ProjectOp obj = (ProjectOp)child.Op;
		PlanCompiler.Assert(obj.Outputs.Count == 1, "input to ElementOp has more than one output var?");
		Var first = obj.Outputs.First;
		return AddSubqueryToParentRelOp(first, child);
	}

	public override Node Visit(ExistsOp op, Node n)
	{
		m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.Normalization);
		return base.Visit(op, n);
	}

	public override Node Visit(FunctionOp op, Node n)
	{
		if (op.Function.IsFunctionImport)
		{
			PlanCompiler.Assert(op.Function.IsComposableAttribute, "Cannot process a non-composable function inside query tree composition.");
			FunctionImportMapping targetFunctionMapping = null;
			if (!base.m_command.MetadataWorkspace.TryGetFunctionImportMapping(op.Function, out targetFunctionMapping))
			{
				throw new MetadataException(Strings.EntityClient_UnmappedFunctionImport(op.Function.FullName));
			}
			PlanCompiler.Assert(targetFunctionMapping is FunctionImportMappingComposable, "Composable function import must have corresponding mapping.");
			FunctionImportMappingComposable functionImportMappingComposable = (FunctionImportMappingComposable)targetFunctionMapping;
			VisitChildren(n);
			Node internalTree = functionImportMappingComposable.GetInternalTree(base.m_command, n.Children);
			if (op.Function.EntitySet != null)
			{
				m_entityTypeScopes.Push(op.Function.EntitySet);
				AddEntitySetReference(op.Function.EntitySet);
				PlanCompiler.Assert(functionImportMappingComposable.TvfKeys != null && functionImportMappingComposable.TvfKeys.Length != 0, "Function imports returning entities must have inferred keys.");
				if (!m_tvfResultKeys.ContainsKey(functionImportMappingComposable.TargetFunction))
				{
					m_tvfResultKeys.Add(functionImportMappingComposable.TargetFunction, functionImportMappingComposable.TvfKeys);
				}
			}
			internalTree = VisitNode(internalTree);
			if (op.Function.EntitySet != null)
			{
				PlanCompiler.Assert(m_entityTypeScopes.Pop() == op.Function.EntitySet, "m_entityTypeScopes stack is broken");
			}
			return internalTree;
		}
		PlanCompiler.Assert(op.Function.EntitySet == null, "Entity type scope is not supported on functions that aren't mapped.");
		if (TypeSemantics.IsCollectionType(op.Type) || PlanCompilerUtil.IsCollectionAggregateFunction(op, n))
		{
			m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.NestPullup);
			m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.Normalization);
		}
		return base.Visit(op, n);
	}

	public override Node Visit(CaseOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		if (PlanCompilerUtil.IsRowTypeCaseOpWithNullability(op, n, out var _))
		{
			m_typesNeedingNullSentinel.Add(op.Type.EdmType.Identity);
		}
		return n;
	}

	public override Node Visit(ConditionalOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		ProcessConditionalOp(op, n);
		return n;
	}

	private void ProcessConditionalOp(ConditionalOp op, Node n)
	{
		if ((op.OpType == OpType.IsNull && TypeSemantics.IsRowType(n.Child0.Op.Type)) || TypeSemantics.IsComplexType(n.Child0.Op.Type))
		{
			StructuredTypeNullabilityAnalyzer.MarkAsNeedingNullSentinel(m_typesNeedingNullSentinel, n.Child0.Op.Type);
		}
	}

	private static void ValidateNavPropertyOp(PropertyOp op)
	{
		NavigationProperty navigationProperty = (NavigationProperty)op.PropertyInfo;
		TypeUsage typeUsage = navigationProperty.ToEndMember.TypeUsage;
		if (TypeSemantics.IsReferenceType(typeUsage))
		{
			typeUsage = TypeHelpers.GetElementTypeUsage(typeUsage);
		}
		if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
		{
			typeUsage = TypeUsage.Create(typeUsage.EdmType.GetCollectionType());
		}
		if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(typeUsage, op.Type))
		{
			throw new MetadataException(Strings.EntityClient_IncompatibleNavigationPropertyResult(navigationProperty.DeclaringType.FullName, navigationProperty.Name));
		}
	}

	private Node VisitNavPropertyOp(PropertyOp op, Node n)
	{
		ValidateNavPropertyOp(op);
		if (!IsNavigationPropertyOverVarRef(n.Child0))
		{
			VisitScalarOpDefault(op, n);
		}
		NavigationPropertyOpInfo navigationPropertyOpInfo = new NavigationPropertyOpInfo(n, FindRelOpAncestor(), base.m_command);
		if (_navigationPropertyOpRewrites.TryGetValue(navigationPropertyOpInfo, out var value))
		{
			return OpCopier.Copy(base.m_command, value);
		}
		navigationPropertyOpInfo.Seal();
		value = RewriteNavigationProperty((NavigationProperty)op.PropertyInfo, n.Child0, op.Type);
		value = VisitNode(value);
		_navigationPropertyOpRewrites.Add(navigationPropertyOpInfo, value);
		return value;
	}

	private static bool IsNavigationPropertyOverVarRef(Node n)
	{
		if (n.Op.OpType != OpType.Property || !Helper.IsNavigationProperty(((PropertyOp)n.Op).PropertyInfo))
		{
			return false;
		}
		Node child = n.Child0;
		if (child.Op.OpType == OpType.SoftCast)
		{
			child = child.Child0;
		}
		return child.Op.OpType == OpType.VarRef;
	}

	public override Node Visit(PropertyOp op, Node n)
	{
		if (Helper.IsNavigationProperty(op.PropertyInfo))
		{
			return VisitNavPropertyOp(op, n);
		}
		return VisitScalarOpDefault(op, n);
	}

	public override Node Visit(RefOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		AddEntitySetReference(op.EntitySet);
		return n;
	}

	public override Node Visit(TreatOp op, Node n)
	{
		n = base.Visit(op, n);
		if (CanRewriteTypeTest(op.Type.EdmType, n.Child0.Op.Type.EdmType))
		{
			return n.Child0;
		}
		return n;
	}

	public override Node Visit(IsOfOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		AddTypeReference(op.IsOfType);
		if (CanRewriteTypeTest(op.IsOfType.EdmType, n.Child0.Op.Type.EdmType))
		{
			n = RewriteIsOfAsIsNull(op, n);
		}
		if (op.IsOfOnly && op.IsOfType.EdmType.Abstract)
		{
			m_suppressDiscriminatorMaps = true;
		}
		return n;
	}

	private bool CanRewriteTypeTest(EdmType testType, EdmType argumentType)
	{
		if (!testType.EdmEquals(argumentType))
		{
			return false;
		}
		if (testType.BaseType != null)
		{
			return false;
		}
		int num = 0;
		foreach (EdmType item in MetadataHelper.GetTypeAndSubtypesOf(testType, base.m_command.MetadataWorkspace, includeAbstractTypes: true))
		{
			_ = item;
			num++;
			if (2 == num)
			{
				break;
			}
		}
		return 1 == num;
	}

	private Node RewriteIsOfAsIsNull(IsOfOp op, Node n)
	{
		ConditionalOp op2 = base.m_command.CreateConditionalOp(OpType.IsNull);
		Node node = base.m_command.CreateNode(op2, n.Child0);
		ProcessConditionalOp(op2, node);
		ConditionalOp op3 = base.m_command.CreateConditionalOp(OpType.Not);
		Node arg = base.m_command.CreateNode(op3, node);
		ConstantBaseOp op4 = base.m_command.CreateConstantOp(op.Type, true);
		Node arg2 = base.m_command.CreateNode(op4);
		NullOp op5 = base.m_command.CreateNullOp(op.Type);
		Node arg3 = base.m_command.CreateNode(op5);
		CaseOp op6 = base.m_command.CreateCaseOp(op.Type);
		Node arg4 = base.m_command.CreateNode(op6, arg, arg2, arg3);
		ComparisonOp op7 = base.m_command.CreateComparisonOp(OpType.EQ);
		return base.m_command.CreateNode(op7, arg4, arg2);
	}

	public override Node Visit(NavigateOp op, Node n)
	{
		VisitScalarOpDefault(op, n);
		Node n2 = RewriteNavigateOp(n, op, out var outputVar);
		n2 = VisitNode(n2);
		if (outputVar != null)
		{
			n2 = AddSubqueryToParentRelOp(outputVar, n2);
		}
		return n2;
	}

	private EntitySet GetCurrentEntityTypeScope()
	{
		if (m_entityTypeScopes.Count == 0)
		{
			return null;
		}
		return m_entityTypeScopes.Peek();
	}

	private static RelationshipSet FindRelationshipSet(EntitySetBase entitySet, RelProperty relProperty)
	{
		foreach (EntitySetBase baseEntitySet in entitySet.EntityContainer.BaseEntitySets)
		{
			if (baseEntitySet is AssociationSet associationSet && associationSet.ElementType.EdmEquals(relProperty.Relationship) && associationSet.AssociationSetEnds[relProperty.FromEnd.Identity].EntitySet.EdmEquals(entitySet))
			{
				return associationSet;
			}
		}
		return null;
	}

	private static int FindPosition(EdmType type, EdmMember member)
	{
		int num = 0;
		foreach (EdmMember allStructuralMember in TypeHelpers.GetAllStructuralMembers(type))
		{
			if (allStructuralMember.EdmEquals(member))
			{
				return num;
			}
			num++;
		}
		PlanCompiler.Assert(condition: false, "Could not find property " + member?.ToString() + " in type " + type.Name);
		return -1;
	}

	private Node BuildKeyExpressionForNewEntityOp(Op op, Node n)
	{
		PlanCompiler.Assert(op.OpType == OpType.NewEntity || op.OpType == OpType.DiscriminatedNewEntity, "BuildKeyExpression: Unexpected OpType:" + op.OpType);
		int num = ((op.OpType == OpType.DiscriminatedNewEntity) ? 1 : 0);
		EntityTypeBase entityTypeBase = (EntityTypeBase)op.Type.EdmType;
		List<Node> list = new List<Node>();
		List<KeyValuePair<string, TypeUsage>> list2 = new List<KeyValuePair<string, TypeUsage>>();
		foreach (EdmMember keyMember in entityTypeBase.KeyMembers)
		{
			int num2 = FindPosition(entityTypeBase, keyMember) + num;
			PlanCompiler.Assert(n.Children.Count > num2, "invalid position " + num2 + "; total count = " + n.Children.Count);
			list.Add(n.Children[num2]);
			list2.Add(new KeyValuePair<string, TypeUsage>(keyMember.Name, keyMember.TypeUsage));
		}
		TypeUsage type = TypeHelpers.CreateRowTypeUsage(list2);
		NewRecordOp op2 = base.m_command.CreateNewRecordOp(type);
		return base.m_command.CreateNode(op2, list);
	}

	private Node BuildRelPropertyExpression(EntitySetBase entitySet, RelProperty relProperty, Node keyExpr)
	{
		keyExpr = OpCopier.Copy(base.m_command, keyExpr);
		RelationshipSet relationshipSet = FindRelationshipSet(entitySet, relProperty);
		if (relationshipSet == null)
		{
			return base.m_command.CreateNode(base.m_command.CreateNullOp(relProperty.ToEnd.TypeUsage));
		}
		ScanTableOp scanTableOp = base.m_command.CreateScanTableOp(Command.CreateTableDefinition(relationshipSet));
		PlanCompiler.Assert(scanTableOp.Table.Columns.Count == 1, "Unexpected column count for table:" + scanTableOp.Table.TableMetadata.Extent?.ToString() + "=" + scanTableOp.Table.Columns.Count);
		Var var = scanTableOp.Table.Columns[0];
		Node arg = base.m_command.CreateNode(scanTableOp);
		Node arg2 = base.m_command.CreateNode(base.m_command.CreatePropertyOp(relProperty.FromEnd), base.m_command.CreateNode(base.m_command.CreateVarRefOp(var)));
		Node arg3 = base.m_command.BuildComparison(OpType.EQ, keyExpr, base.m_command.CreateNode(base.m_command.CreateGetRefKeyOp(keyExpr.Op.Type), arg2), useDatabaseNullSemantics: true);
		Node n = base.m_command.CreateNode(base.m_command.CreateFilterOp(), arg, arg3);
		Node subquery = VisitNode(n);
		subquery = AddSubqueryToParentRelOp(var, subquery);
		return base.m_command.CreateNode(base.m_command.CreatePropertyOp(relProperty.ToEnd), subquery);
	}

	private IEnumerable<Node> BuildAllRelPropertyExpressions(EntitySetBase entitySet, List<RelProperty> relPropertyList, Dictionary<RelProperty, Node> prebuiltExpressions, Node keyExpr)
	{
		foreach (RelProperty relProperty in relPropertyList)
		{
			if (!prebuiltExpressions.TryGetValue(relProperty, out var value))
			{
				value = BuildRelPropertyExpression(entitySet, relProperty, keyExpr);
			}
			yield return value;
		}
	}

	public override Node Visit(NewEntityOp op, Node n)
	{
		if (op.Scoped || op.Type.EdmType.BuiltInTypeKind != BuiltInTypeKind.EntityType)
		{
			return base.Visit(op, n);
		}
		EntityType entityType = (EntityType)op.Type.EdmType;
		EntitySet currentEntityTypeScope = GetCurrentEntityTypeScope();
		List<RelProperty> list;
		List<Node> list2;
		if (currentEntityTypeScope == null)
		{
			m_freeFloatingEntityConstructorTypes.Add(entityType);
			PlanCompiler.Assert(op.RelationshipProperties == null || op.RelationshipProperties.Count == 0, "Related Entities cannot be specified for Entity constructors that are not part of the Query Mapping View for an Entity Set.");
			VisitScalarOpDefault(op, n);
			list = op.RelationshipProperties;
			list2 = n.Children;
		}
		else
		{
			list = new List<RelProperty>(m_relPropertyHelper.GetRelProperties(entityType));
			int num = op.RelationshipProperties.Count - 1;
			List<RelProperty> list3 = new List<RelProperty>(op.RelationshipProperties);
			int num2 = n.Children.Count - 1;
			while (num2 >= entityType.Properties.Count)
			{
				if (!list.Contains(op.RelationshipProperties[num]))
				{
					n.Children.RemoveAt(num2);
					list3.RemoveAt(num);
				}
				num2--;
				num--;
			}
			VisitScalarOpDefault(op, n);
			Node keyExpr = BuildKeyExpressionForNewEntityOp(op, n);
			Dictionary<RelProperty, Node> dictionary = new Dictionary<RelProperty, Node>();
			num = 0;
			int num3 = entityType.Properties.Count;
			while (num3 < n.Children.Count)
			{
				dictionary[list3[num]] = n.Children[num3];
				num3++;
				num++;
			}
			list2 = new List<Node>();
			for (int i = 0; i < entityType.Properties.Count; i++)
			{
				list2.Add(n.Children[i]);
			}
			foreach (Node item in BuildAllRelPropertyExpressions(currentEntityTypeScope, list, dictionary, keyExpr))
			{
				list2.Add(item);
			}
		}
		Op op2 = base.m_command.CreateScopedNewEntityOp(op.Type, list, currentEntityTypeScope);
		return base.m_command.CreateNode(op2, list2);
	}

	public override Node Visit(DiscriminatedNewEntityOp op, Node n)
	{
		HashSet<RelProperty> hashSet = new HashSet<RelProperty>();
		List<RelProperty> list = new List<RelProperty>();
		foreach (KeyValuePair<object, EntityType> item in op.DiscriminatorMap.TypeMap)
		{
			EntityTypeBase value = item.Value;
			AddTypeReference(TypeUsage.Create(value));
			foreach (RelProperty relProperty in m_relPropertyHelper.GetRelProperties(value))
			{
				hashSet.Add(relProperty);
			}
		}
		list = new List<RelProperty>(hashSet);
		VisitScalarOpDefault(op, n);
		Node keyExpr = BuildKeyExpressionForNewEntityOp(op, n);
		List<Node> list2 = new List<Node>();
		int num = n.Children.Count - op.RelationshipProperties.Count;
		for (int i = 0; i < num; i++)
		{
			list2.Add(n.Children[i]);
		}
		Dictionary<RelProperty, Node> dictionary = new Dictionary<RelProperty, Node>();
		int num2 = num;
		int num3 = 0;
		while (num2 < n.Children.Count)
		{
			dictionary[op.RelationshipProperties[num3]] = n.Children[num2];
			num2++;
			num3++;
		}
		foreach (Node item2 in BuildAllRelPropertyExpressions(op.EntitySet, list, dictionary, keyExpr))
		{
			list2.Add(item2);
		}
		Op op2 = base.m_command.CreateDiscriminatedNewEntityOp(op.Type, op.DiscriminatorMap, op.EntitySet, list);
		return base.m_command.CreateNode(op2, list2);
	}

	public override Node Visit(NewMultisetOp op, Node n)
	{
		Node resultNode = null;
		Var resultVar = null;
		CollectionType edmType = TypeHelpers.GetEdmType<CollectionType>(op.Type);
		if (!n.HasChild0)
		{
			Node arg = base.m_command.CreateNode(base.m_command.CreateSingleRowTableOp());
			Node input = base.m_command.CreateNode(base.m_command.CreateFilterOp(), arg, base.m_command.CreateNode(base.m_command.CreateFalseOp()));
			Node computedExpression = base.m_command.CreateNode(base.m_command.CreateNullOp(edmType.TypeUsage));
			resultNode = base.m_command.BuildProject(input, computedExpression, out var projectVar);
			resultVar = projectVar;
		}
		else if (n.Children.Count == 1 || AreAllConstantsOrNulls(n.Children))
		{
			List<Node> list = new List<Node>();
			List<Var> list2 = new List<Var>();
			foreach (Node child in n.Children)
			{
				Node input2 = base.m_command.CreateNode(base.m_command.CreateSingleRowTableOp());
				Var projectVar2;
				Node item = base.m_command.BuildProject(input2, child, out projectVar2);
				list.Add(item);
				list2.Add(projectVar2);
			}
			base.m_command.BuildUnionAllLadder((IList<Node>)list, (IList<Var>)list2, out resultNode, out resultVar);
		}
		else
		{
			List<Node> list3 = new List<Node>();
			List<Var> list4 = new List<Var>();
			for (int i = 0; i < n.Children.Count; i++)
			{
				Node input3 = base.m_command.CreateNode(base.m_command.CreateSingleRowTableOp());
				Node computedExpression2 = base.m_command.CreateNode(base.m_command.CreateInternalConstantOp(base.m_command.IntegerType, i));
				Var projectVar3;
				Node item2 = base.m_command.BuildProject(input3, computedExpression2, out projectVar3);
				list3.Add(item2);
				list4.Add(projectVar3);
			}
			base.m_command.BuildUnionAllLadder((IList<Node>)list3, (IList<Var>)list4, out resultNode, out resultVar);
			List<Node> list5 = new List<Node>(n.Children.Count * 2 + 1);
			for (int j = 0; j < n.Children.Count; j++)
			{
				if (j != n.Children.Count - 1)
				{
					ComparisonOp op2 = base.m_command.CreateComparisonOp(OpType.EQ);
					Node item3 = base.m_command.CreateNode(op2, base.m_command.CreateNode(base.m_command.CreateVarRefOp(resultVar)), base.m_command.CreateNode(base.m_command.CreateConstantOp(base.m_command.IntegerType, j)));
					list5.Add(item3);
				}
				list5.Add(n.Children[j]);
			}
			Node computedExpression3 = base.m_command.CreateNode(base.m_command.CreateCaseOp(edmType.TypeUsage), list5);
			resultNode = base.m_command.BuildProject(resultNode, computedExpression3, out resultVar);
		}
		PhysicalProjectOp op3 = base.m_command.CreatePhysicalProjectOp(resultVar);
		Node arg2 = base.m_command.CreateNode(op3, resultNode);
		CollectOp op4 = base.m_command.CreateCollectOp(op.Type);
		Node n2 = base.m_command.CreateNode(op4, arg2);
		return VisitNode(n2);
	}

	private static bool AreAllConstantsOrNulls(List<Node> nodes)
	{
		foreach (Node node in nodes)
		{
			if (node.Op.OpType != 0 && node.Op.OpType != OpType.Null)
			{
				return false;
			}
		}
		return true;
	}

	public override Node Visit(CollectOp op, Node n)
	{
		m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.NestPullup);
		return VisitScalarOpDefault(op, n);
	}

	private void HandleTableOpMetadata(ScanTableBaseOp op)
	{
		if (op.Table.TableMetadata.Extent is EntitySet entitySet)
		{
			AddEntitySetReference(entitySet);
		}
		TypeUsage type = TypeUsage.Create(op.Table.TableMetadata.Extent.ElementType);
		AddTypeReference(type);
	}

	private Node ProcessScanTable(Node scanTableNode, ScanTableOp scanTableOp, ref IsOfOp typeFilter)
	{
		HandleTableOpMetadata(scanTableOp);
		PlanCompiler.Assert(scanTableOp.Table.TableMetadata.Extent != null, "ScanTableOp must reference a table with an extent");
		Node node = null;
		if (scanTableOp.Table.TableMetadata.Extent.EntityContainer.DataSpace == DataSpace.SSpace)
		{
			return scanTableNode;
		}
		node = ExpandView(scanTableOp, ref typeFilter);
		return VisitNode(node);
	}

	public override Node Visit(ScanTableOp op, Node n)
	{
		IsOfOp typeFilter = null;
		return ProcessScanTable(n, op, ref typeFilter);
	}

	public override Node Visit(ScanViewOp op, Node n)
	{
		bool flag = false;
		if (op.Table.TableMetadata.Extent.BuiltInTypeKind == BuiltInTypeKind.EntitySet)
		{
			m_entityTypeScopes.Push((EntitySet)op.Table.TableMetadata.Extent);
			flag = true;
		}
		HandleTableOpMetadata(op);
		VisitRelOpDefault(op, n);
		if (flag)
		{
			PlanCompiler.Assert(m_entityTypeScopes.Pop() == op.Table.TableMetadata.Extent, "m_entityTypeScopes stack is broken");
		}
		return n;
	}

	protected override Node VisitJoinOp(JoinBaseOp op, Node n)
	{
		if (op.OpType == OpType.InnerJoin || op.OpType == OpType.LeftOuterJoin)
		{
			m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.JoinElimination);
		}
		if (ProcessJoinOp(n))
		{
			m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.Normalization);
		}
		return n;
	}

	protected override Node VisitApplyOp(ApplyBaseOp op, Node n)
	{
		m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.JoinElimination);
		return VisitRelOpDefault(op, n);
	}

	private bool IsSortUnnecessary()
	{
		Node node = m_ancestors.Peek();
		PlanCompiler.Assert(node != null, "unexpected SortOp as root node?");
		if (node.Op.OpType == OpType.PhysicalProject)
		{
			return false;
		}
		return true;
	}

	public override Node Visit(SortOp op, Node n)
	{
		if (IsSortUnnecessary())
		{
			return VisitNode(n.Child0);
		}
		return VisitRelOpDefault(op, n);
	}

	private static bool IsOfTypeOverScanTable(Node n, out IsOfOp typeFilter)
	{
		typeFilter = null;
		if (!(n.Child1.Op is IsOfOp isOfOp))
		{
			return false;
		}
		if (!(n.Child0.Op is ScanTableOp scanTableOp) || scanTableOp.Table.Columns.Count != 1)
		{
			return false;
		}
		if (!(n.Child1.Child0.Op is VarRefOp varRefOp) || varRefOp.Var != scanTableOp.Table.Columns[0])
		{
			return false;
		}
		typeFilter = isOfOp;
		return true;
	}

	public override Node Visit(FilterOp op, Node n)
	{
		if (IsOfTypeOverScanTable(n, out var typeFilter))
		{
			Node node = ProcessScanTable(n.Child0, (ScanTableOp)n.Child0.Op, ref typeFilter);
			if (typeFilter != null)
			{
				n.Child1 = VisitNode(n.Child1);
				n.Child0 = node;
				node = n;
			}
			return node;
		}
		return VisitRelOpDefault(op, n);
	}

	public override Node Visit(ProjectOp op, Node n)
	{
		PlanCompiler.Assert(n.HasChild0, "projectOp without input?");
		if (OpType.Sort == n.Child0.Op.OpType || OpType.ConstrainedSort == n.Child0.Op.OpType)
		{
			SortBaseOp sortBaseOp = (SortBaseOp)n.Child0.Op;
			if (sortBaseOp.Keys.Count > 0)
			{
				IList<Node> list = new List<Node>();
				list.Add(n);
				for (int i = 1; i < n.Child0.Children.Count; i++)
				{
					list.Add(n.Child0.Children[i]);
				}
				n.Child0 = n.Child0.Child0;
				foreach (SortKey key in sortBaseOp.Keys)
				{
					op.Outputs.Set(key.Var);
				}
				return VisitNode(base.m_command.CreateNode(sortBaseOp, list));
			}
		}
		return VisitRelOpDefault(op, n);
	}

	public override Node Visit(GroupByIntoOp op, Node n)
	{
		m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.AggregatePushdown);
		return base.Visit(op, n);
	}

	public override Node Visit(ComparisonOp op, Node n)
	{
		if (op.OpType == OpType.EQ || op.OpType == OpType.NE)
		{
			m_compilerState.MarkPhaseAsNeeded(PlanCompilerPhase.NullSemantics);
		}
		return base.Visit(op, n);
	}
}
