using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.Threading;

namespace dnlib.DotNet.Pdb;

[ComVisible(true)]
public sealed class PdbState
{
	private struct CreateScopeState
	{
		public SymbolScope SymScope;

		public PdbScope PdbScope;

		public IList<SymbolScope> Children;

		public int ChildrenIndex;
	}

	private readonly SymbolReader reader;

	private readonly Dictionary<PdbDocument, PdbDocument> docDict = new Dictionary<PdbDocument, PdbDocument>();

	internal readonly Dictionary<MDToken, PdbDocument> tokenToDocument = new Dictionary<MDToken, PdbDocument>();

	private MethodDef userEntryPoint;

	private readonly Compiler compiler;

	private readonly PdbFileKind originalPdbFileKind;

	private readonly Lock theLock = Lock.Create();

	private static readonly UTF8String nameAssemblyVisualBasic = new UTF8String("Microsoft.VisualBasic");

	private static readonly UTF8String nameAssemblyVisualBasicCore = new UTF8String("Microsoft.VisualBasic.Core");

	public PdbFileKind PdbFileKind { get; set; }

	public MethodDef UserEntryPoint
	{
		get
		{
			return userEntryPoint;
		}
		set
		{
			userEntryPoint = value;
		}
	}

	public IEnumerable<PdbDocument> Documents
	{
		get
		{
			theLock.EnterWriteLock();
			try
			{
				return new List<PdbDocument>(docDict.Values);
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public bool HasDocuments
	{
		get
		{
			theLock.EnterWriteLock();
			try
			{
				return docDict.Count > 0;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	internal Compiler Compiler => compiler;

	public PdbState(ModuleDef module, PdbFileKind pdbFileKind)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		compiler = CalculateCompiler(module);
		PdbFileKind = pdbFileKind;
		originalPdbFileKind = pdbFileKind;
	}

	public PdbState(SymbolReader reader, ModuleDefMD module)
	{
		if (module == null)
		{
			throw new ArgumentNullException("module");
		}
		this.reader = reader ?? throw new ArgumentNullException("reader");
		reader.Initialize(module);
		PdbFileKind = reader.PdbFileKind;
		originalPdbFileKind = reader.PdbFileKind;
		compiler = CalculateCompiler(module);
		userEntryPoint = module.ResolveToken(reader.UserEntryPoint) as MethodDef;
		IList<SymbolDocument> documents = reader.Documents;
		int count = documents.Count;
		for (int i = 0; i < count; i++)
		{
			Add_NoLock(documents[i]);
		}
	}

	public PdbDocument Add(PdbDocument doc)
	{
		theLock.EnterWriteLock();
		try
		{
			return Add_NoLock(doc);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private PdbDocument Add_NoLock(PdbDocument doc)
	{
		if (docDict.TryGetValue(doc, out var value))
		{
			return value;
		}
		docDict.Add(doc, doc);
		if (doc.MDToken.HasValue)
		{
			tokenToDocument.Add(doc.MDToken.Value, doc);
		}
		return doc;
	}

	private PdbDocument Add_NoLock(SymbolDocument symDoc)
	{
		PdbDocument pdbDocument = PdbDocument.CreatePartialForCompare(symDoc);
		if (docDict.TryGetValue(pdbDocument, out var value))
		{
			return value;
		}
		pdbDocument.Initialize(symDoc);
		docDict.Add(pdbDocument, pdbDocument);
		if (symDoc.MDToken.HasValue)
		{
			tokenToDocument.Add(symDoc.MDToken.Value, pdbDocument);
		}
		return pdbDocument;
	}

	public bool Remove(PdbDocument doc)
	{
		theLock.EnterWriteLock();
		try
		{
			if (doc.MDToken.HasValue)
			{
				tokenToDocument.Remove(doc.MDToken.Value);
			}
			return docDict.Remove(doc);
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public PdbDocument GetExisting(PdbDocument doc)
	{
		theLock.EnterWriteLock();
		try
		{
			docDict.TryGetValue(doc, out var value);
			return value;
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public void RemoveAllDocuments()
	{
		RemoveAllDocuments(returnDocs: false);
	}

	public List<PdbDocument> RemoveAllDocuments(bool returnDocs)
	{
		theLock.EnterWriteLock();
		try
		{
			List<PdbDocument> result = (returnDocs ? new List<PdbDocument>(docDict.Values) : null);
			tokenToDocument.Clear();
			docDict.Clear();
			return result;
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	internal void InitializeMethodBody(ModuleDefMD module, MethodDef ownerMethod, CilBody body)
	{
		if (reader != null)
		{
			SymbolMethod method = reader.GetMethod(ownerMethod, 1);
			if (method != null)
			{
				PdbMethod pdbMethod = new PdbMethod();
				pdbMethod.Scope = CreateScope(module, GenericParamContext.Create(ownerMethod), body, method.RootScope);
				AddSequencePoints(body, method);
				body.PdbMethod = pdbMethod;
			}
		}
	}

	internal void InitializeCustomDebugInfos(MethodDef ownerMethod, CilBody body, IList<PdbCustomDebugInfo> customDebugInfos)
	{
		if (reader != null)
		{
			reader.GetMethod(ownerMethod, 1)?.GetCustomDebugInfos(ownerMethod, body, customDebugInfos);
		}
	}

	private static Compiler CalculateCompiler(ModuleDef module)
	{
		if (module == null)
		{
			return Compiler.Other;
		}
		foreach (AssemblyRef assemblyRef in module.GetAssemblyRefs())
		{
			if (assemblyRef.Name == nameAssemblyVisualBasic || assemblyRef.Name == nameAssemblyVisualBasicCore)
			{
				return Compiler.VisualBasic;
			}
		}
		return Compiler.Other;
	}

	private void AddSequencePoints(CilBody body, SymbolMethod method)
	{
		int index = 0;
		IList<SymbolSequencePoint> sequencePoints = method.SequencePoints;
		int count = sequencePoints.Count;
		for (int i = 0; i < count; i++)
		{
			SymbolSequencePoint symbolSequencePoint = sequencePoints[i];
			Instruction instruction = GetInstruction(body.Instructions, symbolSequencePoint.Offset, ref index);
			if (instruction != null)
			{
				SequencePoint sequencePoint = new SequencePoint
				{
					Document = Add_NoLock(symbolSequencePoint.Document),
					StartLine = symbolSequencePoint.Line,
					StartColumn = symbolSequencePoint.Column,
					EndLine = symbolSequencePoint.EndLine,
					EndColumn = symbolSequencePoint.EndColumn
				};
				instruction.SequencePoint = sequencePoint;
			}
		}
	}

	private PdbScope CreateScope(ModuleDefMD module, GenericParamContext gpContext, CilBody body, SymbolScope symScope)
	{
		if (symScope == null)
		{
			return null;
		}
		Stack<CreateScopeState> stack = new Stack<CreateScopeState>();
		CreateScopeState createScopeState = default(CreateScopeState);
		createScopeState.SymScope = symScope;
		CreateScopeState item = createScopeState;
		int num = (PdbUtils.IsEndInclusive(originalPdbFileKind, Compiler) ? 1 : 0);
		while (true)
		{
			int index = 0;
			item.PdbScope = new PdbScope
			{
				Start = GetInstruction(body.Instructions, item.SymScope.StartOffset, ref index),
				End = GetInstruction(body.Instructions, item.SymScope.EndOffset + num, ref index)
			};
			IList<PdbCustomDebugInfo> customDebugInfos = item.SymScope.CustomDebugInfos;
			int count = customDebugInfos.Count;
			for (int i = 0; i < count; i++)
			{
				item.PdbScope.CustomDebugInfos.Add(customDebugInfos[i]);
			}
			IList<SymbolVariable> locals = item.SymScope.Locals;
			count = locals.Count;
			for (int j = 0; j < count; j++)
			{
				SymbolVariable symbolVariable = locals[j];
				int index2 = symbolVariable.Index;
				if ((uint)index2 < (uint)body.Variables.Count)
				{
					Local local = body.Variables[index2];
					string name = symbolVariable.Name;
					local.SetName(name);
					PdbLocalAttributes attributes = symbolVariable.Attributes;
					local.SetAttributes(attributes);
					PdbLocal pdbLocal = new PdbLocal(local, name, attributes);
					customDebugInfos = symbolVariable.CustomDebugInfos;
					int count2 = customDebugInfos.Count;
					for (int k = 0; k < count2; k++)
					{
						pdbLocal.CustomDebugInfos.Add(customDebugInfos[k]);
					}
					item.PdbScope.Variables.Add(pdbLocal);
				}
			}
			IList<SymbolNamespace> namespaces = item.SymScope.Namespaces;
			count = namespaces.Count;
			for (int l = 0; l < count; l++)
			{
				item.PdbScope.Namespaces.Add(namespaces[l].Name);
			}
			item.PdbScope.ImportScope = item.SymScope.ImportScope;
			IList<PdbConstant> constants = item.SymScope.GetConstants(module, gpContext);
			PdbConstant pdbConstant;
			for (int m = 0; m < constants.Count; item.PdbScope.Constants.Add(pdbConstant), m++)
			{
				pdbConstant = constants[m];
				TypeSig typeSig = pdbConstant.Type.RemovePinnedAndModifiers();
				if (typeSig == null)
				{
					continue;
				}
				switch (typeSig.ElementType)
				{
				case ElementType.Boolean:
					if (pdbConstant.Value is short)
					{
						pdbConstant.Value = (short)pdbConstant.Value != 0;
					}
					continue;
				case ElementType.Char:
					if (pdbConstant.Value is ushort)
					{
						pdbConstant.Value = (char)(ushort)pdbConstant.Value;
					}
					continue;
				case ElementType.I1:
					if (pdbConstant.Value is short)
					{
						pdbConstant.Value = (sbyte)(short)pdbConstant.Value;
					}
					continue;
				case ElementType.U1:
					if (pdbConstant.Value is short)
					{
						pdbConstant.Value = (byte)(short)pdbConstant.Value;
					}
					continue;
				case ElementType.String:
					if (PdbFileKind == PdbFileKind.WindowsPDB)
					{
						if (pdbConstant.Value is int && (int)pdbConstant.Value == 0)
						{
							pdbConstant.Value = null;
						}
						else if (pdbConstant.Value == null)
						{
							pdbConstant.Value = string.Empty;
						}
					}
					continue;
				case ElementType.GenericInst:
					if (((GenericInstSig)typeSig).GenericType is ValueTypeSig)
					{
						continue;
					}
					break;
				case ElementType.Var:
				case ElementType.MVar:
				{
					GenericParam genericParam = ((GenericSig)typeSig).GenericParam;
					if (genericParam == null || genericParam.HasNotNullableValueTypeConstraint || !genericParam.HasReferenceTypeConstraint)
					{
						continue;
					}
					break;
				}
				case ElementType.Void:
				case ElementType.I2:
				case ElementType.U2:
				case ElementType.I4:
				case ElementType.U4:
				case ElementType.I8:
				case ElementType.U8:
				case ElementType.R4:
				case ElementType.R8:
				case ElementType.Ptr:
				case ElementType.ByRef:
				case ElementType.ValueType:
				case ElementType.TypedByRef:
				case ElementType.I:
				case ElementType.U:
				case ElementType.FnPtr:
					continue;
				}
				if (pdbConstant.Value is int && (int)pdbConstant.Value == 0)
				{
					pdbConstant.Value = null;
				}
			}
			item.ChildrenIndex = 0;
			item.Children = item.SymScope.Children;
			while (item.ChildrenIndex >= item.Children.Count)
			{
				if (stack.Count == 0)
				{
					return item.PdbScope;
				}
				PdbScope pdbScope = item.PdbScope;
				item = stack.Pop();
				item.PdbScope.Scopes.Add(pdbScope);
				item.ChildrenIndex++;
			}
			SymbolScope symScope2 = item.Children[item.ChildrenIndex];
			stack.Push(item);
			createScopeState = default(CreateScopeState);
			createScopeState.SymScope = symScope2;
			item = createScopeState;
		}
	}

	private static Instruction GetInstruction(IList<Instruction> instrs, int offset, ref int index)
	{
		if (instrs.Count > 0 && offset > instrs[instrs.Count - 1].Offset)
		{
			return null;
		}
		for (int i = index; i < instrs.Count; i++)
		{
			Instruction instruction = instrs[i];
			if (instruction.Offset >= offset)
			{
				if (instruction.Offset != offset)
				{
					break;
				}
				index = i;
				return instruction;
			}
		}
		for (int j = 0; j < index; j++)
		{
			Instruction instruction2 = instrs[j];
			if (instruction2.Offset >= offset)
			{
				if (instruction2.Offset != offset)
				{
					break;
				}
				index = j;
				return instruction2;
			}
		}
		return null;
	}

	internal void InitializeCustomDebugInfos(MDToken token, GenericParamContext gpContext, IList<PdbCustomDebugInfo> result)
	{
		reader?.GetCustomDebugInfos(token.ToInt32(), gpContext, result);
	}

	internal void Dispose()
	{
		reader?.Dispose();
	}
}
