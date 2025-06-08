using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace Server.Helper
{
	// Token: 0x0200007D RID: 125
	public class WordTextBox : TextBox
	{
		// Token: 0x1700002C RID: 44
		// (get) Token: 0x060002B8 RID: 696 RVA: 0x000249BC File Offset: 0x00022BBC
		// (set) Token: 0x060002B9 RID: 697 RVA: 0x000249C4 File Offset: 0x00022BC4
		public override int MaxLength
		{
			get
			{
				return base.MaxLength;
			}
			set
			{
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060002BA RID: 698 RVA: 0x000249C6 File Offset: 0x00022BC6
		// (set) Token: 0x060002BB RID: 699 RVA: 0x000249D0 File Offset: 0x00022BD0
		public bool IsHexNumber
		{
			get
			{
				return this.isHexNumber;
			}
			set
			{
				if (this.isHexNumber == value)
				{
					return;
				}
				if (value)
				{
					if (this.Type == WordTextBox.WordType.DWORD)
					{
						this.Text = this.UIntValue.ToString("x");
					}
					else
					{
						this.Text = this.ULongValue.ToString("x");
					}
				}
				else if (this.Type == WordTextBox.WordType.DWORD)
				{
					this.Text = this.UIntValue.ToString();
				}
				else
				{
					this.Text = this.ULongValue.ToString();
				}
				this.isHexNumber = value;
				this.UpdateMaxLength();
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x060002BC RID: 700 RVA: 0x00024A67 File Offset: 0x00022C67
		// (set) Token: 0x060002BD RID: 701 RVA: 0x00024A6F File Offset: 0x00022C6F
		public WordTextBox.WordType Type
		{
			get
			{
				return this.type;
			}
			set
			{
				if (this.type == value)
				{
					return;
				}
				this.type = value;
				this.UpdateMaxLength();
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060002BE RID: 702 RVA: 0x00024A88 File Offset: 0x00022C88
		public uint UIntValue
		{
			get
			{
				uint result;
				try
				{
					if (string.IsNullOrEmpty(this.Text))
					{
						result = 0U;
					}
					else if (this.IsHexNumber)
					{
						result = uint.Parse(this.Text, NumberStyles.HexNumber);
					}
					else
					{
						result = uint.Parse(this.Text);
					}
				}
				catch (Exception)
				{
					result = uint.MaxValue;
				}
				return result;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x060002BF RID: 703 RVA: 0x00024AE8 File Offset: 0x00022CE8
		public ulong ULongValue
		{
			get
			{
				ulong result;
				try
				{
					if (string.IsNullOrEmpty(this.Text))
					{
						result = 0UL;
					}
					else if (this.IsHexNumber)
					{
						result = ulong.Parse(this.Text, NumberStyles.HexNumber);
					}
					else
					{
						result = ulong.Parse(this.Text);
					}
				}
				catch (Exception)
				{
					result = ulong.MaxValue;
				}
				return result;
			}
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x00024B48 File Offset: 0x00022D48
		public bool IsConversionValid()
		{
			return string.IsNullOrEmpty(this.Text) || this.IsHexNumber || this.ConvertToHex();
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x00024B69 File Offset: 0x00022D69
		public WordTextBox()
		{
			this.InitializeComponent();
			base.MaxLength = 8;
		}

		// Token: 0x060002C2 RID: 706 RVA: 0x00024B7E File Offset: 0x00022D7E
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
			e.Handled = !this.IsValidChar(e.KeyChar);
		}

		// Token: 0x060002C3 RID: 707 RVA: 0x00024B9C File Offset: 0x00022D9C
		private bool IsValidChar(char ch)
		{
			return char.IsControl(ch) || char.IsDigit(ch) || (this.IsHexNumber && char.IsLetter(ch) && char.ToLower(ch) <= 'f');
		}

		// Token: 0x060002C4 RID: 708 RVA: 0x00024BCF File Offset: 0x00022DCF
		private void UpdateMaxLength()
		{
			if (this.Type == WordTextBox.WordType.DWORD)
			{
				if (this.IsHexNumber)
				{
					base.MaxLength = 8;
					return;
				}
				base.MaxLength = 10;
				return;
			}
			else
			{
				if (this.IsHexNumber)
				{
					base.MaxLength = 16;
					return;
				}
				base.MaxLength = 20;
				return;
			}
		}

		// Token: 0x060002C5 RID: 709 RVA: 0x00024C0C File Offset: 0x00022E0C
		private bool ConvertToHex()
		{
			bool result;
			try
			{
				if (this.Type == WordTextBox.WordType.DWORD)
				{
					uint.Parse(this.Text);
				}
				else
				{
					ulong.Parse(this.Text);
				}
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x060002C6 RID: 710 RVA: 0x00024C58 File Offset: 0x00022E58
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00024C77 File Offset: 0x00022E77
		private void InitializeComponent()
		{
			this.components = new Container();
		}

		// Token: 0x04000183 RID: 387
		private bool isHexNumber;

		// Token: 0x04000184 RID: 388
		private WordTextBox.WordType type;

		// Token: 0x04000185 RID: 389
		private IContainer components;

		// Token: 0x0200021F RID: 543
		public enum WordType
		{
			// Token: 0x0400081B RID: 2075
			DWORD,
			// Token: 0x0400081C RID: 2076
			QWORD
		}
	}
}
