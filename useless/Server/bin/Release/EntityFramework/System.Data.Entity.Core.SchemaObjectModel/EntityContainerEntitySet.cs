using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal sealed class EntityContainerEntitySet : SchemaElement
{
	private SchemaEntityType _entityType;

	private string _unresolvedEntityTypeName;

	private string _schema;

	private string _table;

	private EntityContainerEntitySetDefiningQuery _definingQueryElement;

	public override string FQName => base.ParentElement.Name + "." + Name;

	public SchemaEntityType EntityType => _entityType;

	public string DbSchema => _schema;

	public string Table => _table;

	public string DefiningQuery
	{
		get
		{
			if (_definingQueryElement != null)
			{
				return _definingQueryElement.Query;
			}
			return null;
		}
	}

	public EntityContainerEntitySet(EntityContainer parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
		{
			if (CanHandleElement(reader, "DefiningQuery"))
			{
				HandleDefiningQueryElement(reader);
				return true;
			}
		}
		else if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			if (CanHandleElement(reader, "ValueAnnotation"))
			{
				SkipElement(reader);
				return true;
			}
			if (CanHandleElement(reader, "TypeAnnotation"))
			{
				SkipElement(reader);
				return true;
			}
		}
		return false;
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "EntityType"))
		{
			HandleEntityTypeAttribute(reader);
			return true;
		}
		if (base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
		{
			if (SchemaElement.CanHandleAttribute(reader, "Schema"))
			{
				HandleDbSchemaAttribute(reader);
				return true;
			}
			if (SchemaElement.CanHandleAttribute(reader, "Table"))
			{
				HandleTableAttribute(reader);
				return true;
			}
		}
		return false;
	}

	private void HandleDefiningQueryElement(XmlReader reader)
	{
		EntityContainerEntitySetDefiningQuery entityContainerEntitySetDefiningQuery = new EntityContainerEntitySetDefiningQuery(this);
		entityContainerEntitySetDefiningQuery.Parse(reader);
		_definingQueryElement = entityContainerEntitySetDefiningQuery;
	}

	protected override void HandleNameAttribute(XmlReader reader)
	{
		if (base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
		{
			Name = reader.Value;
		}
		else
		{
			base.HandleNameAttribute(reader);
		}
	}

	private void HandleEntityTypeAttribute(XmlReader reader)
	{
		ReturnValue<string> returnValue = HandleDottedNameAttribute(reader, _unresolvedEntityTypeName);
		if (returnValue.Succeeded)
		{
			_unresolvedEntityTypeName = returnValue.Value;
		}
	}

	private void HandleDbSchemaAttribute(XmlReader reader)
	{
		_schema = reader.Value;
	}

	private void HandleTableAttribute(XmlReader reader)
	{
		_table = reader.Value;
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (_entityType != null)
		{
			return;
		}
		SchemaType type = null;
		if (base.Schema.ResolveTypeName(this, _unresolvedEntityTypeName, out type))
		{
			_entityType = type as SchemaEntityType;
			if (_entityType == null)
			{
				AddError(ErrorCode.InvalidPropertyType, EdmSchemaErrorSeverity.Error, Strings.InvalidEntitySetType(_unresolvedEntityTypeName));
			}
		}
	}

	internal override void Validate()
	{
		base.Validate();
		if (_entityType.KeyProperties.Count == 0)
		{
			AddError(ErrorCode.EntitySetTypeHasNoKeys, EdmSchemaErrorSeverity.Error, Strings.EntitySetTypeHasNoKeys(Name, _entityType.FQName));
		}
		if (_definingQueryElement != null)
		{
			_definingQueryElement.Validate();
			if (DbSchema != null || Table != null)
			{
				AddError(ErrorCode.TableAndSchemaAreMutuallyExclusiveWithDefiningQuery, EdmSchemaErrorSeverity.Error, Strings.TableAndSchemaAreMutuallyExclusiveWithDefiningQuery(FQName));
			}
		}
	}

	internal override SchemaElement Clone(SchemaElement parentElement)
	{
		return new EntityContainerEntitySet((EntityContainer)parentElement)
		{
			_definingQueryElement = _definingQueryElement,
			_entityType = _entityType,
			_schema = _schema,
			_table = _table,
			Name = Name
		};
	}
}
