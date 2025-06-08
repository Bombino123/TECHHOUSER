using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Server.Helper.HexEditor
{
	// Token: 0x0200008A RID: 138
	public class Caret
	{
		// Token: 0x17000040 RID: 64
		// (get) Token: 0x06000328 RID: 808 RVA: 0x000265B3 File Offset: 0x000247B3
		public int SelectionStart
		{
			get
			{
				if (this._endIndex < this._startIndex)
				{
					return this._endIndex;
				}
				return this._startIndex;
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x06000329 RID: 809 RVA: 0x000265D0 File Offset: 0x000247D0
		public int SelectionLength
		{
			get
			{
				if (this._endIndex < this._startIndex)
				{
					return this._startIndex - this._endIndex;
				}
				return this._endIndex - this._startIndex;
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x0600032A RID: 810 RVA: 0x000265FB File Offset: 0x000247FB
		public bool Focused
		{
			get
			{
				return this._isCaretActive;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x0600032B RID: 811 RVA: 0x00026603 File Offset: 0x00024803
		public int CurrentIndex
		{
			get
			{
				return this._endIndex;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x0600032C RID: 812 RVA: 0x0002660B File Offset: 0x0002480B
		public Point Location
		{
			get
			{
				return this._location;
			}
		}

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x0600032D RID: 813 RVA: 0x00026614 File Offset: 0x00024814
		// (remove) Token: 0x0600032E RID: 814 RVA: 0x0002664C File Offset: 0x0002484C
		public event EventHandler SelectionStartChanged;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x0600032F RID: 815 RVA: 0x00026684 File Offset: 0x00024884
		// (remove) Token: 0x06000330 RID: 816 RVA: 0x000266BC File Offset: 0x000248BC
		public event EventHandler SelectionLengthChanged;

		// Token: 0x06000331 RID: 817 RVA: 0x000266F1 File Offset: 0x000248F1
		public Caret(HexEditor editor)
		{
			this._editor = editor;
			this._isCaretActive = false;
			this._startIndex = 0;
			this._endIndex = 0;
			this._isCaretHidden = true;
			this._location = new Point(0, 0);
		}

		// Token: 0x06000332 RID: 818 RVA: 0x0002672C File Offset: 0x0002492C
		private bool Create(IntPtr hWHandler)
		{
			if (!this._isCaretActive)
			{
				this._isCaretActive = true;
				return Caret.CreateCaret(hWHandler, IntPtr.Zero, 0, (int)this._editor.CharSize.Height - 2);
			}
			return false;
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0002676C File Offset: 0x0002496C
		private bool Show(IntPtr hWnd)
		{
			if (this._isCaretActive)
			{
				this._isCaretHidden = false;
				return Caret.ShowCaret(hWnd);
			}
			return false;
		}

		// Token: 0x06000334 RID: 820 RVA: 0x00026785 File Offset: 0x00024985
		public bool Hide(IntPtr hWnd)
		{
			if (this._isCaretActive && !this._isCaretHidden)
			{
				this._isCaretHidden = true;
				return Caret.HideCaret(hWnd);
			}
			return false;
		}

		// Token: 0x06000335 RID: 821 RVA: 0x000267A6 File Offset: 0x000249A6
		public bool Destroy()
		{
			if (this._isCaretActive)
			{
				this._isCaretActive = false;
				this.DeSelect();
				Caret.DestroyCaret();
			}
			return false;
		}

		// Token: 0x06000336 RID: 822 RVA: 0x000267C4 File Offset: 0x000249C4
		public void SetStartIndex(int index)
		{
			this._startIndex = index;
			this._endIndex = this._startIndex;
			if (this.SelectionStartChanged != null)
			{
				this.SelectionStartChanged(this, EventArgs.Empty);
			}
			if (this.SelectionLengthChanged != null)
			{
				this.SelectionLengthChanged(this, EventArgs.Empty);
			}
		}

		// Token: 0x06000337 RID: 823 RVA: 0x00026816 File Offset: 0x00024A16
		public void SetEndIndex(int index)
		{
			this._endIndex = index;
			if (this.SelectionStartChanged != null)
			{
				this.SelectionStartChanged(this, EventArgs.Empty);
			}
			if (this.SelectionLengthChanged != null)
			{
				this.SelectionLengthChanged(this, EventArgs.Empty);
			}
		}

		// Token: 0x06000338 RID: 824 RVA: 0x00026854 File Offset: 0x00024A54
		public void SetCaretLocation(Point start)
		{
			this.Create(this._editor.Handle);
			this._location = start;
			Caret.SetCaretPos(this._location.X, this._location.Y);
			this.Show(this._editor.Handle);
		}

		// Token: 0x06000339 RID: 825 RVA: 0x000268A8 File Offset: 0x00024AA8
		public bool IsSelected(int byteIndex)
		{
			return this.SelectionStart <= byteIndex && byteIndex < this.SelectionStart + this.SelectionLength;
		}

		// Token: 0x0600033A RID: 826 RVA: 0x000268C8 File Offset: 0x00024AC8
		private void DeSelect()
		{
			if (this._endIndex < this._startIndex)
			{
				this._startIndex = this._endIndex;
			}
			else
			{
				this._endIndex = this._startIndex;
			}
			if (this.SelectionStartChanged != null)
			{
				this.SelectionStartChanged(this, EventArgs.Empty);
			}
			if (this.SelectionLengthChanged != null)
			{
				this.SelectionLengthChanged(this, EventArgs.Empty);
			}
		}

		// Token: 0x0600033B RID: 827
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

		// Token: 0x0600033C RID: 828
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyCaret();

		// Token: 0x0600033D RID: 829
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool SetCaretPos(int x, int y);

		// Token: 0x0600033E RID: 830
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool ShowCaret(IntPtr hWnd);

		// Token: 0x0600033F RID: 831
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool HideCaret(IntPtr hWnd);

		// Token: 0x040001D1 RID: 465
		private int _startIndex;

		// Token: 0x040001D2 RID: 466
		private int _endIndex;

		// Token: 0x040001D3 RID: 467
		private bool _isCaretActive;

		// Token: 0x040001D4 RID: 468
		private bool _isCaretHidden;

		// Token: 0x040001D5 RID: 469
		private Point _location;

		// Token: 0x040001D6 RID: 470
		private HexEditor _editor;
	}
}
