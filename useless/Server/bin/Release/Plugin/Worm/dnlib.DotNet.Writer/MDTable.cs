using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.MD;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public sealed class MDTable<TRow> : IMDTable where TRow : struct
{
	private readonly Table table;

	private readonly Dictionary<TRow, uint> cachedDict;

	private readonly List<TRow> cached;

	private TableInfo tableInfo;

	private bool isSorted;

	private bool isReadOnly;

	public Table Table => table;

	public bool IsEmpty => cached.Count == 0;

	public int Rows => cached.Count;

	public bool IsSorted
	{
		get
		{
			return isSorted;
		}
		set
		{
			isSorted = value;
		}
	}

	public bool IsReadOnly => isReadOnly;

	public TableInfo TableInfo
	{
		get
		{
			return tableInfo;
		}
		set
		{
			tableInfo = value;
		}
	}

	public TRow this[uint rid]
	{
		get
		{
			return cached[(int)(rid - 1)];
		}
		set
		{
			cached[(int)(rid - 1)] = value;
		}
	}

	public MDTable(Table table, IEqualityComparer<TRow> equalityComparer)
	{
		this.table = table;
		cachedDict = new Dictionary<TRow, uint>(equalityComparer);
		cached = new List<TRow>();
	}

	public void SetReadOnly()
	{
		isReadOnly = true;
	}

	public uint Add(TRow row)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException($"Trying to modify table {table} after it's been set to read-only");
		}
		if (cachedDict.TryGetValue(row, out var value))
		{
			return value;
		}
		return Create(row);
	}

	public uint Create(TRow row)
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException($"Trying to modify table {table} after it's been set to read-only");
		}
		uint num = (uint)(cached.Count + 1);
		if (!cachedDict.ContainsKey(row))
		{
			cachedDict[row] = num;
		}
		cached.Add(row);
		return num;
	}

	public void ReAddRows()
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException($"Trying to modify table {table} after it's been set to read-only");
		}
		cachedDict.Clear();
		for (int i = 0; i < cached.Count; i++)
		{
			uint value = (uint)(i + 1);
			TRow key = cached[i];
			if (!cachedDict.ContainsKey(key))
			{
				cachedDict[key] = value;
			}
		}
	}

	public void Reset()
	{
		if (isReadOnly)
		{
			throw new ModuleWriterException($"Trying to modify table {table} after it's been set to read-only");
		}
		cachedDict.Clear();
		cached.Clear();
	}
}
