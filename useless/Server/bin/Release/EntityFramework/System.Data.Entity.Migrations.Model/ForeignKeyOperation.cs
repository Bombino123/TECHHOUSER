using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Migrations.Model;

public abstract class ForeignKeyOperation : MigrationOperation
{
	private string _principalTable;

	private string _dependentTable;

	private readonly List<string> _dependentColumns = new List<string>();

	private string _name;

	public string PrincipalTable
	{
		get
		{
			return _principalTable;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_principalTable = value;
		}
	}

	public string DependentTable
	{
		get
		{
			return _dependentTable;
		}
		set
		{
			Check.NotEmpty(value, "value");
			_dependentTable = value;
		}
	}

	public IList<string> DependentColumns => _dependentColumns;

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

	internal string DefaultName => string.Format(CultureInfo.InvariantCulture, "FK_{0}_{1}_{2}", new object[3]
	{
		DependentTable,
		PrincipalTable,
		DependentColumns.Join(null, "_")
	}).RestrictTo(128);

	protected ForeignKeyOperation(object anonymousArguments = null)
		: base(anonymousArguments)
	{
	}
}
