namespace dnlib.DotNet.MD;

public readonly struct RawModuleRow
{
	public readonly ushort Generation;

	public readonly uint Name;

	public readonly uint Mvid;

	public readonly uint EncId;

	public readonly uint EncBaseId;

	public uint this[int index] => index switch
	{
		0 => Generation, 
		1 => Name, 
		2 => Mvid, 
		3 => EncId, 
		4 => EncBaseId, 
		_ => 0u, 
	};

	public RawModuleRow(ushort Generation, uint Name, uint Mvid, uint EncId, uint EncBaseId)
	{
		this.Generation = Generation;
		this.Name = Name;
		this.Mvid = Mvid;
		this.EncId = EncId;
		this.EncBaseId = EncBaseId;
	}
}
