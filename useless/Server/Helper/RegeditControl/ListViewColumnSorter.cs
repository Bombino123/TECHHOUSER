using System;
using System.Collections;
using System.Windows.Forms;

namespace Server.Helper.RegeditControl
{
	// Token: 0x02000088 RID: 136
	public class ListViewColumnSorter : IComparer
	{
		// Token: 0x06000316 RID: 790 RVA: 0x00026448 File Offset: 0x00024648
		public ListViewColumnSorter()
		{
			this.ColumnToSort = 0;
			this.OrderOfSort = SortOrder.None;
			this.ObjectCompare = new CaseInsensitiveComparer();
		}

		// Token: 0x06000317 RID: 791 RVA: 0x0002646C File Offset: 0x0002466C
		public int Compare(object x, object y)
		{
			ListViewItem listViewItem = (ListViewItem)x;
			ListViewItem listViewItem2 = (ListViewItem)y;
			int num = this.ObjectCompare.Compare(listViewItem.SubItems[this.ColumnToSort].Text, listViewItem2.SubItems[this.ColumnToSort].Text);
			if (this.OrderOfSort == SortOrder.Ascending)
			{
				return num;
			}
			if (this.OrderOfSort == SortOrder.Descending)
			{
				return -num;
			}
			return 0;
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000319 RID: 793 RVA: 0x000264E0 File Offset: 0x000246E0
		// (set) Token: 0x06000318 RID: 792 RVA: 0x000264D7 File Offset: 0x000246D7
		public int SortColumn
		{
			get
			{
				return this.ColumnToSort;
			}
			set
			{
				this.ColumnToSort = value;
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600031B RID: 795 RVA: 0x000264F1 File Offset: 0x000246F1
		// (set) Token: 0x0600031A RID: 794 RVA: 0x000264E8 File Offset: 0x000246E8
		public SortOrder Order
		{
			get
			{
				return this.OrderOfSort;
			}
			set
			{
				this.OrderOfSort = value;
			}
		}

		// Token: 0x040001CD RID: 461
		private int ColumnToSort;

		// Token: 0x040001CE RID: 462
		private SortOrder OrderOfSort;

		// Token: 0x040001CF RID: 463
		private CaseInsensitiveComparer ObjectCompare;
	}
}
