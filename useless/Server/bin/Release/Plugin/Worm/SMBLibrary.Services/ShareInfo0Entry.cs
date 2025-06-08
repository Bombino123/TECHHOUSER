using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ShareInfo0Entry : IShareInfoEntry, INDRStructure
{
	public NDRUnicodeString NetName;

	public uint Level => 0u;

	public ShareInfo0Entry()
	{
	}

	public ShareInfo0Entry(string shareName)
	{
		NetName = new NDRUnicodeString(shareName);
	}

	public ShareInfo0Entry(NDRParser parser)
	{
		Read(parser);
	}

	public void Read(NDRParser parser)
	{
		parser.BeginStructure();
		parser.ReadEmbeddedStructureFullPointer(ref NetName);
		parser.EndStructure();
	}

	public void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		writer.WriteEmbeddedStructureFullPointer(NetName);
		writer.EndStructure();
	}
}
