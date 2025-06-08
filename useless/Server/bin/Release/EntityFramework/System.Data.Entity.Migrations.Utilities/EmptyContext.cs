using System.Data.Common;

namespace System.Data.Entity.Migrations.Utilities;

internal class EmptyContext : DbContext
{
	public EmptyContext(DbConnection existingConnection)
		: base(existingConnection, contextOwnsConnection: false)
	{
		InternalContext.InitializerDisabled = true;
	}
}
