using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Diagnostics;
using System.Globalization;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

[DebuggerDisplay("Name={Name}")]
internal sealed class EntityContainer : SchemaType
{
	private SchemaElementLookUpTable<SchemaElement> _members;

	private ISchemaElementLookUpTable<EntityContainerEntitySet> _entitySets;

	private ISchemaElementLookUpTable<EntityContainerRelationshipSet> _relationshipSets;

	private ISchemaElementLookUpTable<Function> _functionImports;

	private string _unresolvedExtendedEntityContainerName;

	private EntityContainer _entityContainerGettingExtended;

	private bool _isAlreadyValidated;

	private bool _isAlreadyResolved;

	private SchemaElementLookUpTable<SchemaElement> Members
	{
		get
		{
			if (_members == null)
			{
				_members = new SchemaElementLookUpTable<SchemaElement>();
			}
			return _members;
		}
	}

	public ISchemaElementLookUpTable<EntityContainerEntitySet> EntitySets
	{
		get
		{
			if (_entitySets == null)
			{
				_entitySets = new FilteredSchemaElementLookUpTable<EntityContainerEntitySet, SchemaElement>(Members);
			}
			return _entitySets;
		}
	}

	public ISchemaElementLookUpTable<EntityContainerRelationshipSet> RelationshipSets
	{
		get
		{
			if (_relationshipSets == null)
			{
				_relationshipSets = new FilteredSchemaElementLookUpTable<EntityContainerRelationshipSet, SchemaElement>(Members);
			}
			return _relationshipSets;
		}
	}

	public ISchemaElementLookUpTable<Function> FunctionImports
	{
		get
		{
			if (_functionImports == null)
			{
				_functionImports = new FilteredSchemaElementLookUpTable<Function, SchemaElement>(Members);
			}
			return _functionImports;
		}
	}

	public EntityContainer ExtendingEntityContainer => _entityContainerGettingExtended;

	public override string FQName => Name;

	public override string Identity => Name;

