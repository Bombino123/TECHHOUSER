using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Migrations.Model;

public class AddColumnOperation : MigrationOperation, IAnnotationTarget
{
	private readonly string _table;

	private readonly ColumnModel _column;

	public string Table => _table;

	public ColumnModel Column => _column;

	public override MigrationOperation Inverse => new DropColumnOperation(Table, Column.Name, Column.Annotations.ToDictionary((KeyValuePair<string, AnnotationValues> a) => a.Key, (KeyValuePair<string, AnnotationValues> a) => a.Value.NewValue));

	public override bool IsDestructiveChange => false;

	bool IAnnotationTarget.HasAnnotations => Column.Annotations.Any();

	public AddColumnOperation(string table, ColumnModel column, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(table, "table");
		Check.NotNull(column, "column");
		_table = table;
		_column = column;
	}
}
