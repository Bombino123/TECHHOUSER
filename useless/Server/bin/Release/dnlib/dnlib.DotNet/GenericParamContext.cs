namespace dnlib.DotNet;

public readonly struct GenericParamContext
{
	public readonly TypeDef Type;

	public readonly MethodDef Method;

	public bool IsEmpty
	{
		get
		{
			if (Type == null)
			{
				return Method == null;
			}
			return false;
		}
	}

	public static GenericParamContext Create(MethodDef method)
	{
		if (method == null)
		{
			return default(GenericParamContext);
		}
		return new GenericParamContext(method.DeclaringType, method);
	}

	public static GenericParamContext Create(TypeDef type)
	{
		return new GenericParamContext(type);
	}

	public GenericParamContext(TypeDef type)
	{
		Type = type;
		Method = null;
	}

	public GenericParamContext(MethodDef method)
	{
		Type = null;
		Method = method;
	}

	public GenericParamContext(TypeDef type, MethodDef method)
	{
		Type = type;
		Method = method;
	}
}
