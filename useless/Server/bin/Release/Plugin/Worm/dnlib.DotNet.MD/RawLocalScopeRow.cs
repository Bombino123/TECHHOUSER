using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawLocalScopeRow
{
	public readonly uint Method;

	public readonly uint ImportScope;

	public readonly uint VariableList;

	public readonly uint ConstantList;

	public readonly uint StartOffset;

	public readonly uint Length;

	public uint this[int index] => index switch
	{
		0 => Method, 
		1 => ImportScope, 
		2 => VariableList, 
		3 => ConstantList, 
		4 => StartOffset, 
		5 => Length, 
		_ => 0u, 
	};

	public RawLocalScopeRow(uint Method, uint ImportScope, uint VariableList, uint ConstantList, uint StartOffset, uint Length)
	{
		this.Method = Method;
		this.ImportScope = ImportScope;
		this.VariableList = VariableList;
		this.ConstantList = ConstantList;
		this.StartOffset = StartOffset;
		this.Length = Length;
	}
}
