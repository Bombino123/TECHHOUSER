using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Server.Helper;
using Server.Helper.HexEditor;

namespace Server.Forms
{
	// Token: 0x020000BA RID: 186
	public partial class FormRegisterValueEditBinary : FormMaterial
	{
		// Token: 0x06000606 RID: 1542 RVA: 0x00057734 File Offset: 0x00055934
		public FormRegisterValueEditBinary(RegistrySeeker.RegValueData value)
		{
			this._value = value;
			this.InitializeComponent();
		}

		// Token: 0x06000607 RID: 1543 RVA: 0x00057749 File Offset: 0x00055949
		private void FormRegisterValueEditString_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x00057768 File Offset: 0x00055968
		private void ChangeScheme(object sender)
		{
			this.hexEditor.ForeColor = FormMaterial.PrimaryColor;
			this.valueNameTxtBox.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton4.BackColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x06000609 RID: 1545 RVA: 0x000577B8 File Offset: 0x000559B8
		private void rjButton1_Click(object sender, EventArgs e)
		{
			byte[] hexTable = this.hexEditor.HexTable;
			if (hexTable != null)
			{
				try
				{
					this._value.Data = hexTable;
					base.DialogResult = DialogResult.OK;
					base.Tag = this._value;
				}
				catch
				{
					MessageBox.Show("The binary value was invalid and could not be converted correctly.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					base.DialogResult = DialogResult.None;
				}
			}
			base.Close();
		}

		// Token: 0x0600060A RID: 1546 RVA: 0x00057828 File Offset: 0x00055A28
		private void rjButton4_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x040004D0 RID: 1232
		private readonly RegistrySeeker.RegValueData _value;

		// Token: 0x040004D1 RID: 1233
		private const string INVALID_BINARY_ERROR = "The binary value was invalid and could not be converted correctly.";
	}
}
