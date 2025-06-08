using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class Function : SchemaType
{
	private bool _isAggregate;

	private bool _isBuiltIn;

	private bool _isNiladicFunction;

	protected bool _isComposable = true;

	protected FunctionCommandText _commandText;

	private string _storeFunctionName;

	protected SchemaType _type;

	private string _unresolvedType;

	protected bool _isRefType;

	protected SchemaElementLookUpTable<Parameter> _parameters;

	protected List<ReturnType> _returnTypeList;

	private CollectionKind _returnTypeCollectionKind;

	private ParameterTypeSemantics _parameterTypeSemantics;

	private string _schema;

	private string _functionStrongName;

	private static readonly Regex _typeParser = new Regex("^(?<modifier>((Collection)|(Ref)))\\s*\\(\\s*(?<typeName>\\S*)\\s*\\)$", RegexOptions.Compiled);

	public bool IsAggregate
	{
		get
		{
			return _isAggregate;
		}
		internal set
		{
			_isAggregate = value;
		}
	}

	public bool IsBuiltIn
	{
		get
		{
			return _isBuiltIn;
		}
		internal set
		{
			_isBuiltIn = value;
		}
	}

	public bool IsNiladicFunction
	{
		get
		{
			return _isNiladicFunction;
		}
		internal set
		{
			_isNiladicFunction = value;
		}
	}

	public bool IsComposable
	{
		get
		{
			return _isComposable;
		}
		internal set
		{
			_isComposable = value;
		}
	}

	public string CommandText
	{
		get
		{
			if (_commandText != null)
			{
				return _commandText.CommandText;
			}
			return null;
		}
	}

	public ParameterTypeSemantics ParameterTypeSemantics
	{
		get
		{
			return _parameterTypeSemantics;
		}
		internal set
		{
			_parameterTypeSemantics = value;
		}
	}

	public string StoreFunctionName
	{
		get
		{
			return _storeFunctionName;
		}
		internal set
		{
			_storeFunctionName = value;
		}
	}

	public virtual SchemaType Type
	{
		get
		{
			if (_returnTypeList != null)
			{
				return _returnTypeList[0].Type;
			}
			return _type;
		}
	}

	public IList<ReturnType> ReturnTypeList
	{
		get
		{
			if (_returnTypeList == null)
			{
				return null;
			}
			return new ReadOnlyCollection<ReturnType>(_returnTypeList);
		}
	}

	public SchemaElementLookUpTable<Parameter> Parameters
	{
		get
		{
			if (_parameters == null)
			{
				_parameters = new SchemaElementLookUpTable<Parameter>();
			}
			return _parameters;
		}
	}

	public CollectionKind CollectionKind
	{
		get
		{
			return _returnTypeCollectionKind;
		}
		internal set
		{
			_returnTypeCollectionKind = value;
		}
	}

	public override string Identity
	{
		get
		{
			if (string.IsNullOrEmpty(_functionStrongName))
			{
				StringBuilder stringBuilder = new StringBuilder(FQName);
				bool flag = true;
				stringBuilder.Append('(');
				foreach (Parameter parameter in Parameters)
				{
					if (!flag)
					{
						stringBuilder.Append(',');
					}
					else
					{
						flag = false;
					}
					stringBuilder.Append(Helper.ToString(parameter.ParameterDirection));
					stringBuilder.Append(' ');
					parameter.WriteIdentity(stringBuilder);
				}
				stringBuilder.Append(')');
				_functionStrongName = stringBuilder.ToString();
			}
			return _functionStrongName;
		}
	}

	public bool IsReturnAttributeReftype => _isRefType;

	public virtual bool IsFunctionImport => false;

	public string DbSchema => _schema;

	internal string UnresolvedReturnType
	{
		get
		{
			return _unresolvedType;
		}
		set
		{
			_unresolvedType = value;
		}
	}

	internal static void RemoveTypeModifier(ref string type, out TypeModifier typeModifier, out bool isRefType)
	{
		isRefType = false;
		typeModifier = TypeModifier.None;
		Match match = _typeParser.Match(type);
		if (match.Success)
		{
			type = match.Groups["typeName"].Value;
			switch (match.Groups["modifier"].Value)
			{
			case "Collection":
				typeModifier = TypeModifier.Array;
				break;
			case "Ref":
				isRefType = true;
				break;
			}
		}
	}

	internal static string GetTypeNameForErrorMessage(SchemaType type, CollectionKind colKind, bool isRef)
	{
		string text = type.FQName;
		if (isRef)
		{
			text = "Ref(" + text + ")";
		}
		if (colKind == CollectionKind.Bag)
		{
			text = "Collection(" + text + ")";
		}
		return text;
	}

	public Function(Schema parentElement)
		: base(parentElement)
	{
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "CommandText"))
		{
			HandleCommandTextFunctionElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "Parameter"))
		{
			HandleParameterElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "ReturnType"))
		{
			HandleReturnTypeElement(reader);
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

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "ReturnType"))
		{
			HandleReturnTypeAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Aggregate"))
		{
			HandleAggregateAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "BuiltIn"))
		{
			HandleBuiltInAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "StoreFunctionName"))
		{
			HandleStoreFunctionNameAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "NiladicFunction"))
		{
			HandleNiladicFunctionAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "IsComposable"))
		{
			HandleIsComposableAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "ParameterTypeSemantics"))
		{
			HandleParameterTypeSemanticsAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Schema"))
		{
			HandleDbSchemaAttribute(reader);
			return true;
		}
		return false;
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (_unresolvedType != null)
		{
			base.Schema.ResolveTypeName(this, UnresolvedReturnType, out _type);
		}
		if (_returnTypeList != null)
		{
			foreach (ReturnType returnType in _returnTypeList)
			{
				returnType.ResolveTopLevelNames();
			}
		}
		foreach (Parameter parameter in Parameters)
		{
			parameter.ResolveTopLevelNames();
		}
	}

	internal override void Validate()
	{
		base.Validate();
		if (_type != null && _returnTypeList != null)
		{
			AddError(ErrorCode.ReturnTypeDeclaredAsAttributeAndElement, EdmSchemaErrorSeverity.Error, Strings.TypeDeclaredAsAttributeAndElement);
		}
		if (_returnTypeList == null && Type == null)
		{
			if (IsComposable)
			{
				AddError(ErrorCode.ComposableFunctionOrFunctionImportWithoutReturnType, EdmSchemaErrorSeverity.Error, Strings.ComposableFunctionOrFunctionImportMustDeclareReturnType);
			}
		}
		else if (!IsComposable && !IsFunctionImport)
		{
			AddError(ErrorCode.NonComposableFunctionWithReturnType, EdmSchemaErrorSeverity.Error, Strings.NonComposableFunctionMustNotDeclareReturnType);
		}
		if (base.Schema.DataModel != 0)
		{
			if (IsAggregate)
			{
				if (Parameters.Count == 0)
				{
					AddError(ErrorCode.InvalidNumberOfParametersForAggregateFunction, EdmSchemaErrorSeverity.Error, this, Strings.InvalidNumberOfParametersForAggregateFunction(FQName));
				}
				else if (Parameters.GetElementAt(0).CollectionKind == CollectionKind.None)
				{
					Parameter elementAt = Parameters.GetElementAt(0);
					AddError(ErrorCode.InvalidParameterTypeForAggregateFunction, EdmSchemaErrorSeverity.Error, this, Strings.InvalidParameterTypeForAggregateFunction(elementAt.Name, FQName));
				}
			}
			if (!IsComposable && (IsAggregate || IsNiladicFunction || IsBuiltIn))
			{
				AddError(ErrorCode.NonComposableFunctionAttributesNotValid, EdmSchemaErrorSeverity.Error, Strings.NonComposableFunctionHasDisallowedAttribute);
			}
			if (CommandText != null)
			{
				if (IsComposable)
				{
					AddError(ErrorCode.ComposableFunctionWithCommandText, EdmSchemaErrorSeverity.Error, Strings.CommandTextFunctionsNotComposable);
				}
				if (StoreFunctionName != null)
				{
					AddError(ErrorCode.FunctionDeclaresCommandTextAndStoreFunctionName, EdmSchemaErrorSeverity.Error, Strings.CommandTextFunctionsCannotDeclareStoreFunctionName);
				}
			}
		}
		if (base.Schema.DataModel == SchemaDataModelOption.ProviderDataModel && _type != null && (!(_type is ScalarType) || _returnTypeCollectionKind != 0))
		{
			AddError(ErrorCode.FunctionWithNonPrimitiveTypeNotSupported, EdmSchemaErrorSeverity.Error, this, Strings.FunctionWithNonPrimitiveTypeNotSupported(GetTypeNameForErrorMessage(_type, _returnTypeCollectionKind, _isRefType), FQName));
		}
		if (_returnTypeList != null)
		{
			foreach (ReturnType returnType in _returnTypeList)
			{
				returnType.Validate();
			}
		}
		if (_parameters != null)
		{
			foreach (Parameter parameter in _parameters)
			{
				parameter.Validate();
			}
		}
		if (_commandText != null)
		{
			_commandText.Validate();
		}
	}

	internal override void ResolveSecondLevelNames()
	{
		foreach (Parameter parameter in _parameters)
		{
			parameter.ResolveSecondLevelNames();
		}
	}

	internal override SchemaElement Clone(SchemaElement parentElement)
	{
		throw Error.NotImplemented();
	}

	protected void CloneSetFunctionFields(Function clone)
	{
		clone._isAggregate = _isAggregate;
		clone._isBuiltIn = _isBuiltIn;
		clone._isNiladicFunction = _isNiladicFunction;
		clone._isComposable = _isComposable;
		clone._commandText = _commandText;
		clone._storeFunctionName = _storeFunctionName;
		clone._type = _type;
		clone._returnTypeList = _returnTypeList;
		clone._returnTypeCollectionKind = _returnTypeCollectionKind;
		clone._parameterTypeSemantics = _parameterTypeSemantics;
		clone._schema = _schema;
		clone.Name = Name;
		foreach (Parameter parameter in Parameters)
		{
			clone.Parameters.TryAdd((Parameter)parameter.Clone(clone));
		}
	}

	private void HandleDbSchemaAttribute(XmlReader reader)
	{
		_schema = reader.Value;
	}

	private void HandleAggregateAttribute(XmlReader reader)
	{
		bool field = false;
		HandleBoolAttribute(reader, ref field);
		IsAggregate = field;
	}

	private void HandleBuiltInAttribute(XmlReader reader)
	{
		bool field = false;
		HandleBoolAttribute(reader, ref field);
		IsBuiltIn = field;
	}

	private void HandleStoreFunctionNameAttribute(XmlReader reader)
	{
		string value = reader.Value;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();
			StoreFunctionName = value;
		}
	}

	private void HandleNiladicFunctionAttribute(XmlReader reader)
	{
		bool field = false;
		HandleBoolAttribute(reader, ref field);
		IsNiladicFunction = field;
	}

	private void HandleIsComposableAttribute(XmlReader reader)
	{
		bool field = true;
		HandleBoolAttribute(reader, ref field);
		IsComposable = field;
	}

	private void HandleCommandTextFunctionElement(XmlReader reader)
	{
		FunctionCommandText functionCommandText = new FunctionCommandText(this);
		functionCommandText.Parse(reader);
		_commandText = functionCommandText;
	}

	protected virtual void HandleReturnTypeAttribute(XmlReader reader)
	{
		if (Utils.GetString(base.Schema, reader, out var value))
		{
			RemoveTypeModifier(ref value, out var typeModifier, out _isRefType);
			if (typeModifier != 0 && typeModifier == TypeModifier.Array)
			{
				CollectionKind = CollectionKind.Bag;
			}
			if (Utils.ValidateDottedName(base.Schema, reader, value))
			{
				UnresolvedReturnType = value;
			}
		}
	}

	protected void HandleParameterElement(XmlReader reader)
	{
		Parameter parameter = new Parameter(this);
		parameter.Parse(reader);
		Parameters.Add(parameter, doNotAddErrorForEmptyName: true, Strings.ParameterNameAlreadyDefinedDuplicate);
	}

	protected void HandleReturnTypeElement(XmlReader reader)
	{
		ReturnType returnType = new ReturnType(this);
		returnType.Parse(reader);
		if (_returnTypeList == null)
		{
			_returnTypeList = new List<ReturnType>();
		}
		_returnTypeList.Add(returnType);
	}

	private void HandleParameterTypeSemanticsAttribute(XmlReader reader)
	{
		string value = reader.Value;
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		value = value.Trim();
		if (!string.IsNullOrEmpty(value))
		{
			switch (value)
			{
			case "ExactMatchOnly":
				ParameterTypeSemantics = ParameterTypeSemantics.ExactMatchOnly;
				break;
			case "AllowImplicitPromotion":
				ParameterTypeSemantics = ParameterTypeSemantics.AllowImplicitPromotion;
				break;
			case "AllowImplicitConversion":
				ParameterTypeSemantics = ParameterTypeSemantics.AllowImplicitConversion;
				break;
			default:
				AddError(ErrorCode.InvalidValueForParameterTypeSemantics, EdmSchemaErrorSeverity.Error, reader, Strings.InvalidValueForParameterTypeSemanticsAttribute(value));
				break;
			}
		}
	}
}
