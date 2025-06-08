using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Migrations.Model;

public class AlterColumnOperation : MigrationOperation, IAnnotationTarget
{
	private readonly string _table;

	private readonly ColumnModel _column;

	private readonly AlterColumnOperation _inverse;

	private readonly bool _destructiveChange;

	public string Table => _table;

	public ColumnModel Column => _column;

	public override MigrationOperation Inverse => _inverse;

	public override bool IsDestructiveChange => _destructiveChange;

	bool IAnnotationTarget.HasAnnotations
	{
		get
		{
			AlterColumnOperation alterColumnOperation = Inverse as AlterColumnOperation;
			if (!Column.Annotations.Any())
			{
				return alterColumnOperation?.Column.Annotations.Any() ?? false;
			}
			return true;
		}
	}

	public AlterColumnOperation(string table, ColumnModel column, bool isDestructiveChange, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(table, "table");
		Check.NotNull(column, "column");
		_table = table;
		_column = column;
		_destructiveChange = isDestructiveChange;
	}

	public AlterColumnOperation(string table, ColumnModel column, bool isDestructiveChange, AlterColumnOperation inverse, object anonymousArguments = null)
		: this(table, column, isDestructiveChange, anonymousArguments)
	{
		Check.NotNull(inverse, "inverse");
		_inverse = inverse;
	}
}
