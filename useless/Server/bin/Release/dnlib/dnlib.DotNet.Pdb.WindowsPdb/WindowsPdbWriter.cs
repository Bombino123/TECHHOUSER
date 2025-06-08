using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.WindowsPdb;

internal sealed class WindowsPdbWriter : IDisposable
{
	private sealed class SequencePointHelper
	{
		private readonly Dictionary<PdbDocument, bool> checkedPdbDocs = new Dictionary<PdbDocument, bool>();

		private int[] instrOffsets = Array2.Empty<int>();

		private int[] startLines;

		private int[] startColumns;

		private int[] endLines;

		private int[] endColumns;

		public void Write(WindowsPdbWriter pdbWriter, IList<Instruction> instrs)
		{
			checkedPdbDocs.Clear();
			while (true)
			{
				PdbDocument pdbDocument = null;
				bool flag = false;
				int num = 0;
				int i = 0;
				Instruction instruction = null;
				for (int j = 0; j < instrs.Count; j++, i += instruction.GetSize())
				{
					instruction = instrs[j];
					SequencePoint sequencePoint = instruction.SequencePoint;
					if (sequencePoint == null || sequencePoint.Document == null || checkedPdbDocs.ContainsKey(sequencePoint.Document))
					{
						continue;
					}
					if (pdbDocument == null)
					{
						pdbDocument = sequencePoint.Document;
					}
					else if (pdbDocument != sequencePoint.Document)
					{
						flag = true;
						continue;
					}
					if (num >= instrOffsets.Length)
					{
						int num2 = num * 2;
						if (num2 < 64)
						{
							num2 = 64;
						}
						Array.Resize(ref instrOffsets, num2);
						Array.Resize(ref startLines, num2);
						Array.Resize(ref startColumns, num2);
						Array.Resize(ref endLines, num2);
						Array.Resize(ref endColumns, num2);
					}
					instrOffsets[num] = i;
					startLines[num] = sequencePoint.StartLine;
					startColumns[num] = sequencePoint.StartColumn;
					endLines[num] = sequencePoint.EndLine;
					endColumns[num] = sequencePoint.EndColumn;
					num++;
				}
				if (num != 0)
				{
					pdbWriter.writer.DefineSequencePoints(pdbWriter.Add(pdbDocument), (uint)num, instrOffsets, startLines, startColumns, endLines, endColumns);
				}
				if (flag)
				{
					if (pdbDocument != null)
					{
						checkedPdbDocs.Add(pdbDocument, value: true);
					}
					continue;
				}
				break;
			}
		}
	}

	private struct CurrentMethod
	{
		private readonly WindowsPdbWriter pdbWriter;

		public readonly MethodDef Method;

		private readonly Dictionary<Instruction, uint> toOffset;

		public readonly uint BodySize;

		public CurrentMethod(WindowsPdbWriter pdbWriter, MethodDef method, Dictionary<Instruction, uint> toOffset)
		{
			this.pdbWriter = pdbWriter;
			Method = method;
			this.toOffset = toOffset;
			toOffset.Clear();
			uint num = 0u;
			IList<Instruction> instructions = method.Body.Instructions;
			int count = instructions.Count;
			for (int i = 0; i < count; i++)
			{
				Instruction instruction = instructions[i];
				toOffset[instruction] = num;
				num += (uint)instruction.GetSize();
			}
			BodySize = num;
		}

		public readonly int GetOffset(Instruction instr)
		{
			if (instr == null)
			{
				return (int)BodySize;
			}
			if (toOffset.TryGetValue(instr, out var value))
			{
				return (int)value;
			}
			pdbWriter.Error("Instruction was removed from the body but is referenced from PdbScope: {0}", instr);
			return (int)BodySize;
		}
	}

	private SymbolWriter writer;

	private readonly PdbState pdbState;

	private readonly ModuleDef module;

	private readonly dnlib.DotNet.Writer.Metadata metadata;

	private readonly Dictionary<PdbDocument, ISymbolDocumentWriter> pdbDocs = new Dictionary<PdbDocument, ISymbolDocumentWriter>();

	private readonly SequencePointHelper seqPointsHelper = new SequencePointHelper();

	private readonly Dictionary<Instruction, uint> instrToOffset;

	private readonly PdbCustomDebugInfoWriterContext customDebugInfoWriterContext;

	private readonly int localsEndScopeIncValue;

	private static readonly object boxedZeroInt32 = 0;

	public ILogger Logger { get; set; }

	public WindowsPdbWriter(SymbolWriter writer, PdbState pdbState, dnlib.DotNet.Writer.Metadata metadata)
		: this(pdbState, metadata)
	{
		if (pdbState == null)
		{
			throw new ArgumentNullException("pdbState");
		}
		if (metadata == null)
		{
			throw new ArgumentNullException("metadata");
		}
		this.writer = writer ?? throw new ArgumentNullException("writer");
		writer.Initialize(metadata);
	}

