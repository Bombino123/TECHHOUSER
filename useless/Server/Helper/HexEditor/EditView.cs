using System;
using System.Drawing;
using System.Windows.Forms;

namespace Server.Helper.HexEditor
{
	// Token: 0x0200008B RID: 139
	public class EditView : IKeyMouseEventHandler
	{
		// Token: 0x06000340 RID: 832 RVA: 0x0002692F File Offset: 0x00024B2F
		public EditView(HexEditor editor)
		{
			this._editor = editor;
			this._hexView = new HexViewHandler(editor);
			this._stringView = new StringViewHandler(editor);
		}

		// Token: 0x06000341 RID: 833 RVA: 0x00026956 File Offset: 0x00024B56
		public void OnKeyPress(KeyPressEventArgs e)
		{
			if (this.InHexView(this._editor.CaretPosX))
			{
				this._hexView.OnKeyPress(e);
				return;
			}
			this._stringView.OnKeyPress(e);
		}

		// Token: 0x06000342 RID: 834 RVA: 0x00026984 File Offset: 0x00024B84
		public void OnKeyDown(KeyEventArgs e)
		{
			if (this.InHexView(this._editor.CaretPosX))
			{
				this._hexView.OnKeyDown(e);
				return;
			}
			this._stringView.OnKeyDown(e);
		}

		// Token: 0x06000343 RID: 835 RVA: 0x000269B2 File Offset: 0x00024BB2
		public void OnKeyUp(KeyEventArgs e)
		{
		}

		// Token: 0x06000344 RID: 836 RVA: 0x000269B4 File Offset: 0x00024BB4
		public void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (this.InHexView(e.X))
				{
					this._hexView.OnMouseDown(e.X, e.Y);
					return;
				}
				this._stringView.OnMouseDown(e.X, e.Y);
			}
		}

		// Token: 0x06000345 RID: 837 RVA: 0x00026A0C File Offset: 0x00024C0C
		public void OnMouseDragged(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (this.InHexView(e.X))
				{
					this._hexView.OnMouseDragged(e.X, e.Y);
					return;
				}
				this._stringView.OnMouseDragged(e.X, e.Y);
			}
		}

		// Token: 0x06000346 RID: 838 RVA: 0x00026A63 File Offset: 0x00024C63
		public void OnMouseUp(MouseEventArgs e)
		{
		}

		// Token: 0x06000347 RID: 839 RVA: 0x00026A65 File Offset: 0x00024C65
		public void OnMouseDoubleClick(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (this.InHexView(e.X))
				{
					this._hexView.OnMouseDoubleClick();
					return;
				}
				this._stringView.OnMouseDoubleClick();
			}
		}

		// Token: 0x06000348 RID: 840 RVA: 0x00026A99 File Offset: 0x00024C99
		public void OnGotFocus(EventArgs e)
		{
			if (this.InHexView(this._editor.CaretPosX))
			{
				this._hexView.Focus();
				return;
			}
			this._stringView.Focus();
		}

		// Token: 0x06000349 RID: 841 RVA: 0x00026AC5 File Offset: 0x00024CC5
		public void SetLowerCase()
		{
			this._hexView.SetLowerCase();
		}

		// Token: 0x0600034A RID: 842 RVA: 0x00026AD2 File Offset: 0x00024CD2
		public void SetUpperCase()
		{
			this._hexView.SetUpperCase();
		}

		// Token: 0x0600034B RID: 843 RVA: 0x00026ADF File Offset: 0x00024CDF
		public void Update(int startPositionX, Rectangle area)
		{
			this._hexView.Update(startPositionX, area);
			this._stringView.Update(this._hexView.MaxWidth, area);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x00026B08 File Offset: 0x00024D08
		public void Paint(Graphics g, int startIndex, int endIndex)
		{
			int num = 0;
			while (num + startIndex < endIndex)
			{
				this._hexView.Paint(g, num, startIndex);
				this._stringView.Paint(g, num, startIndex);
				num++;
			}
		}

		// Token: 0x0600034D RID: 845 RVA: 0x00026B3F File Offset: 0x00024D3F
		private bool InHexView(int x)
		{
			return x < this._hexView.MaxWidth + this._editor.EntityMargin - 2;
		}

		// Token: 0x040001D9 RID: 473
		private HexViewHandler _hexView;

		// Token: 0x040001DA RID: 474
		private StringViewHandler _stringView;

		// Token: 0x040001DB RID: 475
		private HexEditor _editor;
	}
}
