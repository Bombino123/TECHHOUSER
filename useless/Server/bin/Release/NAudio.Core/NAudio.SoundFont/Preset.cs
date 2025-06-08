namespace NAudio.SoundFont;

public class Preset
{
	internal ushort startPresetZoneIndex;

	internal ushort endPresetZoneIndex;

	internal uint library;

	internal uint genre;

	internal uint morphology;

	public string Name { get; set; }

	public ushort PatchNumber { get; set; }

	public ushort Bank { get; set; }

	public Zone[] Zones { get; set; }

	public override string ToString()
	{
		return $"{Bank}-{PatchNumber} {Name}";
	}
}
