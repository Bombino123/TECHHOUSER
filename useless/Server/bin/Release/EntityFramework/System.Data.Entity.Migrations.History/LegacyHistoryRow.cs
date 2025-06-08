using System.ComponentModel.DataAnnotations.Schema;

namespace System.Data.Entity.Migrations.History;

[Table("__MigrationHistory")]
internal sealed class LegacyHistoryRow
{
	public int Id { get; set; }

	public DateTime CreatedOn { get; set; }
}
