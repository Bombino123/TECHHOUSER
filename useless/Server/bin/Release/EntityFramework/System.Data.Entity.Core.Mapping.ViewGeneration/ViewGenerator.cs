using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.QueryRewriting;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class ViewGenerator : InternalBase
{
	private readonly Set<Cell> m_cellGroup;

	private readonly ConfigViewGenerator m_config;

	private readonly MemberDomainMap m_queryDomainMap;

	private readonly MemberDomainMap m_updateDomainMap;

	private readonly Dictionary<EntitySetBase, QueryRewriter> m_queryRewriterCache;

	private readonly List<ForeignConstraint> m_foreignKeyConstraints;

	private readonly EntityContainerMapping m_entityContainerMapping;

	internal ViewGenerator(Set<Cell> cellGroup, ConfigViewGenerator config, List<ForeignConstraint> foreignKeyConstraints, EntityContainerMapping entityContainerMapping)
	{
		m_cellGroup = cellGroup;
		m_config = config;
		m_queryRewriterCache = new Dictionary<EntitySetBase, QueryRewriter>();
		m_foreignKeyConstraints = foreignKeyConstraints;
		m_entityContainerMapping = entityContainerMapping;
		Dictionary<EntityType, Set<EntityType>> inheritanceGraph = MetadataHelper.BuildUndirectedGraphOfTypes(entityContainerMapping.StorageMappingItemCollection.EdmItemCollection);
		SetConfiguration(entityContainerMapping);
		m_queryDomainMap = new MemberDomainMap(ViewTarget.QueryView, m_config.IsValidationEnabled, cellGroup, entityContainerMapping.StorageMappingItemCollection.EdmItemCollection, m_config, inheritanceGraph);
		m_updateDomainMap = new MemberDomainMap(ViewTarget.UpdateView, m_config.IsValidationEnabled, cellGroup, entityContainerMapping.StorageMappingItemCollection.EdmItemCollection, m_config, inheritanceGraph);
		MemberDomainMap.PropagateUpdateDomainToQueryDomain(cellGroup, m_queryDomainMap, m_updateDomainMap);
		UpdateWhereClauseForEachCell(cellGroup, m_queryDomainMap, m_updateDomainMap, m_config);
		MemberDomainMap openDomain = m_queryDomainMap.GetOpenDomain();
		MemberDomainMap openDomain2 = m_updateDomainMap.GetOpenDomain();
		foreach (Cell item in cellGroup)
		{
			item.CQuery.WhereClause.FixDomainMap(openDomain);
			item.SQuery.WhereClause.FixDomainMap(openDomain2);
			item.CQuery.WhereClause.ExpensiveSimplify();
			item.SQuery.WhereClause.ExpensiveSimplify();
			item.CQuery.WhereClause.FixDomainMap(m_queryDomainMap);
			item.SQuery.WhereClause.FixDomainMap(m_updateDomainMap);
		}
	}

	private void SetConfiguration(EntityContainerMapping entityContainerMapping)
	{
		m_config.IsValidationEnabled = entityContainerMapping.Validate;
		m_config.GenerateUpdateViews = entityContainerMapping.GenerateUpdateViews;
	}

	internal ErrorLog GenerateAllBidirectionalViews(KeyToListMap<EntitySetBase, GeneratedView> views, CqlIdentifiers identifiers)
	{
		if (m_config.IsNormalTracing)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Cell.CellsToBuilder(stringBuilder, m_cellGroup);
			Helpers.StringTraceLine(stringBuilder.ToString());
		}
		m_config.SetTimeForFinishedActivity(PerfType.CellCreation);
		ErrorLog errorLog = new CellGroupValidator(m_cellGroup, m_config).Validate();
		if (errorLog.Count > 0)
		{
			errorLog.PrintTrace();
			return errorLog;
		}
		m_config.SetTimeForFinishedActivity(PerfType.KeyConstraint);
		if (m_config.GenerateUpdateViews)
		{
			errorLog = GenerateDirectionalViews(ViewTarget.UpdateView, identifiers, views);
			if (errorLog.Count > 0)
			{
				return errorLog;
			}
		}
		if (m_config.IsValidationEnabled)
		{
			CheckForeignKeyConstraints(errorLog);
		}
		m_config.SetTimeForFinishedActivity(PerfType.ForeignConstraint);
		if (errorLog.Count > 0)
		{
			errorLog.PrintTrace();
			return errorLog;
		}
		m_updateDomainMap.ExpandDomainsToIncludeAllPossibleValues();
		return GenerateDirectionalViews(ViewTarget.QueryView, identifiers, views);
	}

	internal ErrorLog GenerateQueryViewForSingleExtent(KeyToListMap<EntitySetBase, GeneratedView> views, CqlIdentifiers identifiers, EntitySetBase entity, EntityTypeBase type, ViewGenMode mode)
	{
		if (m_config.IsNormalTracing)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Cell.CellsToBuilder(stringBuilder, m_cellGroup);
			Helpers.StringTraceLine(stringBuilder.ToString());
		}
		ErrorLog errorLog = new CellGroupValidator(m_cellGroup, m_config).Validate();
		if (errorLog.Count > 0)
		{
			errorLog.PrintTrace();
			return errorLog;
		}
		if (m_config.IsValidationEnabled)
		{
			CheckForeignKeyConstraints(errorLog);
		}
		if (errorLog.Count > 0)
		{
			errorLog.PrintTrace();
			return errorLog;
		}
		m_updateDomainMap.ExpandDomainsToIncludeAllPossibleValues();
		foreach (Cell item in m_cellGroup)
		{
			item.SQuery.WhereClause.FixDomainMap(m_updateDomainMap);
		}
		return GenerateQueryViewForExtentAndType(identifiers, views, entity, type, mode);
	}

	private static void UpdateWhereClauseForEachCell(IEnumerable<Cell> extentCells, MemberDomainMap queryDomainMap, MemberDomainMap updateDomainMap, ConfigViewGenerator config)
	{
		foreach (Cell extentCell in extentCells)
		{
			extentCell.CQuery.UpdateWhereClause(queryDomainMap);
			extentCell.SQuery.UpdateWhereClause(updateDomainMap);
		}
		queryDomainMap.ReduceEnumerableDomainToEnumeratedValues(config);
		updateDomainMap.ReduceEnumerableDomainToEnumeratedValues(config);
	}

	private ErrorLog GenerateQueryViewForExtentAndType(CqlIdentifiers identifiers, KeyToListMap<EntitySetBase, GeneratedView> views, EntitySetBase entity, EntityTypeBase type, ViewGenMode mode)
	{
		ErrorLog errorLog = new ErrorLog();
		if (m_config.IsViewTracing)
		{
			Helpers.StringTraceLine(string.Empty);
			Helpers.StringTraceLine(string.Empty);
			Helpers.FormatTraceLine("================= Generating {0} Query View for: {1} ===========================", (mode == ViewGenMode.OfTypeViews) ? "OfType" : "OfTypeOnly", entity.Name);
			Helpers.StringTraceLine(string.Empty);
			Helpers.StringTraceLine(string.Empty);
		}
		try
		{
			ViewgenContext context = CreateViewgenContext(entity, ViewTarget.QueryView, identifiers);
			GenerateViewsForExtentAndType(type, context, identifiers, views, mode);
		}
		catch (InternalMappingException ex)
		{
			errorLog.Merge(ex.ErrorLog);
		}
		return errorLog;
	}

	private ErrorLog GenerateDirectionalViews(ViewTarget viewTarget, CqlIdentifiers identifiers, KeyToListMap<EntitySetBase, GeneratedView> views)
	{
		bool flag = viewTarget == ViewTarget.QueryView;
		KeyToListMap<EntitySetBase, Cell> keyToListMap = GroupCellsByExtent(m_cellGroup, viewTarget);
		ErrorLog errorLog = new ErrorLog();
		foreach (EntitySetBase key in keyToListMap.Keys)
		{
			if (m_config.IsViewTracing)
			{
				Helpers.StringTraceLine(string.Empty);
				Helpers.StringTraceLine(string.Empty);
				Helpers.FormatTraceLine("================= Generating {0} View for: {1} ===========================", flag ? "Query" : "Update", key.Name);
				Helpers.StringTraceLine(string.Empty);
				Helpers.StringTraceLine(string.Empty);
			}
			try
			{
				QueryRewriter queryRewriter = GenerateDirectionalViewsForExtent(viewTarget, key, identifiers, views);
				if (viewTarget == ViewTarget.UpdateView && m_config.IsValidationEnabled)
				{
					if (m_config.IsViewTracing)
					{
						Helpers.StringTraceLine(string.Empty);
						Helpers.StringTraceLine(string.Empty);
						Helpers.FormatTraceLine("----------------- Validation for generated update view for: {0} -----------------", key.Name);
						Helpers.StringTraceLine(string.Empty);
						Helpers.StringTraceLine(string.Empty);
					}
					new RewritingValidator(queryRewriter.ViewgenContext, queryRewriter.BasicView).Validate();
				}
			}
			catch (InternalMappingException ex)
			{
				errorLog.Merge(ex.ErrorLog);
			}
		}
		return errorLog;
	}

	private QueryRewriter GenerateDirectionalViewsForExtent(ViewTarget viewTarget, EntitySetBase extent, CqlIdentifiers identifiers, KeyToListMap<EntitySetBase, GeneratedView> views)
	{
		ViewgenContext context = CreateViewgenContext(extent, viewTarget, identifiers);
		QueryRewriter queryRewriter = null;
		if (m_config.GenerateViewsForEachType)
		{
			foreach (EdmType item in MetadataHelper.GetTypeAndSubtypesOf(extent.ElementType, m_entityContainerMapping.StorageMappingItemCollection.EdmItemCollection, includeAbstractTypes: false))
			{
				if (m_config.IsViewTracing && !item.Equals(extent.ElementType))
				{
					Helpers.FormatTraceLine("CQL View for {0} and type {1}", extent.Name, item.Name);
				}
				queryRewriter = GenerateViewsForExtentAndType(item, context, identifiers, views, ViewGenMode.OfTypeViews);
			}
		}
		else
		{
			queryRewriter = GenerateViewsForExtentAndType(extent.ElementType, context, identifiers, views, ViewGenMode.OfTypeViews);
		}
		if (viewTarget == ViewTarget.QueryView)
		{
			m_config.SetTimeForFinishedActivity(PerfType.QueryViews);
		}
		else
		{
			m_config.SetTimeForFinishedActivity(PerfType.UpdateViews);
		}
		m_queryRewriterCache[extent] = queryRewriter;
		return queryRewriter;
	}

	private ViewgenContext CreateViewgenContext(EntitySetBase extent, ViewTarget viewTarget, CqlIdentifiers identifiers)
	{
		if (!m_queryRewriterCache.TryGetValue(extent, out var value))
		{
			List<Cell> extentCells = m_cellGroup.Where((Cell c) => c.GetLeftQuery(viewTarget).Extent == extent).ToList();
			return new ViewgenContext(viewTarget, extent, extentCells, identifiers, m_config, m_queryDomainMap, m_updateDomainMap, m_entityContainerMapping);
		}
		return value.ViewgenContext;
	}

	private QueryRewriter GenerateViewsForExtentAndType(EdmType generatedType, ViewgenContext context, CqlIdentifiers identifiers, KeyToListMap<EntitySetBase, GeneratedView> views, ViewGenMode mode)
	{
		QueryRewriter queryRewriter = new QueryRewriter(generatedType, context, mode);
		queryRewriter.GenerateViewComponents();
		CellTreeNode basicView = queryRewriter.BasicView;
		if (m_config.IsNormalTracing)
		{
			Helpers.StringTrace("Basic View: ");
			Helpers.StringTraceLine(basicView.ToString());
		}
		CellTreeNode cellTreeNode = GenerateSimplifiedView(basicView, queryRewriter.UsedCells);
		if (m_config.IsNormalTracing)
		{
			Helpers.StringTraceLine(string.Empty);
			Helpers.StringTrace("Simplified View: ");
			Helpers.StringTraceLine(cellTreeNode.ToString());
		}
		CqlGenerator cqlGenerator = new CqlGenerator(cellTreeNode, queryRewriter.CaseStatements, identifiers, context.MemberMaps.ProjectedSlotMap, queryRewriter.UsedCells.Count, queryRewriter.TopLevelWhereClause, m_entityContainerMapping.StorageMappingItemCollection);
		string eSQL;
		DbQueryCommandTree commandTree;
		if (m_config.GenerateEsql)
		{
			eSQL = cqlGenerator.GenerateEsql();
			commandTree = null;
		}
		else
		{
			eSQL = null;
			commandTree = cqlGenerator.GenerateCqt();
		}
		GeneratedView value = GeneratedView.CreateGeneratedView(context.Extent, generatedType, commandTree, eSQL, m_entityContainerMapping.StorageMappingItemCollection, m_config);
		views.Add(context.Extent, value);
		return queryRewriter;
	}

	private static CellTreeNode GenerateSimplifiedView(CellTreeNode basicView, List<LeftCellWrapper> usedCells)
	{
		int count = usedCells.Count;
		for (int i = 0; i < count; i++)
		{
			usedCells[i].RightCellQuery.InitializeBoolExpressions(count, i);
		}
		return CellTreeSimplifier.MergeNodes(basicView);
	}

	private void CheckForeignKeyConstraints(ErrorLog errorLog)
	{
		foreach (ForeignConstraint foreignKeyConstraint in m_foreignKeyConstraints)
		{
			QueryRewriter value = null;
			QueryRewriter value2 = null;
			m_queryRewriterCache.TryGetValue(foreignKeyConstraint.ChildTable, out value);
			m_queryRewriterCache.TryGetValue(foreignKeyConstraint.ParentTable, out value2);
			foreignKeyConstraint.CheckConstraint(m_cellGroup, value, value2, errorLog, m_config);
		}
	}

	private static KeyToListMap<EntitySetBase, Cell> GroupCellsByExtent(IEnumerable<Cell> cells, ViewTarget viewTarget)
	{
		KeyToListMap<EntitySetBase, Cell> keyToListMap = new KeyToListMap<EntitySetBase, Cell>(EqualityComparer<EntitySetBase>.Default);
		foreach (Cell cell in cells)
		{
			CellQuery leftQuery = cell.GetLeftQuery(viewTarget);
			keyToListMap.Add(leftQuery.Extent, cell);
		}
		return keyToListMap;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		Cell.CellsToBuilder(builder, m_cellGroup);
	}
}
