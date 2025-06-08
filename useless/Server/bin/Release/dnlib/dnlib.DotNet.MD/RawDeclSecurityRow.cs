namespace dnlib.DotNet.MD;

public readonly struct RawDeclSecurityRow
{
	public readonly short Action;

	public readonly uint Parent;

	public readonly uint PermissionSet;

	public uint this[int index] => index switch
	{
		0 => (uint)Action, 
		1 => Parent, 
		2 => PermissionSet, 
		_ => 0u, 
	};

	public RawDeclSecurityRow(short Action, uint Parent, uint PermissionSet)
	{
		this.Action = Action;
		this.Parent = Parent;
		this.PermissionSet = PermissionSet;
	}
}
