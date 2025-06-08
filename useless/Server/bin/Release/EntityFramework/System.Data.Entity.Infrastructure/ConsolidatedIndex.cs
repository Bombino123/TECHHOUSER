using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Resources;
using System.Linq;

namespace System.Data.Entity.Infrastructure;

internal class ConsolidatedIndex
{
	private readonly string _table;

	private IndexAttribute _index;

	private readonly IDictionary<int, string> _columns = new Dictionary<int, string>();

	public IndexAttribute Index => _index;

	public IEnumerable<string> Columns => from c in _columns
		orderby c.Key
		select c.Value;

	public string Table => _table;

	public ConsolidatedIndex(string table, IndexAttribute index)
	{
		_table = table;
		_index = index;
	}

	public ConsolidatedIndex(string table, string column, IndexAttribute index)
		: this(table, index)
	{
		_columns[index.Order] = column;
	}

	public static IEnumerable<ConsolidatedIndex> BuildIndexes(string tableName, IEnumerable<Tuple<string, EdmProperty>> columns)
	{
		List<ConsolidatedIndex> list = new List<ConsolidatedIndex>();
		foreach (Tuple<string, EdmProperty> column in columns)
		{
			foreach (IndexAttribute index in (from a in column.Item2.Annotations
				where a.Name == "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:Index"
				select a.Value).OfType<IndexAnnotation>().SelectMany((IndexAnnotation a) => a.Indexes))
			{
				ConsolidatedIndex consolidatedIndex = ((index.Name == null) ? null : list.FirstOrDefault((ConsolidatedIndex i) => i.Index.Name == index.Name));
				if (consolidatedIndex == null)
				{
					list.Add(new ConsolidatedIndex(tableName, column.Item1, index));
				}
				else
				{
					consolidatedIndex.Add(column.Item1, index);
				}
			}
		}
		return list;
	}

	public void Add(string columnName, IndexAttribute index)
	{
		if (_columns.ContainsKey(index.Order))
		{
			throw new InvalidOperationException(Strings.OrderConflictWhenConsolidating(index.Name, _table, index.Order, _columns[index.Order], columnName));
		}
		_columns[index.Order] = columnName;
		CompatibilityResult compatibilityResult = _index.IsCompatibleWith(index, ignoreOrder: true);
		if (!compatibilityResult)
		{
			throw new InvalidOperationException(Strings.ConflictWhenConsolidating(index.Name, _table, compatibilityResult.ErrorMessage));
		}
		_index = _index.MergeWith(index, ignoreOrder: true);
	}

	public CreateIndexOperation CreateCreateIndexOperation()
	{
		string[] array = Columns.ToArray();
		CreateIndexOperation createIndexOperation = new CreateIndexOperation
		{
			Name = (_index.Name ?? IndexOperation.BuildDefaultName(array)),
			Table = _table
		};
		string[] array2 = array;
		foreach (string item in array2)
		{
			createIndexOperation.Columns.Add(item);
		}
		if (_index.IsClusteredConfigured)
		{
			createIndexOperation.IsClustered = _index.IsClustered;
		}
		if (_index.IsUniqueConfigured)
		{
			createIndexOperation.IsUnique = _index.IsUnique;
		}
		return createIndexOperation;
	}

	public DropIndexOperation CreateDropIndexOperation()
	{
		return (DropIndexOperation)CreateCreateIndexOperation().Inverse;
	}
}
