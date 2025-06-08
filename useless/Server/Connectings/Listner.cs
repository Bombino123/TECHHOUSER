using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using Server.Helper;

namespace Server.Connectings
{
	// Token: 0x020000CC RID: 204
	public class Listner
	{
		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000691 RID: 1681 RVA: 0x0005C661 File Offset: 0x0005A861
		// (set) Token: 0x06000692 RID: 1682 RVA: 0x0005C669 File Offset: 0x0005A869
		private Socket Server { get; set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000693 RID: 1683 RVA: 0x0005C672 File Offset: 0x0005A872
		// (set) Token: 0x06000694 RID: 1684 RVA: 0x0005C67A File Offset: 0x0005A87A
		public int port { get; set; }

		// Token: 0x06000695 RID: 1685 RVA: 0x0005C684 File Offset: 0x0005A884
		public void Stop()
		{
			Socket server = this.Server;
			if (server != null)
			{
				server.Dispose();
			}
			Methods.AppendLogs("Server", "Stop Listner: " + this.port.ToString(), Color.Red);
		}

		// Token: 0x06000696 RID: 1686 RVA: 0x0005C6CC File Offset: 0x0005A8CC
		public Listner(int port)
		{
			this.port = port;
			IPEndPoint localEP = new IPEndPoint(IPAddress.Any, port);
			this.Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.Server.ReceiveBufferSize = 512000;
			this.Server.SendBufferSize = 512000;
			this.Server.Bind(localEP);
			this.Server.Listen(2500);
			this.Server.BeginAccept(new AsyncCallback(this.EndAccept), null);
			Methods.AppendLogs("Server", "Start Listner: " + port.ToString(), Color.Green);
		}

		// Token: 0x06000697 RID: 1687 RVA: 0x0005C778 File Offset: 0x0005A978
		private void EndAccept(IAsyncResult ar)
		{
			try
			{
				new Clients(this.Server.EndAccept(ar));
			}
			catch
			{
			}
			try
			{
				this.Server.BeginAccept(new AsyncCallback(this.EndAccept), null);
			}
			catch
			{
			}
		}
	}
}
