using System;
using System.Collections.Generic;
using System.Drawing;

namespace AntdUI;

public abstract class ICell
{
	private Table.CELL? _PARENT;

	public Action<object>? DropDownValueChanged;

	public Table.CELL PARENT
	{
		get
		{
			if (_PARENT == null)
			{
				throw new ArgumentNullException();
			}
			return _PARENT;
		}
	}

	public Rectangle Rect { get; set; }

	public TAlignFrom DropDownPlacement { get; set; } = TAlignFrom.BL;


	public int DropDownMaxCount { get; set; } = 4;


	public int? DropDownRadius { get; set; }

	public bool DropDownArrow { get; set; }

	public Size DropDownPadding { get; set; } = new Size(12, 5);


	public bool DropDownClickEnd { get; set; }

	public bool DropDownClickSwitchDropdown { get; set; } = true;


	public IList<object>? DropDownItems { get; set; }

	public object? DropDownValue { get; set; }

	public Action<bool>? Changed { get; set; }

	public abstract Size GetSize(Canvas g, Font font, int gap, int gap2);

	public abstract void SetRect(Canvas g, Font font, Rectangle rect, Size size, int gap, int gap2);

	public abstract void PaintBack(Canvas g);

	public abstract void Paint(Canvas g, Font font, bool enable, SolidBrush fore);

	internal void SetCELL(Table.CELL row)
	{
		_PARENT = row;
	}

	public void OnPropertyChanged(bool layout = false)
	{
		Changed?.Invoke(layout);
	}
}