	private WindowsPdbWriter(PdbState pdbState, dnlib.DotNet.Writer.Metadata metadata)
	{
		this.pdbState = pdbState;
		this.metadata = metadata;
		module = metadata.Module;
		instrToOffset = new Dictionary<Instruction, uint>();
		customDebugInfoWriterContext = new PdbCustomDebugInfoWriterContext();
		localsEndScopeIncValue = (PdbUtils.IsEndInclusive(PdbFileKind.WindowsPDB, pdbState.Compiler) ? 1 : 0);
	}

	private ISymbolDocumentWriter Add(PdbDocument pdbDoc)
	{
		if (pdbDocs.TryGetValue(pdbDoc, out var value))
		{
			return value;
		}
		value = writer.DefineDocument(pdbDoc.Url, pdbDoc.Language, pdbDoc.LanguageVendor, pdbDoc.DocumentType);
		value.SetCheckSum(pdbDoc.CheckSumAlgorithmId, pdbDoc.CheckSum);
		if (TryGetCustomDebugInfo<PdbEmbeddedSourceCustomDebugInfo>(pdbDoc, out var cdi))
		{
			value.SetSource(cdi.SourceCodeBlob);
		}
		pdbDocs.Add(pdbDoc, value);
		return value;
	}

	private static bool TryGetCustomDebugInfo<TCDI>(IHasCustomDebugInformation hci, out TCDI cdi) where TCDI : PdbCustomDebugInfo
	{
		IList<PdbCustomDebugInfo> customDebugInfos = hci.CustomDebugInfos;
		int count = customDebugInfos.Count;
		for (int i = 0; i < count; i++)
		{
			if (customDebugInfos[i] is TCDI val)
			{
				cdi = val;
				return true;
			}
		}
		cdi = null;
		return false;
	}

	public void Write()
	{
		writer.SetUserEntryPoint(GetUserEntryPointToken());
		List<PdbCustomDebugInfo> cdiBuilder = new List<PdbCustomDebugInfo>();
		foreach (TypeDef type in module.GetTypes())
		{
			if (type == null)
			{
				continue;
			}
			IList<MethodDef> methods = type.Methods;
			int count = methods.Count;
			for (int i = 0; i < count; i++)
			{
				MethodDef methodDef = methods[i];
				if (methodDef != null && ShouldAddMethod(methodDef))
				{
					Write(methodDef, cdiBuilder);
				}
			}
		}
		if (TryGetCustomDebugInfo<PdbSourceLinkCustomDebugInfo>(module, out var cdi))
		{
			writer.SetSourceLinkData(cdi.FileBlob);
		}
		if (TryGetCustomDebugInfo<PdbSourceServerCustomDebugInfo>(module, out var cdi2))
		{
			writer.SetSourceServerData(cdi2.FileBlob);
		}
	}

	private bool ShouldAddMethod(MethodDef method)
	{
		CilBody body = method.Body;
		if (body == null)
		{
			return false;
		}
		if (body.HasPdbMethod)
		{
			return true;
		}
		LocalList variables = body.Variables;
		int count = variables.Count;
		for (int i = 0; i < count; i++)
		{
			Local local = variables[i];
			if (local.Name != null)
			{
				return true;
			}
			if (local.Attributes != 0)
			{
				return true;
			}
		}
		IList<Instruction> instructions = body.Instructions;
		count = instructions.Count;
		for (int j = 0; j < count; j++)
		{
			if (instructions[j].SequencePoint != null)
			{
				return true;
			}
		}
		return false;
	}

