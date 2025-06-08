using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawInterfaceImplRow
{
	public readonly uint Class;

	public readonly uint Interface;

	public uint this[int index] => index switch
	{
		0 => Class, 
		1 => Interface, 
		_ => 0u, 
	};

	public RawInterfaceImplRow(uint Class, uint Interface)
	{
		this.Class = Class;
		this.Interface = Interface;
	}
}
