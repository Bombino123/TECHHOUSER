using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Server.Connectings;
using Server.Connectings.Events;

namespace Server.Helper.Sock5
{
	// Token: 0x02000086 RID: 134
	public class Server
	{
		// Token: 0x14000004 RID: 4
		// (add) Token: 0x060002EE RID: 750 RVA: 0x00025B90 File Offset: 0x00023D90
		// (remove) Token: 0x060002EF RID: 751 RVA: 0x00025BC8 File Offset: 0x00023DC8
		public event Server.BHandler ResponceEvent;

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x060002F0 RID: 752 RVA: 0x00025C00 File Offset: 0x00023E00
		// (remove) Token: 0x060002F1 RID: 753 RVA: 0x00025C38 File Offset: 0x00023E38
		public event Server.SHandler DisconnectEvent;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x060002F2 RID: 754 RVA: 0x00025C70 File Offset: 0x00023E70
		// (remove) Token: 0x060002F3 RID: 755 RVA: 0x00025CA8 File Offset: 0x00023EA8
		public event Server.SHandler ConnectEvent;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x060002F4 RID: 756 RVA: 0x00025CE0 File Offset: 0x00023EE0
		// (remove) Token: 0x060002F5 RID: 757 RVA: 0x00025D18 File Offset: 0x00023F18
		public event Server.AHandler StopedServer;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x060002F6 RID: 758 RVA: 0x00025D50 File Offset: 0x00023F50
		// (remove) Token: 0x060002F7 RID: 759 RVA: 0x00025D88 File Offset: 0x00023F88
		public event Server.AHandler DisconnectClientEvent;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x060002F8 RID: 760 RVA: 0x00025DC0 File Offset: 0x00023FC0
		// (remove) Token: 0x060002F9 RID: 761 RVA: 0x00025DF8 File Offset: 0x00023FF8
		public event Server.AHandler StartedServer;

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x060002FA RID: 762 RVA: 0x00025E30 File Offset: 0x00024030
		// (remove) Token: 0x060002FB RID: 763 RVA: 0x00025E68 File Offset: 0x00024068
		public event Server.AHandler ResponceServerEvent;

		// Token: 0x060002FC RID: 764 RVA: 0x00025E9D File Offset: 0x0002409D
		public Server(List<Clients> ClientReverse)
		{
			this.Stoped = true;
			this.ClientReverse = ClientReverse;
		}

		// Token: 0x060002FD RID: 765 RVA: 0x00025ED4 File Offset: 0x000240D4
		public Server(Clients ClientReverse)
		{
			this.Stoped = true;
			this.Disconnected = false;
			this.ClientReverse.Add(ClientReverse);
			ClientReverse.eventDisconnect += this.Stop;
		}

		// Token: 0x060002FE RID: 766 RVA: 0x00025F34 File Offset: 0x00024134
		public void Responce(Client client, long Sent, long Recive)
		{
			this.Recives += Recive;
			this.Sents += Sent;
			if (this.ResponceServerEvent != null)
			{
				this.ResponceServerEvent(this);
			}
		}

		// Token: 0x060002FF RID: 767 RVA: 0x00025F68 File Offset: 0x00024168
		public void Start(int port)
		{
			if (!this.Stoped)
			{
				return;
			}
			this.Stoped = false;
			this.port = port;
			IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
			this.server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.server.Bind(localEP);
			this.server.Listen(2500);
			this.server.BeginAccept(new AsyncCallback(this.EndAccept), null);
			if (this.StartedServer != null)
			{
				this.StartedServer(this);
			}
		}

		// Token: 0x06000300 RID: 768 RVA: 0x00025FF0 File Offset: 0x000241F0
		public void Stop(object sender, EventDisconnect disconnect)
		{
			this.Stop();
			if (!this.Disconnected)
			{
				this.Disconnected = true;
				if (this.DisconnectClientEvent != null)
				{
					this.DisconnectClientEvent(this);
				}
			}
			List<Clients> clientReverse = this.ClientReverse;
			lock (clientReverse)
			{
				this.ClientReverse.Remove(disconnect.clients);
			}
		}

		// Token: 0x06000301 RID: 769 RVA: 0x00026068 File Offset: 0x00024268
		public void Stop()
		{
			if (this.Stoped)
			{
				return;
			}
			this.Stoped = true;
			if (this.StopedServer != null)
			{
				this.StopedServer(this);
			}
			Socket socket = this.server;
			if (socket != null)
			{
				socket.Dispose();
			}
			Client[] array = this.clients.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Disconnect();
			}
			Clients[] array2 = this.ClientReverses.ToArray();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Disconnect();
			}
		}

		// Token: 0x06000302 RID: 770 RVA: 0x000260F0 File Offset: 0x000242F0
		public Client Search(int connectid)
		{
			foreach (Client client in this.clients.ToArray())
			{
				if (client.ConnectionId == connectid)
				{
					return client;
				}
			}
			return null;
		}

		// Token: 0x06000303 RID: 771 RVA: 0x00026128 File Offset: 0x00024328
		private void EndAccept(IAsyncResult ar)
		{
			try
			{
				Client client = new Client(this.server.EndAccept(ar), this);
				if (this.ResponceEvent != null)
				{
					client.ResponceEvent += delegate(Client cl, long ls, long rs)
					{
						this.ResponceEvent(cl, ls, rs);
					};
				}
				if (this.DisconnectEvent != null)
				{
					client.DisconnectEvent += delegate(Client cl)
					{
						this.DisconnectEvent(cl);
					};
				}
				if (this.ConnectEvent != null)
				{
					client.ConnectEvent += delegate(Client cl)
					{
						this.ConnectEvent(cl);
					};
				}
				client.ResponceEvent += this.Responce;
				List<Client> obj = this.clients;
				lock (obj)
				{
					this.clients.Add(client);
				}
			}
			catch (Exception)
			{
			}
			try
			{
				this.server.BeginAccept(new AsyncCallback(this.EndAccept), null);
			}
			catch
			{
			}
		}

		// Token: 0x040001B5 RID: 437
		public List<Clients> ClientReverse = new List<Clients>();

		// Token: 0x040001B6 RID: 438
		public List<Clients> ClientReverses = new List<Clients>();

		// Token: 0x040001B7 RID: 439
		public List<Client> clients = new List<Client>();

		// Token: 0x040001B8 RID: 440
		public Socket server;

		// Token: 0x040001B9 RID: 441
		public int port;

		// Token: 0x040001BA RID: 442
		public bool fullclose;

		// Token: 0x040001C2 RID: 450
		public object Tag;

		// Token: 0x040001C3 RID: 451
		public bool Stoped;

		// Token: 0x040001C4 RID: 452
		public bool Disconnected;

		// Token: 0x040001C5 RID: 453
		public long Recives;

		// Token: 0x040001C6 RID: 454
		public long Sents;

		// Token: 0x02000228 RID: 552
		// (Invoke) Token: 0x06000902 RID: 2306
		public delegate void SHandler(Client client);

		// Token: 0x02000229 RID: 553
		// (Invoke) Token: 0x06000906 RID: 2310
		public delegate void AHandler(Server server);

		// Token: 0x0200022A RID: 554
		// (Invoke) Token: 0x0600090A RID: 2314
		public delegate void BHandler(Client client, long Sent, long Recive);
	}
}
