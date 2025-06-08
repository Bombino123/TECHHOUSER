using System.Runtime.InteropServices;
using SMBLibrary.RPC;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class ShareInfo0Container : IShareInfoContainer, INDRStructure
{
	public NDRConformantArray<ShareInfo0Entry> Entries;

	public uint Level => 0u;

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

	public ShareInfo0Container()
	{
	}

	public ShareInfo0Container(NDRParser parser)
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

	public void Add(ShareInfo0Entry entry)
	{
		if (Entries == null)
		{
			Entries = new NDRConformantArray<ShareInfo0Entry>();
		}
		Entries.Add(entry);
	}
}
