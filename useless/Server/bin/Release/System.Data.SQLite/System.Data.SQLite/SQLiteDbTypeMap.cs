using System.Collections.Generic;

namespace System.Data.SQLite;

internal sealed class SQLiteDbTypeMap : Dictionary<string, SQLiteDbTypeMapping>
{
	private Dictionary<DbType, SQLiteDbTypeMapping> reverse;

	public SQLiteDbTypeMap()
		: base((IEqualityComparer<string>?)new TypeNameStringComparer())
	{
		reverse = new Dictionary<DbType, SQLiteDbTypeMapping>();
	}

	public SQLiteDbTypeMap(IEnumerable<SQLiteDbTypeMapping> collection)
		: this()
	{
		Add(collection);
	}

	public new int Clear()
	{
		int num = 0;
		if (reverse != null)
		{
			num += reverse.Count;
			reverse.Clear();
		}
		num += base.Count;
		base.Clear();
		return num;
	}

	public void Add(IEnumerable<SQLiteDbTypeMapping> collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		foreach (SQLiteDbTypeMapping item in collection)
		{
			Add(item);
		}
	}

	public void Add(SQLiteDbTypeMapping item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (item.typeName == null)
		{
			throw new ArgumentException("item type name cannot be null");
		}
		Add(item.typeName, item);
		if (item.primary)
		{
			reverse.Add(item.dataType, item);
		}
	}

	public bool ContainsKey(DbType key)
	{
		if (reverse == null)
		{
			return false;
		}
		return reverse.ContainsKey(key);
	}

	public bool TryGetValue(DbType key, out SQLiteDbTypeMapping value)
	{
		if (reverse == null)
		{
			value = null;
			return false;
		}
		return reverse.TryGetValue(key, out value);
	}

	public bool Remove(DbType key)
	{
		if (reverse == null)
		{
			return false;
		}
		return reverse.Remove(key);
	}
}
