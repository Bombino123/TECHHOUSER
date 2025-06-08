using System;
using System.Windows.Forms;

namespace Server.Helper
{
	// Token: 0x0200007C RID: 124
	public class RegistryValueLstItem : ListViewItem
	{
		// Token: 0x17000027 RID: 39
		// (get) Token: 0x060002AC RID: 684 RVA: 0x0002482C File Offset: 0x00022A2C
		// (set) Token: 0x060002AD RID: 685 RVA: 0x00024834 File Offset: 0x00022A34
		private string _type { get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x060002AE RID: 686 RVA: 0x0002483D File Offset: 0x00022A3D
		// (set) Token: 0x060002AF RID: 687 RVA: 0x00024845 File Offset: 0x00022A45
		private string _data { get; set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x060002B0 RID: 688 RVA: 0x0002484E File Offset: 0x00022A4E
		// (set) Token: 0x060002B1 RID: 689 RVA: 0x00024856 File Offset: 0x00022A56
		public string RegName
		{
			get
			{
				return base.Name;
			}
			set
			{
				base.Name = value;
				base.Text = RegValueHelper.GetName(value);
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x060002B2 RID: 690 RVA: 0x0002486B File Offset: 0x00022A6B
		// (set) Token: 0x060002B3 RID: 691 RVA: 0x00024874 File Offset: 0x00022A74
		public string Type
		{
			get
			{
				return this._type;
			}
			set
			{
				this._type = value;
				if (base.SubItems.Count < 2)
				{
					base.SubItems.Add(this._type);
				}
				else
				{
					base.SubItems[1].Text = this._type;
				}
				base.ImageIndex = this.GetRegistryValueImgIndex(this._type);
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x060002B4 RID: 692 RVA: 0x000248D3 File Offset: 0x00022AD3
		// (set) Token: 0x060002B5 RID: 693 RVA: 0x000248DC File Offset: 0x00022ADC
		public string Data
		{
			get
			{
				return this._data;
			}
			set
			{
				this._data = value;
				if (base.SubItems.Count < 3)
				{
					base.SubItems.Add(this._data);
					return;
				}
				base.SubItems[2].Text = this._data;
			}
		}

		// Token: 0x060002B6 RID: 694 RVA: 0x00024928 File Offset: 0x00022B28
		public RegistryValueLstItem(RegistrySeeker.RegValueData value)
		{
			this.RegName = value.Name;
			this.Type = value.Kind.RegistryTypeToString();
			this.Data = RegValueHelper.RegistryValueToString(value);
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0002495C File Offset: 0x00022B5C
		private int GetRegistryValueImgIndex(string type)
		{
			if (!(type == "REG_MULTI_SZ") && !(type == "REG_SZ") && !(type == "REG_EXPAND_SZ"))
			{
				if (!(type == "REG_BINARY") && !(type == "REG_DWORD") && !(type == "REG_QWORD"))
				{
				}
				return 1;
			}
			return 0;
		}
	}
}
