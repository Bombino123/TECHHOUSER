using System;
using System.IO;
using System.Security;
using dnlib.IO;

namespace dnlib.DotNet.Pdb;

internal static class DataReaderFactoryUtils
{
	public static DataReaderFactory TryCreateDataReaderFactory(string filename)
	{
		try
		{
			if (!File.Exists(filename))
			{
				return null;
			}
			return ByteArrayDataReaderFactory.Create(File.ReadAllBytes(filename), filename);
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		catch (SecurityException)
		{
		}
		return null;
	}
}
