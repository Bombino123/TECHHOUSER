namespace dnlib.DotNet.MD;

public readonly struct RawTypeRefRow
{
	public readonly uint ResolutionScope;

	public readonly uint Name;

	public readonly uint Namespace;

	public uint this[int index] => index switch
	{
		0 => ResolutionScope, 
		1 => Name, 
		2 => Namespace, 
		_ => 0u, 
	};

	public RawTypeRefRow(uint ResolutionScope, uint Name, uint Namespace)
	{
		this.ResolutionScope = ResolutionScope;
		this.Name = Name;
		this.Namespace = Namespace;
	}
}
