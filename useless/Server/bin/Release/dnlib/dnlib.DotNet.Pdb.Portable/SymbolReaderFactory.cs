using System.IO;
using System.IO.Compression;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Pdb.Portable;

internal static class SymbolReaderFactory
{
	public static SymbolReader TryCreate(PdbReaderContext pdbContext, DataReaderFactory pdbStream, bool isEmbeddedPortablePdb)
	{
		bool flag = true;
		try
		{
			if (!pdbContext.HasDebugInfo)
			{
				return null;
			}
			if (pdbStream == null)
			{
				return null;
			}
			if (pdbStream.Length < 4)
			{
				return null;
			}
			if (pdbStream.CreateReader().ReadUInt32() != 1112167234)
			{
				return null;
			}
			ImageDebugDirectory codeViewDebugDirectory = pdbContext.CodeViewDebugDirectory;
			if (codeViewDebugDirectory == null)
			{
				return null;
			}
			if (!pdbContext.TryGetCodeViewData(out var guid, out var age))
			{
				return null;
			}
			PortablePdbReader portablePdbReader = new PortablePdbReader(pdbStream, (!isEmbeddedPortablePdb) ? PdbFileKind.PortablePDB : PdbFileKind.EmbeddedPortablePDB);
			if (!portablePdbReader.MatchesModule(guid, codeViewDebugDirectory.TimeDateStamp, age))
			{
				return null;
			}
			flag = false;
			return portablePdbReader;
		}
		catch (IOException)
		{
		}
		finally
		{
			if (flag)
			{
				pdbStream?.Dispose();
			}
		}
		return null;
	}

	public static SymbolReader TryCreateEmbeddedPortablePdbReader(PdbReaderContext pdbContext, Metadata metadata)
	{
		if (metadata == null)
		{
			return null;
		}
		try
		{
			if (!pdbContext.HasDebugInfo)
			{
				return null;
			}
			ImageDebugDirectory imageDebugDirectory = pdbContext.TryGetDebugDirectoryEntry(ImageDebugType.EmbeddedPortablePdb);
			if (imageDebugDirectory == null)
			{
				return null;
			}
			DataReader dataReader = pdbContext.CreateReader(imageDebugDirectory.AddressOfRawData, imageDebugDirectory.SizeOfData);
			if (dataReader.Length < 8)
			{
				return null;
			}
			if (dataReader.ReadUInt32() != 1111773261)
			{
				return null;
			}
			uint num = dataReader.ReadUInt32();
			if ((num & 0x80000000u) != 0)
			{
				return null;
			}
			byte[] array = new byte[num];
			using DeflateStream deflateStream = new DeflateStream(dataReader.AsStream(), CompressionMode.Decompress);
			int i;
			int num2;
			for (i = 0; i < array.Length; i += num2)
			{
				num2 = deflateStream.Read(array, i, array.Length - i);
				if (num2 == 0)
				{
					break;
				}
			}
			if (i != array.Length)
			{
				return null;
			}
			ByteArrayDataReaderFactory pdbStream = ByteArrayDataReaderFactory.Create(array, null);
			return TryCreate(pdbContext, pdbStream, isEmbeddedPortablePdb: true);
		}
		catch (IOException)
		{
		}
		return null;
	}
}
