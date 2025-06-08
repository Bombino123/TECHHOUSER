using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using dnlib.DotNet.Emit;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.WindowsPdb;

internal struct PdbCustomDebugInfoReader
{
	private readonly ModuleDef module;

	private readonly TypeDef typeOpt;

	private readonly CilBody bodyOpt;

	private readonly GenericParamContext gpContext;

	private DataReader reader;

	public static void Read(MethodDef method, CilBody body, IList<PdbCustomDebugInfo> result, byte[] data)
	{
		try
		{
			DataReader dataReader = ByteArrayDataReaderFactory.CreateReader(data);
			new PdbCustomDebugInfoReader(method, body, ref dataReader).Read(result);
		}
		catch (ArgumentException)
		{
		}
		catch (OutOfMemoryException)
		{
		}
		catch (IOException)
		{
		}
	}

	private PdbCustomDebugInfoReader(MethodDef method, CilBody body, ref DataReader reader)
	{
		module = method.Module;
		typeOpt = method.DeclaringType;
		bodyOpt = body;
		gpContext = GenericParamContext.Create(method);
		this.reader = reader;
	}

	private void Read(IList<PdbCustomDebugInfo> result)
	{
		if (reader.Length < 4 || reader.ReadByte() != 4)
		{
			return;
		}
		reader.ReadByte();
		reader.Position += 2u;
		while (reader.CanRead(8u))
		{
			int num = reader.ReadByte();
			PdbCustomDebugInfoKind pdbCustomDebugInfoKind = (PdbCustomDebugInfoKind)reader.ReadByte();
			reader.Position++;
			int num2 = reader.ReadByte();
			int num3 = reader.ReadInt32();
			if (num3 < 8 || (ulong)((long)reader.Position - 8L + (uint)num3) > (ulong)reader.Length)
			{
				break;
			}
			if (pdbCustomDebugInfoKind <= PdbCustomDebugInfoKind.DynamicLocals)
			{
				num2 = 0;
			}
			if (num2 > 3)
			{
				break;
			}
			uint position = reader.Position - 8 + (uint)num3;
			if (num == 4)
			{
				ulong num4 = (ulong)((long)reader.Position - 8L + (uint)num3 - (uint)num2);
				PdbCustomDebugInfo pdbCustomDebugInfo = ReadRecord(pdbCustomDebugInfoKind, num4);
				if (reader.Position > num4)
				{
					break;
				}
				if (pdbCustomDebugInfo != null)
				{
					result.Add(pdbCustomDebugInfo);
				}
			}
			reader.Position = position;
		}
	}

