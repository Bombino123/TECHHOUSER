namespace NAudio.SoundFont;

public class Zone
{
	internal ushort generatorIndex;

	internal ushort modulatorIndex;

	internal ushort generatorCount;

	internal ushort modulatorCount;

	public Modulator[] Modulators { get; set; }

	public Generator[] Generators { get; set; }

	public override string ToString()
	{
		return $"Zone {generatorCount} Gens:{generatorIndex} {modulatorCount} Mods:{modulatorIndex}";
	}
}
