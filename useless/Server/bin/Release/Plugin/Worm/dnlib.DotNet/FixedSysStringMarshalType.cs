using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class FixedSysStringMarshalType : MarshalType
{
	private int size;

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

	public bool IsSizeValid => size >= 0;

	public FixedSysStringMarshalType()
		: this(-1)
	{
	}

	public FixedSysStringMarshalType(int size)
		: base(NativeType.FixedSysString)
	{
		this.size = size;
	}

	public override string ToString()
	{
		if (IsSizeValid)
		{
			return $"{nativeType} ({size})";
		}
		return $"{nativeType} (<no size>)";
	}
}
