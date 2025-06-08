namespace NAudio.SoundFont;

public class Modulator
{
	public ModulatorType SourceModulationData { get; set; }

	public GeneratorEnum DestinationGenerator { get; set; }

	public short Amount { get; set; }

	public ModulatorType SourceModulationAmount { get; set; }

	public TransformEnum SourceTransform { get; set; }

	public override string ToString()
	{
		return $"Modulator {SourceModulationData} {DestinationGenerator} {Amount} {SourceModulationAmount} {SourceTransform}";
	}
}
