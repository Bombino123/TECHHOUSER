using System;
using System.Threading;

namespace AForge;

public sealed class Parallel
{
	public delegate void ForLoopBody(int index);

	private static int threadsCount = Environment.ProcessorCount;

	private static object sync = new object();

	private static volatile Parallel instance = null;

	private Thread[] threads;

	private AutoResetEvent[] jobAvailable;

	private ManualResetEvent[] threadIdle;

	private int currentIndex;

	private int stopIndex;

	private ForLoopBody loopBody;

	public static int ThreadsCount
	{
		get
		{
			return threadsCount;
		}
		set
		{
			lock (sync)
			{
				threadsCount = Math.Max(1, value);
			}
		}
	}

	private static Parallel Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new Parallel();
				instance.Initialize();
			}
			else if (instance.threads.Length != threadsCount)
			{
				instance.Terminate();
				instance.Initialize();
			}
			return instance;
		}
	}

	public static void For(int start, int stop, ForLoopBody loopBody)
	{
		lock (sync)
		{
			Parallel parallel = Instance;
			parallel.currentIndex = start - 1;
			parallel.stopIndex = stop;
			parallel.loopBody = loopBody;
			for (int i = 0; i < threadsCount; i++)
			{
				parallel.threadIdle[i].Reset();
				parallel.jobAvailable[i].Set();
			}
			for (int j = 0; j < threadsCount; j++)
			{
				parallel.threadIdle[j].WaitOne();
			}
			parallel.loopBody = null;
		}
	}

	private Parallel()
	{
	}

	private void Initialize()
	{
		jobAvailable = new AutoResetEvent[threadsCount];
		threadIdle = new ManualResetEvent[threadsCount];
		threads = new Thread[threadsCount];
		for (int i = 0; i < threadsCount; i++)
		{
			jobAvailable[i] = new AutoResetEvent(initialState: false);
			threadIdle[i] = new ManualResetEvent(initialState: true);
			threads[i] = new Thread(WorkerThread);
			threads[i].Name = "AForge.Parallel";
			threads[i].IsBackground = true;
			threads[i].Start(i);
		}
	}

	private void Terminate()
	{
		loopBody = null;
		int i = 0;
		for (int num = threads.Length; i < num; i++)
		{
			jobAvailable[i].Set();
			threads[i].Join();
			jobAvailable[i].Close();
			threadIdle[i].Close();
		}
		jobAvailable = null;
		threadIdle = null;
		threads = null;
	}

	private void WorkerThread(object index)
	{
		int num = (int)index;
		int num2 = 0;
		while (true)
		{
			jobAvailable[num].WaitOne();
			if (loopBody == null)
			{
				break;
			}
			while (true)
			{
				num2 = Interlocked.Increment(ref currentIndex);
				if (num2 >= stopIndex)
				{
					break;
				}
				loopBody(num2);
			}
			threadIdle[num].Set();
		}
	}
}
