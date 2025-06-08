using System;
using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class SymbolDocumentImpl : SymbolDocument
{
	private readonly ISymUnmanagedDocument document;

	private PdbCustomDebugInfo[] customDebugInfos;

	public ISymUnmanagedDocument SymUnmanagedDocument => document;

	public override Guid CheckSumAlgorithmId
	{
		get
		{
			document.GetCheckSumAlgorithmId(out var pRetVal);
			return pRetVal;
		}
	}

	public override Guid DocumentType
	{
		get
		{
			document.GetDocumentType(out var pRetVal);
			return pRetVal;
		}
	}

	public override Guid Language
	{
		get
		{
			document.GetLanguage(out var pRetVal);
			return pRetVal;
		}
	}

	public override Guid LanguageVendor
	{
		get
		{
			document.GetLanguageVendor(out var pRetVal);
			return pRetVal;
		}
	}

	public override string URL
	{
		get
		{
			document.GetURL(0u, out var pcchUrl, null);
			char[] array = new char[pcchUrl];
			document.GetURL((uint)array.Length, out pcchUrl, array);
			if (array.Length == 0)
			{
				return string.Empty;
			}
			return new string(array, 0, array.Length - 1);
		}
	}

	public override byte[] CheckSum
	{
		get
		{
			document.GetCheckSum(0u, out var pcData, null);
			byte[] array = new byte[pcData];
			document.GetCheckSum((uint)array.Length, out pcData, array);
			return array;
		}
	}

	private byte[] SourceCode
	{
		get
		{
			if (document.GetSourceLength(out var pRetVal) < 0)
			{
				return null;
			}
			if (pRetVal <= 0)
			{
				return null;
			}
			byte[] array = new byte[pRetVal];
			if (document.GetSourceRange(0u, 0u, 2147483647u, 2147483647u, pRetVal, out var pcSourceBytes, array) < 0)
			{
				return null;
			}
			if (pcSourceBytes <= 0)
			{
				return null;
			}
			if (pcSourceBytes != array.Length)
			{
				Array.Resize(ref array, pcSourceBytes);
			}
			return array;
		}
	}

	public override PdbCustomDebugInfo[] CustomDebugInfos
	{
		get
		{
			if (customDebugInfos == null)
			{
				byte[] sourceCode = SourceCode;
				if (sourceCode != null)
				{
					customDebugInfos = new PdbCustomDebugInfo[1]
					{
						new PdbEmbeddedSourceCustomDebugInfo(sourceCode)
					};
				}
				else
				{
					customDebugInfos = Array2.Empty<PdbCustomDebugInfo>();
				}
			}
			return customDebugInfos;
		}
	}

	public override MDToken? MDToken => null;

	public SymbolDocumentImpl(ISymUnmanagedDocument document)
	{
		this.document = document;
	}
}
