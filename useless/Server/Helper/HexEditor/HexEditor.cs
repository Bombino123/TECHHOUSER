using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Server.Helper.HexEditor
{
	// Token: 0x0200008C RID: 140
	public class HexEditor : Control
	{
		// Token: 0x17000045 RID: 69
		// (set) Token: 0x0600034E RID: 846 RVA: 0x00026B5D File Offset: 0x00024D5D
		public override Font Font
		{
			set
			{
				base.Font = value;
				this.UpdateRectanglePositioning();
				base.Invalidate();
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x0600034F RID: 847 RVA: 0x00026B72 File Offset: 0x00024D72
		// (set) Token: 0x06000350 RID: 848 RVA: 0x00026B7A File Offset: 0x00024D7A
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				base.Text = value;
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x06000351 RID: 849 RVA: 0x00026B84 File Offset: 0x00024D84
		// (set) Token: 0x06000352 RID: 850 RVA: 0x00026BCC File Offset: 0x00024DCC
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public byte[] HexTable
		{
			get
			{
				object hexTableLock = this._hexTableLock;
				byte[] result;
				lock (hexTableLock)
				{
					result = this._hexTable.ToArray();
				}
				return result;
			}
			set
			{
				object hexTableLock = this._hexTableLock;
				lock (hexTableLock)
				{
					if (value == this._hexTable.ToArray())
					{
						return;
					}
					this._hexTable = new ByteCollection(value);
				}
				this.UpdateRectanglePositioning();
				base.Invalidate();
			}
		}

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000353 RID: 851 RVA: 0x00026C30 File Offset: 0x00024E30
		// (set) Token: 0x06000354 RID: 852 RVA: 0x00026C38 File Offset: 0x00024E38
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public SizeF CharSize
		{
			get
			{
				return this._charSize;
			}
			private set
			{
				if (this._charSize == value)
				{
					return;
				}
				this._charSize = value;
				if (this.CharSizeChanged != null)
				{
					this.CharSizeChanged(this, EventArgs.Empty);
				}
			}
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000355 RID: 853 RVA: 0x00026C69 File Offset: 0x00024E69
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MaxBytesV
		{
			get
			{
				return this._maxBytesV;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000356 RID: 854 RVA: 0x00026C71 File Offset: 0x00024E71
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int FirstVisibleByte
		{
			get
			{
				return this._firstByte;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x06000357 RID: 855 RVA: 0x00026C79 File Offset: 0x00024E79
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int LastVisibleByte
		{
			get
			{
				return this._lastByte;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000358 RID: 856 RVA: 0x00026C81 File Offset: 0x00024E81
		// (set) Token: 0x06000359 RID: 857 RVA: 0x00026C8C File Offset: 0x00024E8C
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool VScrollBarHidden
		{
			get
			{
				return this._isVScrollHidden;
			}
			set
			{
				if (this._isVScrollHidden == value)
				{
					return;
				}
				this._isVScrollHidden = value;
				if (!this._isVScrollHidden)
				{
					base.Controls.Add(this._vScrollBar);
				}
				else
				{
					base.Controls.Remove(this._vScrollBar);
				}
				this.UpdateRectanglePositioning();
				base.Invalidate();
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600035A RID: 858 RVA: 0x00026CE2 File Offset: 0x00024EE2
		// (set) Token: 0x0600035B RID: 859 RVA: 0x00026CEA File Offset: 0x00024EEA
		[DefaultValue(8)]
		[Category("Hex")]
		[Description("Property that specifies the number of bytes to display per line.")]
		public int BytesPerLine
		{
			get
			{
				return this._bytesPerLine;
			}
			set
			{
				if (this._bytesPerLine == value)
				{
					return;
				}
				this._bytesPerLine = value;
				this.UpdateRectanglePositioning();
				base.Invalidate();
			}
		}

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x0600035C RID: 860 RVA: 0x00026D09 File Offset: 0x00024F09
		// (set) Token: 0x0600035D RID: 861 RVA: 0x00026D11 File Offset: 0x00024F11
		[DefaultValue(10)]
		[Category("Hex")]
		[Description("Property that specifies the margin between each of the entitys in the control.")]
		public int EntityMargin
		{
			get
			{
				return this._entityMargin;
			}
			set
			{
				if (this._entityMargin == value)
				{
					return;
				}
				this._entityMargin = value;
				this.UpdateRectanglePositioning();
				base.Invalidate();
			}
		}

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x0600035E RID: 862 RVA: 0x00026D30 File Offset: 0x00024F30
		// (set) Token: 0x0600035F RID: 863 RVA: 0x00026D38 File Offset: 0x00024F38
		[DefaultValue(BorderStyle.Fixed3D)]
		[Category("Appearance")]
		[Description("Indicates where the control should have a border.")]
		public BorderStyle BorderStyle
		{
			get
			{
				return this._borderStyle;
			}
			set
			{
				if (this._borderStyle == value)
				{
					return;
				}
				if (value != BorderStyle.FixedSingle)
				{
					this._borderColor = Color.Empty;
				}
				this._borderStyle = value;
				this.UpdateRectanglePositioning();
				base.Invalidate();
			}
		}

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000360 RID: 864 RVA: 0x00026D66 File Offset: 0x00024F66
		// (set) Token: 0x06000361 RID: 865 RVA: 0x00026D6E File Offset: 0x00024F6E
		[DefaultValue(typeof(Color), "Empty")]
		[Category("Appearance")]
		[Description("Indicates the color to be used when displaying a FixedSingle border.")]
		public Color BorderColor
		{
			get
			{
				return this._borderColor;
			}
			set
			{
				if (this.BorderStyle != BorderStyle.FixedSingle || this._borderColor == value)
				{
					return;
				}
				this._borderColor = value;
				base.Invalidate();
			}
		}

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000362 RID: 866 RVA: 0x00026D95 File Offset: 0x00024F95
		// (set) Token: 0x06000363 RID: 867 RVA: 0x00026D9D File Offset: 0x00024F9D
		[DefaultValue(typeof(Color), "Blue")]
		[Category("Hex")]
		[Description("Property for the background color of the selected text areas.")]
		public Color SelectionBackColor
		{
			get
			{
				return this._selectionBackColor;
			}
			set
			{
				if (this._selectionBackColor == value)
				{
					return;
				}
				this._selectionBackColor = value;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000364 RID: 868 RVA: 0x00026DB5 File Offset: 0x00024FB5
		// (set) Token: 0x06000365 RID: 869 RVA: 0x00026DBD File Offset: 0x00024FBD
		[DefaultValue(typeof(Color), "White")]
		[Category("Hex")]
		[Description("Property for the foreground color of the selected text areas.")]
		public Color SelectionForeColor
		{
			get
			{
				return this._selectionForeColor;
			}
			set
			{
				if (this._selectionForeColor == value)
				{
					return;
				}
				this._selectionForeColor = value;
			}
		}

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x06000366 RID: 870 RVA: 0x00026DD5 File Offset: 0x00024FD5
		// (set) Token: 0x06000367 RID: 871 RVA: 0x00026DDD File Offset: 0x00024FDD
		[DefaultValue(HexEditor.CaseStyle.UpperCase)]
		[Category("Hex")]
		[Description("Property for the case type to use on the line counter.")]
		public HexEditor.CaseStyle LineCountCaseStyle
		{
			get
			{
				return this._lineCountCaseStyle;
			}
			set
			{
				if (this._lineCountCaseStyle == value)
				{
					return;
				}
				this._lineCountCaseStyle = value;
				if (this._lineCountCaseStyle == HexEditor.CaseStyle.LowerCase)
				{
					this._lineCountCaps = "x";
				}
				else
				{
					this._lineCountCaps = "X";
				}
				base.Invalidate();
			}
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000368 RID: 872 RVA: 0x00026E16 File Offset: 0x00025016
		// (set) Token: 0x06000369 RID: 873 RVA: 0x00026E1E File Offset: 0x0002501E
		[DefaultValue(HexEditor.CaseStyle.UpperCase)]
		[Category("Hex")]
		[Description("Property for the case type to use for the hexadecimal values view.")]
		public HexEditor.CaseStyle HexViewCaseStyle
		{
			get
			{
				return this._hexViewCaseStyle;
			}
			set
			{
				if (this._hexViewCaseStyle == value)
				{
					return;
				}
				this._hexViewCaseStyle = value;
				if (this._hexViewCaseStyle == HexEditor.CaseStyle.LowerCase)
				{
					this._editView.SetLowerCase();
				}
				else
				{
					this._editView.SetUpperCase();
				}
				base.Invalidate();
			}
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x0600036A RID: 874 RVA: 0x00026E57 File Offset: 0x00025057
		// (set) Token: 0x0600036B RID: 875 RVA: 0x00026E5F File Offset: 0x0002505F
		[DefaultValue(false)]
		[Category("Hex")]
		[Description("Property for the visibility of the vertical scrollbar.")]
		public bool VScrollBarVisisble
		{
			get
			{
				return this._isVScrollVisible;
			}
			set
			{
				if (this._isVScrollVisible == value)
				{
					return;
				}
				this._isVScrollVisible = value;
				this.UpdateRectanglePositioning();
				base.Invalidate();
			}
		}

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x0600036C RID: 876 RVA: 0x00026E80 File Offset: 0x00025080
		// (remove) Token: 0x0600036D RID: 877 RVA: 0x00026EB8 File Offset: 0x000250B8
		[Description("Event that is triggered whenever the hextable has been changed.")]
		public event EventHandler HexTableChanged;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x0600036E RID: 878 RVA: 0x00026EF0 File Offset: 0x000250F0
		// (remove) Token: 0x0600036F RID: 879 RVA: 0x00026F28 File Offset: 0x00025128
		[Description("Event that is triggered whenever the SelectionStart value is changed.")]
		public event EventHandler SelectionStartChanged;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x06000370 RID: 880 RVA: 0x00026F60 File Offset: 0x00025160
		// (remove) Token: 0x06000371 RID: 881 RVA: 0x00026F98 File Offset: 0x00025198
		[Description("Event that is triggered whenever the SelectionLength value is changed.")]
		public event EventHandler SelectionLengthChanged;

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x06000372 RID: 882 RVA: 0x00026FD0 File Offset: 0x000251D0
		// (remove) Token: 0x06000373 RID: 883 RVA: 0x00027008 File Offset: 0x00025208
		[Description("Event that is triggered whenever the size of the char is changed.")]
		public event EventHandler CharSizeChanged;

		// Token: 0x06000374 RID: 884 RVA: 0x00027040 File Offset: 0x00025240
		protected void OnVScrollBarScroll(object sender, ScrollEventArgs e)
		{
			switch (e.Type)
			{
			case ScrollEventType.SmallDecrement:
				this.ScrollLineUp(1);
				break;
			case ScrollEventType.SmallIncrement:
				this.ScrollLineDown(1);
				break;
			case ScrollEventType.LargeDecrement:
				this.ScrollLineUp(this._vScrollLarge);
				break;
			case ScrollEventType.LargeIncrement:
				this.ScrollLineDown(this._vScrollLarge);
				break;
			case ScrollEventType.ThumbTrack:
				this.ScrollThumbTrack(e.NewValue - e.OldValue);
				break;
			}
			base.Invalidate();
		}

		// Token: 0x06000375 RID: 885 RVA: 0x000270BB File Offset: 0x000252BB
		protected void CaretSelectionStartChanged(object sender, EventArgs e)
		{
			if (this.SelectionStartChanged != null)
			{
				this.SelectionStartChanged(this, e);
			}
		}

		// Token: 0x06000376 RID: 886 RVA: 0x000270D2 File Offset: 0x000252D2
		protected void CaretSelectionLengthChanged(object sender, EventArgs e)
		{
			if (this.SelectionLengthChanged != null)
			{
				this.SelectionLengthChanged(this, e);
			}
		}

		// Token: 0x06000377 RID: 887 RVA: 0x000270E9 File Offset: 0x000252E9
		protected override void OnMarginChanged(EventArgs e)
		{
			base.OnMarginChanged(e);
			this.UpdateRectanglePositioning();
			base.Invalidate();
		}

		// Token: 0x06000378 RID: 888 RVA: 0x000270FE File Offset: 0x000252FE
		protected override void OnGotFocus(EventArgs e)
		{
			if (this._handler != null)
			{
				this._handler.OnGotFocus(e);
			}
			this.UpdateRectanglePositioning();
			base.Invalidate();
			base.OnGotFocus(e);
		}

		// Token: 0x06000379 RID: 889 RVA: 0x00027127 File Offset: 0x00025327
		protected override void OnLostFocus(EventArgs e)
		{
			this._dragging = false;
			this.DestroyCaret();
			base.OnLostFocus(e);
		}

		// Token: 0x0600037A RID: 890 RVA: 0x0002713D File Offset: 0x0002533D
		protected override bool IsInputKey(Keys keyData)
		{
			return keyData - Keys.Left <= 3 || keyData - (Keys.LButton | Keys.MButton | Keys.Space | Keys.Shift) <= 3 || base.IsInputKey(keyData);
		}

		// Token: 0x0600037B RID: 891 RVA: 0x00027159 File Offset: 0x00025359
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (this._handler != null)
			{
				this._handler.OnKeyPress(e);
			}
			base.OnKeyPress(e);
		}

		// Token: 0x0600037C RID: 892 RVA: 0x00027178 File Offset: 0x00025378
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Next)
			{
				if (!this._isVScrollHidden)
				{
					this.ScrollLineDown(this._vScrollLarge);
					base.Invalidate();
				}
			}
			else if (e.KeyCode == Keys.Prior)
			{
				if (!this._isVScrollHidden)
				{
					this.ScrollLineUp(this._vScrollLarge);
					base.Invalidate();
				}
			}
			else if (this._handler != null)
			{
				this._handler.OnKeyDown(e);
			}
			base.OnKeyDown(e);
		}

		// Token: 0x0600037D RID: 893 RVA: 0x000271EC File Offset: 0x000253EC
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (this._handler != null)
			{
				this._handler.OnKeyUp(e);
			}
			base.OnKeyUp(e);
		}

		// Token: 0x0600037E RID: 894 RVA: 0x0002720C File Offset: 0x0002540C
		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (this.Focused)
			{
				if (this._handler != null)
				{
					this._handler.OnMouseDown(e);
				}
				if (e.Button == MouseButtons.Left)
				{
					this._dragging = true;
					base.Invalidate();
				}
			}
			else
			{
				base.Focus();
			}
			base.OnMouseDown(e);
		}

		// Token: 0x0600037F RID: 895 RVA: 0x0002725F File Offset: 0x0002545F
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (this.Focused && this._dragging)
			{
				if (this._handler != null)
				{
					this._handler.OnMouseDragged(e);
				}
				base.Invalidate();
			}
			base.OnMouseMove(e);
		}

		// Token: 0x06000380 RID: 896 RVA: 0x00027292 File Offset: 0x00025492
		protected override void OnMouseUp(MouseEventArgs e)
		{
			this._dragging = false;
			if (this.Focused && this._handler != null)
			{
				this._handler.OnMouseUp(e);
			}
			base.OnMouseUp(e);
		}

		// Token: 0x06000381 RID: 897 RVA: 0x000272BE File Offset: 0x000254BE
		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			if (this.Focused && this._handler != null)
			{
				this._handler.OnMouseDoubleClick(e);
			}
			base.OnMouseDoubleClick(e);
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x06000382 RID: 898 RVA: 0x000272E4 File Offset: 0x000254E4
		public int CaretPosX
		{
			get
			{
				object caretLock = this._caretLock;
				int x;
				lock (caretLock)
				{
					x = this._caret.Location.X;
				}
				return x;
			}
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x06000383 RID: 899 RVA: 0x00027334 File Offset: 0x00025534
		public int CaretPosY
		{
			get
			{
				object caretLock = this._caretLock;
				int y;
				lock (caretLock)
				{
					y = this._caret.Location.Y;
				}
				return y;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000384 RID: 900 RVA: 0x00027384 File Offset: 0x00025584
		public int SelectionStart
		{
			get
			{
				object caretLock = this._caretLock;
				int selectionStart;
				lock (caretLock)
				{
					selectionStart = this._caret.SelectionStart;
				}
				return selectionStart;
			}
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000385 RID: 901 RVA: 0x000273CC File Offset: 0x000255CC
		public int SelectionLength
		{
			get
			{
				object caretLock = this._caretLock;
				int selectionLength;
				lock (caretLock)
				{
					selectionLength = this._caret.SelectionLength;
				}
				return selectionLength;
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000386 RID: 902 RVA: 0x00027414 File Offset: 0x00025614
		public int CaretIndex
		{
			get
			{
				object caretLock = this._caretLock;
				int currentIndex;
				lock (caretLock)
				{
					currentIndex = this._caret.CurrentIndex;
				}
				return currentIndex;
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000387 RID: 903 RVA: 0x0002745C File Offset: 0x0002565C
		public bool CaretFocused
		{
			get
			{
				object caretLock = this._caretLock;
				bool focused;
				lock (caretLock)
				{
					focused = this._caret.Focused;
				}
				return focused;
			}
		}

		// Token: 0x06000388 RID: 904 RVA: 0x000274A4 File Offset: 0x000256A4
		public void SetCaretStart(int index, Point location)
		{
			location = this.ScrollToCaret(index, location);
			object caretLock = this._caretLock;
			lock (caretLock)
			{
				this._caret.SetStartIndex(index);
				this._caret.SetCaretLocation(location);
			}
			base.Invalidate();
		}

		// Token: 0x06000389 RID: 905 RVA: 0x00027508 File Offset: 0x00025708
		public void SetCaretEnd(int index, Point location)
		{
			location = this.ScrollToCaret(index, location);
			object caretLock = this._caretLock;
			lock (caretLock)
			{
				this._caret.SetEndIndex(index);
				this._caret.SetCaretLocation(location);
			}
			base.Invalidate();
		}

		// Token: 0x0600038A RID: 906 RVA: 0x0002756C File Offset: 0x0002576C
		public bool IsSelected(int byteIndex)
		{
			object caretLock = this._caretLock;
			bool result;
			lock (caretLock)
			{
				result = this._caret.IsSelected(byteIndex);
			}
			return result;
		}

		// Token: 0x0600038B RID: 907 RVA: 0x000275B4 File Offset: 0x000257B4
		public void DestroyCaret()
		{
			object caretLock = this._caretLock;
			lock (caretLock)
			{
				this._caret.Destroy();
			}
		}

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x0600038C RID: 908 RVA: 0x000275FC File Offset: 0x000257FC
		public int HexTableLength
		{
			get
			{
				object hexTableLock = this._hexTableLock;
				int length;
				lock (hexTableLock)
				{
					length = this._hexTable.Length;
				}
				return length;
			}
		}

		// Token: 0x0600038D RID: 909 RVA: 0x00027644 File Offset: 0x00025844
		public void RemoveSelectedBytes()
		{
			int selectionStart = this.SelectionStart;
			int selectionLength = this.SelectionLength;
			if (selectionLength > 0)
			{
				object hexTableLock = this._hexTableLock;
				lock (hexTableLock)
				{
					this._hexTable.RemoveRange(selectionStart, selectionLength);
				}
				this.UpdateRectanglePositioning();
				base.Invalidate();
				if (this.HexTableChanged != null)
				{
					this.HexTableChanged(this, EventArgs.Empty);
				}
			}
		}

		// Token: 0x0600038E RID: 910 RVA: 0x000276C4 File Offset: 0x000258C4
		public void RemoveByteAt(int index)
		{
			object hexTableLock = this._hexTableLock;
			lock (hexTableLock)
			{
				this._hexTable.RemoveAt(index);
			}
			this.UpdateRectanglePositioning();
			base.Invalidate();
			if (this.HexTableChanged != null)
			{
				this.HexTableChanged(this, EventArgs.Empty);
			}
		}

		// Token: 0x0600038F RID: 911 RVA: 0x00027730 File Offset: 0x00025930
		public void AppendByte(byte item)
		{
			object hexTableLock = this._hexTableLock;
			lock (hexTableLock)
			{
				this._hexTable.Add(item);
			}
			this.UpdateRectanglePositioning();
			base.Invalidate();
			if (this.HexTableChanged != null)
			{
				this.HexTableChanged(this, EventArgs.Empty);
			}
		}

		// Token: 0x06000390 RID: 912 RVA: 0x0002779C File Offset: 0x0002599C
		public void InsertByte(int index, byte item)
		{
			object hexTableLock = this._hexTableLock;
			lock (hexTableLock)
			{
				this._hexTable.Insert(index, item);
			}
			this.UpdateRectanglePositioning();
			base.Invalidate();
			if (this.HexTableChanged != null)
			{
				this.HexTableChanged(this, EventArgs.Empty);
			}
		}

		// Token: 0x06000391 RID: 913 RVA: 0x00027808 File Offset: 0x00025A08
		public char GetByteAsChar(int index)
		{
			object hexTableLock = this._hexTableLock;
			char charAt;
			lock (hexTableLock)
			{
				charAt = this._hexTable.GetCharAt(index);
			}
			return charAt;
		}

		// Token: 0x06000392 RID: 914 RVA: 0x00027850 File Offset: 0x00025A50
		public byte GetByte(int index)
		{
			object hexTableLock = this._hexTableLock;
			byte at;
			lock (hexTableLock)
			{
				at = this._hexTable.GetAt(index);
			}
			return at;
		}

		// Token: 0x06000393 RID: 915 RVA: 0x00027898 File Offset: 0x00025A98
		public void SetByte(int index, byte item)
		{
			object hexTableLock = this._hexTableLock;
			lock (hexTableLock)
			{
				this._hexTable.SetAt(index, item);
			}
			base.Invalidate();
			if (this.HexTableChanged != null)
			{
				this.HexTableChanged(this, EventArgs.Empty);
			}
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00027900 File Offset: 0x00025B00
		public void ScrollLineUp(int lines)
		{
			if (this._firstByte > 0)
			{
				lines = ((lines > this._vScrollPos) ? this._vScrollPos : lines);
				this._vScrollPos -= this._vScrollSmall * lines;
				this.UpdateVisibleByteIndex();
				this.UpdateScrollValues();
				if (this.CaretFocused)
				{
					Point caretLocation = new Point(this.CaretPosX, this.CaretPosY);
					caretLocation.Y += this._recLineCount.Height * lines;
					object caretLock = this._caretLock;
					lock (caretLock)
					{
						this._caret.SetCaretLocation(caretLocation);
					}
				}
			}
		}

		// Token: 0x06000395 RID: 917 RVA: 0x000279C0 File Offset: 0x00025BC0
		public void ScrollLineDown(int lines)
		{
			if (this._vScrollPos <= this._vScrollMax - this._vScrollLarge)
			{
				lines = ((lines + this._vScrollPos > this._vScrollMax - this._vScrollLarge) ? (this._vScrollMax - this._vScrollLarge - this._vScrollPos + 1) : lines);
				this._vScrollPos += this._vScrollSmall * lines;
				this.UpdateVisibleByteIndex();
				this.UpdateScrollValues();
				if (this.CaretFocused)
				{
					Point caretLocation = new Point(this.CaretPosX, this.CaretPosY);
					caretLocation.Y -= this._recLineCount.Height * lines;
					object caretLock = this._caretLock;
					lock (caretLock)
					{
						this._caret.SetCaretLocation(caretLocation);
						if (caretLocation.Y < this._recContent.Y)
						{
							this._caret.Hide(base.Handle);
						}
					}
				}
			}
		}

		// Token: 0x06000396 RID: 918 RVA: 0x00027AD0 File Offset: 0x00025CD0
		public void ScrollThumbTrack(int lines)
		{
			if (lines == 0)
			{
				return;
			}
			if (lines < 0)
			{
				this.ScrollLineUp(-1 * lines);
				return;
			}
			this.ScrollLineDown(lines);
		}

		// Token: 0x06000397 RID: 919 RVA: 0x00027AEC File Offset: 0x00025CEC
		public Point ScrollToCaret(int caretIndex, Point position)
		{
			if (position.Y < 0)
			{
				this._vScrollPos -= Math.Abs((position.Y - this._recContent.Y) / this._recLineCount.Height) * this._vScrollSmall;
				this.UpdateVisibleByteIndex();
				this.UpdateScrollValues();
				if (this.CaretFocused)
				{
					position.Y = this._recContent.Y;
				}
			}
			else if (position.Y > this._maxVisibleBytesV * this._recLineCount.Height)
			{
				this._vScrollPos += (position.Y / this._recLineCount.Height - (this._maxVisibleBytesV - 1)) * this._vScrollSmall;
				if (this._vScrollPos > this._vScrollMax - (this._vScrollLarge - 1))
				{
					this._vScrollPos = this._vScrollMax - (this._vScrollLarge - 1);
				}
				this.UpdateVisibleByteIndex();
				this.UpdateScrollValues();
				if (this.CaretFocused)
				{
					position.Y = (this._maxVisibleBytesV - 1) * this._recLineCount.Height + this._recContent.Y;
				}
			}
			return position;
		}

		// Token: 0x06000398 RID: 920 RVA: 0x00027C20 File Offset: 0x00025E20
		private void UpdateRectanglePositioning()
		{
			if (base.ClientRectangle.Width == 0)
			{
				return;
			}
			SizeF sizeF;
			using (Graphics graphics = base.CreateGraphics())
			{
				sizeF = graphics.MeasureString("D", this.Font, 100, this._stringFormat);
			}
			this.CharSize = new SizeF((float)Math.Ceiling((double)sizeF.Width), (float)Math.Ceiling((double)sizeF.Height));
			this._recContent = base.ClientRectangle;
			this._recContent.X = this._recContent.X + base.Margin.Left;
			this._recContent.Y = this._recContent.Y + base.Margin.Top;
			this._recContent.Width = this._recContent.Width - base.Margin.Right;
			this._recContent.Height = this._recContent.Height - base.Margin.Bottom;
			if (this.BorderStyle == BorderStyle.Fixed3D)
			{
				this._recContent.X = this._recContent.X + 2;
				this._recContent.Y = this._recContent.Y + 2;
				this._recContent.Width = this._recContent.Width - 1;
				this._recContent.Height = this._recContent.Height - 1;
			}
			else if (this.BorderStyle == BorderStyle.FixedSingle)
			{
				this._recContent.X = this._recContent.X + 1;
				this._recContent.Y = this._recContent.Y + 1;
				this._recContent.Width = this._recContent.Width - 1;
				this._recContent.Height = this._recContent.Height - 1;
			}
			if (!this.VScrollBarHidden)
			{
				this._recContent.Width = this._recContent.Width - this._vScrollBarWidth;
				this._vScrollBar.Left = this._recContent.X + this._recContent.Width - base.Margin.Left;
				this._vScrollBar.Top = this._recContent.Y - base.Margin.Top;
				this._vScrollBar.Width = this._vScrollBarWidth;
				this._vScrollBar.Height = this._recContent.Height;
			}
			this._recLineCount = new Rectangle(this._recContent.X, this._recContent.Y, (int)(this._charSize.Width * 4f), (int)this._charSize.Height - 2);
			this._editView.Update(this._recLineCount.X + this._recLineCount.Width + this._entityMargin / 2, this._recContent);
			this._maxBytesH = this._bytesPerLine;
			this._maxBytesV = (int)Math.Ceiling((double)((float)this._recContent.Height / (float)this._recLineCount.Height));
			this._maxBytes = this._maxBytesH * this._maxBytesV;
			this._maxVisibleBytesV = (int)Math.Floor((double)((float)this._recContent.Height / (float)this._recLineCount.Height));
			this.UpdateScrollBarSize();
		}

		// Token: 0x06000399 RID: 921 RVA: 0x00027F60 File Offset: 0x00026160
		private void UpdateVisibleByteIndex()
		{
			if (this._hexTable.Length == 0)
			{
				this._firstByte = 0;
				this._lastByte = 0;
				return;
			}
			this._firstByte = this._vScrollPos * this._maxBytesH;
			this._lastByte = Math.Min(this.HexTableLength, this._firstByte + this._maxBytes);
		}

		// Token: 0x0600039A RID: 922 RVA: 0x00027FBC File Offset: 0x000261BC
		private void UpdateScrollValues()
		{
			if (!this._isVScrollHidden)
			{
				this._vScrollBar.Minimum = this._vScrollMin;
				this._vScrollBar.Maximum = this._vScrollMax;
				this._vScrollBar.Value = this._vScrollPos;
				this._vScrollBar.SmallChange = this._vScrollSmall;
				this._vScrollBar.LargeChange = this._vScrollLarge;
				this._vScrollBar.Visible = true;
				return;
			}
			this._vScrollBar.Visible = false;
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00028040 File Offset: 0x00026240
		private void UpdateScrollBarSize()
		{
			if (!this.VScrollBarVisisble || this._maxVisibleBytesV <= 0 || this._maxBytesH <= 0)
			{
				this.VScrollBarHidden = true;
				return;
			}
			int maxVisibleBytesV = this._maxVisibleBytesV;
			int num = 1;
			int vScrollMin = 0;
			int num2 = this.HexTableLength / this._maxBytesH;
			int num3 = this._firstByte / this._maxBytesH;
			if (maxVisibleBytesV != this._vScrollLarge || num != this._vScrollSmall)
			{
				this._vScrollLarge = maxVisibleBytesV;
				this._vScrollSmall = num;
			}
			if (num2 >= maxVisibleBytesV)
			{
				if (num2 != this._vScrollMax || num3 != this._vScrollPos)
				{
					this._vScrollMin = vScrollMin;
					this._vScrollMax = num2;
					this._vScrollPos = num3;
				}
				this.VScrollBarHidden = false;
				this.UpdateScrollValues();
				return;
			}
			this.VScrollBarHidden = true;
		}

		// Token: 0x0600039C RID: 924 RVA: 0x00028102 File Offset: 0x00026302
		public HexEditor() : this(new ByteCollection())
		{
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00028110 File Offset: 0x00026310
		public HexEditor(ByteCollection collection)
		{
			this._stringFormat = new StringFormat(StringFormat.GenericTypographic);
			this._stringFormat.Alignment = StringAlignment.Center;
			this._stringFormat.LineAlignment = StringAlignment.Center;
			this._hexTable = collection;
			this._vScrollBar = new VScrollBar();
			this._vScrollBar.Scroll += this.OnVScrollBarScroll;
			base.SetStyle(ControlStyles.ResizeRedraw, true);
			base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
			base.SetStyle(ControlStyles.Selectable, true);
			this._caret = new Caret(this);
			this._caret.SelectionStartChanged += this.CaretSelectionStartChanged;
			this._caret.SelectionLengthChanged += this.CaretSelectionLengthChanged;
			this._editView = new EditView(this);
			this._handler = this._editView;
			this.Cursor = Cursors.IBeam;
		}

		// Token: 0x0600039E RID: 926 RVA: 0x00028270 File Offset: 0x00026470
		private RectangleF GetLineCountBound(int index)
		{
			return new RectangleF((float)this._recLineCount.X, (float)(this._recLineCount.Y + this._recLineCount.Height * index), (float)this._recLineCount.Width, (float)this._recLineCount.Height);
		}

		// Token: 0x0600039F RID: 927 RVA: 0x000282C0 File Offset: 0x000264C0
		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			if (this.BorderStyle == BorderStyle.Fixed3D)
			{
				SolidBrush brush = new SolidBrush(this.BackColor);
				Rectangle clientRectangle = base.ClientRectangle;
				pevent.Graphics.FillRectangle(brush, clientRectangle);
				ControlPaint.DrawBorder3D(pevent.Graphics, base.ClientRectangle, Border3DStyle.Sunken);
				return;
			}
			if (this.BorderStyle == BorderStyle.FixedSingle)
			{
				SolidBrush brush2 = new SolidBrush(this.BackColor);
				Rectangle clientRectangle2 = base.ClientRectangle;
				pevent.Graphics.FillRectangle(brush2, clientRectangle2);
				ControlPaint.DrawBorder(pevent.Graphics, base.ClientRectangle, this.BorderColor, ButtonBorderStyle.Solid);
				return;
			}
			base.OnPaintBackground(pevent);
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x00028354 File Offset: 0x00026554
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			Region region = new Region(base.ClientRectangle);
			region.Exclude(this._recContent);
			e.Graphics.ExcludeClip(region);
			this.UpdateVisibleByteIndex();
			this.PaintLineCount(e.Graphics, this._firstByte, this._lastByte);
			this._editView.Paint(e.Graphics, this._firstByte, this._lastByte);
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x000283C8 File Offset: 0x000265C8
		private void PaintLineCount(Graphics g, int startIndex, int lastIndex)
		{
			SolidBrush brush = new SolidBrush(this.ForeColor);
			int num = 0;
			while (num * this._maxBytesH + startIndex <= lastIndex)
			{
				RectangleF lineCountBound = this.GetLineCountBound(num);
				string text = (startIndex + num * this._maxBytesH).ToString(this._lineCountCaps);
				int num2 = this._nrCharsLineCount - text.Length;
				string s;
				if (num2 > -1)
				{
					s = new string('0', num2) + text;
				}
				else
				{
					s = new string('~', this._nrCharsLineCount);
				}
				g.DrawString(s, this.Font, brush, lineCountBound, this._stringFormat);
				num++;
			}
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x00028464 File Offset: 0x00026664
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.UpdateRectanglePositioning();
			base.Invalidate();
		}

		// Token: 0x040001DC RID: 476
		private object _caretLock = new object();

		// Token: 0x040001DD RID: 477
		private object _hexTableLock = new object();

		// Token: 0x040001DE RID: 478
		private IKeyMouseEventHandler _handler;

		// Token: 0x040001DF RID: 479
		private EditView _editView;

		// Token: 0x040001E0 RID: 480
		private ByteCollection _hexTable;

		// Token: 0x040001E1 RID: 481
		private string _lineCountCaps = "X";

		// Token: 0x040001E2 RID: 482
		private int _nrCharsLineCount = 4;

		// Token: 0x040001E3 RID: 483
		private Caret _caret;

		// Token: 0x040001E4 RID: 484
		private Rectangle _recContent;

		// Token: 0x040001E5 RID: 485
		private Rectangle _recLineCount;

		// Token: 0x040001E6 RID: 486
		private StringFormat _stringFormat;

		// Token: 0x040001E7 RID: 487
		private int _firstByte;

		// Token: 0x040001E8 RID: 488
		private int _lastByte;

		// Token: 0x040001E9 RID: 489
		private int _maxBytesH;

		// Token: 0x040001EA RID: 490
		private int _maxBytesV;

		// Token: 0x040001EB RID: 491
		private int _maxBytes;

		// Token: 0x040001EC RID: 492
		private int _maxVisibleBytesV;

		// Token: 0x040001ED RID: 493
		private VScrollBar _vScrollBar;

		// Token: 0x040001EE RID: 494
		private int _vScrollBarWidth = 20;

		// Token: 0x040001EF RID: 495
		private int _vScrollPos;

		// Token: 0x040001F0 RID: 496
		private int _vScrollMax;

		// Token: 0x040001F1 RID: 497
		private int _vScrollMin;

		// Token: 0x040001F2 RID: 498
		private int _vScrollSmall;

		// Token: 0x040001F3 RID: 499
		private int _vScrollLarge;

		// Token: 0x040001F4 RID: 500
		private SizeF _charSize;

		// Token: 0x040001F5 RID: 501
		private bool _isVScrollHidden = true;

		// Token: 0x040001F6 RID: 502
		private int _bytesPerLine = 8;

		// Token: 0x040001F7 RID: 503
		private int _entityMargin = 10;

		// Token: 0x040001F8 RID: 504
		private BorderStyle _borderStyle = BorderStyle.Fixed3D;

		// Token: 0x040001F9 RID: 505
		private Color _borderColor = Color.Empty;

		// Token: 0x040001FA RID: 506
		private Color _selectionBackColor = Color.Blue;

		// Token: 0x040001FB RID: 507
		private Color _selectionForeColor = Color.White;

		// Token: 0x040001FC RID: 508
		private HexEditor.CaseStyle _lineCountCaseStyle = HexEditor.CaseStyle.UpperCase;

		// Token: 0x040001FD RID: 509
		private HexEditor.CaseStyle _hexViewCaseStyle = HexEditor.CaseStyle.UpperCase;

		// Token: 0x040001FE RID: 510
		private bool _isVScrollVisible;

		// Token: 0x04000203 RID: 515
		private bool _dragging;

		// Token: 0x0200022B RID: 555
		public enum CaseStyle
		{
			// Token: 0x04000825 RID: 2085
			LowerCase,
			// Token: 0x04000826 RID: 2086
			UpperCase
		}
	}
}
