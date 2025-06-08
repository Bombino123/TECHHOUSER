using System.IO;
using System.Text;

namespace NAudio.Midi;

public class RawMetaEvent : MetaEvent
{
	public byte[] Data { get; set; }

	public RawMetaEvent(MetaEventType metaEventType, long absoluteTime, byte[] data)
		: base(metaEventType, (data != null) ? data.Length : 0, absoluteTime)
	{
		Data = data;
	}

	public override MidiEvent Clone()
	{
		return new RawMetaEvent(base.MetaEventType, base.AbsoluteTime, (byte[])Data?.Clone());
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder().Append(base.ToString());
		byte[] data = Data;
		foreach (byte b in data)
		{
			stringBuilder.AppendFormat(" {0:X2}", b);
		}
		return stringBuilder.ToString();
	}

	public override void Export(ref long absoluteTime, BinaryWriter writer)
	{
		base.Export(ref absoluteTime, writer);
		if (Data != null)
		{
			writer.Write(Data, 0, Data.Length);
		}
	}
}
