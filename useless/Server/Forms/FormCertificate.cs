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
	// Token: 0x02000094 RID: 148
	public partial class FormCertificate : FormMaterial
	{
		// Token: 0x060003FD RID: 1021 RVA: 0x00031247 File Offset: 0x0002F447
		public FormCertificate()
		{
			this.InitializeComponent();
		}

		// Token: 0x060003FE RID: 1022 RVA: 0x00031255 File Offset: 0x0002F455
		private void FormAbout_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x00031274 File Offset: 0x0002F474
		private void ChangeScheme(object sender)
		{
			this.rjTextBox1.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BorderColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x00031296 File Offset: 0x0002F496
		private void rjButton1_Click(object sender, EventArgs e)
		{
			this.rjButton1.Text = "Wait...";
			Certificate.CreateCertificateAuthority(this.rjTextBox1.Texts);
			base.Close();
		}
	}
}
