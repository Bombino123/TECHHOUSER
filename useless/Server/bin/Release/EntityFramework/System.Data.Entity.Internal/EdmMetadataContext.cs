using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration;

namespace System.Data.Entity.Internal;

internal class EdmMetadataContext : DbContext
{
	public const string TableName = "EdmMetadata";

	public virtual IDbSet<EdmMetadata> Metadata { get; set; }

	static EdmMetadataContext()
	{
		Database.SetInitializer<EdmMetadataContext>(null);
	}

	public EdmMetadataContext(DbConnection existingConnection)
		: base(existingConnection, contextOwnsConnection: false)
	{
	}

	protected override void OnModelCreating(DbModelBuilder modelBuilder)
	{
		ConfigureEdmMetadata(modelBuilder.ModelConfiguration);
	}

	public static void ConfigureEdmMetadata(System.Data.Entity.ModelConfiguration.Configuration.ModelConfiguration modelConfiguration)
	{
		modelConfiguration.Entity(typeof(EdmMetadata)).ToTable("EdmMetadata");
	}
}
