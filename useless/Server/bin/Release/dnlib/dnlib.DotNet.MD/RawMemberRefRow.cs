namespace dnlib.DotNet.MD;

public readonly struct RawMemberRefRow
{
	public readonly uint Class;

	public readonly uint Name;

	public readonly uint Signature;

	public uint this[int index] => index switch
	{
		0 => Class, 
		1 => Name, 
		2 => Signature, 
		_ => 0u, 
	};

	public RawMemberRefRow(uint Class, uint Name, uint Signature)
	{
		this.Class = Class;
		this.Name = Name;
		this.Signature = Signature;
	}
}
