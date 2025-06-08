using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal static class TablePrimitiveOperations
{
	public static void AddColumn(EntityType table, EdmProperty column)
	{
		if (!table.Properties.Contains(column))
		{
			if (!(column.GetConfiguration() is System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration primitivePropertyConfiguration) || string.IsNullOrWhiteSpace(primitivePropertyConfiguration.ColumnName))
			{
				string name = column.GetPreferredName() ?? column.Name;
				column.SetUnpreferredUniqueName(column.Name);
				column.Name = table.Properties.UniquifyName(name);
			}
			table.AddMember(column);
		}
	}

	public static EdmProperty RemoveColumn(EntityType table, EdmProperty column)
	{
		if (!column.IsPrimaryKeyColumn)
		{
			table.RemoveMember(column);
		}
		return column;
	}

	public static EdmProperty IncludeColumn(EntityType table, EdmProperty templateColumn, Func<EdmProperty, bool> isCompatible, bool useExisting)
	{
		EdmProperty edmProperty = table.Properties.FirstOrDefault(isCompatible);
		templateColumn = ((edmProperty == null) ? templateColumn.Clone() : ((useExisting || edmProperty.IsPrimaryKeyColumn) ? edmProperty : templateColumn.Clone()));
		AddColumn(table, templateColumn);
		return templateColumn;
	}

	public static Func<EdmProperty, bool> GetNameMatcher(string name)
	{
		return (EdmProperty c) => string.Equals(c.Name, name, StringComparison.Ordinal);
	}
}
