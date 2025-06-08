using System;
using System.IO;

namespace NAudio.Midi;

public class KeySignatureEvent : MetaEvent
{
	private readonly byte sharpsFlats;

	private readonly byte majorMinor;

	public int SharpsFlats => (sbyte)sharpsFlats;

	public int MajorMinor => majorMinor;

	public KeySignatureEvent(BinaryReader br, int length)
	{
		if (length != 2)
		{
			throw new FormatException("Invalid key signature length");
		}
		sharpsFlats = br.ReadByte();
		majorMinor = br.ReadByte();
	}

	public KeySignatureEvent(int sharpsFlats, int majorMinor, long absoluteTime)
		: base(MetaEventType.KeySignature, 2, absoluteTime)
	{
		this.sharpsFlats = (byte)sharpsFlats;
		this.majorMinor = (byte)majorMinor;
	}

	public override MidiEvent Clone()
	{
		return (KeySignatureEvent)MemberwiseClone();
	}

	public override string ToString()
	{
		return $"{base.ToString()} {SharpsFlats} {majorMinor}";
	}

	public override void Export(ref long absoluteTime, BinaryWriter writer)
	{
		base.Export(ref absoluteTime, writer);
		writer.Write(sharpsFlats);
		writer.Write(majorMinor);
	}
}
