using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.SQLite.EF6;

internal sealed class JoinSymbol : Symbol
{
	private List<Symbol> columnList;

	private List<Symbol> extentList;

	private List<Symbol> flattenedExtentList;

	private Dictionary<string, Symbol> nameToExtent;

	private bool isNestedJoin;

	internal List<Symbol> ColumnList
	{
		get
		{
			if (columnList == null)
			{
				columnList = new List<Symbol>();
			}
			return columnList;
		}
		set
		{
			columnList = value;
		}
	}

	internal List<Symbol> ExtentList => extentList;

	internal List<Symbol> FlattenedExtentList
	{
		get
		{
			if (flattenedExtentList == null)
			{
				flattenedExtentList = new List<Symbol>();
			}
			return flattenedExtentList;
		}
		set
		{
			flattenedExtentList = value;
		}
	}

	internal Dictionary<string, Symbol> NameToExtent => nameToExtent;

	internal bool IsNestedJoin
	{
		get
		{
			return isNestedJoin;
		}
		set
		{
			isNestedJoin = value;
		}
	}

	public JoinSymbol(string name, TypeUsage type, List<Symbol> extents)
		: base(name, type)
	{
		extentList = new List<Symbol>(extents.Count);
		nameToExtent = new Dictionary<string, Symbol>(extents.Count, StringComparer.OrdinalIgnoreCase);
		foreach (Symbol extent in extents)
		{
			nameToExtent[extent.Name] = extent;
			ExtentList.Add(extent);
		}
	}
}
