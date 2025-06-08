using System;
using System.Runtime.InteropServices;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class ReaderMetaDataImport : MetaDataImport, IDisposable
{
	private Metadata metadata;

	private unsafe byte* blobPtr;

	private IntPtr addrToFree;

	public unsafe ReaderMetaDataImport(Metadata metadata)
	{
		this.metadata = metadata ?? throw new ArgumentNullException("metadata");
		DataReader dataReader = metadata.BlobStream.CreateReader();
		addrToFree = Marshal.AllocHGlobal((int)dataReader.BytesLeft);
		blobPtr = (byte*)(void*)addrToFree;
		if (blobPtr == null)
		{
			throw new OutOfMemoryException();
		}
		dataReader.ReadBytes(blobPtr, (int)dataReader.BytesLeft);
	}

	~ReaderMetaDataImport()
	{
		Dispose(disposing: false);
	}

	public unsafe override void GetTypeRefProps(uint tr, uint* ptkResolutionScope, ushort* szName, uint cchName, uint* pchName)
	{
		MDToken mDToken = new MDToken(tr);
		if (mDToken.Table != Table.TypeRef)
		{
			throw new ArgumentException();
		}
		if (!metadata.TablesStream.TryReadTypeRefRow(mDToken.Rid, out var row))
		{
			throw new ArgumentException();
		}
		if (ptkResolutionScope != null)
		{
			*ptkResolutionScope = row.ResolutionScope;
		}
		if (szName != null || pchName != null)
		{
			UTF8String uTF8String = metadata.StringsStream.ReadNoNull(row.Namespace);
			UTF8String uTF8String2 = metadata.StringsStream.ReadNoNull(row.Name);
			CopyTypeName(uTF8String, uTF8String2, szName, cchName, pchName);
		}
	}

	public unsafe override void GetTypeDefProps(uint td, ushort* szTypeDef, uint cchTypeDef, uint* pchTypeDef, uint* pdwTypeDefFlags, uint* ptkExtends)
	{
		MDToken mDToken = new MDToken(td);
		if (mDToken.Table != Table.TypeDef)
		{
			throw new ArgumentException();
		}
		if (!metadata.TablesStream.TryReadTypeDefRow(mDToken.Rid, out var row))
		{
			throw new ArgumentException();
		}
		if (pdwTypeDefFlags != null)
		{
			*pdwTypeDefFlags = row.Flags;
		}
		if (ptkExtends != null)
		{
			*ptkExtends = row.Extends;
		}
		if (szTypeDef != null || pchTypeDef != null)
		{
			UTF8String uTF8String = metadata.StringsStream.ReadNoNull(row.Namespace);
			UTF8String uTF8String2 = metadata.StringsStream.ReadNoNull(row.Name);
			CopyTypeName(uTF8String, uTF8String2, szTypeDef, cchTypeDef, pchTypeDef);
		}
	}

	public unsafe override void GetSigFromToken(uint mdSig, byte** ppvSig, uint* pcbSig)
	{
		MDToken mDToken = new MDToken(mdSig);
		if (mDToken.Table != Table.StandAloneSig)
		{
			throw new ArgumentException();
		}
		if (!metadata.TablesStream.TryReadStandAloneSigRow(mDToken.Rid, out var row))
		{
			throw new ArgumentException();
		}
		if (!metadata.BlobStream.TryCreateReader(row.Signature, out var reader))
		{
			throw new ArgumentException();
		}
		if (ppvSig != null)
		{
			*ppvSig = blobPtr + (uint)(reader.StartOffset - metadata.BlobStream.StartOffset);
		}
		if (pcbSig != null)
		{
			*pcbSig = reader.Length;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private unsafe void Dispose(bool disposing)
	{
		metadata = null;
		IntPtr intPtr = Interlocked.Exchange(ref addrToFree, IntPtr.Zero);
		blobPtr = null;
		if (intPtr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(intPtr);
		}
	}
}
