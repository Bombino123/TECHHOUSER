using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Globalization;
using System.IO;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class Symbol : ISqlFragment
{
	private Dictionary<string, Symbol> columns;

	private Dictionary<string, Symbol> outputColumns;

	private readonly string name;

	internal Dictionary<string, Symbol> Columns
	{
		get
		{
			if (columns == null)
			{
				columns = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
			}
			return columns;
		}
	}

	internal Dictionary<string, Symbol> OutputColumns
	{
		get
		{
			if (outputColumns == null)
			{
				outputColumns = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
			}
			return outputColumns;
		}
	}

	internal bool NeedsRenaming { get; set; }

	internal bool OutputColumnsRenamed { get; set; }

	public string Name => name;

	public string NewName { get; set; }

	internal TypeUsage Type { get; set; }

	public Symbol(string name, TypeUsage type)
	{
		this.name = name;
		NewName = name;
		Type = type;
	}

	public Symbol(string name, TypeUsage type, Dictionary<string, Symbol> outputColumns, bool outputColumnsRenamed)
	{
		this.name = name;
		NewName = name;
		Type = type;
		this.outputColumns = outputColumns;
		OutputColumnsRenamed = outputColumnsRenamed;
	}

	public void WriteSql(SqlWriter writer, SqlGenerator sqlGenerator)
	{
		if (NeedsRenaming)
		{
			if (sqlGenerator.AllColumnNames.TryGetValue(NewName, out var value))
			{
				string text;
				do
				{
					value++;
					text = NewName + value.ToString(CultureInfo.InvariantCulture);
				}
				while (sqlGenerator.AllColumnNames.ContainsKey(text));
				sqlGenerator.AllColumnNames[NewName] = value;
				NewName = text;
			}
			sqlGenerator.AllColumnNames[NewName] = 0;
			NeedsRenaming = false;
		}
		((TextWriter)(object)writer).Write(SqlGenerator.QuoteIdentifier(NewName));
	}
}
