using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Mapping;

public class ConditionPropertyMapping : PropertyMapping
{
	private EdmProperty _column;

	private readonly object _value;

	private readonly bool? _isNull;

	internal object Value => _value;

	internal bool? IsNull => _isNull;

	public override EdmProperty Property
	{
		get
		{
			return base.Property;
		}
		internal set
		{
			base.Property = value;
		}
	}

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

	internal ConditionPropertyMapping(EdmProperty propertyOrColumn, object value, bool? isNull)
	{
		DataSpace dataSpace = propertyOrColumn.TypeUsage.EdmType.DataSpace;
		switch (dataSpace)
		{
		case DataSpace.CSpace:
			base.Property = propertyOrColumn;
			break;
		case DataSpace.SSpace:
			_column = propertyOrColumn;
			break;
		default:
			throw new ArgumentException(Strings.MetadataItem_InvalidDataSpace(dataSpace, typeof(EdmProperty).Name), "propertyOrColumn");
		}
		_value = value;
		_isNull = isNull;
	}

	internal ConditionPropertyMapping(EdmProperty property, EdmProperty column, object value, bool? isNull)
		: base(property)
	{
		_column = column;
		_value = value;
		_isNull = isNull;
	}
}
