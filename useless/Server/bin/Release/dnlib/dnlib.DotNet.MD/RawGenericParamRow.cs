namespace dnlib.DotNet.MD;

public readonly struct RawGenericParamRow
{
	public readonly ushort Number;

	public readonly ushort Flags;

	public readonly uint Owner;

	public readonly uint Name;

	public readonly uint Kind;

	public uint this[int index] => index switch
	{
		0 => Number, 
		1 => Flags, 
		2 => Owner, 
		3 => Name, 
		4 => Kind, 
		_ => 0u, 
	};

	public RawGenericParamRow(ushort Number, ushort Flags, uint Owner, uint Name, uint Kind)
	{
		this.Number = Number;
		this.Flags = Flags;
		this.Owner = Owner;
		this.Name = Name;
		this.Kind = Kind;
	}

	public RawGenericParamRow(ushort Number, ushort Flags, uint Owner, uint Name)
	{
		this.Number = Number;
		this.Flags = Flags;
		this.Owner = Owner;
		this.Name = Name;
		Kind = 0u;
	}
}
