using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace AForge.Video;

public class AsyncVideoSource : IVideoSource
{
	private readonly IVideoSource nestedVideoSource;

	private Bitmap lastVideoFrame;

	private Thread imageProcessingThread;

	private AutoResetEvent isNewFrameAvailable;

	private AutoResetEvent isProcessingThreadAvailable;

	private bool skipFramesIfBusy;

	private int framesProcessed;

	public IVideoSource NestedVideoSource => nestedVideoSource;

	public bool SkipFramesIfBusy
	{
		get
		{
			return skipFramesIfBusy;
		}
		set
		{
			skipFramesIfBusy = value;
		}
	}

	public string Source => nestedVideoSource.Source;

	public int FramesReceived => nestedVideoSource.FramesReceived;

	public long BytesReceived => nestedVideoSource.BytesReceived;

	public int FramesProcessed
	{
		get
		{
			int result = framesProcessed;
			framesProcessed = 0;
			return result;
		}
	}

	public bool IsRunning
	{
		get
		{
			bool isRunning = nestedVideoSource.IsRunning;
			if (!isRunning)
			{
				Free();
			}
			return isRunning;
		}
	}

	public event NewFrameEventHandler NewFrame;

	public event VideoSourceErrorEventHandler VideoSourceError
	{
		add
		{
			nestedVideoSource.VideoSourceError += value;
		}
		remove
		{
			nestedVideoSource.VideoSourceError -= value;
		}
	}

	public event PlayingFinishedEventHandler PlayingFinished
	{
		add
		{
			nestedVideoSource.PlayingFinished += value;
		}
		remove
		{
			nestedVideoSource.PlayingFinished -= value;
		}
	}

	public AsyncVideoSource(IVideoSource nestedVideoSource)
	{
		this.nestedVideoSource = nestedVideoSource;
	}

	public AsyncVideoSource(IVideoSource nestedVideoSource, bool skipFramesIfBusy)
	{
		this.nestedVideoSource = nestedVideoSource;
		this.skipFramesIfBusy = skipFramesIfBusy;
	}

	public void Start()
	{
		if (!IsRunning)
		{
			framesProcessed = 0;
			isNewFrameAvailable = new AutoResetEvent(initialState: false);
			isProcessingThreadAvailable = new AutoResetEvent(initialState: true);
			imageProcessingThread = new Thread(imageProcessingThread_Worker);
			imageProcessingThread.Start();
			nestedVideoSource.NewFrame += nestedVideoSource_NewFrame;
			nestedVideoSource.Start();
		}
	}

	public void SignalToStop()
	{
		nestedVideoSource.SignalToStop();
	}

	public void WaitForStop()
	{
		nestedVideoSource.WaitForStop();
		Free();
	}

	public void Stop()
	{
		nestedVideoSource.Stop();
		Free();
	}

	private void Free()
	{
		if (imageProcessingThread != null)
		{
			nestedVideoSource.NewFrame -= nestedVideoSource_NewFrame;
			isProcessingThreadAvailable.WaitOne();
			lastVideoFrame = null;
			isNewFrameAvailable.Set();
			imageProcessingThread.Join();
			imageProcessingThread = null;
			isNewFrameAvailable.Close();
			isNewFrameAvailable = null;
			isProcessingThreadAvailable.Close();
			isProcessingThreadAvailable = null;
		}
	}

	private void nestedVideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
	{
		if (this.NewFrame == null)
		{
			return;
		}
		if (skipFramesIfBusy)
		{
			if (!isProcessingThreadAvailable.WaitOne(0, exitContext: false))
			{
				return;
			}
		}
		else
		{
			isProcessingThreadAvailable.WaitOne();
		}
		lastVideoFrame = CloneImage(eventArgs.Frame);
		isNewFrameAvailable.Set();
	}

	private void imageProcessingThread_Worker()
	{
		while (true)
		{
			isNewFrameAvailable.WaitOne();
			if (lastVideoFrame == null)
			{
				break;
			}
			if (this.NewFrame != null)
			{
				this.NewFrame(this, new NewFrameEventArgs(lastVideoFrame));
			}
			((Image)lastVideoFrame).Dispose();
			lastVideoFrame = null;
			framesProcessed++;
			isProcessingThreadAvailable.Set();
		}
	}

	private static Bitmap CloneImage(Bitmap source)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Invalid comparison between Unknown and I4
		BitmapData val = source.LockBits(new Rectangle(0, 0, ((Image)source).Width, ((Image)source).Height), (ImageLockMode)1, ((Image)source).PixelFormat);
		Bitmap val2 = CloneImage(val);
		source.UnlockBits(val);
		if ((int)((Image)source).PixelFormat == 196865 || (int)((Image)source).PixelFormat == 197634 || (int)((Image)source).PixelFormat == 198659 || (int)((Image)source).PixelFormat == 65536)
		{
			ColorPalette palette = ((Image)source).Palette;
			ColorPalette palette2 = ((Image)val2).Palette;
			int num = palette.Entries.Length;
			for (int i = 0; i < num; i++)
			{
				ref Color reference = ref palette2.Entries[i];
				reference = palette.Entries[i];
			}
			((Image)val2).Palette = palette2;
		}
		return val2;
	}

	private static Bitmap CloneImage(BitmapData sourceData)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		int width = sourceData.Width;
		int height = sourceData.Height;
		Bitmap val = new Bitmap(width, height, sourceData.PixelFormat);
		BitmapData val2 = val.LockBits(new Rectangle(0, 0, width, height), (ImageLockMode)3, ((Image)val).PixelFormat);
		SystemTools.CopyUnmanagedMemory(val2.Scan0, sourceData.Scan0, height * sourceData.Stride);
		val.UnlockBits(val2);
		return val;
	}
}
