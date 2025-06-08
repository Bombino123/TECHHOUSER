using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Linq;
using System.Transactions;

namespace System.Data.Entity.Internal;

internal class DatabaseTableChecker
{
	public DatabaseExistenceState AnyModelTableExists(InternalContext internalContext)
	{
		if (!internalContext.DatabaseOperations.Exists(internalContext.Connection, internalContext.CommandTimeout, new Lazy<StoreItemCollection>(() => CreateStoreItemCollection(internalContext))))
		{
			return DatabaseExistenceState.DoesNotExist;
		}
		using ClonedObjectContext clonedObjectContext = internalContext.CreateObjectContextForDdlOps();
		try
		{
			if (internalContext.CodeFirstModel == null)
			{
				return DatabaseExistenceState.Exists;
			}
			TableExistenceChecker service = DbConfiguration.DependencyResolver.GetService<TableExistenceChecker>(internalContext.ProviderName);
			if (service == null)
			{
				return DatabaseExistenceState.Exists;
			}
			List<EntitySet> list = GetModelTables(internalContext).ToList();
			if (!list.Any())
			{
				return DatabaseExistenceState.Exists;
			}
			if (QueryForTableExistence(service, clonedObjectContext, list))
			{
				return DatabaseExistenceState.Exists;
			}
			return internalContext.HasHistoryTableEntry() ? DatabaseExistenceState.Exists : DatabaseExistenceState.ExistsConsideredEmpty;
		}
		catch (Exception)
		{
			return DatabaseExistenceState.Exists;
		}
	}

	private static StoreItemCollection CreateStoreItemCollection(InternalContext internalContext)
	{
		using ClonedObjectContext clonedObjectContext = internalContext.CreateObjectContextForDdlOps();
		return (StoreItemCollection)((EntityConnection)clonedObjectContext.ObjectContext.Connection).GetMetadataWorkspace().GetItemCollection(DataSpace.SSpace);
	}

	public virtual bool QueryForTableExistence(TableExistenceChecker checker, ClonedObjectContext clonedObjectContext, List<EntitySet> modelTables)
	{
		using (new TransactionScope(TransactionScopeOption.Suppress))
		{
			if (checker.AnyModelTableExistsInDatabase(clonedObjectContext.ObjectContext, clonedObjectContext.Connection, modelTables, "EdmMetadata"))
			{
				return true;
			}
		}
		return false;
	}

	public virtual IEnumerable<EntitySet> GetModelTables(InternalContext internalContext)
	{
		return from s in internalContext.ObjectContext.MetadataWorkspace.GetItemCollection(DataSpace.SSpace).GetItems<EntityContainer>().Single()
				.BaseEntitySets.OfType<EntitySet>()
			where !s.MetadataProperties.Contains("Type") || (string)s.MetadataProperties["Type"].Value == "Tables"
			select s;
	}
}
