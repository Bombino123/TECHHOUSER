using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawENCLogRow
{
	public readonly uint Token;

	public readonly uint FuncCode;

	public uint this[int index] => index switch
	{
		0 => Token, 
		1 => FuncCode, 
		_ => 0u, 
	};

	public RawENCLogRow(uint Token, uint FuncCode)
	{
		this.Token = Token;
		this.FuncCode = FuncCode;
	}
}
