using System.Collections.Generic;
using System.IO;
using System.Text;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.WindowsPdb;

internal struct PdbCustomDebugInfoWriter
{
	private readonly Metadata metadata;

	private readonly MethodDef method;

	private readonly ILogger logger;

	private readonly MemoryStream memoryStream;

	private readonly DataWriter writer;

	private readonly Dictionary<Instruction, uint> instructionToOffsetDict;

	private uint bodySize;

	private bool instructionToOffsetDictInitd;

	public static byte[] Write(Metadata metadata, MethodDef method, PdbCustomDebugInfoWriterContext context, IList<PdbCustomDebugInfo> customDebugInfos)
	{
		return new PdbCustomDebugInfoWriter(metadata, method, context).Write(customDebugInfos);
	}

	private PdbCustomDebugInfoWriter(Metadata metadata, MethodDef method, PdbCustomDebugInfoWriterContext context)
	{
		this.metadata = metadata;
		this.method = method;
		logger = context.Logger;
		memoryStream = context.MemoryStream;
		writer = context.Writer;
		instructionToOffsetDict = context.InstructionToOffsetDict;
		bodySize = 0u;
		instructionToOffsetDictInitd = false;
		memoryStream.SetLength(0L);
		memoryStream.Position = 0L;
	}

	private void InitializeInstructionDictionary()
	{
		instructionToOffsetDict.Clear();
		CilBody body = method.Body;
		if (body != null)
		{
			IList<Instruction> instructions = body.Instructions;
			uint num = 0u;
			for (int i = 0; i < instructions.Count; i++)
			{
				Instruction instruction = instructions[i];
				instructionToOffsetDict[instruction] = num;
				num += (uint)instruction.GetSize();
			}
			bodySize = num;
			instructionToOffsetDictInitd = true;
		}
	}

	private uint GetInstructionOffset(Instruction instr, bool nullIsEndOfMethod)
	{
		if (!instructionToOffsetDictInitd)
		{
			InitializeInstructionDictionary();
		}
		if (instr == null)
		{
			if (nullIsEndOfMethod)
			{
				return bodySize;
			}
			Error("Instruction is null");
			return uint.MaxValue;
		}
		if (instructionToOffsetDict.TryGetValue(instr, out var value))
		{
			return value;
		}
		Error("Instruction is missing in body but it's still being referenced by PDB data. Method {0} (0x{1:X8}), instruction: {2}", method, method.MDToken.Raw, instr);
		return uint.MaxValue;
	}

	private void Error(string message, params object[] args)
	{
		logger.Log(this, LoggerEvent.Error, message, args);
	}

