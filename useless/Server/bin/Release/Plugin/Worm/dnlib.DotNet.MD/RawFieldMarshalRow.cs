using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawFieldMarshalRow
{
	public readonly uint Parent;

	public readonly uint NativeType;

	public uint this[int index] => index switch
	{
		0 => Parent, 
		1 => NativeType, 
		_ => 0u, 
	};

	public RawFieldMarshalRow(uint Parent, uint NativeType)
	{
		this.Parent = Parent;
		this.NativeType = NativeType;
	}
}
