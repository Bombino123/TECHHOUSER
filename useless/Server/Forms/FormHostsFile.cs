using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x0200009B RID: 155
	public partial class FormHostsFile : FormMaterial
	{
		// Token: 0x06000430 RID: 1072 RVA: 0x0003377B File Offset: 0x0003197B
		public FormHostsFile()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x0003379C File Offset: 0x0003199C
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.richTextBox1.KeyDown += this.RichTextBox1_KeyDown;
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x000337E8 File Offset: 0x000319E8
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

		// Token: 0x06000433 RID: 1075 RVA: 0x00033835 File Offset: 0x00031A35
		private void ChangeScheme(object sender)
		{
			this.richTextBox1.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x00033847 File Offset: 0x00031A47
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x0003385C File Offset: 0x00031A5C
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

		// Token: 0x04000282 RID: 642
		public Clients client;

		// Token: 0x04000283 RID: 643
		public Clients parrent;
	}
}