	private byte[] Write(IList<PdbCustomDebugInfo> customDebugInfos)
	{
		if (customDebugInfos.Count == 0)
		{
			return null;
		}
		if (customDebugInfos.Count > 255)
		{
			Error("Too many custom debug infos. Count must be <= 255");
			return null;
		}
		writer.WriteByte(4);
		writer.WriteByte((byte)customDebugInfos.Count);
		writer.WriteUInt16(0);
		for (int i = 0; i < customDebugInfos.Count; i++)
		{
			PdbCustomDebugInfo pdbCustomDebugInfo = customDebugInfos[i];
			if (pdbCustomDebugInfo == null)
			{
				Error("Custom debug info is null");
				return null;
			}
			if ((uint)pdbCustomDebugInfo.Kind > 255u)
			{
				Error("Invalid custom debug info kind");
				return null;
			}
			long position = writer.Position;
			writer.WriteByte(4);
			writer.WriteByte((byte)pdbCustomDebugInfo.Kind);
			writer.WriteUInt16(0);
			writer.WriteUInt32(0u);
			switch (pdbCustomDebugInfo.Kind)
			{
			case PdbCustomDebugInfoKind.UsingGroups:
			{
				if (!(pdbCustomDebugInfo is PdbUsingGroupsCustomDebugInfo pdbUsingGroupsCustomDebugInfo))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				int count = pdbUsingGroupsCustomDebugInfo.UsingCounts.Count;
				if (count > 65535)
				{
					Error("UsingCounts contains more than 0xFFFF elements");
					return null;
				}
				writer.WriteUInt16((ushort)count);
				for (int j = 0; j < count; j++)
				{
					writer.WriteUInt16(pdbUsingGroupsCustomDebugInfo.UsingCounts[j]);
				}
				break;
			}
			case PdbCustomDebugInfoKind.ForwardMethodInfo:
			{
				if (!(pdbCustomDebugInfo is PdbForwardMethodInfoCustomDebugInfo pdbForwardMethodInfoCustomDebugInfo))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				uint methodToken = GetMethodToken(pdbForwardMethodInfoCustomDebugInfo.Method);
				if (methodToken == 0)
				{
					return null;
				}
				writer.WriteUInt32(methodToken);
				break;
			}
			case PdbCustomDebugInfoKind.ForwardModuleInfo:
			{
				if (!(pdbCustomDebugInfo is PdbForwardModuleInfoCustomDebugInfo pdbForwardModuleInfoCustomDebugInfo))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				uint methodToken = GetMethodToken(pdbForwardModuleInfoCustomDebugInfo.Method);
				if (methodToken == 0)
				{
					return null;
				}
				writer.WriteUInt32(methodToken);
				break;
			}
			case PdbCustomDebugInfoKind.StateMachineHoistedLocalScopes:
			{
				if (!(pdbCustomDebugInfo is PdbStateMachineHoistedLocalScopesCustomDebugInfo pdbStateMachineHoistedLocalScopesCustomDebugInfo))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				int count = pdbStateMachineHoistedLocalScopesCustomDebugInfo.Scopes.Count;
				writer.WriteInt32(count);
				for (int j = 0; j < count; j++)
				{
					StateMachineHoistedLocalScope stateMachineHoistedLocalScope = pdbStateMachineHoistedLocalScopesCustomDebugInfo.Scopes[j];
					if (stateMachineHoistedLocalScope.IsSynthesizedLocal)
					{
						writer.WriteInt32(0);
						writer.WriteInt32(0);
					}
					else
					{
						writer.WriteUInt32(GetInstructionOffset(stateMachineHoistedLocalScope.Start, nullIsEndOfMethod: false));
						writer.WriteUInt32(GetInstructionOffset(stateMachineHoistedLocalScope.End, nullIsEndOfMethod: true) - 1);
					}
				}
				break;
			}
			case PdbCustomDebugInfoKind.StateMachineTypeName:
				if (!(pdbCustomDebugInfo is PdbStateMachineTypeNameCustomDebugInfo { Type: var type }))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				if (type == null)
				{
					Error("State machine type is null");
					return null;
				}
				WriteUnicodeZ(MetadataNameToRoslynName(type.Name));
				break;
			case PdbCustomDebugInfoKind.DynamicLocals:
			{
				if (!(pdbCustomDebugInfo is PdbDynamicLocalsCustomDebugInfo pdbDynamicLocalsCustomDebugInfo))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				int count = pdbDynamicLocalsCustomDebugInfo.Locals.Count;
				writer.WriteInt32(count);
				for (int j = 0; j < count; j++)
				{
					PdbDynamicLocal pdbDynamicLocal = pdbDynamicLocalsCustomDebugInfo.Locals[j];
					if (pdbDynamicLocal == null)
					{
						Error("Dynamic local is null");
						return null;
					}
					if (pdbDynamicLocal.Flags.Count > 64)
					{
						Error("Dynamic local flags is longer than 64 bytes");
						return null;
					}
					string text = pdbDynamicLocal.Name;
					if (text == null)
					{
						text = string.Empty;
					}
					if (text.Length > 64)
					{
						Error("Dynamic local name is longer than 64 chars");
						return null;
					}
					if (text.IndexOf('\0') >= 0)
					{
						Error("Dynamic local name contains a NUL char");
						return null;
					}
					int k;
					for (k = 0; k < pdbDynamicLocal.Flags.Count; k++)
					{
						writer.WriteByte(pdbDynamicLocal.Flags[k]);
					}
					while (k++ < 64)
					{
						writer.WriteByte(0);
					}
					writer.WriteInt32(pdbDynamicLocal.Flags.Count);
					if (pdbDynamicLocal.Local == null)
					{
						writer.WriteInt32(0);
					}
					else
					{
						writer.WriteInt32(pdbDynamicLocal.Local.Index);
					}
					for (k = 0; k < text.Length; k++)
					{
						writer.WriteUInt16(text[k]);
					}
					while (k++ < 64)
					{
						writer.WriteUInt16(0);
					}
				}
				break;
			}
			case PdbCustomDebugInfoKind.EditAndContinueLocalSlotMap:
				if (!(pdbCustomDebugInfo is PdbEditAndContinueLocalSlotMapCustomDebugInfo pdbEditAndContinueLocalSlotMapCustomDebugInfo))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				writer.WriteBytes(pdbEditAndContinueLocalSlotMapCustomDebugInfo.Data);
				break;
			case PdbCustomDebugInfoKind.EditAndContinueLambdaMap:
				if (!(pdbCustomDebugInfo is PdbEditAndContinueLambdaMapCustomDebugInfo pdbEditAndContinueLambdaMapCustomDebugInfo))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				writer.WriteBytes(pdbEditAndContinueLambdaMapCustomDebugInfo.Data);
				break;
			case PdbCustomDebugInfoKind.TupleElementNames:
			{
				if (!(pdbCustomDebugInfo is PdbTupleElementNamesCustomDebugInfo pdbTupleElementNamesCustomDebugInfo))
				{
					Error("Unsupported custom debug info type {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				int count = pdbTupleElementNamesCustomDebugInfo.Names.Count;
				writer.WriteInt32(count);
				for (int j = 0; j < count; j++)
				{
					PdbTupleElementNames pdbTupleElementNames = pdbTupleElementNamesCustomDebugInfo.Names[j];
					if (pdbTupleElementNames == null)
					{
						Error("Tuple name info is null");
						return null;
					}
					writer.WriteInt32(pdbTupleElementNames.TupleElementNames.Count);
					for (int k = 0; k < pdbTupleElementNames.TupleElementNames.Count; k++)
					{
						WriteUTF8Z(pdbTupleElementNames.TupleElementNames[k]);
					}
					if (pdbTupleElementNames.Local == null)
					{
						writer.WriteInt32(-1);
						writer.WriteUInt32(GetInstructionOffset(pdbTupleElementNames.ScopeStart, nullIsEndOfMethod: false));
						writer.WriteUInt32(GetInstructionOffset(pdbTupleElementNames.ScopeEnd, nullIsEndOfMethod: true));
					}
					else
					{
						writer.WriteInt32(pdbTupleElementNames.Local.Index);
						writer.WriteInt64(0L);
					}
					WriteUTF8Z(pdbTupleElementNames.Name);
				}
				break;
			}
			default:
				if (!(pdbCustomDebugInfo is PdbUnknownCustomDebugInfo pdbUnknownCustomDebugInfo))
				{
					Error("Unsupported custom debug info class {0}", pdbCustomDebugInfo.GetType());
					return null;
				}
				writer.WriteBytes(pdbUnknownCustomDebugInfo.Data);
				break;
			}
			long position2 = writer.Position;
			long num = position2 - position;
			long num2 = (num + 3) & -4;
			if (num2 > uint.MaxValue)
			{
				Error("Custom debug info record is too big");
				return null;
			}
			writer.Position = position + 3;
			if (pdbCustomDebugInfo.Kind <= PdbCustomDebugInfoKind.DynamicLocals)
			{
				writer.WriteByte(0);
			}
			else
			{
				writer.WriteByte((byte)(num2 - num));
			}
			writer.WriteUInt32((uint)num2);
			writer.Position = position2;
			while (writer.Position < position + num2)
			{
				writer.WriteByte(0);
			}
		}
		return memoryStream.ToArray();
	}

	private string MetadataNameToRoslynName(string name)
	{
		if (name == null)
		{
			return name;
		}
		int num = name.LastIndexOf('`');
		if (num < 0)
		{
			return name;
		}
		return name.Substring(0, num);
	}

	private void WriteUnicodeZ(string s)
	{
		if (s == null)
		{
			Error("String is null");
			return;
		}
		if (s.IndexOf('\0') >= 0)
		{
			Error("String contains a NUL char: {0}", s);
			return;
		}
		for (int i = 0; i < s.Length; i++)
		{
			writer.WriteUInt16(s[i]);
		}
		writer.WriteUInt16(0);
	}

	private void WriteUTF8Z(string s)
	{
		if (s == null)
		{
			Error("String is null");
		}
		else if (s.IndexOf('\0') >= 0)
		{
			Error("String contains a NUL char: {0}", s);
		}
		else
		{
			writer.WriteBytes(Encoding.UTF8.GetBytes(s));
			writer.WriteByte(0);
		}
	}

	private uint GetMethodToken(IMethodDefOrRef method)
	{
		if (method == null)
		{
			Error("Method is null");
			return 0u;
		}
		if (method is MethodDef methodDef)
		{
			uint rid = metadata.GetRid(methodDef);
			if (rid == 0)
			{
				Error("Method {0} ({1:X8}) is not defined in this module ({2})", method, method.MDToken.Raw, metadata.Module);
				return 0u;
			}
			return new MDToken(methodDef.MDToken.Table, rid).Raw;
		}
		if (method is MemberRef { IsMethodRef: not false } memberRef)
		{
			return metadata.GetToken(memberRef).Raw;
		}
		Error("Not a method");
		return 0u;
	}
}
