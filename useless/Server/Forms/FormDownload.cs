using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000BE RID: 190
	public partial class FormDownload : FormMaterial
	{
		// Token: 0x06000626 RID: 1574 RVA: 0x00059E9D File Offset: 0x0005809D
		public FormDownload()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000627 RID: 1575 RVA: 0x00059EBD File Offset: 0x000580BD
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000628 RID: 1576 RVA: 0x00059ED4 File Offset: 0x000580D4
		private void FormUpload_Load(object sender, EventArgs e)
		{
			this.timer1.Start();
			this.timer2.Start();
			this.materialProgressBar1.Maximum = (int)this.SizeFile;
			this.materialLabel3.Text = "File: " + this.NameFile;
			this.materialLabel4.Text = "Size: " + Methods.BytesToString(this.SizeFile);
			Clients clients = this.client;
			clients.Recevied = (Clients.Delegate)Delegate.Combine(clients.Recevied, new Clients.Delegate(delegate(long item)
			{
				Program.form.Invoke(new MethodInvoker(delegate()
				{
					this.Recevied += item;
					this.Secound += item;
					if (this.Recevied > this.SizeFile)
					{
						return;
					}
					this.materialProgressBar1.Value = (int)this.Recevied;
					this.materialLabel5.Text = "Size Recive: " + Methods.BytesToString(this.Recevied);
					this.client.lastPing.Last();
				}));
			}));
		}

		// Token: 0x06000629 RID: 1577 RVA: 0x00059F6C File Offset: 0x0005816C
		private void timer2_Tick(object sender, EventArgs e)
		{
			this.materialLabel2.Text = Methods.BytesToString(this.Secound) + "\\sec";
			this.Secound = 0L;
			this.materialLabel1.Text = FormDownload.GetPercent(this.SizeFile, this.Recevied).ToString() + "% done";
		}

		// Token: 0x0600062A RID: 1578 RVA: 0x00059FCF File Offset: 0x000581CF
		public static long GetPercent(long b, long a)
		{
			if (b == 0L)
			{
				return 0L;
			}
			return (long)(a / (b / 100m));
		}

		// Token: 0x0600062B RID: 1579 RVA: 0x00059FF9 File Offset: 0x000581F9
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

		// Token: 0x040004F9 RID: 1273
		public string NameFile;

		// Token: 0x040004FA RID: 1274
		public string pathto;

		// Token: 0x040004FB RID: 1275
		public long Recevied;

		// Token: 0x040004FC RID: 1276
		public long Secound;

		// Token: 0x040004FD RID: 1277
		public long SizeFile;

		// Token: 0x040004FE RID: 1278
		public Clients client;

		// Token: 0x040004FF RID: 1279
		public Clients parrent;
	}
}
