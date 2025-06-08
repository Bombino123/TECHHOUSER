using System;
using System.Windows.Forms;

namespace Server.Helper
{
	// Token: 0x0200007B RID: 123
	public class RegistryTreeView : TreeView
	{
		// Token: 0x060002AB RID: 683 RVA: 0x00024818 File Offset: 0x00022A18
		public RegistryTreeView()
		{
			base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
		}
	}
}
