using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;

namespace System.Data.Entity.Core.Metadata.Edm;

public class EdmFunction : EdmType
{
	[Flags]
	private enum FunctionAttributes : byte
	{
		Aggregate = 1,
		BuiltIn = 2,
		NiladicFunction = 4,
		IsComposable = 8,
		IsFromProviderManifest = 0x10,
		IsCachedStoreFunction = 0x20,
		IsFunctionImport = 0x40,
		Default = 8
	}

	private readonly ReadOnlyMetadataCollection<FunctionParameter> _returnParameters;

	private readonly ReadOnlyMetadataCollection<FunctionParameter> _parameters;

	private readonly FunctionAttributes _functionAttributes = FunctionAttributes.IsComposable;

	private string _storeFunctionNameAttribute;

	private readonly ParameterTypeSemantics _parameterTypeSemantics;

	private readonly string _commandTextAttribute;

	private string _schemaName;

	private readonly ReadOnlyCollection<EntitySet> _entitySets;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.EdmFunction;

	public override string FullName => NamespaceName + "." + Name;

	public ReadOnlyMetadataCollection<FunctionParameter> Parameters => _parameters;

	internal bool HasUserDefinedBody
	{
		get
		{
			if (IsModelDefinedFunction)
			{
				return !string.IsNullOrEmpty(CommandTextAttribute);
			}
			return false;
		}
	}

	[MetadataProperty(BuiltInTypeKind.EntitySet, false)]
	internal EntitySet EntitySet
	{
		get
		{
			if (_entitySets.Count == 0)
			{
				return null;
			}
			return _entitySets[0];
		}
	}

	[MetadataProperty(BuiltInTypeKind.EntitySet, true)]
	internal ReadOnlyCollection<EntitySet> EntitySets => _entitySets;

	[MetadataProperty(BuiltInTypeKind.FunctionParameter, false)]
	public FunctionParameter ReturnParameter => _returnParameters.FirstOrDefault();

	[MetadataProperty(BuiltInTypeKind.FunctionParameter, true)]
	public ReadOnlyMetadataCollection<FunctionParameter> ReturnParameters => _returnParameters;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string StoreFunctionNameAttribute
	{
		get
		{
			return _storeFunctionNameAttribute;
		}
		set
		{
			Check.NotEmpty(value, "value");
			Util.ThrowIfReadOnly(this);
			_storeFunctionNameAttribute = value;
		}
	}

	internal string FunctionName => StoreFunctionNameAttribute ?? Name;

	[MetadataProperty(typeof(ParameterTypeSemantics), false)]
	public ParameterTypeSemantics ParameterTypeSemanticsAttribute => _parameterTypeSemantics;

	[MetadataProperty(PrimitiveTypeKind.Boolean, false)]
	public bool AggregateAttribute => GetFunctionAttribute(FunctionAttributes.Aggregate);

	[MetadataProperty(PrimitiveTypeKind.Boolean, false)]
	public virtual bool BuiltInAttribute => GetFunctionAttribute(FunctionAttributes.BuiltIn);

	[MetadataProperty(PrimitiveTypeKind.Boolean, false)]
	public bool IsFromProviderManifest => GetFunctionAttribute(FunctionAttributes.IsFromProviderManifest);

	[MetadataProperty(PrimitiveTypeKind.Boolean, false)]
	public bool NiladicFunctionAttribute => GetFunctionAttribute(FunctionAttributes.NiladicFunction);

	[MetadataProperty(PrimitiveTypeKind.Boolean, false)]
	public bool IsComposableAttribute => GetFunctionAttribute(FunctionAttributes.IsComposable);

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string CommandTextAttribute => _commandTextAttribute;

	internal bool IsCachedStoreFunction => GetFunctionAttribute(FunctionAttributes.IsCachedStoreFunction);

	internal bool IsModelDefinedFunction
	{
		get
		{
			if (DataSpace == DataSpace.CSpace && !IsCachedStoreFunction && !IsFromProviderManifest)
			{
				return !IsFunctionImport;
			}
			return false;
		}
	}

	internal bool IsFunctionImport => GetFunctionAttribute(FunctionAttributes.IsFunctionImport);

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string Schema
	{
		get
		{
			return _schemaName;
		}
		set
		{
			Check.NotEmpty(value, "value");
			Util.ThrowIfReadOnly(this);
			_schemaName = value;
		}
	}

	internal EdmFunction(string name, string namespaceName, DataSpace dataSpace)
		: this(name, namespaceName, dataSpace, new EdmFunctionPayload())
	{
	}

