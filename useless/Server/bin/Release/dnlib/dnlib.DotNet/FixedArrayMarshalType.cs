namespace dnlib.DotNet;

public sealed class FixedArrayMarshalType : MarshalType
{
	private int size;

	private NativeType elementType;

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

	public int Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
		}
	}

	public bool IsElementTypeValid => elementType != NativeType.NotInitialized;

	public bool IsSizeValid => size >= 0;

	public FixedArrayMarshalType()
		: this(0)
	{
	}

	public FixedArrayMarshalType(int size)
		: this(size, NativeType.NotInitialized)
	{
	}

	public FixedArrayMarshalType(int size, NativeType elementType)
		: base(NativeType.FixedArray)
	{
		this.size = size;
		this.elementType = elementType;
	}

	public override string ToString()
	{
		return $"{nativeType} ({size}, {elementType})";
	}
}
