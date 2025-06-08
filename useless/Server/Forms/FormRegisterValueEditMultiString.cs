using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000B8 RID: 184
	public partial class FormRegisterValueEditMultiString : FormMaterial
	{
		// Token: 0x060005F5 RID: 1525 RVA: 0x00056634 File Offset: 0x00054834
		public FormRegisterValueEditMultiString(RegistrySeeker.RegValueData value)
		{
			this._value = value;
			this.InitializeComponent();
			this.valueNameTxtBox.Text = value.Name;
			this.valueDataTxtBox.Text = string.Join("\r\n", Server.Helper.ByteConverter.ToStringArray(value.Data));
		}

		// Token: 0x060005F6 RID: 1526 RVA: 0x00056685 File Offset: 0x00054885
		private void FormRegisterValueEditString_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x000566A4 File Offset: 0x000548A4
		private void ChangeScheme(object sender)
		{
			this.valueDataTxtBox.BackColor = FormMaterial.PrimaryColor;
			this.valueNameTxtBox.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton4.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060005F8 RID: 1528 RVA: 0x000566F4 File Offset: 0x000548F4
		private void rjButton1_Click(object sender, EventArgs e)
		{
			this._value.Data = Server.Helper.ByteConverter.GetBytes(this.valueDataTxtBox.Text.Split(new string[]
			{
				"\r\n"
			}, StringSplitOptions.RemoveEmptyEntries));
			base.Tag = this._value;
			base.DialogResult = DialogResult.OK;
			base.Close();
		}

		// Token: 0x060005F9 RID: 1529 RVA: 0x00056749 File Offset: 0x00054949
		private void rjButton4_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x040004BF RID: 1215
		private readonly RegistrySeeker.RegValueData _value;
	}
}
