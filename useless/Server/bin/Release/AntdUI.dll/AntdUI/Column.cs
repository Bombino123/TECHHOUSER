using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace AntdUI;

public class Column<T> : Column
{
	private Func<object?, T, int, object?>? render;

	public new Func<object?, T, int, object?>? Render
	{
		set
		{
			if ((Delegate?)render == (Delegate?)value)
			{
				return;
			}
			render = value;
			if (render == null)
			{
				base.Render = null;
				return;
			}
			base.Render = (object? val, object record, int index) => (record is T arg) ? render(val, arg, index) : null;
		}
	}

	public Column(string key, string title)
		: base(key, title)
	{
	}

	public Column(string key, string title, ColumnAlign align)
		: base(key, title, align)
	{
	}
}
public class Column
{
	private string _title;

	private bool visible = true;

	private bool lineBreak;

	private bool _fixed;

	private bool sortorder;

	private SortMode sortMode;

	public string Key { get; set; }

	public string Title
	{
		get
		{
			return Localization.GetLangIN(LocalizationTitle, _title, new string[2] { "{id}", Key });
		}
		set
		{
			if (!(_title == value))
			{
				_title = value;
				Invalidates();
			}
		}
	}

	[Description("显示文本")]
	[Category("国际化")]
	[DefaultValue(null)]
	public string? LocalizationTitle { get; set; }

	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			if (visible != value)
			{
				visible = value;
				Invalidates();
			}
		}
	}

	public ColumnAlign Align { get; set; }

	public ColumnAlign? ColAlign { get; set; }

	public string? Width { get; set; }

	public string? MaxWidth { get; set; }

	public bool Ellipsis { get; set; }

	public bool LineBreak
	{
		get
		{
			return lineBreak;
		}
		set
		{
			if (lineBreak != value)
			{
				lineBreak = value;
				Invalidates();
			}
		}
	}

	public bool Fixed
	{
		get
		{
			return _fixed;
		}
		set
		{
			if (_fixed != value)
			{
				_fixed = value;
				Invalidates();
			}
		}
	}

	public bool SortOrder
	{
		get
		{
			return sortorder;
		}
		set
		{
			if (sortorder != value)
			{
				sortorder = value;
				Invalidate();
			}
		}
	}

	public SortMode SortMode
	{
		get
		{
			return sortMode;
		}
		set
		{
			if (sortMode == value)
			{
				return;
			}
			if (PARENT == null || PARENT.rows == null)
			{
				sortMode = value;
				Invalidate();
				return;
			}
			Table.CELL[] cells = PARENT.rows[0].cells;
			foreach (Table.CELL cELL in cells)
			{
				if (cELL.COLUMN.SortOrder)
				{
					cELL.COLUMN.sortMode = SortMode.NONE;
				}
			}
			sortMode = value;
			Invalidate();
		}
	}

	public string? KeyTree { get; set; }

	public Table.CellStyleInfo? Style { get; set; }

	public Table.CellStyleInfo? ColStyle { get; set; }

	internal Table? PARENT { get; set; }

	internal int INDEX { get; set; }

	public Func<object?, object, int, object?>? Render { get; set; }

	public Column(string key, string title)
	{
		Key = key;
		_title = title;
	}

	public Column(string key, string title, ColumnAlign align)
	{
		Key = key;
		_title = title;
		Align = align;
	}

	public Column SetLocalizationTitle(string? value)
	{
		LocalizationTitle = value;
		return this;
	}

	public Column SetLocalizationTitleID(string value)
	{
		LocalizationTitle = value + "{id}";
		return this;
	}

	public Column SetVisible(bool value = false)
	{
		Visible = value;
		return this;
	}

	public Column SetAlign(ColumnAlign value = ColumnAlign.Center)
	{
		Align = value;
		return this;
	}

	public Column SetColAlign(ColumnAlign value = ColumnAlign.Center)
	{
		Align = value;
		return this;
	}

	public Column SetAligns(ColumnAlign value = ColumnAlign.Center, ColumnAlign col = ColumnAlign.Center)
	{
		Align = value;
		ColAlign = col;
		return this;
	}

	public Column SetWidth(string? value = null)
	{
		Width = value;
		return this;
	}

	public Column SetMaxWidth(string? value = null)
	{
		MaxWidth = value;
		return this;
	}

	public Column SetEllipsis(bool value = true)
	{
		Ellipsis = value;
		return this;
	}

	public Column SetLineBreak(bool value = true)
	{
		LineBreak = value;
		return this;
	}

	public Column SetFixed(bool value = true)
	{
		Fixed = value;
		return this;
	}

	public Column SetSortOrder(bool value = true)
	{
		SortOrder = value;
		return this;
	}

	private void Invalidate()
	{
		if (PARENT != null && PARENT.LoadLayout())
		{
			((Control)PARENT).Invalidate();
		}
	}

	private void Invalidates()
	{
		if (PARENT != null)
		{
			PARENT.ExtractHeaderFixed();
			if (PARENT.LoadLayout())
			{
				((Control)PARENT).Invalidate();
			}
		}
	}

	internal bool SetSortMode(SortMode value)
	{
		if (sortMode == value)
		{
			return false;
		}
		sortMode = value;
		return true;
	}
}
