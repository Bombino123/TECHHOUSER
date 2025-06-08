namespace NAudio.SoundFont;

public class Generator
{
	public GeneratorEnum GeneratorType { get; set; }

	public ushort UInt16Amount { get; set; }

	public short Int16Amount
	{
		get
		{
			return (short)UInt16Amount;
		}
		set
		{
			UInt16Amount = (ushort)value;
		}
	}

	public byte LowByteAmount
	{
		get
		{
			return (byte)(UInt16Amount & 0xFFu);
		}
		set
		{
			UInt16Amount &= 65280;
			UInt16Amount += value;
		}
	}

	public byte HighByteAmount
	{
		get
		{
			return (byte)((UInt16Amount & 0xFF00) >> 8);
		}
		set
		{
			UInt16Amount &= 255;
			UInt16Amount += (ushort)(value << 8);
		}
	}

	public Instrument Instrument { get; set; }

	public SampleHeader SampleHeader { get; set; }

	public override string ToString()
	{
		if (GeneratorType == GeneratorEnum.Instrument)
		{
			return "Generator Instrument " + Instrument.Name;
		}
		if (GeneratorType == GeneratorEnum.SampleID)
		{
			return $"Generator SampleID {SampleHeader}";
		}
		return $"Generator {GeneratorType} {UInt16Amount}";
	}
}
