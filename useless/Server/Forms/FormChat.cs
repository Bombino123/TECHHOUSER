using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000B0 RID: 176
	public partial class FormChat : FormMaterial
	{
		// Token: 0x06000576 RID: 1398 RVA: 0x0004F05D File Offset: 0x0004D25D
		public FormChat()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x0004F080 File Offset: 0x0004D280
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.rjTextBox1.textBox1.KeyDown += this._KeyClick;
		}

		// Token: 0x06000578 RID: 1400 RVA: 0x0004F0D1 File Offset: 0x0004D2D1
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.richTextBox1.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000579 RID: 1401 RVA: 0x0004F0F3 File Offset: 0x0004D2F3
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600057A RID: 1402 RVA: 0x0004F108 File Offset: 0x0004D308
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

		// Token: 0x0600057B RID: 1403 RVA: 0x0004F138 File Offset: 0x0004D338
		private void _KeyClick(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return && !string.IsNullOrEmpty(this.rjTextBox1.textBox1.Text))
			{
				this.client.Send(new object[]
				{
					"Message",
					"H@ck3r: " + this.rjTextBox1.textBox1.Text + "\n"
				});
				RichTextBox richTextBox = this.richTextBox1;
				richTextBox.Text = richTextBox.Text + "H@ck3r: " + this.rjTextBox1.textBox1.Text + "\n";
				this.richTextBox1.SelectionStart = this.richTextBox1.Text.Length;
				this.richTextBox1.ScrollToCaret();
				this.rjTextBox1.textBox1.Text = "";
			}
		}

		// Token: 0x0400042C RID: 1068
		public Clients client;

		// Token: 0x0400042D RID: 1069
		public Clients parrent;
	}
}
