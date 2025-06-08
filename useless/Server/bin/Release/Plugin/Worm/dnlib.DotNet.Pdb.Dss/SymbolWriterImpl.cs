using System;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Runtime.InteropServices;
using dnlib.DotNet.Pdb.WindowsPdb;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class SymbolWriterImpl : SymbolWriter
{
	private readonly ISymUnmanagedWriter2 writer;

	private readonly ISymUnmanagedAsyncMethodPropertiesWriter asyncMethodWriter;

	private readonly string pdbFileName;

	private readonly Stream pdbStream;

	private readonly bool ownsStream;

	private readonly bool isDeterministic;

	private bool closeCalled;

	public override bool IsDeterministic => isDeterministic;

	public override bool SupportsAsyncMethods => asyncMethodWriter != null;

	public SymbolWriterImpl(ISymUnmanagedWriter2 writer, string pdbFileName, Stream pdbStream, PdbWriterOptions options, bool ownsStream)
	{
		this.writer = writer ?? throw new ArgumentNullException("writer");
		asyncMethodWriter = writer as ISymUnmanagedAsyncMethodPropertiesWriter;
		this.pdbStream = pdbStream ?? throw new ArgumentNullException("pdbStream");
		this.pdbFileName = pdbFileName;
		this.ownsStream = ownsStream;
		isDeterministic = (options & PdbWriterOptions.Deterministic) != 0 && writer is ISymUnmanagedWriter6;
	}

	public override void Close()
	{
		if (!closeCalled)
		{
			closeCalled = true;
			writer.Close();
		}
	}

	public override void CloseMethod()
	{
		writer.CloseMethod();
	}

	public override void CloseScope(int endOffset)
	{
		writer.CloseScope((uint)endOffset);
	}

	public override void DefineAsyncStepInfo(uint[] yieldOffsets, uint[] breakpointOffset, uint[] breakpointMethod)
	{
		if (asyncMethodWriter == null)
		{
			throw new InvalidOperationException();
		}
		if (yieldOffsets.Length != breakpointOffset.Length || yieldOffsets.Length != breakpointMethod.Length)
		{
			throw new ArgumentException();
		}
		asyncMethodWriter.DefineAsyncStepInfo((uint)yieldOffsets.Length, yieldOffsets, breakpointOffset, breakpointMethod);
	}

	public override void DefineCatchHandlerILOffset(uint catchHandlerOffset)
	{
		if (asyncMethodWriter == null)
		{
			throw new InvalidOperationException();
		}
		asyncMethodWriter.DefineCatchHandlerILOffset(catchHandlerOffset);
	}

	public override void DefineConstant(string name, object value, uint sigToken)
	{
		writer.DefineConstant2(name, value, sigToken);
	}

	public override ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
	{
		writer.DefineDocument(url, ref language, ref languageVendor, ref documentType, out var pRetVal);
		if (pRetVal != null)
		{
			return new SymbolDocumentWriter(pRetVal);
		}
		return null;
	}

	public override void DefineKickoffMethod(uint kickoffMethod)
	{
		if (asyncMethodWriter == null)
		{
			throw new InvalidOperationException();
		}
		asyncMethodWriter.DefineKickoffMethod(kickoffMethod);
	}

	public override void DefineSequencePoints(ISymbolDocumentWriter document, uint arraySize, int[] offsets, int[] lines, int[] columns, int[] endLines, int[] endColumns)
	{
		if (!(document is SymbolDocumentWriter symbolDocumentWriter))
		{
			throw new ArgumentException("document isn't a non-null SymbolDocumentWriter instance");
		}
		writer.DefineSequencePoints(symbolDocumentWriter.SymUnmanagedDocumentWriter, arraySize, offsets, lines, columns, endLines, endColumns);
	}

	public override void OpenMethod(MDToken method)
	{
		writer.OpenMethod(method.Raw);
	}

	public override int OpenScope(int startOffset)
	{
		writer.OpenScope((uint)startOffset, out var pRetVal);
		return (int)pRetVal;
	}

	public override void SetSymAttribute(MDToken parent, string name, byte[] data)
	{
		writer.SetSymAttribute(parent.Raw, name, (uint)data.Length, data);
	}

	public override void SetUserEntryPoint(MDToken entryMethod)
	{
		writer.SetUserEntryPoint(entryMethod.Raw);
	}

	public override void UsingNamespace(string fullName)
	{
		writer.UsingNamespace(fullName);
	}

	public unsafe override bool GetDebugInfo(ChecksumAlgorithm pdbChecksumAlgorithm, ref uint pdbAge, out Guid guid, out uint stamp, out IMAGE_DEBUG_DIRECTORY pIDD, out byte[] codeViewData)
	{
		pIDD = default(IMAGE_DEBUG_DIRECTORY);
		codeViewData = null;
		if (isDeterministic)
		{
			((ISymUnmanagedWriter3)writer).Commit();
			long position = pdbStream.Position;
			pdbStream.Position = 0L;
			byte[] array = Hasher.Hash(pdbChecksumAlgorithm, pdbStream, pdbStream.Length);
			pdbStream.Position = position;
			if (writer is ISymUnmanagedWriter8 symUnmanagedWriter)
			{
				RoslynContentIdProvider.GetContentId(array, out guid, out stamp);
				symUnmanagedWriter.UpdateSignature(guid, stamp, pdbAge);
				return true;
			}
			if (writer is ISymUnmanagedWriter7 symUnmanagedWriter2)
			{
				fixed (byte* value = array)
				{
					symUnmanagedWriter2.UpdateSignatureByHashingContent(new IntPtr(value), (uint)array.Length);
				}
			}
		}
		writer.GetDebugInfo(out pIDD, 0u, out var pcData, null);
		codeViewData = new byte[pcData];
		writer.GetDebugInfo(out pIDD, pcData, out pcData, codeViewData);
		if (writer is IPdbWriter pdbWriter)
		{
			byte[] array2 = new byte[16];
			Array.Copy(codeViewData, 4, array2, 0, 16);
			guid = new Guid(array2);
			pdbWriter.GetSignatureAge(out stamp, out var age);
			pdbAge = age;
			return true;
		}
		guid = default(Guid);
		stamp = 0u;
		return false;
	}

	public override void DefineLocalVariable(string name, uint attributes, uint sigToken, uint addrKind, uint addr1, uint addr2, uint addr3, uint startOffset, uint endOffset)
	{
		writer.DefineLocalVariable2(name, attributes, sigToken, addrKind, addr1, addr2, addr3, startOffset, endOffset);
	}

	public override void Initialize(Metadata metadata)
	{
		if (isDeterministic)
		{
			((ISymUnmanagedWriter6)writer).InitializeDeterministic(new MDEmitter(metadata), new StreamIStream(pdbStream));
		}
		else
		{
			writer.Initialize(new MDEmitter(metadata), pdbFileName, new StreamIStream(pdbStream), fFullBuild: true);
		}
	}

	public unsafe override void SetSourceServerData(byte[] data)
	{
		if (data != null && writer is ISymUnmanagedWriter8 symUnmanagedWriter)
		{
			fixed (byte* ptr = data)
			{
				void* value = ptr;
				symUnmanagedWriter.SetSourceServerData(new IntPtr(value), (uint)data.Length);
			}
		}
	}

	public unsafe override void SetSourceLinkData(byte[] data)
	{
		if (data != null && writer is ISymUnmanagedWriter8 symUnmanagedWriter)
		{
			fixed (byte* ptr = data)
			{
				void* value = ptr;
				symUnmanagedWriter.SetSourceLinkData(new IntPtr(value), (uint)data.Length);
			}
		}
	}

	public override void Dispose()
	{
		Marshal.FinalReleaseComObject(writer);
		if (ownsStream)
		{
			pdbStream.Dispose();
		}
	}
}
