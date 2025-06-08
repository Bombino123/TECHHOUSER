using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public class ScalarPropertyMapping : PropertyMapping
{
	private EdmProperty _column;

	public EdmProperty Column
	{
		get
		{
			return _column;
		}
		internal set
		{
			_column = value;
		}
	}

	public ScalarPropertyMapping(EdmProperty property, EdmProperty column)
		: base(property)
	{
		Check.NotNull(property, "property");
		Check.NotNull(column, "column");
		if (!Helper.IsScalarType(property.TypeUsage.EdmType) || !Helper.IsPrimitiveType(column.TypeUsage.EdmType))
		{
			throw new ArgumentException(Strings.StorageScalarPropertyMapping_OnlyScalarPropertiesAllowed);
		}
		_column = column;
	}
}
