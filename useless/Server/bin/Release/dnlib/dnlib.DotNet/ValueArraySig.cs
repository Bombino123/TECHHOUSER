namespace dnlib.DotNet;

public sealed class ValueArraySig : NonLeafSig
{
	private uint size;

	public override ElementType ElementType => ElementType.ValueArray;

	public uint Size
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

	public ValueArraySig(TypeSig nextSig, uint size)
		: base(nextSig)
	{
		this.size = size;
	}
}
