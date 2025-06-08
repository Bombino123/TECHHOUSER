using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Text;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class Parameter : FacetEnabledSchemaElement
{
	private ParameterDirection _parameterDirection = ParameterDirection.Input;

	private CollectionKind _collectionKind;

	private ModelFunctionTypeElement _typeSubElement;

	private bool _isRefType;

	internal ParameterDirection ParameterDirection => _parameterDirection;

	internal CollectionKind CollectionKind
	{
		get
		{
			return _collectionKind;
		}
		set
		{
			_collectionKind = value;
		}
	}

	internal bool IsRefType => _isRefType;

	internal override TypeUsage TypeUsage
	{
		get
		{
			if (_typeSubElement != null)
			{
				return _typeSubElement.GetTypeUsage();
			}
			if (base.TypeUsage == null)
			{
				return null;
			}
			if (CollectionKind != 0)
			{
				return TypeUsage.Create(new CollectionType(base.TypeUsage));
			}
			return base.TypeUsage;
		}
	}

	internal new SchemaType Type => _type;

	internal Parameter(Function parentElement)
		: base(parentElement)
	{
		_typeUsageBuilder = new TypeUsageBuilder(this);
	}

	internal void WriteIdentity(StringBuilder builder)
	{
		builder.Append("Parameter(");
		if (!string.IsNullOrWhiteSpace(base.UnresolvedType))
		{
			if (_collectionKind != 0)
			{
				builder.Append("Collection(" + base.UnresolvedType + ")");
			}
			else if (_isRefType)
			{
				builder.Append("Ref(" + base.UnresolvedType + ")");
			}
			else
			{
				builder.Append(base.UnresolvedType);
			}
		}
		else if (_typeSubElement != null)
		{
			_typeSubElement.WriteIdentity(builder);
		}
		builder.Append(")");
	}

	internal override SchemaElement Clone(SchemaElement parentElement)
	{
		return new Parameter((Function)parentElement)
		{
			_collectionKind = _collectionKind,
			_parameterDirection = _parameterDirection,
			_type = _type,
			Name = Name,
			_typeUsageBuilder = _typeUsageBuilder
		};
	}

	internal bool ResolveNestedTypeNames(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems)
	{
		if (_typeSubElement == null)
		{
			return false;
		}
		return _typeSubElement.ResolveNameAndSetTypeUsage(convertedItemCache, newGlobalItems);
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
		if (SchemaElement.CanHandleAttribute(reader, "Mode"))
		{
			HandleModeAttribute(reader);
			return true;
		}
		if (_typeUsageBuilder.HandleAttribute(reader))
		{
			return true;
		}
		return false;
	}

	private void HandleTypeAttribute(XmlReader reader)
	{
		if (Utils.GetString(base.Schema, reader, out var value))
		{
			Function.RemoveTypeModifier(ref value, out var typeModifier, out _isRefType);
			if (typeModifier == TypeModifier.Array)
			{
				CollectionKind = CollectionKind.Bag;
			}
			if (Utils.ValidateDottedName(base.Schema, reader, value))
			{
				base.UnresolvedType = value;
			}
		}
	}

	private void HandleModeAttribute(XmlReader reader)
	{
		string value = reader.Value;
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		value = value.Trim();
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		switch (value)
		{
		case "In":
			_parameterDirection = ParameterDirection.Input;
			break;
		case "Out":
			_parameterDirection = ParameterDirection.Output;
			if (base.ParentElement.IsComposable && base.ParentElement.IsFunctionImport)
			{
				AddErrorBadParameterDirection(value, reader, Strings.BadParameterDirectionForComposableFunctions);
			}
			break;
		case "InOut":
			_parameterDirection = ParameterDirection.InputOutput;
			if (base.ParentElement.IsComposable && base.ParentElement.IsFunctionImport)
			{
				AddErrorBadParameterDirection(value, reader, Strings.BadParameterDirectionForComposableFunctions);
			}
			break;
		default:
			AddErrorBadParameterDirection(value, reader, Strings.BadParameterDirection);
			break;
		}
	}

	private void AddErrorBadParameterDirection(string value, XmlReader reader, Func<object, object, object, object, string> errorFunc)
	{
		AddError(ErrorCode.BadParameterDirection, EdmSchemaErrorSeverity.Error, reader, errorFunc(base.ParentElement.Parameters.Count, base.ParentElement.Name, base.ParentElement.ParentElement.FQName, value));
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
	}

	internal override void Validate()
	{
		base.Validate();
		ValidationHelper.ValidateTypeDeclaration(this, _type, _typeSubElement);
		if (base.Schema.DataModel != 0)
		{
			bool isAggregate = base.ParentElement.IsAggregate;
			if (_type != null && (!(_type is ScalarType) || (!isAggregate && _collectionKind != 0)))
			{
				string p = "";
				if (_type != null)
				{
					p = Function.GetTypeNameForErrorMessage(_type, _collectionKind, _isRefType);
				}
				else if (_typeSubElement != null)
				{
					p = _typeSubElement.FQName;
				}
				if (base.Schema.DataModel == SchemaDataModelOption.ProviderManifestModel)
				{
					AddError(ErrorCode.FunctionWithNonEdmTypeNotSupported, EdmSchemaErrorSeverity.Error, this, Strings.FunctionWithNonEdmPrimitiveTypeNotSupported(p, base.ParentElement.FQName));
				}
				else
				{
					AddError(ErrorCode.FunctionWithNonPrimitiveTypeNotSupported, EdmSchemaErrorSeverity.Error, this, Strings.FunctionWithNonPrimitiveTypeNotSupported(p, base.ParentElement.FQName));
				}
				return;
			}
		}
		ValidationHelper.ValidateFacets(this, _type, _typeUsageBuilder);
		if (_isRefType)
		{
			ValidationHelper.ValidateRefType(this, _type);
		}
		if (_typeSubElement != null)
		{
			_typeSubElement.Validate();
		}
	}
}
