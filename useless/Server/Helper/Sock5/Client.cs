using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Leb128;
using Server.Connectings;
using Server.Connectings.Events;

namespace Server.Helper.Sock5
{
	// Token: 0x02000081 RID: 129
	public class Client
	{
		// Token: 0x17000031 RID: 49
		// (get) Token: 0x060002D2 RID: 722 RVA: 0x00025276 File Offset: 0x00023476
		// (set) Token: 0x060002D3 RID: 723 RVA: 0x0002527E File Offset: 0x0002347E
		public bool ItsConnect { get; private set; }

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x060002D4 RID: 724 RVA: 0x00025288 File Offset: 0x00023488
		public int ConnectionId
		{
			get
			{
				return this.socket.Handle.ToInt32();
			}
		}

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060002D5 RID: 725 RVA: 0x000252A8 File Offset: 0x000234A8
		// (set) Token: 0x060002D6 RID: 726 RVA: 0x000252B0 File Offset: 0x000234B0
		public string IPAddress { get; private set; }

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060002D7 RID: 727 RVA: 0x000252B9 File Offset: 0x000234B9
		// (set) Token: 0x060002D8 RID: 728 RVA: 0x000252C1 File Offset: 0x000234C1
		public string Port { get; private set; }

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060002D9 RID: 729 RVA: 0x000252CA File Offset: 0x000234CA
		// (set) Token: 0x060002DA RID: 730 RVA: 0x000252D2 File Offset: 0x000234D2
		public long Sents { get; private set; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060002DB RID: 731 RVA: 0x000252DB File Offset: 0x000234DB
		// (set) Token: 0x060002DC RID: 732 RVA: 0x000252E3 File Offset: 0x000234E3
		public long Recives { get; private set; }

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x060002DD RID: 733 RVA: 0x000252EC File Offset: 0x000234EC
		// (remove) Token: 0x060002DE RID: 734 RVA: 0x00025324 File Offset: 0x00023524
		public event Client.BHandler ResponceEvent;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x060002DF RID: 735 RVA: 0x0002535C File Offset: 0x0002355C
		// (remove) Token: 0x060002E0 RID: 736 RVA: 0x00025394 File Offset: 0x00023594
		public event Client.RHandler DisconnectEvent;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x060002E1 RID: 737 RVA: 0x000253CC File Offset: 0x000235CC
		// (remove) Token: 0x060002E2 RID: 738 RVA: 0x00025404 File Offset: 0x00023604
		public event Client.RHandler ConnectEvent;

		// Token: 0x060002E3 RID: 739 RVA: 0x0002543C File Offset: 0x0002363C
		public Client(Socket socket, Server server)
		{
			this.socket = socket;
			this.server = server;
			this.ItsConnect = true;
			this.Auth = false;
			this.Sents = 0L;
			this.Recives = 0L;
			this.Last();
			this.TimeOutTimer = new Timer(new TimerCallback(this.Check), null, 1, 5000);
			server.ClientReverse[Randomizer.random.Next(server.ClientReverse.Count)].Send(new object[]
			{
				"Accept",
				this.ConnectionId,
				server.port
			});
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x000254F0 File Offset: 0x000236F0
		public void Accept(Clients ClientReverse)
		{
			this.ClientReverse = ClientReverse;
			this.ClientReverse.eventDisconnect += this.Disconnect;
			this.Buffer = new byte[12000];
			this.socket.BeginReceive(this.Buffer, 0, 12000, SocketFlags.None, new AsyncCallback(this.ReadAsyncConnect), null);
		}

		// Token: 0x060002E5 RID: 741 RVA: 0x00025554 File Offset: 0x00023754
		public void Disconnect(object sender, EventDisconnect eventDisconnect)
		{
			if (!this.ItsConnect)
			{
				return;
			}
			this.ItsConnect = false;
			Timer timeOutTimer = this.TimeOutTimer;
			if (timeOutTimer != null)
			{
				timeOutTimer.Dispose();
			}
			Socket socket = this.socket;
			if (socket != null)
			{
				socket.Dispose();
			}
			List<Client> clients = this.server.clients;
			lock (clients)
			{
				this.server.clients.Remove(this);
			}
			if (this.DisconnectEvent != null)
			{
				this.DisconnectEvent(this);
			}
		}

		// Token: 0x060002E6 RID: 742 RVA: 0x000255EC File Offset: 0x000237EC
		public void Disconnect()
		{
			if (!this.ItsConnect)
			{
				return;
			}
			this.ItsConnect = false;
			Timer timeOutTimer = this.TimeOutTimer;
			if (timeOutTimer != null)
			{
				timeOutTimer.Dispose();
			}
			Socket socket = this.socket;
			if (socket != null)
			{
				socket.Dispose();
			}
			Clients clientReverse = this.ClientReverse;
			if (clientReverse != null)
			{
				clientReverse.Disconnect();
			}
			List<Client> clients = this.server.clients;
			lock (clients)
			{
				this.server.clients.Remove(this);
			}
			if (this.DisconnectEvent != null)
			{
				this.DisconnectEvent(this);
			}
		}

		// Token: 0x060002E7 RID: 743 RVA: 0x00025694 File Offset: 0x00023894
		public void Send(byte[] Data)
		{
			if (!this.ItsConnect)
			{
				return;
			}
			this.Last();
			this.socket.Send(Data);
			this.Recives += (long)Data.Length;
			if (this.ResponceEvent != null)
			{
				this.ResponceEvent(this, 0L, (long)Data.Length);
			}
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x000256E8 File Offset: 0x000238E8
		public void ReadAsyncConnect(IAsyncResult ar)
		{
			if (!this.ItsConnect || !this.ClientReverse.itsConnect)
			{
				return;
			}
			try
			{
				int num = this.socket.EndReceive(ar);
				if (num > 0)
				{
					byte[] array = new byte[num];
					Array.Copy(this.Buffer, array, num);
					this.Last();
					if (array[0] == 5)
					{
						if (this.Auth)
						{
							if (array.Length >= 6)
							{
								if (array[1] == 1)
								{
									if (array[3] == 4)
									{
										this.Send(new byte[]
										{
											5,
											7
										});
										this.Disconnect();
									}
									else
									{
										Socks5Request socks5Request = Socks5Request.Parse(array);
										byte[] array2 = LEB128.Write(new object[]
										{
											"Connect",
											socks5Request.DestinationAddress,
											(int)socks5Request.DestinationPort
										});
										this.IPAddress = socks5Request.DestinationAddress;
										this.Port = socks5Request.DestinationPort.ToString();
										this.ClientReverse.Send(array2);
										this.Sents += (long)array2.Length;
										if (this.ResponceEvent != null)
										{
											this.ResponceEvent(this, (long)array2.Length, 0L);
										}
									}
								}
								else
								{
									this.Send(new byte[]
									{
										5,
										7
									});
									this.Disconnect();
								}
							}
							else
							{
								this.socket.BeginReceive(this.Buffer, 0, 12000, SocketFlags.None, new AsyncCallback(this.ReadAsyncConnect), null);
							}
						}
						else if (this.Auth)
						{
							this.Send(new byte[]
							{
								5,
								byte.MaxValue
							});
							this.Disconnect();
						}
						else
						{
							this.Auth = true;
							byte[] array3 = new byte[2];
							array3[0] = 5;
							this.Send(array3);
							this.socket.BeginReceive(this.Buffer, 0, 12000, SocketFlags.None, new AsyncCallback(this.ReadAsyncConnect), null);
						}
					}
					else
					{
						this.Send(new byte[]
						{
							5,
							7
						});
						this.Disconnect();
					}
				}
				else
				{
					this.Disconnect();
				}
			}
			catch
			{
				this.Disconnect();
			}
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0002590C File Offset: 0x00023B0C
		public void ReadAsyncData(IAsyncResult ar)
		{
			if (!this.ItsConnect || !this.ClientReverse.itsConnect)
			{
				return;
			}
			try
			{
				int num = this.socket.EndReceive(ar);
				if (num > 0)
				{
					byte[] array = new byte[num];
					Array.Copy(this.Buffer, array, num);
					this.Last();
					this.ClientReverse.Send(new object[]
					{
						"Data",
						array
					});
					this.Sents += (long)array.Length;
					if (this.ResponceEvent != null)
					{
						this.ResponceEvent(this, (long)array.Length, 0L);
					}
					this.socket.BeginReceive(this.Buffer, 0, 12000, SocketFlags.None, new AsyncCallback(this.ReadAsyncData), null);
				}
				else
				{
					this.Disconnect();
				}
			}
			catch
			{
				this.Disconnect();
			}
		}

		// Token: 0x060002EA RID: 746 RVA: 0x000259F0 File Offset: 0x00023BF0
		public void HandleCommandResponse(object[] response)
		{
			if (!this.ItsConnect || !this.ClientReverse.itsConnect)
			{
				return;
			}
			byte b = 2;
			if (Convert.ToBoolean(response[(int)b]))
			{
				try
				{
					List<byte> list = new List<byte>();
					list.Add(5);
					list.Add(0);
					list.Add(0);
					list.Add(1);
					list.AddRange((byte[])response[(int)(b + 1)]);
					list.Add((byte)Math.Floor((int)response[(int)(b + 2)] / 256m));
					list.Add((byte)((int)response[(int)(b + 2)] % 256));
					this.Send(list.ToArray());
					this.Last();
					this.socket.BeginReceive(this.Buffer, 0, 12000, SocketFlags.None, new AsyncCallback(this.ReadAsyncData), null);
					if (this.ConnectEvent != null)
					{
						this.ConnectEvent(this);
					}
					return;
				}
				catch (Exception)
				{
					this.Send(new byte[]
					{
						5,
						1
					});
					this.Disconnect();
					return;
				}
			}
			this.Send(new byte[]
			{
				5,
				4
			});
			this.Disconnect();
		}

		// Token: 0x060002EB RID: 747 RVA: 0x00025B30 File Offset: 0x00023D30
		private double DiffSeconds(DateTime startTime, DateTime endTime)
		{
			return Math.Abs(new TimeSpan(endTime.Ticks - startTime.Ticks).TotalSeconds);
		}

		// Token: 0x060002EC RID: 748 RVA: 0x00025B5E File Offset: 0x00023D5E
		private void Check(object obj)
		{
			if (this.DiffSeconds(this.lastPing, DateTime.Now) > 30.0)
			{
				this.Disconnect();
			}
		}

		// Token: 0x060002ED RID: 749 RVA: 0x00025B82 File Offset: 0x00023D82
		public void Last()
		{
			this.lastPing = DateTime.Now;
		}

		// Token: 0x0400018C RID: 396
		public const int BufferSize = 12000;

		// Token: 0x0400018D RID: 397
		public const int TimeOut = 5000;

		// Token: 0x0400018E RID: 398
		private byte[] Buffer;

		// Token: 0x0400018F RID: 399
		private Socket socket;

		// Token: 0x04000190 RID: 400
		public Server server;

		// Token: 0x04000191 RID: 401
		public Clients ClientReverse;

		// Token: 0x04000192 RID: 402
		private Timer TimeOutTimer;

		// Token: 0x04000193 RID: 403
		private DateTime lastPing;

		// Token: 0x04000194 RID: 404
		private bool Auth;

		// Token: 0x0400019A RID: 410
		public object Tag;

		// Token: 0x02000226 RID: 550
		// (Invoke) Token: 0x060008FA RID: 2298
		public delegate void RHandler(Client client);

		// Token: 0x02000227 RID: 551
		// (Invoke) Token: 0x060008FE RID: 2302
		public delegate void BHandler(Client client, long Sent, long Recive);
	}
}
