using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000B4 RID: 180
	public partial class FormKeyLogger : FormMaterial
	{
		// Token: 0x060005A2 RID: 1442 RVA: 0x000524C4 File Offset: 0x000506C4
		public FormKeyLogger()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x060005A3 RID: 1443 RVA: 0x000524E4 File Offset: 0x000506E4
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x060005A4 RID: 1444 RVA: 0x0005250E File Offset: 0x0005070E
		private void ChangeScheme(object sender)
		{
			this.richTextBox1.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x00052520 File Offset: 0x00050720
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x060005A6 RID: 1446 RVA: 0x00052535 File Offset: 0x00050735
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

		// Token: 0x04000476 RID: 1142
		public Clients client;

		// Token: 0x04000477 RID: 1143
		public Clients parrent;
	}
}
