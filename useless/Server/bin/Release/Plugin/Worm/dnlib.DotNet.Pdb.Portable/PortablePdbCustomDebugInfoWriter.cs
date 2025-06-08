using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.Portable;

internal readonly struct PortablePdbCustomDebugInfoWriter
{
	private readonly IPortablePdbCustomDebugInfoWriterHelper helper;

	private readonly SerializerMethodContext methodContext;

	private readonly Metadata systemMetadata;

	private readonly MemoryStream outStream;

	private readonly DataWriter writer;

	public static byte[] Write(IPortablePdbCustomDebugInfoWriterHelper helper, SerializerMethodContext methodContext, Metadata systemMetadata, PdbCustomDebugInfo cdi, DataWriterContext context)
	{
		return new PortablePdbCustomDebugInfoWriter(helper, methodContext, systemMetadata, context).Write(cdi);
	}

	private PortablePdbCustomDebugInfoWriter(IPortablePdbCustomDebugInfoWriterHelper helper, SerializerMethodContext methodContext, Metadata systemMetadata, DataWriterContext context)
	{
		this.helper = helper;
		this.methodContext = methodContext;
		this.systemMetadata = systemMetadata;
		outStream = context.OutStream;
		writer = context.Writer;
		outStream.SetLength(0L);
		outStream.Position = 0L;
	}

	private byte[] Write(PdbCustomDebugInfo cdi)
	{
		switch (cdi.Kind)
		{
		default:
			helper.Error("Unreachable code, caller should filter these out");
			return null;
		case PdbCustomDebugInfoKind.StateMachineHoistedLocalScopes:
			WriteStateMachineHoistedLocalScopes((PdbStateMachineHoistedLocalScopesCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.EditAndContinueLocalSlotMap:
			WriteEditAndContinueLocalSlotMap((PdbEditAndContinueLocalSlotMapCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.EditAndContinueLambdaMap:
			WriteEditAndContinueLambdaMap((PdbEditAndContinueLambdaMapCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.Unknown:
			WriteUnknown((PdbUnknownCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.TupleElementNames_PortablePdb:
			WriteTupleElementNames((PortablePdbTupleElementNamesCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.DefaultNamespace:
			WriteDefaultNamespace((PdbDefaultNamespaceCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.DynamicLocalVariables:
			WriteDynamicLocalVariables((PdbDynamicLocalVariablesCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.EmbeddedSource:
			WriteEmbeddedSource((PdbEmbeddedSourceCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.SourceLink:
			WriteSourceLink((PdbSourceLinkCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.AsyncMethod:
			WriteAsyncMethodSteppingInformation((PdbAsyncMethodCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.CompilationMetadataReferences:
			WriteCompilationMetadataReferences((PdbCompilationMetadataReferencesCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.CompilationOptions:
			WriteCompilationOptions((PdbCompilationOptionsCustomDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.TypeDefinitionDocuments:
			WriteTypeDefinitionDocuments((PdbTypeDefinitionDocumentsDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.EditAndContinueStateMachineStateMap:
			WriteEditAndContinueStateMachineStateMap((PdbEditAndContinueStateMachineStateMapDebugInfo)cdi);
			break;
		case PdbCustomDebugInfoKind.PrimaryConstructorInformationBlob:
			WritePrimaryConstructorInformationBlob((PrimaryConstructorInformationBlobDebugInfo)cdi);
			break;
		}
		return outStream.ToArray();
	}

	private void WriteUTF8Z(string s)
	{
		if (s.IndexOf('\0') >= 0)
		{
			helper.Error("String must not contain any NUL bytes");
		}
		byte[] bytes = Encoding.UTF8.GetBytes(s);
		writer.WriteBytes(bytes);
		writer.WriteByte(0);
	}

	private void WriteStateMachineHoistedLocalScopes(PdbStateMachineHoistedLocalScopesCustomDebugInfo cdi)
	{
		if (!methodContext.HasBody)
		{
			helper.Error2("Method has no body, can't write custom debug info: {0}.", cdi.Kind);
			return;
		}
		IList<StateMachineHoistedLocalScope> scopes = cdi.Scopes;
		int count = scopes.Count;
		for (int i = 0; i < count; i++)
		{
			StateMachineHoistedLocalScope stateMachineHoistedLocalScope = scopes[i];
			uint num;
			uint num2;
			if (stateMachineHoistedLocalScope.IsSynthesizedLocal)
			{
				num = 0u;
				num2 = 0u;
			}
			else
			{
				Instruction start = stateMachineHoistedLocalScope.Start;
				if (start == null)
				{
					helper.Error("Instruction is null");
					break;
				}
				num = methodContext.GetOffset(start);
				num2 = methodContext.GetOffset(stateMachineHoistedLocalScope.End);
			}
			if (num > num2)
			{
				helper.Error("End instruction is before start instruction");
				break;
			}
			writer.WriteUInt32(num);
			writer.WriteUInt32(num2 - num);
		}
	}

	private void WriteEditAndContinueLocalSlotMap(PdbEditAndContinueLocalSlotMapCustomDebugInfo cdi)
	{
		byte[] data = cdi.Data;
		if (data == null)
		{
			helper.Error("Data blob is null");
		}
		else
		{
			writer.WriteBytes(data);
		}
	}

	private void WriteEditAndContinueLambdaMap(PdbEditAndContinueLambdaMapCustomDebugInfo cdi)
	{
		byte[] data = cdi.Data;
		if (data == null)
		{
			helper.Error("Data blob is null");
		}
		else
		{
			writer.WriteBytes(data);
		}
	}

	private void WriteUnknown(PdbUnknownCustomDebugInfo cdi)
	{
		byte[] data = cdi.Data;
		if (data == null)
		{
			helper.Error("Data blob is null");
		}
		else
		{
			writer.WriteBytes(data);
		}
	}

	private void WriteTupleElementNames(PortablePdbTupleElementNamesCustomDebugInfo cdi)
	{
		IList<string> names = cdi.Names;
		int count = names.Count;
		for (int i = 0; i < count; i++)
		{
			string text = names[i];
			if (text == null)
			{
				helper.Error("Tuple name is null");
				break;
			}
			WriteUTF8Z(text);
		}
	}

	private void WriteDefaultNamespace(PdbDefaultNamespaceCustomDebugInfo cdi)
	{
		string @namespace = cdi.Namespace;
		if (@namespace == null)
		{
			helper.Error("Default namespace is null");
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(@namespace);
		writer.WriteBytes(bytes);
	}

	private void WriteDynamicLocalVariables(PdbDynamicLocalVariablesCustomDebugInfo cdi)
	{
		bool[] flags = cdi.Flags;
		for (int i = 0; i < flags.Length; i += 8)
		{
			writer.WriteByte(ToByte(flags, i));
		}
	}

	private static byte ToByte(bool[] flags, int index)
	{
		int num = 0;
		int num2 = 1;
		int num3 = index;
		while (num3 < flags.Length)
		{
			if (flags[num3])
			{
				num |= num2;
			}
			num3++;
			num2 <<= 1;
		}
		return (byte)num;
	}

	private void WriteEmbeddedSource(PdbEmbeddedSourceCustomDebugInfo cdi)
	{
		byte[] sourceCodeBlob = cdi.SourceCodeBlob;
		if (sourceCodeBlob == null)
		{
			helper.Error("Source code blob is null");
		}
		else
		{
			writer.WriteBytes(sourceCodeBlob);
		}
	}

	private void WriteSourceLink(PdbSourceLinkCustomDebugInfo cdi)
	{
		byte[] fileBlob = cdi.FileBlob;
		if (fileBlob == null)
		{
			helper.Error("Source link blob is null");
		}
		else
		{
			writer.WriteBytes(fileBlob);
		}
	}

	private void WriteAsyncMethodSteppingInformation(PdbAsyncMethodCustomDebugInfo cdi)
	{
		if (!methodContext.HasBody)
		{
			helper.Error2("Method has no body, can't write custom debug info: {0}.", cdi.Kind);
			return;
		}
		uint value = ((cdi.CatchHandlerInstruction != null) ? (methodContext.GetOffset(cdi.CatchHandlerInstruction) + 1) : 0u);
		writer.WriteUInt32(value);
		IList<PdbAsyncStepInfo> stepInfos = cdi.StepInfos;
		int count = stepInfos.Count;
		for (int i = 0; i < count; i++)
		{
			PdbAsyncStepInfo pdbAsyncStepInfo = stepInfos[i];
			if (pdbAsyncStepInfo.YieldInstruction == null)
			{
				helper.Error("YieldInstruction is null");
				break;
			}
			if (pdbAsyncStepInfo.BreakpointMethod == null)
			{
				helper.Error("BreakpointMethod is null");
				break;
			}
			if (pdbAsyncStepInfo.BreakpointInstruction == null)
			{
				helper.Error("BreakpointInstruction is null");
				break;
			}
			uint offset = methodContext.GetOffset(pdbAsyncStepInfo.YieldInstruction);
			uint value2 = ((!methodContext.IsSameMethod(pdbAsyncStepInfo.BreakpointMethod)) ? GetOffsetSlow(pdbAsyncStepInfo.BreakpointMethod, pdbAsyncStepInfo.BreakpointInstruction) : methodContext.GetOffset(pdbAsyncStepInfo.BreakpointInstruction));
			uint rid = systemMetadata.GetRid(pdbAsyncStepInfo.BreakpointMethod);
			writer.WriteUInt32(offset);
			writer.WriteUInt32(value2);
			writer.WriteCompressedUInt32(rid);
		}
	}

	private uint GetOffsetSlow(MethodDef method, Instruction instr)
	{
		CilBody body = method.Body;
		if (body == null)
		{
			helper.Error("Method has no body");
			return uint.MaxValue;
		}
		IList<Instruction> instructions = body.Instructions;
		uint num = 0u;
		for (int i = 0; i < instructions.Count; i++)
		{
			Instruction instruction = instructions[i];
			if (instruction == instr)
			{
				return num;
			}
			num += (uint)instruction.GetSize();
		}
		helper.Error("Couldn't find an instruction, maybe it was removed. It's still being referenced by some code or by the PDB");
		return uint.MaxValue;
	}

	private void WriteCompilationMetadataReferences(PdbCompilationMetadataReferencesCustomDebugInfo cdi)
	{
		foreach (PdbCompilationMetadataReference reference in cdi.References)
		{
			string name = reference.Name;
			if (name == null)
			{
				helper.Error("Metadata reference name is null");
				break;
			}
			WriteUTF8Z(name);
			string aliases = reference.Aliases;
			if (aliases == null)
			{
				helper.Error("Metadata reference aliases is null");
				break;
			}
			WriteUTF8Z(aliases);
			writer.WriteByte((byte)reference.Flags);
			writer.WriteUInt32(reference.Timestamp);
			writer.WriteUInt32(reference.SizeOfImage);
			writer.WriteBytes(reference.Mvid.ToByteArray());
		}
	}

	private void WriteCompilationOptions(PdbCompilationOptionsCustomDebugInfo cdi)
	{
		foreach (KeyValuePair<string, string> option in cdi.Options)
		{
			if (option.Key == null)
			{
				helper.Error("Compiler option `key` is null");
				break;
			}
			if (option.Value == null)
			{
				helper.Error("Compiler option `value` is null");
				break;
			}
			WriteUTF8Z(option.Key);
			WriteUTF8Z(option.Value);
		}
	}

	private void WriteTypeDefinitionDocuments(PdbTypeDefinitionDocumentsDebugInfo cdi)
	{
		foreach (PdbDocument document in cdi.Documents)
		{
			writer.WriteCompressedUInt32(systemMetadata.GetRid(document));
		}
	}

	private void WriteEditAndContinueStateMachineStateMap(PdbEditAndContinueStateMachineStateMapDebugInfo cdi)
	{
		writer.WriteCompressedUInt32((uint)cdi.StateMachineStates.Count);
		if (cdi.StateMachineStates.Count <= 0)
		{
			return;
		}
		int num = Math.Min(cdi.StateMachineStates.Min((StateMachineStateInfo state) => state.SyntaxOffset), 0);
		writer.WriteCompressedUInt32((uint)(-num));
		foreach (StateMachineStateInfo stateMachineState in cdi.StateMachineStates)
		{
			writer.WriteCompressedInt32((int)stateMachineState.State);
			writer.WriteCompressedUInt32((uint)(stateMachineState.SyntaxOffset - num));
		}
	}

	private void WritePrimaryConstructorInformationBlob(PrimaryConstructorInformationBlobDebugInfo cdi)
	{
		byte[] blob = cdi.Blob;
		if (blob == null)
		{
			helper.Error("Primary constructor information blob is null");
		}
		else
		{
			writer.WriteBytes(blob);
		}
	}
}