	private PdbCustomDebugInfo ReadRecord(PdbCustomDebugInfoKind recKind, ulong recPosEnd)
	{
		switch (recKind)
		{
		case PdbCustomDebugInfoKind.UsingGroups:
		{
			int num = reader.ReadUInt16();
			if (num < 0)
			{
				return null;
			}
			PdbUsingGroupsCustomDebugInfo pdbUsingGroupsCustomDebugInfo = new PdbUsingGroupsCustomDebugInfo(num);
			for (int m = 0; m < num; m++)
			{
				pdbUsingGroupsCustomDebugInfo.UsingCounts.Add(reader.ReadUInt16());
			}
			return pdbUsingGroupsCustomDebugInfo;
		}
		case PdbCustomDebugInfoKind.ForwardMethodInfo:
			if (!(module.ResolveToken(reader.ReadUInt32(), gpContext) is IMethodDefOrRef method2))
			{
				return null;
			}
			return new PdbForwardMethodInfoCustomDebugInfo(method2);
		case PdbCustomDebugInfoKind.ForwardModuleInfo:
			if (!(module.ResolveToken(reader.ReadUInt32(), gpContext) is IMethodDefOrRef method))
			{
				return null;
			}
			return new PdbForwardModuleInfoCustomDebugInfo(method);
		case PdbCustomDebugInfoKind.StateMachineHoistedLocalScopes:
		{
			if (bodyOpt == null)
			{
				return null;
			}
			int num = reader.ReadInt32();
			if (num < 0)
			{
				return null;
			}
			PdbStateMachineHoistedLocalScopesCustomDebugInfo pdbStateMachineHoistedLocalScopesCustomDebugInfo = new PdbStateMachineHoistedLocalScopesCustomDebugInfo(num);
			for (int n = 0; n < num; n++)
			{
				uint num6 = reader.ReadUInt32();
				uint num7 = reader.ReadUInt32();
				if (num6 > num7)
				{
					return null;
				}
				if (num7 == 0)
				{
					pdbStateMachineHoistedLocalScopesCustomDebugInfo.Scopes.Add(default(StateMachineHoistedLocalScope));
					continue;
				}
				Instruction instruction = GetInstruction(num6);
				Instruction instruction2 = GetInstruction(num7 + 1);
				if (instruction == null)
				{
					return null;
				}
				pdbStateMachineHoistedLocalScopesCustomDebugInfo.Scopes.Add(new StateMachineHoistedLocalScope(instruction, instruction2));
			}
			return pdbStateMachineHoistedLocalScopesCustomDebugInfo;
		}
		case PdbCustomDebugInfoKind.StateMachineTypeName:
		{
			string text2 = ReadUnicodeZ(recPosEnd, needZeroChar: true);
			if (text2 == null)
			{
				return null;
			}
			TypeDef nestedType = GetNestedType(text2);
			if (nestedType == null)
			{
				return null;
			}
			return new PdbStateMachineTypeNameCustomDebugInfo(nestedType);
		}
		case PdbCustomDebugInfoKind.DynamicLocals:
		{
			if (bodyOpt == null)
			{
				return null;
			}
			int num = reader.ReadInt32();
			if ((ulong)(reader.Position + (long)(uint)num * 200L) > recPosEnd)
			{
				return null;
			}
			PdbDynamicLocalsCustomDebugInfo pdbDynamicLocalsCustomDebugInfo = new PdbDynamicLocalsCustomDebugInfo(num);
			for (int k = 0; k < num; k++)
			{
				reader.Position += 64u;
				int num4 = reader.ReadInt32();
				if ((uint)num4 > 64u)
				{
					return null;
				}
				PdbDynamicLocal pdbDynamicLocal = new PdbDynamicLocal(num4);
				uint position = reader.Position;
				reader.Position -= 68u;
				for (int l = 0; l < num4; l++)
				{
					pdbDynamicLocal.Flags.Add(reader.ReadByte());
				}
				reader.Position = position;
				int num3 = reader.ReadInt32();
				if (num3 != 0 && (uint)num3 >= (uint)bodyOpt.Variables.Count)
				{
					return null;
				}
				uint num5 = reader.Position + 128;
				string text2 = ReadUnicodeZ(num5, needZeroChar: false);
				reader.Position = num5;
				Local local = ((num3 < bodyOpt.Variables.Count) ? bodyOpt.Variables[num3] : null);
				if (num3 == 0 && local != null && local.Name != text2)
				{
					local = null;
				}
				if (local != null && local.Name == text2)
				{
					text2 = null;
				}
				pdbDynamicLocal.Name = text2;
				pdbDynamicLocal.Local = local;
				pdbDynamicLocalsCustomDebugInfo.Locals.Add(pdbDynamicLocal);
			}
			return pdbDynamicLocalsCustomDebugInfo;
		}
		case PdbCustomDebugInfoKind.EditAndContinueLocalSlotMap:
		{
			byte[] data = reader.ReadBytes((int)(recPosEnd - reader.Position));
			return new PdbEditAndContinueLocalSlotMapCustomDebugInfo(data);
		}
		case PdbCustomDebugInfoKind.EditAndContinueLambdaMap:
		{
			byte[] data = reader.ReadBytes((int)(recPosEnd - reader.Position));
			return new PdbEditAndContinueLambdaMapCustomDebugInfo(data);
		}
		case PdbCustomDebugInfoKind.TupleElementNames:
		{
			if (bodyOpt == null)
			{
				return null;
			}
			int num = reader.ReadInt32();
			if (num < 0)
			{
				return null;
			}
			PdbTupleElementNamesCustomDebugInfo pdbTupleElementNamesCustomDebugInfo = new PdbTupleElementNamesCustomDebugInfo(num);
			for (int i = 0; i < num; i++)
			{
				int num2 = reader.ReadInt32();
				if ((uint)num2 >= 10000u)
				{
					return null;
				}
				PdbTupleElementNames pdbTupleElementNames = new PdbTupleElementNames(num2);
				for (int j = 0; j < num2; j++)
				{
					string text = ReadUTF8Z(recPosEnd);
					if (text == null)
					{
						return null;
					}
					pdbTupleElementNames.TupleElementNames.Add(text);
				}
				int num3 = reader.ReadInt32();
				uint offset = reader.ReadUInt32();
				uint offset2 = reader.ReadUInt32();
				string text2 = ReadUTF8Z(recPosEnd);
				if (text2 == null)
				{
					return null;
				}
				Local local;
				if (num3 == -1)
				{
					local = null;
					pdbTupleElementNames.ScopeStart = GetInstruction(offset);
					pdbTupleElementNames.ScopeEnd = GetInstruction(offset2);
					if (pdbTupleElementNames.ScopeStart == null)
					{
						return null;
					}
				}
				else
				{
					if ((uint)num3 >= (uint)bodyOpt.Variables.Count)
					{
						return null;
					}
					local = bodyOpt.Variables[num3];
				}
				if (local != null && local.Name == text2)
				{
					text2 = null;
				}
				pdbTupleElementNames.Local = local;
				pdbTupleElementNames.Name = text2;
				pdbTupleElementNamesCustomDebugInfo.Names.Add(pdbTupleElementNames);
			}
			return pdbTupleElementNamesCustomDebugInfo;
		}
		default:
		{
			byte[] data = reader.ReadBytes((int)(recPosEnd - reader.Position));
			return new PdbUnknownCustomDebugInfo(recKind, data);
		}
		}
	}

