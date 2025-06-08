namespace dnlib.DotNet;

public sealed class ArrayMarshalType : MarshalType
{
	private NativeType elementType;

	private int paramNum;

	private int numElems;

	private int flags;

	private const int ntaSizeParamIndexSpecified = 1;

	public NativeType ElementType
	{
		get
		{
			return elementType;
		}
		set
		{
			elementType = value;
		}
	}

	public int ParamNumber
	{
		get
		{
			return paramNum;
		}
		set
		{
			paramNum = value;
		}
	}

	public int Size
	{
		get
		{
			return numElems;
		}
		set
		{
			numElems = value;
		}
	}

	public int Flags
	{
		get
		{
			return flags;
		}
		set
		{
			flags = value;
		}
	}

	public bool IsElementTypeValid => elementType != NativeType.NotInitialized;

	public bool IsParamNumberValid => paramNum >= 0;

	public bool IsSizeValid => numElems >= 0;

	public bool IsFlagsValid => flags >= 0;

	public bool IsSizeParamIndexSpecified
	{
		get
		{
			if (IsFlagsValid)
			{
				return (flags & 1) != 0;
			}
			return false;
		}
	}

	public bool IsSizeParamIndexNotSpecified
	{
		get
		{
			if (IsFlagsValid)
			{
				return (flags & 1) == 0;
			}
			return false;
		}
	}

	public ArrayMarshalType()
		: this(NativeType.NotInitialized, -1, -1, -1)
	{
	}

	public ArrayMarshalType(NativeType elementType)
		: this(elementType, -1, -1, -1)
	{
	}

	public ArrayMarshalType(NativeType elementType, int paramNum)
		: this(elementType, paramNum, -1, -1)
	{
	}

	public ArrayMarshalType(NativeType elementType, int paramNum, int numElems)
		: this(elementType, paramNum, numElems, -1)
	{
	}

	public ArrayMarshalType(NativeType elementType, int paramNum, int numElems, int flags)
		: base(NativeType.Array)
	{
		this.elementType = elementType;
		this.paramNum = paramNum;
		this.numElems = numElems;
		this.flags = flags;
	}

	public override string ToString()
	{
		return $"{nativeType} ({elementType}, {paramNum}, {numElems}, {flags})";
	}
}
