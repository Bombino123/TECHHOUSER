namespace System.Data.Entity.Migrations.History;

public class HistoryRow
{
	public string MigrationId { get; set; }

	public string ContextKey { get; set; }

	public byte[] Model { get; set; }

	public string ProductVersion { get; set; }
}
