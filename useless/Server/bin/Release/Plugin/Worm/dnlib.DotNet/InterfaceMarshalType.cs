using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class InterfaceMarshalType : MarshalType
{
	private int iidParamIndex;

	public int IidParamIndex
	{
		get
		{
			return iidParamIndex;
		}
		set
		{
			iidParamIndex = value;
		}
	}

	public bool IsIidParamIndexValid => iidParamIndex >= 0;

	public InterfaceMarshalType(NativeType nativeType)
		: this(nativeType, -1)
	{
	}

	public InterfaceMarshalType(NativeType nativeType, int iidParamIndex)
		: base(nativeType)
	{
		if (nativeType != NativeType.IUnknown && nativeType != NativeType.IDispatch && nativeType != NativeType.IntF)
		{
			throw new ArgumentException("Invalid nativeType");
		}
		this.iidParamIndex = iidParamIndex;
	}

	public override string ToString()
	{
		return $"{nativeType} ({iidParamIndex})";
	}
}
