using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Infrastructure;

public abstract class TableExistenceChecker
{
	public abstract bool AnyModelTableExistsInDatabase(ObjectContext context, DbConnection connection, IEnumerable<EntitySet> modelTables, string edmMetadataContextTableName);

	protected virtual string GetTableName(EntitySet modelTable)
	{
		if (!modelTable.MetadataProperties.Contains("Table") || modelTable.MetadataProperties["Table"].Value == null)
		{
			return modelTable.Name;
		}
		return (string)modelTable.MetadataProperties["Table"].Value;
	}
}