	private void Write(MethodDef method, List<PdbCustomDebugInfo> cdiBuilder)
	{
		uint rid = metadata.GetRid(method);
		if (rid == 0)
		{
			Error("Method {0} ({1:X8}) is not defined in this module ({2})", method, method.MDToken.Raw, module);
			return;
		}
		CurrentMethod info = new CurrentMethod(this, method, instrToOffset);
		CilBody body = method.Body;
		MDToken mDToken = new MDToken(Table.Method, rid);
		writer.OpenMethod(mDToken);
		seqPointsHelper.Write(this, info.Method.Body.Instructions);
		PdbMethod pdbMethod = body.PdbMethod;
		if (pdbMethod == null)
		{
			pdbMethod = (body.PdbMethod = new PdbMethod());
		}
		PdbScope pdbScope = pdbMethod.Scope;
		if (pdbScope == null)
		{
			pdbScope = (pdbMethod.Scope = new PdbScope());
		}
		if (pdbScope.Namespaces.Count == 0 && pdbScope.Variables.Count == 0 && pdbScope.Constants.Count == 0)
		{
			if (pdbScope.Scopes.Count == 0)
			{
				writer.OpenScope(0);
				writer.CloseScope((int)info.BodySize);
			}
			else
			{
				IList<PdbScope> scopes = pdbScope.Scopes;
				int count = scopes.Count;
				for (int i = 0; i < count; i++)
				{
					WriteScope(ref info, scopes[i], 0);
				}
			}
		}
		else
		{
			WriteScope(ref info, pdbScope, 0);
		}
		GetPseudoCustomDebugInfos(method.CustomDebugInfos, cdiBuilder, out var asyncMethod);
		if (cdiBuilder.Count != 0)
		{
			customDebugInfoWriterContext.Logger = GetLogger();
			byte[] array = PdbCustomDebugInfoWriter.Write(metadata, method, customDebugInfoWriterContext, cdiBuilder);
			if (array != null)
			{
				writer.SetSymAttribute(mDToken, "MD2", array);
			}
		}
		if (asyncMethod != null)
		{
			if (!writer.SupportsAsyncMethods)
			{
				Error("PDB symbol writer doesn't support writing async methods");
			}
			else
			{
				WriteAsyncMethod(ref info, asyncMethod);
			}
		}
		writer.CloseMethod();
	}

	private void GetPseudoCustomDebugInfos(IList<PdbCustomDebugInfo> customDebugInfos, List<PdbCustomDebugInfo> cdiBuilder, out PdbAsyncMethodCustomDebugInfo asyncMethod)
	{
		cdiBuilder.Clear();
		asyncMethod = null;
		int count = customDebugInfos.Count;
		for (int i = 0; i < count; i++)
		{
			PdbCustomDebugInfo pdbCustomDebugInfo = customDebugInfos[i];
			if (pdbCustomDebugInfo.Kind == PdbCustomDebugInfoKind.AsyncMethod)
			{
				if (asyncMethod != null)
				{
					Error("Duplicate async method custom debug info");
				}
				else
				{
					asyncMethod = (PdbAsyncMethodCustomDebugInfo)pdbCustomDebugInfo;
				}
			}
			else if ((uint)pdbCustomDebugInfo.Kind > 255u)
			{
				Error("Custom debug info {0} isn't supported by Windows PDB files", pdbCustomDebugInfo.Kind);
			}
			else
			{
				cdiBuilder.Add(pdbCustomDebugInfo);
			}
		}
	}

	private uint GetMethodToken(MethodDef method)
	{
		uint rid = metadata.GetRid(method);
		if (rid == 0)
		{
			Error("Method {0} ({1:X8}) is not defined in this module ({2})", method, method.MDToken.Raw, module);
		}
		return new MDToken(Table.Method, rid).Raw;
	}

	private void WriteAsyncMethod(ref CurrentMethod info, PdbAsyncMethodCustomDebugInfo asyncMethod)
	{
		if (asyncMethod.KickoffMethod == null)
		{
			Error("KickoffMethod is null");
			return;
		}
		uint methodToken = GetMethodToken(asyncMethod.KickoffMethod);
		writer.DefineKickoffMethod(methodToken);
		if (asyncMethod.CatchHandlerInstruction != null)
		{
			int offset = info.GetOffset(asyncMethod.CatchHandlerInstruction);
			writer.DefineCatchHandlerILOffset((uint)offset);
		}
		IList<PdbAsyncStepInfo> stepInfos = asyncMethod.StepInfos;
		uint[] array = new uint[stepInfos.Count];
		uint[] array2 = new uint[stepInfos.Count];
		uint[] array3 = new uint[stepInfos.Count];
		for (int i = 0; i < array.Length; i++)
		{
			PdbAsyncStepInfo pdbAsyncStepInfo = stepInfos[i];
			if (pdbAsyncStepInfo.YieldInstruction == null)
			{
				Error("YieldInstruction is null");
				return;
			}
			if (pdbAsyncStepInfo.BreakpointMethod == null)
			{
				Error("BreakpointMethod is null");
				return;
			}
			if (pdbAsyncStepInfo.BreakpointInstruction == null)
			{
				Error("BreakpointInstruction is null");
				return;
			}
			array[i] = (uint)info.GetOffset(pdbAsyncStepInfo.YieldInstruction);
			array2[i] = (uint)GetExternalInstructionOffset(ref info, pdbAsyncStepInfo.BreakpointMethod, pdbAsyncStepInfo.BreakpointInstruction);
			array3[i] = GetMethodToken(pdbAsyncStepInfo.BreakpointMethod);
		}
		writer.DefineAsyncStepInfo(array, array2, array3);
	}

