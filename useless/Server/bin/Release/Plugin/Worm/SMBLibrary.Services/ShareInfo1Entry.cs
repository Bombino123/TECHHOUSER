using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ShareInfo1Entry : IShareInfoEntry, INDRStructure
{
	public NDRUnicodeString NetName;

	public ShareTypeExtended ShareType;

	public NDRUnicodeString Remark;

	public uint Level => 1u;

	public ShareInfo1Entry()
	{
	}

	public ShareInfo1Entry(string shareName, ShareTypeExtended shareType)
	{
		NetName = new NDRUnicodeString(shareName);
		ShareType = shareType;
		Remark = new NDRUnicodeString(string.Empty);
	}

	public ShareInfo1Entry(NDRParser parser)
	{
		Read(parser);
	}

	public void Read(NDRParser parser)
	{
		parser.BeginStructure();
		parser.ReadEmbeddedStructureFullPointer(ref NetName);
		ShareType = new ShareTypeExtended(parser);
		parser.ReadEmbeddedStructureFullPointer(ref Remark);
		parser.EndStructure();
	}

	public void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		writer.WriteEmbeddedStructureFullPointer(NetName);
		ShareType.Write(writer);
		writer.WriteEmbeddedStructureFullPointer(Remark);
		writer.EndStructure();
	}
}
