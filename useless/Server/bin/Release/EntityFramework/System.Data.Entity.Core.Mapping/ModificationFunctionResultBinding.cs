using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Mapping;

public sealed class ModificationFunctionResultBinding : MappingItem
{
	private string _columnName;

	private readonly EdmProperty _property;

	public string ColumnName
	{
		get
		{
			return _columnName;
		}
		internal set
		{
			_columnName = value;
		}
	}

	public EdmProperty Property => _property;

	public ModificationFunctionResultBinding(string columnName, EdmProperty property)
	{
		Check.NotNull(columnName, "columnName");
		Check.NotNull(property, "property");
		_columnName = columnName;
		_property = property;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}->{1}", new object[2] { ColumnName, Property });
	}
}
