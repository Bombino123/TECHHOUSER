using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

public class EndPropertyMapping : PropertyMapping
{
	private AssociationEndMember _associationEnd;

	private readonly List<ScalarPropertyMapping> _properties = new List<ScalarPropertyMapping>();

	public AssociationEndMember AssociationEnd
	{
		get
		{
			return _associationEnd;
		}
		internal set
		{
			_associationEnd = value;
		}
	}

	public ReadOnlyCollection<ScalarPropertyMapping> PropertyMappings => new ReadOnlyCollection<ScalarPropertyMapping>(_properties);

	internal IEnumerable<EdmMember> StoreProperties => PropertyMappings.Select((ScalarPropertyMapping propertyMap) => propertyMap.Column);

	public EndPropertyMapping(AssociationEndMember associationEnd)
	{
		Check.NotNull(associationEnd, "associationEnd");
		_associationEnd = associationEnd;
	}

	internal EndPropertyMapping()
	{
	}

	public void AddPropertyMapping(ScalarPropertyMapping propertyMapping)
	{
		Check.NotNull(propertyMapping, "propertyMapping");
		ThrowIfReadOnly();
		_properties.Add(propertyMapping);
	}

	public void RemovePropertyMapping(ScalarPropertyMapping propertyMapping)
	{
		Check.NotNull(propertyMapping, "propertyMapping");
		ThrowIfReadOnly();
		_properties.Remove(propertyMapping);
	}

	internal override void SetReadOnly()
	{
		_properties.TrimExcess();
		MappingItem.SetReadOnly(_properties);
		base.SetReadOnly();
	}
}
