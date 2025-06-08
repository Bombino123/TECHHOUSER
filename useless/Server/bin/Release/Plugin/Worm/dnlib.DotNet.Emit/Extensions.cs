using System.Runtime.InteropServices;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet.Emit;

[ComVisible(true)]
public static class Extensions
{
	public static bool IsExperimental(this Code code)
	{
		byte b = (byte)((int)code >> 8);
		if (b >= 240)
		{
			return b <= 251;
		}
		return false;
	}

	public static OpCode ToOpCode(this Code code)
	{
		byte b = (byte)((int)code >> 8);
		byte b2 = (byte)code;
		return b switch
		{
			0 => OpCodes.OneByteOpCodes[b2], 
			254 => OpCodes.TwoByteOpCodes[b2], 
			_ => code switch
			{
				Code.UNKNOWN1 => OpCodes.UNKNOWN1, 
				Code.UNKNOWN2 => OpCodes.UNKNOWN2, 
				_ => null, 
			}, 
		};
	}

	public static OpCode ToOpCode(this Code code, ModuleContext context)
	{
		byte b = (byte)((int)code >> 8);
		byte b2 = (byte)code;
		switch (b)
		{
		case 0:
			return OpCodes.OneByteOpCodes[b2];
		case 254:
			return OpCodes.TwoByteOpCodes[b2];
		default:
		{
			OpCode experimentalOpCode = context.GetExperimentalOpCode(b, b2);
			if (experimentalOpCode != null)
			{
				return experimentalOpCode;
			}
			return code switch
			{
				Code.UNKNOWN1 => OpCodes.UNKNOWN1, 
				Code.UNKNOWN2 => OpCodes.UNKNOWN2, 
				_ => null, 
			};
		}
		}
	}

	public static OpCode GetOpCode(this Instruction self)
	{
		return self?.OpCode ?? OpCodes.UNKNOWN1;
	}

	public static object GetOperand(this Instruction self)
	{
		return self?.Operand;
	}

	public static uint GetOffset(this Instruction self)
	{
		return self?.Offset ?? 0;
	}

	public static SequencePoint GetSequencePoint(this Instruction self)
	{
		return self?.SequencePoint;
	}

	public static IMDTokenProvider ResolveToken(this IInstructionOperandResolver self, uint token)
	{
		return self.ResolveToken(token, default(GenericParamContext));
	}
}
