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
	// Token: 0x020000AB RID: 171
	public partial class FormAutoRunSet : FormMaterial
	{
		// Token: 0x06000547 RID: 1351 RVA: 0x0004C293 File Offset: 0x0004A493
		public FormAutoRunSet()
		{
			this.InitializeComponent();
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x0004C2A1 File Offset: 0x0004A4A1
		private void FormHVNCrun_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x0004C2C0 File Offset: 0x0004A4C0
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjComboBox2.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton2.BackColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x0004C320 File Offset: 0x0004A520
		private void rjButton1_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.rjTextBox1.Texts) || string.IsNullOrEmpty(this.rjTextBox2.Texts) || string.IsNullOrEmpty(this.rjComboBox2.Texts))
			{
				return;
			}
			this.client.Send(new object[]
			{
				"Set",
				this.rjComboBox2.Texts,
				this.rjTextBox2.Texts,
				this.rjTextBox1.Texts
			});
			base.Close();
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x0004C3AD File Offset: 0x0004A5AD
		private void rjButton2_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x040003F6 RID: 1014
		public Clients client;
	}
}
