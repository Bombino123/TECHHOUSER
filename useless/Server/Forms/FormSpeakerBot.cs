using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x0200009C RID: 156
	public partial class FormSpeakerBot : FormMaterial
	{
		// Token: 0x06000438 RID: 1080 RVA: 0x00033A47 File Offset: 0x00031C47
		public FormSpeakerBot()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x00033A67 File Offset: 0x00031C67
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x00033A94 File Offset: 0x00031C94
		private void ChangeScheme(object sender)
		{
			this.rjButton1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.ForeColor = FormMaterial.PrimaryColor;
			this.richTextBox1.ForeColor = FormMaterial.PrimaryColor;
			this.rjComboBox1.BorderColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x00033AE1 File Offset: 0x00031CE1
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600043C RID: 1084 RVA: 0x00033AF6 File Offset: 0x00031CF6
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

		// Token: 0x0600043D RID: 1085 RVA: 0x00033B28 File Offset: 0x00031D28
		private void rjButton1_Click_1(object sender, EventArgs e)
		{
			if (this.client.itsConnect && this.rjComboBox1.SelectedIndex != 0)
			{
				this.client.Send(new object[]
				{
					"Speak",
					this.trackBar2.Value,
					this.trackBar1.Value,
					((string)this.rjComboBox1.SelectedItem).Split(new string[]
					{
						" | "
					}, StringSplitOptions.None)[0],
					this.richTextBox1.Text
				});
			}
		}

		// Token: 0x04000287 RID: 647
		public Clients client;

		// Token: 0x04000288 RID: 648
		public Clients parrent;
	}
}
