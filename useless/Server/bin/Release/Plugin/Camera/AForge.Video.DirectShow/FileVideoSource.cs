using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using AForge.Video.DirectShow.Internals;

namespace AForge.Video.DirectShow;

public class FileVideoSource : IVideoSource
{
	private class Grabber : ISampleGrabberCB
	{
		private FileVideoSource parent;

		private int width;

		private int height;

		public int Width
		{
			get
			{
				return width;
			}
			set
			{
				width = value;
			}
		}

		public int Height
		{
			get
			{
				return height;
			}
			set
			{
				height = value;
			}
		}

		public Grabber(FileVideoSource parent)
		{
			this.parent = parent;
		}

		public int SampleCB(double sampleTime, IntPtr sample)
		{
			return 0;
		}

		public unsafe int BufferCB(double sampleTime, IntPtr buffer, int bufferLen)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			if (parent.NewFrame != null)
			{
				Bitmap val = new Bitmap(width, height, (PixelFormat)137224);
				BitmapData val2 = val.LockBits(new Rectangle(0, 0, width, height), (ImageLockMode)3, (PixelFormat)137224);
				int stride = val2.Stride;
				int stride2 = val2.Stride;
				byte* ptr = (byte*)val2.Scan0.ToPointer() + (nint)stride2 * (nint)(height - 1);
				byte* ptr2 = (byte*)buffer.ToPointer();
				for (int i = 0; i < height; i++)
				{
					Win32.memcpy(ptr, ptr2, stride);
					ptr -= stride2;
					ptr2 += stride;
				}
				val.UnlockBits(val2);
				parent.OnNewFrame(val);
				((Image)val).Dispose();
			}
			return 0;
		}
	}

	private string fileName;

	private int framesReceived;

	private long bytesReceived;

	private bool preventFreezing;

	private bool referenceClockEnabled = true;

	private Thread thread;

	private ManualResetEvent stopEvent;

	public virtual string Source
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	public int FramesReceived
	{
		get
		{
			int result = framesReceived;
			framesReceived = 0;
			return result;
		}
	}

	public long BytesReceived
	{
		get
		{
			long result = bytesReceived;
			bytesReceived = 0L;
			return result;
		}
	}

	public bool IsRunning
	{
		get
		{
			if (thread != null)
			{
				if (!thread.Join(0))
				{
					return true;
				}
				Free();
			}
			return false;
		}
	}

	public bool PreventFreezing
	{
		get
		{
			return preventFreezing;
		}
		set
		{
			preventFreezing = value;
		}
	}

	public bool ReferenceClockEnabled
	{
		get
		{
			return referenceClockEnabled;
		}
		set
		{
			referenceClockEnabled = value;
		}
	}

	public event NewFrameEventHandler NewFrame;

	public event VideoSourceErrorEventHandler VideoSourceError;

	public event PlayingFinishedEventHandler PlayingFinished;

	public FileVideoSource()
	{
	}

	public FileVideoSource(string fileName)
	{
		this.fileName = fileName;
	}

	public void Start()
	{
		if (!IsRunning)
		{
			if (fileName == null || fileName == string.Empty)
			{
				throw new ArgumentException("Video source is not specified");
			}
			framesReceived = 0;
			bytesReceived = 0L;
			stopEvent = new ManualResetEvent(initialState: false);
			thread = new Thread(WorkerThread);
			thread.Name = fileName;
			thread.Start();
		}
	}

	public void SignalToStop()
	{
		if (thread != null)
		{
			stopEvent.Set();
		}
	}

	public void WaitForStop()
	{
		if (thread != null)
		{
			thread.Join();
			Free();
		}
	}

	public void Stop()
	{
		if (IsRunning)
		{
			thread.Abort();
			WaitForStop();
		}
	}

	private void Free()
	{
		thread = null;
		stopEvent.Close();
		stopEvent = null;
	}

	private void WorkerThread()
	{
		ReasonToFinishPlaying reason = ReasonToFinishPlaying.StoppedByUser;
		Grabber grabber = new Grabber(this);
		object obj = null;
		object obj2 = null;
		IGraphBuilder graphBuilder = null;
		IBaseFilter filter = null;
		IBaseFilter baseFilter = null;
		ISampleGrabber sampleGrabber = null;
		IMediaControl mediaControl = null;
		IMediaEventEx mediaEventEx = null;
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.FilterGraph);
			if ((object)typeFromCLSID == null)
			{
				throw new ApplicationException("Failed creating filter graph");
			}
			obj = Activator.CreateInstance(typeFromCLSID);
			graphBuilder = (IGraphBuilder)obj;
			graphBuilder.AddSourceFilter(fileName, "source", out filter);
			if (filter == null)
			{
				throw new ApplicationException("Failed creating source filter");
			}
			typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SampleGrabber);
			if ((object)typeFromCLSID == null)
			{
				throw new ApplicationException("Failed creating sample grabber");
			}
			obj2 = Activator.CreateInstance(typeFromCLSID);
			sampleGrabber = (ISampleGrabber)obj2;
			baseFilter = (IBaseFilter)obj2;
			graphBuilder.AddFilter(baseFilter, "grabber");
			AMMediaType aMMediaType = new AMMediaType();
			aMMediaType.MajorType = MediaType.Video;
			aMMediaType.SubType = MediaSubType.RGB24;
			sampleGrabber.SetMediaType(aMMediaType);
			int num = 0;
			IPin inPin = Tools.GetInPin(baseFilter, 0);
			IPin pin = null;
			while (true)
			{
				pin = Tools.GetOutPin(filter, num);
				if (pin == null)
				{
					Marshal.ReleaseComObject(inPin);
					throw new ApplicationException("Did not find acceptable output video pin in the given source");
				}
				if (graphBuilder.Connect(pin, inPin) >= 0)
				{
					break;
				}
				Marshal.ReleaseComObject(pin);
				pin = null;
				num++;
			}
			Marshal.ReleaseComObject(pin);
			Marshal.ReleaseComObject(inPin);
			if (sampleGrabber.GetConnectedMediaType(aMMediaType) == 0)
			{
				VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(aMMediaType.FormatPtr, typeof(VideoInfoHeader));
				grabber.Width = videoInfoHeader.BmiHeader.Width;
				grabber.Height = videoInfoHeader.BmiHeader.Height;
				aMMediaType.Dispose();
			}
			if (!preventFreezing)
			{
				graphBuilder.Render(Tools.GetOutPin(baseFilter, 0));
				IVideoWindow videoWindow = (IVideoWindow)obj;
				videoWindow.put_AutoShow(autoShow: false);
				videoWindow = null;
			}
			sampleGrabber.SetBufferSamples(bufferThem: false);
			sampleGrabber.SetOneShot(oneShot: false);
			sampleGrabber.SetCallback(grabber, 1);
			if (!referenceClockEnabled)
			{
				IMediaFilter mediaFilter = (IMediaFilter)obj;
				mediaFilter.SetSyncSource(null);
			}
			mediaControl = (IMediaControl)obj;
			mediaEventEx = (IMediaEventEx)obj;
			mediaControl.Run();
			do
			{
				if (mediaEventEx != null && mediaEventEx.GetEvent(out var lEventCode, out var lParam, out var lParam2, 0) >= 0)
				{
					mediaEventEx.FreeEventParams(lEventCode, lParam, lParam2);
					if (lEventCode == DsEvCode.Complete)
					{
						reason = ReasonToFinishPlaying.EndOfStreamReached;
						break;
					}
				}
			}
			while (!stopEvent.WaitOne(100, exitContext: false));
			mediaControl.Stop();
		}
		catch (Exception ex)
		{
			if (this.VideoSourceError != null)
			{
				this.VideoSourceError(this, new VideoSourceErrorEventArgs(ex.Message));
			}
		}
		finally
		{
			graphBuilder = null;
			baseFilter = null;
			sampleGrabber = null;
			mediaControl = null;
			mediaEventEx = null;
			if (obj != null)
			{
				Marshal.ReleaseComObject(obj);
				obj = null;
			}
			if (filter != null)
			{
				Marshal.ReleaseComObject(filter);
				filter = null;
			}
			if (obj2 != null)
			{
				Marshal.ReleaseComObject(obj2);
				obj2 = null;
			}
		}
		if (this.PlayingFinished != null)
		{
			this.PlayingFinished(this, reason);
		}
	}

	protected void OnNewFrame(Bitmap image)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		framesReceived++;
		bytesReceived += ((Image)image).Width * ((Image)image).Height * (Image.GetPixelFormatSize(((Image)image).PixelFormat) >> 3);
		if (!stopEvent.WaitOne(0, exitContext: false) && this.NewFrame != null)
		{
			this.NewFrame(this, new NewFrameEventArgs(image));
		}
	}
}
