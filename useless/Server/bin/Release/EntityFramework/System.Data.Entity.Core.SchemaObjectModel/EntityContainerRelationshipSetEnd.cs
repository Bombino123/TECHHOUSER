using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class EntityContainerRelationshipSetEnd : SchemaElement
{
	private IRelationshipEnd _relationshipEnd;

	private string _unresolvedEntitySetName;

	private EntityContainerEntitySet _entitySet;

	public IRelationshipEnd RelationshipEnd
	{
		get
		{
			return _relationshipEnd;
		}
		internal set
		{
			_relationshipEnd = value;
		}
	}

	public EntityContainerEntitySet EntitySet
	{
		get
		{
			return _entitySet;
		}
		internal set
		{
			_entitySet = value;
		}
	}

	internal new EntityContainerRelationshipSet ParentElement => (EntityContainerRelationshipSet)base.ParentElement;

	public EntityContainerRelationshipSetEnd(EntityContainerRelationshipSet parentElement)
		: base(parentElement)
	{
	}

	protected override bool ProhibitAttribute(string namespaceUri, string localName)
	{
		if (base.ProhibitAttribute(namespaceUri, localName))
		{
			return true;
		}
		if (namespaceUri == null)
		{
			_ = localName == "Name";
			return false;
		}
		return false;
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "EntitySet"))
		{
			HandleEntitySetAttribute(reader);
			return true;
		}
		return false;
	}

	private void HandleEntitySetAttribute(XmlReader reader)
	{
		if (base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
		{
			_unresolvedEntitySetName = reader.Value;
		}
		else
		{
			_unresolvedEntitySetName = HandleUndottedNameAttribute(reader, _unresolvedEntitySetName);
		}
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (_entitySet == null)
		{
			_entitySet = ParentElement.ParentElement.FindEntitySet(_unresolvedEntitySetName);
			if (_entitySet == null)
			{
				AddError(ErrorCode.InvalidEndEntitySet, EdmSchemaErrorSeverity.Error, Strings.InvalidEntitySetNameReference(_unresolvedEntitySetName, Name));
			}
		}
	}

	internal override void Validate()
	{
		base.Validate();
		if (_relationshipEnd != null && _entitySet != null && !_relationshipEnd.Type.IsOfType(_entitySet.EntityType) && !_entitySet.EntityType.IsOfType(_relationshipEnd.Type))
		{
			AddError(ErrorCode.InvalidEndEntitySet, EdmSchemaErrorSeverity.Error, Strings.InvalidEndEntitySetTypeMismatch(_relationshipEnd.Name));
		}
	}
}