	public EntityContainer(Schema parentElement)
		: base(parentElement)
	{
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			base.OtherContent.Add(base.Schema.SchemaSource);
		}
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Extends"))
		{
			HandleExtendsAttribute(reader);
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
		if (CanHandleElement(reader, "EntitySet"))
		{
			HandleEntitySetElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "AssociationSet"))
		{
			HandleAssociationSetElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "FunctionImport"))
		{
			HandleFunctionImport(reader);
			return true;
		}
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
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

	private void HandleEntitySetElement(XmlReader reader)
	{
		EntityContainerEntitySet entityContainerEntitySet = new EntityContainerEntitySet(this);
		entityContainerEntitySet.Parse(reader);
		Members.Add(entityContainerEntitySet, doNotAddErrorForEmptyName: true, Strings.DuplicateEntityContainerMemberName);
	}

	private void HandleAssociationSetElement(XmlReader reader)
	{
		EntityContainerAssociationSet entityContainerAssociationSet = new EntityContainerAssociationSet(this);
		entityContainerAssociationSet.Parse(reader);
		Members.Add(entityContainerAssociationSet, doNotAddErrorForEmptyName: true, Strings.DuplicateEntityContainerMemberName);
	}

	private void HandleFunctionImport(XmlReader reader)
	{
		FunctionImportElement functionImportElement = new FunctionImportElement(this);
		functionImportElement.Parse(reader);
		Members.Add(functionImportElement, doNotAddErrorForEmptyName: true, Strings.DuplicateEntityContainerMemberName);
	}

	private void HandleExtendsAttribute(XmlReader reader)
	{
		_unresolvedExtendedEntityContainerName = HandleUndottedNameAttribute(reader, _unresolvedExtendedEntityContainerName);
	}

	internal override void ResolveTopLevelNames()
	{
		if (_isAlreadyResolved)
		{
			return;
		}
		base.ResolveTopLevelNames();
		if (!string.IsNullOrEmpty(_unresolvedExtendedEntityContainerName))
		{
			SchemaType schemaType;
			if (_unresolvedExtendedEntityContainerName == Name)
			{
				AddError(ErrorCode.EntityContainerCannotExtendItself, EdmSchemaErrorSeverity.Error, Strings.EntityContainerCannotExtendItself(Name));
			}
			else if (!base.Schema.SchemaManager.TryResolveType(null, _unresolvedExtendedEntityContainerName, out schemaType))
			{
				AddError(ErrorCode.InvalidEntityContainerNameInExtends, EdmSchemaErrorSeverity.Error, Strings.InvalidEntityContainerNameInExtends(_unresolvedExtendedEntityContainerName));
			}
			else
			{
				_entityContainerGettingExtended = (EntityContainer)schemaType;
				_entityContainerGettingExtended.ResolveTopLevelNames();
			}
		}
		foreach (SchemaElement member in Members)
		{
			member.ResolveTopLevelNames();
		}
		_isAlreadyResolved = true;
	}

	internal override void ResolveSecondLevelNames()
	{
		base.ResolveSecondLevelNames();
		foreach (SchemaElement member in Members)
		{
			member.ResolveSecondLevelNames();
		}
	}

	internal override void Validate()
	{
		if (_isAlreadyValidated)
		{
			return;
		}
		base.Validate();
		if (ExtendingEntityContainer != null)
		{
			ExtendingEntityContainer.Validate();
			foreach (SchemaElement member in ExtendingEntityContainer.Members)
			{
				AddErrorKind error = Members.TryAdd(member.Clone(this));
				DuplicateOrEquivalentMemberNameWhileExtendingEntityContainer(member, error);
			}
		}
		HashSet<string> tableKeys = new HashSet<string>();
		foreach (SchemaElement member2 in Members)
		{
			if (member2 is EntityContainerEntitySet entitySet && base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
			{
				CheckForDuplicateTableMapping(tableKeys, entitySet);
			}
			member2.Validate();
		}
		ValidateRelationshipSetHaveUniqueEnds();
		ValidateOnlyBaseEntitySetTypeDefinesConcurrency();
		_isAlreadyValidated = true;
	}

	internal EntityContainerEntitySet FindEntitySet(string name)
	{
		for (EntityContainer entityContainer = this; entityContainer != null; entityContainer = entityContainer.ExtendingEntityContainer)
		{
			foreach (EntityContainerEntitySet entitySet in entityContainer.EntitySets)
			{
				if (Utils.CompareNames(entitySet.Name, name) == 0)
				{
					return entitySet;
				}
			}
		}
		return null;
	}

	private void DuplicateOrEquivalentMemberNameWhileExtendingEntityContainer(SchemaElement schemaElement, AddErrorKind error)
	{
		if (error != 0)
		{
			schemaElement.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, Strings.DuplicateMemberNameInExtendedEntityContainer(schemaElement.Name, ExtendingEntityContainer.Name, Name));
		}
	}

	private void ValidateOnlyBaseEntitySetTypeDefinesConcurrency()
	{
		Dictionary<SchemaEntityType, EntityContainerEntitySet> dictionary = new Dictionary<SchemaEntityType, EntityContainerEntitySet>();
		foreach (SchemaElement member in Members)
		{
			if (member is EntityContainerEntitySet entityContainerEntitySet && !dictionary.ContainsKey(entityContainerEntitySet.EntityType))
			{
				dictionary.Add(entityContainerEntitySet.EntityType, entityContainerEntitySet);
			}
		}
		foreach (SchemaType schemaType in base.Schema.SchemaTypes)
		{
			if (schemaType is SchemaEntityType schemaEntityType && TypeIsSubTypeOf(schemaEntityType, dictionary, out var set) && TypeDefinesNewConcurrencyProperties(schemaEntityType))
			{
				AddError(ErrorCode.ConcurrencyRedefinedOnSubTypeOfEntitySetType, EdmSchemaErrorSeverity.Error, Strings.ConcurrencyRedefinedOnSubTypeOfEntitySetType(schemaEntityType.FQName, set.EntityType.FQName, set.FQName));
			}
		}
	}

	private void ValidateRelationshipSetHaveUniqueEnds()
	{
		List<EntityContainerRelationshipSetEnd> list = new List<EntityContainerRelationshipSetEnd>();
		bool flag = true;
		foreach (EntityContainerRelationshipSet relationshipSet in RelationshipSets)
		{
			foreach (EntityContainerRelationshipSetEnd end in relationshipSet.Ends)
			{
				flag = false;
				foreach (EntityContainerRelationshipSetEnd item in list)
				{
					if (AreRelationshipEndsEqual(item, end))
					{
						AddError(ErrorCode.SimilarRelationshipEnd, EdmSchemaErrorSeverity.Error, Strings.SimilarRelationshipEnd(item.Name, item.ParentElement.Name, end.ParentElement.Name, item.EntitySet.Name, FQName));
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(end);
				}
			}
		}
	}

	private static bool TypeIsSubTypeOf(SchemaEntityType itemType, Dictionary<SchemaEntityType, EntityContainerEntitySet> baseEntitySetTypes, out EntityContainerEntitySet set)
	{
		if (itemType.IsTypeHierarchyRoot)
		{
			set = null;
			return false;
		}
		for (SchemaEntityType schemaEntityType = itemType.BaseType as SchemaEntityType; schemaEntityType != null; schemaEntityType = schemaEntityType.BaseType as SchemaEntityType)
		{
			if (baseEntitySetTypes.ContainsKey(schemaEntityType))
			{
				set = baseEntitySetTypes[schemaEntityType];
				return true;
			}
		}
		set = null;
		return false;
	}

	private static bool TypeDefinesNewConcurrencyProperties(SchemaEntityType itemType)
	{
		foreach (StructuredProperty property in itemType.Properties)
		{
			if (property.Type is ScalarType && MetadataHelper.GetConcurrencyMode(property.TypeUsage) != 0)
			{
				return true;
			}
		}
		return false;
	}

	private void CheckForDuplicateTableMapping(HashSet<string> tableKeys, EntityContainerEntitySet entitySet)
	{
		string text = ((!string.IsNullOrEmpty(entitySet.DbSchema)) ? entitySet.DbSchema : Name);
		string text2 = ((!string.IsNullOrEmpty(entitySet.Table)) ? entitySet.Table : entitySet.Name);
		string item = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2] { text, text2 });
		if (entitySet.DefiningQuery != null)
		{
			item = entitySet.Name;
		}
		if (!tableKeys.Add(item))
		{
			entitySet.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, Strings.DuplicateEntitySetTable(entitySet.Name, text, text2));
		}
	}

	private static bool AreRelationshipEndsEqual(EntityContainerRelationshipSetEnd left, EntityContainerRelationshipSetEnd right)
	{
		if (left.EntitySet == right.EntitySet && left.ParentElement.Relationship == right.ParentElement.Relationship && left.Name == right.Name)
		{
			return true;
		}
		return false;
	}
}
