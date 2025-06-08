using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

public sealed class FunctionParameter : MetadataItem, INamedDataModelItem
{
	internal static Func<FunctionParameter, SafeLink<EdmFunction>> DeclaringFunctionLinker = (FunctionParameter fp) => fp._declaringFunction;

	private readonly SafeLink<EdmFunction> _declaringFunction = new SafeLink<EdmFunction>();

	private readonly TypeUsage _typeUsage;

	private string _name;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.FunctionParameter;

	[MetadataProperty(BuiltInTypeKind.ParameterMode, false)]
	public ParameterMode Mode => GetParameterMode();

	string INamedDataModelItem.Identity => Identity;

	internal override string Identity => _name;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			Check.NotEmpty(value, "value");
			SetName(value);
		}
	}

	[MetadataProperty(BuiltInTypeKind.TypeUsage, false)]
	public TypeUsage TypeUsage => _typeUsage;

	public string TypeName => TypeUsage.EdmType.Name;

	public bool IsMaxLengthConstant
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: false, out var item))
			{
				return item.Description.IsConstant;
			}
			return false;
		}
	}

	public int? MaxLength
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: false, out var item))
			{
				return null;
			}
			return item.Value as int?;
		}
	}

	public bool IsMaxLength
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("MaxLength", ignoreCase: false, out var item))
			{
				return item.IsUnbounded;
			}
			return false;
		}
	}

	public bool IsPrecisionConstant
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("Precision", ignoreCase: false, out var item))
			{
				return item.Description.IsConstant;
			}
			return false;
		}
	}

	public byte? Precision
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("Precision", ignoreCase: false, out var item))
			{
				return null;
			}
			return item.Value as byte?;
		}
	}

	public bool IsScaleConstant
	{
		get
		{
			if (TypeUsage.Facets.TryGetValue("Scale", ignoreCase: false, out var item))
			{
				return item.Description.IsConstant;
			}
			return false;
		}
	}

	public byte? Scale
	{
		get
		{
			if (!TypeUsage.Facets.TryGetValue("Scale", ignoreCase: false, out var item))
			{
				return null;
			}
			return item.Value as byte?;
		}
	}

	public EdmFunction DeclaringFunction => _declaringFunction.Value;

	internal FunctionParameter()
	{
	}

	internal FunctionParameter(string name, TypeUsage typeUsage, ParameterMode parameterMode)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(typeUsage, "typeUsage");
		_name = name;
		_typeUsage = typeUsage;
		SetParameterMode(parameterMode);
	}

	private void SetName(string name)
	{
		_name = name;
		if (DeclaringFunction != null)
		{
			((Mode == ParameterMode.ReturnValue) ? DeclaringFunction.ReturnParameters.Source : DeclaringFunction.Parameters.Source).InvalidateCache();
		}
	}

	public override string ToString()
	{
		return Name;
	}

	internal override void SetReadOnly()
	{
		if (!base.IsReadOnly)
		{
			base.SetReadOnly();
		}
	}

	public static FunctionParameter Create(string name, EdmType edmType, ParameterMode parameterMode)
	{
		Check.NotEmpty(name, "name");
		Check.NotNull(edmType, "edmType");
		FunctionParameter functionParameter = new FunctionParameter(name, TypeUsage.Create(edmType, FacetValues.NullFacetValues), parameterMode);
		functionParameter.SetReadOnly();
		return functionParameter;
	}
}
