using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.Update.Internal;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class CellPartitioner : InternalBase
{
	private readonly IEnumerable<Cell> m_cells;

	private readonly IEnumerable<ForeignConstraint> m_foreignKeyConstraints;

	internal CellPartitioner(IEnumerable<Cell> cells, IEnumerable<ForeignConstraint> foreignKeyConstraints)
	{
		m_foreignKeyConstraints = foreignKeyConstraints;
		m_cells = cells;
	}

	internal List<Set<Cell>> GroupRelatedCells()
	{
		UndirectedGraph<EntitySetBase> undirectedGraph = new UndirectedGraph<EntitySetBase>(EqualityComparer<EntitySetBase>.Default);
		Dictionary<EntitySetBase, Set<Cell>> extentToCell = new Dictionary<EntitySetBase, Set<Cell>>(EqualityComparer<EntitySetBase>.Default);
		foreach (Cell cell in m_cells)
		{
			EntitySetBase[] array = new EntitySetBase[2]
			{
				cell.CQuery.Extent,
				cell.SQuery.Extent
			};
			foreach (EntitySetBase entitySetBase in array)
			{
				if (!extentToCell.TryGetValue(entitySetBase, out var value))
				{
					value = (extentToCell[entitySetBase] = new Set<Cell>());
				}
				value.Add(cell);
				undirectedGraph.AddVertex(entitySetBase);
			}
			undirectedGraph.AddEdge(cell.CQuery.Extent, cell.SQuery.Extent);
			if (!(cell.CQuery.Extent is AssociationSet associationSet))
			{
				continue;
			}
			foreach (AssociationSetEnd associationSetEnd in associationSet.AssociationSetEnds)
			{
				undirectedGraph.AddEdge(associationSetEnd.EntitySet, associationSet);
			}
		}
		foreach (ForeignConstraint foreignKeyConstraint in m_foreignKeyConstraints)
		{
			undirectedGraph.AddEdge(foreignKeyConstraint.ChildTable, foreignKeyConstraint.ParentTable);
		}
		KeyToListMap<int, EntitySetBase> keyToListMap = undirectedGraph.GenerateConnectedComponents();
		List<Set<Cell>> list = new List<Set<Cell>>();
		foreach (int key in keyToListMap.Keys)
		{
			IEnumerable<Set<Cell>> enumerable = from e in keyToListMap.ListForKey(key)
				select extentToCell[e];
			Set<Cell> set2 = new Set<Cell>();
			foreach (Set<Cell> item in enumerable)
			{
				set2.AddRange(item);
			}
			list.Add(set2);
		}
		return list;
	}

	internal override void ToCompactString(StringBuilder builder)
	{
		Cell.CellsToBuilder(builder, m_cells);
	}
}
