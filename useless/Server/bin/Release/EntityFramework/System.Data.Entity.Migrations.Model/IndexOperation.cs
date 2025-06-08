using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Migrations.Model;

public abstract class IndexOperation : MigrationOperation
{
	private string _table;

	private readonly List<string> _columns = new List<string>();

	private string _name;

	public string Table
	{
		get
		{
			return _table;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_table = value;
		}
	}

	public IList<string> Columns => _columns;

	public bool HasDefaultName => string.Equals(Name, DefaultName, StringComparison.Ordinal);

	public string Name
	{
		get
		{
			return _name ?? DefaultName;
		}
		set
		{
			_name = value;
		}
	}

	internal string DefaultName => BuildDefaultName(Columns);

	public static string BuildDefaultName(IEnumerable<string> columns)
	{
		Check.NotNull(columns, "columns");
		return string.Format(CultureInfo.InvariantCulture, "IX_{0}", new object[1] { columns.Join(null, "_") }).RestrictTo(128);
	}

	protected IndexOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
	}
}
