using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class BasicViewGenerator : InternalBase
{
	private readonly MemberProjectionIndex m_projectedSlotMap;

	private readonly List<LeftCellWrapper> m_usedCells;

	private readonly FragmentQuery m_activeDomain;

	private readonly ViewgenContext m_viewgenContext;

	private readonly ErrorLog m_errorLog;

	private readonly ConfigViewGenerator m_config;

	private readonly MemberDomainMap m_domainMap;

	private FragmentQueryProcessor LeftQP => m_viewgenContext.LeftFragmentQP;

	internal BasicViewGenerator(MemberProjectionIndex projectedSlotMap, List<LeftCellWrapper> usedCells, FragmentQuery activeDomain, ViewgenContext context, MemberDomainMap domainMap, ErrorLog errorLog, ConfigViewGenerator config)
	{
		m_projectedSlotMap = projectedSlotMap;
		m_usedCells = usedCells;
		m_viewgenContext = context;
		m_activeDomain = activeDomain;
		m_errorLog = errorLog;
		m_config = config;
		m_domainMap = domainMap;
	}

	internal CellTreeNode CreateViewExpression()
	{
		OpCellTreeNode opCellTreeNode = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.FOJ);
		foreach (LeftCellWrapper usedCell in m_usedCells)
		{
			LeafCellTreeNode child = new LeafCellTreeNode(m_viewgenContext, usedCell);
			opCellTreeNode.Add(child);
		}
		CellTreeNode rootNode = GroupByRightExtent(opCellTreeNode);
		rootNode = IsolateUnions(rootNode);
		rootNode = IsolateByOperator(rootNode, CellTreeOpType.Union);
		rootNode = IsolateByOperator(rootNode, CellTreeOpType.IJ);
		rootNode = IsolateByOperator(rootNode, CellTreeOpType.LOJ);
		if (m_viewgenContext.ViewTarget == ViewTarget.QueryView)
		{
			rootNode = ConvertUnionsToNormalizedLOJs(rootNode);
		}
		return rootNode;
	}

	internal CellTreeNode GroupByRightExtent(CellTreeNode rootNode)
	{
		KeyToListMap<EntitySetBase, LeafCellTreeNode> keyToListMap = new KeyToListMap<EntitySetBase, LeafCellTreeNode>(EqualityComparer<EntitySetBase>.Default);
		foreach (LeafCellTreeNode child in rootNode.Children)
		{
			EntitySetBase extent = child.LeftCellWrapper.RightCellQuery.Extent;
			keyToListMap.Add(extent, child);
		}
		OpCellTreeNode opCellTreeNode = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.FOJ);
		foreach (EntitySetBase key in keyToListMap.Keys)
		{
			OpCellTreeNode opCellTreeNode2 = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.FOJ);
			foreach (LeafCellTreeNode item in keyToListMap.ListForKey(key))
			{
				opCellTreeNode2.Add(item);
			}
			opCellTreeNode.Add(opCellTreeNode2);
		}
		return opCellTreeNode.Flatten();
	}

	private CellTreeNode IsolateUnions(CellTreeNode rootNode)
	{
		if (rootNode.Children.Count <= 1)
		{
			return rootNode;
		}
		for (int i = 0; i < rootNode.Children.Count; i++)
		{
			rootNode.Children[i] = IsolateUnions(rootNode.Children[i]);
		}
		OpCellTreeNode opCellTreeNode = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.Union);
		ModifiableIteratorCollection<CellTreeNode> modifiableIteratorCollection = new ModifiableIteratorCollection<CellTreeNode>(rootNode.Children);
		while (!modifiableIteratorCollection.IsEmpty)
		{
			OpCellTreeNode opCellTreeNode2 = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.FOJ);
			CellTreeNode child = modifiableIteratorCollection.RemoveOneElement();
			opCellTreeNode2.Add(child);
			foreach (CellTreeNode item in modifiableIteratorCollection.Elements())
			{
				if (!IsDisjoint(opCellTreeNode2, item))
				{
					opCellTreeNode2.Add(item);
					modifiableIteratorCollection.RemoveCurrentOfIterator();
					modifiableIteratorCollection.ResetIterator();
				}
			}
			opCellTreeNode.Add(opCellTreeNode2);
		}
		return opCellTreeNode.Flatten();
	}

	private CellTreeNode ConvertUnionsToNormalizedLOJs(CellTreeNode rootNode)
	{
		for (int i = 0; i < rootNode.Children.Count; i++)
		{
			rootNode.Children[i] = ConvertUnionsToNormalizedLOJs(rootNode.Children[i]);
		}
		if (rootNode.OpType != CellTreeOpType.LOJ || rootNode.Children.Count < 2)
		{
			return rootNode;
		}
		OpCellTreeNode opCellTreeNode = new OpCellTreeNode(m_viewgenContext, rootNode.OpType);
		List<CellTreeNode> list = new List<CellTreeNode>();
		OpCellTreeNode opCellTreeNode2 = null;
		HashSet<CellTreeNode> hashSet = null;
		if (rootNode.Children[0].OpType == CellTreeOpType.IJ)
		{
			opCellTreeNode2 = new OpCellTreeNode(m_viewgenContext, rootNode.Children[0].OpType);
			opCellTreeNode.Add(opCellTreeNode2);
			list.AddRange(rootNode.Children[0].Children);
			hashSet = new HashSet<CellTreeNode>(rootNode.Children[0].Children);
		}
		else
		{
			opCellTreeNode.Add(rootNode.Children[0]);
		}
		foreach (CellTreeNode item in rootNode.Children.Skip(1))
		{
			if (item is OpCellTreeNode { OpType: CellTreeOpType.Union } opCellTreeNode3)
			{
				list.AddRange(opCellTreeNode3.Children);
			}
			else
			{
				list.Add(item);
			}
		}
		KeyToListMap<EntitySet, LeafCellTreeNode> keyToListMap = new KeyToListMap<EntitySet, LeafCellTreeNode>(EqualityComparer<EntitySet>.Default);
		foreach (CellTreeNode item2 in list)
		{
			if (item2 is LeafCellTreeNode leafCellTreeNode)
			{
				EntitySetBase leafNodeTable = GetLeafNodeTable(leafCellTreeNode);
				if (leafNodeTable != null)
				{
					keyToListMap.Add((EntitySet)leafNodeTable, leafCellTreeNode);
				}
			}
			else if (hashSet != null && hashSet.Contains(item2))
			{
				opCellTreeNode2.Add(item2);
			}
			else
			{
				opCellTreeNode.Add(item2);
			}
		}
		KeyValuePair<EntitySet, List<LeafCellTreeNode>>[] array = keyToListMap.KeyValuePairs.Where((KeyValuePair<EntitySet, List<LeafCellTreeNode>> m) => m.Value.Count > 1).ToArray();
		for (int j = 0; j < array.Length; j++)
		{
			KeyValuePair<EntitySet, List<LeafCellTreeNode>> keyValuePair = array[j];
			keyToListMap.RemoveKey(keyValuePair.Key);
			foreach (LeafCellTreeNode item3 in keyValuePair.Value)
			{
				if (hashSet != null && hashSet.Contains(item3))
				{
					opCellTreeNode2.Add(item3);
				}
				else
				{
					opCellTreeNode.Add(item3);
				}
			}
		}
		KeyToListMap<EntitySet, EntitySet> keyToListMap2 = new KeyToListMap<EntitySet, EntitySet>(EqualityComparer<EntitySet>.Default);
		Dictionary<EntitySet, OpCellTreeNode> dictionary = new Dictionary<EntitySet, OpCellTreeNode>(EqualityComparer<EntitySet>.Default);
		foreach (KeyValuePair<EntitySet, List<LeafCellTreeNode>> keyValuePair2 in keyToListMap.KeyValuePairs)
		{
			EntitySet key = keyValuePair2.Key;
			foreach (EntitySet fKOverPKDependent in GetFKOverPKDependents(key))
			{
				if (keyToListMap.TryGetListForKey(fKOverPKDependent, out var valueCollection) && (hashSet == null || !hashSet.Contains(valueCollection.Single())))
				{
					keyToListMap2.Add(key, fKOverPKDependent);
				}
			}
			OpCellTreeNode opCellTreeNode4 = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.LOJ);
			opCellTreeNode4.Add(keyValuePair2.Value.Single());
			dictionary.Add(key, opCellTreeNode4);
		}
		Dictionary<EntitySet, EntitySet> dictionary2 = new Dictionary<EntitySet, EntitySet>(EqualityComparer<EntitySet>.Default);
		foreach (KeyValuePair<EntitySet, List<EntitySet>> keyValuePair3 in keyToListMap2.KeyValuePairs)
		{
			EntitySet key2 = keyValuePair3.Key;
			foreach (EntitySet item4 in keyValuePair3.Value)
			{
				if (dictionary.TryGetValue(item4, out var value) && !dictionary2.ContainsKey(item4) && !CheckLOJCycle(item4, key2, dictionary2))
				{
					dictionary[keyValuePair3.Key].Add(value);
					dictionary2.Add(item4, key2);
				}
			}
		}
		foreach (KeyValuePair<EntitySet, OpCellTreeNode> item5 in dictionary)
		{
			if (!dictionary2.ContainsKey(item5.Key))
			{
				OpCellTreeNode value2 = item5.Value;
				if (hashSet != null && hashSet.Contains(value2.Children[0]))
				{
					opCellTreeNode2.Add(value2);
				}
				else
				{
					opCellTreeNode.Add(value2);
				}
			}
		}
		return opCellTreeNode.Flatten();
	}

	private static IEnumerable<EntitySet> GetFKOverPKDependents(EntitySet principal)
	{
		foreach (Tuple<AssociationSet, ReferentialConstraint> pkFkInfo in principal.ForeignKeyPrincipals)
		{
			ReadOnlyMetadataCollection<EdmMember> keyMembers = pkFkInfo.Item2.ToRole.GetEntityType().KeyMembers;
			ReadOnlyMetadataCollection<EdmProperty> toProperties = pkFkInfo.Item2.ToProperties;
			if (keyMembers.Count != toProperties.Count)
			{
				continue;
			}
			int i;
			for (i = 0; i < keyMembers.Count && keyMembers[i].EdmEquals(toProperties[i]); i++)
			{
			}
			if (i == keyMembers.Count)
			{
				yield return pkFkInfo.Item1.AssociationSetEnds.Where((AssociationSetEnd ase) => ase.Name == pkFkInfo.Item2.ToRole.Name).Single().EntitySet;
			}
		}
	}

	private static EntitySet GetLeafNodeTable(LeafCellTreeNode leaf)
	{
		return leaf.LeftCellWrapper.RightCellQuery.Extent as EntitySet;
	}

	private static bool CheckLOJCycle(EntitySet child, EntitySet parent, Dictionary<EntitySet, EntitySet> nestedExtents)
	{
		do
		{
			if (EqualityComparer<EntitySet>.Default.Equals(parent, child))
			{
				return true;
			}
		}
		while (nestedExtents.TryGetValue(parent, out parent));
		return false;
	}

	internal CellTreeNode IsolateByOperator(CellTreeNode rootNode, CellTreeOpType opTypeToIsolate)
	{
		List<CellTreeNode> children = rootNode.Children;
		if (children.Count <= 1)
		{
			return rootNode;
		}
		for (int i = 0; i < children.Count; i++)
		{
			children[i] = IsolateByOperator(children[i], opTypeToIsolate);
		}
		if ((rootNode.OpType != CellTreeOpType.FOJ && rootNode.OpType != CellTreeOpType.LOJ) || rootNode.OpType == opTypeToIsolate)
		{
			return rootNode;
		}
		OpCellTreeNode opCellTreeNode = new OpCellTreeNode(m_viewgenContext, rootNode.OpType);
		ModifiableIteratorCollection<CellTreeNode> modifiableIteratorCollection = new ModifiableIteratorCollection<CellTreeNode>(children);
		while (!modifiableIteratorCollection.IsEmpty)
		{
			OpCellTreeNode opCellTreeNode2 = new OpCellTreeNode(m_viewgenContext, opTypeToIsolate);
			CellTreeNode child = modifiableIteratorCollection.RemoveOneElement();
			opCellTreeNode2.Add(child);
			foreach (CellTreeNode item in modifiableIteratorCollection.Elements())
			{
				if (TryAddChildToGroup(opTypeToIsolate, item, opCellTreeNode2))
				{
					modifiableIteratorCollection.RemoveCurrentOfIterator();
					if (opTypeToIsolate == CellTreeOpType.LOJ)
					{
						modifiableIteratorCollection.ResetIterator();
					}
				}
			}
			opCellTreeNode.Add(opCellTreeNode2);
		}
		return opCellTreeNode.Flatten();
	}

	private bool TryAddChildToGroup(CellTreeOpType opTypeToIsolate, CellTreeNode childNode, OpCellTreeNode groupNode)
	{
		switch (opTypeToIsolate)
		{
		case CellTreeOpType.IJ:
			if (IsEquivalentTo(childNode, groupNode))
			{
				groupNode.Add(childNode);
				return true;
			}
			break;
		case CellTreeOpType.LOJ:
			if (IsContainedIn(childNode, groupNode))
			{
				groupNode.Add(childNode);
				return true;
			}
			if (IsContainedIn(groupNode, childNode))
			{
				groupNode.AddFirst(childNode);
				return true;
			}
			break;
		case CellTreeOpType.Union:
			if (IsDisjoint(childNode, groupNode))
			{
				groupNode.Add(childNode);
				return true;
			}
			break;
		}
		return false;
	}

	private bool IsDisjoint(CellTreeNode n1, CellTreeNode n2)
	{
		bool flag = LeftQP.IsDisjointFrom(n1.LeftFragmentQuery, n2.LeftFragmentQuery);
		if (flag && m_viewgenContext.ViewTarget == ViewTarget.QueryView)
		{
			return true;
		}
		bool isEmptyRightFragmentQuery = new OpCellTreeNode(m_viewgenContext, CellTreeOpType.IJ, n1, n2).IsEmptyRightFragmentQuery;
		if (m_viewgenContext.ViewTarget == ViewTarget.UpdateView && flag && !isEmptyRightFragmentQuery)
		{
			if (ErrorPatternMatcher.FindMappingErrors(m_viewgenContext, m_domainMap, m_errorLog))
			{
				return false;
			}
			StringBuilder stringBuilder = new StringBuilder(Strings.Viewgen_RightSideNotDisjoint(m_viewgenContext.Extent.ToString()));
			stringBuilder.AppendLine();
			FragmentQuery fragmentQuery = LeftQP.Intersect(n1.RightFragmentQuery, n2.RightFragmentQuery);
			if (LeftQP.IsSatisfiable(fragmentQuery))
			{
				fragmentQuery.Condition.ExpensiveSimplify();
				RewritingValidator.EntityConfigurationToUserString(fragmentQuery.Condition, stringBuilder);
			}
			m_errorLog.AddEntry(new ErrorLog.Record(ViewGenErrorCode.DisjointConstraintViolation, stringBuilder.ToString(), m_viewgenContext.AllWrappersForExtent, string.Empty));
			ExceptionHelpers.ThrowMappingException(m_errorLog, m_config);
			return false;
		}
		return flag || isEmptyRightFragmentQuery;
	}

	private bool IsContainedIn(CellTreeNode n1, CellTreeNode n2)
	{
		FragmentQuery q = LeftQP.Intersect(n1.LeftFragmentQuery, m_activeDomain);
		FragmentQuery q2 = LeftQP.Intersect(n2.LeftFragmentQuery, m_activeDomain);
		if (LeftQP.IsContainedIn(q, q2))
		{
			return true;
		}
		return new OpCellTreeNode(m_viewgenContext, CellTreeOpType.LASJ, n1, n2).IsEmptyRightFragmentQuery;
	}

	private bool IsEquivalentTo(CellTreeNode n1, CellTreeNode n2)
	{
		if (IsContainedIn(n1, n2))
		{
			return IsContainedIn(n2, n1);
		}
		return false;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		m_projectedSlotMap.ToCompactString(builder);
	}
}
