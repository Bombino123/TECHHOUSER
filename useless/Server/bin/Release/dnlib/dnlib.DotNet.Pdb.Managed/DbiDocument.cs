using System;
using System.Diagnostics.SymbolStore;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

internal sealed class DbiDocument : SymbolDocument
{
	private readonly string url;

	private Guid language;

	private Guid languageVendor;

	private Guid documentType;

	private Guid checkSumAlgorithmId;

	private byte[] checkSum;

	private byte[] sourceCode;

	private PdbCustomDebugInfo[] customDebugInfos;

	public override string URL => url;

	public override Guid Language => language;

	public override Guid LanguageVendor => languageVendor;

	public override Guid DocumentType => documentType;

	public override Guid CheckSumAlgorithmId => checkSumAlgorithmId;

	public override byte[] CheckSum => checkSum;

	private byte[] SourceCode => sourceCode;

	public override PdbCustomDebugInfo[] CustomDebugInfos
	{
		get
		{
			if (customDebugInfos == null)
			{
				byte[] array = SourceCode;
				if (array != null)
				{
					customDebugInfos = new PdbCustomDebugInfo[1]
					{
						new PdbEmbeddedSourceCustomDebugInfo(array)
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

	public DbiDocument(string url)
	{
		this.url = url;
		documentType = SymDocumentType.Text;
	}

	public void Read(ref DataReader reader)
	{
		reader.Position = 0u;
		language = reader.ReadGuid();
		languageVendor = reader.ReadGuid();
		documentType = reader.ReadGuid();
		checkSumAlgorithmId = reader.ReadGuid();
		int length = reader.ReadInt32();
		int num = reader.ReadInt32();
		checkSum = reader.ReadBytes(length);
		sourceCode = ((num == 0) ? null : reader.ReadBytes(num));
	}
}
