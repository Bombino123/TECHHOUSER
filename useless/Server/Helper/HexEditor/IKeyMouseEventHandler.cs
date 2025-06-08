using System;
using System.Windows.Forms;

namespace Server.Helper.HexEditor
{
	// Token: 0x0200008E RID: 142
	public interface IKeyMouseEventHandler
	{
		// Token: 0x060003B8 RID: 952
		void OnKeyPress(KeyPressEventArgs e);

		// Token: 0x060003B9 RID: 953
		void OnKeyDown(KeyEventArgs e);

		// Token: 0x060003BA RID: 954
		void OnKeyUp(KeyEventArgs e);

		// Token: 0x060003BB RID: 955
		void OnMouseDown(MouseEventArgs e);

		// Token: 0x060003BC RID: 956
		void OnMouseDragged(MouseEventArgs e);

		// Token: 0x060003BD RID: 957
		void OnMouseUp(MouseEventArgs e);

		// Token: 0x060003BE RID: 958
		void OnMouseDoubleClick(MouseEventArgs e);

		// Token: 0x060003BF RID: 959
		void OnGotFocus(EventArgs e);
	}
}
