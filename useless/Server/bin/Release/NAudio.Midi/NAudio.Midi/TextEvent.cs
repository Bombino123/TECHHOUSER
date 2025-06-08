using System.IO;
using System.Text;
using NAudio.Utils;

namespace NAudio.Midi;

public class TextEvent : MetaEvent
{
	private byte[] data;

	public string Text
	{
		get
		{
			return ((Encoding)(object)ByteEncoding.Instance).GetString(data);
		}
		set
		{
			Encoding instance = (Encoding)(object)ByteEncoding.Instance;
			data = instance.GetBytes(value);
			metaDataLength = data.Length;
		}
	}

	public byte[] Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
			metaDataLength = data.Length;
		}
	}

	public TextEvent(BinaryReader br, int length)
	{
		data = br.ReadBytes(length);
	}

	public TextEvent(string text, MetaEventType metaEventType, long absoluteTime)
		: base(metaEventType, text.Length, absoluteTime)
	{
		Text = text;
	}

	public override MidiEvent Clone()
	{
		return (TextEvent)MemberwiseClone();
	}

	public override string ToString()
	{
		return $"{base.ToString()} {Text}";
	}

	public override void Export(ref long absoluteTime, BinaryWriter writer)
	{
		base.Export(ref absoluteTime, writer);
		writer.Write(data);
	}
}
