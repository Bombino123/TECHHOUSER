using System.Data.Common;

namespace System.Data.Entity.Migrations.History;

internal sealed class LegacyHistoryContext : DbContext
{
	public IDbSet<LegacyHistoryRow> History { get; set; }

	public LegacyHistoryContext(DbConnection existingConnection)
		: base(existingConnection, contextOwnsConnection: false)
	{
		InternalContext.InitializerDisabled = true;
	}
}
