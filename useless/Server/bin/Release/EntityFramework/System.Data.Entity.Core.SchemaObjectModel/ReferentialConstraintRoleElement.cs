using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class ReferentialConstraintRoleElement : SchemaElement
{
	private List<PropertyRefElement> _roleProperties;

	private IRelationshipEnd _end;

	public IList<PropertyRefElement> RoleProperties
	{
		get
		{
			if (_roleProperties == null)
			{
				_roleProperties = new List<PropertyRefElement>();
			}
			return _roleProperties;
		}
	}

	public IRelationshipEnd End => _end;

	public ReferentialConstraintRoleElement(ReferentialConstraint parentElement)
		: base(parentElement)
	{
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

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (SchemaElement.CanHandleAttribute(reader, "Role"))
		{
			HandleRoleAttribute(reader);
			return true;
		}
		return false;
	}

	private void HandlePropertyRefElement(XmlReader reader)
	{
		PropertyRefElement propertyRefElement = new PropertyRefElement(base.ParentElement);
		propertyRefElement.Parse(reader);
		RoleProperties.Add(propertyRefElement);
	}

	private void HandleRoleAttribute(XmlReader reader)
	{
		Utils.GetString(base.Schema, reader, out var value);
		Name = value;
	}

	internal override void ResolveTopLevelNames()
	{
		IRelationship relationship = (IRelationship)base.ParentElement.ParentElement;
		if (!relationship.TryGetEnd(Name, out _end))
		{
			AddError(ErrorCode.InvalidRoleInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.InvalidEndRoleInRelationshipConstraint(Name, relationship.Name));
		}
		else
		{
			_ = _end.Type;
		}
	}

	internal override void Validate()
	{
		base.Validate();
		foreach (PropertyRefElement roleProperty in _roleProperties)
		{
			if (!roleProperty.ResolveNames(_end.Type))
			{
				AddError(ErrorCode.InvalidPropertyInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.InvalidPropertyInRelationshipConstraint(roleProperty.Name, Name));
			}
		}
	}
}
