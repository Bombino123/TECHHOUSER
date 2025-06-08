using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;
using WinFormAnimation;

namespace Server.Forms
{
	// Token: 0x02000098 RID: 152
	public partial class FormPerformance : FormMaterial
	{
		// Token: 0x06000419 RID: 1049 RVA: 0x00032603 File Offset: 0x00030803
		public FormPerformance()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x00032623 File Offset: 0x00030823
		private void FormPerformance_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x0003264D File Offset: 0x0003084D
		private void ChangeScheme(object sender)
		{
			this.circularProgressBar1.ProgressColor = FormMaterial.PrimaryColor;
			this.circularProgressBar2.ProgressColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x0003266F File Offset: 0x0003086F
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x00032684 File Offset: 0x00030884
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

		// Token: 0x04000268 RID: 616
		public Clients client;

		// Token: 0x04000269 RID: 617
		public Clients parrent;
	}
}
