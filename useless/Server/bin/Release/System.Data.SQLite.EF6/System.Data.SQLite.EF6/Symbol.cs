using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;

namespace System.Data.SQLite.EF6;

internal class Symbol : ISqlFragment
{
	private Dictionary<string, Symbol> columns = new Dictionary<string, Symbol>(StringComparer.CurrentCultureIgnoreCase);

	private bool needsRenaming;

	private bool isUnnest;

	private string name;

	private string newName;

	private TypeUsage type;

	internal Dictionary<string, Symbol> Columns => columns;

	internal bool NeedsRenaming
	{
		get
		{
			return needsRenaming;
		}
		set
		{
			needsRenaming = value;
		}
	}

	internal bool IsUnnest
	{
		get
		{
			return isUnnest;
		}
		set
		{
			isUnnest = value;
		}
	}

	public string Name => name;

	public string NewName
	{
		get
		{
			return newName;
		}
		set
		{
			newName = value;
		}
	}

	internal TypeUsage Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}

	public Symbol(string name, TypeUsage type)
	{
		this.name = name;
		newName = name;
		Type = type;
	}

	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		if (NeedsRenaming)
		{
			int num = sqlGenerator.AllColumnNames[NewName];
			string key;
			do
			{
				num++;
				key = Name + num.ToString(CultureInfo.InvariantCulture);
			}
			while (sqlGenerator.AllColumnNames.ContainsKey(key));
			sqlGenerator.AllColumnNames[NewName] = num;
			NeedsRenaming = false;
			NewName = key;
			sqlGenerator.AllColumnNames[key] = 0;
		}
		writer.Write(SqlGenerator.QuoteIdentifier(NewName));
	}
}
