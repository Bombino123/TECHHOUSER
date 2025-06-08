using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class ReferentialConstraint : SchemaElement
{
	private const char KEY_DELIMITER = ' ';

	private ReferentialConstraintRoleElement _principalRole;

	private ReferentialConstraintRoleElement _dependentRole;

	internal new IRelationship ParentElement => (IRelationship)base.ParentElement;

	internal ReferentialConstraintRoleElement PrincipalRole => _principalRole;

	internal ReferentialConstraintRoleElement DependentRole => _dependentRole;

	public ReferentialConstraint(Relationship relationship)
		: base(relationship)
	{
	}

	internal override void Validate()
	{
		base.Validate();
		_principalRole.Validate();
		_dependentRole.Validate();
		if (!ReadyForFurtherValidation(_principalRole) || !ReadyForFurtherValidation(_dependentRole))
		{
			return;
		}
		IRelationshipEnd end = _principalRole.End;
		IRelationshipEnd end2 = _dependentRole.End;
		if (_principalRole.Name == _dependentRole.Name)
		{
			AddError(ErrorCode.SameRoleReferredInReferentialConstraint, EdmSchemaErrorSeverity.Error, Strings.SameRoleReferredInReferentialConstraint(ParentElement.Name));
		}
		IsKeyProperty(_dependentRole, end2.Type, out var isKeyProperty, out var areAllPropertiesNullable, out var isAnyPropertyNullable, out var isSubsetOfKeyProperties);
		IsKeyProperty(_principalRole, end.Type, out var isKeyProperty2, out var areAllPropertiesNullable2, out var isAnyPropertyNullable2, out var _);
		if (!isKeyProperty2)
		{
			AddError(ErrorCode.InvalidPropertyInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.InvalidFromPropertyInRelationshipConstraint(PrincipalRole.Name, end.Type.FQName, ParentElement.FQName));
			return;
		}
		bool flag = base.Schema.SchemaVersion <= 1.1;
		RelationshipMultiplicity relationshipMultiplicity = ((!(flag ? areAllPropertiesNullable2 : isAnyPropertyNullable2)) ? RelationshipMultiplicity.One : RelationshipMultiplicity.ZeroOrOne);
		RelationshipMultiplicity relationshipMultiplicity2 = ((!(flag ? areAllPropertiesNullable : isAnyPropertyNullable)) ? RelationshipMultiplicity.Many : RelationshipMultiplicity.ZeroOrOne);
		end.Multiplicity = end.Multiplicity ?? relationshipMultiplicity;
		end2.Multiplicity = end2.Multiplicity ?? relationshipMultiplicity2;
		if (end.Multiplicity == RelationshipMultiplicity.Many)
		{
			AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.InvalidMultiplicityFromRoleUpperBoundMustBeOne(_principalRole.Name, ParentElement.Name));
		}
		else if (areAllPropertiesNullable && end.Multiplicity == RelationshipMultiplicity.One)
		{
			string message = Strings.InvalidMultiplicityFromRoleToPropertyNullableV1(_principalRole.Name, ParentElement.Name);
			AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint, EdmSchemaErrorSeverity.Error, message);
		}
		else if (((flag && !areAllPropertiesNullable) || (!flag && !isAnyPropertyNullable)) && end.Multiplicity != RelationshipMultiplicity.One)
		{
			string message2 = ((!flag) ? Strings.InvalidMultiplicityFromRoleToPropertyNonNullableV2(_principalRole.Name, ParentElement.Name) : Strings.InvalidMultiplicityFromRoleToPropertyNonNullableV1(_principalRole.Name, ParentElement.Name));
			AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint, EdmSchemaErrorSeverity.Error, message2);
		}
		if (end2.Multiplicity == RelationshipMultiplicity.One && base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
		{
			AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.InvalidMultiplicityToRoleLowerBoundMustBeZero(_dependentRole.Name, ParentElement.Name));
		}
		if (!isSubsetOfKeyProperties && !ParentElement.IsForeignKey && base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			AddError(ErrorCode.InvalidPropertyInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.InvalidToPropertyInRelationshipConstraint(DependentRole.Name, end2.Type.FQName, ParentElement.FQName));
		}
		if (isKeyProperty)
		{
			if (end2.Multiplicity == RelationshipMultiplicity.Many)
			{
				AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.InvalidMultiplicityToRoleUpperBoundMustBeOne(end2.Name, ParentElement.Name));
			}
		}
		else if (end2.Multiplicity != RelationshipMultiplicity.Many)
		{
			AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.InvalidMultiplicityToRoleUpperBoundMustBeMany(end2.Name, ParentElement.Name));
		}
		if (_dependentRole.RoleProperties.Count != _principalRole.RoleProperties.Count)
		{
			AddError(ErrorCode.MismatchNumberOfPropertiesInRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.MismatchNumberOfPropertiesinRelationshipConstraint);
			return;
		}
		for (int i = 0; i < _dependentRole.RoleProperties.Count; i++)
		{
			if (_dependentRole.RoleProperties[i].Property.Type != _principalRole.RoleProperties[i].Property.Type)
			{
				AddError(ErrorCode.TypeMismatchRelationshipConstraint, EdmSchemaErrorSeverity.Error, Strings.TypeMismatchRelationshipConstraint(_dependentRole.RoleProperties[i].Name, _dependentRole.End.Type.Identity, _principalRole.RoleProperties[i].Name, _principalRole.End.Type.Identity, ParentElement.Name));
			}
		}
	}

	private static bool ReadyForFurtherValidation(ReferentialConstraintRoleElement role)
	{
		if (role == null)
		{
			return false;
		}
		if (role.End == null)
		{
			return false;
		}
		if (role.RoleProperties.Count == 0)
		{
			return false;
		}
		foreach (PropertyRefElement roleProperty in role.RoleProperties)
		{
			if (roleProperty.Property == null)
			{
				return false;
			}
		}
		return true;
	}

	private static void IsKeyProperty(ReferentialConstraintRoleElement roleElement, SchemaEntityType itemType, out bool isKeyProperty, out bool areAllPropertiesNullable, out bool isAnyPropertyNullable, out bool isSubsetOfKeyProperties)
	{
		isKeyProperty = true;
		areAllPropertiesNullable = true;
		isAnyPropertyNullable = false;
		isSubsetOfKeyProperties = true;
		if (itemType.KeyProperties.Count != roleElement.RoleProperties.Count)
		{
			isKeyProperty = false;
		}
		for (int i = 0; i < roleElement.RoleProperties.Count; i++)
		{
			if (isSubsetOfKeyProperties)
			{
				bool flag = false;
				for (int j = 0; j < itemType.KeyProperties.Count; j++)
				{
					if (itemType.KeyProperties[j].Property == roleElement.RoleProperties[i].Property)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					isKeyProperty = false;
					isSubsetOfKeyProperties = false;
				}
			}
			areAllPropertiesNullable &= roleElement.RoleProperties[i].Property.Nullable;
			isAnyPropertyNullable |= roleElement.RoleProperties[i].Property.Nullable;
		}
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
		if (CanHandleElement(reader, "Principal"))
		{
			HandleReferentialConstraintPrincipalRoleElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "Dependent"))
		{
			HandleReferentialConstraintDependentRoleElement(reader);
			return true;
		}
		return false;
	}

	internal void HandleReferentialConstraintPrincipalRoleElement(XmlReader reader)
	{
		_principalRole = new ReferentialConstraintRoleElement(this);
		_principalRole.Parse(reader);
	}

	internal void HandleReferentialConstraintDependentRoleElement(XmlReader reader)
	{
		_dependentRole = new ReferentialConstraintRoleElement(this);
		_dependentRole.Parse(reader);
	}

	internal override void ResolveTopLevelNames()
	{
		_dependentRole.ResolveTopLevelNames();
		_principalRole.ResolveTopLevelNames();
	}
}
