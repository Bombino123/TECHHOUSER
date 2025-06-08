using System;
using System.IO;

namespace NAudio.Midi;

public class MetaEvent : MidiEvent
{
	private MetaEventType metaEvent;

	internal int metaDataLength;

	public MetaEventType MetaEventType => metaEvent;

	protected MetaEvent()
	{
	}

	public MetaEvent(MetaEventType metaEventType, int metaDataLength, long absoluteTime)
		: base(absoluteTime, 1, MidiCommandCode.MetaEvent)
	{
		metaEvent = metaEventType;
		this.metaDataLength = metaDataLength;
	}

	public override MidiEvent Clone()
	{
		return new MetaEvent(metaEvent, metaDataLength, base.AbsoluteTime);
	}

	public static MetaEvent ReadMetaEvent(BinaryReader br)
	{
		MetaEventType metaEventType = (MetaEventType)br.ReadByte();
		int num = MidiEvent.ReadVarInt(br);
		MetaEvent metaEvent = new MetaEvent();
		if (metaEventType <= MetaEventType.SetTempo)
		{
			if (metaEventType <= MetaEventType.DeviceName)
			{
				if (metaEventType != 0)
				{
					if (metaEventType - 1 > MetaEventType.ProgramName)
					{
						goto IL_00a6;
					}
					metaEvent = new TextEvent(br, num);
				}
				else
				{
					metaEvent = new TrackSequenceNumberEvent(br, num);
				}
			}
			else if (metaEventType != MetaEventType.EndTrack)
			{
				if (metaEventType != MetaEventType.SetTempo)
				{
					goto IL_00a6;
				}
				metaEvent = new TempoEvent(br, num);
			}
			else if (num != 0)
			{
				throw new FormatException("End track length");
			}
		}
		else if (metaEventType <= MetaEventType.TimeSignature)
		{
			if (metaEventType != MetaEventType.SmpteOffset)
			{
				if (metaEventType != MetaEventType.TimeSignature)
				{
					goto IL_00a6;
				}
				metaEvent = new TimeSignatureEvent(br, num);
			}
			else
			{
				metaEvent = new SmpteOffsetEvent(br, num);
			}
		}
		else if (metaEventType != MetaEventType.KeySignature)
		{
			if (metaEventType != MetaEventType.SequencerSpecific)
			{
				goto IL_00a6;
			}
			metaEvent = new SequencerSpecificEvent(br, num);
		}
		else
		{
			metaEvent = new KeySignatureEvent(br, num);
		}
		metaEvent.metaEvent = metaEventType;
		metaEvent.metaDataLength = num;
		return metaEvent;
		IL_00a6:
		byte[] array = br.ReadBytes(num);
		if (array.Length != num)
		{
			throw new FormatException("Failed to read metaevent's data fully");
		}
		return new RawMetaEvent(metaEventType, 0L, array);
	}

	public override string ToString()
	{
		return $"{base.AbsoluteTime} {metaEvent}";
	}

	public override void Export(ref long absoluteTime, BinaryWriter writer)
	{
		base.Export(ref absoluteTime, writer);
		writer.Write((byte)metaEvent);
		MidiEvent.WriteVarInt(writer, metaDataLength);
	}
}
