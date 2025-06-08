using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb;

[DebuggerDisplay("{Url}")]
[ComVisible(true)]
public sealed class PdbDocument : IHasCustomDebugInformation
{
	private IList<PdbCustomDebugInfo> customDebugInfos;

	public string Url { get; set; }

	public Guid Language { get; set; }

	public Guid LanguageVendor { get; set; }

	public Guid DocumentType { get; set; }

	public Guid CheckSumAlgorithmId { get; set; }

	public byte[] CheckSum { get; set; }

	public int HasCustomDebugInformationTag => 22;

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	public IList<PdbCustomDebugInfo> CustomDebugInfos => customDebugInfos;

	public MDToken? MDToken { get; internal set; }

	public PdbDocument()
	{
	}

	public PdbDocument(SymbolDocument symDoc)
		: this(symDoc, partial: false)
	{
	}

	private PdbDocument(SymbolDocument symDoc, bool partial)
	{
		if (symDoc == null)
		{
			throw new ArgumentNullException("symDoc");
		}
		Url = symDoc.URL;
		if (!partial)
		{
			Initialize(symDoc);
		}
	}

	internal static PdbDocument CreatePartialForCompare(SymbolDocument symDoc)
	{
		return new PdbDocument(symDoc, partial: true);
	}

	internal void Initialize(SymbolDocument symDoc)
	{
		Language = symDoc.Language;
		LanguageVendor = symDoc.LanguageVendor;
		DocumentType = symDoc.DocumentType;
		CheckSumAlgorithmId = symDoc.CheckSumAlgorithmId;
		CheckSum = symDoc.CheckSum;
		customDebugInfos = new List<PdbCustomDebugInfo>();
		PdbCustomDebugInfo[] array = symDoc.CustomDebugInfos;
		foreach (PdbCustomDebugInfo item in array)
		{
			customDebugInfos.Add(item);
		}
		MDToken = symDoc.MDToken;
	}

	public PdbDocument(string url, Guid language, Guid languageVendor, Guid documentType, Guid checkSumAlgorithmId, byte[] checkSum)
	{
		Url = url;
		Language = language;
		LanguageVendor = languageVendor;
		DocumentType = documentType;
		CheckSumAlgorithmId = checkSumAlgorithmId;
		CheckSum = checkSum;
	}

	public override int GetHashCode()
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(Url ?? string.Empty);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PdbDocument pdbDocument))
		{
			return false;
		}
		return StringComparer.OrdinalIgnoreCase.Equals(Url ?? string.Empty, pdbDocument.Url ?? string.Empty);
	}
}
