using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class RawMarshalType : MarshalType
{
	private byte[] data;

	public byte[] Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public RawMarshalType(byte[] data)
		: base(NativeType.RawBlob)
	{
		this.data = data;
	}
}
