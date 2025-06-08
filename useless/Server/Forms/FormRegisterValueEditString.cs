using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000BB RID: 187
	public partial class FormRegisterValueEditString : FormMaterial
	{
		// Token: 0x0600060D RID: 1549 RVA: 0x00057D88 File Offset: 0x00055F88
		public FormRegisterValueEditString(RegistrySeeker.RegValueData value)
		{
			this._value = value;
			this.InitializeComponent();
			this.valueNameTxtBox.Text = RegValueHelper.GetName(value.Name);
			this.valueDataTxtBox.Text = Server.Helper.ByteConverter.ToString(value.Data);
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x00057DD4 File Offset: 0x00055FD4
		private void FormRegisterValueEditString_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x00057DF4 File Offset: 0x00055FF4
		private void ChangeScheme(object sender)
		{
			this.valueDataTxtBox.BackColor = FormMaterial.PrimaryColor;
			this.valueNameTxtBox.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton4.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x00057E41 File Offset: 0x00056041
		private void rjButton1_Click(object sender, EventArgs e)
		{
			this._value.Data = Server.Helper.ByteConverter.GetBytes(this.valueDataTxtBox.Text);
			base.Tag = this._value;
			base.DialogResult = DialogResult.OK;
			base.Close();
		}

		// Token: 0x06000611 RID: 1553 RVA: 0x00057E77 File Offset: 0x00056077
		private void rjButton4_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x040004D7 RID: 1239
		private readonly RegistrySeeker.RegValueData _value;
	}
}
