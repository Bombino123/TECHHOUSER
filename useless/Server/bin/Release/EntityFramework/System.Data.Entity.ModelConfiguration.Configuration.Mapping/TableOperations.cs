using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping;

internal static class TableOperations
{
	public static EdmProperty CopyColumnAndAnyConstraints(EdmModel database, EntityType fromTable, EntityType toTable, EdmProperty column, Func<EdmProperty, bool> isCompatible, bool useExisting)
	{
		EdmProperty edmProperty = column;
		if (fromTable != toTable)
		{
			edmProperty = TablePrimitiveOperations.IncludeColumn(toTable, column, isCompatible, useExisting);
			if (!edmProperty.IsPrimaryKeyColumn)
			{
				ForeignKeyPrimitiveOperations.CopyAllForeignKeyConstraintsForColumn(database, fromTable, toTable, column, edmProperty);
			}
		}
		return edmProperty;
	}

	public static EdmProperty MoveColumnAndAnyConstraints(EntityType fromTable, EntityType toTable, EdmProperty column, bool useExisting)
	{
		EdmProperty result = column;
		if (fromTable != toTable)
		{
			result = TablePrimitiveOperations.IncludeColumn(toTable, column, TablePrimitiveOperations.GetNameMatcher(column.Name), useExisting);
			TablePrimitiveOperations.RemoveColumn(fromTable, column);
			ForeignKeyPrimitiveOperations.MoveAllForeignKeyConstraintsForColumn(fromTable, toTable, column);
		}
		return result;
	}
}
