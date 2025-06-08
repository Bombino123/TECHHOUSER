using System.IO;
using System.Text;

namespace NAudio.SoundFont;

public class PresetsChunk
{
	private PresetBuilder presetHeaders = new PresetBuilder();

	private ZoneBuilder presetZones = new ZoneBuilder();

	private ModulatorBuilder presetZoneModulators = new ModulatorBuilder();

	private GeneratorBuilder presetZoneGenerators = new GeneratorBuilder();

	private InstrumentBuilder instruments = new InstrumentBuilder();

	private ZoneBuilder instrumentZones = new ZoneBuilder();

	private ModulatorBuilder instrumentZoneModulators = new ModulatorBuilder();

	private GeneratorBuilder instrumentZoneGenerators = new GeneratorBuilder();

	private SampleHeaderBuilder sampleHeaders = new SampleHeaderBuilder();

	public Preset[] Presets => presetHeaders.Presets;

	public Instrument[] Instruments => instruments.Instruments;

	public SampleHeader[] SampleHeaders => sampleHeaders.SampleHeaders;

	internal PresetsChunk(RiffChunk chunk)
	{
		string text = chunk.ReadChunkID();
		if (text != "pdta")
		{
			throw new InvalidDataException($"Not a presets data chunk ({text})");
		}
		RiffChunk nextSubChunk;
		while ((nextSubChunk = chunk.GetNextSubChunk()) != null)
		{
			switch (nextSubChunk.ChunkID)
			{
			case "phdr":
			case "PHDR":
				nextSubChunk.GetDataAsStructureArray(presetHeaders);
				break;
			case "pbag":
			case "PBAG":
				nextSubChunk.GetDataAsStructureArray(presetZones);
				break;
			case "pmod":
			case "PMOD":
				nextSubChunk.GetDataAsStructureArray(presetZoneModulators);
				break;
			case "pgen":
			case "PGEN":
				nextSubChunk.GetDataAsStructureArray(presetZoneGenerators);
				break;
			case "inst":
			case "INST":
				nextSubChunk.GetDataAsStructureArray(instruments);
				break;
			case "ibag":
			case "IBAG":
				nextSubChunk.GetDataAsStructureArray(instrumentZones);
				break;
			case "imod":
			case "IMOD":
				nextSubChunk.GetDataAsStructureArray(instrumentZoneModulators);
				break;
			case "igen":
			case "IGEN":
				nextSubChunk.GetDataAsStructureArray(instrumentZoneGenerators);
				break;
			case "shdr":
			case "SHDR":
				nextSubChunk.GetDataAsStructureArray(sampleHeaders);
				break;
			default:
				throw new InvalidDataException($"Unknown chunk type {nextSubChunk.ChunkID}");
			}
		}
		instrumentZoneGenerators.Load(sampleHeaders.SampleHeaders);
		instrumentZones.Load(instrumentZoneModulators.Modulators, instrumentZoneGenerators.Generators);
		instruments.LoadZones(instrumentZones.Zones);
		presetZoneGenerators.Load(instruments.Instruments);
		presetZones.Load(presetZoneModulators.Modulators, presetZoneGenerators.Generators);
		presetHeaders.LoadZones(presetZones.Zones);
		sampleHeaders.RemoveEOS();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Preset Headers:\r\n");
		Preset[] presets = presetHeaders.Presets;
		foreach (Preset arg in presets)
		{
			stringBuilder.AppendFormat("{0}\r\n", arg);
		}
		stringBuilder.Append("Instruments:\r\n");
		Instrument[] array = instruments.Instruments;
		foreach (Instrument arg2 in array)
		{
			stringBuilder.AppendFormat("{0}\r\n", arg2);
		}
		return stringBuilder.ToString();
	}
}
