using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace AForge.Video;

public class MJPEGStream : IVideoSource
{
	private const int bufSize = 1048576;

	private const int readSize = 1024;

	private string source;

	private string login;

	private string password;

	private IWebProxy proxy;

	private int framesReceived;

	private long bytesReceived;

	private bool useSeparateConnectionGroup = true;

	private int requestTimeout = 10000;

	private bool forceBasicAuthentication;

	private Thread thread;

	private ManualResetEvent stopEvent;

	private ManualResetEvent reloadEvent;

	private string userAgent = "Mozilla/5.0";

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

	public string Source
	{
		get
		{
			return source;
		}
		set
		{
			source = value;
			if (thread != null)
			{
				reloadEvent.Set();
			}
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

	public string HttpUserAgent
	{
		get
		{
			return userAgent;
		}
		set
		{
			userAgent = value;
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

	public MJPEGStream()
	{
	}

	public MJPEGStream(string source)
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
			reloadEvent = new ManualResetEvent(initialState: false);
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
		reloadEvent.Close();
		reloadEvent = null;
	}

	private void WorkerThread()
	{
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Expected O, but got Unknown
		byte[] array = new byte[1048576];
		byte[] array2 = new byte[3] { 255, 216, 255 };
		int num = 3;
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		while (!stopEvent.WaitOne(0, exitContext: false))
		{
			reloadEvent.Reset();
			HttpWebRequest httpWebRequest = null;
			WebResponse webResponse = null;
			Stream stream = null;
			byte[] array3 = null;
			string text = null;
			bool flag = false;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 1;
			int num6 = 0;
			int num7 = 0;
			try
			{
				httpWebRequest = (HttpWebRequest)WebRequest.Create(source);
				if (userAgent != null)
				{
					httpWebRequest.UserAgent = userAgent;
				}
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
				string contentType = webResponse.ContentType;
				string[] array4 = contentType.Split(new char[1] { '/' });
				int num8;
				if (array4[0] == "application" && array4[1] == "octet-stream")
				{
					num8 = 0;
					array3 = new byte[0];
				}
				else
				{
					if (!(array4[0] == "multipart") || !contentType.Contains("mixed"))
					{
						throw new Exception("Invalid content type.");
					}
					int num9 = contentType.IndexOf("boundary", 0);
					if (num9 != -1)
					{
						num9 = contentType.IndexOf("=", num9 + 8);
					}
					if (num9 == -1)
					{
						num8 = 0;
						array3 = new byte[0];
					}
					else
					{
						text = contentType.Substring(num9 + 1);
						text = text.Trim(' ', '"');
						array3 = aSCIIEncoding.GetBytes(text);
						num8 = array3.Length;
						flag = false;
					}
				}
				stream = webResponse.GetResponseStream();
				stream.ReadTimeout = requestTimeout;
				while (!stopEvent.WaitOne(0, exitContext: false) && !reloadEvent.WaitOne(0, exitContext: false))
				{
					if (num3 > 1047552)
					{
						num3 = (num4 = (num2 = 0));
					}
					int num10;
					if ((num10 = stream.Read(array, num3, 1024)) == 0)
					{
						throw new ApplicationException();
					}
					num3 += num10;
					num2 += num10;
					bytesReceived += num10;
					if (num8 != 0 && !flag)
					{
						num4 = ByteArrayUtils.Find(array, array3, 0, num2);
						if (num4 == -1)
						{
							continue;
						}
						for (int num11 = num4 - 1; num11 >= 0; num11--)
						{
							byte b = array[num11];
							if (b == 10 || b == 13)
							{
								break;
							}
							text = (char)b + text;
						}
						array3 = aSCIIEncoding.GetBytes(text);
						num8 = array3.Length;
						flag = true;
					}
					if (num5 == 1 && num2 >= num)
					{
						num6 = ByteArrayUtils.Find(array, array2, num4, num2);
						if (num6 != -1)
						{
							num4 = num6 + num;
							num2 = num3 - num4;
							num5 = 2;
						}
						else
						{
							num2 = num - 1;
							num4 = num3 - num2;
						}
					}
					while (num5 == 2 && num2 != 0 && num2 >= num8)
					{
						num7 = ByteArrayUtils.Find(array, (num8 != 0) ? array3 : array2, num4, num2);
						if (num7 != -1)
						{
							num4 = num7;
							num2 = num3 - num4;
							framesReceived++;
							if (this.NewFrame != null && !stopEvent.WaitOne(0, exitContext: false))
							{
								Bitmap val = (Bitmap)Image.FromStream((Stream)new MemoryStream(array, num6, num7 - num6));
								this.NewFrame(this, new NewFrameEventArgs(val));
								((Image)val).Dispose();
								val = null;
							}
							num4 = num7 + num8;
							num2 = num3 - num4;
							Array.Copy(array, num4, array, 0, num2);
							num3 = num2;
							num4 = 0;
							num5 = 1;
						}
						else if (num8 != 0)
						{
							num2 = num8 - 1;
							num4 = num3 - num2;
						}
						else
						{
							num2 = 0;
							num4 = num3;
						}
					}
				}
			}
			catch (ApplicationException)
			{
				Thread.Sleep(250);
			}
			catch (ThreadAbortException)
			{
				break;
			}
			catch (Exception ex3)
			{
				if (this.VideoSourceError != null)
				{
					this.VideoSourceError(this, new VideoSourceErrorEventArgs(ex3.Message));
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
