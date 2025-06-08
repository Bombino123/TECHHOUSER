using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Internal;

internal interface IInternalConnection : IDisposable
{
	DbConnection Connection { get; }

	string ConnectionKey { get; }

	bool ConnectionHasModel { get; }

	DbConnectionStringOrigin ConnectionStringOrigin { get; }

	AppConfig AppConfig { get; set; }

	string ProviderName { get; set; }

	string ConnectionStringName { get; }

	string OriginalConnectionString { get; }

	ObjectContext CreateObjectContextFromConnectionModel();
}
