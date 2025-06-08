using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Migrations.Model;

public abstract class PrimaryKeyOperation : MigrationOperation
{
	private readonly List<string> _columns = new List<string>();

	private string _table;

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

	public override bool IsDestructiveChange => false;

	internal string DefaultName => BuildDefaultName(Table);

	public bool IsClustered { get; set; }

	public static string BuildDefaultName(string table)
	{
		Check.NotEmpty(table, "table");
		return string.Format(CultureInfo.InvariantCulture, "PK_{0}", new object[1] { table }).RestrictTo(128);
	}

	protected PrimaryKeyOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
		IsClustered = true;
	}
}
