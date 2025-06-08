using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

public abstract class PropertyMapping : MappingItem
{
	private EdmProperty _property;

	public virtual EdmProperty Property
	{
		get
		{
			return _property;
		}
		internal set
		{
			_property = value;
		}
	}

	internal PropertyMapping(EdmProperty property)
	{
		_property = property;
	}

	internal PropertyMapping()
	{
	}
}
