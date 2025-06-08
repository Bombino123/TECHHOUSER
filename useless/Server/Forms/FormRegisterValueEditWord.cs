using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CustomControls.RJControls;
using MaterialSkin;
using Microsoft.Win32;
using Server.Helper;

namespace Server.Forms
{
	// Token: 0x020000B9 RID: 185
	public partial class FormRegisterValueEditWord : FormMaterial
	{
		// Token: 0x060005FC RID: 1532 RVA: 0x00056D60 File Offset: 0x00054F60
		public FormRegisterValueEditWord(RegistrySeeker.RegValueData value)
		{
			this._value = value;
			this.InitializeComponent();
			this.valueNameTxtBox.Text = value.Name;
			if (value.Kind == RegistryValueKind.DWord)
			{
				this.Text = "Register Value Edit DWORD (32-bit) Value";
				this.valueDataTxtBox.Type = WordTextBox.WordType.DWORD;
				this.valueDataTxtBox.Text = Server.Helper.ByteConverter.ToUInt32(value.Data).ToString("x");
				return;
			}
			this.Text = "Register Value Edit QWORD (64-bit) Value";
			this.valueDataTxtBox.Type = WordTextBox.WordType.QWORD;
			this.valueDataTxtBox.Text = Server.Helper.ByteConverter.ToUInt64(value.Data).ToString("x");
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x00056E0F File Offset: 0x0005500F
		private void FormRegisterValueEditString_Load(object sender, EventArgs e)
		{
			MaterialSkinManager.Instance.ThemeChanged += this.ChangeScheme;
			this.ChangeScheme(this);
		}

		// Token: 0x060005FE RID: 1534 RVA: 0x00056E30 File Offset: 0x00055030
		private void ChangeScheme(object sender)
		{
			this.valueDataTxtBox.BackColor = FormMaterial.PrimaryColor;
			this.valueNameTxtBox.BorderColor = FormMaterial.PrimaryColor;
			this.rjButton1.BackColor = FormMaterial.PrimaryColor;
			this.rjButton4.BackColor = FormMaterial.PrimaryColor;
			this.baseBox.ForeColor = FormMaterial.PrimaryColor;
			this.radioHexa.ForeColor = FormMaterial.PrimaryColor;
			this.radioDecimal.ForeColor = FormMaterial.PrimaryColor;
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x00056EB0 File Offset: 0x000550B0
		private void rjButton1_Click(object sender, EventArgs e)
		{
			if (this.valueDataTxtBox.IsConversionValid() || this.IsOverridePossible())
			{
				this._value.Data = ((this._value.Kind == RegistryValueKind.DWord) ? Server.Helper.ByteConverter.GetBytes(this.valueDataTxtBox.UIntValue) : Server.Helper.ByteConverter.GetBytes(this.valueDataTxtBox.ULongValue));
				base.Tag = this._value;
				base.DialogResult = DialogResult.OK;
			}
			else
			{
				base.DialogResult = DialogResult.None;
			}
			base.Close();
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x00056F2F File Offset: 0x0005512F
		private DialogResult ShowWarning(string msg, string caption)
		{
			return MessageBox.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x00056F3B File Offset: 0x0005513B
		private bool IsOverridePossible()
		{
			return this.ShowWarning((this._value.Kind == RegistryValueKind.DWord) ? "The decimal value entered is greater than the maximum value of a DWORD (32-bit number). Should the value be truncated in order to continue?" : "The decimal value entered is greater than the maximum value of a QWORD (64-bit number). Should the value be truncated in order to continue?", "Overflow") == DialogResult.Yes;
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x00056F68 File Offset: 0x00055168
		private void radioHexa_CheckedChanged(object sender, EventArgs e)
		{
			if (this.valueDataTxtBox.IsHexNumber == this.radioHexa.Checked)
			{
				return;
			}
			if (this.valueDataTxtBox.IsConversionValid() || this.IsOverridePossible())
			{
				this.valueDataTxtBox.IsHexNumber = this.radioHexa.Checked;
				return;
			}
			this.radioDecimal.Checked = true;
		}

		// Token: 0x06000603 RID: 1539 RVA: 0x00056FC6 File Offset: 0x000551C6
		private void rjButton4_Click(object sender, EventArgs e)
		{
			base.Close();
		}

		// Token: 0x040004C5 RID: 1221
		private readonly RegistrySeeker.RegValueData _value;

		// Token: 0x040004C6 RID: 1222
		private const string DWORD_WARNING = "The decimal value entered is greater than the maximum value of a DWORD (32-bit number). Should the value be truncated in order to continue?";

		// Token: 0x040004C7 RID: 1223
		private const string QWORD_WARNING = "The decimal value entered is greater than the maximum value of a QWORD (64-bit number). Should the value be truncated in order to continue?";
	}
}
