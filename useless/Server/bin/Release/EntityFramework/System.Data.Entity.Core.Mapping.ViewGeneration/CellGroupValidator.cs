#define TRACE
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class CellGroupValidator
{
	private class ExtentPair
	{
		internal readonly EntitySetBase cExtent;

		internal readonly EntitySetBase sExtent;

		internal ExtentPair(EntitySetBase acExtent, EntitySetBase asExtent)
		{
			cExtent = acExtent;
			sExtent = asExtent;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (!(obj is ExtentPair extentPair))
			{
				return false;
			}
			if (extentPair.cExtent.Equals(cExtent))
			{
				return extentPair.sExtent.Equals(sExtent);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return cExtent.GetHashCode() ^ sExtent.GetHashCode();
		}
	}

	private readonly IEnumerable<Cell> m_cells;

	private readonly ConfigViewGenerator m_config;

	private readonly ErrorLog m_errorLog;

	private SchemaConstraints<ViewKeyConstraint> m_cViewConstraints;

	private SchemaConstraints<ViewKeyConstraint> m_sViewConstraints;

	internal CellGroupValidator(IEnumerable<Cell> cells, ConfigViewGenerator config)
	{
		m_cells = cells;
		m_config = config;
		m_errorLog = new ErrorLog();
	}

	internal ErrorLog Validate()
	{
		if (m_config.IsValidationEnabled)
		{
			if (!PerformSingleCellChecks())
			{
				return m_errorLog;
			}
		}
		else if (!CheckCellsWithDistinctFlag())
		{
			return m_errorLog;
		}
		SchemaConstraints<BasicKeyConstraint> schemaConstraints = new SchemaConstraints<BasicKeyConstraint>();
		SchemaConstraints<BasicKeyConstraint> schemaConstraints2 = new SchemaConstraints<BasicKeyConstraint>();
		ConstructCellRelationsWithConstraints(schemaConstraints, schemaConstraints2);
		if (m_config.IsVerboseTracing)
		{
			Trace.WriteLine(string.Empty);
			Trace.WriteLine("C-Level Basic Constraints");
			Trace.WriteLine(schemaConstraints);
			Trace.WriteLine("S-Level Basic Constraints");
			Trace.WriteLine(schemaConstraints2);
		}
		m_cViewConstraints = PropagateConstraints(schemaConstraints);
		m_sViewConstraints = PropagateConstraints(schemaConstraints2);
		if (m_config.IsVerboseTracing)
		{
			Trace.WriteLine(string.Empty);
			Trace.WriteLine("C-Level View Constraints");
			Trace.WriteLine(m_cViewConstraints);
			Trace.WriteLine("S-Level View Constraints");
			Trace.WriteLine(m_sViewConstraints);
		}
		if (m_config.IsValidationEnabled)
		{
			CheckImplication(m_cViewConstraints, m_sViewConstraints);
		}
		return m_errorLog;
	}

	private void ConstructCellRelationsWithConstraints(SchemaConstraints<BasicKeyConstraint> cConstraints, SchemaConstraints<BasicKeyConstraint> sConstraints)
	{
		int num = 0;
		foreach (Cell cell in m_cells)
		{
			cell.CreateViewCellRelation(num);
			BasicCellRelation basicCellRelation = cell.CQuery.BasicCellRelation;
			BasicCellRelation basicCellRelation2 = cell.SQuery.BasicCellRelation;
			PopulateBaseConstraints(basicCellRelation, cConstraints);
			PopulateBaseConstraints(basicCellRelation2, sConstraints);
			num++;
		}
		foreach (Cell cell2 in m_cells)
		{
			foreach (Cell cell3 in m_cells)
			{
			}
		}
	}

	private static void PopulateBaseConstraints(BasicCellRelation baseRelation, SchemaConstraints<BasicKeyConstraint> constraints)
	{
		baseRelation.PopulateKeyConstraints(constraints);
	}

	private static SchemaConstraints<ViewKeyConstraint> PropagateConstraints(SchemaConstraints<BasicKeyConstraint> baseConstraints)
	{
		SchemaConstraints<ViewKeyConstraint> schemaConstraints = new SchemaConstraints<ViewKeyConstraint>();
		foreach (BasicKeyConstraint keyConstraint in baseConstraints.KeyConstraints)
		{
			ViewKeyConstraint viewKeyConstraint = keyConstraint.Propagate();
			if (viewKeyConstraint != null)
			{
				schemaConstraints.Add(viewKeyConstraint);
			}
		}
		return schemaConstraints;
	}

	private void CheckImplication(SchemaConstraints<ViewKeyConstraint> cViewConstraints, SchemaConstraints<ViewKeyConstraint> sViewConstraints)
	{
		CheckImplicationKeyConstraints(cViewConstraints, sViewConstraints);
		KeyToListMap<ExtentPair, ViewKeyConstraint> keyToListMap = new KeyToListMap<ExtentPair, ViewKeyConstraint>(EqualityComparer<ExtentPair>.Default);
		foreach (ViewKeyConstraint keyConstraint in cViewConstraints.KeyConstraints)
		{
			ExtentPair key = new ExtentPair(keyConstraint.Cell.CQuery.Extent, keyConstraint.Cell.SQuery.Extent);
			keyToListMap.Add(key, keyConstraint);
		}
		foreach (ExtentPair key2 in keyToListMap.Keys)
		{
			ReadOnlyCollection<ViewKeyConstraint> readOnlyCollection = keyToListMap.ListForKey(key2);
			bool flag = false;
			foreach (ViewKeyConstraint item in readOnlyCollection)
			{
				foreach (ViewKeyConstraint keyConstraint2 in sViewConstraints.KeyConstraints)
				{
					if (keyConstraint2.Implies(item))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				m_errorLog.AddEntry(ViewKeyConstraint.GetErrorRecord(readOnlyCollection));
			}
		}
	}

	private void CheckImplicationKeyConstraints(SchemaConstraints<ViewKeyConstraint> leftViewConstraints, SchemaConstraints<ViewKeyConstraint> rightViewConstraints)
	{
		foreach (ViewKeyConstraint keyConstraint in rightViewConstraints.KeyConstraints)
		{
			bool flag = false;
			foreach (ViewKeyConstraint keyConstraint2 in leftViewConstraints.KeyConstraints)
			{
				if (keyConstraint2.Implies(keyConstraint))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_errorLog.AddEntry(ViewKeyConstraint.GetErrorRecord(keyConstraint));
			}
		}
	}

	private bool CheckCellsWithDistinctFlag()
	{
		int count = m_errorLog.Count;
		foreach (Cell cell in m_cells)
		{
			if (cell.SQuery.SelectDistinctFlag == CellQuery.SelectDistinct.Yes)
			{
				EntitySetBase cExtent = cell.CQuery.Extent;
				EntitySetBase sExtent = cell.SQuery.Extent;
				IEnumerable<Cell> enumerable = from otherCell in m_cells
					where otherCell != cell
					where otherCell.CQuery.Extent == cExtent && otherCell.SQuery.Extent == sExtent
					select otherCell;
				if (enumerable.Any())
				{
					IEnumerable<Cell> sourceCells = Enumerable.Repeat(cell, 1).Union(enumerable);
					ErrorLog.Record record = new ErrorLog.Record(ViewGenErrorCode.MultipleFragmentsBetweenCandSExtentWithDistinct, Strings.Viewgen_MultipleFragmentsBetweenCandSExtentWithDistinct(cExtent.Name, sExtent.Name), sourceCells, string.Empty);
					m_errorLog.AddEntry(record);
				}
			}
		}
		return m_errorLog.Count == count;
	}

	private bool PerformSingleCellChecks()
	{
		int count = m_errorLog.Count;
		foreach (Cell cell in m_cells)
		{
			ErrorLog.Record record = cell.SQuery.CheckForDuplicateFields(cell.CQuery, cell);
			if (record != null)
			{
				m_errorLog.AddEntry(record);
			}
			record = cell.CQuery.VerifyKeysPresent(cell, Strings.ViewGen_EntitySetKey_Missing, Strings.ViewGen_AssociationSetKey_Missing, ViewGenErrorCode.KeyNotMappedForCSideExtent);
			if (record != null)
			{
				m_errorLog.AddEntry(record);
			}
			record = cell.SQuery.VerifyKeysPresent(cell, Strings.ViewGen_TableKey_Missing, null, ViewGenErrorCode.KeyNotMappedForTable);
			if (record != null)
			{
				m_errorLog.AddEntry(record);
			}
			record = cell.CQuery.CheckForProjectedNotNullSlots(cell, m_cells.Where((Cell c) => c.SQuery.Extent is AssociationSet));
			if (record != null)
			{
				m_errorLog.AddEntry(record);
			}
			record = cell.SQuery.CheckForProjectedNotNullSlots(cell, m_cells.Where((Cell c) => c.CQuery.Extent is AssociationSet));
			if (record != null)
			{
				m_errorLog.AddEntry(record);
			}
		}
		return m_errorLog.Count == count;
	}

	[Conditional("DEBUG")]
	private static void CheckConstraintSanity(SchemaConstraints<BasicKeyConstraint> cConstraints, SchemaConstraints<BasicKeyConstraint> sConstraints, SchemaConstraints<ViewKeyConstraint> cViewConstraints, SchemaConstraints<ViewKeyConstraint> sViewConstraints)
	{
	}
}
