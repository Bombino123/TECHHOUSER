using System;
using System.IO;
using System.Text;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb.Dss;
using dnlib.DotNet.Pdb.Managed;
using dnlib.DotNet.Pdb.Portable;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.IO;

namespace dnlib.DotNet.Pdb;

internal static class SymbolReaderFactory
{
	private static readonly char[] windowsPathSepChars = new char[2] { '\\', '/' };

	public static SymbolReader CreateFromAssemblyFile(PdbReaderOptions options, Metadata metadata, string assemblyFileName)
	{
		PdbReaderContext pdbReaderContext = new PdbReaderContext(metadata.PEImage, options);
		if (!pdbReaderContext.HasDebugInfo)
		{
			return null;
		}
		if (!pdbReaderContext.TryGetCodeViewData(out var _, out var _, out var pdbFilename))
		{
			return null;
		}
		int num = pdbFilename.LastIndexOfAny(windowsPathSepChars);
		string text = ((num < 0) ? pdbFilename : pdbFilename.Substring(num + 1));
		string text2;
		try
		{
			text2 = ((assemblyFileName == string.Empty) ? text : Path.Combine(Path.GetDirectoryName(assemblyFileName), text));
			if (!File.Exists(text2))
			{
				string text3 = Path.GetExtension(text);
				if (string.IsNullOrEmpty(text3))
				{
					text3 = "pdb";
				}
				text2 = Path.ChangeExtension(assemblyFileName, text3);
			}
		}
		catch (ArgumentException)
		{
			return null;
		}
		return Create(options, metadata, text2);
	}

	public static SymbolReader Create(PdbReaderOptions options, Metadata metadata, string pdbFileName)
	{
		PdbReaderContext pdbContext = new PdbReaderContext(metadata.PEImage, options);
		if (!pdbContext.HasDebugInfo)
		{
			return null;
		}
		return CreateCore(pdbContext, metadata, DataReaderFactoryUtils.TryCreateDataReaderFactory(pdbFileName));
	}

	public static SymbolReader Create(PdbReaderOptions options, Metadata metadata, byte[] pdbData)
	{
		PdbReaderContext pdbContext = new PdbReaderContext(metadata.PEImage, options);
		if (!pdbContext.HasDebugInfo)
		{
			return null;
		}
		return CreateCore(pdbContext, metadata, ByteArrayDataReaderFactory.Create(pdbData, null));
	}

	public static SymbolReader Create(PdbReaderOptions options, Metadata metadata, DataReaderFactory pdbStream)
	{
		return CreateCore(new PdbReaderContext(metadata.PEImage, options), metadata, pdbStream);
	}

	private static SymbolReader CreateCore(PdbReaderContext pdbContext, Metadata metadata, DataReaderFactory pdbStream)
	{
		SymbolReader symbolReader = null;
		bool flag = true;
		try
		{
			if (!pdbContext.HasDebugInfo)
			{
				return null;
			}
			bool flag2 = true;
			symbolReader = ((!((pdbContext.Options & PdbReaderOptions.MicrosoftComReader) != 0 && flag2) || pdbStream == null || !IsWindowsPdb(pdbStream.CreateReader())) ? CreateManaged(pdbContext, metadata, pdbStream) : SymbolReaderWriterFactory.Create(pdbContext, metadata, pdbStream));
			if (symbolReader != null)
			{
				flag = false;
				return symbolReader;
			}
		}
		catch (IOException)
		{
		}
		finally
		{
			if (flag)
			{
				pdbStream?.Dispose();
				symbolReader?.Dispose();
			}
		}
		return null;
	}

	private static bool IsWindowsPdb(DataReader reader)
	{
		if (!reader.CanRead("Microsoft C/C++ MSF 7.00\r\n\u001aDS\0".Length))
		{
			return false;
		}
		return reader.ReadString("Microsoft C/C++ MSF 7.00\r\n\u001aDS\0".Length, Encoding.ASCII) == "Microsoft C/C++ MSF 7.00\r\n\u001aDS\0";
	}

	public static SymbolReader TryCreateEmbeddedPdbReader(PdbReaderOptions options, Metadata metadata)
	{
		PdbReaderContext pdbContext = new PdbReaderContext(metadata.PEImage, options);
		if (!pdbContext.HasDebugInfo)
		{
			return null;
		}
		return TryCreateEmbeddedPortablePdbReader(pdbContext, metadata);
	}

	private static SymbolReader CreateManaged(PdbReaderContext pdbContext, Metadata metadata, DataReaderFactory pdbStream)
	{
		try
		{
			SymbolReader symbolReader = TryCreateEmbeddedPortablePdbReader(pdbContext, metadata);
			if (symbolReader != null)
			{
				pdbStream?.Dispose();
				return symbolReader;
			}
			return CreateManagedCore(pdbContext, pdbStream);
		}
		catch
		{
			pdbStream?.Dispose();
			throw;
		}
	}

	private static SymbolReader CreateManagedCore(PdbReaderContext pdbContext, DataReaderFactory pdbStream)
	{
		if (pdbStream == null)
		{
			return null;
		}
		try
		{
			DataReader dataReader = pdbStream.CreateReader();
			if (dataReader.Length >= 4)
			{
				if (dataReader.ReadUInt32() == 1112167234)
				{
					return dnlib.DotNet.Pdb.Portable.SymbolReaderFactory.TryCreate(pdbContext, pdbStream, isEmbeddedPortablePdb: false);
				}
				return dnlib.DotNet.Pdb.Managed.SymbolReaderFactory.Create(pdbContext, pdbStream);
			}
		}
		catch (IOException)
		{
		}
		pdbStream?.Dispose();
		return null;
	}

	private static SymbolReader TryCreateEmbeddedPortablePdbReader(PdbReaderContext pdbContext, Metadata metadata)
	{
		return dnlib.DotNet.Pdb.Portable.SymbolReaderFactory.TryCreateEmbeddedPortablePdbReader(pdbContext, metadata);
	}
}
