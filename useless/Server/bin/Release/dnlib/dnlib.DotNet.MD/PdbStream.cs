using dnlib.IO;

namespace dnlib.DotNet.MD;

public sealed class PdbStream : HeapStream
{
	public byte[] Id { get; private set; }

	public MDToken EntryPoint { get; private set; }

	public ulong ReferencedTypeSystemTables { get; private set; }

	public uint[] TypeSystemTableRows { get; private set; }

	public PdbStream(DataReaderFactory mdReaderFactory, uint metadataBaseOffset, StreamHeader streamHeader)
		: base(mdReaderFactory, metadataBaseOffset, streamHeader)
	{
		DataReader dataReader = CreateReader();
		Id = dataReader.ReadBytes(20);
		EntryPoint = new MDToken(dataReader.ReadUInt32());
		ulong num2 = (ReferencedTypeSystemTables = dataReader.ReadUInt64());
		uint[] array = new uint[64];
		int num3 = 0;
		while (num3 < array.Length)
		{
			if (((uint)(int)num2 & (true ? 1u : 0u)) != 0)
			{
				array[num3] = dataReader.ReadUInt32();
			}
			num3++;
			num2 >>= 1;
		}
		TypeSystemTableRows = array;
	}
}
