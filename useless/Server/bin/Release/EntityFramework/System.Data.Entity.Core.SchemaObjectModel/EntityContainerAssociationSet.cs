using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class EntityContainerAssociationSet : EntityContainerRelationshipSet
{
	private readonly Dictionary<string, EntityContainerAssociationSetEnd> _relationshipEnds = new Dictionary<string, EntityContainerAssociationSetEnd>();

	private readonly List<EntityContainerAssociationSetEnd> _rolelessEnds = new List<EntityContainerAssociationSetEnd>();

	internal override IEnumerable<EntityContainerRelationshipSetEnd> Ends
	{
		get
		{
			foreach (EntityContainerAssociationSetEnd value in _relationshipEnds.Values)
			{
				yield return value;
			}
			foreach (EntityContainerAssociationSetEnd rolelessEnd in _rolelessEnds)
			{
				yield return rolelessEnd;
			}
		}
	}

	public EntityContainerAssociationSet(EntityContainer parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Association"))
		{
			HandleRelationshipTypeNameAttribute(reader);
			return true;
		}
		return false;
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "End"))
		{
			HandleEndElement(reader);
			return true;
		}
		return false;
	}

	private void HandleEndElement(XmlReader reader)
	{
		EntityContainerAssociationSetEnd entityContainerAssociationSetEnd = new EntityContainerAssociationSetEnd(this);
		entityContainerAssociationSetEnd.Parse(reader);
		if (entityContainerAssociationSetEnd.Role == null)
		{
			_rolelessEnds.Add(entityContainerAssociationSetEnd);
		}
		else if (HasEnd(entityContainerAssociationSetEnd.Role))
		{
			entityContainerAssociationSetEnd.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader, Strings.DuplicateEndName(entityContainerAssociationSetEnd.Name));
		}
		else
		{
			_relationshipEnds.Add(entityContainerAssociationSetEnd.Role, entityContainerAssociationSetEnd);
		}
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
	}

	internal override void ResolveSecondLevelNames()
	{
		base.ResolveSecondLevelNames();
		foreach (EntityContainerAssociationSetEnd rolelessEnd in _rolelessEnds)
		{
			if (rolelessEnd.Role != null)
			{
				if (HasEnd(rolelessEnd.Role))
				{
					rolelessEnd.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, Strings.InferRelationshipEndGivesAlreadyDefinedEnd(rolelessEnd.EntitySet.FQName, Name));
				}
				else
				{
					_relationshipEnds.Add(rolelessEnd.Role, rolelessEnd);
				}
			}
		}
		_rolelessEnds.Clear();
	}

	protected override void AddEnd(IRelationshipEnd relationshipEnd, EntityContainerEntitySet entitySet)
	{
		EntityContainerAssociationSetEnd entityContainerAssociationSetEnd = new EntityContainerAssociationSetEnd(this);
		entityContainerAssociationSetEnd.Role = relationshipEnd.Name;
		entityContainerAssociationSetEnd.RelationshipEnd = relationshipEnd;
		entityContainerAssociationSetEnd.EntitySet = entitySet;
		if (entityContainerAssociationSetEnd.EntitySet != null)
		{
			_relationshipEnds.Add(entityContainerAssociationSetEnd.Role, entityContainerAssociationSetEnd);
		}
	}

	protected override bool HasEnd(string role)
	{
		return _relationshipEnds.ContainsKey(role);
	}

	internal override SchemaElement Clone(SchemaElement parentElement)
	{
		EntityContainerAssociationSet entityContainerAssociationSet = new EntityContainerAssociationSet((EntityContainer)parentElement);
		entityContainerAssociationSet.Name = Name;
		entityContainerAssociationSet.Relationship = base.Relationship;
		foreach (EntityContainerAssociationSetEnd end in Ends)
		{
			EntityContainerAssociationSetEnd entityContainerAssociationSetEnd = (EntityContainerAssociationSetEnd)end.Clone(entityContainerAssociationSet);
			entityContainerAssociationSet._relationshipEnds.Add(entityContainerAssociationSetEnd.Role, entityContainerAssociationSetEnd);
		}
		return entityContainerAssociationSet;
	}
}
