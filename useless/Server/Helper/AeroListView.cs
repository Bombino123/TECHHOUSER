using System;
using System.Windows.Forms;
using Server.Helper.RegeditControl;

namespace Server.Helper
{
	// Token: 0x02000076 RID: 118
	public class AeroListView : ListView
	{
		// Token: 0x0600028B RID: 651 RVA: 0x00023E5E File Offset: 0x0002205E
		public static int MakeWin32Long(short wLow, short wHigh)
		{
			return (int)wLow << 16 | (int)wHigh;
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600028C RID: 652 RVA: 0x00023E66 File Offset: 0x00022066
		// (set) Token: 0x0600028D RID: 653 RVA: 0x00023E6E File Offset: 0x0002206E
		private ListViewColumnSorter LvwColumnSorter { get; set; }

		// Token: 0x0600028E RID: 654 RVA: 0x00023E78 File Offset: 0x00022078
		public AeroListView()
		{
			base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
			this.LvwColumnSorter = new ListViewColumnSorter();
			base.ListViewItemSorter = this.LvwColumnSorter;
			base.View = View.Details;
			base.FullRowSelect = true;
		}

		// Token: 0x0600028F RID: 655 RVA: 0x00023ED0 File Offset: 0x000220D0
		protected override void OnColumnClick(ColumnClickEventArgs e)
		{
			base.OnColumnClick(e);
			if (e.Column == this.LvwColumnSorter.SortColumn)
			{
				this.LvwColumnSorter.Order = ((this.LvwColumnSorter.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending);
			}
			else
			{
				this.LvwColumnSorter.SortColumn = e.Column;
				this.LvwColumnSorter.Order = SortOrder.Ascending;
			}
			if (!base.VirtualMode)
			{
				base.Sort();
			}
		}

		// Token: 0x04000179 RID: 377
		private const uint WM_CHANGEUISTATE = 295U;

		// Token: 0x0400017A RID: 378
		private const short UIS_SET = 1;

		// Token: 0x0400017B RID: 379
		private const short UISF_HIDEFOCUS = 1;

		// Token: 0x0400017C RID: 380
		private readonly IntPtr _removeDots = new IntPtr(AeroListView.MakeWin32Long(1, 1));
	}
}
