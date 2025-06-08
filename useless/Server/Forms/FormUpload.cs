using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Server.Connectings;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000BF RID: 191
	public partial class FormUpload : FormMaterial
	{
		// Token: 0x0600062F RID: 1583 RVA: 0x0005A59C File Offset: 0x0005879C
		public FormUpload()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0005A5BC File Offset: 0x000587BC
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x0005A5D4 File Offset: 0x000587D4
		private void FormUpload_Load(object sender, EventArgs e)
		{
			this.timer1.Start();
			this.bytes = File.ReadAllBytes(this.path);
			this.materialProgressBar1.Maximum = this.bytes.Length;
			this.materialLabel3.Text = "File: " + Path.GetFileName(this.path);
			this.materialLabel4.Text = "Size: " + Methods.BytesToString((long)this.bytes.Length);
		}

		// Token: 0x06000632 RID: 1586 RVA: 0x0005A652 File Offset: 0x00058852
		public void Connected()
		{
			this.timer2.Start();
			Clients clients = this.client;
			clients.Sents = (Clients.Delegate)Delegate.Combine(clients.Sents, new Clients.Delegate(delegate(long item)
			{
				Program.form.Invoke(new MethodInvoker(delegate()
				{
					this.sends += item;
					this.Secound += item;
					if (this.sends > (long)this.bytes.Length)
					{
						return;
					}
					this.materialProgressBar1.Value = (int)this.sends;
					this.materialLabel5.Text = "Size Sents: " + Methods.BytesToString(this.sends);
					this.client.lastPing.Last();
				}));
			}));
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x0005A688 File Offset: 0x00058888
		private void timer2_Tick(object sender, EventArgs e)
		{
			this.materialLabel2.Text = Methods.BytesToString(this.Secound) + "\\sec";
			this.Secound = 0L;
			this.materialLabel1.Text = FormUpload.GetPercent((long)this.bytes.Length, this.sends).ToString() + "% done";
		}

		// Token: 0x06000634 RID: 1588 RVA: 0x0005A6EE File Offset: 0x000588EE
		public static long GetPercent(long b, long a)
		{
			if (b == 0L)
			{
				return 0L;
			}
			return (long)(a / (b / 100m));
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x0005A718 File Offset: 0x00058918
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

		// Token: 0x04000509 RID: 1289
		public string path;

		// Token: 0x0400050A RID: 1290
		public string pathto;

		// Token: 0x0400050B RID: 1291
		public byte[] bytes;

		// Token: 0x0400050C RID: 1292
		public long sends;

		// Token: 0x0400050D RID: 1293
		public long Secound;

		// Token: 0x0400050E RID: 1294
		public Clients client;

		// Token: 0x0400050F RID: 1295
		public Clients parrent;
	}
}
