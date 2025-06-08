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
	// Token: 0x0200009E RID: 158
	public partial class FormClipboard : FormMaterial
	{
		// Token: 0x0600044B RID: 1099 RVA: 0x00036516 File Offset: 0x00034716
		public FormClipboard()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x00036536 File Offset: 0x00034736
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x0600044D RID: 1101 RVA: 0x00036560 File Offset: 0x00034760
		private void ChangeScheme(object sender)
		{
			this.richTextBox1.ForeColor = FormMaterial.PrimaryColor;
			this.rjButton1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.ForeColor = FormMaterial.PrimaryColor;
			this.rjButton2.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton2.ForeColor = FormMaterial.PrimaryColor;
			this.rjButton3.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton3.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600044E RID: 1102 RVA: 0x000365DD File Offset: 0x000347DD
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600044F RID: 1103 RVA: 0x000365F2 File Offset: 0x000347F2
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

		// Token: 0x06000450 RID: 1104 RVA: 0x00036622 File Offset: 0x00034822
		private void rjButton1_Click(object sender, EventArgs e)
		{
			if (this.client.itsConnect)
			{
				this.client.Send(new object[]
				{
					"Get"
				});
			}
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x0003664A File Offset: 0x0003484A
		private void rjButton2_Click(object sender, EventArgs e)
		{
			if (this.client.itsConnect)
			{
				this.client.Send(new object[]
				{
					"Set",
					this.richTextBox1.Text
				});
			}
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x00036680 File Offset: 0x00034880
		private void rjButton3_Click(object sender, EventArgs e)
		{
			if (this.client.itsConnect)
			{
				this.client.Send(new object[]
				{
					"Clear"
				});
			}
		}

		// Token: 0x040002AC RID: 684
		public Clients client;

		// Token: 0x040002AD RID: 685
		public Clients parrent;
	}
}
