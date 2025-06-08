using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace System.Data.Entity.Internal;

internal class EdmMetadataRepository : RepositoryBase
{
	private readonly DbTransaction _existingTransaction;

	public EdmMetadataRepository(InternalContext usersContext, string connectionString, DbProviderFactory providerFactory)
		: base(usersContext, connectionString, providerFactory)
	{
		_existingTransaction = usersContext.TryGetCurrentStoreTransaction();
	}

	public virtual string QueryForModelHash(Func<DbConnection, EdmMetadataContext> createContext)
	{
		DbConnection dbConnection = CreateConnection();
		try
		{
			using EdmMetadataContext edmMetadataContext = createContext(dbConnection);
			if (_existingTransaction != null && _existingTransaction.Connection == dbConnection)
			{
				edmMetadataContext.Database.UseTransaction(_existingTransaction);
			}
			try
			{
				return (from m in edmMetadataContext.Metadata.AsNoTracking()
					orderby m.Id descending
					select m).FirstOrDefault()?.ModelHash;
			}
			catch (EntityCommandExecutionException)
			{
				return null;
			}
		}
		finally
		{
			DisposeConnection(dbConnection);
		}
	}
}
