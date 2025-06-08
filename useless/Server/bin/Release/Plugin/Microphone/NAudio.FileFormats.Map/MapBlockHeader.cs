using System.IO;

namespace NAudio.FileFormats.Map;

internal class MapBlockHeader
{
	private int length;

	private int value2;

	private short value3;

	private short value4;

	public int Length => length;

	public static MapBlockHeader Read(BinaryReader reader)
	{
		return new MapBlockHeader
		{
			length = reader.ReadInt32(),
			value2 = reader.ReadInt32(),
			value3 = reader.ReadInt16(),
			value4 = reader.ReadInt16()
		};
	}

	public override string ToString()
	{
		return $"{length} {value2:X8} {value3:X4} {value4:X4}";
	}
}
