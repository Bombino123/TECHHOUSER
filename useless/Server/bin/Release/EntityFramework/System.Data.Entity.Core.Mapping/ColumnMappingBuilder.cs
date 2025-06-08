using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

internal class ColumnMappingBuilder
{
	private EdmProperty _columnProperty;

	private readonly IList<EdmProperty> _propertyPath;

	private ScalarPropertyMapping _scalarPropertyMapping;

	public IList<EdmProperty> PropertyPath => _propertyPath;

	public EdmProperty ColumnProperty
	{
		get
		{
			return _columnProperty;
		}
		internal set
		{
			_columnProperty = value;
			if (_scalarPropertyMapping != null)
			{
				_scalarPropertyMapping.Column = _columnProperty;
			}
		}
	}

	public ColumnMappingBuilder(EdmProperty columnProperty, IList<EdmProperty> propertyPath)
	{
		Check.NotNull(columnProperty, "columnProperty");
		Check.NotNull(propertyPath, "propertyPath");
		_columnProperty = columnProperty;
		_propertyPath = propertyPath;
	}

	internal void SetTarget(ScalarPropertyMapping scalarPropertyMapping)
	{
		_scalarPropertyMapping = scalarPropertyMapping;
	}
}
