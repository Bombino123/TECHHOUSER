using System.IO;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

internal static class SymbolReaderFactory
{
	public static SymbolReader Create(PdbReaderContext pdbContext, DataReaderFactory pdbStream)
	{
		if (pdbStream == null)
		{
			return null;
		}
		try
		{
			if (pdbContext.CodeViewDebugDirectory == null)
			{
				return null;
			}
			if (!pdbContext.TryGetCodeViewData(out var guid, out var age))
			{
				return null;
			}
			PdbReader pdbReader = new PdbReader(guid, age);
			pdbReader.Read(pdbStream.CreateReader());
			if (pdbReader.MatchesModule)
			{
				return pdbReader;
			}
			return null;
		}
		catch (PdbException)
		{
		}
		catch (IOException)
		{
		}
		finally
		{
			pdbStream?.Dispose();
		}
		return null;
	}
}
