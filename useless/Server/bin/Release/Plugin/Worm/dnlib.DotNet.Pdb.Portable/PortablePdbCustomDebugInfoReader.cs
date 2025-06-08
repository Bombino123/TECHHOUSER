using System;
using System.Collections.Generic;
using System.IO;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Portable;

internal struct PortablePdbCustomDebugInfoReader
{
	private readonly ModuleDef module;

	private readonly TypeDef typeOpt;

	private readonly CilBody bodyOpt;

	private readonly GenericParamContext gpContext;

	private DataReader reader;

	public static PdbCustomDebugInfo Read(ModuleDef module, TypeDef typeOpt, CilBody bodyOpt, GenericParamContext gpContext, Guid kind, ref DataReader reader)
	{
		try
		{
			return new PortablePdbCustomDebugInfoReader(module, typeOpt, bodyOpt, gpContext, ref reader).Read(kind);
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
		return null;
	}

	private PortablePdbCustomDebugInfoReader(ModuleDef module, TypeDef typeOpt, CilBody bodyOpt, GenericParamContext gpContext, ref DataReader reader)
	{
		this.module = module;
		this.typeOpt = typeOpt;
		this.bodyOpt = bodyOpt;
		this.gpContext = gpContext;
		this.reader = reader;
	}

	private PdbCustomDebugInfo Read(Guid kind)
	{
		if (kind == CustomDebugInfoGuids.AsyncMethodSteppingInformationBlob)
		{
			return ReadAsyncMethodSteppingInformationBlob();
		}
		if (kind == CustomDebugInfoGuids.DefaultNamespace)
		{
			return ReadDefaultNamespace();
		}
		if (kind == CustomDebugInfoGuids.DynamicLocalVariables)
		{
			return ReadDynamicLocalVariables(reader.Length);
		}
		if (kind == CustomDebugInfoGuids.EmbeddedSource)
		{
			return ReadEmbeddedSource();
		}
		if (kind == CustomDebugInfoGuids.EncLambdaAndClosureMap)
		{
			return ReadEncLambdaAndClosureMap(reader.Length);
		}
		if (kind == CustomDebugInfoGuids.EncLocalSlotMap)
		{
			return ReadEncLocalSlotMap(reader.Length);
		}
		if (kind == CustomDebugInfoGuids.SourceLink)
		{
			return ReadSourceLink();
		}
		if (kind == CustomDebugInfoGuids.StateMachineHoistedLocalScopes)
		{
			return ReadStateMachineHoistedLocalScopes();
		}
		if (kind == CustomDebugInfoGuids.TupleElementNames)
		{
			return ReadTupleElementNames();
		}
		if (kind == CustomDebugInfoGuids.CompilationMetadataReferences)
		{
			return ReadCompilationMetadataReferences();
		}
		if (kind == CustomDebugInfoGuids.CompilationOptions)
		{
			return ReadCompilationOptions();
		}
		if (kind == CustomDebugInfoGuids.TypeDefinitionDocuments)
		{
			return ReadTypeDefinitionDocuments();
		}
		if (kind == CustomDebugInfoGuids.EncStateMachineStateMap)
		{
			return ReadEncStateMachineStateMap();
		}
		if (kind == CustomDebugInfoGuids.PrimaryConstructorInformationBlob)
		{
			return ReadPrimaryConstructorInformationBlob();
		}
		return new PdbUnknownCustomDebugInfo(kind, reader.ReadRemainingBytes());
	}

	private PdbCustomDebugInfo ReadAsyncMethodSteppingInformationBlob()
	{
		if (bodyOpt == null)
		{
			return null;
		}
		uint num = reader.ReadUInt32() - 1;
		Instruction instruction;
		if (num == uint.MaxValue)
		{
			instruction = null;
		}
		else
		{
			instruction = GetInstruction(num);
			if (instruction == null)
			{
				return null;
			}
		}
		PdbAsyncMethodSteppingInformationCustomDebugInfo pdbAsyncMethodSteppingInformationCustomDebugInfo = new PdbAsyncMethodSteppingInformationCustomDebugInfo();
		pdbAsyncMethodSteppingInformationCustomDebugInfo.CatchHandler = instruction;
		while (reader.Position < reader.Length)
		{
			Instruction instruction2 = GetInstruction(reader.ReadUInt32());
			if (instruction2 == null)
			{
				return null;
			}
			uint offset = reader.ReadUInt32();
			uint rid = reader.ReadCompressedUInt32();
			MDToken mDToken = new MDToken(Table.Method, rid);
			MethodDef methodDef;
			Instruction instruction3;
			if (gpContext.Method != null && mDToken == gpContext.Method.MDToken)
			{
				methodDef = gpContext.Method;
				instruction3 = GetInstruction(offset);
			}
			else
			{
				methodDef = module.ResolveToken(mDToken, gpContext) as MethodDef;
				if (methodDef == null)
				{
					return null;
				}
				instruction3 = GetInstruction(methodDef, offset);
			}
			if (instruction3 == null)
			{
				return null;
			}
			pdbAsyncMethodSteppingInformationCustomDebugInfo.AsyncStepInfos.Add(new PdbAsyncStepInfo(instruction2, methodDef, instruction3));
		}
		return pdbAsyncMethodSteppingInformationCustomDebugInfo;
	}

	private PdbCustomDebugInfo ReadDefaultNamespace()
	{
		return new PdbDefaultNamespaceCustomDebugInfo(reader.ReadUtf8String((int)reader.BytesLeft));
	}

	private PdbCustomDebugInfo ReadDynamicLocalVariables(long recPosEnd)
	{
		bool[] array = new bool[reader.Length * 8];
		int num = 0;
		while (reader.Position < reader.Length)
		{
			int num2 = reader.ReadByte();
			for (int num3 = 1; num3 < 256; num3 <<= 1)
			{
				array[num++] = (num2 & num3) != 0;
			}
		}
		return new PdbDynamicLocalVariablesCustomDebugInfo(array);
	}

	private PdbCustomDebugInfo ReadEmbeddedSource()
	{
		return new PdbEmbeddedSourceCustomDebugInfo(reader.ReadRemainingBytes());
	}

	private PdbCustomDebugInfo ReadEncLambdaAndClosureMap(long recPosEnd)
	{
		return new PdbEditAndContinueLambdaMapCustomDebugInfo(reader.ReadBytes((int)(recPosEnd - reader.Position)));
	}

	private PdbCustomDebugInfo ReadEncLocalSlotMap(long recPosEnd)
	{
		return new PdbEditAndContinueLocalSlotMapCustomDebugInfo(reader.ReadBytes((int)(recPosEnd - reader.Position)));
	}

	private PdbCustomDebugInfo ReadSourceLink()
	{
		return new PdbSourceLinkCustomDebugInfo(reader.ReadRemainingBytes());
	}

	private PdbCustomDebugInfo ReadStateMachineHoistedLocalScopes()
	{
		if (bodyOpt == null)
		{
			return null;
		}
		int num = (int)(reader.Length / 8);
		PdbStateMachineHoistedLocalScopesCustomDebugInfo pdbStateMachineHoistedLocalScopesCustomDebugInfo = new PdbStateMachineHoistedLocalScopesCustomDebugInfo(num);
		for (int i = 0; i < num; i++)
		{
			uint num2 = reader.ReadUInt32();
			uint num3 = reader.ReadUInt32();
			if (num2 == 0 && num3 == 0)
			{
				pdbStateMachineHoistedLocalScopesCustomDebugInfo.Scopes.Add(default(StateMachineHoistedLocalScope));
				continue;
			}
			Instruction instruction = GetInstruction(num2);
			Instruction instruction2 = GetInstruction(num2 + num3);
			if (instruction == null)
			{
				return null;
			}
			pdbStateMachineHoistedLocalScopesCustomDebugInfo.Scopes.Add(new StateMachineHoistedLocalScope(instruction, instruction2));
		}
		return pdbStateMachineHoistedLocalScopesCustomDebugInfo;
	}

	private PdbCustomDebugInfo ReadTupleElementNames()
	{
		PortablePdbTupleElementNamesCustomDebugInfo portablePdbTupleElementNamesCustomDebugInfo = new PortablePdbTupleElementNamesCustomDebugInfo();
		while (reader.Position < reader.Length)
		{
			string item = ReadUTF8Z(reader.Length);
			portablePdbTupleElementNamesCustomDebugInfo.Names.Add(item);
		}
		return portablePdbTupleElementNamesCustomDebugInfo;
	}

	private string ReadUTF8Z(long recPosEnd)
	{
		if (reader.Position > recPosEnd)
		{
			return null;
		}
		return reader.TryReadZeroTerminatedUtf8String();
	}

	private PdbCustomDebugInfo ReadCompilationMetadataReferences()
	{
		PdbCompilationMetadataReferencesCustomDebugInfo pdbCompilationMetadataReferencesCustomDebugInfo = new PdbCompilationMetadataReferencesCustomDebugInfo();
		while (reader.BytesLeft != 0)
		{
			string text = reader.TryReadZeroTerminatedUtf8String();
			if (text == null)
			{
				break;
			}
			string text2 = reader.TryReadZeroTerminatedUtf8String();
			if (text2 == null || reader.BytesLeft < 25)
			{
				break;
			}
			PdbCompilationMetadataReferenceFlags flags = (PdbCompilationMetadataReferenceFlags)reader.ReadByte();
			uint timestamp = reader.ReadUInt32();
			uint sizeOfImage = reader.ReadUInt32();
			Guid mvid = reader.ReadGuid();
			PdbCompilationMetadataReference item = new PdbCompilationMetadataReference(text, text2, flags, timestamp, sizeOfImage, mvid);
			pdbCompilationMetadataReferencesCustomDebugInfo.References.Add(item);
		}
		return pdbCompilationMetadataReferencesCustomDebugInfo;
	}

	private PdbCustomDebugInfo ReadCompilationOptions()
	{
		PdbCompilationOptionsCustomDebugInfo pdbCompilationOptionsCustomDebugInfo = new PdbCompilationOptionsCustomDebugInfo();
		while (reader.BytesLeft != 0)
		{
			string text = reader.TryReadZeroTerminatedUtf8String();
			if (text == null)
			{
				break;
			}
			string text2 = reader.TryReadZeroTerminatedUtf8String();
			if (text2 == null)
			{
				break;
			}
			pdbCompilationOptionsCustomDebugInfo.Options.Add(new KeyValuePair<string, string>(text, text2));
		}
		return pdbCompilationOptionsCustomDebugInfo;
	}

	private PdbCustomDebugInfo ReadTypeDefinitionDocuments()
	{
		List<MDToken> list = new List<MDToken>();
		while (reader.BytesLeft != 0)
		{
			list.Add(new MDToken(Table.Document, reader.ReadCompressedUInt32()));
		}
		return new PdbTypeDefinitionDocumentsDebugInfoMD(module, list);
	}

	private PdbCustomDebugInfo ReadEncStateMachineStateMap()
	{
		PdbEditAndContinueStateMachineStateMapDebugInfo pdbEditAndContinueStateMachineStateMapDebugInfo = new PdbEditAndContinueStateMachineStateMapDebugInfo();
		uint num = reader.ReadCompressedUInt32();
		if (num != 0)
		{
			long num2 = 0L - (long)reader.ReadCompressedUInt32();
			while (num != 0)
			{
				int state = reader.ReadCompressedInt32();
				int syntaxOffset = (int)(num2 + reader.ReadCompressedUInt32());
				pdbEditAndContinueStateMachineStateMapDebugInfo.StateMachineStates.Add(new StateMachineStateInfo(syntaxOffset, (StateMachineState)state));
				num--;
			}
		}
		return pdbEditAndContinueStateMachineStateMapDebugInfo;
	}

	private PdbCustomDebugInfo ReadPrimaryConstructorInformationBlob()
	{
		return new PrimaryConstructorInformationBlobDebugInfo(reader.ReadRemainingBytes());
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

	private static Instruction GetInstruction(MethodDef method, uint offset)
	{
		if (method == null)
		{
			return null;
		}
		CilBody body = method.Body;
		if (body == null)
		{
			return null;
		}
		IList<Instruction> instructions = body.Instructions;
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
