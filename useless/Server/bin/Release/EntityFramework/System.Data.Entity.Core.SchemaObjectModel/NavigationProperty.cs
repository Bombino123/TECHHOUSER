using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

[DebuggerDisplay("Name={Name}, Relationship={_unresolvedRelationshipName}, FromRole={_unresolvedFromEndRole}, ToRole={_unresolvedToEndRole}")]
internal sealed class NavigationProperty : Property
{
	private string _unresolvedFromEndRole;

	private string _unresolvedToEndRole;

	private string _unresolvedRelationshipName;

	private IRelationshipEnd _fromEnd;

	private IRelationshipEnd _toEnd;

	private IRelationship _relationship;

	public new SchemaEntityType ParentElement => base.ParentElement as SchemaEntityType;

	internal IRelationship Relationship => _relationship;

	internal IRelationshipEnd ToEnd => _toEnd;

	internal IRelationshipEnd FromEnd => _fromEnd;

	public override SchemaType Type
	{
		get
		{
			if (_toEnd == null || _toEnd.Type == null)
			{
				return null;
			}
			return _toEnd.Type;
		}
	}

	public NavigationProperty(SchemaEntityType parent)
		: base(parent)
	{
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Relationship"))
		{
			HandleAssociationAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "FromRole"))
		{
			HandleFromRoleAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "ToRole"))
		{
			HandleToRoleAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "ContainsTarget"))
		{
			return true;
		}
		return false;
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (!base.Schema.ResolveTypeName(this, _unresolvedRelationshipName, out var type))
		{
			return;
		}
		_relationship = type as IRelationship;
		if (_relationship == null)
		{
			AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error, Strings.BadNavigationPropertyRelationshipNotRelationship(_unresolvedRelationshipName));
			return;
		}
		bool flag = true;
		if (!_relationship.TryGetEnd(_unresolvedFromEndRole, out _fromEnd))
		{
			AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error, Strings.BadNavigationPropertyUndefinedRole(_unresolvedFromEndRole, _relationship.FQName));
			flag = false;
		}
		if (!_relationship.TryGetEnd(_unresolvedToEndRole, out _toEnd))
		{
			AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error, Strings.BadNavigationPropertyUndefinedRole(_unresolvedToEndRole, _relationship.FQName));
			flag = false;
		}
		if (flag && _fromEnd == _toEnd)
		{
			AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error, Strings.BadNavigationPropertyRolesCannotBeTheSame);
		}
	}

	internal override void Validate()
	{
		base.Validate();
		if (_fromEnd.Type != ParentElement)
		{
			AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error, Strings.BadNavigationPropertyBadFromRoleType(Name, _fromEnd.Type.FQName, _fromEnd.Name, _relationship.FQName, ParentElement.FQName));
		}
	}

	private void HandleToRoleAttribute(XmlReader reader)
	{
		_unresolvedToEndRole = HandleUndottedNameAttribute(reader, _unresolvedToEndRole);
	}

	private void HandleFromRoleAttribute(XmlReader reader)
	{
		_unresolvedFromEndRole = HandleUndottedNameAttribute(reader, _unresolvedFromEndRole);
	}

	private void HandleAssociationAttribute(XmlReader reader)
	{
		if (Utils.GetDottedName(base.Schema, reader, out var name))
		{
			_unresolvedRelationshipName = name;
		}
	}
}
