using System;

namespace NAudio.Wave;

public class BextChunkInfo
{
	public string Description { get; set; }

	public string Originator { get; set; }

	public string OriginatorReference { get; set; }

	public DateTime OriginationDateTime { get; set; }

	public string OriginationDate => OriginationDateTime.ToString("yyyy-MM-dd");

	public string OriginationTime => OriginationDateTime.ToString("HH:mm:ss");

	public long TimeReference { get; set; }

	public ushort Version => 1;

	public string UniqueMaterialIdentifier { get; set; }

	public byte[] Reserved { get; }

	public string CodingHistory { get; set; }

	public BextChunkInfo()
	{
		Reserved = new byte[190];
	}
}
