using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class EntityContainerAssociationSetEnd : EntityContainerRelationshipSetEnd
{
	private string _unresolvedRelationshipEndRole;

	public string Role
	{
		get
		{
			return _unresolvedRelationshipEndRole;
		}
		set
		{
			_unresolvedRelationshipEndRole = value;
		}
	}

	public override string Name => Role;

	public EntityContainerAssociationSetEnd(EntityContainerAssociationSet parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Role"))
		{
			HandleRoleAttribute(reader);
			return true;
		}
		return false;
	}

	private void HandleRoleAttribute(XmlReader reader)
	{
		_unresolvedRelationshipEndRole = HandleUndottedNameAttribute(reader, _unresolvedRelationshipEndRole);
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		_ = base.ParentElement.Relationship;
	}

	internal override void ResolveSecondLevelNames()
	{
		base.ResolveSecondLevelNames();
		if (_unresolvedRelationshipEndRole == null && base.EntitySet != null)
		{
			base.RelationshipEnd = InferRelationshipEnd(base.EntitySet);
			if (base.RelationshipEnd != null)
			{
				_unresolvedRelationshipEndRole = base.RelationshipEnd.Name;
			}
		}
		else if (_unresolvedRelationshipEndRole != null)
		{
			IRelationship relationship = base.ParentElement.Relationship;
			if (relationship.TryGetEnd(_unresolvedRelationshipEndRole, out var end))
			{
				base.RelationshipEnd = end;
			}
			else
			{
				AddError(ErrorCode.InvalidContainerTypeForEnd, EdmSchemaErrorSeverity.Error, Strings.InvalidEntityEndName(Role, relationship.FQName));
			}
		}
	}

	private IRelationshipEnd InferRelationshipEnd(EntityContainerEntitySet set)
	{
		if (base.ParentElement.Relationship == null)
		{
			return null;
		}
		List<IRelationshipEnd> list = new List<IRelationshipEnd>();
		foreach (IRelationshipEnd end in base.ParentElement.Relationship.Ends)
		{
			if (set.EntityType.IsOfType(end.Type))
			{
				list.Add(end);
			}
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		if (list.Count == 0)
		{
			AddError(ErrorCode.FailedInference, EdmSchemaErrorSeverity.Error, Strings.InferRelationshipEndFailedNoEntitySetMatch(set.Name, base.ParentElement.Name, base.ParentElement.Relationship.FQName, set.EntityType.FQName, base.ParentElement.ParentElement.FQName));
		}
		else
		{
			AddError(ErrorCode.FailedInference, EdmSchemaErrorSeverity.Error, Strings.InferRelationshipEndAmbiguous(set.Name, base.ParentElement.Name, base.ParentElement.Relationship.FQName, set.EntityType.FQName, base.ParentElement.ParentElement.FQName));
		}
		return null;
	}

	internal override SchemaElement Clone(SchemaElement parentElement)
	{
		return new EntityContainerAssociationSetEnd((EntityContainerAssociationSet)parentElement)
		{
			_unresolvedRelationshipEndRole = _unresolvedRelationshipEndRole,
			EntitySet = base.EntitySet
		};
	}
}
