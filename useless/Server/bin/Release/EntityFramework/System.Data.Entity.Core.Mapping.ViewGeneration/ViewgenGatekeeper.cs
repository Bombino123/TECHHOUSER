using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal abstract class ViewgenGatekeeper : InternalBase
{
	internal static ViewGenResults GenerateViewsFromMapping(EntityContainerMapping containerMapping, ConfigViewGenerator config)
	{
		CellCreator cellCreator = new CellCreator(containerMapping);
		List<Cell> cells = cellCreator.GenerateCells();
		CqlIdentifiers identifiers = cellCreator.Identifiers;
		return GenerateViewsFromCells(cells, config, identifiers, containerMapping);
	}

	internal static ViewGenResults GenerateTypeSpecificQueryView(EntityContainerMapping containerMapping, ConfigViewGenerator config, EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out bool success)
	{
		if (config.IsNormalTracing)
		{
			Helpers.StringTraceLine("");
			Helpers.StringTraceLine("<<<<<<<< Generating Query View for Entity [" + entity.Name + "] OfType" + (includeSubtypes ? "" : "Only") + "(" + type.Name + ") >>>>>>>");
		}
		if (containerMapping.GetEntitySetMapping(entity.Name).QueryView != null)
		{
			success = false;
			return null;
		}
		InputForComputingCellGroups args = new InputForComputingCellGroups(containerMapping, config);
		OutputFromComputeCellGroups cellgroups = containerMapping.GetCellgroups(args);
		success = cellgroups.Success;
		if (!success)
		{
			return null;
		}
		List<ForeignConstraint> foreignKeyConstraints = cellgroups.ForeignKeyConstraints;
		List<Set<Cell>> list = cellgroups.CellGroups.Select((Set<Cell> setOfCells) => new Set<Cell>(setOfCells.Select((Cell cell) => new Cell(cell)))).ToList();
		List<Cell> cells = cellgroups.Cells;
		CqlIdentifiers identifiers = cellgroups.Identifiers;
		ViewGenResults viewGenResults = new ViewGenResults();
		ErrorLog errorLog = EnsureAllCSpaceContainerSetsAreMapped(cells, containerMapping);
		if (errorLog.Count > 0)
		{
			viewGenResults.AddErrors(errorLog);
			Helpers.StringTraceLine(viewGenResults.ErrorsToString());
			success = true;
			return viewGenResults;
		}
		foreach (Set<Cell> item in list)
		{
			if (DoesCellGroupContainEntitySet(item, entity))
			{
				ViewGenerator viewGenerator = null;
				ErrorLog errorLog2 = new ErrorLog();
				try
				{
					viewGenerator = new ViewGenerator(item, config, foreignKeyConstraints, containerMapping);
				}
				catch (InternalMappingException ex)
				{
					errorLog2 = ex.ErrorLog;
				}
				if (errorLog2.Count > 0)
				{
					break;
				}
				ViewGenMode mode = (includeSubtypes ? ViewGenMode.OfTypeViews : ViewGenMode.OfTypeOnlyViews);
				errorLog2 = viewGenerator.GenerateQueryViewForSingleExtent(viewGenResults.Views, identifiers, entity, type, mode);
				if (errorLog2.Count != 0)
				{
					viewGenResults.AddErrors(errorLog2);
				}
			}
		}
		success = true;
		return viewGenResults;
	}

	private static ViewGenResults GenerateViewsFromCells(List<Cell> cells, ConfigViewGenerator config, CqlIdentifiers identifiers, EntityContainerMapping containerMapping)
	{
		EntityContainer storageEntityContainer = containerMapping.StorageEntityContainer;
		ViewGenResults viewGenResults = new ViewGenResults();
		ErrorLog errorLog = EnsureAllCSpaceContainerSetsAreMapped(cells, containerMapping);
		if (errorLog.Count > 0)
		{
			viewGenResults.AddErrors(errorLog);
			Helpers.StringTraceLine(viewGenResults.ErrorsToString());
			return viewGenResults;
		}
		List<ForeignConstraint> foreignConstraints = ForeignConstraint.GetForeignConstraints(storageEntityContainer);
		foreach (Set<Cell> item in new CellPartitioner(cells, foreignConstraints).GroupRelatedCells())
		{
			ViewGenerator viewGenerator = null;
			ErrorLog errorLog2 = new ErrorLog();
			try
			{
				viewGenerator = new ViewGenerator(item, config, foreignConstraints, containerMapping);
			}
			catch (InternalMappingException ex)
			{
				errorLog2 = ex.ErrorLog;
			}
			if (errorLog2.Count == 0)
			{
				errorLog2 = viewGenerator.GenerateAllBidirectionalViews(viewGenResults.Views, identifiers);
			}
			if (errorLog2.Count != 0)
			{
				viewGenResults.AddErrors(errorLog2);
			}
		}
		return viewGenResults;
	}

	private static ErrorLog EnsureAllCSpaceContainerSetsAreMapped(IEnumerable<Cell> cells, EntityContainerMapping containerMapping)
	{
		Set<EntitySetBase> set = new Set<EntitySetBase>();
		EntityContainer entityContainer = null;
		foreach (Cell cell in cells)
		{
			set.Add(cell.CQuery.Extent);
			entityContainer = cell.CQuery.Extent.EntityContainer;
		}
		List<EntitySetBase> list = new List<EntitySetBase>();
		foreach (EntitySetBase baseEntitySet in entityContainer.BaseEntitySets)
		{
			if (!set.Contains(baseEntitySet) && !containerMapping.HasQueryViewForSetMap(baseEntitySet.Name) && (!(baseEntitySet is AssociationSet associationSet) || !associationSet.ElementType.IsForeignKey))
			{
				list.Add(baseEntitySet);
			}
		}
		ErrorLog errorLog = new ErrorLog();
		if (list.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (EntitySetBase item in list)
			{
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				flag = false;
				stringBuilder.Append(item.Name);
			}
			string message = Strings.ViewGen_Missing_Set_Mapping(stringBuilder);
			int num = -1;
			foreach (Cell cell2 in cells)
			{
				if (num == -1 || cell2.CellLabel.StartLineNumber < num)
				{
					num = cell2.CellLabel.StartLineNumber;
				}
			}
			ErrorLog.Record record = new ErrorLog.Record(new EdmSchemaError(message, 3027, EdmSchemaErrorSeverity.Error, containerMapping.SourceLocation, containerMapping.StartLineNumber, containerMapping.StartLinePosition, null));
			errorLog.AddEntry(record);
		}
		return errorLog;
	}

	private static bool DoesCellGroupContainEntitySet(Set<Cell> group, EntitySetBase entity)
	{
		foreach (Cell item in group)
		{
			if (item.GetLeftQuery(ViewTarget.QueryView).Extent.Equals(entity))
			{
				return true;
			}
		}
		return false;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
	}
}
