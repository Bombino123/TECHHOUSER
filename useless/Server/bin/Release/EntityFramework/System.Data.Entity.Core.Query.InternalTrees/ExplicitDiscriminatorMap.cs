using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Mapping.ViewGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal class ExplicitDiscriminatorMap
{
	private readonly ReadOnlyCollection<KeyValuePair<object, EntityType>> m_typeMap;

	private readonly EdmMember m_discriminatorProperty;

	private readonly ReadOnlyCollection<EdmProperty> m_properties;

	internal ReadOnlyCollection<KeyValuePair<object, EntityType>> TypeMap => m_typeMap;

	internal EdmMember DiscriminatorProperty => m_discriminatorProperty;

	internal ReadOnlyCollection<EdmProperty> Properties => m_properties;

	internal ExplicitDiscriminatorMap(DiscriminatorMap template)
	{
		m_typeMap = template.TypeMap;
		m_discriminatorProperty = template.Discriminator.Property;
		m_properties = new ReadOnlyCollection<EdmProperty>(template.PropertyMap.Select((KeyValuePair<EdmProperty, DbExpression> propertyValuePair) => propertyValuePair.Key).ToList());
	}

	internal object GetTypeId(EntityType entityType)
	{
		object result = null;
		foreach (KeyValuePair<object, EntityType> item in TypeMap)
		{
			if (item.Value.EdmEquals(entityType))
			{
				result = item.Key;
				break;
			}
		}
		return result;
	}
}
