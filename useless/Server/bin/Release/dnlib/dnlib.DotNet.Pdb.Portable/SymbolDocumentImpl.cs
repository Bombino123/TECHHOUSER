using System;
using System.Diagnostics;
using System.Text;
using dnlib.DotNet.Pdb.Symbols;

namespace dnlib.DotNet.Pdb.Portable;

[DebuggerDisplay("{GetDebuggerString(),nq}")]
internal sealed class SymbolDocumentImpl : SymbolDocument
{
	private readonly string url;

	private Guid language;

	private Guid languageVendor;

	private Guid documentType;

	private Guid checkSumAlgorithmId;

	private readonly byte[] checkSum;

	private readonly PdbCustomDebugInfo[] customDebugInfos;

	private MDToken mdToken;

	public override string URL => url;

	public override Guid Language => language;

	public override Guid LanguageVendor => languageVendor;

	public override Guid DocumentType => documentType;

	public override Guid CheckSumAlgorithmId => checkSumAlgorithmId;

	public override byte[] CheckSum => checkSum;

	public override PdbCustomDebugInfo[] CustomDebugInfos => customDebugInfos;

	public override MDToken? MDToken => mdToken;

	private string GetDebuggerString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (language == PdbDocumentConstants.LanguageCSharp)
		{
			stringBuilder.Append("C#");
		}
		else if (language == PdbDocumentConstants.LanguageVisualBasic)
		{
			stringBuilder.Append("VB");
		}
		else if (language == PdbDocumentConstants.LanguageFSharp)
		{
			stringBuilder.Append("F#");
		}
		else
		{
			stringBuilder.Append(language.ToString());
		}
		stringBuilder.Append(", ");
		if (checkSumAlgorithmId == PdbDocumentConstants.HashSHA1)
		{
			stringBuilder.Append("SHA-1");
		}
		else if (checkSumAlgorithmId == PdbDocumentConstants.HashSHA256)
		{
			stringBuilder.Append("SHA-256");
		}
		else
		{
			stringBuilder.Append(checkSumAlgorithmId.ToString());
		}
		stringBuilder.Append(": ");
		stringBuilder.Append(url);
		return stringBuilder.ToString();
	}

	public SymbolDocumentImpl(string url, Guid language, Guid languageVendor, Guid documentType, Guid checkSumAlgorithmId, byte[] checkSum, PdbCustomDebugInfo[] customDebugInfos, MDToken mdToken)
	{
		this.url = url;
		this.language = language;
		this.languageVendor = languageVendor;
		this.documentType = documentType;
		this.checkSumAlgorithmId = checkSumAlgorithmId;
		this.checkSum = checkSum;
		this.customDebugInfos = customDebugInfos;
		this.mdToken = mdToken;
	}
}
