using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Linq;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class ReturnType : ModelFunctionTypeElement
{
	private CollectionKind _collectionKind;

	private bool _isRefType;

	private string _unresolvedEntitySet;

	private bool _entitySetPathDefined;

	private ModelFunctionTypeElement _typeSubElement;

	private EntityContainerEntitySet _entitySet;

	internal bool IsRefType => _isRefType;

	internal CollectionKind CollectionKind => _collectionKind;

	internal EntityContainerEntitySet EntitySet => _entitySet;

	internal bool EntitySetPathDefined => _entitySetPathDefined;

	internal ModelFunctionTypeElement SubElement => _typeSubElement;

	internal override TypeUsage TypeUsage
	{
		get
		{
			if (_typeSubElement != null)
			{
				return _typeSubElement.GetTypeUsage();
			}
			if (_typeUsage != null)
			{
				return _typeUsage;
			}
			if (base.TypeUsage == null)
			{
				return null;
			}
			if (_collectionKind != 0)
			{
				return TypeUsage.Create(new CollectionType(base.TypeUsage));
			}
			return base.TypeUsage;
		}
	}

	internal ReturnType(Function parentElement)
		: base(parentElement)
	{
		_typeUsageBuilder = new TypeUsageBuilder(this);
	}

	internal override SchemaElement Clone(SchemaElement parentElement)
	{
		return new ReturnType((Function)parentElement)
		{
			_type = _type,
			Name = Name,
			_typeUsageBuilder = _typeUsageBuilder,
			_unresolvedType = _unresolvedType,
			_unresolvedEntitySet = _unresolvedEntitySet,
			_entitySetPathDefined = _entitySetPathDefined,
			_entitySet = _entitySet
		};
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Type"))
		{
			HandleTypeAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "EntitySet"))
		{
			HandleEntitySetAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "EntitySetPath"))
		{
			HandleEntitySetPathAttribute(reader);
			return true;
		}
		if (_typeUsageBuilder.HandleAttribute(reader))
		{
			return true;
		}
		return false;
	}

	internal bool ResolveNestedTypeNames(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		return _typeSubElement.ResolveNameAndSetTypeUsage(convertedItemCache, newGlobalItems);
	}

	private void HandleTypeAttribute(XmlReader reader)
	{
		if (Utils.GetString(base.Schema, reader, out var value))
		{
			Function.RemoveTypeModifier(ref value, out var typeModifier, out _isRefType);
			if (typeModifier == TypeModifier.Array)
			{
				_collectionKind = CollectionKind.Bag;
			}
			if (Utils.ValidateDottedName(base.Schema, reader, value))
			{
				base.UnresolvedType = value;
			}
		}
	}

	private void HandleEntitySetAttribute(XmlReader reader)
	{
		if (Utils.GetString(base.Schema, reader, out var value))
		{
			_unresolvedEntitySet = value;
		}
	}

	private void HandleEntitySetPathAttribute(XmlReader reader)
	{
		if (Utils.GetString(base.Schema, reader, out var _))
		{
			_entitySetPathDefined = true;
		}
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "CollectionType"))
		{
			HandleCollectionTypeElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "ReferenceType"))
		{
			HandleReferenceTypeElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "TypeRef"))
		{
			HandleTypeRefElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "RowType"))
		{
			HandleRowTypeElement(reader);
			return true;
		}
		return false;
	}

	protected void HandleCollectionTypeElement(XmlReader reader)
	{
		CollectionTypeElement collectionTypeElement = new CollectionTypeElement(this);
		collectionTypeElement.Parse(reader);
		_typeSubElement = collectionTypeElement;
	}

	protected void HandleReferenceTypeElement(XmlReader reader)
	{
		ReferenceTypeElement referenceTypeElement = new ReferenceTypeElement(this);
		referenceTypeElement.Parse(reader);
		_typeSubElement = referenceTypeElement;
	}

	protected void HandleTypeRefElement(XmlReader reader)
	{
		TypeRefElement typeRefElement = new TypeRefElement(this);
		typeRefElement.Parse(reader);
		_typeSubElement = typeRefElement;
	}

	protected void HandleRowTypeElement(XmlReader reader)
	{
		RowTypeElement rowTypeElement = new RowTypeElement(this);
		rowTypeElement.Parse(reader);
		_typeSubElement = rowTypeElement;
	}

	internal override void ResolveTopLevelNames()
	{
		if (_unresolvedType != null)
		{
			base.ResolveTopLevelNames();
		}
		if (_typeSubElement != null)
		{
			_typeSubElement.ResolveTopLevelNames();
		}
		if (base.ParentElement.IsFunctionImport && _unresolvedEntitySet != null)
		{
			((FunctionImportElement)base.ParentElement).ResolveEntitySet(this, _unresolvedEntitySet, ref _entitySet);
		}
	}

	internal override void Validate()
	{
		base.Validate();
		ValidationHelper.ValidateTypeDeclaration(this, _type, _typeSubElement);
		ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
		if (_isRefType)
		{
			ValidationHelper.ValidateRefType(this, _type);
		}
		if (base.Schema.DataModel != 0)
		{
			if (base.Schema.DataModel == SchemaDataModelOption.ProviderManifestModel)
			{
				if ((_type != null && (!(_type is ScalarType) || _collectionKind != 0)) || (_typeSubElement != null && !(_typeSubElement.Type is ScalarType)))
				{
					string p2 = "";
					if (_type != null)
					{
						p2 = Function.GetTypeNameForErrorMessage(_type, _collectionKind, _isRefType);
					}
					else if (_typeSubElement != null)
					{
						p2 = _typeSubElement.FQName;
					}
					AddError(ErrorCode.FunctionWithNonEdmTypeNotSupported, EdmSchemaErrorSeverity.Error, this, Strings.FunctionWithNonEdmPrimitiveTypeNotSupported(p2, base.ParentElement.FQName));
				}
			}
			else if (_type != null)
			{
				if (!(_type is ScalarType) || _collectionKind != 0)
				{
					AddError(ErrorCode.FunctionWithNonPrimitiveTypeNotSupported, EdmSchemaErrorSeverity.Error, this, Strings.FunctionWithNonPrimitiveTypeNotSupported(_isRefType ? _unresolvedType : _type.FQName, base.ParentElement.FQName));
				}
			}
			else if (_typeSubElement != null && !(_typeSubElement.Type is ScalarType))
			{
				if (base.Schema.SchemaVersion < 3.0)
				{
					AddError(ErrorCode.FunctionWithNonPrimitiveTypeNotSupported, EdmSchemaErrorSeverity.Error, this, Strings.FunctionWithNonPrimitiveTypeNotSupported(_typeSubElement.FQName, base.ParentElement.FQName));
				}
				else if (_typeSubElement is CollectionTypeElement { SubElement: RowTypeElement subElement } && subElement.Properties.Any((RowTypePropertyElement p) => !p.ValidateIsScalar()))
				{
					AddError(ErrorCode.TVFReturnTypeRowHasNonScalarProperty, EdmSchemaErrorSeverity.Error, this, Strings.TVFReturnTypeRowHasNonScalarProperty);
				}
			}
		}
		if (_typeSubElement != null)
		{
			_typeSubElement.Validate();
		}
	}

	internal override void WriteIdentity(StringBuilder builder)
	{
	}

	internal override TypeUsage GetTypeUsage()
	{
		return TypeUsage;
	}

	internal override bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		return false;
	}
}
