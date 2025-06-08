using System.IO;

namespace NAudio.SoundFont;

public class InfoChunk
{
	public SFVersion SoundFontVersion { get; }

	public string WaveTableSoundEngine { get; set; }

	public string BankName { get; set; }

	public string DataROM { get; set; }

	public string CreationDate { get; set; }

	public string Author { get; set; }

	public string TargetProduct { get; set; }

	public string Copyright { get; set; }

	public string Comments { get; set; }

	public string Tools { get; set; }

	public SFVersion ROMVersion { get; set; }

	internal InfoChunk(RiffChunk chunk)
	{
		bool flag = false;
		bool flag2 = false;
		if (chunk.ReadChunkID() != "INFO")
		{
			throw new InvalidDataException("Not an INFO chunk");
		}
		RiffChunk nextSubChunk;
		while ((nextSubChunk = chunk.GetNextSubChunk()) != null)
		{
			switch (nextSubChunk.ChunkID)
			{
			case "ifil":
				flag = true;
				SoundFontVersion = nextSubChunk.GetDataAsStructure(new SFVersionBuilder());
				break;
			case "isng":
				WaveTableSoundEngine = nextSubChunk.GetDataAsString();
				break;
			case "INAM":
				flag2 = true;
				BankName = nextSubChunk.GetDataAsString();
				break;
			case "irom":
				DataROM = nextSubChunk.GetDataAsString();
				break;
			case "iver":
				ROMVersion = nextSubChunk.GetDataAsStructure(new SFVersionBuilder());
				break;
			case "ICRD":
				CreationDate = nextSubChunk.GetDataAsString();
				break;
			case "IENG":
				Author = nextSubChunk.GetDataAsString();
				break;
			case "IPRD":
				TargetProduct = nextSubChunk.GetDataAsString();
				break;
			case "ICOP":
				Copyright = nextSubChunk.GetDataAsString();
				break;
			case "ICMT":
				Comments = nextSubChunk.GetDataAsString();
				break;
			case "ISFT":
				Tools = nextSubChunk.GetDataAsString();
				break;
			default:
				throw new InvalidDataException("Unknown chunk type " + nextSubChunk.ChunkID);
			}
		}
		if (!flag)
		{
			throw new InvalidDataException("Missing SoundFont version information");
		}
		if (!flag2)
		{
			throw new InvalidDataException("Missing SoundFont name information");
		}
	}

	public override string ToString()
	{
		return string.Format("Bank Name: {0}\r\nAuthor: {1}\r\nCopyright: {2}\r\nCreation Date: {3}\r\nTools: {4}\r\nComments: {5}\r\nSound Engine: {6}\r\nSoundFont Version: {7}\r\nTarget Product: {8}\r\nData ROM: {9}\r\nROM Version: {10}", BankName, Author, Copyright, CreationDate, Tools, "TODO-fix comments", WaveTableSoundEngine, SoundFontVersion, TargetProduct, DataROM, ROMVersion);
	}
}
