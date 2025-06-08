using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Internal;

internal class DatabaseOperations
{
	public virtual bool Create(ObjectContext objectContext)
	{
		objectContext.CreateDatabase();
		return true;
	}

	public virtual bool Exists(DbConnection connection, int? commandTimeout, Lazy<StoreItemCollection> storeItemCollection)
	{
		if (connection.State == ConnectionState.Open)
		{
			return true;
		}
		try
		{
			return DbProviderServices.GetProviderServices(connection).DatabaseExists(connection, commandTimeout, storeItemCollection);
		}
		catch
		{
			try
			{
				connection.Open();
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				connection.Close();
			}
		}
	}

	public virtual void Delete(ObjectContext objectContext)
	{
		objectContext.DeleteDatabase();
	}
}
