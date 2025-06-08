namespace NAudio.SoundFont;

public class Instrument
{
	internal ushort startInstrumentZoneIndex;

	internal ushort endInstrumentZoneIndex;

	public string Name { get; set; }

	public Zone[] Zones { get; set; }

	public override string ToString()
	{
		return Name;
	}
}
