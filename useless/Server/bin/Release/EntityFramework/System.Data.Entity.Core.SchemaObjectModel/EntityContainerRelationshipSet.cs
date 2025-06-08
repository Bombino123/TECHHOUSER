using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal abstract class EntityContainerRelationshipSet : SchemaElement
{
	private IRelationship _relationship;

	private string _unresolvedRelationshipTypeName;

	public override string FQName => ParentElement.Name + "." + Name;

	internal IRelationship Relationship
	{
		get
		{
			return _relationship;
		}
		set
		{
			_relationship = value;
		}
	}

	internal abstract IEnumerable<EntityContainerRelationshipSetEnd> Ends { get; }

	internal new EntityContainer ParentElement => (EntityContainer)base.ParentElement;

	public EntityContainerRelationshipSet(EntityContainer parentElement)
		: base(parentElement)
	{
	}

	protected abstract bool HasEnd(string role);

	protected abstract void AddEnd(IRelationshipEnd relationshipEnd, EntityContainerEntitySet entitySet);

	protected void HandleRelationshipTypeNameAttribute(XmlReader reader)
	{
		ReturnValue<string> returnValue = HandleDottedNameAttribute(reader, _unresolvedRelationshipTypeName);
		if (returnValue.Succeeded)
		{
			_unresolvedRelationshipTypeName = returnValue.Value;
		}
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (_relationship == null)
		{
			if (!base.Schema.ResolveTypeName(this, _unresolvedRelationshipTypeName, out var type))
			{
				return;
			}
			_relationship = type as IRelationship;
			if (_relationship == null)
			{
				AddError(ErrorCode.InvalidPropertyType, EdmSchemaErrorSeverity.Error, Strings.InvalidRelationshipSetType(type.Name));
				return;
			}
		}
		foreach (EntityContainerRelationshipSetEnd end in Ends)
		{
			end.ResolveTopLevelNames();
		}
	}

	internal override void ResolveSecondLevelNames()
	{
		base.ResolveSecondLevelNames();
		foreach (EntityContainerRelationshipSetEnd end in Ends)
		{
			end.ResolveSecondLevelNames();
		}
	}

	internal override void Validate()
	{
		base.Validate();
		InferEnds();
		foreach (EntityContainerRelationshipSetEnd end in Ends)
		{
			end.Validate();
		}
	}

	private void InferEnds()
	{
		foreach (IRelationshipEnd end in Relationship.Ends)
		{
			if (!HasEnd(end.Name))
			{
				EntityContainerEntitySet entityContainerEntitySet = InferEntitySet(end);
				if (entityContainerEntitySet != null)
				{
					AddEnd(end, entityContainerEntitySet);
				}
			}
		}
	}

	private EntityContainerEntitySet InferEntitySet(IRelationshipEnd relationshipEnd)
	{
		List<EntityContainerEntitySet> list = new List<EntityContainerEntitySet>();
		foreach (EntityContainerEntitySet entitySet in ParentElement.EntitySets)
		{
			if (relationshipEnd.Type.IsOfType(entitySet.EntityType))
			{
				list.Add(entitySet);
			}
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		if (list.Count == 0)
		{
			AddError(ErrorCode.MissingExtentEntityContainerEnd, EdmSchemaErrorSeverity.Error, Strings.MissingEntityContainerEnd(relationshipEnd.Name, FQName));
		}
		else
		{
			AddError(ErrorCode.AmbiguousEntityContainerEnd, EdmSchemaErrorSeverity.Error, Strings.AmbiguousEntityContainerEnd(relationshipEnd.Name, FQName));
		}
		return null;
	}
}