	private int GetExternalInstructionOffset(ref CurrentMethod info, MethodDef method, Instruction instr)
	{
		if (info.Method == method)
		{
			return info.GetOffset(instr);
		}
		CilBody body = method.Body;
		if (body == null)
		{
			Error("Method body is null");
			return 0;
		}
		IList<Instruction> instructions = body.Instructions;
		int num = 0;
		for (int i = 0; i < instructions.Count; i++)
		{
			Instruction instruction = instructions[i];
			if (instruction == instr)
			{
				return num;
			}
			num += instruction.GetSize();
		}
		if (instr == null)
		{
			return num;
		}
		Error("Async method instruction has been removed but it's still being referenced by PDB info: BP Instruction: {0}, BP Method: {1} (0x{2:X8}), Current Method: {3} (0x{4:X8})", instr, method, method.MDToken.Raw, info.Method, info.Method.MDToken.Raw);
		return 0;
	}

	private void WriteScope(ref CurrentMethod info, PdbScope scope, int recursionCounter)
	{
		if (recursionCounter >= 1000)
		{
			Error("Too many PdbScopes");
			return;
		}
		int offset = info.GetOffset(scope.Start);
		int offset2 = info.GetOffset(scope.End);
		writer.OpenScope(offset);
		AddLocals(info.Method, scope.Variables, (uint)offset, (uint)offset2);
		if (scope.Constants.Count > 0)
		{
			IList<PdbConstant> constants = scope.Constants;
			FieldSig fieldSig = new FieldSig();
			for (int i = 0; i < constants.Count; i++)
			{
				PdbConstant pdbConstant = constants[i];
				fieldSig.Type = pdbConstant.Type;
				MDToken token = metadata.GetToken(fieldSig);
				writer.DefineConstant(pdbConstant.Name, pdbConstant.Value ?? boxedZeroInt32, token.Raw);
			}
		}
		IList<string> namespaces = scope.Namespaces;
		int count = namespaces.Count;
		for (int j = 0; j < count; j++)
		{
			writer.UsingNamespace(namespaces[j]);
		}
		IList<PdbScope> scopes = scope.Scopes;
		count = scopes.Count;
		for (int k = 0; k < count; k++)
		{
			WriteScope(ref info, scopes[k], recursionCounter + 1);
		}
		writer.CloseScope((offset == 0 && offset2 == info.BodySize) ? offset2 : (offset2 - localsEndScopeIncValue));
	}

	private void AddLocals(MethodDef method, IList<PdbLocal> locals, uint startOffset, uint endOffset)
	{
		if (locals.Count == 0)
		{
			return;
		}
		uint localVarSigToken = metadata.GetLocalVarSigToken(method);
		if (localVarSigToken == 0)
		{
			Error("Method {0} ({1:X8}) has no local signature token", method, method.MDToken.Raw);
			return;
		}
		int count = locals.Count;
		for (int i = 0; i < count; i++)
		{
			PdbLocal pdbLocal = locals[i];
			uint pdbLocalFlags = GetPdbLocalFlags(pdbLocal.Attributes);
			if (pdbLocalFlags != 0 || pdbLocal.Name != null)
			{
				writer.DefineLocalVariable(pdbLocal.Name ?? string.Empty, pdbLocalFlags, localVarSigToken, 1u, (uint)pdbLocal.Index, 0u, 0u, startOffset, endOffset);
			}
		}
	}

	private static uint GetPdbLocalFlags(PdbLocalAttributes attributes)
	{
		if ((attributes & PdbLocalAttributes.DebuggerHidden) != 0)
		{
			return 1u;
		}
		return 0u;
	}

	private MDToken GetUserEntryPointToken()
	{
		MethodDef userEntryPoint = pdbState.UserEntryPoint;
		if (userEntryPoint == null)
		{
			return default(MDToken);
		}
		uint rid = metadata.GetRid(userEntryPoint);
		if (rid == 0)
		{
			Error("PDB user entry point method {0} ({1:X8}) is not defined in this module ({2})", userEntryPoint, userEntryPoint.MDToken.Raw, module);
			return default(MDToken);
		}
		return new MDToken(Table.Method, rid);
	}

	public bool GetDebugInfo(ChecksumAlgorithm pdbChecksumAlgorithm, ref uint pdbAge, out Guid guid, out uint stamp, out IMAGE_DEBUG_DIRECTORY idd, out byte[] codeViewData)
	{
		return writer.GetDebugInfo(pdbChecksumAlgorithm, ref pdbAge, out guid, out stamp, out idd, out codeViewData);
	}

	public void Close()
	{
		writer.Close();
	}

	private ILogger GetLogger()
	{
		return Logger ?? DummyLogger.ThrowModuleWriterExceptionOnErrorInstance;
	}

	private void Error(string message, params object[] args)
	{
		GetLogger().Log(this, LoggerEvent.Error, message, args);
	}

	public void Dispose()
	{
		if (writer != null)
		{
			Close();
		}
		writer?.Dispose();
		writer = null;
	}
}
