using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Mapping;

public class ComplexPropertyMapping : PropertyMapping
{
	private readonly List<ComplexTypeMapping> _typeMappings;

	public ReadOnlyCollection<ComplexTypeMapping> TypeMappings => new ReadOnlyCollection<ComplexTypeMapping>(_typeMappings);

	public ComplexPropertyMapping(EdmProperty property)
		: base(property)
	{
		Check.NotNull(property, "property");
		if (!TypeSemantics.IsComplexType(property.TypeUsage))
		{
			throw new ArgumentException(Strings.StorageComplexPropertyMapping_OnlyComplexPropertyAllowed, "property");
		}
		_typeMappings = new List<ComplexTypeMapping>();
	}

	public void AddTypeMapping(ComplexTypeMapping typeMapping)
	{
		Check.NotNull(typeMapping, "typeMapping");
		ThrowIfReadOnly();
		_typeMappings.Add(typeMapping);
	}

	public void RemoveTypeMapping(ComplexTypeMapping typeMapping)
	{
		Check.NotNull(typeMapping, "typeMapping");
		ThrowIfReadOnly();
		_typeMappings.Remove(typeMapping);
	}

	internal override void SetReadOnly()
	{
		_typeMappings.TrimExcess();
		MappingItem.SetReadOnly(_typeMappings);
		base.SetReadOnly();
	}
}
