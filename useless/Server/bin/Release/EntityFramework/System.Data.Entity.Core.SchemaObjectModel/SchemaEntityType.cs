using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

[DebuggerDisplay("Name={Name}, BaseType={BaseType.FQName}, HasKeys={HasKeys}")]
internal sealed class SchemaEntityType : StructuredType
{
	private const char KEY_DELIMITER = ' ';

	private ISchemaElementLookUpTable<NavigationProperty> _navigationProperties;

	private EntityKeyElement _keyElement;

	private static readonly List<PropertyRefElement> _emptyKeyProperties = new List<PropertyRefElement>(0);

	public EntityKeyElement KeyElement => _keyElement;

	public IList<PropertyRefElement> DeclaredKeyProperties
	{
		get
		{
			if (KeyElement == null)
			{
				return _emptyKeyProperties;
			}
			return KeyElement.KeyProperties;
		}
	}

	public IList<PropertyRefElement> KeyProperties
	{
		get
		{
			if (KeyElement == null)
			{
				if (base.BaseType != null)
				{
					return (base.BaseType as SchemaEntityType).KeyProperties;
				}
				return _emptyKeyProperties;
			}
			return KeyElement.KeyProperties;
		}
	}

	public ISchemaElementLookUpTable<NavigationProperty> NavigationProperties
	{
		get
		{
			if (_navigationProperties == null)
			{
				_navigationProperties = new FilteredSchemaElementLookUpTable<NavigationProperty, SchemaElement>(base.NamedMembers);
			}
			return _navigationProperties;
		}
	}

	public SchemaEntityType(Schema parentElement)
		: base(parentElement)
	{
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			base.OtherContent.Add(base.Schema.SchemaSource);
		}
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (base.BaseType != null)
		{
			if (!(base.BaseType is SchemaEntityType))
			{
				AddError(ErrorCode.InvalidBaseType, EdmSchemaErrorSeverity.Error, Strings.InvalidBaseTypeForItemType(base.BaseType.FQName, FQName));
			}
			else if (_keyElement != null && base.BaseType != null)
			{
				AddError(ErrorCode.InvalidKey, EdmSchemaErrorSeverity.Error, Strings.InvalidKeyKeyDefinedInBaseClass(FQName, base.BaseType.FQName));
			}
		}
		else if (_keyElement == null)
		{
			AddError(ErrorCode.KeyMissingOnEntityType, EdmSchemaErrorSeverity.Error, Strings.KeyMissingOnEntityType(FQName));
		}
		else if (base.BaseType != null || base.UnresolvedBaseType == null)
		{
			_keyElement.ResolveTopLevelNames();
		}
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "OpenType") && base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			return true;
		}
		return false;
	}

	internal override void Validate()
	{
		base.Validate();
		if (KeyElement != null)
		{
			KeyElement.Validate();
		}
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "Key"))
		{
			HandleKeyElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "NavigationProperty"))
		{
			HandleNavigationPropertyElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "ValueAnnotation") && base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			SkipElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "TypeAnnotation") && base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			SkipElement(reader);
			return true;
		}
		return false;
	}

	private void HandleNavigationPropertyElement(XmlReader reader)
	{
		NavigationProperty navigationProperty = new NavigationProperty(this);
		navigationProperty.Parse(reader);
		AddMember(navigationProperty);
	}

	private void HandleKeyElement(XmlReader reader)
	{
		_keyElement = new EntityKeyElement(this);
		_keyElement.Parse(reader);
	}
}
