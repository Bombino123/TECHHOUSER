using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ShareInfo2Entry : IShareInfoEntry, INDRStructure
{
	public const uint UnlimitedConnections = uint.MaxValue;

	public NDRUnicodeString NetName;

	public ShareTypeExtended ShareType;

	public NDRUnicodeString Remark;

	public Permissions Permissions;

	public uint MaxUses;

	public uint CurrentUses;

	public NDRUnicodeString Path;

	public NDRUnicodeString Password;

	public uint Level => 2u;

	public ShareInfo2Entry()
	{
	}

	public ShareInfo2Entry(string shareName, ShareTypeExtended shareType)
	{
		NetName = new NDRUnicodeString(shareName);
		ShareType = shareType;
		Remark = new NDRUnicodeString(string.Empty);
		MaxUses = uint.MaxValue;
		Path = new NDRUnicodeString(string.Empty);
		Password = null;
	}

	public ShareInfo2Entry(NDRParser parser)
	{
		Read(parser);
	}

	public void Read(NDRParser parser)
	{
		parser.BeginStructure();
		parser.ReadEmbeddedStructureFullPointer(ref NetName);
		ShareType = new ShareTypeExtended(parser);
		parser.ReadEmbeddedStructureFullPointer(ref Remark);
		Permissions = (Permissions)parser.ReadUInt32();
		MaxUses = parser.ReadUInt32();
		CurrentUses = parser.ReadUInt32();
		parser.ReadEmbeddedStructureFullPointer(ref Path);
		parser.ReadEmbeddedStructureFullPointer(ref Password);
		parser.EndStructure();
	}

	public void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		writer.WriteEmbeddedStructureFullPointer(NetName);
		ShareType.Write(writer);
		writer.WriteEmbeddedStructureFullPointer(Remark);
		writer.WriteUInt32((uint)Permissions);
		writer.WriteUInt32(MaxUses);
		writer.WriteUInt32(CurrentUses);
		writer.WriteEmbeddedStructureFullPointer(Path);
		writer.WriteEmbeddedStructureFullPointer(Password);
		writer.EndStructure();
	}
}
