using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public readonly struct RawStateMachineMethodRow
{
	public readonly uint MoveNextMethod;

	public readonly uint KickoffMethod;

	public uint this[int index] => index switch
	{
		0 => MoveNextMethod, 
		1 => KickoffMethod, 
		_ => 0u, 
	};

	public RawStateMachineMethodRow(uint MoveNextMethod, uint KickoffMethod)
	{
		this.MoveNextMethod = MoveNextMethod;
		this.KickoffMethod = KickoffMethod;
	}
}
