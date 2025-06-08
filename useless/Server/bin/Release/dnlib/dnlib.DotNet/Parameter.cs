namespace dnlib.DotNet;

public sealed class Parameter : IVariable
{
	private readonly ParameterList parameterList;

	private TypeSig typeSig;

	private readonly int paramIndex;

	private readonly int methodSigIndex;

	public const int HIDDEN_THIS_METHOD_SIG_INDEX = -2;

	public const int RETURN_TYPE_METHOD_SIG_INDEX = -1;

	public int Index => paramIndex;

	public int MethodSigIndex => methodSigIndex;

	public bool IsNormalMethodParameter => methodSigIndex >= 0;

	public bool IsHiddenThisParameter => methodSigIndex == -2;

	public bool IsReturnTypeParameter => methodSigIndex == -1;

	public TypeSig Type
	{
		get
		{
			return typeSig;
		}
		set
		{
			typeSig = value;
			if (parameterList != null)
			{
				parameterList.TypeUpdated(this);
			}
		}
	}

	public MethodDef Method => parameterList?.Method;

	public ParamDef ParamDef => parameterList?.FindParamDef(this);

	public bool HasParamDef => ParamDef != null;

	public string Name
	{
		get
		{
			ParamDef paramDef = ParamDef;
			if (paramDef != null)
			{
				return UTF8String.ToSystemStringOrEmpty(paramDef.Name);
			}
			return string.Empty;
		}
		set
		{
			ParamDef paramDef = ParamDef;
			if (paramDef != null)
			{
				paramDef.Name = value;
			}
		}
	}

	public Parameter(int paramIndex)
	{
		this.paramIndex = paramIndex;
		methodSigIndex = paramIndex;
	}

	public Parameter(int paramIndex, TypeSig type)
	{
		this.paramIndex = paramIndex;
		methodSigIndex = paramIndex;
		typeSig = type;
	}

	public Parameter(int paramIndex, int methodSigIndex)
	{
		this.paramIndex = paramIndex;
		this.methodSigIndex = methodSigIndex;
	}

	public Parameter(int paramIndex, int methodSigIndex, TypeSig type)
	{
		this.paramIndex = paramIndex;
		this.methodSigIndex = methodSigIndex;
		typeSig = type;
	}

	internal Parameter(ParameterList parameterList, int paramIndex, int methodSigIndex)
	{
		this.parameterList = parameterList;
		this.paramIndex = paramIndex;
		this.methodSigIndex = methodSigIndex;
	}

	public void CreateParamDef()
	{
		if (parameterList != null)
		{
			parameterList.CreateParamDef(this);
		}
	}

	public override string ToString()
	{
		string name = Name;
		if (string.IsNullOrEmpty(name))
		{
			if (IsReturnTypeParameter)
			{
				return "RET_PARAM";
			}
			return $"A_{paramIndex}";
		}
		return name;
	}
}
