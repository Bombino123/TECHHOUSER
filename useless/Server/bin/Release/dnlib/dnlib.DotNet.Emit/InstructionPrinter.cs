using System.Collections.Generic;
using System.Text;

namespace dnlib.DotNet.Emit;

public static class InstructionPrinter
{
	public static string ToString(Instruction instr)
	{
		if (instr == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"IL_{instr.Offset:X4}: ");
		stringBuilder.Append(instr.OpCode.Name);
		AddOperandString(stringBuilder, instr, " ");
		return stringBuilder.ToString();
	}

	public static string GetOperandString(Instruction instr)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AddOperandString(stringBuilder, instr, string.Empty);
		return stringBuilder.ToString();
	}

	public static void AddOperandString(StringBuilder sb, Instruction instr)
	{
		AddOperandString(sb, instr, string.Empty);
	}

	public static void AddOperandString(StringBuilder sb, Instruction instr, string extra)
	{
		object operand = instr.Operand;
		switch (instr.OpCode.OperandType)
		{
		case OperandType.InlineBrTarget:
		case OperandType.ShortInlineBrTarget:
			sb.Append(extra);
			AddInstructionTarget(sb, operand as Instruction);
			break;
		case OperandType.InlineField:
		case OperandType.InlineMethod:
		case OperandType.InlineTok:
		case OperandType.InlineType:
			sb.Append(extra);
			if (operand is IFullName)
			{
				sb.Append((operand as IFullName).FullName);
			}
			else if (operand != null)
			{
				sb.Append(operand.ToString());
			}
			else
			{
				sb.Append("null");
			}
			break;
		case OperandType.InlineI:
		case OperandType.InlineI8:
		case OperandType.InlineR:
		case OperandType.ShortInlineI:
		case OperandType.ShortInlineR:
			sb.Append($"{extra}{operand}");
			break;
		case OperandType.InlineSig:
			sb.Append(extra);
			FullNameFactory.MethodFullNameSB(null, (UTF8String)null, operand as MethodSig, null, null, null, sb);
			break;
		case OperandType.InlineString:
			sb.Append(extra);
			EscapeString(sb, operand as string, addQuotes: true);
			break;
		case OperandType.InlineSwitch:
		{
			if (!(operand is IList<Instruction> list))
			{
				sb.Append("null");
				break;
			}
			sb.Append('(');
			for (int i = 0; i < list.Count; i++)
			{
				if (i != 0)
				{
					sb.Append(',');
				}
				AddInstructionTarget(sb, list[i]);
			}
			sb.Append(')');
			break;
		}
		case OperandType.InlineVar:
		case OperandType.ShortInlineVar:
			sb.Append(extra);
			if (operand == null)
			{
				sb.Append("null");
			}
			else
			{
				sb.Append(operand.ToString());
			}
			break;
		case OperandType.InlineNone:
		case OperandType.InlinePhi:
		case OperandType.NOT_USED_8:
			break;
		}
	}

	private static void AddInstructionTarget(StringBuilder sb, Instruction targetInstr)
	{
		if (targetInstr == null)
		{
			sb.Append("null");
		}
		else
		{
			sb.Append($"IL_{targetInstr.Offset:X4}");
		}
	}

	private static void EscapeString(StringBuilder sb, string s, bool addQuotes)
	{
		if (s == null)
		{
			sb.Append("null");
			return;
		}
		if (addQuotes)
		{
			sb.Append('"');
		}
		foreach (char c in s)
		{
			if (c < ' ')
			{
				switch (c)
				{
				case '\a':
					sb.Append("\\a");
					break;
				case '\b':
					sb.Append("\\b");
					break;
				case '\f':
					sb.Append("\\f");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;
				case '\v':
					sb.Append("\\v");
					break;
				default:
					sb.Append($"\\u{(int)c:X4}");
					break;
				}
			}
			else if (c == '\\' || c == '"')
			{
				sb.Append('\\');
				sb.Append(c);
			}
			else
			{
				sb.Append(c);
			}
		}
		if (addQuotes)
		{
			sb.Append('"');
		}
	}
}
