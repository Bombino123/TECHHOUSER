using System.Data.Common;
using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.Migrations.History;

public class HistoryContext : DbContext, IDbModelCacheKeyProvider
{
	public const string DefaultTableName = "__MigrationHistory";

	internal const int ContextKeyMaxLength = 300;

	internal const int MigrationIdMaxLength = 150;

	private readonly string _defaultSchema;

	internal static readonly Func<DbConnection, string, HistoryContext> DefaultFactory = (DbConnection e, string d) => new HistoryContext(e, d);

	public virtual string CacheKey => _defaultSchema;

	protected string DefaultSchema => _defaultSchema;

	public virtual IDbSet<HistoryRow> History { get; set; }

	internal HistoryContext()
	{
		InternalContext.InitializerDisabled = true;
	}

	public HistoryContext(DbConnection existingConnection, string defaultSchema)
		: base(existingConnection, contextOwnsConnection: false)
	{
		_defaultSchema = defaultSchema;
		base.Configuration.ValidateOnSaveEnabled = false;
		InternalContext.InitializerDisabled = true;
	}

	protected override void OnModelCreating(DbModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(_defaultSchema);
		modelBuilder.Entity<HistoryRow>().ToTable("__MigrationHistory");
		modelBuilder.Entity<HistoryRow>().HasKey((HistoryRow h) => new { h.MigrationId, h.ContextKey });
		modelBuilder.Entity<HistoryRow>().Property((HistoryRow h) => h.MigrationId).HasMaxLength(150)
			.IsRequired();
		modelBuilder.Entity<HistoryRow>().Property((HistoryRow h) => h.ContextKey).HasMaxLength(300)
			.IsRequired();
		modelBuilder.Entity<HistoryRow>().Property((HistoryRow h) => h.Model).IsRequired()
			.IsMaxLength();
		modelBuilder.Entity<HistoryRow>().Property((HistoryRow h) => h.ProductVersion).HasMaxLength(32)
			.IsRequired();
	}
}
