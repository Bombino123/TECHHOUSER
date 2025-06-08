using System;
using System.Collections.Generic;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Portable;

internal sealed class PortablePdbReader : SymbolReader
{
	private readonly PdbFileKind pdbFileKind;

	private ModuleDef module;

	private readonly Metadata pdbMetadata;

	private SymbolDocument[] documents;

	public override PdbFileKind PdbFileKind => pdbFileKind;

	public override int UserEntryPoint => pdbMetadata.PdbStream.EntryPoint.ToInt32();

	public override IList<SymbolDocument> Documents => documents;

	public PortablePdbReader(DataReaderFactory pdbStream, PdbFileKind pdbFileKind)
	{
		this.pdbFileKind = pdbFileKind;
		pdbMetadata = MetadataFactory.CreateStandalonePortablePDB(pdbStream, verify: true);
	}

	internal bool MatchesModule(Guid pdbGuid, uint timestamp, uint age)
	{
		PdbStream pdbStream = pdbMetadata.PdbStream;
		if (pdbStream != null)
		{
			byte[] array = pdbStream.Id;
			Array.Resize(ref array, 16);
			if (new Guid(array) != pdbGuid)
			{
				return false;
			}
			if (BitConverter.ToUInt32(pdbStream.Id, 16) != timestamp)
			{
				return false;
			}
			if (age != 1)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override void Initialize(ModuleDef module)
	{
		this.module = module;
		documents = ReadDocuments();
	}

	private static Guid GetLanguageVendor(Guid language)
	{
		if (language == PdbDocumentConstants.LanguageCSharp || language == PdbDocumentConstants.LanguageVisualBasic || language == PdbDocumentConstants.LanguageFSharp)
		{
			return PdbDocumentConstants.LanguageVendorMicrosoft;
		}
		return Guid.Empty;
	}

	private SymbolDocument[] ReadDocuments()
	{
		SymbolDocument[] array = new SymbolDocument[pdbMetadata.TablesStream.DocumentTable.Rows];
		DocumentNameReader documentNameReader = new DocumentNameReader(pdbMetadata.BlobStream);
		List<PdbCustomDebugInfo> list = ListCache<PdbCustomDebugInfo>.AllocList();
		GenericParamContext gpContext = default(GenericParamContext);
		for (int i = 0; i < array.Length; i++)
		{
			uint rid = (uint)(i + 1);
			pdbMetadata.TablesStream.TryReadDocumentRow(rid, out var row);
			string url = documentNameReader.ReadDocumentName(row.Name);
			Guid language = pdbMetadata.GuidStream.Read(row.Language) ?? Guid.Empty;
			Guid languageVendor = GetLanguageVendor(language);
			Guid documentTypeText = PdbDocumentConstants.DocumentTypeText;
			Guid checkSumAlgorithmId = pdbMetadata.GuidStream.Read(row.HashAlgorithm) ?? Guid.Empty;
			byte[] checkSum = pdbMetadata.BlobStream.ReadNoNull(row.Hash);
			MDToken mdToken = new MDToken(Table.Document, rid);
			int token = mdToken.ToInt32();
			list.Clear();
			GetCustomDebugInfos(token, gpContext, list);
			PdbCustomDebugInfo[] customDebugInfos = ((list.Count == 0) ? Array2.Empty<PdbCustomDebugInfo>() : list.ToArray());
			array[i] = new SymbolDocumentImpl(url, language, languageVendor, documentTypeText, checkSumAlgorithmId, checkSum, customDebugInfos, mdToken);
		}
		ListCache<PdbCustomDebugInfo>.Free(ref list);
		return array;
	}

	private bool TryGetSymbolDocument(uint rid, out SymbolDocument document)
	{
		int num = (int)(rid - 1);
		if ((uint)num >= (uint)documents.Length)
		{
			document = null;
			return false;
		}
		document = documents[num];
		return true;
	}

	public override SymbolMethod GetMethod(MethodDef method, int version)
	{
		if (version != 1)
		{
			return null;
		}
		MDTable methodDebugInformationTable = pdbMetadata.TablesStream.MethodDebugInformationTable;
		uint rid = method.Rid;
		if (!methodDebugInformationTable.IsValidRID(rid))
		{
			return null;
		}
		SymbolSequencePoint[] sequencePoints = ReadSequencePoints(rid) ?? Array2.Empty<SymbolSequencePoint>();
		GenericParamContext gpContext = GenericParamContext.Create(method);
		SymbolScopeImpl symbolScopeImpl = ReadScope(rid, gpContext);
		int kickoffMethod = GetKickoffMethod(rid);
		return symbolScopeImpl.method = new SymbolMethodImpl(this, method.MDToken.ToInt32(), symbolScopeImpl, sequencePoints, kickoffMethod);
	}

	private int GetKickoffMethod(uint methodRid)
	{
		uint stateMachineMethodRid = pdbMetadata.GetStateMachineMethodRid(methodRid);
		if (stateMachineMethodRid == 0)
		{
			return 0;
		}
		if (!pdbMetadata.TablesStream.TryReadStateMachineMethodRow(stateMachineMethodRid, out var row))
		{
			return 0;
		}
		return (int)(100663296 + row.KickoffMethod);
	}

	private SymbolSequencePoint[] ReadSequencePoints(uint methodRid)
	{
		if (!pdbMetadata.TablesStream.MethodDebugInformationTable.IsValidRID(methodRid))
		{
			return null;
		}
		if (!pdbMetadata.TablesStream.TryReadMethodDebugInformationRow(methodRid, out var row))
		{
			return null;
		}
		if (row.SequencePoints == 0)
		{
			return null;
		}
		uint num = row.Document;
		if (!pdbMetadata.BlobStream.TryCreateReader(row.SequencePoints, out var reader))
		{
			return null;
		}
		List<SymbolSequencePoint> list = ListCache<SymbolSequencePoint>.AllocList();
		reader.ReadCompressedUInt32();
		if (num == 0)
		{
			num = reader.ReadCompressedUInt32();
		}
		TryGetSymbolDocument(num, out var document);
		uint num2 = uint.MaxValue;
		int num3 = -1;
		int num4 = 0;
		bool flag = false;
		while (reader.Position < reader.Length)
		{
			uint num5 = reader.ReadCompressedUInt32();
			if (num5 == 0 && flag)
			{
				num = reader.ReadCompressedUInt32();
				TryGetSymbolDocument(num, out document);
			}
			else
			{
				if (document == null)
				{
					return null;
				}
				SymbolSequencePoint symbolSequencePoint = default(SymbolSequencePoint);
				symbolSequencePoint.Document = document;
				SymbolSequencePoint item = symbolSequencePoint;
				if (num2 == uint.MaxValue)
				{
					num2 = num5;
				}
				else
				{
					if (num5 == 0)
					{
						return null;
					}
					num2 += num5;
				}
				item.Offset = (int)num2;
				uint num6 = reader.ReadCompressedUInt32();
				int num7 = ((num6 == 0) ? ((int)reader.ReadCompressedUInt32()) : reader.ReadCompressedInt32());
				if (num6 == 0 && num7 == 0)
				{
					item.Line = 16707566;
					item.EndLine = 16707566;
					item.Column = 0;
					item.EndColumn = 0;
				}
				else
				{
					if (num3 < 0)
					{
						num3 = (int)reader.ReadCompressedUInt32();
						num4 = (int)reader.ReadCompressedUInt32();
					}
					else
					{
						num3 += reader.ReadCompressedInt32();
						num4 += reader.ReadCompressedInt32();
					}
					item.Line = num3;
					item.EndLine = num3 + (int)num6;
					item.Column = num4;
					item.EndColumn = num4 + num7;
				}
				list.Add(item);
			}
			flag = true;
		}
		return ListCache<SymbolSequencePoint>.FreeAndToArray(ref list);
	}

	private SymbolScopeImpl ReadScope(uint methodRid, GenericParamContext gpContext)
	{
		RidList localScopeRidList = pdbMetadata.GetLocalScopeRidList(methodRid);
		SymbolScopeImpl symbolScopeImpl = null;
		if (localScopeRidList.Count != 0)
		{
			List<PdbCustomDebugInfo> list = ListCache<PdbCustomDebugInfo>.AllocList();
			List<SymbolScopeImpl> list2 = ListCache<SymbolScopeImpl>.AllocList();
			ImportScopeBlobReader importScopeBlobReader = new ImportScopeBlobReader(module, pdbMetadata.BlobStream);
			for (int i = 0; i < localScopeRidList.Count; i++)
			{
				uint num = localScopeRidList[i];
				int token = new MDToken(Table.LocalScope, num).ToInt32();
				pdbMetadata.TablesStream.TryReadLocalScopeRow(num, out var row);
				uint startOffset = row.StartOffset;
				uint num2 = startOffset + row.Length;
				SymbolScopeImpl symbolScopeImpl2 = null;
				while (list2.Count > 0)
				{
					SymbolScopeImpl symbolScopeImpl3 = list2[list2.Count - 1];
					if (startOffset >= symbolScopeImpl3.StartOffset && num2 <= symbolScopeImpl3.EndOffset)
					{
						symbolScopeImpl2 = symbolScopeImpl3;
						break;
					}
					list2.RemoveAt(list2.Count - 1);
				}
				list.Clear();
				GetCustomDebugInfos(token, gpContext, list);
				PdbCustomDebugInfo[] customDebugInfos = ((list.Count == 0) ? Array2.Empty<PdbCustomDebugInfo>() : list.ToArray());
				SymbolScopeImpl symbolScopeImpl4 = new SymbolScopeImpl(this, symbolScopeImpl2, (int)startOffset, (int)num2, customDebugInfos);
				if (symbolScopeImpl == null)
				{
					symbolScopeImpl = symbolScopeImpl4;
				}
				list2.Add(symbolScopeImpl4);
				symbolScopeImpl2?.childrenList.Add(symbolScopeImpl4);
				symbolScopeImpl4.importScope = ReadPdbImportScope(ref importScopeBlobReader, row.ImportScope, gpContext);
				ReadVariables(symbolScopeImpl4, gpContext, pdbMetadata.GetLocalVariableRidList(num));
				ReadConstants(symbolScopeImpl4, pdbMetadata.GetLocalConstantRidList(num));
			}
			ListCache<SymbolScopeImpl>.Free(ref list2);
			ListCache<PdbCustomDebugInfo>.Free(ref list);
		}
		return symbolScopeImpl ?? new SymbolScopeImpl(this, null, 0, int.MaxValue, Array2.Empty<PdbCustomDebugInfo>());
	}

	private PdbImportScope ReadPdbImportScope(ref ImportScopeBlobReader importScopeBlobReader, uint importScope, GenericParamContext gpContext)
	{
		if (importScope == 0)
		{
			return null;
		}
		PdbImportScope pdbImportScope = null;
		PdbImportScope pdbImportScope2 = null;
		int num = 0;
		while (importScope != 0)
		{
			if (num >= 1000)
			{
				return null;
			}
			int token = new MDToken(Table.ImportScope, importScope).ToInt32();
			if (!pdbMetadata.TablesStream.TryReadImportScopeRow(importScope, out var row))
			{
				return null;
			}
			PdbImportScope pdbImportScope3 = new PdbImportScope();
			GetCustomDebugInfos(token, gpContext, pdbImportScope3.CustomDebugInfos);
			if (pdbImportScope == null)
			{
				pdbImportScope = pdbImportScope3;
			}
			if (pdbImportScope2 != null)
			{
				pdbImportScope2.Parent = pdbImportScope3;
			}
			importScopeBlobReader.Read(row.Imports, pdbImportScope3.Imports);
			pdbImportScope2 = pdbImportScope3;
			importScope = row.Parent;
			num++;
		}
		return pdbImportScope;
	}

	private void ReadVariables(SymbolScopeImpl scope, GenericParamContext gpContext, RidList rids)
	{
		if (rids.Count != 0)
		{
			_ = pdbMetadata.TablesStream.LocalVariableTable;
			List<PdbCustomDebugInfo> list = ListCache<PdbCustomDebugInfo>.AllocList();
			for (int i = 0; i < rids.Count; i++)
			{
				uint rid = rids[i];
				int token = new MDToken(Table.LocalVariable, rid).ToInt32();
				list.Clear();
				GetCustomDebugInfos(token, gpContext, list);
				PdbCustomDebugInfo[] customDebugInfos = ((list.Count == 0) ? Array2.Empty<PdbCustomDebugInfo>() : list.ToArray());
				pdbMetadata.TablesStream.TryReadLocalVariableRow(rid, out var row);
				UTF8String uTF8String = pdbMetadata.StringsStream.Read(row.Name);
				scope.localsList.Add(new SymbolVariableImpl(uTF8String, ToSymbolVariableAttributes(row.Attributes), row.Index, customDebugInfos));
			}
			ListCache<PdbCustomDebugInfo>.Free(ref list);
		}
	}

	private static PdbLocalAttributes ToSymbolVariableAttributes(ushort attributes)
	{
		PdbLocalAttributes pdbLocalAttributes = PdbLocalAttributes.None;
		if (((uint)attributes & (true ? 1u : 0u)) != 0)
		{
			pdbLocalAttributes |= PdbLocalAttributes.DebuggerHidden;
		}
		return pdbLocalAttributes;
	}

	private void ReadConstants(SymbolScopeImpl scope, RidList rids)
	{
		if (rids.Count != 0)
		{
			scope.SetConstants(pdbMetadata, rids);
		}
	}

	internal void GetCustomDebugInfos(SymbolMethodImpl symMethod, MethodDef method, CilBody body, IList<PdbCustomDebugInfo> result)
	{
		GetCustomDebugInfos(method.MDToken.ToInt32(), GenericParamContext.Create(method), result, method, body, out var asyncStepInfo);
		if (asyncStepInfo != null)
		{
			PdbAsyncMethodCustomDebugInfo pdbAsyncMethodCustomDebugInfo = TryCreateAsyncMethod(module, symMethod.KickoffMethod, asyncStepInfo.AsyncStepInfos, asyncStepInfo.CatchHandler);
			if (pdbAsyncMethodCustomDebugInfo != null)
			{
				result.Add(pdbAsyncMethodCustomDebugInfo);
			}
		}
		else if (symMethod.KickoffMethod != 0)
		{
			PdbIteratorMethodCustomDebugInfo pdbIteratorMethodCustomDebugInfo = TryCreateIteratorMethod(module, symMethod.KickoffMethod);
			if (pdbIteratorMethodCustomDebugInfo != null)
			{
				result.Add(pdbIteratorMethodCustomDebugInfo);
			}
		}
	}

	private PdbAsyncMethodCustomDebugInfo TryCreateAsyncMethod(ModuleDef module, int asyncKickoffMethod, IList<PdbAsyncStepInfo> asyncStepInfos, Instruction asyncCatchHandler)
	{
		MDToken mdToken = new MDToken(asyncKickoffMethod);
		if (mdToken.Table != Table.Method)
		{
			return null;
		}
		PdbAsyncMethodCustomDebugInfo pdbAsyncMethodCustomDebugInfo = new PdbAsyncMethodCustomDebugInfo(asyncStepInfos.Count);
		pdbAsyncMethodCustomDebugInfo.KickoffMethod = module.ResolveToken(mdToken) as MethodDef;
		pdbAsyncMethodCustomDebugInfo.CatchHandlerInstruction = asyncCatchHandler;
		int count = asyncStepInfos.Count;
		for (int i = 0; i < count; i++)
		{
			pdbAsyncMethodCustomDebugInfo.StepInfos.Add(asyncStepInfos[i]);
		}
		return pdbAsyncMethodCustomDebugInfo;
	}

	private PdbIteratorMethodCustomDebugInfo TryCreateIteratorMethod(ModuleDef module, int iteratorKickoffMethod)
	{
		MDToken mdToken = new MDToken(iteratorKickoffMethod);
		if (mdToken.Table != Table.Method)
		{
			return null;
		}
		return new PdbIteratorMethodCustomDebugInfo(module.ResolveToken(mdToken) as MethodDef);
	}

	public override void GetCustomDebugInfos(int token, GenericParamContext gpContext, IList<PdbCustomDebugInfo> result)
	{
		GetCustomDebugInfos(token, gpContext, result, null, null, out var _);
	}

	private void GetCustomDebugInfos(int token, GenericParamContext gpContext, IList<PdbCustomDebugInfo> result, MethodDef methodOpt, CilBody bodyOpt, out PdbAsyncMethodSteppingInformationCustomDebugInfo asyncStepInfo)
	{
		asyncStepInfo = null;
		MDToken mDToken = new MDToken(token);
		RidList customDebugInformationRidList = pdbMetadata.GetCustomDebugInformationRidList(mDToken.Table, mDToken.Rid);
		if (customDebugInformationRidList.Count == 0)
		{
			return;
		}
		TypeDef typeOpt = methodOpt?.DeclaringType;
		for (int i = 0; i < customDebugInformationRidList.Count; i++)
		{
			uint rid = customDebugInformationRidList[i];
			if (!pdbMetadata.TablesStream.TryReadCustomDebugInformationRow(rid, out var row))
			{
				continue;
			}
			Guid? guid = pdbMetadata.GuidStream.Read(row.Kind);
			if (!pdbMetadata.BlobStream.TryCreateReader(row.Value, out var reader) || !guid.HasValue)
			{
				continue;
			}
			PdbCustomDebugInfo pdbCustomDebugInfo = PortablePdbCustomDebugInfoReader.Read(module, typeOpt, bodyOpt, gpContext, guid.Value, ref reader);
			if (pdbCustomDebugInfo != null)
			{
				if (pdbCustomDebugInfo is PdbAsyncMethodSteppingInformationCustomDebugInfo pdbAsyncMethodSteppingInformationCustomDebugInfo)
				{
					asyncStepInfo = pdbAsyncMethodSteppingInformationCustomDebugInfo;
				}
				else
				{
					result.Add(pdbCustomDebugInfo);
				}
			}
		}
	}

	public override void Dispose()
	{
		pdbMetadata.Dispose();
	}
}
