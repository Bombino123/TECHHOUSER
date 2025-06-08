using System;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Managed;

internal sealed class MsfStream
{
	public DataReader Content;

	public MsfStream(DataReader[] pages, uint length)
	{
		byte[] array = new byte[length];
		int num = 0;
		for (int i = 0; i < pages.Length; i++)
		{
			DataReader dataReader = pages[i];
			dataReader.Position = 0u;
			int num2 = Math.Min((int)dataReader.Length, (int)(length - num));
			dataReader.ReadBytes(array, num, num2);
			num += num2;
		}
		Content = ByteArrayDataReaderFactory.CreateReader(array);
	}
}
