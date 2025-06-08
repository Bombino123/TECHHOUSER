using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ShareInfo1Container : IShareInfoContainer, INDRStructure
{
	public NDRConformantArray<ShareInfo1Entry> Entries;

	public uint Level => 1u;

	public int Count
	{
		get
		{
			if (Entries != null)
			{
				return Entries.Count;
			}
			return 0;
		}
	}

	public ShareInfo1Container()
	{
	}

	public ShareInfo1Container(NDRParser parser)
	{
		Read(parser);
	}

	public void Read(NDRParser parser)
	{
		parser.BeginStructure();
		parser.ReadUInt32();
		parser.ReadEmbeddedStructureFullPointer(ref Entries);
		parser.EndStructure();
	}

	public void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		writer.WriteUInt32((uint)Count);
		writer.WriteEmbeddedStructureFullPointer(Entries);
		writer.EndStructure();
	}

	public void Add(ShareInfo1Entry entry)
	{
		if (Entries == null)
		{
			Entries = new NDRConformantArray<ShareInfo1Entry>();
		}
		Entries.Add(entry);
	}
}
