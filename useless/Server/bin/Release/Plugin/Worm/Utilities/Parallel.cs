using System;
using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class Parallel
{
	public static void For(int fromInclusive, int toExclusive, ForDelegate forDelegate)
	{
		int chunkSize = 4;
		For(fromInclusive, toExclusive, chunkSize, forDelegate);
	}

	public static void For(int fromInclusive, int toExclusive, int chunkSize, ForDelegate forDelegate)
	{
		int processorCount = Environment.ProcessorCount;
		For(fromInclusive, toExclusive, chunkSize, processorCount, forDelegate);
	}

	public static void For(int fromInclusive, int toExclusive, int chunkSize, int threadCount, ForDelegate forDelegate)
	{
		int index = fromInclusive - chunkSize;
		object locker = new object();
		DelegateProcess delegateProcess = delegate
		{
			while (true)
			{
				int num = 0;
				lock (locker)
				{
					index += chunkSize;
					num = index;
				}
				for (int k = num; k < num + chunkSize; k++)
				{
					if (k >= toExclusive)
					{
						return;
					}
					forDelegate(k);
				}
			}
		};
		IAsyncResult[] array = new IAsyncResult[threadCount];
		for (int i = 0; i < threadCount; i++)
		{
			array[i] = delegateProcess.BeginInvoke(null, null);
		}
		for (int j = 0; j < threadCount; j++)
		{
			delegateProcess.EndInvoke(array[j]);
		}
	}
}
