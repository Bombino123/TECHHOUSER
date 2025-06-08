using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public struct StateMachineHoistedLocalScope
{
	public Instruction Start;

	public Instruction End;

	public readonly bool IsSynthesizedLocal
	{
		get
		{
			if (Start == null)
			{
				return End == null;
			}
			return false;
		}
	}

	public StateMachineHoistedLocalScope(Instruction start, Instruction end)
	{
		Start = start;
		End = end;
	}
}