	private TypeDef GetNestedType(string name)
	{
		if (typeOpt == null)
		{
			return null;
		}
		IList<TypeDef> nestedTypes = typeOpt.NestedTypes;
		int count = nestedTypes.Count;
		for (int i = 0; i < count; i++)
		{
			TypeDef typeDef = nestedTypes[i];
			if (!UTF8String.IsNullOrEmpty(typeDef.Namespace))
			{
				continue;
			}
			if (typeDef.Name == name)
			{
				return typeDef;
			}
			string @string = typeDef.Name.String;
			if (!@string.StartsWith(name) || @string.Length < name.Length + 2)
			{
				continue;
			}
			int length = name.Length;
			if (@string[length] != '`')
			{
				continue;
			}
			bool flag = true;
			for (length++; length < @string.Length; length++)
			{
				if (!char.IsDigit(@string[length]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return typeDef;
			}
		}
		return null;
	}

	private string ReadUnicodeZ(ulong recPosEnd, bool needZeroChar)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			if (reader.Position >= recPosEnd)
			{
				if (!needZeroChar)
				{
					return stringBuilder.ToString();
				}
				return null;
			}
			char c = reader.ReadChar();
			if (c == '\0')
			{
				break;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private string ReadUTF8Z(ulong recPosEnd)
	{
		if (reader.Position > recPosEnd)
		{
			return null;
		}
		return reader.TryReadZeroTerminatedUtf8String();
	}

	private Instruction GetInstruction(uint offset)
	{
		IList<Instruction> instructions = bodyOpt.Instructions;
		int num = 0;
		int num2 = instructions.Count - 1;
		while (num <= num2 && num2 != -1)
		{
			int num3 = (num + num2) / 2;
			Instruction instruction = instructions[num3];
			if (instruction.Offset == offset)
			{
				return instruction;
			}
			if (offset < instruction.Offset)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return null;
	}
}
