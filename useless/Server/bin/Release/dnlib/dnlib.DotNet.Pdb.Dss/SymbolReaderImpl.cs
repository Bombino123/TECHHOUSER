using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.DotNet.Pdb.WindowsPdb;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class SymbolReaderImpl : SymbolReader
{
	private ModuleDef module;

	private ISymUnmanagedReader reader;

	private object[] objsToKeepAlive;

	private const int E_FAIL = -2147467259;

	private volatile SymbolDocument[] documents;

	public override PdbFileKind PdbFileKind => PdbFileKind.WindowsPDB;

	public override int UserEntryPoint
	{
		get
		{
			uint pToken;
			int userEntryPoint = reader.GetUserEntryPoint(out pToken);
			if (userEntryPoint == -2147467259)
			{
				return 0;
			}
			Marshal.ThrowExceptionForHR(userEntryPoint);
			return (int)pToken;
		}
	}

	public override IList<SymbolDocument> Documents
	{
		get
		{
			if (documents == null)
			{
				reader.GetDocuments(0u, out var pcDocs, null);
				ISymUnmanagedDocument[] array = new ISymUnmanagedDocument[pcDocs];
				reader.GetDocuments((uint)array.Length, out pcDocs, array);
				SymbolDocument[] array2 = new SymbolDocument[pcDocs];
				for (uint num = 0u; num < pcDocs; num++)
				{
					array2[num] = new SymbolDocumentImpl(array[num]);
				}
				documents = array2;
			}
			return documents;
		}
	}

	public SymbolReaderImpl(ISymUnmanagedReader reader, object[] objsToKeepAlive)
	{
		this.reader = reader ?? throw new ArgumentNullException("reader");
		this.objsToKeepAlive = objsToKeepAlive ?? throw new ArgumentNullException("objsToKeepAlive");
	}

	~SymbolReaderImpl()
	{
		Dispose(disposing: false);
	}

	public override void Initialize(ModuleDef module)
	{
		this.module = module;
	}

	public override SymbolMethod GetMethod(MethodDef method, int version)
	{
		ISymUnmanagedMethod pRetVal;
		int methodByVersion = reader.GetMethodByVersion(method.MDToken.Raw, version, out pRetVal);
		if (methodByVersion == -2147467259)
		{
			return null;
		}
		Marshal.ThrowExceptionForHR(methodByVersion);
		if (pRetVal != null)
		{
			return new SymbolMethodImpl(this, pRetVal);
		}
		return null;
	}

	internal void GetCustomDebugInfos(SymbolMethodImpl symMethod, MethodDef method, CilBody body, IList<PdbCustomDebugInfo> result)
	{
		PdbAsyncMethodCustomDebugInfo pdbAsyncMethodCustomDebugInfo = PseudoCustomDebugInfoFactory.TryCreateAsyncMethod(method.Module, method, body, symMethod.AsyncKickoffMethod, symMethod.AsyncStepInfos, symMethod.AsyncCatchHandlerILOffset);
		if (pdbAsyncMethodCustomDebugInfo != null)
		{
			result.Add(pdbAsyncMethodCustomDebugInfo);
		}
		reader.GetSymAttribute(method.MDToken.Raw, "MD2", 0u, out var pcBuffer, null);
		if (pcBuffer != 0)
		{
			byte[] array = new byte[pcBuffer];
			reader.GetSymAttribute(method.MDToken.Raw, "MD2", (uint)array.Length, out pcBuffer, array);
			PdbCustomDebugInfoReader.Read(method, body, result, array);
		}
	}

	public override void GetCustomDebugInfos(int token, GenericParamContext gpContext, IList<PdbCustomDebugInfo> result)
	{
		if (token == 1)
		{
			GetCustomDebugInfos_ModuleDef(result);
		}
	}

	private void GetCustomDebugInfos_ModuleDef(IList<PdbCustomDebugInfo> result)
	{
		byte[] sourceLinkData = GetSourceLinkData();
		if (sourceLinkData != null)
		{
			result.Add(new PdbSourceLinkCustomDebugInfo(sourceLinkData));
		}
		byte[] sourceServerData = GetSourceServerData();
		if (sourceServerData != null)
		{
			result.Add(new PdbSourceServerCustomDebugInfo(sourceServerData));
		}
	}

	private byte[] GetSourceLinkData()
	{
		if (reader is ISymUnmanagedReader4 symUnmanagedReader && symUnmanagedReader.GetSourceServerData(out var data, out var pcData) == 0)
		{
			if (pcData == 0)
			{
				return Array2.Empty<byte>();
			}
			byte[] array = new byte[pcData];
			Marshal.Copy(data, array, 0, array.Length);
			return array;
		}
		return null;
	}

	private byte[] GetSourceServerData()
	{
		if (reader is ISymUnmanagedSourceServerModule symUnmanagedSourceServerModule)
		{
			IntPtr ppData = IntPtr.Zero;
			try
			{
				if (symUnmanagedSourceServerModule.GetSourceServerData(out var pDataByteCount, out ppData) == 0)
				{
					if (pDataByteCount == 0)
					{
						return Array2.Empty<byte>();
					}
					byte[] array = new byte[pDataByteCount];
					Marshal.Copy(ppData, array, 0, array.Length);
					return array;
				}
			}
			finally
			{
				if (ppData != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(ppData);
				}
			}
		}
		return null;
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		(reader as ISymUnmanagedDispose)?.Destroy();
		object[] array = objsToKeepAlive;
		if (array != null)
		{
			object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				(array2[i] as IDisposable)?.Dispose();
			}
		}
		module = null;
		reader = null;
		objsToKeepAlive = null;
	}

	public bool MatchesModule(Guid pdbId, uint stamp, uint age)
	{
		if (reader is ISymUnmanagedReader4 symUnmanagedReader)
		{
			if (symUnmanagedReader.MatchesModule(pdbId, stamp, age, out var result) < 0)
			{
				return false;
			}
			return result;
		}
		return true;
	}
}
