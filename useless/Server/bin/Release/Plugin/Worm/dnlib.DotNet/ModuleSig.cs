using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class ModuleSig : NonLeafSig
{
	private uint index;

	public override ElementType ElementType => ElementType.Module;

	public uint Index
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public ModuleSig(uint index, TypeSig nextSig)
		: base(nextSig)
	{
		this.index = index;
	}
}
