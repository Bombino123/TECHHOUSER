using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawImportScopeRow
{
	public readonly uint Parent;

	public readonly uint Imports;

	public uint this[int index] => index switch
	{
		0 => Parent, 
		1 => Imports, 
		_ => 0u, 
	};

	public RawImportScopeRow(uint Parent, uint Imports)
	{
		this.Parent = Parent;
		this.Imports = Imports;
	}
}
