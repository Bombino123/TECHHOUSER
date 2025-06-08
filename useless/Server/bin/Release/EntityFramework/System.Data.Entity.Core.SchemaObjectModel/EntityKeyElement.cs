using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class EntityKeyElement : SchemaElement
{
	private List<PropertyRefElement> _keyProperties;

	public IList<PropertyRefElement> KeyProperties
	{
		get
		{
			if (_keyProperties == null)
			{
				_keyProperties = new List<PropertyRefElement>();
			}
			return _keyProperties;
		}
	}

	public EntityKeyElement(SchemaEntityType parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		return false;
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "PropertyRef"))
		{
			HandlePropertyRefElement(reader);
			return true;
		}
		return false;
	}

	private void HandlePropertyRefElement(XmlReader reader)
	{
		PropertyRefElement propertyRefElement = new PropertyRefElement(base.ParentElement);
		propertyRefElement.Parse(reader);
		KeyProperties.Add(propertyRefElement);
	}

	internal override void ResolveTopLevelNames()
	{
		foreach (PropertyRefElement keyProperty in _keyProperties)
		{
			if (!keyProperty.ResolveNames((SchemaEntityType)base.ParentElement))
			{
				AddError(ErrorCode.InvalidKey, EdmSchemaErrorSeverity.Error, Strings.InvalidKeyNoProperty(base.ParentElement.FQName, keyProperty.Name));
			}
		}
	}

	internal override void Validate()
	{
		Dictionary<string, PropertyRefElement> dictionary = new Dictionary<string, PropertyRefElement>(StringComparer.Ordinal);
		foreach (PropertyRefElement keyProperty in _keyProperties)
		{
			StructuredProperty property = keyProperty.Property;
			if (dictionary.ContainsKey(property.Name))
			{
				AddError(ErrorCode.DuplicatePropertySpecifiedInEntityKey, EdmSchemaErrorSeverity.Error, Strings.DuplicatePropertyNameSpecifiedInEntityKey(base.ParentElement.FQName, property.Name));
				continue;
			}
			dictionary.Add(property.Name, keyProperty);
			if (property.Nullable)
			{
				AddError(ErrorCode.InvalidKey, EdmSchemaErrorSeverity.Error, Strings.InvalidKeyNullablePart(property.Name, base.ParentElement.Name));
			}
			if ((!(property.Type is ScalarType) && !(property.Type is SchemaEnumType)) || property.CollectionKind != 0)
			{
				AddError(ErrorCode.EntityKeyMustBeScalar, EdmSchemaErrorSeverity.Error, Strings.EntityKeyMustBeScalar(property.Name, base.ParentElement.Name));
			}
			else
			{
				if (property.Type is SchemaEnumType)
				{
					continue;
				}
				PrimitiveType primitiveType = (PrimitiveType)property.TypeUsage.EdmType;
				if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
				{
					if ((primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary && base.Schema.SchemaVersion < 2.0) || Helper.IsSpatialType(primitiveType))
					{
						AddError(ErrorCode.EntityKeyTypeCurrentlyNotSupported, EdmSchemaErrorSeverity.Error, Strings.EntityKeyTypeCurrentlyNotSupported(property.Name, base.ParentElement.FQName, primitiveType.PrimitiveTypeKind));
					}
				}
				else if ((primitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary && base.Schema.SchemaVersion < 2.0) || Helper.IsSpatialType(primitiveType))
				{
					AddError(ErrorCode.EntityKeyTypeCurrentlyNotSupported, EdmSchemaErrorSeverity.Error, Strings.EntityKeyTypeCurrentlyNotSupportedInSSDL(property.Name, base.ParentElement.FQName, property.TypeUsage.EdmType.Name, property.TypeUsage.EdmType.BaseType.FullName, primitiveType.PrimitiveTypeKind));
				}
			}
		}
	}
}
