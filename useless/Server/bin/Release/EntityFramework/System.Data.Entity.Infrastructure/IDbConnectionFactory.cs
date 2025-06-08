using System.Data.Common;

namespace System.Data.Entity.Infrastructure;

public interface IDbConnectionFactory
{
	DbConnection CreateConnection(string nameOrConnectionString);
}
