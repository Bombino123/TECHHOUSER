using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary.RPC;

[ComVisible(true)]
public class NDRConformantArray<T> : List<T>, INDRStructure where T : INDRStructure, new()
{
	public void Read(NDRParser parser)
	{
		parser.BeginStructure();
		uint num = parser.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			T item = new T();
			item.Read(parser);
			Add(item);
		}
		parser.EndStructure();
	}

	public void Write(NDRWriter writer)
	{
		writer.BeginStructure();
		uint count = (uint)base.Count;
		writer.WriteUInt32(count);
		for (int i = 0; i < base.Count; i++)
		{
			base[i].Write(writer);
		}
		writer.EndStructure();
	}
}