	internal EdmFunction(string name, string namespaceName, DataSpace dataSpace, EdmFunctionPayload payload)
		: base(name, namespaceName, dataSpace)
	{
		_schemaName = payload.Schema;
		IList<FunctionParameter> list = payload.ReturnParameters ?? new FunctionParameter[0];
		foreach (FunctionParameter item in list)
		{
			if ((item ?? throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("ReturnParameters"))).Mode != ParameterMode.ReturnValue)
			{
				throw new ArgumentException(Strings.NonReturnParameterInReturnParameterCollection);
			}
		}
		_returnParameters = new ReadOnlyMetadataCollection<FunctionParameter>(list.Select((FunctionParameter returnParameter) => SafeLink<EdmFunction>.BindChild(this, FunctionParameter.DeclaringFunctionLinker, returnParameter)).ToList());
		if (payload.IsAggregate.HasValue)
		{
			SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.Aggregate, payload.IsAggregate.Value);
		}
		if (payload.IsBuiltIn.HasValue)
		{
			SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.BuiltIn, payload.IsBuiltIn.Value);
		}
		if (payload.IsNiladic.HasValue)
		{
			SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.NiladicFunction, payload.IsNiladic.Value);
		}
		if (payload.IsComposable.HasValue)
		{
			SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.IsComposable, payload.IsComposable.Value);
		}
		if (payload.IsFromProviderManifest.HasValue)
		{
			SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.IsFromProviderManifest, payload.IsFromProviderManifest.Value);
		}
		if (payload.IsCachedStoreFunction.HasValue)
		{
			SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.IsCachedStoreFunction, payload.IsCachedStoreFunction.Value);
		}
		if (payload.IsFunctionImport.HasValue)
		{
			SetFunctionAttribute(ref _functionAttributes, FunctionAttributes.IsFunctionImport, payload.IsFunctionImport.Value);
		}
		if (payload.ParameterTypeSemantics.HasValue)
		{
			_parameterTypeSemantics = payload.ParameterTypeSemantics.Value;
		}
		if (payload.StoreFunctionName != null)
		{
			_storeFunctionNameAttribute = payload.StoreFunctionName;
		}
		if (payload.EntitySets != null)
		{
			if (payload.EntitySets.Count != list.Count)
			{
				throw new ArgumentException(Strings.NumberOfEntitySetsDoesNotMatchNumberOfReturnParameters);
			}
			_entitySets = new ReadOnlyCollection<EntitySet>(payload.EntitySets);
		}
		else
		{
			if (_returnParameters.Count > 1)
			{
				throw new ArgumentException(Strings.NullEntitySetsForFunctionReturningMultipleResultSets);
			}
			_entitySets = new ReadOnlyCollection<EntitySet>(_returnParameters.Select((FunctionParameter p) => (EntitySet)null).ToList());
		}
		if (payload.CommandText != null)
		{
			_commandTextAttribute = payload.CommandText;
		}
		if (payload.Parameters != null)
		{
			foreach (FunctionParameter parameter in payload.Parameters)
			{
				if ((parameter ?? throw new ArgumentException(Strings.ADP_CollectionParameterElementIsNull("parameters"))).Mode == ParameterMode.ReturnValue)
				{
					throw new ArgumentException(Strings.ReturnParameterInInputParameterCollection);
				}
			}
			_parameters = new SafeLinkCollection<EdmFunction, FunctionParameter>(this, FunctionParameter.DeclaringFunctionLinker, new MetadataCollection<FunctionParameter>(payload.Parameters));
		}
		else
		{
			_parameters = new ReadOnlyMetadataCollection<FunctionParameter>(new MetadataCollection<FunctionParameter>());
		}
	}

	public void AddParameter(FunctionParameter functionParameter)
	{
		Check.NotNull(functionParameter, "functionParameter");
		Util.ThrowIfReadOnly(this);
		if (functionParameter.Mode == ParameterMode.ReturnValue)
		{
			throw new ArgumentException(Strings.ReturnParameterInInputParameterCollection);
		}
		_parameters.Source.Add(functionParameter);
	}

	internal override void SetReadOnly()
	{
		if (base.IsReadOnly)
		{
			return;
		}
		base.SetReadOnly();
		Parameters.Source.SetReadOnly();
		foreach (FunctionParameter returnParameter in ReturnParameters)
		{
			returnParameter.SetReadOnly();
		}
	}

	internal override void BuildIdentity(StringBuilder builder)
	{
		if (base.CacheIdentity != null)
		{
			builder.Append(base.CacheIdentity);
			return;
		}
		BuildIdentity(builder, FullName, Parameters, (FunctionParameter param) => param.TypeUsage, (FunctionParameter param) => param.Mode);
	}

	internal static string BuildIdentity(string functionName, IEnumerable<TypeUsage> functionParameters)
	{
		StringBuilder stringBuilder = new StringBuilder();
		BuildIdentity(stringBuilder, functionName, functionParameters, (TypeUsage param) => param, (TypeUsage param) => ParameterMode.In);
		return stringBuilder.ToString();
	}

	internal static void BuildIdentity<TParameterMetadata>(StringBuilder builder, string functionName, IEnumerable<TParameterMetadata> functionParameters, Func<TParameterMetadata, TypeUsage> getParameterTypeUsage, Func<TParameterMetadata, ParameterMode> getParameterMode)
	{
		builder.Append(functionName);
		builder.Append('(');
		bool flag = true;
		foreach (TParameterMetadata functionParameter in functionParameters)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				builder.Append(",");
			}
			builder.Append(Helper.ToString(getParameterMode(functionParameter)));
			builder.Append(' ');
			getParameterTypeUsage(functionParameter).BuildIdentity(builder);
		}
		builder.Append(')');
	}

	private bool GetFunctionAttribute(FunctionAttributes attribute)
	{
		return attribute == (attribute & _functionAttributes);
	}

	private static void SetFunctionAttribute(ref FunctionAttributes field, FunctionAttributes attribute, bool isSet)
	{
		if (isSet)
		{
			field |= attribute;
		}
		else
		{
			field ^= field & attribute;
		}
	}

	public static EdmFunction Create(string name, string namespaceName, DataSpace dataSpace, EdmFunctionPayload payload, IEnumerable<MetadataProperty> metadataProperties)
	{
		Check.NotEmpty(name, "name");
		Check.NotEmpty(namespaceName, "namespaceName");
		EdmFunction edmFunction = new EdmFunction(name, namespaceName, dataSpace, payload);
		if (metadataProperties != null)
		{
			edmFunction.AddMetadataProperties(metadataProperties);
		}
		edmFunction.SetReadOnly();
		return edmFunction;
	}
}
