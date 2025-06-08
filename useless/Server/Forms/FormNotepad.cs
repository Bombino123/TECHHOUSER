using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x0200009A RID: 154
	public partial class FormNotepad : FormMaterial
	{
		// Token: 0x06000428 RID: 1064 RVA: 0x000334AD File Offset: 0x000316AD
		public FormNotepad()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x000334D0 File Offset: 0x000316D0
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.richTextBox1.KeyDown += this.RichTextBox1_KeyDown;
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x0003351C File Offset: 0x0003171C
		private void RichTextBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.S)
			{
				this.client.Send(new object[]
				{
					"Save",
					this.richTextBox1.Text
				});
				e.SuppressKeyPress = true;
			}
		}

		// Token: 0x0600042B RID: 1067 RVA: 0x00033569 File Offset: 0x00031769
		private void ChangeScheme(object sender)
		{
			this.richTextBox1.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x0003357B File Offset: 0x0003177B
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x00033590 File Offset: 0x00031790
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

		// Token: 0x0400027D RID: 637
		public Clients client;

		// Token: 0x0400027E RID: 638
		public Clients parrent;
	}
}
