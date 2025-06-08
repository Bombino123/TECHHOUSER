using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class FunctionImportElement : Function
{
	private string _unresolvedEntitySet;

	private bool _entitySetPathDefined;

	private EntityContainer _container;

	private EntityContainerEntitySet _entitySet;

	private bool? _isSideEffecting;

	public override bool IsFunctionImport => true;

	public override string FQName => _container.Name + "." + Name;

	public override string Identity => base.Name;

	public EntityContainer Container => _container;

	public EntityContainerEntitySet EntitySet => _entitySet;

	internal FunctionImportElement(EntityContainer container)
		: base(container.Schema)
	{
		if (base.Schema.DataModel == SchemaDataModelOption.EntityDataModel)
		{
			base.OtherContent.Add(base.Schema.SchemaSource);
		}
		_container = container;
		_isComposable = false;
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "EntitySet"))
		{
			if (Utils.GetString(base.Schema, reader, out var value))
			{
				_unresolvedEntitySet = value;
			}
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "EntitySetPath"))
		{
			if (Utils.GetString(base.Schema, reader, out var _))
			{
				_entitySetPathDefined = true;
			}
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "IsBindable"))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "IsSideEffecting"))
		{
			bool field = true;
			if (HandleBoolAttribute(reader, ref field))
			{
				_isSideEffecting = field;
			}
			return true;
		}
		return false;
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		ResolveEntitySet(this, _unresolvedEntitySet, ref _entitySet);
	}

	internal void ResolveEntitySet(SchemaElement owner, string unresolvedEntitySet, ref EntityContainerEntitySet entitySet)
	{
		if (entitySet == null && unresolvedEntitySet != null)
		{
			entitySet = _container.FindEntitySet(unresolvedEntitySet);
			if (entitySet == null)
			{
				owner.AddError(ErrorCode.FunctionImportUnknownEntitySet, EdmSchemaErrorSeverity.Error, Strings.FunctionImportUnknownEntitySet(unresolvedEntitySet, FQName));
			}
		}
	}

	internal override void Validate()
	{
		base.Validate();
		ValidateFunctionImportReturnType(this, _type, base.CollectionKind, _entitySet, _entitySetPathDefined);
		if (_returnTypeList != null)
		{
			foreach (ReturnType returnType in _returnTypeList)
			{
				ValidateFunctionImportReturnType(returnType, returnType.Type, returnType.CollectionKind, returnType.EntitySet, returnType.EntitySetPathDefined);
			}
		}
		if (_isComposable && _isSideEffecting.HasValue && _isSideEffecting.Value)
		{
			AddError(ErrorCode.FunctionImportComposableAndSideEffectingNotAllowed, EdmSchemaErrorSeverity.Error, Strings.FunctionImportComposableAndSideEffectingNotAllowed(FQName));
		}
		if (_parameters == null)
		{
			return;
		}
		foreach (Parameter parameter in _parameters)
		{
			if (parameter.IsRefType || parameter.CollectionKind != 0)
			{
				AddError(ErrorCode.FunctionImportCollectionAndRefParametersNotAllowed, EdmSchemaErrorSeverity.Error, Strings.FunctionImportCollectionAndRefParametersNotAllowed(FQName));
			}
			if (!parameter.TypeUsageBuilder.Nullable)
			{
				AddError(ErrorCode.FunctionImportNonNullableParametersNotAllowed, EdmSchemaErrorSeverity.Error, Strings.FunctionImportNonNullableParametersNotAllowed(FQName));
			}
		}
	}

	private void ValidateFunctionImportReturnType(SchemaElement owner, SchemaType returnType, CollectionKind returnTypeCollectionKind, EntityContainerEntitySet entitySet, bool entitySetPathDefined)
	{
		if (returnType != null && !ReturnTypeMeetsFunctionImportBasicRequirements(returnType, returnTypeCollectionKind))
		{
			owner.AddError(ErrorCode.FunctionImportUnsupportedReturnType, EdmSchemaErrorSeverity.Error, owner, GetReturnTypeErrorMessage(Name));
		}
		ValidateFunctionImportReturnType(owner, returnType, entitySet, entitySetPathDefined);
	}

	private bool ReturnTypeMeetsFunctionImportBasicRequirements(SchemaType type, CollectionKind returnTypeCollectionKind)
	{
		if (type is ScalarType && returnTypeCollectionKind == CollectionKind.Bag)
		{
			return true;
		}
		if (type is SchemaEntityType && returnTypeCollectionKind == CollectionKind.Bag)
		{
			return true;
		}
		if (base.Schema.SchemaVersion == 1.1)
		{
			if (type is ScalarType && returnTypeCollectionKind == CollectionKind.None)
			{
				return true;
			}
			if (type is SchemaEntityType && returnTypeCollectionKind == CollectionKind.None)
			{
				return true;
			}
			if (type is SchemaComplexType && returnTypeCollectionKind == CollectionKind.None)
			{
				return true;
			}
			if (type is SchemaComplexType && returnTypeCollectionKind == CollectionKind.Bag)
			{
				return true;
			}
		}
		if (base.Schema.SchemaVersion >= 2.0 && type is SchemaComplexType && returnTypeCollectionKind == CollectionKind.Bag)
		{
			return true;
		}
		if (base.Schema.SchemaVersion >= 3.0 && type is SchemaEnumType && returnTypeCollectionKind == CollectionKind.Bag)
		{
			return true;
		}
		return false;
	}

	private void ValidateFunctionImportReturnType(SchemaElement owner, SchemaType returnType, EntityContainerEntitySet entitySet, bool entitySetPathDefined)
	{
		SchemaEntityType schemaEntityType = returnType as SchemaEntityType;
		if (entitySet != null && entitySetPathDefined)
		{
			owner.AddError(ErrorCode.FunctionImportEntitySetAndEntitySetPathDeclared, EdmSchemaErrorSeverity.Error, Strings.FunctionImportEntitySetAndEntitySetPathDeclared(FQName));
		}
		if (schemaEntityType != null)
		{
			if (entitySet == null)
			{
				owner.AddError(ErrorCode.FunctionImportReturnsEntitiesButDoesNotSpecifyEntitySet, EdmSchemaErrorSeverity.Error, Strings.FunctionImportReturnEntitiesButDoesNotSpecifyEntitySet(FQName));
			}
			else if (entitySet.EntityType != null && !schemaEntityType.IsOfType(entitySet.EntityType))
			{
				owner.AddError(ErrorCode.FunctionImportEntityTypeDoesNotMatchEntitySet, EdmSchemaErrorSeverity.Error, Strings.FunctionImportEntityTypeDoesNotMatchEntitySet(FQName, entitySet.EntityType.FQName, entitySet.Name));
			}
		}
		else if (returnType is SchemaComplexType schemaComplexType)
		{
			if (entitySet != null || entitySetPathDefined)
			{
				owner.AddError(ErrorCode.ComplexTypeAsReturnTypeAndDefinedEntitySet, EdmSchemaErrorSeverity.Error, owner.LineNumber, owner.LinePosition, Strings.ComplexTypeAsReturnTypeAndDefinedEntitySet(FQName, schemaComplexType.Name));
			}
		}
		else if (entitySet != null || entitySetPathDefined)
		{
			owner.AddError(ErrorCode.FunctionImportSpecifiesEntitySetButDoesNotReturnEntityType, EdmSchemaErrorSeverity.Error, Strings.FunctionImportSpecifiesEntitySetButNotEntityType(FQName));
		}
	}

	private string GetReturnTypeErrorMessage(string functionName)
	{
		if (base.Schema.SchemaVersion == 1.0)
		{
			return Strings.FunctionImportWithUnsupportedReturnTypeV1(functionName);
		}
		if (base.Schema.SchemaVersion == 1.1)
		{
			return Strings.FunctionImportWithUnsupportedReturnTypeV1_1(functionName);
		}
		return Strings.FunctionImportWithUnsupportedReturnTypeV2(functionName);
	}

	internal override SchemaElement Clone(SchemaElement parentElement)
	{
		FunctionImportElement functionImportElement = new FunctionImportElement((EntityContainer)parentElement);
		CloneSetFunctionFields(functionImportElement);
		functionImportElement._container = _container;
		functionImportElement._entitySet = _entitySet;
		functionImportElement._unresolvedEntitySet = _unresolvedEntitySet;
		functionImportElement._entitySetPathDefined = _entitySetPathDefined;
		return functionImportElement;
	}
}
