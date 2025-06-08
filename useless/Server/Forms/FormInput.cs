using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000AD RID: 173
	public partial class FormInput : FormMaterial
	{
		// Token: 0x06000555 RID: 1365 RVA: 0x0004D1C6 File Offset: 0x0004B3C6
		public FormInput()
		{
			this.InitializeComponent();
		}

		// Token: 0x06000556 RID: 1366 RVA: 0x0004D1D4 File Offset: 0x0004B3D4
		private void FormInput_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
			this.rjTextBox1.textBox1.KeyDown += this.rjTextBox1_KeyDown;
			this.rjTextBox1.Focus();
		}

		// Token: 0x06000557 RID: 1367 RVA: 0x0004D226 File Offset: 0x0004B426
		private void rjTextBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.Run = true;
				base.Close();
				e.SuppressKeyPress = true;
			}
			if (e.KeyCode == Keys.Escape)
			{
				base.Close();
				e.SuppressKeyPress = true;
			}
		}

		// Token: 0x06000558 RID: 1368 RVA: 0x0004D25D File Offset: 0x0004B45D
		private void ChangeScheme(object sender)
		{
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton2.BackColor = FormMaterial.PrimaryColor;
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x0004D28F File Offset: 0x0004B48F
		private void rjButton1_Click(object sender, EventArgs e)
		{
			this.Run = true;
			base.Close();
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x0004D29E File Offset: 0x0004B49E
		private void rjButton2_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x04000403 RID: 1027
		public bool Run;
	}
}
