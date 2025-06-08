using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000AC RID: 172
	public partial class FormHVNCrun : FormMaterial
	{
		// Token: 0x0600054E RID: 1358 RVA: 0x0004CB39 File Offset: 0x0004AD39
		public FormHVNCrun()
		{
			this.InitializeComponent();
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x0004CB47 File Offset: 0x0004AD47
		private void FormHVNCrun_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0004CB68 File Offset: 0x0004AD68
		private void ChangeScheme(object sender)
		{
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton2.BackColor = FormMaterial.PrimaryColor;
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjTextBox2.BorderColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x0004CBB5 File Offset: 0x0004ADB5
		private void rjButton1_Click(object sender, EventArgs e)
		{
			this.Run = true;
		}

		// Token: 0x06000552 RID: 1362 RVA: 0x0004CBBE File Offset: 0x0004ADBE
		private void rjButton2_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x040003FD RID: 1021
		public bool Run;
	}
}
