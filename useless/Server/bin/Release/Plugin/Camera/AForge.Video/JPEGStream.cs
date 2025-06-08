using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace AForge.Video;

public class JPEGStream : IVideoSource
{
	private const int bufferSize = 1048576;

	private const int readSize = 1024;

	private string source;

	private string login;

	private string password;

	private IWebProxy proxy;

	private int framesReceived;

	private long bytesReceived;

	private bool useSeparateConnectionGroup;

	private bool preventCaching = true;

	private int frameInterval;

	private int requestTimeout = 10000;

	private bool forceBasicAuthentication;

	private Thread thread;

	private ManualResetEvent stopEvent;

	public bool SeparateConnectionGroup
	{
		get
		{
			return useSeparateConnectionGroup;
		}
		set
		{
			useSeparateConnectionGroup = value;
		}
	}

	public bool PreventCaching
	{
		get
		{
			return preventCaching;
		}
		set
		{
			preventCaching = value;
		}
	}

	public int FrameInterval
	{
		get
		{
			return frameInterval;
		}
		set
		{
			frameInterval = value;
		}
	}

	public virtual string Source
	{
		get
		{
			return source;
		}
		set
		{
			source = value;
		}
	}

	public string Login
	{
		get
		{
			return login;
		}
		set
		{
			login = value;
		}
	}

	public string Password
	{
		get
		{
			return password;
		}
		set
		{
			password = value;
		}
	}

	public IWebProxy Proxy
	{
		get
		{
			return proxy;
		}
		set
		{
			proxy = value;
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

	public int RequestTimeout
	{
		get
		{
			return requestTimeout;
		}
		set
		{
			requestTimeout = value;
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

	public bool ForceBasicAuthentication
	{
		get
		{
			return forceBasicAuthentication;
		}
		set
		{
			forceBasicAuthentication = value;
		}
	}

	public event NewFrameEventHandler NewFrame;

	public event VideoSourceErrorEventHandler VideoSourceError;

	public event PlayingFinishedEventHandler PlayingFinished;

	public JPEGStream()
	{
	}

	public JPEGStream(string source)
	{
		this.source = source;
	}

	public void Start()
	{
		if (!IsRunning)
		{
			if (source == null || source == string.Empty)
			{
				throw new ArgumentException("Video source is not specified.");
			}
			framesReceived = 0;
			bytesReceived = 0L;
			stopEvent = new ManualResetEvent(initialState: false);
			thread = new Thread(WorkerThread);
			thread.Name = source;
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
			stopEvent.Set();
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
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Expected O, but got Unknown
		byte[] buffer = new byte[1048576];
		HttpWebRequest httpWebRequest = null;
		WebResponse webResponse = null;
		Stream stream = null;
		Random random = new Random((int)DateTime.Now.Ticks);
		while (!stopEvent.WaitOne(0, exitContext: false))
		{
			int num = 0;
			try
			{
				DateTime now = DateTime.Now;
				httpWebRequest = (preventCaching ? ((HttpWebRequest)WebRequest.Create(source + ((source.IndexOf('?') == -1) ? '?' : '&') + "fake=" + random.Next().ToString())) : ((HttpWebRequest)WebRequest.Create(source)));
				if (proxy != null)
				{
					httpWebRequest.Proxy = proxy;
				}
				httpWebRequest.Timeout = requestTimeout;
				if (login != null && password != null && login != string.Empty)
				{
					httpWebRequest.Credentials = new NetworkCredential(login, password);
				}
				if (useSeparateConnectionGroup)
				{
					httpWebRequest.ConnectionGroupName = GetHashCode().ToString();
				}
				if (forceBasicAuthentication)
				{
					string s = $"{login}:{password}";
					s = Convert.ToBase64String(Encoding.Default.GetBytes(s));
					httpWebRequest.Headers["Authorization"] = "Basic " + s;
				}
				webResponse = httpWebRequest.GetResponse();
				stream = webResponse.GetResponseStream();
				stream.ReadTimeout = requestTimeout;
				while (!stopEvent.WaitOne(0, exitContext: false))
				{
					if (num > 1047552)
					{
						num = 0;
					}
					int num2;
					if ((num2 = stream.Read(buffer, num, 1024)) == 0)
					{
						break;
					}
					num += num2;
					bytesReceived += num2;
				}
				if (!stopEvent.WaitOne(0, exitContext: false))
				{
					framesReceived++;
					if (this.NewFrame != null)
					{
						Bitmap val = (Bitmap)Image.FromStream((Stream)new MemoryStream(buffer, 0, num));
						this.NewFrame(this, new NewFrameEventArgs(val));
						((Image)val).Dispose();
						val = null;
					}
				}
				if (frameInterval > 0)
				{
					TimeSpan timeSpan = DateTime.Now.Subtract(now);
					int num3 = frameInterval - (int)timeSpan.TotalMilliseconds;
					if (num3 > 0 && stopEvent.WaitOne(num3, exitContext: false))
					{
						break;
					}
				}
			}
			catch (ThreadAbortException)
			{
				break;
			}
			catch (Exception ex2)
			{
				if (this.VideoSourceError != null)
				{
					this.VideoSourceError(this, new VideoSourceErrorEventArgs(ex2.Message));
				}
				Thread.Sleep(250);
			}
			finally
			{
				if (httpWebRequest != null)
				{
					httpWebRequest.Abort();
					httpWebRequest = null;
				}
				if (stream != null)
				{
					stream.Close();
					stream = null;
				}
				if (webResponse != null)
				{
					webResponse.Close();
					webResponse = null;
				}
			}
			if (stopEvent.WaitOne(0, exitContext: false))
			{
				break;
			}
		}
		if (this.PlayingFinished != null)
		{
			this.PlayingFinished(this, ReasonToFinishPlaying.StoppedByUser);
		}
	}
}
