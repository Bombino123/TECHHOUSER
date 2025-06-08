using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures;

internal class Cell : InternalBase
{
	private readonly CellQuery m_cQuery;

	private readonly CellQuery m_sQuery;

	private readonly int m_cellNumber;

	private readonly CellLabel m_label;

	private ViewCellRelation m_viewCellRelation;

	internal CellQuery CQuery => m_cQuery;

	internal CellQuery SQuery => m_sQuery;

	internal CellLabel CellLabel => m_label;

	internal int CellNumber => m_cellNumber;

	internal string CellNumberAsString => StringUtil.FormatInvariant("V{0}", CellNumber);

	private Cell(CellQuery cQuery, CellQuery sQuery, CellLabel label, int cellNumber)
	{
		m_cQuery = cQuery;
		m_sQuery = sQuery;
		m_label = label;
		m_cellNumber = cellNumber;
	}

	internal Cell(Cell source)
	{
		m_cQuery = new CellQuery(source.m_cQuery);
		m_sQuery = new CellQuery(source.m_sQuery);
		m_label = new CellLabel(source.m_label);
		m_cellNumber = source.m_cellNumber;
	}

	internal void GetIdentifiers(CqlIdentifiers identifiers)
	{
		m_cQuery.GetIdentifiers(identifiers);
		m_sQuery.GetIdentifiers(identifiers);
	}

	internal Set<EdmProperty> GetCSlotsForTableColumns(IEnumerable<MemberPath> columns)
	{
		List<int> projectedPositions = SQuery.GetProjectedPositions(columns);
		if (projectedPositions == null)
		{
			return null;
		}
		Set<EdmProperty> set = new Set<EdmProperty>();
		foreach (int item in projectedPositions)
		{
			if (CQuery.ProjectedSlotAt(item) is MemberProjectedSlot memberProjectedSlot)
			{
				set.Add((EdmProperty)memberProjectedSlot.MemberPath.LeafEdmMember);
				continue;
			}
			return null;
		}
		return set;
	}

	internal CellQuery GetLeftQuery(ViewTarget side)
	{
		if (side != 0)
		{
			return m_sQuery;
		}
		return m_cQuery;
	}

	internal CellQuery GetRightQuery(ViewTarget side)
	{
		if (side != 0)
		{
			return m_cQuery;
		}
		return m_sQuery;
	}

	internal ViewCellRelation CreateViewCellRelation(int cellNumber)
	{
		if (m_viewCellRelation != null)
		{
			return m_viewCellRelation;
		}
		GenerateCellRelations(cellNumber);
		return m_viewCellRelation;
	}

	private void GenerateCellRelations(int cellNumber)
	{
		List<ViewCellSlot> list = new List<ViewCellSlot>();
		for (int i = 0; i < CQuery.NumProjectedSlots; i++)
		{
			ProjectedSlot projectedSlot = CQuery.ProjectedSlotAt(i);
			ProjectedSlot projectedSlot2 = SQuery.ProjectedSlotAt(i);
			MemberProjectedSlot cSlot = (MemberProjectedSlot)projectedSlot;
			MemberProjectedSlot sSlot = (MemberProjectedSlot)projectedSlot2;
			ViewCellSlot item = new ViewCellSlot(i, cSlot, sSlot);
			list.Add(item);
		}
		m_viewCellRelation = new ViewCellRelation(this, list, cellNumber);
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		CQuery.ToCompactString(builder);
		builder.Append(" = ");
		SQuery.ToCompactString(builder);
	}

	internal override void ToFullString(StringBuilder builder)
	{
		CQuery.ToFullString(builder);
		builder.Append(" = ");
		SQuery.ToFullString(builder);
	}

	public override string ToString()
	{
		return ToFullString();
	}

	internal static void CellsToBuilder(StringBuilder builder, IEnumerable<Cell> cells)
	{
		builder.AppendLine();
		builder.AppendLine("=========================================================================");
		foreach (Cell cell in cells)
		{
			builder.AppendLine();
			StringUtil.FormatStringBuilder(builder, "Mapping Cell V{0}:", cell.CellNumber);
			builder.AppendLine();
			builder.Append("C: ");
			cell.CQuery.ToFullString(builder);
			builder.AppendLine();
			builder.AppendLine();
			builder.Append("S: ");
			cell.SQuery.ToFullString(builder);
			builder.AppendLine();
		}
	}

	internal static Cell CreateCS(CellQuery cQuery, CellQuery sQuery, CellLabel label, int cellNumber)
	{
		return new Cell(cQuery, sQuery, label, cellNumber);
	}
}
