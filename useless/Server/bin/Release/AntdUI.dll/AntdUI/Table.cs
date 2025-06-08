using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AntdUI.Design;

namespace AntdUI;

[Description("Table 表格")]
[DefaultEvent("CellClick")]
[ToolboxItem(true)]
public class Table : IControl, IEventListener
{
	private class TempTable
	{
		public TempiColumn[] columns { get; set; }

		public IRow[] rows { get; set; }

		public TempTable(TempiColumn[] _columns, IRow[] _rows)
		{
			columns = _columns;
			rows = _rows;
		}
	}

	private class TempiColumn
	{
		public string key { get; set; }

		public string? text { get; set; }

		public int i { get; set; }

		public TempiColumn(int index, DataColumn dataColumn)
		{
			i = index;
			key = dataColumn.ColumnName;
			if (!string.IsNullOrEmpty(dataColumn.Caption))
			{
				text = dataColumn.Caption;
			}
		}

		public TempiColumn(int index, string name)
		{
			i = index;
			key = name;
		}
	}

	public class IRow
	{
		public int i { get; set; }

		public object record { get; set; }

		public Dictionary<string, object?> cells { get; set; }

		public object? this[int index]
		{
			get
			{
				if (cells == null)
				{
					return null;
				}
				int num = 0;
				foreach (KeyValuePair<string, object> cell in cells)
				{
					if (num == index)
					{
						if (cell.Value is PropertyDescriptor propertyDescriptor)
						{
							return propertyDescriptor.GetValue(record);
						}
						if (cell.Value is AntItem antItem)
						{
							return antItem.value;
						}
						return cell.Value;
					}
					num++;
				}
				return null;
			}
		}

		public object? this[string key]
		{
			get
			{
				if (cells == null)
				{
					return null;
				}
				if (cells.TryGetValue(key, out object value))
				{
					if (value is PropertyDescriptor propertyDescriptor)
					{
						return propertyDescriptor.GetValue(record);
					}
					if (value is AntItem antItem)
					{
						return antItem.value;
					}
					return value;
				}
				return null;
			}
		}

		public IRow(int index, object _record, Dictionary<string, object?> _cells)
		{
			i = index;
			record = _record;
			cells = _cells;
		}

		public IRow(int index, object _record, IList<AntItem> _cells)
		{
			i = index;
			record = _record;
			cells = new Dictionary<string, object>(_cells.Count);
			foreach (AntItem _cell in _cells)
			{
				cells.Add(_cell.key, _cell);
			}
		}
	}

	public delegate void CheckEventHandler(object sender, TableCheckEventArgs e);

	public delegate void ClickEventHandler(object sender, TableClickEventArgs e);

	public delegate void ClickButtonEventHandler(object sender, TableButtonEventArgs e);

	public class CheckStateEventArgs : EventArgs
	{
		public ColumnCheck Column { get; private set; }

		public CheckState? Value { get; private set; }

		public CheckStateEventArgs(ColumnCheck column, CheckState value)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			Column = column;
			Value = value;
		}
	}

	public delegate void CheckStateEventHandler(object sender, CheckStateEventArgs e);

	public delegate bool BeginEditEventHandler(object sender, TableEventArgs e);

	public delegate void BeginEditInputStyleEventHandler(object sender, TableBeginEditInputStyleEventArgs e);

	public delegate bool EndEditEventHandler(object sender, TableEndEditEventArgs e);

	public delegate CellStyleInfo? SetRowStyleEventHandler(object sender, TableSetRowStyleEventArgs e);

	public class CellStyleInfo
	{
		public Color? BackColor { get; set; }

		public Color? ForeColor { get; set; }
	}

	public delegate bool SortModeEventHandler(object sender, TableSortModeEventArgs e);

	internal class RowTemplate
	{
		private Table PARENT;

		internal bool IsColumn;

		internal bool hover;

		internal float AnimationHoverValue;

		internal bool AnimationHover;

		internal ITask? ThreadHover;

		internal Rectangle RectExpand;

		internal bool SHOW { get; set; }

		public Rectangle RECT { get; set; }

		public object? RECORD { get; set; }

		public bool ENABLE { get; set; } = true;


		public int INDEX { get; set; }

		public int INDEX_REAL { get; set; }

		public CELL[] cells { get; set; }

		public int Height { get; set; }

		internal bool Hover
		{
			get
			{
				return hover;
			}
			set
			{
				if (hover == value)
				{
					return;
				}
				hover = value;
				if (!SHOW || (PARENT.RowHoverBg ?? Colour.FillSecondary.Get("Table")).A <= 0)
				{
					return;
				}
				if (Config.Animation)
				{
					ThreadHover?.Dispose();
					AnimationHover = true;
					int t = Animation.TotalFrames(20, 200);
					if (value)
					{
						ThreadHover = new ITask(delegate(int i)
						{
							AnimationHoverValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)PARENT).Invalidate();
							return true;
						}, 20, t, delegate
						{
							AnimationHover = false;
							AnimationHoverValue = 1f;
							((Control)PARENT).Invalidate();
						});
					}
					else
					{
						ThreadHover = new ITask(delegate(int i)
						{
							AnimationHoverValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)PARENT).Invalidate();
							return true;
						}, 20, t, delegate
						{
							AnimationHover = false;
							AnimationHoverValue = 0f;
							((Control)PARENT).Invalidate();
						});
					}
				}
				else
				{
					((Control)PARENT).Invalidate();
				}
			}
		}

		public bool Select { get; set; }

		public bool CanExpand { get; set; }

		public bool Expand { get; set; }

		public bool ShowExpand { get; set; } = true;


		internal int ExpandDepth { get; set; }

		internal int KeyTreeINDEX { get; set; } = -1;


		public RowTemplate(Table table, CELL[] cell, int i, object? value)
		{
			PARENT = table;
			cells = cell;
			RECORD = value;
			INDEX_REAL = i;
		}

		internal bool Contains(int x, int y, bool sethover)
		{
			if (ENABLE)
			{
				if (sethover)
				{
					if (CONTAINS(x, y))
					{
						Hover = true;
						return true;
					}
					Hover = false;
					return false;
				}
				return CONTAINS(x, y);
			}
			return false;
		}

		internal bool CONTAINS(int x, int y)
		{
			if (ENABLE)
			{
				return RECT.Contains(x, y);
			}
			return false;
		}
	}

	private class TCellCheck : CELL
	{
		public bool AnimationCheck;

		public float AnimationCheckValue;

		private ITask? ThreadCheck;

		private bool _checked;

		[Description("选中状态")]
		[Category("行为")]
		[DefaultValue(false)]
		public bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				if (_checked != value)
				{
					_checked = value;
					OnCheck();
				}
			}
		}

		public bool NoTitle { get; set; }

		public bool AutoCheck { get; set; }

		public TCellCheck(Table table, ColumnCheck column, PropertyDescriptor? prop, object? ov, bool value)
			: base(table, column, prop, ov)
		{
			_checked = value;
			AnimationCheckValue = (_checked ? 1f : 0f);
			NoTitle = column.NoTitle;
			AutoCheck = column.AutoCheck;
		}

		private void OnCheck()
		{
			ThreadCheck?.Dispose();
			if (!base.ROW.SHOW || !((Control)base.PARENT).IsHandleCreated)
			{
				return;
			}
			if (Config.Animation)
			{
				AnimationCheck = true;
				if (_checked)
				{
					ThreadCheck = new ITask((Control)(object)base.PARENT, delegate
					{
						AnimationCheckValue = AnimationCheckValue.Calculate(0.2f);
						if (AnimationCheckValue > 1f)
						{
							AnimationCheckValue = 1f;
							return false;
						}
						((Control)base.PARENT).Invalidate();
						return true;
					}, 20, delegate
					{
						AnimationCheck = false;
						((Control)base.PARENT).Invalidate();
					});
					return;
				}
				ThreadCheck = new ITask((Control)(object)base.PARENT, delegate
				{
					AnimationCheckValue = AnimationCheckValue.Calculate(-0.2f);
					if (AnimationCheckValue <= 0f)
					{
						AnimationCheckValue = 0f;
						return false;
					}
					((Control)base.PARENT).Invalidate();
					return true;
				}, 20, delegate
				{
					AnimationCheck = false;
					((Control)base.PARENT).Invalidate();
				});
			}
			else
			{
				AnimationCheckValue = (_checked ? 1f : 0f);
				((Control)base.PARENT).Invalidate();
			}
		}

		public override void SetSize(Canvas g, Font font, Rectangle _rect, int ox, int gap, int gap2)
		{
		}

		public void SetSize(Rectangle _rect, int check_size)
		{
			base.RECT = _rect;
			base.RECT_REAL = new Rectangle(_rect.X + (_rect.Width - check_size) / 2, _rect.Y + (_rect.Height - check_size) / 2, check_size, check_size);
		}

		public override Size GetSize(Canvas g, Font font, int width, int gap, int gap2)
		{
			Size result = g.MeasureString("龍Qq", font);
			base.MinWidth = result.Width;
			return result;
		}

		public override string ToString()
		{
			return Checked.ToString();
		}
	}

	private class TCellRadio : CELL
	{
		public bool AnimationCheck;

		public float AnimationCheckValue;

		private ITask? ThreadCheck;

		private bool _checked;

		[Description("选中状态")]
		[Category("行为")]
		[DefaultValue(false)]
		public bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				if (_checked != value)
				{
					_checked = value;
					OnCheck();
				}
			}
		}

		public bool AutoCheck { get; set; }

		public TCellRadio(Table table, ColumnRadio column, PropertyDescriptor? prop, object? ov, bool value)
			: base(table, column, prop, ov)
		{
			_checked = value;
			AnimationCheckValue = (_checked ? 1f : 0f);
			AutoCheck = column.AutoCheck;
		}

		private void OnCheck()
		{
			ThreadCheck?.Dispose();
			if (!base.ROW.SHOW || !((Control)base.PARENT).IsHandleCreated)
			{
				return;
			}
			if (Config.Animation)
			{
				AnimationCheck = true;
				if (_checked)
				{
					ThreadCheck = new ITask((Control)(object)base.PARENT, delegate
					{
						AnimationCheckValue = AnimationCheckValue.Calculate(0.2f);
						if (AnimationCheckValue > 1f)
						{
							AnimationCheckValue = 1f;
							return false;
						}
						((Control)base.PARENT).Invalidate();
						return true;
					}, 20, delegate
					{
						AnimationCheck = false;
						((Control)base.PARENT).Invalidate();
					});
					return;
				}
				ThreadCheck = new ITask((Control)(object)base.PARENT, delegate
				{
					AnimationCheckValue = AnimationCheckValue.Calculate(-0.2f);
					if (AnimationCheckValue <= 0f)
					{
						AnimationCheckValue = 0f;
						return false;
					}
					((Control)base.PARENT).Invalidate();
					return true;
				}, 20, delegate
				{
					AnimationCheck = false;
					((Control)base.PARENT).Invalidate();
				});
			}
			else
			{
				AnimationCheckValue = (_checked ? 1f : 0f);
				((Control)base.PARENT).Invalidate();
			}
		}

		public override void SetSize(Canvas g, Font font, Rectangle _rect, int ox, int gap, int gap2)
		{
		}

		public void SetSize(Rectangle _rect, int check_size)
		{
			base.RECT = _rect;
			base.RECT_REAL = new Rectangle(_rect.X + (_rect.Width - check_size) / 2, _rect.Y + (_rect.Height - check_size) / 2, check_size, check_size);
		}

		public override Size GetSize(Canvas g, Font font, int width, int gap, int gap2)
		{
			Size result = g.MeasureString("龍Qq", font);
			base.MinWidth = result.Width;
			return result;
		}

		public override string ToString()
		{
			return Checked.ToString();
		}
	}

	private class TCellSwitch : CELL
	{
		internal bool AnimationCheck;

		internal float AnimationCheckValue;

		private ITask? ThreadCheck;

		private bool _checked;

		private ITask? ThreadHover;

		internal float AnimationHoverValue;

		internal bool AnimationHover;

		internal bool _mouseHover;

		private bool loading;

		private ITask? ThreadLoading;

		internal float LineWidth = 6f;

		internal float LineAngle;

		[Description("选中状态")]
		[Category("行为")]
		[DefaultValue(false)]
		public bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				if (_checked != value)
				{
					_checked = value;
					OnCheck();
				}
			}
		}

		internal bool ExtraMouseHover
		{
			get
			{
				return _mouseHover;
			}
			set
			{
				if (_mouseHover == value)
				{
					return;
				}
				_mouseHover = value;
				if (base.ROW.SHOW && ((Control)base.PARENT).IsHandleCreated && Config.Animation)
				{
					ThreadHover?.Dispose();
					AnimationHover = true;
					if (value)
					{
						ThreadHover = new ITask((Control)(object)base.PARENT, delegate
						{
							AnimationHoverValue = AnimationHoverValue.Calculate(0.1f);
							if (AnimationHoverValue > 1f)
							{
								AnimationHoverValue = 1f;
								return false;
							}
							((Control)base.PARENT).Invalidate();
							return true;
						}, 10, delegate
						{
							AnimationHover = false;
							((Control)base.PARENT).Invalidate();
						});
					}
					else
					{
						ThreadHover = new ITask((Control)(object)base.PARENT, delegate
						{
							AnimationHoverValue = AnimationHoverValue.Calculate(-0.1f);
							if (AnimationHoverValue <= 0f)
							{
								AnimationHoverValue = 0f;
								return false;
							}
							((Control)base.PARENT).Invalidate();
							return true;
						}, 10, delegate
						{
							AnimationHover = false;
							((Control)base.PARENT).Invalidate();
						});
					}
				}
				else
				{
					AnimationHoverValue = 255f;
				}
				((Control)base.PARENT).Invalidate();
			}
		}

		public bool Loading
		{
			get
			{
				return loading;
			}
			set
			{
				if (loading == value)
				{
					return;
				}
				loading = value;
				if (base.ROW.SHOW && ((Control)base.PARENT).IsHandleCreated)
				{
					if (loading)
					{
						bool ProgState = false;
						ThreadLoading = new ITask((Control)(object)base.PARENT, delegate
						{
							if (ProgState)
							{
								LineAngle = LineAngle.Calculate(9f);
								LineWidth = LineWidth.Calculate(0.6f);
								if (LineWidth > 75f)
								{
									ProgState = false;
								}
							}
							else
							{
								LineAngle = LineAngle.Calculate(9.6f);
								LineWidth = LineWidth.Calculate(-0.6f);
								if (LineWidth < 6f)
								{
									ProgState = true;
								}
							}
							if (LineAngle >= 360f)
							{
								LineAngle = 0f;
							}
							((Control)base.PARENT).Invalidate();
							return true;
						}, 10);
					}
					else
					{
						ThreadLoading?.Dispose();
					}
				}
				((Control)base.PARENT).Invalidate();
			}
		}

		public bool AutoCheck { get; set; }

		public TCellSwitch(Table table, ColumnSwitch column, PropertyDescriptor? prop, object? ov, bool value)
			: base(table, column, prop, ov)
		{
			_checked = value;
			AnimationCheckValue = (_checked ? 1f : 0f);
			AutoCheck = column.AutoCheck;
		}

		private void OnCheck()
		{
			ThreadCheck?.Dispose();
			if (!base.ROW.SHOW || !((Control)base.PARENT).IsHandleCreated)
			{
				return;
			}
			if (Config.Animation)
			{
				AnimationCheck = true;
				if (_checked)
				{
					ThreadCheck = new ITask((Control)(object)base.PARENT, delegate
					{
						AnimationCheckValue = AnimationCheckValue.Calculate(0.1f);
						if (AnimationCheckValue > 1f)
						{
							AnimationCheckValue = 1f;
							return false;
						}
						((Control)base.PARENT).Invalidate();
						return true;
					}, 10, delegate
					{
						AnimationCheck = false;
						((Control)base.PARENT).Invalidate();
					});
					return;
				}
				ThreadCheck = new ITask((Control)(object)base.PARENT, delegate
				{
					AnimationCheckValue = AnimationCheckValue.Calculate(-0.1f);
					if (AnimationCheckValue <= 0f)
					{
						AnimationCheckValue = 0f;
						return false;
					}
					((Control)base.PARENT).Invalidate();
					return true;
				}, 10, delegate
				{
					AnimationCheck = false;
					((Control)base.PARENT).Invalidate();
				});
			}
			else
			{
				AnimationCheckValue = (_checked ? 1f : 0f);
				((Control)base.PARENT).Invalidate();
			}
		}

		public override void SetSize(Canvas g, Font font, Rectangle _rect, int ox, int gap, int gap2)
		{
		}

		public void SetSize(Rectangle _rect, int check_size)
		{
			int num = check_size * 2;
			base.RECT = _rect;
			base.RECT_REAL = new Rectangle(_rect.X + (_rect.Width - num) / 2, _rect.Y + (_rect.Height - check_size) / 2, num, check_size);
		}

		public override Size GetSize(Canvas g, Font font, int width, int gap, int gap2)
		{
			Size result = g.MeasureString("龍Qq", font);
			base.MinWidth = result.Width;
			return result;
		}

		public override string ToString()
		{
			return Checked.ToString();
		}
	}

	private class TCellSort : CELL
	{
		private bool hover;

		internal float AnimationHoverValue;

		internal bool AnimationHover;

		internal ITask? ThreadHover;

		public Rectangle RECT_ICO { get; set; }

		public bool Hover
		{
			get
			{
				return hover;
			}
			set
			{
				if (hover == value)
				{
					return;
				}
				hover = value;
				if (Config.Animation)
				{
					ThreadHover?.Dispose();
					AnimationHover = true;
					int t = Animation.TotalFrames(20, 200);
					if (value)
					{
						ThreadHover = new ITask(delegate(int i)
						{
							AnimationHoverValue = Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)base.PARENT).Invalidate();
							return true;
						}, 20, t, delegate
						{
							AnimationHover = false;
							AnimationHoverValue = 1f;
							((Control)base.PARENT).Invalidate();
						});
					}
					else
					{
						ThreadHover = new ITask(delegate(int i)
						{
							AnimationHoverValue = 1f - Animation.Animate(i, t, 1f, AnimationType.Ball);
							((Control)base.PARENT).Invalidate();
							return true;
						}, 20, t, delegate
						{
							AnimationHover = false;
							AnimationHoverValue = 0f;
							((Control)base.PARENT).Invalidate();
						});
					}
				}
				else
				{
					((Control)base.PARENT).Invalidate();
				}
			}
		}

		public TCellSort(Table table, ColumnSort column)
			: base(table, column, null, null)
		{
		}

		public override void SetSize(Canvas g, Font font, Rectangle _rect, int ox, int gap, int gap2)
		{
		}

		public void SetSize(Rectangle _rect, int sort_size, int sort_ico_size)
		{
			base.RECT = _rect;
			base.RECT_REAL = new Rectangle(_rect.X + (_rect.Width - sort_size) / 2, _rect.Y + (_rect.Height - sort_size) / 2, sort_size, sort_size);
			RECT_ICO = new Rectangle(_rect.X + (_rect.Width - sort_ico_size) / 2, _rect.Y + (_rect.Height - sort_ico_size) / 2, sort_ico_size, sort_ico_size);
		}

		public override Size GetSize(Canvas g, Font font, int width, int gap, int gap2)
		{
			Size result = g.MeasureString("龍Qq", font);
			base.MinWidth = result.Width;
			return result;
		}

		public bool Contains(int x, int y)
		{
			if (base.RECT_REAL.Contains(x, y))
			{
				Hover = true;
				return true;
			}
			Hover = false;
			return false;
		}
	}

	private class TCellText : CELL
	{
		public string? value { get; set; }

		public TCellText(Table table, Column column, PropertyDescriptor? prop, object? ov, string? txt)
			: base(table, column, prop, ov)
		{
			value = txt;
		}

		public override void SetSize(Canvas g, Font font, Rectangle _rect, int ox, int gap, int gap2)
		{
			base.RECT = _rect;
			base.RECT_REAL = new Rectangle(_rect.X + gap + ox, _rect.Y + gap, _rect.Width - gap2, _rect.Height - gap2);
		}

		public override Size GetSize(Canvas g, Font font, int width, int gap, int gap2)
		{
			if (base.COLUMN.LineBreak && base.COLUMN.Width != null)
			{
				if (base.COLUMN.Width.EndsWith("%") && float.TryParse(base.COLUMN.Width.TrimEnd(new char[1] { '%' }), out var result))
				{
					Size size = g.MeasureString(value, font, (int)Math.Ceiling((float)width * (result / 100f)));
					base.MinWidth = size.Width;
					return new Size(size.Width + gap2, size.Height);
				}
				if (int.TryParse(base.COLUMN.Width, out var result2))
				{
					Size size2 = g.MeasureString(value, font, (int)Math.Ceiling((float)result2 * Config.Dpi));
					base.MinWidth = size2.Width;
					return new Size(size2.Width + gap2, size2.Height);
				}
			}
			Size size3 = g.MeasureString(value, font);
			base.MinWidth = size3.Width;
			return new Size(size3.Width + gap2, size3.Height);
		}

		public override string? ToString()
		{
			return value;
		}
	}

	internal class TCellColumn : CELL
	{
		public int SortWidth;

		public string value { get; set; }

		public Rectangle rect_up { get; set; }

		public Rectangle rect_down { get; set; }

		public TCellColumn(Table table, Column column)
			: base(table, column)
		{
			value = column.Title;
		}

		public override void SetSize(Canvas g, Font font, Rectangle _rect, int ox, int gap, int gap2)
		{
			base.RECT = _rect;
			if (base.COLUMN.SortOrder)
			{
				int num = (int)((float)gap * 0.34f);
				int y = _rect.Y + (_rect.Height - gap * 2 + num) / 2;
				rect_up = new Rectangle(_rect.Right - gap2, y, gap, gap);
				rect_down = new Rectangle(rect_up.X, rect_up.Bottom - num, gap, gap);
			}
		}

		public override Size GetSize(Canvas g, Font font, int width, int gap, int gap2)
		{
			Size size = g.MeasureString(value, font);
			SortWidth = (base.COLUMN.SortOrder ? ((int)((float)size.Height * 0.8f)) : 0);
			base.MinWidth = size.Width + gap2 + SortWidth;
			return new Size(size.Width + gap2 + SortWidth, size.Height);
		}

		public override string ToString()
		{
			return value;
		}
	}

	public abstract class CELL
	{
		private RowTemplate? _ROW;

		public Table PARENT { get; set; }

		public Column COLUMN { get; set; }

		public int INDEX { get; set; }

		public PropertyDescriptor? PROPERTY { get; set; }

		public object? VALUE { get; set; }

		internal RowTemplate ROW
		{
			get
			{
				if (_ROW == null)
				{
					throw new ArgumentNullException();
				}
				return _ROW;
			}
		}

		public Rectangle RECT { get; set; }

		public Rectangle RECT_REAL { get; set; }

		public int MinWidth { get; set; }

		public int MouseDown { get; set; }

		public CELL(Table table, Column column)
		{
			COLUMN = column;
			PARENT = table;
		}

		public CELL(Table table, Column column, PropertyDescriptor? prop, object? ov)
		{
			COLUMN = column;
			PARENT = table;
			PROPERTY = prop;
			VALUE = ov;
		}

		internal void SetROW(RowTemplate row)
		{
			_ROW = row;
		}

		public bool CONTAIN(int x, int y)
		{
			return RECT.Contains(x, y);
		}

		public bool CONTAIN_REAL(int x, int y)
		{
			return RECT_REAL.Contains(x, y);
		}

		public abstract void SetSize(Canvas g, Font font, Rectangle _rect, int ox, int gap, int gap2);

		public abstract Size GetSize(Canvas g, Font font, int width, int gap, int gap2);
	}

	internal class Template : CELL
	{
		private Size[] SIZES = new Size[0];

		public IList<ICell> Value { get; set; }

		public Template(Table table, Column column, PropertyDescriptor? prop, object? ov, ref int processing, IList<ICell> cels)
			: base(table, column, prop, ov)
		{
			Value = cels;
			foreach (ICell cel in cels)
			{
				cel.SetCELL(this);
				if (cel is CellBadge { State: TState.Processing })
				{
					processing++;
				}
			}
		}

		public override void SetSize(Canvas g, Font font, Rectangle _rect, int ox, int _gap, int _gap2)
		{
			Rectangle rECT = (base.RECT_REAL = _rect);
			base.RECT = rECT;
			int num = _rect.X + ox;
			int num2 = _gap / 2;
			int num3 = base.COLUMN.Align switch
			{
				ColumnAlign.Center => num + (_rect.Width - base.MinWidth + _gap) / 2, 
				ColumnAlign.Right => _rect.Right - base.MinWidth + num2, 
				_ => num + num2, 
			};
			for (int i = 0; i < Value.Count; i++)
			{
				ICell cell = Value[i];
				Size size = SIZES[i];
				cell.SetRect(g, font, new Rectangle(num3, _rect.Y, size.Width, _rect.Height), size, num2, _gap);
				num3 += size.Width + num2;
			}
		}

		public override Size GetSize(Canvas g, Font font, int width, int _gap, int _gap2)
		{
			int num = _gap / 2;
			int num2 = 0;
			int num3 = 0;
			List<Size> list = new List<Size>(Value.Count);
			foreach (ICell item in Value)
			{
				Size size = item.GetSize(g, font, num, _gap);
				list.Add(size);
				num2 += size.Width + num;
				if (num3 < size.Height)
				{
					num3 = size.Height;
				}
			}
			base.MinWidth = num2 + num;
			SIZES = list.ToArray();
			return new Size(base.MinWidth, num3);
		}

		public override string? ToString()
		{
			List<string> list = new List<string>(Value.Count);
			foreach (ICell item in Value)
			{
				string text = item.ToString();
				if (text != null && !string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
			}
			return string.Join(" ", list);
		}
	}

	internal class StyleRow
	{
		public RowTemplate row { get; set; }

		public CellStyleInfo? style { get; set; }

		public StyleRow(RowTemplate _row, CellStyleInfo? _style)
		{
			row = _row;
			style = _style;
		}
	}

	internal class AutoWidth
	{
		public int value { get; set; }

		public int minvalue { get; set; }
	}

	internal class MoveHeader
	{
		public Rectangle rect { get; set; }

		public bool MouseDown { get; set; }

		public int x { get; set; }

		public int i { get; set; }

		public int width { get; set; }

		public int min_width { get; set; }

		public MoveHeader(Dictionary<int, MoveHeader> dir, Rectangle r, int index, int w, int min)
		{
			rect = r;
			i = index;
			min_width = min;
			if (dir.TryGetValue(index, out MoveHeader value) && value.MouseDown)
			{
				x = value.x;
				MouseDown = value.MouseDown;
				width = value.width;
			}
			else
			{
				width = w;
			}
		}
	}

	internal class DragHeader
	{
		public int x { get; set; }

		public int xr { get; set; }

		public int i { get; set; }

		public int im { get; set; } = -1;


		public bool last { get; set; }

		public bool hand { get; set; }
	}

	private ColumnCollection? columns;

	private object? dataSource;

	private Color? fore;

	private int _gap = 12;

	private bool fixedHeader = true;

	private bool visibleHeader = true;

	private bool enableHeaderResizing;

	private bool bordered;

	private int radius;

	private int _checksize = 16;

	private int _switchsize = 16;

	private int? rowHeight;

	private int? rowHeightHeader;

	private bool empty = true;

	private string? emptyText;

	private bool emptyHeader;

	private Color? rowHoverBg;

	private Color? rowSelectedBg;

	private Color? rowSelectedFore;

	private Color? borderColor;

	private Font? columnfont;

	private Color? columnback;

	private Color? columnfore;

	private int[] selectedIndex = new int[0];

	[Browsable(false)]
	public ScrollBar ScrollBar;

	private TEditMode editmode;

	private List<int> enableDir = new List<int>();

	private TempTable? dataTmp;

	private bool dataOne = true;

	private bool showFixedColumnL;

	private bool showFixedColumnR;

	private int sFixedR;

	private List<int>? fixedColumnL;

	private List<int>? fixedColumnR;

	private bool inEditMode;

	private string? show_oldrect;

	internal RowTemplate[]? rows;

	internal List<object> rows_Expand = new List<object>();

	private Rectangle[] dividers = new Rectangle[0];

	private Rectangle[] dividerHs = new Rectangle[0];

	private MoveHeader[] moveheaders = new MoveHeader[0];

	private bool has_check;

	private Rectangle rect_read;

	private Rectangle rect_divider;

	private Dictionary<int, int> tmpcol_width = new Dictionary<int, int>(0);

	private ITask? ThreadState;

	internal float AnimationStateValue;

	private float check_radius;

	private float check_border = 1f;

	private CELL? cellMouseDown;

	private int shift_index = -1;

	private LayeredFormSelectDown? subForm;

	private string? oldmove;

	private TooltipForm? tooltipForm;

	private DragHeader? dragHeader;

	private DragHeader? dragBody;

	private int[]? SortHeader;

	private int[]? SortData;

	private bool focused;

	internal static StringFormat stringLeft = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)0);

	internal static StringFormat stringCenter = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	internal static StringFormat stringRight = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)2);

	private static StringFormat stringLeftEllipsis = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private static StringFormat stringCenterEllipsis = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	private static StringFormat stringRightEllipsis = Helper.SF_ALL((StringAlignment)1, (StringAlignment)2);

	private static StringFormat stringLeftN = Helper.SF((StringAlignment)1, (StringAlignment)0);

	private static StringFormat stringCenterN = Helper.SF((StringAlignment)1, (StringAlignment)1);

	private static StringFormat stringRightN = Helper.SF((StringAlignment)1, (StringAlignment)2);

	[Browsable(false)]
	[Description("表格列的配置")]
	[Category("数据")]
	[DefaultValue(null)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ColumnCollection Columns
	{
		get
		{
			if (columns == null)
			{
				columns = new ColumnCollection();
			}
			columns.table = this;
			return columns;
		}
		set
		{
			if (columns != value)
			{
				SortHeader = null;
				columns = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				if (value != null)
				{
					value.table = this;
					OnPropertyChanged("Columns");
				}
			}
		}
	}

	[Browsable(false)]
	[Description("数据数组")]
	[Category("数据")]
	[DefaultValue(null)]
	public object? DataSource
	{
		get
		{
			return dataSource;
		}
		set
		{
			enableDir.Clear();
			dataSource = value;
			SortData = null;
			ScrollBar.Clear();
			ExtractHeaderFixed();
			ExtractData();
			if (LoadLayout())
			{
				((Control)this).Invalidate();
			}
			OnPropertyChanged("DataSource");
		}
	}

	public IRow? this[int index]
	{
		get
		{
			if (dataTmp == null || dataTmp.rows.Length == 0)
			{
				return null;
			}
			if (index < 0 || dataTmp.rows.Length - 1 < index)
			{
				return null;
			}
			return dataTmp.rows[index];
		}
	}

	[Description("文字颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ForeColor
	{
		get
		{
			return fore;
		}
		set
		{
			if (!(fore == value))
			{
				fore = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ForeColor");
			}
		}
	}

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(12)]
	public int Gap
	{
		get
		{
			return _gap;
		}
		set
		{
			if (_gap != value)
			{
				_gap = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Gap");
			}
		}
	}

	[Description("固定表头")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool FixedHeader
	{
		get
		{
			return fixedHeader;
		}
		set
		{
			if (fixedHeader != value)
			{
				fixedHeader = value;
				((Control)this).Invalidate();
				OnPropertyChanged("FixedHeader");
			}
		}
	}

	[Description("显示表头")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool VisibleHeader
	{
		get
		{
			return visibleHeader;
		}
		set
		{
			if (visibleHeader != value)
			{
				visibleHeader = value;
				ScrollBar.RB = !value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("VisibleHeader");
			}
		}
	}

	[Description("手动调整列头宽度")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool EnableHeaderResizing
	{
		get
		{
			return enableHeaderResizing;
		}
		set
		{
			if (enableHeaderResizing != value)
			{
				enableHeaderResizing = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("EnableHeaderResizing");
			}
		}
	}

	[Description("列拖拽排序")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool ColumnDragSort { get; set; }

	[Description("焦点离开清空选中")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool LostFocusClearSelection { get; set; }

	[Description("显示列边框")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Bordered
	{
		get
		{
			return bordered;
		}
		set
		{
			if (bordered != value)
			{
				bordered = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("Bordered");
			}
		}
	}

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(0)]
	public int Radius
	{
		get
		{
			return radius;
		}
		set
		{
			if (radius != value)
			{
				ScrollBar.Radius = (radius = value);
				((Control)this).Invalidate();
				OnPropertyChanged("Radius");
			}
		}
	}

	[Description("复选框大小")]
	[Category("外观")]
	[DefaultValue(16)]
	public int CheckSize
	{
		get
		{
			return _checksize;
		}
		set
		{
			if (_checksize != value)
			{
				_checksize = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("CheckSize");
			}
		}
	}

	[Description("开关大小")]
	[Category("外观")]
	[DefaultValue(16)]
	public int SwitchSize
	{
		get
		{
			return _switchsize;
		}
		set
		{
			if (_switchsize != value)
			{
				_switchsize = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("SwitchSize");
			}
		}
	}

	[Description("树开关按钮大小")]
	[Category("外观")]
	[DefaultValue(16)]
	public int TreeButtonSize { get; set; } = 16;


	[Description("拖拽手柄大小")]
	[Category("外观")]
	[DefaultValue(24)]
	public int DragHandleSize { get; set; } = 24;


	[Description("拖拽手柄图标大小")]
	[Category("外观")]
	[DefaultValue(14)]
	public int DragHandleIconSize { get; set; } = 14;


	[Description("行复制")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool ClipboardCopy { get; set; } = true;


	[Description("列宽自动调整模式")]
	[Category("行为")]
	[DefaultValue(ColumnsMode.Auto)]
	public ColumnsMode AutoSizeColumnsMode { get; set; }

	[Description("行高")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? RowHeight
	{
		get
		{
			return rowHeight;
		}
		set
		{
			if (rowHeight != value)
			{
				rowHeight = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("RowHeight");
			}
		}
	}

	[Description("表头行高")]
	[Category("外观")]
	[DefaultValue(null)]
	public int? RowHeightHeader
	{
		get
		{
			return rowHeightHeader;
		}
		set
		{
			if (rowHeightHeader != value)
			{
				rowHeightHeader = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("RowHeightHeader");
			}
		}
	}

	[Description("是否显示空样式")]
	[Category("外观")]
	[DefaultValue(true)]
	public bool Empty
	{
		get
		{
			return empty;
		}
		set
		{
			if (empty != value)
			{
				empty = value;
				((Control)this).Invalidate();
				OnPropertyChanged("Empty");
			}
		}
	}

	[Description("数据为空显示文字")]
	[Category("外观")]
	[DefaultValue(null)]
	public string? EmptyText
	{
		get
		{
			return emptyText;
		}
		set
		{
			if (!(emptyText == value))
			{
				emptyText = value;
				((Control)this).Invalidate();
				OnPropertyChanged("EmptyText");
			}
		}
	}

	[Description("数据为空显示图片")]
	[Category("外观")]
	[DefaultValue(null)]
	public Image? EmptyImage { get; set; }

	[Description("空是否显示表头")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool EmptyHeader
	{
		get
		{
			return emptyHeader;
		}
		set
		{
			if (emptyHeader != value)
			{
				emptyHeader = value;
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("EmptyHeader");
			}
		}
	}

	[Description("默认是否展开")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool DefaultExpand { get; set; }

	[Description("表格行悬浮背景色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? RowHoverBg
	{
		get
		{
			return rowHoverBg;
		}
		set
		{
			if (!(rowHoverBg == value))
			{
				rowHoverBg = value;
				OnPropertyChanged("RowHoverBg");
			}
		}
	}

	[Description("表格行选中背景色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? RowSelectedBg
	{
		get
		{
			return rowSelectedBg;
		}
		set
		{
			if (!(rowSelectedBg == value))
			{
				rowSelectedBg = value;
				if (selectedIndex.Length != 0)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("RowSelectedBg");
			}
		}
	}

	[Description("表格行选中字色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? RowSelectedFore
	{
		get
		{
			return rowSelectedFore;
		}
		set
		{
			if (!(rowSelectedFore == value))
			{
				rowSelectedFore = value;
				if (selectedIndex.Length != 0)
				{
					((Control)this).Invalidate();
				}
				OnPropertyChanged("RowSelectedFore");
			}
		}
	}

	[Description("表格边框颜色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			if (!(borderColor == value))
			{
				borderColor = value;
				((Control)this).Invalidate();
				OnPropertyChanged("BorderColor");
			}
		}
	}

	[Description("表头字体")]
	[Category("外观")]
	[DefaultValue(null)]
	public Font? ColumnFont
	{
		get
		{
			return columnfont;
		}
		set
		{
			if (columnfont != value)
			{
				columnfont = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ColumnFont");
			}
		}
	}

	[Description("表头背景色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ColumnBack
	{
		get
		{
			return columnback;
		}
		set
		{
			if (!(columnback == value))
			{
				columnback = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ColumnBack");
			}
		}
	}

	[Description("表头文本色")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? ColumnFore
	{
		get
		{
			return columnfore;
		}
		set
		{
			if (!(columnfore == value))
			{
				columnfore = value;
				((Control)this).Invalidate();
				OnPropertyChanged("ColumnFore");
			}
		}
	}

	[Browsable(false)]
	[Description("选中行（1开始）")]
	[Category("数据")]
	[DefaultValue(-1)]
	public int SelectedIndex
	{
		get
		{
			if (selectedIndex.Length != 0)
			{
				return selectedIndex[0];
			}
			return -1;
		}
		set
		{
			if (SetIndex(value))
			{
				((Control)this).Invalidate();
				OnPropertyChanged("SelectedIndex");
				this.SelectIndexChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor", typeof(UITypeEditor))]
	[Browsable(false)]
	[Description("选中多行")]
	[Category("数据")]
	public int[] SelectedIndexs
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			if (selectedIndex != value)
			{
				selectedIndex = value;
				((Control)this).Invalidate();
				OnPropertyChanged("SelectedIndexs");
				this.SelectIndexChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	[Description("多选行")]
	[Category("行为")]
	[DefaultValue(false)]
	public bool MultipleRows { get; set; }

	[Description("处理快捷键")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool HandShortcutKeys { get; set; } = true;


	[Description("省略文字提示")]
	[Category("行为")]
	[DefaultValue(true)]
	public bool ShowTip { get; set; } = true;


	[Description("编辑模式")]
	[Category("行为")]
	[DefaultValue(TEditMode.None)]
	public TEditMode EditMode
	{
		get
		{
			return editmode;
		}
		set
		{
			if (editmode != value)
			{
				editmode = value;
				((Control)this).Invalidate();
				OnPropertyChanged("EditMode");
			}
		}
	}

	[Description("Checked 属性值更改时发生")]
	[Category("行为")]
	public event CheckEventHandler? CheckedChanged;

	[Description("全局 CheckState 属性值更改时发生")]
	[Category("行为")]
	public event CheckStateEventHandler? CheckedOverallChanged;

	[Description("单击时发生")]
	[Category("行为")]
	public event ClickEventHandler? CellClick;

	[Description("单击按钮时发生")]
	[Category("行为")]
	public event ClickButtonEventHandler? CellButtonClick;

	[Description("双击时发生")]
	[Category("行为")]
	public event ClickEventHandler? CellDoubleClick;

	[Description("编辑前发生")]
	[Category("行为")]
	public event BeginEditEventHandler? CellBeginEdit;

	[Description("编辑前文本框样式发生")]
	[Category("行为")]
	public event BeginEditInputStyleEventHandler? CellBeginEditInputStyle;

	[Description("编辑后发生")]
	[Category("行为")]
	public event EndEditEventHandler? CellEndEdit;

	[Description("编辑完成后发生")]
	[Category("行为")]
	public event EventHandler? CellEditComplete;

	public event SetRowStyleEventHandler? SetRowStyle;

	[Description("行排序时发生")]
	[Category("行为")]
	public event IntEventHandler? SortRows;

	[Description("点击排序后发生")]
	[Category("行为")]
	public event SortModeEventHandler? SortModeChanged;

	[Description("选中变化后发生")]
	[Category("行为")]
	public event EventHandler? SelectIndexChanged;

	[Description("自定义排序")]
	[Category("行为")]
	public event Comparison<string>? CustomSort;

	private bool SetIndex(int value)
	{
		if (selectedIndex.Length != 0)
		{
			if (selectedIndex[0] == value)
			{
				return false;
			}
		}
		else if (value == -1)
		{
			return false;
		}
		selectedIndex = new int[1] { value };
		return true;
	}

	private int[] SetIndexs(int value)
	{
		List<int> list = new List<int>(selectedIndex.Length + 1);
		list.AddRange(selectedIndex);
		if (list.Contains(value))
		{
			list.Remove(value);
		}
		else
		{
			list.Add(value);
		}
		return list.ToArray();
	}

	private int[] SetIndexs(int start, int end)
	{
		List<int> list = new List<int>(end - start + 1);
		for (int i = start; i <= end; i++)
		{
			list.Add(i);
		}
		return list.ToArray();
	}

	public Table()
		: base(ControlType.Select)
	{
		ScrollBar = new ScrollBar(this, enabledY: true, enabledX: true, radius, !visibleHeader);
	}

	protected override void Dispose(bool disposing)
	{
		ThreadState?.Dispose();
		ScrollBar.Dispose();
		base.Dispose(disposing);
	}

	public bool GetRowEnable(int i)
	{
		if (enableDir.Contains(i))
		{
			return false;
		}
		return true;
	}

	public void SetRowEnable(int i, bool value = true, bool ui = true)
	{
		if (value)
		{
			if (!enableDir.Contains(i))
			{
				return;
			}
			enableDir.Remove(i);
		}
		else
		{
			if (enableDir.Contains(i))
			{
				return;
			}
			enableDir.Add(i);
		}
		if (rows == null)
		{
			return;
		}
		try
		{
			rows[i + 1].ENABLE = value;
			if (ui)
			{
				((Control)this).Invalidate();
			}
		}
		catch
		{
		}
	}

	public int ScrollLine(int i, bool force = false)
	{
		if (rows == null || !ScrollBar.ShowY)
		{
			return 0;
		}
		return ScrollLine(i, rows, force);
	}

	private int ScrollLine(int i, RowTemplate[] rows, bool force = false)
	{
		if (!ScrollBar.ShowY)
		{
			return 0;
		}
		RowTemplate rowTemplate = rows[i];
		int valueY = ScrollBar.ValueY;
		if (force)
		{
			if (fixedHeader)
			{
				ScrollBar.ValueY = rows[i].RECT.Y - rows[0].RECT.Height;
			}
			else
			{
				ScrollBar.ValueY = rows[i].RECT.Y;
			}
			return valueY - ScrollBar.ValueY;
		}
		if (visibleHeader && fixedHeader)
		{
			if (rowTemplate.RECT.Y - rows[0].RECT.Height < valueY || rowTemplate.RECT.Bottom > valueY + rect_read.Height)
			{
				if (fixedHeader)
				{
					ScrollBar.ValueY = rows[i].RECT.Y - rows[0].RECT.Height;
				}
				else
				{
					ScrollBar.ValueY = rows[i].RECT.Y;
				}
				return valueY - ScrollBar.ValueY;
			}
		}
		else if (rowTemplate.RECT.Y < valueY || rowTemplate.RECT.Bottom > valueY + rect_read.Height)
		{
			if (fixedHeader)
			{
				ScrollBar.ValueY = rows[i].RECT.Y - rows[0].RECT.Height;
			}
			else
			{
				ScrollBar.ValueY = rows[i].RECT.Y;
			}
			return valueY - ScrollBar.ValueY;
		}
		return 0;
	}

	public bool CopyData(int row)
	{
		if (rows != null)
		{
			try
			{
				RowTemplate obj = rows[row];
				List<string> list = new List<string>(obj.cells.Length);
				CELL[] cells = obj.cells;
				foreach (CELL cELL in cells)
				{
					list.Add(cELL.ToString());
				}
				((Control)(object)this).ClipboardSetText(string.Join("\t", list));
				return true;
			}
			catch
			{
			}
		}
		return false;
	}

	public bool CopyData(int[] row)
	{
		if (rows != null)
		{
			try
			{
				List<string> list = new List<string>(row.Length);
				foreach (int num in row)
				{
					RowTemplate obj = rows[num];
					List<string> list2 = new List<string>(obj.cells.Length);
					CELL[] cells = obj.cells;
					foreach (CELL cELL in cells)
					{
						list2.Add(cELL.ToString());
					}
					list.Add(string.Join("\t", list2));
				}
				((Control)(object)this).ClipboardSetText(string.Join("\n", list));
				return true;
			}
			catch
			{
			}
		}
		return false;
	}

	public bool CopyData(int row, int column)
	{
		if (rows != null)
		{
			try
			{
				string text = rows[row].cells[column].ToString();
				if (text == null)
				{
					return false;
				}
				((Control)(object)this).ClipboardSetText(text);
				return true;
			}
			catch
			{
			}
		}
		return false;
	}

	public int[] SortIndex()
	{
		if (SortData == null)
		{
			if (dataTmp == null || dataTmp.rows.Length == 0)
			{
				return new int[0];
			}
			int[] array = new int[dataTmp.rows.Length];
			for (int i = 0; i < dataTmp.rows.Length; i++)
			{
				array[i] = i;
			}
			return array;
		}
		return SortData;
	}

	public object[] SortList()
	{
		if (dataTmp == null || dataTmp.rows.Length == 0)
		{
			return new object[0];
		}
		if (SortData == null)
		{
			object[] array = new object[dataTmp.rows.Length];
			for (int i = 0; i < dataTmp.rows.Length; i++)
			{
				array[i] = dataTmp.rows[i].record;
			}
			return array;
		}
		List<object> list = new List<object>(dataTmp.rows.Length);
		int[] sortData = SortData;
		foreach (int num in sortData)
		{
			list.Add(dataTmp.rows[num].record);
		}
		return list.ToArray();
	}

	public int[] SortColumnsIndex()
	{
		if (SortHeader == null)
		{
			if (columns == null || columns.Count == 0)
			{
				return new int[0];
			}
			int[] array = new int[columns.Count];
			for (int i = 0; i < columns.Count; i++)
			{
				array[i] = i;
			}
			return array;
		}
		return SortHeader;
	}

	public Column[] SortColumnsList()
	{
		if (columns == null || columns.Count == 0)
		{
			return new Column[0];
		}
		if (SortHeader == null)
		{
			Column[] array = new Column[columns.Count];
			for (int i = 0; i < columns.Count; i++)
			{
				array[i] = columns[i];
			}
			return array;
		}
		List<Column> list = new List<Column>(columns.Count);
		int[] sortHeader = SortHeader;
		foreach (int index in sortHeader)
		{
			list.Add(columns[index]);
		}
		return list.ToArray();
	}

	public DataTable? ToDataTable(bool enableRender = true, bool toString = true)
	{
		if (dataTmp == null)
		{
			return null;
		}
		DataTable dataTable = new DataTable();
		Dictionary<string, Column> dictionary;
		if (rows == null)
		{
			dictionary = new Dictionary<string, Column>(0);
			TempiColumn[] array = dataTmp.columns;
			foreach (TempiColumn tempiColumn in array)
			{
				dataTable.Columns.Add(new DataColumn(tempiColumn.key)
				{
					Caption = tempiColumn.text
				});
			}
		}
		else
		{
			dictionary = new Dictionary<string, Column>(dataTmp.columns.Length);
			Dictionary<string, DataColumn> dictionary2 = new Dictionary<string, DataColumn>(dataTmp.columns.Length);
			TempiColumn[] array = dataTmp.columns;
			foreach (TempiColumn tempiColumn2 in array)
			{
				dictionary2.Add(tempiColumn2.key, new DataColumn(tempiColumn2.key)
				{
					Caption = tempiColumn2.text
				});
			}
			CELL[] cells = rows[0].cells;
			for (int i = 0; i < cells.Length; i++)
			{
				TCellColumn tCellColumn = (TCellColumn)cells[i];
				dictionary.Add(tCellColumn.COLUMN.Key, tCellColumn.COLUMN);
				if (!string.IsNullOrWhiteSpace(tCellColumn.value) && dictionary2.TryGetValue(tCellColumn.COLUMN.Key, out var value))
				{
					value.Caption = tCellColumn.value;
				}
			}
			foreach (KeyValuePair<string, DataColumn> item in dictionary2)
			{
				dataTable.Columns.Add(item.Value);
			}
		}
		if (toString)
		{
			IRow[] array2 = dataTmp.rows;
			foreach (IRow row in array2)
			{
				List<object> list = new List<object>(row.cells.Count);
				foreach (KeyValuePair<string, object> cell in row.cells)
				{
					object obj = row[cell.Key];
					if (enableRender && dictionary.TryGetValue(cell.Key, out var value2) && value2.Render != null)
					{
						obj = value2.Render(obj, row.record, row.i);
					}
					if (obj is IList<ICell> list2)
					{
						List<string> list3 = new List<string>(list2.Count);
						foreach (ICell item2 in list2)
						{
							string text = item2.ToString();
							if (text != null)
							{
								list3.Add(text);
							}
						}
						if (list3.Count > 0)
						{
							list.Add(string.Join(" ", list3));
						}
						else
						{
							list.Add(null);
						}
					}
					else
					{
						list.Add(obj);
					}
				}
				dataTable.Rows.Add(list.ToArray());
			}
		}
		else
		{
			IRow[] array2 = dataTmp.rows;
			foreach (IRow row2 in array2)
			{
				List<object> list4 = new List<object>(row2.cells.Count);
				foreach (KeyValuePair<string, object> cell2 in row2.cells)
				{
					object obj2 = row2[cell2.Key];
					if (enableRender && dictionary.TryGetValue(cell2.Key, out var value3) && value3.Render != null)
					{
						obj2 = value3.Render(obj2, row2.record, row2.i);
					}
					list4.Add(obj2);
				}
				dataTable.Rows.Add(list4.ToArray());
			}
		}
		return dataTable;
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		((Control)(object)this).AddListener();
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.LANG && ColumnsHasLocalization() && LoadLayout())
		{
			((Control)this).Invalidate();
		}
	}

	private bool ColumnsHasLocalization()
	{
		if (columns == null)
		{
			return false;
		}
		foreach (Column column in columns)
		{
			if (column.LocalizationTitle != null)
			{
				return true;
			}
		}
		return false;
	}

	private void ExtractData()
	{
		dataOne = true;
		dataTmp = null;
		if (columns != null)
		{
			foreach (Column column in columns)
			{
				if (column is ColumnCheck columnCheck)
				{
					columnCheck.CheckState = (CheckState)0;
				}
			}
		}
		if (dataSource == null)
		{
			rows_Expand.Clear();
			ScrollBar scrollBar = ScrollBar;
			int valueX = (ScrollBar.ValueY = 0);
			scrollBar.ValueX = valueX;
		}
		else if (dataSource is DataTable dataTable)
		{
			if (dataTable.Columns.Count <= 0 || dataTable.Rows.Count <= 0)
			{
				return;
			}
			List<TempiColumn> list = new List<TempiColumn>(dataTable.Columns.Count);
			List<IRow> list2 = new List<IRow>(dataTable.Rows.Count + 1);
			for (int i = 0; i < dataTable.Columns.Count; i++)
			{
				list.Add(new TempiColumn(i, dataTable.Columns[i]));
			}
			for (int j = 0; j < dataTable.Rows.Count; j++)
			{
				DataRow dataRow = dataTable.Rows[j];
				if (dataRow == null)
				{
					continue;
				}
				Dictionary<string, object> dictionary = new Dictionary<string, object>(list.Count);
				foreach (TempiColumn item in list)
				{
					dictionary.Add(item.key, dataRow[item.key]);
				}
				if (dictionary.Count > 0)
				{
					list2.Add(new IRow(j, dataRow, dictionary));
				}
			}
			dataTmp = new TempTable(list.ToArray(), list2.ToArray());
		}
		else if (dataSource is IList list3)
		{
			TempiColumn[] array = new TempiColumn[0];
			List<IRow> list4 = new List<IRow>(list3.Count + 1);
			for (int k = 0; k < list3.Count; k++)
			{
				GetRowAuto(ref list4, list3[k], k, ref array);
			}
			dataTmp = new TempTable(array, list4.ToArray());
		}
	}

	public void Binding<T>(AntList<T> list)
	{
		AntList<T> list2 = list;
		dataOne = true;
		if (list2 == null)
		{
			return;
		}
		dataSource = list2;
		list2.action = delegate(string code, object obj)
		{
			if (dataTmp != null)
			{
				try
				{
					switch (code)
					{
					case "add":
						if (obj is int num2)
						{
							T val = list2[num2];
							if (val != null)
							{
								Dictionary<string, object> row = GetRow(val, dataTmp.columns.Length);
								if (row.Count != 0)
								{
									int num3 = dataTmp.rows.Length + 1;
									if (num3 >= num2)
									{
										List<IRow> list4 = new List<IRow>(num3);
										list4.AddRange(dataTmp.rows);
										list4.Insert(num2, new IRow(num2, val, row));
										dataTmp.rows = ChangeList(list4);
										if (LoadLayout())
										{
											((Control)this).Invalidate();
										}
									}
								}
							}
						}
						else if (obj is int[] array)
						{
							List<IRow> list5 = new List<IRow>(array.Length);
							int[] array2 = array;
							foreach (int index in array2)
							{
								T val2 = list2[index];
								if (val2 != null)
								{
									Dictionary<string, object> row2 = GetRow(val2, dataTmp.columns.Length);
									if (row2.Count > 0)
									{
										list5.Add(new IRow(index, val2, row2));
									}
								}
							}
							List<IRow> list6 = new List<IRow>(dataTmp.rows.Length + list5.Count);
							list6.AddRange(dataTmp.rows);
							list6.InsertRange(array[0], list5);
							dataTmp.rows = ChangeList(list6);
							if (LoadLayout())
							{
								((Control)this).Invalidate();
							}
						}
						break;
					case "edit":
						if (LoadLayout())
						{
							((Control)this).Invalidate();
						}
						break;
					case "del":
						if (obj is int num)
						{
							if (num >= 0 && num < dataTmp.rows.Length)
							{
								List<IRow> list3 = new List<IRow>(dataTmp.rows.Length);
								list3.AddRange(dataTmp.rows);
								list3.RemoveAt(num);
								dataTmp.rows = ChangeList(list3);
								if (LoadLayout())
								{
									((Control)this).Invalidate();
								}
							}
						}
						else if (obj is string)
						{
							dataTmp.rows = new IRow[0];
							if (LoadLayout())
							{
								((Control)this).Invalidate();
							}
						}
						break;
					}
				}
				catch
				{
					IBinding(list2);
				}
			}
		};
		IBinding(list2);
	}

	private void IBinding<T>(AntList<T> list)
	{
		ExtractHeaderFixed();
		TempiColumn[] array = new TempiColumn[0];
		List<IRow> list2 = new List<IRow>(list.Count + 1);
		for (int i = 0; i < list.Count; i++)
		{
			GetRowAuto(ref list2, list[i], i, ref array);
		}
		dataTmp = new TempTable(array, list2.ToArray());
		LoadLayout();
		((Control)this).Invalidate();
	}

	public void Binding<T>(BindingList<T> list)
	{
		dataOne = true;
		if (list != null)
		{
			list.ListChanged += delegate(object Sender, ListChangedEventArgs Args)
			{
				BindingItem<T>(Sender, Args);
			};
			DataSource = list;
		}
	}

	private void BindingItem<T>(object? sender, ListChangedEventArgs args)
	{
		switch (args.ListChangedType)
		{
		case ListChangedType.ItemAdded:
			BindingItemAdded<T>(sender, args.NewIndex);
			break;
		case ListChangedType.ItemDeleted:
			BindingItemDeleted<T>(sender, args.NewIndex);
			break;
		case ListChangedType.ItemChanged:
			BindingItemChanged<T>(sender, args.NewIndex);
			break;
		case ListChangedType.Reset:
			if (dataTmp != null)
			{
				dataTmp.rows = new IRow[0];
				if (LoadLayout())
				{
					((Control)this).Invalidate();
				}
			}
			break;
		default:
			if (sender is IList<T> list)
			{
				DataSource = list;
			}
			break;
		}
	}

	private void BindingItemAdded<T>(object? sender, int i)
	{
		if (dataTmp == null || !(sender is IList<T> list))
		{
			return;
		}
		T val = list[i];
		if (val == null)
		{
			return;
		}
		Dictionary<string, object> row = GetRow(val, dataTmp.columns.Length);
		if (row.Count == 0)
		{
			return;
		}
		int num = dataTmp.rows.Length + 1;
		if (num >= i)
		{
			List<IRow> list2 = new List<IRow>(num);
			list2.AddRange(dataTmp.rows);
			list2.Insert(i, new IRow(i, val, row));
			dataTmp.rows = ChangeList(list2);
			if (LoadLayout())
			{
				((Control)this).Invalidate();
			}
		}
	}

	private void BindingItemChanged<T>(object? sender, int i)
	{
		if (dataTmp == null || !(sender is IList<T> list))
		{
			return;
		}
		T val = list[i];
		if (val == null)
		{
			return;
		}
		Dictionary<string, object> row = GetRow(val, dataTmp.columns.Length);
		if (row.Count != 0)
		{
			List<IRow> list2 = new List<IRow>(dataTmp.rows.Length);
			list2.AddRange(dataTmp.rows);
			list2[i] = new IRow(i, val, row);
			dataTmp.rows = ChangeList(list2);
			if (LoadLayout())
			{
				((Control)this).Invalidate();
			}
		}
	}

	private void BindingItemDeleted<T>(object? sender, int i)
	{
		if (dataTmp != null && sender is IList<T>)
		{
			List<IRow> list = new List<IRow>(dataTmp.rows.Length);
			list.AddRange(dataTmp.rows);
			list.RemoveAt(i);
			dataTmp.rows = ChangeList(list);
			if (LoadLayout())
			{
				((Control)this).Invalidate();
			}
		}
	}

	private IRow[] ChangeList(List<IRow> rows)
	{
		for (int i = 0; i < rows.Count; i++)
		{
			rows[i].i = i;
		}
		return rows.ToArray();
	}

	private void GetRowAuto(ref List<IRow> rows, object? row, int i, ref TempiColumn[] columns)
	{
		if (row == null)
		{
			return;
		}
		if (row is IList<AntItem> list)
		{
			IRow item = new IRow(i, row, list);
			rows.Add(item);
			if (columns.Length == 0)
			{
				columns = new TempiColumn[list.Count];
				for (int j = 0; j < list.Count; j++)
				{
					columns[j] = new TempiColumn(j, list[j].key);
				}
			}
		}
		else
		{
			Dictionary<string, object> rowAuto = GetRowAuto(row, ref columns);
			if (rowAuto.Count > 0)
			{
				rows.Add(new IRow(i, row, rowAuto));
			}
		}
	}

	private Dictionary<string, object?> GetRowAuto(object row, ref TempiColumn[] columns)
	{
		if (columns.Length == 0)
		{
			return GetRow(row, out columns);
		}
		return GetRow(row, columns.Length);
	}

	private Dictionary<string, object?> GetRow(object row, int len)
	{
		if (row is IList<AntItem> list)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>(list.Count);
			{
				foreach (AntItem item in list)
				{
					dictionary.Add(item.key, item);
				}
				return dictionary;
			}
		}
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>(len);
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(row))
		{
			dictionary2.Add(property.Name, property);
		}
		return dictionary2;
	}

	private Dictionary<string, object?> GetRow(object row, out TempiColumn[] _columns)
	{
		PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(row);
		Dictionary<string, object> dictionary = new Dictionary<string, object>(properties.Count);
		int num = 0;
		List<TempiColumn> list = new List<TempiColumn>(properties.Count);
		foreach (PropertyDescriptor item in properties)
		{
			list.Add(new TempiColumn(num, item.Name));
			num++;
			dictionary.Add(item.Name, item);
		}
		_columns = list.ToArray();
		return dictionary;
	}

	internal void ExtractHeaderFixed()
	{
		if (columns == null)
		{
			fixedColumnL = (fixedColumnR = null);
			return;
		}
		List<int> list = new List<int>();
		int num = 0;
		foreach (Column column in columns)
		{
			if (column.Visible)
			{
				if (column.Fixed)
				{
					list.Add(num);
				}
				num++;
			}
		}
		if (list.Count > 0)
		{
			List<int> list2 = new List<int>();
			List<int> list3 = new List<int>();
			foreach (int item in list)
			{
				if (item == 0)
				{
					list2.Add(item);
				}
				else if (list2.Count > 0 && list2[list2.Count - 1] + 1 == item)
				{
					list2.Add(item);
				}
			}
			foreach (int item2 in list2)
			{
				list.Remove(item2);
			}
			if (list.Count > 0)
			{
				list.Reverse();
				foreach (int item3 in list)
				{
					if (item3 == num - 1)
					{
						list3.Add(item3);
					}
					else if (list3.Count > 0 && list3[list3.Count - 1] - 1 == item3)
					{
						list3.Add(item3);
					}
				}
			}
			if (list2.Count > 0)
			{
				fixedColumnL = list2;
			}
			else
			{
				fixedColumnL = null;
			}
			if (list3.Count > 0)
			{
				fixedColumnR = list3;
			}
			else
			{
				fixedColumnR = null;
			}
		}
		else
		{
			fixedColumnL = (fixedColumnR = null);
		}
	}

	public bool EnterEditMode(int row, int column)
	{
		if (rows != null)
		{
			try
			{
				RowTemplate rowTemplate = rows[row];
				CELL cell = rowTemplate.cells[column];
				EditModeClose();
				if (CanEditMode(rowTemplate, cell))
				{
					ScrollLine(row, rows);
					if (showFixedColumnL && fixedColumnL != null && fixedColumnL.Contains(column))
					{
						OnEditMode(rowTemplate, cell, row, column, 0, ScrollBar.ValueY);
					}
					else if (showFixedColumnR && fixedColumnR != null && fixedColumnR.Contains(column))
					{
						OnEditMode(rowTemplate, cell, row, column, sFixedR, ScrollBar.ValueY);
					}
					else
					{
						OnEditMode(rowTemplate, cell, row, column, ScrollBar.ValueX, ScrollBar.ValueY);
					}
					return true;
				}
			}
			catch
			{
			}
		}
		return false;
	}

	public void EditModeClose()
	{
		if (!inEditMode)
		{
			return;
		}
		ScrollBar.OnInvalidate = null;
		if (!focused)
		{
			if (((Control)this).InvokeRequired)
			{
				((Control)this).Invoke((Delegate)(Action)delegate
				{
					((Control)this).Focus();
				});
			}
			else
			{
				((Control)this).Focus();
			}
		}
		inEditMode = false;
	}

	private bool CanEditMode(RowTemplate it, CELL cell)
	{
		if (rows == null)
		{
			return false;
		}
		if (cell is TCellText)
		{
			return true;
		}
		if (cell is Template template)
		{
			foreach (ICell item in template.Value)
			{
				if (item is CellText)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void OnEditMode(RowTemplate it, CELL cell, int i_row, int i_col, int sx, int sy)
	{
		CELL cell2 = cell;
		RowTemplate it2 = it;
		if (rows == null)
		{
			return;
		}
		if (it2.AnimationHover)
		{
			it2.ThreadHover?.Dispose();
			it2.ThreadHover = null;
		}
		bool multiline = cell2.COLUMN.LineBreak;
		TCellText cellText = cell2 as TCellText;
		if (cellText != null)
		{
			object value2 = null;
			if (cell2.PROPERTY != null && cell2.VALUE != null)
			{
				value2 = cell2.PROPERTY.GetValue(cell2.VALUE);
			}
			else if (cell2.VALUE is AntItem antItem)
			{
				value2 = antItem.value;
			}
			else
			{
				value2 = cell2.VALUE;
			}
			bool flag = true;
			if (this.CellBeginEdit != null)
			{
				flag = this.CellBeginEdit(this, new TableEventArgs(value2, it2.RECORD, i_row, i_col));
			}
			if (!flag)
			{
				return;
			}
			inEditMode = true;
			ScrollBar.OnInvalidate = delegate
			{
				EditModeClose();
			};
			((Control)this).BeginInvoke((Delegate)(Action)delegate
			{
				for (int j = 0; j < rows.Length; j++)
				{
					rows[j].hover = j == i_row;
				}
				int gap2 = (int)((float)Math.Max(_gap, 8) * Config.Dpi);
				int num4 = Helper.GDI((Canvas g) => g.MeasureString(value2?.ToString(), ((Control)this).Font, cell2.RECT_REAL.Width).Height + gap2);
				int num5 = cell2.RECT_REAL.Height + gap2;
				int num6 = (multiline ? cell2.RECT.Height : ((num4 > num5) ? num4 : num5));
				if (cell2.RECT_REAL.Height == cell2.RECT.Height && num6 > cell2.RECT.Height)
				{
					num6 = cell2.RECT.Height;
				}
				Input input2 = ShowInput(cell2, sx, sy, num6, multiline, value2, delegate(string _value)
				{
					bool flag4 = true;
					if (this.CellEndEdit != null)
					{
						flag4 = this.CellEndEdit(this, new TableEndEditEventArgs(_value, it2.RECORD, i_row, i_col));
					}
					if (flag4 && GetValue(value2, _value, out object read2))
					{
						cellText.value = _value;
						if (it2.RECORD is DataRow dataRow2)
						{
							TCellText tCellText = cellText;
							string vALUE = (cellText.value = _value);
							tCellText.VALUE = vALUE;
							dataRow2[i_col] = read2;
						}
						else
						{
							SetValue(cell2, read2);
						}
						if (multiline)
						{
							LoadLayout();
						}
						this.CellEditComplete?.Invoke(this, EventArgs.Empty);
					}
				});
				if (cellText.COLUMN.Align == ColumnAlign.Center)
				{
					input2.TextAlign = (HorizontalAlignment)2;
				}
				else if (cellText.COLUMN.Align == ColumnAlign.Right)
				{
					input2.TextAlign = (HorizontalAlignment)1;
				}
				this.CellBeginEditInputStyle?.Invoke(this, new TableBeginEditInputStyleEventArgs(value2, it2.RECORD, i_row, i_col, ref input2));
				((Control)this).Controls.Add((Control)(object)input2);
				((Control)input2).Focus();
			});
		}
		else
		{
			if (!(cell2 is Template template2))
			{
				return;
			}
			foreach (ICell template in template2.Value)
			{
				CellText text = template as CellText;
				if (text == null)
				{
					continue;
				}
				object value = null;
				if (cell2.PROPERTY != null && cell2.VALUE != null)
				{
					value = cell2.PROPERTY.GetValue(cell2.VALUE);
				}
				else if (cell2.VALUE is AntItem antItem2)
				{
					value = antItem2.value;
				}
				else
				{
					value = cell2.VALUE;
				}
				bool flag2 = true;
				if (this.CellBeginEdit != null)
				{
					flag2 = this.CellBeginEdit(this, new TableEventArgs(value, it2.RECORD, i_row, i_col));
				}
				if (!flag2)
				{
					break;
				}
				inEditMode = true;
				ScrollBar.OnInvalidate = delegate
				{
					EditModeClose();
				};
				((Control)this).BeginInvoke((Delegate)(Action)delegate
				{
					for (int i = 0; i < rows.Length; i++)
					{
						rows[i].hover = i == i_row;
					}
					int gap = (int)((float)Math.Max(_gap, 8) * Config.Dpi);
					int num = Helper.GDI((Canvas g) => g.MeasureString(value?.ToString(), ((Control)this).Font, cell2.RECT_REAL.Width).Height + gap);
					int num2 = cell2.RECT_REAL.Height + gap;
					int num3 = (multiline ? cell2.RECT.Height : ((num > num2) ? num : num2));
					if (cell2.RECT_REAL.Height == cell2.RECT.Height && num3 > cell2.RECT.Height)
					{
						num3 = cell2.RECT.Height;
					}
					Input input = ShowInput(cell2, sx, sy, num3, multiline, value, delegate(string _value)
					{
						bool flag3 = true;
						if (this.CellEndEdit != null)
						{
							flag3 = this.CellEndEdit(this, new TableEndEditEventArgs(_value, it2.RECORD, i_row, i_col));
						}
						if (flag3)
						{
							if (value is CellText cellText2)
							{
								cellText2.Text = _value;
								SetValue(cell2, cellText2);
							}
							else
							{
								text.Text = _value;
								if (GetValue(value, _value, out object read))
								{
									if (it2.RECORD is DataRow dataRow)
									{
										dataRow[i_col] = read;
									}
									else
									{
										SetValue(cell2, read);
									}
								}
							}
							this.CellEditComplete?.Invoke(this, EventArgs.Empty);
						}
					});
					this.CellBeginEditInputStyle?.Invoke(this, new TableBeginEditInputStyleEventArgs(value, it2.RECORD, i_row, i_col, ref input));
					if (template.PARENT.COLUMN.Align == ColumnAlign.Center)
					{
						input.TextAlign = (HorizontalAlignment)2;
					}
					else if (template.PARENT.COLUMN.Align == ColumnAlign.Right)
					{
						input.TextAlign = (HorizontalAlignment)1;
					}
					((Control)this).Controls.Add((Control)(object)input);
					((Control)input).Focus();
				});
				break;
			}
		}
	}

	private bool GetValue(object? value, string _value, out object read)
	{
		if (value is int)
		{
			if (int.TryParse(_value, out var result))
			{
				read = result;
				return true;
			}
		}
		else if (value is double)
		{
			if (double.TryParse(_value, out var result2))
			{
				read = result2;
				return true;
			}
		}
		else if (value is decimal)
		{
			if (decimal.TryParse(_value, out var result3))
			{
				read = result3;
				return true;
			}
		}
		else
		{
			if (!(value is float))
			{
				read = _value;
				return true;
			}
			if (float.TryParse(_value, out var result4))
			{
				read = result4;
				return true;
			}
		}
		read = _value;
		return false;
	}

	private Input ShowInput(CELL cell, int sx, int sy, int height, bool multiline, object? value, Action<string> call)
	{
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Expected O, but got Unknown
		Action<string> call2 = call;
		Input input;
		if (value is CellText cellText)
		{
			Input obj = new Input
			{
				Multiline = multiline
			};
			((Control)obj).Location = new Point(cell.RECT.X - sx, cell.RECT.Y - sy + (cell.RECT.Height - height) / 2);
			((Control)obj).Size = new Size(cell.RECT.Width, height);
			((Control)obj).Text = cellText.Text ?? "";
			input = obj;
		}
		else
		{
			Input obj2 = new Input
			{
				Multiline = multiline
			};
			((Control)obj2).Location = new Point(cell.RECT.X - sx, cell.RECT.Y - sy + (cell.RECT.Height - height) / 2);
			((Control)obj2).Size = new Size(cell.RECT.Width, height);
			((Control)obj2).Text = value?.ToString() ?? "";
			input = obj2;
		}
		string old_text = ((Control)input).Text;
		bool isone = true;
		((Control)input).KeyPress += (KeyPressEventHandler)delegate(object a, KeyPressEventArgs b)
		{
			Input input3 = a as Input;
			if (input3 != null && isone && b.KeyChar == '\r')
			{
				isone = false;
				b.Handled = true;
				ScrollBar.OnInvalidate = null;
				if (old_text != ((Control)input3).Text)
				{
					call2(((Control)input3).Text);
				}
				inEditMode = false;
				((Component)(object)input3).Dispose();
			}
		};
		((Control)input).LostFocus += delegate(object a, EventArgs b)
		{
			Input input2 = a as Input;
			if (input2 != null && isone)
			{
				isone = false;
				ScrollBar.OnInvalidate = null;
				if (old_text != ((Control)input2).Text)
				{
					call2(((Control)input2).Text);
				}
				inEditMode = false;
				((Component)(object)input2).Dispose();
			}
		};
		return input;
	}

	internal void OnCheckedOverallChanged(ColumnCheck column, CheckState checkState)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		this.CheckedOverallChanged?.Invoke(this, new CheckStateEventArgs(column, checkState));
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected I4, but got Unknown
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Expected O, but got Unknown
		if ((int)keyData != 13)
		{
			switch (keyData - 32)
			{
			default:
				if ((int)keyData == 131139 && ClipboardCopy && rows != null && !inEditMode && selectedIndex.Length != 0)
				{
					CopyData(selectedIndex);
					if (HandShortcutKeys)
					{
						return true;
					}
				}
				goto IL_032f;
			case 8:
				if (rows != null)
				{
					if (selectedIndex.Length == 0)
					{
						ScrollBar.ValueY += 50;
					}
					else if (selectedIndex[selectedIndex.Length - 1] < rows.Length - 1)
					{
						int i = (SelectedIndex = selectedIndex[selectedIndex.Length - 1] + 1);
						ScrollLine(i, rows);
					}
					else if (selectedIndex.Length > 1)
					{
						int i2 = (SelectedIndex = selectedIndex[selectedIndex.Length - 1]);
						ScrollLine(i2, rows);
					}
					if (HandShortcutKeys)
					{
						return true;
					}
				}
				goto IL_032f;
			case 6:
				if (rows != null)
				{
					if (selectedIndex.Length == 0)
					{
						ScrollBar.ValueY -= 50;
					}
					else if (selectedIndex[0] > 1)
					{
						SelectedIndex--;
						ScrollLine(selectedIndex[0], rows);
					}
					if (HandShortcutKeys)
					{
						return true;
					}
				}
				goto IL_032f;
			case 1:
				if (ScrollBar.ShowY)
				{
					ScrollBar.ValueY -= rect_read.Height;
					if (HandShortcutKeys)
					{
						return true;
					}
				}
				goto IL_032f;
			case 2:
				if (ScrollBar.ShowY)
				{
					ScrollBar.ValueY += rect_read.Height;
					if (HandShortcutKeys)
					{
						return true;
					}
				}
				goto IL_032f;
			case 5:
				if (ScrollBar.ShowX)
				{
					ScrollBar.ValueX -= 50;
					if (HandShortcutKeys)
					{
						return true;
					}
				}
				goto IL_032f;
			case 7:
				if (ScrollBar.ShowX)
				{
					ScrollBar.ValueX += 50;
					if (HandShortcutKeys)
					{
						return true;
					}
				}
				goto IL_032f;
			case 0:
				break;
			case 3:
			case 4:
				goto IL_032f;
			}
		}
		if (rows != null && selectedIndex.Length != 0)
		{
			RowTemplate rowTemplate = rows[selectedIndex[0]];
			this.CellClick?.Invoke(this, new TableClickEventArgs(rowTemplate.RECORD, selectedIndex[0], 0, new Rectangle(rowTemplate.RECT.X - ScrollBar.ValueX, rowTemplate.RECT.Y - ScrollBar.ValueY, rowTemplate.RECT.Width, rowTemplate.RECT.Height), new MouseEventArgs((MouseButtons)1048576, 0, 0, 0, 0)));
		}
		goto IL_032f;
		IL_032f:
		return ((Control)this).ProcessCmdKey(ref msg, keyData);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		if (LoadLayout())
		{
			((Control)this).Invalidate();
		}
		((Control)this).OnFontChanged(e);
	}

	protected override void OnCreateControl()
	{
		((Control)this).OnCreateControl();
		if (dataSource != null && dataOne)
		{
			LoadLayout();
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (((Control)this).IsHandleCreated && clientRectangle.Width > 1 && clientRectangle.Height > 1)
		{
			string text = clientRectangle.Width + "_" + clientRectangle.Height;
			if (!(show_oldrect == text))
			{
				show_oldrect = text;
				LoadLayout(clientRectangle);
				((Control)this).OnSizeChanged(e);
			}
		}
	}

	public bool LoadLayout()
	{
		if (((Control)this).IsHandleCreated)
		{
			Rectangle clientRectangle = ((Control)this).ClientRectangle;
			if (clientRectangle.Width > 1 && clientRectangle.Height > 1)
			{
				LoadLayout(clientRectangle);
			}
			else
			{
				show_oldrect = null;
			}
			return true;
		}
		return false;
	}

	private void LoadLayout(Rectangle rect_t)
	{
		Rectangle rect = LayoutDesign(rect_t);
		ScrollBar.SizeChange(rect);
	}

	private Rectangle LayoutDesign(Rectangle rect)
	{
		has_check = false;
		if (dataTmp == null)
		{
			ThreadState?.Dispose();
			ThreadState = null;
			if (visibleHeader && emptyHeader && columns != null && columns.Count > 0)
			{
				List<Column> Columns;
				bool Processing;
				Dictionary<int, object> ColWidth;
				int KeyTreeIndex;
				List<RowTemplate> list = LayoutDesign(new TempTable(new TempiColumn[0], new IRow[0]), out Columns, out Processing, out ColWidth, out KeyTreeIndex);
				rows = LayoutDesign(rect, list, Columns, ColWidth, KeyTreeIndex, out var _x, out var _y, out var _is_exceed);
				ScrollBar.SetVrSize(_is_exceed ? _x : 0, _y);
				return rect;
			}
			ScrollBar.SetVrSize(0, 0);
			dividers = new Rectangle[0];
			rows = null;
		}
		else
		{
			List<Column> Columns2;
			bool Processing2;
			Dictionary<int, object> ColWidth2;
			int KeyTreeIndex2;
			List<RowTemplate> list2 = LayoutDesign(dataTmp, out Columns2, out Processing2, out ColWidth2, out KeyTreeIndex2);
			if (visibleHeader && EmptyHeader && list2.Count == 0)
			{
				rows = LayoutDesign(rect, list2, Columns2, ColWidth2, KeyTreeIndex2, out var _x2, out var _y2, out var _is_exceed2);
				ScrollBar.SetVrSize(_is_exceed2 ? _x2 : 0, _y2);
				ThreadState?.Dispose();
				ThreadState = null;
				return rect;
			}
			if (list2.Count > 0)
			{
				rows = LayoutDesign(rect, list2, Columns2, ColWidth2, KeyTreeIndex2, out var _x3, out var _y3, out var _is_exceed3);
				ScrollBar.SetVrSize(_is_exceed3 ? _x3 : 0, _y3);
				if (Processing2 && Config.Animation)
				{
					if (ThreadState == null)
					{
						ThreadState = new ITask((Control)(object)this, delegate(float i)
						{
							AnimationStateValue = i;
							((Control)this).Invalidate();
						}, 50, 1f, 0.05f);
					}
				}
				else
				{
					ThreadState?.Dispose();
					ThreadState = null;
				}
				return rect;
			}
			ThreadState?.Dispose();
			ThreadState = null;
			ScrollBar.SetVrSize(0, 0);
			dividers = new Rectangle[0];
			rows = null;
		}
		return Rectangle.Empty;
	}

	private List<RowTemplate> LayoutDesign(TempTable dataTmp, out List<Column> Columns, out bool Processing, out Dictionary<int, object> ColWidth, out int KeyTreeIndex)
	{
		TempTable dataTmp2 = dataTmp;
		List<RowTemplate> _rows = new List<RowTemplate>(dataTmp2.rows.Length);
		List<Column> _columns = new List<Column>(dataTmp2.columns.Length);
		int processing = 0;
		Dictionary<int, object> col_width = new Dictionary<int, object>();
		string KeyTree = null;
		int KeyTreeINDEX = -1;
		if (columns == null)
		{
			if (SortHeader == null)
			{
				TempiColumn[] array = dataTmp2.columns;
				foreach (TempiColumn tempiColumn in array)
				{
					_columns.Add(new Column(tempiColumn.key, tempiColumn.text ?? tempiColumn.key)
					{
						INDEX = _columns.Count
					});
				}
			}
			else
			{
				int[] sortHeader = SortHeader;
				foreach (int num in sortHeader)
				{
					TempiColumn tempiColumn2 = dataTmp2.columns[num];
					_columns.Add(new Column(tempiColumn2.key, tempiColumn2.text ?? tempiColumn2.key)
					{
						INDEX = num
					});
				}
			}
		}
		else
		{
			int x = 0;
			ForColumn(columns, delegate(Column it)
			{
				int count = _columns.Count;
				_columns.Add(it);
				ColumnWidth(it, ref col_width, x);
				x++;
				if (it.KeyTree != null)
				{
					TempiColumn[] array2 = dataTmp2.columns;
					for (int j = 0; j < array2.Length; j++)
					{
						if (array2[j].key == it.KeyTree)
						{
							KeyTree = it.KeyTree;
							break;
						}
					}
				}
				return count;
			});
			if (KeyTree != null)
			{
				foreach (Column item in _columns)
				{
					if (item.KeyTree == KeyTree)
					{
						KeyTreeINDEX = item.INDEX;
					}
				}
			}
		}
		if (KeyTree == null)
		{
			ForRow(dataTmp2, delegate(IRow row)
			{
				List<CELL> cells2 = new List<CELL>(_columns.Count);
				foreach (Column item2 in _columns)
				{
					AddRows(ref cells2, ref processing, item2, row, item2.Key);
				}
				if (cells2.Count > 0)
				{
					AddRows(ref _rows, cells2.ToArray(), row.i, row.record);
				}
			});
		}
		else
		{
			ForRow(dataTmp2, delegate(IRow row)
			{
				List<CELL> cells = new List<CELL>(_columns.Count);
				foreach (Column item3 in _columns)
				{
					AddRows(ref cells, ref processing, item3, row, item3.Key);
				}
				if (cells.Count > 0)
				{
					ForTree(ref _rows, ref processing, AddRows(ref _rows, cells.ToArray(), row.i, row.record), row, _columns, KeyTree, KeyTreeINDEX, 0, show: true);
				}
			});
		}
		Columns = _columns;
		Processing = processing > 0;
		ColWidth = col_width;
		KeyTreeIndex = KeyTreeINDEX;
		dataOne = false;
		return _rows;
	}

	private RowTemplate[] LayoutDesign(Rectangle rect, List<RowTemplate> _rows, List<Column> _columns, Dictionary<int, object> col_width, int KeyTreeINDEX, out int _x, out int _y, out bool _is_exceed)
	{
		List<RowTemplate> _rows2 = _rows;
		List<Column> _columns2 = _columns;
		Dictionary<int, object> col_width2 = col_width;
		if (rows != null)
		{
			List<object> list = new List<object>(rows.Length);
			List<object> list2 = new List<object>(1);
			RowTemplate[] array = rows;
			foreach (RowTemplate rowTemplate in array)
			{
				if (rowTemplate.Select)
				{
					list.Add(rowTemplate.RECORD);
				}
				if (rowTemplate.Hover)
				{
					list2.Add(rowTemplate.RECORD);
				}
			}
			if (list.Count > 0 || list2.Count > 0)
			{
				foreach (RowTemplate item in _rows2)
				{
					if (list.Contains(item.RECORD))
					{
						item.Select = true;
					}
					if (list2.Contains(item.RECORD))
					{
						item.Hover = true;
					}
				}
			}
		}
		List<TCellColumn> list3 = new List<TCellColumn>(_columns2.Count);
		foreach (Column item2 in _columns2)
		{
			list3.Add(new TCellColumn(this, item2));
		}
		AddRows(ref _rows2, list3.ToArray(), dataSource);
		int x = 0;
		int y = 0;
		bool is_exceed = false;
		rect_read.X = rect.X;
		rect_read.Y = rect.Y;
		Helper.GDI(delegate(Canvas g)
		{
			float dpi = Config.Dpi;
			int num = (int)((float)_checksize * dpi);
			int check_size = (int)((float)_switchsize * dpi);
			int num2 = (int)((float)TreeButtonSize * dpi);
			int num3 = (int)((float)_gap * dpi);
			int num4 = num3 * 2;
			int sort_size = (int)((float)DragHandleSize * dpi);
			int sort_ico_size = (int)((float)DragHandleIconSize * dpi);
			int num5 = (int)(1f * dpi);
			int num6 = num5 / 2;
			int num7 = (int)(6f * dpi);
			int num8 = num7 / 2;
			check_radius = (float)num * 0.12f * dpi;
			check_border = (float)num * 0.04f * dpi;
			Dictionary<int, AutoWidth> dictionary = new Dictionary<int, AutoWidth>(_rows2[0].cells.Length);
			for (int j = 0; j < _rows2[0].cells.Length; j++)
			{
				dictionary.Add(j, new AutoWidth());
			}
			for (int k = 0; k < _rows2.Count; k++)
			{
				RowTemplate rowTemplate2 = _rows2[k];
				rowTemplate2.INDEX = k;
				if (rowTemplate2.ShowExpand)
				{
					float num9 = 0f;
					if (rowTemplate2.IsColumn)
					{
						for (int l = 0; l < rowTemplate2.cells.Length; l++)
						{
							CELL cELL = rowTemplate2.cells[l];
							cELL.INDEX = l;
							Size size = cELL.GetSize(g, columnfont ?? ((Control)this).Font, rect.Width, num3, num4);
							if (cELL.COLUMN is ColumnSort)
							{
								dictionary[l].value = -2;
							}
							else if (cELL.COLUMN is ColumnCheck { NoTitle: not false })
							{
								dictionary[l].value = -1;
							}
							else
							{
								int width = size.Width;
								if (dictionary[l].value < width)
								{
									dictionary[l].value = width;
								}
								if (dictionary[l].minvalue < cELL.MinWidth)
								{
									dictionary[l].minvalue = cELL.MinWidth;
								}
							}
							if (num9 < (float)size.Height)
							{
								num9 = size.Height;
							}
						}
						if (rowHeightHeader.HasValue)
						{
							rowTemplate2.Height = (int)((float)rowHeightHeader.Value * dpi);
						}
						else if (rowHeight.HasValue)
						{
							rowTemplate2.Height = (int)((float)rowHeight.Value * dpi);
						}
						else
						{
							rowTemplate2.Height = (int)Math.Round(num9) + num4;
						}
					}
					else
					{
						for (int m = 0; m < rowTemplate2.cells.Length; m++)
						{
							CELL cELL2 = rowTemplate2.cells[m];
							cELL2.INDEX = m;
							if (cELL2.COLUMN is ColumnSort || cELL2 is TCellCheck { NoTitle: not false })
							{
								if (num9 < (float)num4)
								{
									num9 = num4;
								}
							}
							else
							{
								Size size2 = cELL2.GetSize(g, ((Control)this).Font, rect.Width, num3, num4);
								int num10 = size2.Width;
								if (cELL2.ROW.CanExpand && _rows2[0].cells[m].INDEX == KeyTreeINDEX)
								{
									num10 += num2 + num4 + num2 * cELL2.ROW.ExpandDepth;
								}
								if (num9 < (float)size2.Height)
								{
									num9 = size2.Height;
								}
								if (dictionary[m].value < num10)
								{
									dictionary[m].value = num10;
								}
							}
						}
						if (rowHeight.HasValue)
						{
							rowTemplate2.Height = (int)((float)rowHeight.Value * dpi);
						}
						else
						{
							rowTemplate2.Height = (int)Math.Round(num9) + num4;
						}
					}
				}
			}
			foreach (KeyValuePair<int, AutoWidth> item3 in dictionary)
			{
				string maxWidth = _columns2[item3.Key].MaxWidth;
				if (maxWidth != null)
				{
					int result2;
					if (maxWidth.EndsWith("%") && float.TryParse(maxWidth.TrimEnd(new char[1] { '%' }), out var result))
					{
						int num11 = (int)((float)rect.Width * result / 100f);
						if (item3.Value.value > num11)
						{
							item3.Value.value = num11;
						}
					}
					else if (int.TryParse(maxWidth, out result2))
					{
						int num12 = (int)((float)result2 * Config.Dpi);
						if (item3.Value.value > num12)
						{
							item3.Value.value = num12;
						}
					}
				}
			}
			rect_read.Width = rect.Width;
			rect_read.Height = rect.Height;
			Dictionary<int, int> dictionary2 = CalculateWidth(rect, col_width2, dictionary, num4, num, sort_size, ref is_exceed);
			int num13 = ((!visibleHeader) ? (rect.Y - _rows2[0].Height) : rect.Y);
			foreach (RowTemplate item4 in _rows2)
			{
				if (item4.ShowExpand)
				{
					int num14 = rect.X;
					item4.RECT = new Rectangle(rect.X, num13, rect_read.Width, item4.Height);
					for (int n = 0; n < item4.cells.Length; n++)
					{
						CELL cELL3 = item4.cells[n];
						Rectangle rect2 = new Rectangle(num14, num13, dictionary2[n], item4.RECT.Height);
						int ox = 0;
						if (item4.INDEX > 0 && _rows2[0].cells[n].INDEX == KeyTreeINDEX)
						{
							int num15 = num3 + num2 * item4.ExpandDepth;
							ox = num15 + num3 + num2 / 2;
							item4.RectExpand = new Rectangle(num14 + num15 + num7, num13 + (item4.Height - num2) / 2, num2, num2);
						}
						if (cELL3 is TCellCheck tCellCheck2)
						{
							tCellCheck2.SetSize(rect2, num);
						}
						else if (cELL3 is TCellRadio tCellRadio)
						{
							tCellRadio.SetSize(rect2, num);
						}
						else if (cELL3 is TCellSwitch tCellSwitch)
						{
							tCellSwitch.SetSize(rect2, check_size);
						}
						else if (cELL3 is TCellSort tCellSort)
						{
							tCellSort.SetSize(rect2, sort_size, sort_ico_size);
						}
						else if (cELL3 is TCellColumn tCellColumn)
						{
							cELL3.SetSize(g, ((Control)this).Font, rect2, ox, num3, num4);
							if (tCellColumn.COLUMN is ColumnCheck { NoTitle: not false } columnCheck2)
							{
								tCellColumn.COLUMN.SortOrder = false;
								columnCheck2.PARENT = this;
								tCellColumn.RECT_REAL = new Rectangle(rect2.X + (rect2.Width - num) / 2, rect2.Y + (rect2.Height - num) / 2, num, num);
							}
							else
							{
								if (tCellColumn.COLUMN.SortOrder)
								{
									tCellColumn.RECT_REAL = new Rectangle(rect2.X + num3, rect2.Y, rect2.Width - num4 - tCellColumn.SortWidth, rect2.Height);
								}
								else
								{
									tCellColumn.RECT_REAL = new Rectangle(rect2.X + num3, rect2.Y, rect2.Width - num4, rect2.Height);
								}
								if (x < tCellColumn.RECT_REAL.Right)
								{
									x = tCellColumn.RECT_REAL.Right;
								}
							}
						}
						else
						{
							cELL3.SetSize(g, ((Control)this).Font, rect2, ox, num3, num4);
						}
						if (x < rect2.Right)
						{
							x = rect2.Right;
						}
						if (y < rect2.Bottom)
						{
							y = rect2.Bottom;
						}
						num14 += dictionary2[n];
					}
					num13 += item4.Height;
				}
			}
			List<Rectangle> list4 = new List<Rectangle>();
			List<Rectangle> list5 = new List<Rectangle>();
			List<MoveHeader> list6 = new List<MoveHeader>();
			int num16 = _rows2.Count - 1;
			RowTemplate rowTemplate3 = _rows2[num16];
			while (!rowTemplate3.ShowExpand)
			{
				num16--;
				rowTemplate3 = _rows2[num16];
			}
			CELL cELL4 = rowTemplate3.cells[rowTemplate3.cells.Length - 1];
			bool flag = emptyHeader && _rows2.Count == 1;
			if (rect.Y + rect.Height > cELL4.RECT.Bottom && !flag)
			{
				rect_read.Height = cELL4.RECT.Bottom - rect.Y;
			}
			int num17 = num5 * 2;
			rect_divider = new Rectangle(rect_read.X + num5, rect_read.Y + num5, rect_read.Width - num17, rect_read.Height - num17);
			Dictionary<int, MoveHeader> dictionary3 = new Dictionary<int, MoveHeader>(moveheaders.Length);
			MoveHeader[] array2 = moveheaders;
			foreach (MoveHeader moveHeader in array2)
			{
				dictionary3.Add(moveHeader.i, moveHeader);
			}
			foreach (RowTemplate item5 in _rows2)
			{
				if (item5.IsColumn)
				{
					if (EnableHeaderResizing)
					{
						for (int num19 = 0; num19 < item5.cells.Length; num19++)
						{
							CELL cELL5 = item5.cells[num19];
							list6.Add(new MoveHeader(dictionary3, new Rectangle(cELL5.RECT.Right - num8, rect.Y, num7, cELL5.RECT.Height), num19, cELL5.RECT.Width, cELL5.MinWidth));
						}
					}
					if (bordered)
					{
						if (flag)
						{
							for (int num20 = 0; num20 < item5.cells.Length - 1; num20++)
							{
								CELL cELL6 = item5.cells[num20];
								list4.Add(new Rectangle(cELL6.RECT.Right - num6, rect.Y, num5, cELL6.RECT.Height));
							}
						}
						else
						{
							for (int num21 = 0; num21 < item5.cells.Length - 1; num21++)
							{
								CELL cELL7 = item5.cells[num21];
								list4.Add(new Rectangle(cELL7.RECT.Right - num6, rect.Y, num5, rect_read.Height));
							}
						}
						if (visibleHeader)
						{
							list5.Add(new Rectangle(rect.X, item5.RECT.Bottom - num6, rect_read.Width, num5));
						}
					}
					else
					{
						for (int num22 = 0; num22 < item5.cells.Length - 1; num22++)
						{
							CELL cELL8 = item5.cells[num22];
							list4.Add(new Rectangle(cELL8.RECT.Right - num6, cELL8.RECT.Y + num3, num5, cELL8.RECT.Height - num4));
						}
					}
				}
				else if (bordered)
				{
					list5.Add(new Rectangle(rect.X, item5.RECT.Bottom - num6, rect_read.Width, num5));
				}
				else
				{
					list5.Add(new Rectangle(item5.RECT.X, item5.RECT.Bottom - num6, item5.RECT.Width, num5));
				}
			}
			if (bordered && !flag)
			{
				list5.RemoveAt(list5.Count - 1);
			}
			dividerHs = list4.ToArray();
			dividers = list5.ToArray();
			moveheaders = list6.ToArray();
		});
		_x = x;
		_y = y;
		_is_exceed = is_exceed;
		return _rows2.ToArray();
	}

	private void ForColumn(ColumnCollection columns, Func<Column, int> action)
	{
		if (SortHeader == null)
		{
			foreach (Column column2 in columns)
			{
				column2.PARENT = this;
				if (column2.Visible)
				{
					column2.INDEX = action(column2);
				}
			}
			return;
		}
		Dictionary<int, Column> dictionary = new Dictionary<int, Column>();
		foreach (Column column3 in columns)
		{
			if (column3.Visible)
			{
				dictionary.Add(dictionary.Count, column3);
			}
		}
		int[] sortHeader = SortHeader;
		foreach (int num in sortHeader)
		{
			Column column = dictionary[num];
			column.PARENT = this;
			column.INDEX = num;
			if (column.Visible)
			{
				action(column);
			}
		}
	}

	private void ForRow(TempTable data_temp, Action<IRow> action)
	{
		if (SortData == null || SortData.Length != data_temp.rows.Length)
		{
			IRow[] array = data_temp.rows;
			foreach (IRow obj in array)
			{
				action(obj);
			}
		}
		else
		{
			int[] sortData = SortData;
			foreach (int num in sortData)
			{
				action(data_temp.rows[num]);
			}
		}
	}

	private bool ForTree(ref List<RowTemplate> _rows, ref int processing, RowTemplate row_new, IRow row, List<Column> _columns, string KeyTree, int KeyTreeINDEX, int depth, bool show)
	{
		if (DefaultExpand && dataOne && !rows_Expand.Contains(row.record))
		{
			rows_Expand.Add(row.record);
		}
		row_new.ShowExpand = show;
		row_new.ExpandDepth = depth;
		row_new.KeyTreeINDEX = KeyTreeINDEX;
		row_new.Expand = rows_Expand.Contains(row.record);
		int num = 0;
		IList<object> list = ForTreeValue(row, KeyTree);
		if (list != null)
		{
			show = show && row_new.Expand;
			row_new.CanExpand = true;
			num++;
			for (int i = 0; i < list.Count; i++)
			{
				Dictionary<string, object> row2 = GetRow(list[i], _columns.Count);
				if (row2.Count <= 0)
				{
					continue;
				}
				IRow row3 = new IRow(i, list[i], row2);
				List<CELL> cells = new List<CELL>(_columns.Count);
				foreach (Column _column in _columns)
				{
					AddRows(ref cells, ref processing, _column, row3, _column.Key);
				}
				if (ForTree(ref _rows, ref processing, AddRows(ref _rows, cells.ToArray(), row.i, row3.record), row3, _columns, KeyTree, KeyTreeINDEX, depth + 1, show))
				{
					num++;
				}
			}
		}
		return num > 0;
	}

	private static IList<object>? ForTreeValue(IRow row, string KeyTree)
	{
		if (row.cells.ContainsKey(KeyTree))
		{
			object obj = row.cells[KeyTree];
			if (obj is AntItem antItem)
			{
				if (antItem.value is IList<AntItem[]> { Count: >0 } list)
				{
					List<object> list2 = new List<object>(list.Count);
					foreach (AntItem[] item in list)
					{
						list2.Add(item);
					}
					return list2.ToArray();
				}
			}
			else if (obj is PropertyDescriptor propertyDescriptor && propertyDescriptor.GetValue(row.record) is IList<object> { Count: >0 } list3)
			{
				return list3;
			}
		}
		return null;
	}

	private Dictionary<int, int> CalculateWidth(Rectangle rect, Dictionary<int, object> col_width, Dictionary<int, AutoWidth> read_width, int gap2, int check_size, int sort_size, ref bool is_exceed)
	{
		int num = rect.Width;
		float num2 = 0f;
		foreach (KeyValuePair<int, AutoWidth> item in read_width)
		{
			object value2;
			if (tmpcol_width.TryGetValue(item.Key, out var value))
			{
				num2 += (float)value;
			}
			else if (col_width.TryGetValue(item.Key, out value2))
			{
				if (value2 is int num3)
				{
					num2 = num3 switch
					{
						-1 => num2 + (float)item.Value.value, 
						-2 => num2 + (float)item.Value.minvalue, 
						_ => num2 + (float)num3, 
					};
				}
				if (value2 is float num4)
				{
					num2 += (float)rect.Width * num4;
				}
			}
			else if ((float)item.Value.value == -1f)
			{
				int num5 = check_size * 2;
				num2 += (float)num5;
				num -= num5;
			}
			else if ((float)item.Value.value == -2f)
			{
				int num6 = sort_size + gap2;
				num2 += (float)num6;
				num -= num6;
			}
			else
			{
				num2 += (float)item.Value.value;
			}
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>(read_width.Count);
		if (num2 > (float)rect.Width)
		{
			is_exceed = true;
			foreach (KeyValuePair<int, AutoWidth> item2 in read_width)
			{
				object value4;
				if (tmpcol_width.TryGetValue(item2.Key, out var value3))
				{
					dictionary.Add(item2.Key, value3);
				}
				else if (col_width.TryGetValue(item2.Key, out value4))
				{
					if (value4 is int num7)
					{
						switch (num7)
						{
						case -1:
							dictionary.Add(item2.Key, item2.Value.value);
							break;
						case -2:
							dictionary.Add(item2.Key, item2.Value.value);
							break;
						default:
							dictionary.Add(item2.Key, num7);
							break;
						}
					}
					else if (value4 is float num8)
					{
						dictionary.Add(item2.Key, (int)Math.Ceiling((float)rect.Width * num8));
					}
				}
				else if ((float)item2.Value.value == -1f)
				{
					dictionary.Add(item2.Key, check_size * 2);
				}
				else if ((float)item2.Value.value == -2f)
				{
					dictionary.Add(item2.Key, sort_size + gap2);
				}
				else
				{
					dictionary.Add(item2.Key, item2.Value.value);
				}
			}
		}
		else
		{
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, AutoWidth> item3 in read_width)
			{
				object value6;
				if (tmpcol_width.TryGetValue(item3.Key, out var value5))
				{
					dictionary.Add(item3.Key, value5);
				}
				else if (col_width.TryGetValue(item3.Key, out value6))
				{
					if (value6 is int num9)
					{
						switch (num9)
						{
						case -1:
							dictionary.Add(item3.Key, item3.Value.value);
							break;
						case -2:
							list.Add(item3.Key);
							break;
						default:
							dictionary.Add(item3.Key, num9);
							break;
						}
					}
					else if (value6 is float num10)
					{
						dictionary.Add(item3.Key, (int)Math.Ceiling((float)rect.Width * num10));
					}
				}
				else if ((float)item3.Value.value == -1f)
				{
					dictionary.Add(item3.Key, check_size * 2);
				}
				else if ((float)item3.Value.value == -2f)
				{
					dictionary.Add(item3.Key, sort_size + gap2);
				}
				else
				{
					dictionary.Add(item3.Key, (int)Math.Ceiling((float)num * ((float)item3.Value.value / num2)));
				}
			}
			int num11 = 0;
			foreach (KeyValuePair<int, int> item4 in dictionary)
			{
				num11 += item4.Value;
			}
			if (list.Count > 0)
			{
				int value7 = (rect.Width - num11) / list.Count;
				foreach (int item5 in list)
				{
					dictionary.Add(item5, value7);
				}
				num11 = rect.Width;
			}
			if (rect_read.Width > num11)
			{
				if (AutoSizeColumnsMode == ColumnsMode.Fill)
				{
					Dictionary<int, int> dictionary2 = new Dictionary<int, int>(dictionary.Count);
					foreach (KeyValuePair<int, int> item6 in dictionary)
					{
						dictionary2.Add(item6.Key, (int)Math.Round((double)rect_read.Width * ((double)item6.Value * 1.0) / (double)num11));
					}
					dictionary = dictionary2;
				}
				else
				{
					rect_read.Width = num11;
				}
			}
		}
		return dictionary;
	}

	private void ColumnWidth(Column it, ref Dictionary<int, object> col_width, int x)
	{
		if (it.Width != null)
		{
			int result2;
			if (it.Width.EndsWith("%") && float.TryParse(it.Width.TrimEnd(new char[1] { '%' }), out var result))
			{
				col_width.Add(x, result / 100f);
			}
			else if (int.TryParse(it.Width, out result2))
			{
				col_width.Add(x, (int)((float)result2 * Config.Dpi));
			}
			else if (it.Width.Contains("fill"))
			{
				col_width.Add(x, -2);
			}
			else
			{
				col_width.Add(x, -1);
			}
		}
	}

	private void AddRows(ref List<CELL> cells, ref int processing, Column column, IRow row, string key)
	{
		object value;
		if (column is ColumnSort column2)
		{
			AddRows(ref cells, new TCellSort(this, column2));
		}
		else if (row.cells.TryGetValue(key, out value))
		{
			PropertyDescriptor property;
			object value2;
			object obj = OGetValue(value, row.record, out property, out value2);
			if (column.Render == null)
			{
				AddRows(ref cells, ref processing, column, value2, obj, property);
			}
			else
			{
				AddRows(ref cells, ref processing, column, value2, column.Render?.Invoke(obj, row.record, row.i), property);
			}
		}
		else
		{
			AddRows(ref cells, ref processing, column, null, column.Render?.Invoke(null, row.record, row.i), null);
		}
	}

	private void AddRows(ref List<CELL> cells, ref int processing, Column column, object? ov, object? value, PropertyDescriptor? prop)
	{
		if (value == null)
		{
			cells.Add(new TCellText(this, column, prop, ov, null));
		}
		else if (column is ColumnCheck column2)
		{
			has_check = true;
			bool value2 = false;
			if (value is bool flag)
			{
				value2 = flag;
			}
			AddRows(ref cells, new TCellCheck(this, column2, prop, ov, value2));
		}
		else if (column is ColumnRadio column3)
		{
			has_check = true;
			bool value3 = false;
			if (value is bool flag2)
			{
				value3 = flag2;
			}
			AddRows(ref cells, new TCellRadio(this, column3, prop, ov, value3));
		}
		else if (column is ColumnSwitch column4)
		{
			bool value4 = false;
			if (value is bool flag3)
			{
				value4 = flag3;
			}
			AddRows(ref cells, new TCellSwitch(this, column4, prop, ov, value4));
		}
		else if (value is IList<ICell> cels)
		{
			AddRows(ref cells, new Template(this, column, prop, ov, ref processing, cels));
		}
		else if (value is ICell cell)
		{
			AddRows(ref cells, new Template(this, column, prop, ov, ref processing, new ICell[1] { cell }));
		}
		else
		{
			cells.Add(new TCellText(this, column, prop, ov, value.ToString()));
		}
		if (ov is INotifyPropertyChanged notifyPropertyChanged)
		{
			notifyPropertyChanged.PropertyChanged -= Notify_PropertyChanged;
			notifyPropertyChanged.PropertyChanged += Notify_PropertyChanged;
		}
	}

	private void AddRows(ref List<CELL> cells, CELL data)
	{
		cells.Add(data);
		if (!(data is Template template))
		{
			return;
		}
		foreach (ICell item in template.Value)
		{
			item.Changed = delegate(bool layout)
			{
				if (layout)
				{
					LoadLayout();
				}
				((Control)this).Invalidate();
			};
		}
	}

	private RowTemplate AddRows(ref List<RowTemplate> rows, CELL[] cells, int row_i, object? record)
	{
		RowTemplate rowTemplate = new RowTemplate(this, cells, row_i, record);
		if (enableDir.Contains(row_i))
		{
			rowTemplate.ENABLE = false;
		}
		CELL[] cells2 = rowTemplate.cells;
		for (int i = 0; i < cells2.Length; i++)
		{
			cells2[i].SetROW(rowTemplate);
		}
		rows.Add(rowTemplate);
		return rowTemplate;
	}

	private RowTemplate AddRows(ref List<RowTemplate> rows, TCellColumn[] cells, object? record)
	{
		RowTemplate rowTemplate = new RowTemplate(this, cells, -1, record)
		{
			IsColumn = true
		};
		for (int i = 0; i < rowTemplate.cells.Length; i++)
		{
			CELL cELL = rowTemplate.cells[i];
			if (cELL.COLUMN is ColumnCheck { NoTitle: not false } columnCheck)
			{
				if (rows.Count > 0)
				{
					int count = rows.Count;
					int num = 0;
					for (int j = 0; j < rows.Count; j++)
					{
						if (rows[j].cells[i] is TCellCheck { Checked: not false })
						{
							num++;
						}
					}
					if (count == num)
					{
						columnCheck.CheckState = (CheckState)1;
					}
					else if (num > 0)
					{
						columnCheck.CheckState = (CheckState)2;
					}
					else
					{
						columnCheck.CheckState = (CheckState)0;
					}
				}
				else
				{
					columnCheck.CheckState = (CheckState)0;
				}
			}
			cELL.SetROW(rowTemplate);
		}
		rows.Insert(0, rowTemplate);
		return rowTemplate;
	}

	private string? OGetValue(TempTable data_temp, int i_r, string key)
	{
		object obj = data_temp.rows[i_r].cells[key];
		if (obj is AntItem antItem)
		{
			object value = antItem.value;
			if (value is IList<ICell> list)
			{
				List<string> list2 = new List<string>(list.Count);
				foreach (ICell item in list)
				{
					string text = item.ToString();
					if (!string.IsNullOrEmpty(text))
					{
						list2.Add(text);
					}
				}
				return string.Join(" ", list2);
			}
			return value?.ToString();
		}
		if (obj is PropertyDescriptor propertyDescriptor)
		{
			object value2 = propertyDescriptor.GetValue(data_temp.rows[i_r].record);
			if (value2 is IList<ICell> list3)
			{
				List<string> list4 = new List<string>(list3.Count);
				foreach (ICell item2 in list3)
				{
					string text2 = item2.ToString();
					if (!string.IsNullOrEmpty(text2))
					{
						list4.Add(text2);
					}
				}
				return string.Join(" ", list4);
			}
			return value2?.ToString();
		}
		return obj?.ToString();
	}

	private object? OGetValue(object? ov, object record, out PropertyDescriptor? property, out object? value)
	{
		value = ov;
		property = null;
		if (ov is AntItem antItem)
		{
			return antItem.value;
		}
		if (ov is PropertyDescriptor propertyDescriptor)
		{
			value = record;
			property = propertyDescriptor;
			return propertyDescriptor.GetValue(record);
		}
		return ov;
	}

	private void Notify_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (sender != null && e.PropertyName != null)
		{
			PropertyChanged(sender, e.PropertyName);
		}
	}

	private void PropertyChanged(object data, string key)
	{
		if (rows == null || key == null)
		{
			return;
		}
		for (int i = 0; i < rows[0].cells.Length; i++)
		{
			CELL cELL = rows[0].cells[i];
			if (key == cELL.COLUMN.Key)
			{
				RowTemplate[] array;
				if (data is AntItem antItem)
				{
					array = rows;
					foreach (RowTemplate rowTemplate in array)
					{
						if (rowTemplate.RECORD is IList<AntItem> list && list.Contains(antItem))
						{
							RefreshItem(rows, rowTemplate, rowTemplate.cells[i], i, antItem.value);
							break;
						}
					}
					break;
				}
				array = rows;
				foreach (RowTemplate rowTemplate2 in array)
				{
					if (rowTemplate2.RECORD == data)
					{
						CELL cELL2 = rowTemplate2.cells[i];
						if (cELL2.PROPERTY != null)
						{
							RefreshItem(rows, rowTemplate2, cELL2, i, cELL2.PROPERTY.GetValue(data));
						}
						break;
					}
				}
				break;
			}
			if (key == cELL.COLUMN.KeyTree)
			{
				int num = rows.Length;
				LoadLayout();
				int num2 = rows.Length;
				if (selectedIndex.Length != 0 && num2 < num)
				{
					SetIndex(selectedIndex[0] - num - num2);
				}
				((Control)this).Invalidate();
				break;
			}
		}
	}

	private void RefreshItem(RowTemplate[] rows, RowTemplate row, CELL cel, int cel_i, object? value)
	{
		if (cel is Template template)
		{
			int num = 0;
			if (value == null)
			{
				num++;
			}
			else if (value is IList<ICell> list)
			{
				if (template.Value.Count == list.Count)
				{
					for (int i = 0; i < template.Value.Count; i++)
					{
						template.Value[i] = list[i];
						num++;
					}
				}
				else
				{
					num++;
				}
			}
			else if (value is ICell value2)
			{
				if (template.Value.Count == 1)
				{
					template.Value[0] = value2;
					num++;
				}
				else
				{
					num++;
				}
			}
			else
			{
				num++;
			}
			if (num > 0)
			{
				LoadLayout();
				((Control)this).Invalidate();
			}
		}
		else if (cel is TCellText tCellText)
		{
			if (value is IList<ICell> || value is ICell)
			{
				LoadLayout();
			}
			else
			{
				tCellText.value = value?.ToString();
			}
			((Control)this).Invalidate();
		}
		else if (cel is TCellCheck tCellCheck)
		{
			if (value is bool @checked)
			{
				tCellCheck.Checked = @checked;
			}
			row.Select = RowISelect(row);
			if (cel.COLUMN is ColumnCheck { NoTitle: not false } columnCheck)
			{
				int num2 = rows.Length - 1;
				int num3 = 0;
				for (int j = 1; j < rows.Length; j++)
				{
					if (rows[j].cells[cel_i] is TCellCheck { Checked: not false })
					{
						num3++;
					}
				}
				if (num2 == num3)
				{
					columnCheck.CheckState = (CheckState)1;
				}
				else if (num3 > 0)
				{
					columnCheck.CheckState = (CheckState)2;
				}
				else
				{
					columnCheck.CheckState = (CheckState)0;
				}
			}
			((Control)this).Invalidate();
		}
		else if (cel is TCellRadio tCellRadio)
		{
			if (value is bool checked2)
			{
				tCellRadio.Checked = checked2;
			}
			row.Select = RowISelect(row);
			((Control)this).Invalidate();
		}
		else if (cel is TCellSwitch tCellSwitch)
		{
			if (value is bool checked3)
			{
				tCellSwitch.Checked = checked3;
			}
			((Control)this).Invalidate();
		}
		else
		{
			LoadLayout();
			((Control)this).Invalidate();
		}
	}

	internal void CheckAll(int i_cel, ColumnCheck columnCheck, bool value)
	{
		if (rows == null)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 1; i < rows.Length; i++)
		{
			CELL cELL = rows[i].cells[i_cel];
			if (cELL is TCellCheck tCellCheck)
			{
				num++;
				if (tCellCheck.Checked != value)
				{
					tCellCheck.Checked = value;
					SetValue(cELL, tCellCheck.Checked);
					this.CheckedChanged?.Invoke(this, new TableCheckEventArgs(value, rows[i].RECORD, i, i_cel));
				}
				else
				{
					num2++;
				}
			}
		}
		if (num > 0 && num2 == num)
		{
			columnCheck.Checked = value;
		}
	}

	private void SetValue(CELL cel, object? value)
	{
		if (cel.PROPERTY == null)
		{
			if (cel.VALUE is AntItem antItem)
			{
				antItem.value = value;
			}
		}
		else
		{
			cel.PROPERTY.SetValue(cel.VALUE, Convert.ChangeType(value, cel.PROPERTY.PropertyType));
		}
	}

	private bool RowISelect(RowTemplate row)
	{
		for (int i = 0; i < row.cells.Length; i++)
		{
			CELL cELL = row.cells[i];
			if (cELL is TCellCheck tCellCheck)
			{
				if (tCellCheck.Checked)
				{
					return true;
				}
			}
			else if (cELL is TCellRadio { Checked: not false })
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Invalid comparison between Unknown and I4
		cellMouseDown = null;
		if (ClipboardCopy)
		{
			((Control)this).Focus();
		}
		subForm?.IClose();
		subForm = null;
		if (ScrollBar.MouseDownY(e.Location) && ScrollBar.MouseDownX(e.Location))
		{
			((Control)this).OnMouseDown(e);
			if (rows == null)
			{
				return;
			}
			OnTouchDown(e.X, e.Y);
			int r_x;
			int r_y;
			int offset_x;
			int offset_xi;
			int offset_y;
			int i_row;
			int i_cel;
			int mode;
			CELL cELL = CellContains(rows, sethover: true, e.X, e.Y, out r_x, out r_y, out offset_x, out offset_xi, out offset_y, out i_row, out i_cel, out mode);
			if (cELL == null)
			{
				return;
			}
			if (MultipleRows && ((Enum)Control.ModifierKeys).HasFlag((Enum)(object)(Keys)65536))
			{
				if (shift_index == -1)
				{
					SelectedIndexs = SetIndexs(i_row);
				}
				else if (shift_index > i_row)
				{
					SelectedIndexs = SetIndexs(i_row, shift_index);
				}
				else
				{
					SelectedIndexs = SetIndexs(shift_index, i_row);
				}
			}
			else if (MultipleRows && ((Enum)Control.ModifierKeys).HasFlag((Enum)(object)(Keys)131072))
			{
				SelectedIndexs = SetIndexs(i_row);
			}
			else
			{
				SelectedIndex = i_row;
			}
			shift_index = i_row;
			RowTemplate rowTemplate = rows[i_row];
			if (mode > 0)
			{
				if (moveheaders.Length != 0)
				{
					MoveHeader[] array = moveheaders;
					foreach (MoveHeader moveHeader in array)
					{
						if (moveHeader.rect.Contains(r_x, r_y))
						{
							moveHeader.x = e.X;
							Window.CanHandMessage = false;
							moveHeader.MouseDown = true;
							return;
						}
					}
				}
				cELL.MouseDown = ((e.Clicks <= 1) ? 1 : 2);
				cellMouseDown = cELL;
				if (cELL.MouseDown == 1 && cELL.COLUMN is ColumnCheck { NoTitle: not false } columnCheck && (int)e.Button == 1048576 && cELL.CONTAIN_REAL(r_x, r_y))
				{
					CheckAll(i_cel, columnCheck, !columnCheck.Checked);
				}
				else if (ColumnDragSort)
				{
					dragHeader = new DragHeader
					{
						i = cELL.INDEX,
						x = e.X
					};
				}
			}
			else if (cELL.COLUMN is ColumnSort && cELL.CONTAIN_REAL(r_x, r_y))
			{
				dragBody = new DragHeader
				{
					i = cELL.ROW.INDEX,
					x = e.Y
				};
			}
			else if (cELL.ROW.CanExpand && cELL.ROW.RECORD != null && cELL.ROW.RectExpand.Contains(r_x, r_y))
			{
				if (cELL.ROW.Expand)
				{
					rows_Expand.Remove(cELL.ROW.RECORD);
				}
				else
				{
					rows_Expand.Add(cELL.ROW.RECORD);
				}
				LoadLayout();
				((Control)this).Invalidate();
			}
			else
			{
				MouseDownRow(e, rowTemplate.cells[i_cel], r_x, r_y);
			}
		}
		else
		{
			shift_index = -1;
		}
	}

	private void MouseDownRow(MouseEventArgs e, CELL cell, int x, int y)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		cellMouseDown = cell;
		cell.MouseDown = ((e.Clicks <= 1) ? 1 : 2);
		if (!(cell is Template template) || (int)e.Button != 1048576)
		{
			return;
		}
		foreach (ICell item in template.Value)
		{
			if (item is CellLink { Enabled: not false, Rect: var rect } cellLink && rect.Contains(x, y))
			{
				cellLink.ExtraMouseDown = true;
				break;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		((Control)this).OnMouseUp(e);
		if (moveheaders.Length != 0)
		{
			MoveHeader[] array = moveheaders;
			foreach (MoveHeader moveHeader in array)
			{
				if (moveHeader.MouseDown)
				{
					int num = moveHeader.width + e.X - moveHeader.x;
					if (num < moveHeader.min_width)
					{
						num = moveHeader.min_width;
					}
					if (tmpcol_width.ContainsKey(moveHeader.i))
					{
						tmpcol_width[moveHeader.i] = num;
					}
					else
					{
						tmpcol_width.Add(moveHeader.i, num);
					}
					moveHeader.MouseDown = false;
					Window.CanHandMessage = true;
					LoadLayout();
					((Control)this).Invalidate();
					OnTouchCancel();
					return;
				}
			}
		}
		if (dragHeader != null)
		{
			bool hand = dragHeader.hand;
			if (hand && dragHeader.im != -1)
			{
				if (rows == null)
				{
					return;
				}
				CELL[] cells = rows[0].cells;
				List<int> list = new List<int>(cells.Length);
				int num2 = dragHeader.im;
				int item = dragHeader.i;
				if (SortHeader != null)
				{
					int[] sortHeader = SortHeader;
					foreach (int num3 in sortHeader)
					{
						CELL cELL = cells[num3];
						if (dragHeader.im == cELL.INDEX)
						{
							num2 = cELL.COLUMN.INDEX;
						}
						if (dragHeader.i == cELL.INDEX)
						{
							item = cELL.COLUMN.INDEX;
						}
					}
				}
				CELL[] array2 = cells;
				for (int i = 0; i < array2.Length; i++)
				{
					int iNDEX = array2[i].COLUMN.INDEX;
					if (iNDEX == num2)
					{
						if (dragHeader.last)
						{
							list.Add(iNDEX);
						}
						if (list.Contains(item))
						{
							list.Remove(item);
						}
						list.Add(item);
					}
					if (!list.Contains(iNDEX))
					{
						list.Add(iNDEX);
					}
				}
				SortHeader = list.ToArray();
				LoadLayout();
			}
			dragHeader = null;
			if (hand)
			{
				((Control)this).Invalidate();
				OnTouchCancel();
				return;
			}
		}
		if (dragBody != null)
		{
			bool hand2 = dragBody.hand;
			if (hand2 && dragBody.im != -1)
			{
				if (rows == null)
				{
					return;
				}
				List<int> list2 = new List<int>(rows.Length);
				int num4 = dragBody.im;
				int item2 = dragBody.i;
				RowTemplate[] array3 = rows;
				foreach (RowTemplate rowTemplate in array3)
				{
					rowTemplate.hover = false;
					if (dragBody.im == rowTemplate.INDEX)
					{
						rowTemplate.hover = true;
						num4 = rowTemplate.INDEX_REAL;
					}
					if (dragBody.i == rowTemplate.INDEX)
					{
						item2 = rowTemplate.INDEX_REAL;
					}
				}
				SetIndex(dragBody.im);
				array3 = rows;
				for (int i = 0; i < array3.Length; i++)
				{
					int iNDEX_REAL = array3[i].INDEX_REAL;
					if (iNDEX_REAL <= -1)
					{
						continue;
					}
					if (iNDEX_REAL == num4)
					{
						if (dragBody.last)
						{
							list2.Add(iNDEX_REAL);
						}
						if (list2.Contains(item2))
						{
							list2.Remove(item2);
						}
						list2.Add(item2);
					}
					if (!list2.Contains(iNDEX_REAL))
					{
						list2.Add(iNDEX_REAL);
					}
				}
				SortData = list2.ToArray();
				LoadLayout();
				this.SortRows?.Invoke(this, new IntEventArgs(-1));
			}
			dragBody = null;
			if (hand2)
			{
				((Control)this).Invalidate();
				OnTouchCancel();
				return;
			}
		}
		if (ScrollBar.MouseUpY() && ScrollBar.MouseUpX())
		{
			if (rows == null)
			{
				return;
			}
			if (OnTouchUp())
			{
				if (cellMouseDown == null)
				{
					return;
				}
				for (int j = 0; j < rows.Length; j++)
				{
					RowTemplate rowTemplate2 = rows[j];
					for (int k = 0; k < rowTemplate2.cells.Length; k++)
					{
						if (MouseUpRow(rows, rowTemplate2, rowTemplate2.cells[k], e, j, k))
						{
							return;
						}
					}
				}
			}
		}
		cellMouseDown = null;
	}

	private bool MouseUpRow(RowTemplate[] rows, RowTemplate it, CELL cell, MouseEventArgs e, int i_r, int i_c)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Invalid comparison between Unknown and I4
		//IL_0830: Unknown result type (might be due to invalid IL or missing references)
		//IL_083a: Invalid comparison between Unknown and I4
		//IL_0814: Unknown result type (might be due to invalid IL or missing references)
		//IL_081e: Invalid comparison between Unknown and I4
		RowTemplate it2 = it;
		CELL cell2 = cell;
		if (cellMouseDown == cell2 && cell2.MouseDown > 0)
		{
			int r_x;
			int r_y;
			int offset_x;
			int offset_xi;
			int offset_y;
			int i_row;
			int i_cel;
			int mode;
			CELL cELL = CellContains(rows, sethover: true, e.X, e.Y, out r_x, out r_y, out offset_x, out offset_xi, out offset_y, out i_row, out i_cel, out mode);
			if (cELL == null || i_r != i_row || i_c != i_cel)
			{
				cell2.MouseDown = 0;
			}
			else
			{
				if (selectedIndex.Length == 1)
				{
					SelectedIndex = i_r;
				}
				if ((int)e.Button == 1048576)
				{
					if (cell2 is TCellCheck tCellCheck)
					{
						if (tCellCheck.CONTAIN_REAL(r_x, r_y))
						{
							if (tCellCheck.COLUMN is ColumnCheck { Call: not null } columnCheck)
							{
								bool flag = columnCheck.Call(!tCellCheck.Checked, it2.RECORD, i_r, i_c);
								if (tCellCheck.Checked != flag)
								{
									tCellCheck.Checked = flag;
									SetValue(cell2, tCellCheck.Checked);
									this.CheckedChanged?.Invoke(this, new TableCheckEventArgs(tCellCheck.Checked, it2.RECORD, i_r, i_c));
								}
							}
							else if (tCellCheck.AutoCheck)
							{
								tCellCheck.Checked = !tCellCheck.Checked;
								SetValue(cell2, tCellCheck.Checked);
								this.CheckedChanged?.Invoke(this, new TableCheckEventArgs(tCellCheck.Checked, it2.RECORD, i_r, i_c));
							}
						}
					}
					else if (cell2 is TCellRadio tCellRadio)
					{
						if (tCellRadio.CONTAIN_REAL(r_x, r_y) && !tCellRadio.Checked)
						{
							bool flag2 = false;
							if (tCellRadio.COLUMN is ColumnRadio { Call: not null } columnRadio)
							{
								if (columnRadio.Call(arg1: true, it2.RECORD, i_r, i_c))
								{
									flag2 = true;
								}
							}
							else if (tCellRadio.AutoCheck)
							{
								flag2 = true;
							}
							if (flag2)
							{
								for (int i = 0; i < rows.Length; i++)
								{
									if (i != i_r)
									{
										CELL cELL2 = rows[i].cells[i_c];
										if (cELL2 is TCellRadio { Checked: not false } tCellRadio2)
										{
											tCellRadio2.Checked = false;
											SetValue(cELL2, false);
										}
									}
								}
								tCellRadio.Checked = true;
								SetValue(cell2, tCellRadio.Checked);
								this.CheckedChanged?.Invoke(this, new TableCheckEventArgs(tCellRadio.Checked, it2.RECORD, i_r, i_c));
							}
						}
					}
					else
					{
						TCellSwitch switchCell = cell2 as TCellSwitch;
						if (switchCell != null)
						{
							if (switchCell.CONTAIN_REAL(r_x, r_y) && !switchCell.Loading)
							{
								Column cOLUMN = switchCell.COLUMN;
								ColumnSwitch columnSwitch = cOLUMN as ColumnSwitch;
								if (columnSwitch != null && columnSwitch.Call != null)
								{
									switchCell.Loading = true;
									ITask.Run(delegate
									{
										bool flag4 = columnSwitch.Call(!switchCell.Checked, it2.RECORD, i_r, i_c);
										if (switchCell.Checked != flag4)
										{
											switchCell.Checked = flag4;
											SetValue(cell2, flag4);
										}
									}).ContinueWith(delegate
									{
										switchCell.Loading = false;
									});
								}
								else if (switchCell.AutoCheck)
								{
									switchCell.Checked = !switchCell.Checked;
									SetValue(cell2, switchCell.Checked);
									this.CheckedChanged?.Invoke(this, new TableCheckEventArgs(switchCell.Checked, it2.RECORD, i_r, i_c));
								}
							}
						}
						else if (it2.IsColumn && cell2.COLUMN.SortOrder && cell2 is TCellColumn tCellColumn)
						{
							SortMode sortMode = SortMode.NONE;
							if (tCellColumn.rect_up.Contains(r_x, r_y))
							{
								sortMode = SortMode.ASC;
							}
							else if (tCellColumn.rect_down.Contains(r_x, r_y))
							{
								sortMode = SortMode.DESC;
							}
							else
							{
								sortMode = tCellColumn.COLUMN.SortMode + 1;
								if (sortMode > SortMode.DESC)
								{
									sortMode = SortMode.NONE;
								}
							}
							if (tCellColumn.COLUMN.SetSortMode(sortMode))
							{
								CELL[] cells = it2.cells;
								foreach (CELL cELL3 in cells)
								{
									if (cELL3.COLUMN.SortOrder && cELL3.INDEX != i_c)
									{
										cELL3.COLUMN.SetSortMode(SortMode.NONE);
									}
								}
								SortModeEventHandler? sortModeChanged = this.SortModeChanged;
								if (sortModeChanged != null && sortModeChanged(this, new TableSortModeEventArgs(sortMode, tCellColumn.COLUMN)))
								{
									((Control)this).Invalidate();
								}
								else
								{
									((Control)this).Invalidate();
									switch (sortMode)
									{
									case SortMode.ASC:
										SortDataASC(tCellColumn.COLUMN.Key);
										break;
									case SortMode.DESC:
										SortDataDESC(tCellColumn.COLUMN.Key);
										break;
									default:
										SortData = null;
										break;
									}
									LoadLayout();
									this.SortRows?.Invoke(this, new IntEventArgs(i_c));
								}
							}
						}
						else if (cell2 is Template template)
						{
							foreach (ICell item in template.Value)
							{
								if (item is CellLink { ExtraMouseDown: not false, Rect: var rect } cellLink)
								{
									if (rect.Contains(r_x, r_y))
									{
										cellLink.Click();
										this.CellButtonClick?.Invoke(this, new TableButtonEventArgs(cellLink, it2.RECORD, i_r, i_c, e));
									}
									cellLink.ExtraMouseDown = false;
								}
							}
						}
					}
				}
				bool num = cell2.MouseDown == 2;
				cell2.MouseDown = 0;
				this.CellClick?.Invoke(this, new TableClickEventArgs(it2.RECORD, i_row, i_cel, new Rectangle(cELL.RECT.X - offset_x, cELL.RECT.Y - offset_y, cELL.RECT.Width, cELL.RECT.Height), e));
				bool flag3 = false;
				if (num)
				{
					this.CellDoubleClick?.Invoke(this, new TableClickEventArgs(it2.RECORD, i_row, i_cel, new Rectangle(cELL.RECT.X - offset_x, cELL.RECT.Y - offset_y, cELL.RECT.Width, cELL.RECT.Height), e));
					if ((int)e.Button == 1048576 && editmode == TEditMode.DoubleClick)
					{
						flag3 = true;
					}
				}
				else if ((int)e.Button == 1048576 && editmode == TEditMode.Click)
				{
					flag3 = true;
				}
				if (flag3)
				{
					EditModeClose();
					if (CanEditMode(it2, cELL))
					{
						int num2 = ScrollLine(i_row, rows);
						OnEditMode(it2, cELL, i_row, i_cel, offset_xi, offset_y - num2);
					}
				}
				else if (cell2 is Template template2)
				{
					foreach (ICell item2 in template2.Value)
					{
						if (item2.DropDownItems != null && item2.DropDownItems.Count > 0 && item2.Rect.Contains(r_x, r_y))
						{
							subForm?.IClose();
							subForm = null;
							Rectangle rect2 = item2.Rect;
							rect2.Offset(-offset_xi, -offset_y);
							subForm = new LayeredFormSelectDown(this, item2, rect2, item2.DropDownItems);
							((Form)subForm).Show((IWin32Window)(object)this);
							return true;
						}
					}
				}
			}
			return true;
		}
		return false;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		((Control)this).OnMouseMove(e);
		if (moveheaders.Length != 0)
		{
			MoveHeader[] array = moveheaders;
			foreach (MoveHeader moveHeader in array)
			{
				if (!moveHeader.MouseDown)
				{
					continue;
				}
				int num = moveHeader.width + e.X - moveHeader.x;
				if (num >= moveHeader.min_width)
				{
					if (tmpcol_width.ContainsKey(moveHeader.i))
					{
						tmpcol_width[moveHeader.i] = num;
					}
					else
					{
						tmpcol_width.Add(moveHeader.i, num);
					}
					if (LoadLayout())
					{
						((Control)this).Invalidate();
					}
					SetCursor(CursorType.VSplit);
				}
				return;
			}
		}
		if (dragHeader != null)
		{
			SetCursor(CursorType.SizeAll);
			dragHeader.hand = true;
			dragHeader.xr = e.X - dragHeader.x;
			if (rows == null)
			{
				return;
			}
			int ex = dragHeader.x + dragHeader.xr;
			CELL[] cells = rows[0].cells;
			dragHeader.last = e.X > dragHeader.x;
			if (CellContains(rows, sethover: false, ex, e.Y, out var _, out var _, out var _, out var _, out var _, out var _, out var i_cel, out var _) != null)
			{
				CELL cELL = cells[i_cel];
				if (cELL.INDEX == dragHeader.i)
				{
					dragHeader.im = -1;
				}
				else
				{
					dragHeader.im = cELL.INDEX;
				}
				((Control)this).Invalidate();
			}
			else
			{
				if (cells[^1].INDEX == dragHeader.i)
				{
					dragHeader.im = -1;
				}
				else
				{
					dragHeader.im = cells[^1].INDEX;
				}
				((Control)this).Invalidate();
			}
		}
		else if (dragBody != null)
		{
			SetCursor(CursorType.SizeAll);
			dragBody.hand = true;
			dragBody.xr = e.Y - dragBody.x;
			if (rows == null)
			{
				return;
			}
			int ey = dragBody.x + dragBody.xr;
			dragBody.last = e.Y > dragBody.x;
			if (CellContains(rows, sethover: false, e.X, ey, out var _, out var _, out var _, out var _, out var _, out var i_row2, out var _, out var _) != null)
			{
				if (i_row2 == dragBody.i)
				{
					dragBody.im = -1;
				}
				else
				{
					dragBody.im = i_row2;
				}
				((Control)this).Invalidate();
			}
			else
			{
				if (rows[rows.Length - 1].INDEX == dragBody.i)
				{
					dragBody.im = -1;
				}
				else
				{
					dragBody.im = rows[rows.Length - 1].INDEX;
				}
				((Control)this).Invalidate();
			}
		}
		else if (ScrollBar.MouseMoveY(e.Location) && ScrollBar.MouseMoveX(e.Location) && OnTouchMove(e.X, e.Y))
		{
			if (rows == null || inEditMode)
			{
				return;
			}
			int r_x3;
			int r_y3;
			int offset_x3;
			int offset_xi3;
			int offset_y3;
			int i_row3;
			int i_cel3;
			int mode3;
			CELL cELL2 = CellContains(rows, sethover: true, e.X, e.Y, out r_x3, out r_y3, out offset_x3, out offset_xi3, out offset_y3, out i_row3, out i_cel3, out mode3);
			if (cELL2 == null)
			{
				RowTemplate[] array2 = rows;
				foreach (RowTemplate rowTemplate in array2)
				{
					if (rowTemplate.IsColumn)
					{
						continue;
					}
					rowTemplate.Hover = false;
					CELL[] cells2 = rowTemplate.cells;
					foreach (CELL cELL3 in cells2)
					{
						if (cELL3 is TCellSort tCellSort)
						{
							tCellSort.Hover = false;
						}
						else
						{
							if (!(cELL3 is Template template))
							{
								continue;
							}
							foreach (ICell item in template.Value)
							{
								if (item is CellLink cellLink)
								{
									cellLink.ExtraMouseHover = false;
								}
							}
						}
					}
				}
				SetCursor(val: false);
				return;
			}
			if (mode3 > 0)
			{
				for (int k = 1; k < rows.Length; k++)
				{
					rows[k].Hover = false;
					CELL[] cells2 = rows[k].cells;
					for (int i = 0; i < cells2.Length; i++)
					{
						if (!(cells2[i] is Template template2))
						{
							continue;
						}
						foreach (ICell item2 in template2.Value)
						{
							if (item2 is CellLink cellLink2)
							{
								cellLink2.ExtraMouseHover = false;
							}
						}
					}
				}
				TCellColumn tCellColumn = (TCellColumn)cELL2;
				if (moveheaders.Length != 0)
				{
					MoveHeader[] array = moveheaders;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].rect.Contains(r_x3, r_y3))
						{
							SetCursor(CursorType.VSplit);
							return;
						}
					}
				}
				if (tCellColumn.SortWidth > 0)
				{
					SetCursor(val: true);
				}
				else if (has_check && tCellColumn.COLUMN is ColumnCheck { NoTitle: not false } && tCellColumn.CONTAIN_REAL(r_x3, r_y3))
				{
					SetCursor(val: true);
				}
				else if (ColumnDragSort)
				{
					SetCursor(CursorType.SizeAll);
				}
				else
				{
					SetCursor(val: false);
				}
				return;
			}
			int num2 = 0;
			for (int l = 1; l < rows.Length; l++)
			{
				if (l == i_row3)
				{
					if (cELL2 is TCellSort tCellSort2)
					{
						tCellSort2.Hover = tCellSort2.Contains(r_x3, r_y3);
						if (tCellSort2.Hover)
						{
							num2++;
						}
					}
					rows[l].Hover = true;
					continue;
				}
				rows[l].Hover = false;
				CELL[] cells2 = rows[l].cells;
				foreach (CELL cELL4 in cells2)
				{
					if (cELL4 is TCellSort tCellSort3)
					{
						tCellSort3.Hover = false;
					}
					else
					{
						if (!(cELL4 is Template template3))
						{
							continue;
						}
						foreach (ICell item3 in template3.Value)
						{
							if (item3 is CellLink cellLink3)
							{
								cellLink3.ExtraMouseHover = false;
							}
						}
					}
				}
			}
			if (num2 > 0)
			{
				SetCursor(CursorType.SizeAll);
			}
			else if (cELL2.ROW.CanExpand && cELL2.ROW.RectExpand.Contains(r_x3, r_y3))
			{
				SetCursor(val: true);
			}
			else
			{
				SetCursor(MouseMoveRow(cELL2, r_x3, r_y3, offset_x3, offset_xi3, offset_y3));
			}
		}
		else
		{
			ILeave();
		}
	}

	private bool MouseMoveRow(CELL cel, int x, int y, int offset_x, int offset_xi, int offset_y)
	{
		if (cel is TCellCheck tCellCheck)
		{
			if (tCellCheck.AutoCheck && tCellCheck.CONTAIN_REAL(x, y))
			{
				return true;
			}
			return false;
		}
		if (cel is TCellRadio tCellRadio)
		{
			if (tCellRadio.AutoCheck && tCellRadio.CONTAIN_REAL(x, y))
			{
				return true;
			}
			return false;
		}
		if (cel is TCellSwitch tCellSwitch)
		{
			if (tCellSwitch.AutoCheck || tCellSwitch.COLUMN is ColumnSwitch { Call: not null })
			{
				tCellSwitch.ExtraMouseHover = tCellSwitch.CONTAIN_REAL(x, y);
				if (tCellSwitch.ExtraMouseHover)
				{
					return true;
				}
			}
			else
			{
				tCellSwitch.ExtraMouseHover = false;
			}
			return false;
		}
		if (cel is Template template)
		{
			ICell cell = null;
			int num = 0;
			foreach (ICell item in template.Value)
			{
				if (item is CellLink cellLink)
				{
					if (cellLink.Enabled)
					{
						cellLink.ExtraMouseHover = cellLink.Rect.Contains(x, y);
						if (cellLink.ExtraMouseHover)
						{
							num++;
							cell = cellLink;
						}
					}
					else
					{
						cellLink.ExtraMouseHover = false;
					}
				}
				else if (item is CellImage { Tooltip: not null, Rect: var rect } cellImage && rect.Contains(x, y))
				{
					cell = cellImage;
				}
			}
			if (cell == null)
			{
				CloseTip();
			}
			else if (cell is CellLink cellLink2)
			{
				if (cellLink2.Tooltip == null)
				{
					CloseTip();
				}
				else
				{
					Rectangle rectangle = ((Control)this).RectangleToScreen(((Control)this).ClientRectangle);
					Rectangle rect2 = new Rectangle(rectangle.X + cellLink2.Rect.X - offset_xi, rectangle.Y + cellLink2.Rect.Y - offset_y, cellLink2.Rect.Width, cellLink2.Rect.Height);
					if (tooltipForm == null)
					{
						tooltipForm = new TooltipForm((Control)(object)this, rect2, cellLink2.Tooltip, new TooltipConfig
						{
							Font = ((Control)this).Font,
							ArrowAlign = TAlign.Top
						});
						((Form)tooltipForm).Show((IWin32Window)(object)this);
					}
					else
					{
						tooltipForm.SetText(rect2, cellLink2.Tooltip);
					}
				}
			}
			else if (cell is CellImage cellImage2)
			{
				if (cellImage2.Tooltip == null)
				{
					CloseTip();
				}
				else
				{
					Rectangle rectangle2 = ((Control)this).RectangleToScreen(((Control)this).ClientRectangle);
					Rectangle rect3 = new Rectangle(rectangle2.X + cellImage2.Rect.X - offset_xi, rectangle2.Y + cellImage2.Rect.Y - offset_y, cellImage2.Rect.Width, cellImage2.Rect.Height);
					if (tooltipForm == null)
					{
						tooltipForm = new TooltipForm((Control)(object)this, rect3, cellImage2.Tooltip, new TooltipConfig
						{
							Font = ((Control)this).Font,
							ArrowAlign = TAlign.Top
						});
						((Form)tooltipForm).Show((IWin32Window)(object)this);
					}
					else
					{
						tooltipForm.SetText(rect3, cellImage2.Tooltip);
					}
				}
			}
			return num > 0;
		}
		if (ShowTip)
		{
			string text = cel.INDEX + "_" + cel.ROW.INDEX;
			if (oldmove != text)
			{
				CloseTip();
				oldmove = text;
				if (!cel.COLUMN.LineBreak && cel.MinWidth > cel.RECT_REAL.Width)
				{
					string text2 = cel.ToString();
					if (!string.IsNullOrEmpty(text2))
					{
						Rectangle rectangle3 = ((Control)this).RectangleToScreen(((Control)this).ClientRectangle);
						Rectangle rect4 = new Rectangle(rectangle3.X + cel.RECT.X - offset_xi, rectangle3.Y + cel.RECT.Y - offset_y, cel.RECT.Width, cel.RECT.Height);
						if (tooltipForm == null)
						{
							tooltipForm = new TooltipForm((Control)(object)this, rect4, text2, new TooltipConfig
							{
								Font = ((Control)this).Font,
								ArrowAlign = TAlign.Top
							});
							((Form)tooltipForm).Show((IWin32Window)(object)this);
						}
						else
						{
							tooltipForm.SetText(rect4, text2);
						}
					}
				}
			}
		}
		return false;
	}

	private void CloseTip(bool clear = false)
	{
		tooltipForm?.IClose();
		tooltipForm = null;
		if (clear)
		{
			oldmove = null;
		}
	}

	private CELL? CellContains(RowTemplate[] rows, bool sethover, int ex, int ey, out int r_x, out int r_y, out int offset_x, out int offset_xi, out int offset_y, out int i_row, out int i_cel, out int mode)
	{
		mode = 0;
		int valueX = ScrollBar.ValueX;
		int valueY = ScrollBar.ValueY;
		int num = ex + valueX;
		int num2 = ey + valueY;
		foreach (RowTemplate rowTemplate in rows)
		{
			if (rowTemplate.IsColumn)
			{
				if (fixedHeader)
				{
					if (!rowTemplate.CONTAINS(ex, ey))
					{
						continue;
					}
					mode = 2;
					List<int> list = new List<int>();
					if (showFixedColumnL && fixedColumnL != null)
					{
						foreach (int item in fixedColumnL)
						{
							list.Add(item);
							CELL cELL = rowTemplate.cells[item];
							if (cELL.CONTAIN(ex, ey))
							{
								r_x = ex;
								r_y = ey;
								offset_x = (offset_xi = 0);
								offset_y = valueY;
								i_row = rowTemplate.INDEX;
								i_cel = item;
								return cELL;
							}
						}
					}
					if (showFixedColumnR && fixedColumnR != null)
					{
						foreach (int item2 in fixedColumnR)
						{
							list.Add(item2);
							CELL cELL2 = rowTemplate.cells[item2];
							if (cELL2.CONTAIN(ex + sFixedR, ey))
							{
								r_x = ex + sFixedR;
								r_y = ey;
								offset_x = -sFixedR;
								offset_xi = sFixedR;
								offset_y = valueY;
								i_row = rowTemplate.INDEX;
								i_cel = item2;
								return cELL2;
							}
						}
					}
					for (int j = 0; j < rowTemplate.cells.Length; j++)
					{
						if (!list.Contains(j))
						{
							CELL cELL3 = rowTemplate.cells[j];
							if (cELL3.CONTAIN(num, ey))
							{
								r_x = num;
								r_y = ey;
								offset_x = (offset_xi = valueX);
								offset_y = valueY;
								i_row = rowTemplate.INDEX;
								i_cel = j;
								return cELL3;
							}
						}
					}
				}
				else
				{
					if (!rowTemplate.CONTAINS(ex, num2))
					{
						continue;
					}
					mode = 1;
					List<int> list2 = new List<int>();
					if (showFixedColumnL && fixedColumnL != null)
					{
						foreach (int item3 in fixedColumnL)
						{
							list2.Add(item3);
							CELL cELL4 = rowTemplate.cells[item3];
							if (cELL4.CONTAIN(ex, num2))
							{
								r_x = ex;
								r_y = num2;
								offset_x = (offset_xi = 0);
								offset_y = valueY;
								i_row = rowTemplate.INDEX;
								i_cel = item3;
								return cELL4;
							}
						}
					}
					if (showFixedColumnR && fixedColumnR != null)
					{
						foreach (int item4 in fixedColumnR)
						{
							list2.Add(item4);
							CELL cELL5 = rowTemplate.cells[item4];
							if (cELL5.CONTAIN(ex + sFixedR, num2))
							{
								r_x = ex + sFixedR;
								r_y = num2;
								offset_x = -sFixedR;
								offset_xi = sFixedR;
								offset_y = valueY;
								i_row = rowTemplate.INDEX;
								i_cel = item4;
								return cELL5;
							}
						}
					}
					for (int k = 0; k < rowTemplate.cells.Length; k++)
					{
						if (!list2.Contains(k))
						{
							CELL cELL6 = rowTemplate.cells[k];
							if (cELL6.CONTAIN(num, num2))
							{
								r_x = num;
								r_y = num2;
								offset_x = (offset_xi = valueX);
								offset_y = valueY;
								i_row = rowTemplate.INDEX;
								i_cel = k;
								return cELL6;
							}
						}
					}
				}
			}
			else
			{
				if (!rowTemplate.Contains(ex, num2, sethover))
				{
					continue;
				}
				List<int> list3 = new List<int>();
				if (showFixedColumnL && fixedColumnL != null)
				{
					foreach (int item5 in fixedColumnL)
					{
						list3.Add(item5);
						CELL cELL7 = rowTemplate.cells[item5];
						if (cELL7.CONTAIN(ex, num2))
						{
							r_x = ex;
							r_y = num2;
							offset_x = (offset_xi = 0);
							offset_y = valueY;
							i_row = rowTemplate.INDEX;
							i_cel = item5;
							return cELL7;
						}
					}
				}
				if (showFixedColumnR && fixedColumnR != null)
				{
					foreach (int item6 in fixedColumnR)
					{
						list3.Add(item6);
						CELL cELL8 = rowTemplate.cells[item6];
						if (cELL8.CONTAIN(ex + sFixedR, num2))
						{
							r_x = ex + sFixedR;
							r_y = num2;
							offset_x = -sFixedR;
							offset_xi = sFixedR;
							offset_y = valueY;
							i_row = rowTemplate.INDEX;
							i_cel = item6;
							return cELL8;
						}
					}
				}
				for (int l = 0; l < rowTemplate.cells.Length; l++)
				{
					if (!list3.Contains(l))
					{
						CELL cELL9 = rowTemplate.cells[l];
						if (cELL9.CONTAIN(num, num2))
						{
							r_x = num;
							r_y = num2;
							offset_x = (offset_xi = valueX);
							offset_y = valueY;
							i_row = rowTemplate.INDEX;
							i_cel = l;
							return cELL9;
						}
					}
				}
			}
		}
		r_x = (r_y = (offset_x = (offset_xi = (offset_y = (i_row = (i_cel = 0))))));
		return null;
	}

	private List<SortModel> SortDatas(string key)
	{
		if (dataTmp == null)
		{
			return new List<SortModel>(0);
		}
		List<SortModel> list = new List<SortModel>(dataTmp.rows.Length);
		for (int i = 0; i < dataTmp.rows.Length; i++)
		{
			list.Add(new SortModel(i, OGetValue(dataTmp, i, key)?.ToString()));
		}
		return list;
	}

	private void SortDataASC(string key)
	{
		List<SortModel> list = SortDatas(key);
		if (this.CustomSort == null)
		{
			list.Sort((SortModel x, SortModel y) => FilesNameComparerClass.Compare(x.v, y.v));
		}
		else
		{
			list.Sort((SortModel x, SortModel y) => this.CustomSort(x.v, y.v));
		}
		List<int> list2 = new List<int>(list.Count);
		foreach (SortModel item in list)
		{
			list2.Add(item.i);
		}
		SortData = list2.ToArray();
	}

	private void SortDataDESC(string key)
	{
		List<SortModel> list = SortDatas(key);
		if (this.CustomSort == null)
		{
			list.Sort((SortModel y, SortModel x) => FilesNameComparerClass.Compare(x.v, y.v));
		}
		else
		{
			list.Sort((SortModel y, SortModel x) => this.CustomSort(x.v, y.v));
		}
		List<int> list2 = new List<int>(list.Count);
		foreach (SortModel item in list)
		{
			list2.Add(item.i);
		}
		SortData = list2.ToArray();
	}

	protected override void OnGotFocus(EventArgs e)
	{
		focused = true;
		((Control)this).OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		((Control)this).OnLostFocus(e);
		focused = false;
		if (LostFocusClearSelection)
		{
			SelectedIndex = -1;
		}
		CloseTip(clear: true);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		((Control)this).OnMouseLeave(e);
		ScrollBar.Leave();
		ILeave();
		CloseTip(clear: true);
	}

	protected override void OnLeave(EventArgs e)
	{
		((Control)this).OnLeave(e);
		ScrollBar.Leave();
		ILeave();
		CloseTip(clear: true);
	}

	private void ILeave()
	{
		SetCursor(val: false);
		if (rows == null || inEditMode)
		{
			return;
		}
		RowTemplate[] array = rows;
		foreach (RowTemplate obj in array)
		{
			obj.Hover = false;
			CELL[] cells = obj.cells;
			foreach (CELL cELL in cells)
			{
				if (cELL is TCellSort tCellSort)
				{
					tCellSort.Hover = false;
				}
				else
				{
					if (!(cELL is Template template))
					{
						continue;
					}
					foreach (ICell item in template.Value)
					{
						if (item is CellLink cellLink)
						{
							cellLink.ExtraMouseHover = false;
						}
					}
				}
			}
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		subForm?.IClose();
		subForm = null;
		CloseTip();
		ScrollBar.MouseWheel(e.Delta);
		base.OnMouseWheel(e);
	}

	protected override bool OnTouchScrollX(int value)
	{
		return ScrollBar.MouseWheelX(value);
	}

	protected override bool OnTouchScrollY(int value)
	{
		return ScrollBar.MouseWheelY(value);
	}

	internal static void PaintButton(Canvas g, Font font, int gap, Rectangle rect_read, CellButton btn, bool enable)
	{
		float num = ((btn.Shape == TShape.Round || btn.Shape == TShape.Circle) ? ((float)rect_read.Height) : ((float)btn.Radius * Config.Dpi));
		if (btn.Type == TTypeMini.Default)
		{
			Color color = Colour.DefaultColor.Get("Button");
			Color color2 = Colour.Primary.Get("Button");
			Color color3;
			Color color4;
			if (btn.BorderWidth > 0f)
			{
				color3 = Colour.PrimaryHover.Get("Button");
				color4 = Colour.PrimaryActive.Get("Button");
			}
			else
			{
				color3 = Colour.FillSecondary.Get("Button");
				color4 = Colour.Fill.Get("Button");
			}
			if (btn.Fore.HasValue)
			{
				color = btn.Fore.Value;
			}
			if (btn.BackHover.HasValue)
			{
				color3 = btn.BackHover.Value;
			}
			if (btn.BackActive.HasValue)
			{
				color4 = btn.BackActive.Value;
			}
			GraphicsPath val = PathButton(rect_read, btn, num);
			try
			{
				if (btn.AnimationClick)
				{
					float num2 = (float)rect_read.Width + (float)gap * btn.AnimationClickValue;
					float num3 = (float)rect_read.Height + (float)gap * btn.AnimationClickValue;
					float alpha = 100f * (1f - btn.AnimationClickValue);
					GraphicsPath val2 = new RectangleF((float)rect_read.X + ((float)rect_read.Width - num2) / 2f, (float)rect_read.Y + ((float)rect_read.Height - num3) / 2f, num2, num3).RoundPath(num, btn.Shape);
					try
					{
						val2.AddPath(val, false);
						g.Fill(Helper.ToColor(alpha, color2), val2);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				if (enable && btn.Enabled)
				{
					if (!btn.Ghost)
					{
						GraphicsPath val3 = new RectangleF(rect_read.X, rect_read.Y + 3, rect_read.Width, rect_read.Height).RoundPath(num);
						try
						{
							val3.AddPath(val, false);
							g.Fill(Colour.FillQuaternary.Get("Button"), val3);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
						g.Fill(btn.DefaultBack ?? Colour.DefaultBg.Get("Button"), val);
					}
					if (btn.BorderWidth > 0f)
					{
						float width = btn.BorderWidth * Config.Dpi;
						if (btn.ExtraMouseDown)
						{
							g.Draw(color4, width, val);
							PaintButton(g, font, btn, color4, rect_read);
						}
						else if (btn.AnimationHover)
						{
							Color overlay = Helper.ToColor(btn.AnimationHoverValue, color3);
							g.Draw(Colour.DefaultBorder.Get("Button").BlendColors(overlay), width, val);
							PaintButton(g, font, btn, color.BlendColors(overlay), rect_read);
						}
						else if (btn.ExtraMouseHover)
						{
							g.Draw(color3, width, val);
							PaintButton(g, font, btn, color3, rect_read);
						}
						else
						{
							g.Draw(btn.DefaultBorderColor ?? Colour.DefaultBorder.Get("Button"), width, val);
							PaintButton(g, font, btn, color, rect_read);
						}
					}
					else
					{
						if (btn.ExtraMouseDown)
						{
							g.Fill(color4, val);
						}
						else if (btn.AnimationHover)
						{
							g.Fill(Helper.ToColor(btn.AnimationHoverValue, color3), val);
						}
						else if (btn.ExtraMouseHover)
						{
							g.Fill(color3, val);
						}
						PaintButton(g, font, btn, color, rect_read);
					}
				}
				else
				{
					if (btn.BorderWidth > 0f)
					{
						g.Fill(Colour.FillTertiary.Get("Button"), val);
					}
					PaintButton(g, font, btn, Colour.TextQuaternary.Get("Button"), rect_read);
				}
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		Color color5;
		Color color6;
		Color color7;
		Color color8;
		switch (btn.Type)
		{
		case TTypeMini.Error:
			color5 = Colour.Error.Get("Button");
			color6 = Colour.ErrorColor.Get("Button");
			color7 = Colour.ErrorHover.Get("Button");
			color8 = Colour.ErrorActive.Get("Button");
			break;
		case TTypeMini.Success:
			color5 = Colour.Success.Get("Button");
			color6 = Colour.SuccessColor.Get("Button");
			color7 = Colour.SuccessHover.Get("Button");
			color8 = Colour.SuccessActive.Get("Button");
			break;
		case TTypeMini.Info:
			color5 = Colour.Info.Get("Button");
			color6 = Colour.InfoColor.Get("Button");
			color7 = Colour.InfoHover.Get("Button");
			color8 = Colour.InfoActive.Get("Button");
			break;
		case TTypeMini.Warn:
			color5 = Colour.Warning.Get("Button");
			color6 = Colour.WarningColor.Get("Button");
			color7 = Colour.WarningHover.Get("Button");
			color8 = Colour.WarningActive.Get("Button");
			break;
		default:
			color5 = Colour.Primary.Get("Button");
			color6 = Colour.PrimaryColor.Get("Button");
			color7 = Colour.PrimaryHover.Get("Button");
			color8 = Colour.PrimaryActive.Get("Button");
			break;
		}
		if (btn.Fore.HasValue)
		{
			color6 = btn.Fore.Value;
		}
		if (btn.Back.HasValue)
		{
			color5 = btn.Back.Value;
		}
		if (btn.BackHover.HasValue)
		{
			color7 = btn.BackHover.Value;
		}
		if (btn.BackActive.HasValue)
		{
			color8 = btn.BackActive.Value;
		}
		GraphicsPath val4 = PathButton(rect_read, btn, num);
		try
		{
			if (btn.AnimationClick)
			{
				float num4 = (float)rect_read.Width + (float)gap * btn.AnimationClickValue;
				float num5 = (float)rect_read.Height + (float)gap * btn.AnimationClickValue;
				float alpha2 = 100f * (1f - btn.AnimationClickValue);
				GraphicsPath val5 = new RectangleF((float)rect_read.X + ((float)rect_read.Width - num4) / 2f, (float)rect_read.Y + ((float)rect_read.Height - num5) / 2f, num4, num5).RoundPath(num, btn.Shape);
				try
				{
					val5.AddPath(val4, false);
					g.Fill(Helper.ToColor(alpha2, color5), val5);
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
			}
			if (btn.Ghost)
			{
				if (btn.BorderWidth > 0f)
				{
					float width2 = btn.BorderWidth * Config.Dpi;
					if (btn.ExtraMouseDown)
					{
						g.Draw(color8, width2, val4);
						PaintButton(g, font, btn, color8, rect_read);
						return;
					}
					if (btn.AnimationHover)
					{
						Color overlay2 = Helper.ToColor(btn.AnimationHoverValue, color7);
						g.Draw(((enable && btn.Enabled) ? color5 : Colour.FillTertiary.Get("Button")).BlendColors(overlay2), width2, val4);
						PaintButton(g, font, btn, color5.BlendColors(overlay2), rect_read);
						return;
					}
					if (btn.ExtraMouseHover)
					{
						g.Draw(color7, width2, val4);
						PaintButton(g, font, btn, color7, rect_read);
						return;
					}
					if (enable && btn.Enabled)
					{
						Brush val6 = btn.BackExtend.BrushEx(rect_read, color5);
						try
						{
							g.Draw(val6, width2, val4);
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
					else
					{
						g.Draw(Colour.FillTertiary.Get("Button"), width2, val4);
					}
					PaintButton(g, font, btn, (enable && btn.Enabled) ? color5 : Colour.TextQuaternary.Get("Button"), rect_read);
				}
				else
				{
					PaintButton(g, font, btn, (enable && btn.Enabled) ? color5 : Colour.TextQuaternary.Get("Button"), rect_read);
				}
				return;
			}
			if (enable && btn.Enabled)
			{
				GraphicsPath val7 = new RectangleF(rect_read.X, rect_read.Y + 3, rect_read.Width, rect_read.Height).RoundPath(num);
				try
				{
					val7.AddPath(val4, false);
					g.Fill(color5.rgba((Config.Mode == TMode.Dark) ? 0.15f : 0.1f), val7);
				}
				finally
				{
					((IDisposable)val7)?.Dispose();
				}
			}
			if (enable && btn.Enabled)
			{
				Brush val8 = btn.BackExtend.BrushEx(rect_read, color5);
				try
				{
					g.Fill(val8, val4);
				}
				finally
				{
					((IDisposable)val8)?.Dispose();
				}
			}
			else
			{
				g.Fill(Colour.FillTertiary.Get("Button"), val4);
			}
			if (btn.ExtraMouseDown)
			{
				g.Fill(color8, val4);
			}
			else if (btn.AnimationHover)
			{
				g.Fill(Helper.ToColor(btn.AnimationHoverValue, color7), val4);
			}
			else if (btn.ExtraMouseHover)
			{
				g.Fill(color7, val4);
			}
			PaintButton(g, font, btn, (enable && btn.Enabled) ? color6 : Colour.TextQuaternary.Get("Button"), rect_read);
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
	}

	private static GraphicsPath PathButton(RectangleF rect_read, CellButton btn, float _radius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if (btn.Shape == TShape.Circle)
		{
			GraphicsPath val = new GraphicsPath();
			val.AddEllipse(rect_read);
			return val;
		}
		return rect_read.RoundPath(_radius);
	}

	private static void PaintButton(Canvas g, Font font, CellButton btn, Color color, Rectangle rect_read)
	{
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected O, but got Unknown
		if (string.IsNullOrEmpty(btn.Text))
		{
			Size font_size = g.MeasureString("龍Qq", font);
			Rectangle rect = PaintButtonImageRectCenter(btn, font_size, rect_read);
			if (PaintButtonImageNoText(g, btn, color, rect) && btn.ShowArrow)
			{
				int num = (int)((float)font_size.Height * btn.IconRatio);
				Rectangle rect2 = new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
				PaintButtonTextArrow(g, btn, rect2, color);
			}
			return;
		}
		Size font_size2 = g.MeasureString(btn.Text ?? "龍Qq", font);
		bool hasIcon = btn.HasIcon;
		bool showArrow = btn.ShowArrow;
		Rectangle rect_text;
		if (hasIcon || showArrow)
		{
			if (hasIcon && showArrow)
			{
				rect_text = Button.RectAlignLR(g, btn.textLine, font, btn.IconPosition, btn.IconRatio, btn.IconGap, font_size2, rect_read, out var rect_l, out var rect_r);
				PaintButtonPaintImage(g, btn, color, rect_l);
				PaintButtonTextArrow(g, btn, rect_r, color);
			}
			else if (hasIcon)
			{
				rect_text = Button.RectAlignL(g, btn.textLine, textCenter: false, font, btn.IconPosition, btn.IconRatio, btn.IconGap, font_size2, rect_read, out var rect_l2);
				PaintButtonPaintImage(g, btn, color, rect_l2);
			}
			else
			{
				rect_text = Button.RectAlignR(g, btn.textLine, font, btn.IconPosition, btn.IconRatio, btn.IconGap, font_size2, rect_read, out var rect_r2);
				PaintButtonTextArrow(g, btn, rect_r2, color);
			}
		}
		else
		{
			int num2 = (int)((float)font_size2.Height * 0.4f);
			int num3 = num2 * 2;
			rect_text = new Rectangle(rect_read.X + num2, rect_read.Y + num2, rect_read.Width - num3, rect_read.Height - num3);
			PaintButtonTextAlign(btn, rect_read, ref rect_text);
		}
		SolidBrush val = new SolidBrush(color);
		try
		{
			g.String(btn.Text, font, (Brush)(object)val, rect_text, btn.stringFormat);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void PaintButtonTextArrow(Canvas g, CellButton btn, Rectangle rect, Color color)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Pen val = new Pen(color, 2f * Config.Dpi);
		try
		{
			LineCap startCap = (LineCap)2;
			val.EndCap = (LineCap)2;
			val.StartCap = startCap;
			if (btn.IsLink)
			{
				GraphicsState state = g.Save();
				float num = (float)rect.Width / 2f;
				g.TranslateTransform((float)rect.X + num, (float)rect.Y + num);
				g.RotateTransform(-90f);
				g.DrawLines(val, new RectangleF(0f - num, 0f - num, rect.Width, rect.Height).TriangleLines(btn.ArrowProg));
				g.ResetTransform();
				g.Restore(state);
			}
			else
			{
				g.DrawLines(val, rect.TriangleLines(btn.ArrowProg));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void PaintButtonTextAlign(CellButton btn, Rectangle rect_read, ref Rectangle rect_text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		ContentAlignment textAlign = btn.TextAlign;
		if ((int)textAlign <= 4)
		{
			if (textAlign - 1 <= 1 || (int)textAlign == 4)
			{
				rect_text.Height = rect_read.Height - rect_text.Y;
			}
		}
		else if ((int)textAlign == 16 || (int)textAlign == 32 || (int)textAlign == 64)
		{
			rect_text.Y = rect_read.Y;
			rect_text.Height = rect_read.Height;
		}
	}

	private static bool PaintButtonImageNoText(Canvas g, CellButton btn, Color? color, Rectangle rect)
	{
		if (btn.AnimationImageHover)
		{
			PaintButtonCoreImage(g, btn, rect, color, 1f - btn.AnimationImageHoverValue);
			PaintButtonCoreImageHover(g, btn, rect, color, btn.AnimationImageHoverValue);
			return false;
		}
		if (btn.ExtraMouseHover && PaintButtonCoreImageHover(g, btn, rect, color))
		{
			return false;
		}
		if (PaintButtonCoreImage(g, btn, rect, color))
		{
			return false;
		}
		return true;
	}

	private static bool PaintButtonCoreImage(Canvas g, CellButton btn, Rectangle rect, Color? color, float opacity = 1f)
	{
		if (btn.IconSvg != null)
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(btn.IconSvg, rect, color);
			try
			{
				if (imgExtend != null)
				{
					g.Image(imgExtend, rect, opacity);
					return true;
				}
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		else if (btn.Icon != null)
		{
			g.Image(btn.Icon, rect, opacity);
			return true;
		}
		return false;
	}

	private static bool PaintButtonCoreImageHover(Canvas g, CellButton btn, Rectangle rect, Color? color, float opacity = 1f)
	{
		if (btn.IconHoverSvg != null)
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(btn.IconHoverSvg, rect, color);
			try
			{
				if (imgExtend != null)
				{
					g.Image(imgExtend, rect, opacity);
					return true;
				}
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		else if (btn.IconHover != null)
		{
			g.Image(btn.IconHover, rect, opacity);
			return true;
		}
		return false;
	}

	private static Rectangle PaintButtonImageRectCenter(CellButton btn, Size font_size, Rectangle rect_read)
	{
		int num = (int)((float)font_size.Height * btn.IconRatio * 1.125f);
		return new Rectangle(rect_read.X + (rect_read.Width - num) / 2, rect_read.Y + (rect_read.Height - num) / 2, num, num);
	}

	private static void PaintButtonPaintImage(Canvas g, CellButton btn, Color? color, Rectangle rectl)
	{
		if (btn.AnimationImageHover)
		{
			PaintButtonCoreImage(g, btn, rectl, color, 1f - btn.AnimationImageHoverValue);
			PaintButtonCoreImageHover(g, btn, rectl, color, btn.AnimationImageHoverValue);
		}
		else if (!btn.ExtraMouseHover || !PaintButtonCoreImageHover(g, btn, rectl, color))
		{
			PaintButtonCoreImage(g, btn, rectl, color);
		}
	}

	internal static void PaintLink(Canvas g, Font font, Rectangle rect_read, CellLink link, bool enable)
	{
		if (link.ExtraMouseDown)
		{
			g.String(link.Text, font, Colour.PrimaryActive.Get("Button"), rect_read, link.stringFormat);
		}
		else if (link.AnimationHover)
		{
			g.String(link.Text, font, Colour.Primary.Get("Button").BlendColors(link.AnimationHoverValue, Colour.PrimaryHover.Get("Button")), rect_read, link.stringFormat);
		}
		else if (link.ExtraMouseHover)
		{
			g.String(link.Text, font, Colour.PrimaryHover.Get("Button"), rect_read, link.stringFormat);
		}
		else
		{
			g.String(link.Text, font, (enable && link.Enabled) ? Colour.Primary.Get("Button") : Colour.TextQuaternary.Get("Button"), rect_read, link.stringFormat);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		Canvas g = e.Graphics.High();
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (rows == null)
		{
			if (Empty)
			{
				PaintEmpty(g, clientRectangle, 0);
			}
			((Control)this).OnPaint(e);
			return;
		}
		try
		{
			if (columnfont == null)
			{
				Font val = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
				try
				{
					PaintTable(g, rows, clientRectangle, val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else
			{
				PaintTable(g, rows, clientRectangle, columnfont);
			}
			if (emptyHeader && Empty && rows.Length == 1)
			{
				PaintEmpty(g, clientRectangle, rows[0].RECT.Height);
			}
		}
		catch
		{
		}
		ScrollBar.Paint(g);
		this.PaintBadge(g);
		((Control)this).OnPaint(e);
	}

	private void PaintTable(Canvas g, RowTemplate[] rows, Rectangle rect, Font column_font)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_0a6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a71: Expected O, but got Unknown
		float num = (float)radius * Config.Dpi;
		int valueX = ScrollBar.ValueX;
		int valueY = ScrollBar.ValueY;
		SolidBrush val = new SolidBrush(fore ?? Colour.Text.Get("Table"));
		try
		{
			SolidBrush val2 = new SolidBrush(fore ?? Colour.TextQuaternary.Get("Table"));
			try
			{
				SolidBrush val3 = new SolidBrush(columnfore ?? fore ?? Colour.Text.Get("Table"));
				try
				{
					SolidBrush val4 = new SolidBrush(borderColor ?? Colour.BorderColor.Get("Table"));
					try
					{
						List<StyleRow> list = new List<StyleRow>(rows.Length);
						if (visibleHeader)
						{
							if (num > 0f)
							{
								GraphicsPath val5 = rect_divider.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
								try
								{
									g.SetClip(val5);
								}
								finally
								{
									((IDisposable)val5)?.Dispose();
								}
							}
							if (fixedHeader)
							{
								RowTemplate[] array = rows;
								foreach (RowTemplate rowTemplate in array)
								{
									int num2 = rowTemplate.RECT.Y - valueY;
									int num3 = rowTemplate.RECT.Bottom - valueY;
									rowTemplate.SHOW = rowTemplate.ShowExpand && !rowTemplate.IsColumn && (rect_read.Contains(rect_read.X, num2) || rect_read.Contains(rect_read.X, num3) || (rowTemplate.RECT.Height > rect_read.Height && rect_read.Y > num2 && rect_read.Bottom < num3));
									if (rowTemplate.SHOW)
									{
										list.Add(new StyleRow(rowTemplate, this.SetRowStyle?.Invoke(this, new TableSetRowStyleEventArgs(rowTemplate.RECORD, rowTemplate.INDEX))));
									}
								}
								g.TranslateTransform(0f, -valueY);
								foreach (StyleRow item in list)
								{
									PaintBgRowFront(g, item);
								}
								g.ResetTransform();
								g.TranslateTransform(-valueX, -valueY);
								foreach (StyleRow item2 in list)
								{
									PaintBgRowItem(g, item2.row);
								}
								g.ResetTransform();
								g.TranslateTransform(0f, -valueY);
								foreach (StyleRow item3 in list)
								{
									PaintBg(g, item3.row);
								}
								if (dividers.Length != 0)
								{
									Rectangle[] array2 = dividers;
									foreach (Rectangle rect2 in array2)
									{
										g.Fill((Brush)(object)val4, rect2);
									}
								}
								g.ResetTransform();
								g.TranslateTransform(-valueX, -valueY);
								foreach (StyleRow item4 in list)
								{
									PaintForeItem(g, item4, val, val2);
								}
								g.ResetTransform();
								g.ResetClip();
								PaintTableBgHeader(g, rows[0], num);
								g.TranslateTransform(-valueX, 0f);
								PaintTableHeader(g, rows[0], val3, column_font, num);
								if (dividerHs.Length != 0)
								{
									Rectangle[] array2 = dividerHs;
									foreach (Rectangle rect3 in array2)
									{
										g.Fill((Brush)(object)val4, rect3);
									}
								}
							}
							else
							{
								RowTemplate[] array = rows;
								foreach (RowTemplate rowTemplate2 in array)
								{
									rowTemplate2.SHOW = rowTemplate2.ShowExpand && rowTemplate2.RECT.Y > valueY - rowTemplate2.RECT.Height && rowTemplate2.RECT.Bottom < valueY + rect_read.Height + rowTemplate2.RECT.Height;
									if (rowTemplate2.SHOW)
									{
										list.Add(new StyleRow(rowTemplate2, this.SetRowStyle?.Invoke(this, new TableSetRowStyleEventArgs(rowTemplate2.RECORD, rowTemplate2.INDEX))));
									}
								}
								g.TranslateTransform(0f, -valueY);
								foreach (StyleRow item5 in list)
								{
									if (!item5.row.IsColumn)
									{
										PaintBgRowFront(g, item5);
									}
								}
								g.TranslateTransform(-valueX, 0f);
								foreach (StyleRow item6 in list)
								{
									if (!item6.row.IsColumn)
									{
										PaintBgRowItem(g, item6.row);
									}
								}
								g.ResetTransform();
								g.TranslateTransform(0f, -valueY);
								foreach (StyleRow item7 in list)
								{
									if (item7.row.IsColumn)
									{
										PaintTableBgHeader(g, item7.row, num);
									}
									else
									{
										PaintBg(g, item7.row);
									}
								}
								if (dividers.Length != 0)
								{
									Rectangle[] array2 = dividers;
									foreach (Rectangle rect4 in array2)
									{
										g.Fill((Brush)(object)val4, rect4);
									}
								}
								g.ResetTransform();
								g.TranslateTransform(-valueX, -valueY);
								foreach (StyleRow item8 in list)
								{
									if (item8.row.IsColumn)
									{
										PaintTableHeader(g, item8.row, val3, column_font, num);
									}
									else
									{
										PaintForeItem(g, item8, val, val2);
									}
								}
								if (bordered)
								{
									g.ResetTransform();
									g.TranslateTransform(-valueX, 0f);
								}
								if (dividerHs.Length != 0)
								{
									Rectangle[] array2 = dividerHs;
									foreach (Rectangle rect5 in array2)
									{
										g.Fill((Brush)(object)val4, rect5);
									}
								}
							}
						}
						else
						{
							if (num > 0f)
							{
								GraphicsPath val6 = rect_divider.RoundPath(num);
								try
								{
									g.SetClip(val6);
								}
								finally
								{
									((IDisposable)val6)?.Dispose();
								}
							}
							rows[0].SHOW = false;
							for (int j = 1; j < rows.Length; j++)
							{
								RowTemplate rowTemplate3 = rows[j];
								rowTemplate3.SHOW = rowTemplate3.RECT.Y > valueY - rowTemplate3.RECT.Height && rowTemplate3.RECT.Bottom < valueY + rect_read.Height + rowTemplate3.RECT.Height;
								if (rowTemplate3.SHOW)
								{
									list.Add(new StyleRow(rowTemplate3, this.SetRowStyle?.Invoke(this, new TableSetRowStyleEventArgs(rowTemplate3.RECORD, rowTemplate3.INDEX))));
								}
							}
							g.TranslateTransform(0f, -valueY);
							foreach (StyleRow item9 in list)
							{
								PaintBgRowFront(g, item9);
							}
							g.ResetTransform();
							g.TranslateTransform(-valueX, -valueY);
							foreach (StyleRow item10 in list)
							{
								PaintBgRowItem(g, item10.row);
							}
							g.ResetTransform();
							g.TranslateTransform(0f, -valueY);
							foreach (StyleRow item11 in list)
							{
								PaintBg(g, item11.row);
							}
							if (dividers.Length != 0)
							{
								Rectangle[] array2 = dividers;
								foreach (Rectangle rect6 in array2)
								{
									g.Fill((Brush)(object)val4, rect6);
								}
							}
							g.ResetTransform();
							g.TranslateTransform(-valueX, -valueY);
							foreach (StyleRow item12 in list)
							{
								PaintForeItem(g, item12, val, val2);
							}
							if (bordered)
							{
								g.ResetTransform();
								g.TranslateTransform(-valueX, 0f);
							}
							if (dividerHs.Length != 0)
							{
								Rectangle[] array2 = dividerHs;
								foreach (Rectangle rect7 in array2)
								{
									g.Fill((Brush)(object)val4, rect7);
								}
							}
						}
						g.ResetClip();
						g.ResetTransform();
						if (list.Count > 0 && (fixedColumnL != null || fixedColumnR != null))
						{
							PaintFixedColumnL(g, rect, rows, list, val, val2, val3, column_font, val4, valueX, valueY, num);
							PaintFixedColumnR(g, rect, rows, list, val, val2, val3, column_font, val4, valueX, valueY, num);
						}
						else
						{
							showFixedColumnL = (showFixedColumnR = false);
						}
						if (!bordered)
						{
							return;
						}
						int num4 = ((dividerHs.Length != 0) ? dividerHs[0].Width : ((int)Config.Dpi));
						if (num > 0f)
						{
							Pen val7 = new Pen(val4.Color, (float)num4);
							try
							{
								if (visibleHeader)
								{
									GraphicsPath val8 = rect_divider.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
									try
									{
										g.Draw(val7, val8);
										return;
									}
									finally
									{
										((IDisposable)val8)?.Dispose();
									}
								}
								GraphicsPath val9 = rect_divider.RoundPath(num);
								try
								{
									g.Draw(val7, val9);
									return;
								}
								finally
								{
									((IDisposable)val9)?.Dispose();
								}
							}
							finally
							{
								((IDisposable)val7)?.Dispose();
							}
						}
						g.Draw(val4.Color, num4, rect_divider);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintTableBgHeader(Canvas g, RowTemplate row, float radius)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		SolidBrush val = new SolidBrush(columnback ?? Colour.TagDefaultBg.Get("Table"));
		try
		{
			if (radius > 0f)
			{
				GraphicsPath val2 = row.RECT.RoundPath(radius, TL: true, TR: true, BR: false, BL: false);
				try
				{
					g.Fill((Brush)(object)val, val2);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			else
			{
				g.Fill((Brush)(object)val, row.RECT);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		CELL[] cells = row.cells;
		foreach (CELL cELL in cells)
		{
			if (cELL.COLUMN.ColStyle != null && cELL.COLUMN.ColStyle.BackColor.HasValue)
			{
				SolidBrush val3 = new SolidBrush(cELL.COLUMN.ColStyle.BackColor.Value);
				try
				{
					g.Fill((Brush)(object)val3, cELL.RECT);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
		}
	}

	private void PaintTableHeader(Canvas g, RowTemplate row, SolidBrush fore, Font column_font, float radius)
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Expected O, but got Unknown
		CELL[] cells = row.cells;
		for (int i = 0; i < cells.Length; i++)
		{
			TCellColumn tCellColumn = (TCellColumn)cells[i];
			if (tCellColumn.COLUMN.SortOrder)
			{
				g.GetImgExtend("CaretUpFilled", tCellColumn.rect_up, (tCellColumn.COLUMN.SortMode == SortMode.ASC) ? Colour.Primary.Get("Table") : Colour.TextQuaternary.Get("Table"));
				g.GetImgExtend("CaretDownFilled", tCellColumn.rect_down, (tCellColumn.COLUMN.SortMode == SortMode.DESC) ? Colour.Primary.Get("Table") : Colour.TextQuaternary.Get("Table"));
			}
			if (tCellColumn.COLUMN is ColumnCheck { NoTitle: not false } columnCheck)
			{
				PaintCheck(g, tCellColumn, columnCheck);
			}
			else if (tCellColumn.COLUMN.ColStyle != null && tCellColumn.COLUMN.ColStyle.ForeColor.HasValue)
			{
				SolidBrush val = new SolidBrush(tCellColumn.COLUMN.ColStyle.ForeColor.Value);
				try
				{
					g.String(tCellColumn.value, column_font, (Brush)(object)val, tCellColumn.RECT_REAL, StringF(tCellColumn.COLUMN.ColAlign ?? tCellColumn.COLUMN.Align));
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else
			{
				g.String(tCellColumn.value, column_font, (Brush)(object)fore, tCellColumn.RECT_REAL, StringF(tCellColumn.COLUMN.ColAlign ?? tCellColumn.COLUMN.Align));
			}
		}
		if (dragHeader == null)
		{
			return;
		}
		cells = row.cells;
		foreach (CELL cELL in cells)
		{
			if (dragHeader.i == cELL.INDEX)
			{
				SolidBrush val2 = new SolidBrush(Colour.FillSecondary.Get("Table"));
				try
				{
					if (radius > 0f)
					{
						if (cELL.INDEX == 0)
						{
							GraphicsPath val3 = cELL.RECT.RoundPath(radius, TL: true, TR: false, BR: false, BL: false);
							try
							{
								g.Fill((Brush)(object)val2, val3);
							}
							finally
							{
								((IDisposable)val3)?.Dispose();
							}
						}
						else if (cELL.INDEX == row.cells.Length - 1)
						{
							GraphicsPath val4 = cELL.RECT.RoundPath(radius, TL: false, TR: true, BR: false, BL: false);
							try
							{
								g.Fill((Brush)(object)val2, val4);
							}
							finally
							{
								((IDisposable)val4)?.Dispose();
							}
						}
						else
						{
							g.Fill((Brush)(object)val2, cELL.RECT);
						}
					}
					else
					{
						g.Fill((Brush)(object)val2, cELL.RECT);
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			if (dragHeader.im != cELL.INDEX)
			{
				continue;
			}
			SolidBrush val5 = new SolidBrush(Colour.BorderColor.Get("Table"));
			try
			{
				int num = (int)(2f * Config.Dpi);
				if (dragHeader.last)
				{
					g.Fill((Brush)(object)val5, new Rectangle(cELL.RECT.Right - num, cELL.RECT.Y, num * 2, cELL.RECT.Height));
				}
				else
				{
					g.Fill((Brush)(object)val5, new Rectangle(cELL.RECT.X - num, cELL.RECT.Y, num * 2, cELL.RECT.Height));
				}
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
		}
	}

	private void PaintBgRowFront(Canvas g, StyleRow row)
	{
		if (row.style != null && row.style.BackColor.HasValue)
		{
			g.Fill(row.style.BackColor.Value, row.row.RECT);
		}
		if (selectedIndex.Contains(row.row.INDEX) || row.row.Select)
		{
			g.Fill(rowSelectedBg ?? Colour.PrimaryBg.Get("Table"), row.row.RECT);
			if (selectedIndex.Contains(row.row.INDEX) && row.row.Select)
			{
				g.Fill(Color.FromArgb(40, Colour.PrimaryActive.Get("Table")), row.row.RECT);
			}
		}
		CELL[] cells = row.row.cells;
		foreach (CELL cELL in cells)
		{
			if (cELL.COLUMN.Style != null && cELL.COLUMN.Style.BackColor.HasValue)
			{
				g.Fill(cELL.COLUMN.Style.BackColor.Value, cELL.RECT);
			}
		}
	}

	private void PaintBg(Canvas g, RowTemplate row)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if (dragBody != null)
		{
			if (dragBody.i == row.INDEX)
			{
				g.Fill(Colour.FillSecondary.Get("Table"), row.RECT);
			}
			else
			{
				if (dragBody.im != row.INDEX)
				{
					return;
				}
				SolidBrush val = new SolidBrush(Colour.BorderColor.Get("Table"));
				try
				{
					int num = (int)(2f * Config.Dpi);
					if (dragBody.last)
					{
						g.Fill((Brush)(object)val, new Rectangle(row.RECT.X, row.RECT.Bottom - num, row.RECT.Width, num * 2));
					}
					else
					{
						g.Fill((Brush)(object)val, new Rectangle(row.RECT.X, row.RECT.Y - num, row.RECT.Width, num * 2));
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		else if (row.AnimationHover)
		{
			g.Fill(Helper.ToColorN(row.AnimationHoverValue, Colour.FillSecondary.Get("Table")), row.RECT);
		}
		else if (row.Hover)
		{
			g.Fill(rowHoverBg ?? Colour.FillSecondary.Get("Table"), row.RECT);
		}
	}

	private void PaintBgRowItem(Canvas g, RowTemplate row)
	{
		CELL[] cells = row.cells;
		foreach (CELL it in cells)
		{
			PaintItemBg(g, it);
		}
	}

	private void PaintItemBg(Canvas g, CELL it)
	{
		if (!(it is Template template))
		{
			return;
		}
		foreach (ICell item in template.Value)
		{
			item.PaintBack(g);
		}
	}

	private void PaintForeItem(Canvas g, StyleRow row, SolidBrush fore, SolidBrush foreEnable)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		if (selectedIndex.Contains(row.row.INDEX) && rowSelectedFore.HasValue)
		{
			SolidBrush val = new SolidBrush(rowSelectedFore.Value);
			try
			{
				for (int i = 0; i < row.row.cells.Length; i++)
				{
					PaintItem(g, row.row.cells[i], row.row.ENABLE, val);
				}
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (row.style != null && row.style.ForeColor.HasValue)
		{
			SolidBrush val2 = new SolidBrush(row.style.ForeColor.Value);
			try
			{
				for (int j = 0; j < row.row.cells.Length; j++)
				{
					PaintItem(g, row.row.cells[j], row.row.ENABLE, val2);
				}
				return;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		for (int k = 0; k < row.row.cells.Length; k++)
		{
			PaintItem(g, row.row.cells[k], row.row.ENABLE, row.row.ENABLE ? fore : foreEnable);
		}
	}

	private void PaintItem(Canvas g, CELL it, bool enable, SolidBrush fore)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		if (it.COLUMN.Style == null || !it.COLUMN.Style.ForeColor.HasValue)
		{
			PaintItem(g, it.INDEX, it, enable, fore);
			return;
		}
		SolidBrush val = new SolidBrush(it.COLUMN.Style.ForeColor.Value);
		try
		{
			PaintItem(g, it.INDEX, it, enable, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintItemFixed(Canvas g, CELL it, bool enable, SolidBrush fore, CellStyleInfo? style)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		if (selectedIndex.Contains(it.ROW.INDEX) && rowSelectedFore.HasValue)
		{
			SolidBrush val = new SolidBrush(rowSelectedFore.Value);
			try
			{
				PaintItem(g, it.INDEX, it, enable, val);
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		if (style != null && style.ForeColor.HasValue)
		{
			SolidBrush val2 = new SolidBrush(style.ForeColor.Value);
			try
			{
				PaintItem(g, it.INDEX, it, enable, val2);
				return;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if (it.COLUMN.Style == null || !it.COLUMN.Style.ForeColor.HasValue)
		{
			PaintItem(g, it.INDEX, it, enable, fore);
			return;
		}
		SolidBrush val3 = new SolidBrush(it.COLUMN.Style.ForeColor.Value);
		try
		{
			PaintItem(g, it.INDEX, it, enable, val3);
		}
		finally
		{
			((IDisposable)val3)?.Dispose();
		}
	}

	private void PaintItem(Canvas g, int columnIndex, CELL it, bool enable, SolidBrush fore)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		GraphicsState state = g.Save();
		try
		{
			if (it is TCellCheck check)
			{
				PaintCheck(g, check, enable);
			}
			else if (it is TCellRadio radio)
			{
				PaintRadio(g, radio, enable);
			}
			else if (it is TCellSwitch @switch)
			{
				PaintSwitch(g, @switch, enable);
			}
			else if (it is TCellSort tCellSort)
			{
				if (tCellSort.AnimationHover)
				{
					SolidBrush val = new SolidBrush(Helper.ToColorN(tCellSort.AnimationHoverValue, Colour.FillTertiary.Get("Table")));
					try
					{
						GraphicsPath val2 = tCellSort.RECT_REAL.RoundPath(check_radius);
						try
						{
							g.Fill((Brush)(object)val, val2);
						}
						finally
						{
							((IDisposable)val2)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
				}
				else if (tCellSort.Hover)
				{
					GraphicsPath val3 = tCellSort.RECT_REAL.RoundPath(check_radius);
					try
					{
						g.Fill(Colour.FillTertiary.Get("Table"), val3);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				g.GetImgExtend("HolderOutlined", tCellSort.RECT_ICO, fore.Color);
			}
			else if (it is Template template)
			{
				foreach (ICell item in template.Value)
				{
					item.Paint(g, ((Control)this).Font, enable, fore);
				}
			}
			else if (it is TCellText tCellText)
			{
				g.SetClip(it.RECT);
				g.String(tCellText.value, ((Control)this).Font, (Brush)(object)fore, tCellText.RECT_REAL, StringF(tCellText.COLUMN));
			}
			if (dragHeader != null && dragHeader.i == it.INDEX)
			{
				g.Fill(Colour.FillSecondary.Get("Table"), it.RECT);
			}
			if (it.ROW.CanExpand && it.ROW.KeyTreeINDEX == columnIndex)
			{
				GraphicsPath val4 = it.ROW.RectExpand.RoundPath(check_radius, round: false);
				try
				{
					g.Fill(Colour.BgBase.Get("Table"), val4);
					g.Draw(Colour.BorderColor.Get("Table"), check_border, val4);
					PaintArrow(g, it.ROW, fore, it.ROW.Expand ? 90 : 0);
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
		}
		catch
		{
		}
		g.Restore(state);
	}

	private void PaintArrow(Canvas g, RowTemplate item, SolidBrush color, int ArrowProg)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		int num = item.RectExpand.Width / 2;
		GraphicsState state = g.Save();
		g.TranslateTransform(item.RectExpand.X + num, item.RectExpand.Y + num);
		g.RotateTransform(-90f + (float)ArrowProg);
		Pen val = new Pen((Brush)(object)color, check_border * 2f);
		try
		{
			LineCap startCap = (LineCap)2;
			val.EndCap = (LineCap)2;
			val.StartCap = startCap;
			g.DrawLines(val, new Rectangle(-num, -num, item.RectExpand.Width, item.RectExpand.Height).TriangleLines(-1f, 0.6f));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		g.Restore(state);
	}

	private void PaintFixedColumnL(Canvas g, Rectangle rect, RowTemplate[] rows, List<StyleRow> shows, SolidBrush fore, SolidBrush foreEnable, SolidBrush forecolumn, Font column_font, SolidBrush brush_split, int sx, int sy, float radius)
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Expected O, but got Unknown
		if (fixedColumnL != null && sx > 0)
		{
			showFixedColumnL = true;
			CELL cELL = shows[shows.Count - 1].row.cells[fixedColumnL[fixedColumnL.Count - 1]];
			Rectangle rectangle = new Rectangle(rect.X, rect.Y, cELL.RECT.Right, cELL.RECT.Bottom);
			if (_gap > 0)
			{
				Rectangle rectangle2 = new Rectangle(rect.X + cELL.RECT.Right - _gap, rect.Y, _gap * 2, cELL.RECT.Bottom);
				LinearGradientBrush val = new LinearGradientBrush(rectangle2, Colour.FillSecondary.Get("Table"), Color.Transparent, 0f);
				try
				{
					g.Fill((Brush)(object)val, rectangle2);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			g.Fill(Colour.BgBase.Get("Table"), rectangle);
			g.SetClip(rectangle);
			g.TranslateTransform(0f, -sy);
			foreach (StyleRow show in shows)
			{
				if (show.row.IsColumn)
				{
					PaintTableBgHeader(g, show.row, radius);
					PaintTableHeader(g, show.row, forecolumn, column_font, radius);
				}
				else
				{
					PaintBgRowFront(g, show);
					PaintBgRowItem(g, show.row);
					PaintBg(g, show.row);
				}
			}
			foreach (StyleRow show2 in shows)
			{
				foreach (int item in fixedColumnL)
				{
					if (!show2.row.IsColumn)
					{
						PaintItemFixed(g, show2.row.cells[item], show2.row.ENABLE, show2.row.ENABLE ? fore : foreEnable, show2.style);
					}
				}
			}
			if (dividers.Length != 0)
			{
				Rectangle[] array = dividers;
				foreach (Rectangle rect2 in array)
				{
					g.Fill((Brush)(object)brush_split, rect2);
				}
			}
			g.ResetTransform();
			if (fixedHeader)
			{
				PaintTableBgHeader(g, rows[0], radius);
				PaintTableHeader(g, rows[0], forecolumn, column_font, radius);
			}
			if (dividerHs.Length != 0)
			{
				Rectangle[] array = dividerHs;
				foreach (Rectangle rect3 in array)
				{
					g.Fill((Brush)(object)brush_split, rect3);
				}
			}
			g.ResetClip();
		}
		else
		{
			showFixedColumnL = false;
		}
	}

	private void PaintFixedColumnR(Canvas g, Rectangle rect, RowTemplate[] rows, List<StyleRow> shows, SolidBrush fore, SolidBrush foreEnable, SolidBrush forecolumn, Font column_font, SolidBrush brush_split, int sx, int sy, float radius)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		if (fixedColumnR != null && ScrollBar.ShowX)
		{
			try
			{
				StyleRow styleRow = shows[shows.Count - 1];
				CELL cELL = styleRow.row.cells[fixedColumnR[fixedColumnR.Count - 1]];
				CELL cELL2 = styleRow.row.cells[fixedColumnR[0]];
				if (sx + rect.Width < cELL2.RECT.Right)
				{
					sFixedR = cELL2.RECT.Right - rect.Width;
					showFixedColumnR = true;
					int num = cELL2.RECT.Right - cELL.RECT.Left;
					Rectangle rectangle = new Rectangle(rect.Right - num, rect.Y, cELL2.RECT.Right, cELL2.RECT.Bottom);
					if (_gap > 0)
					{
						Rectangle rectangle2 = new Rectangle(rect.Right - num - _gap, rect.Y, _gap * 2, cELL2.RECT.Bottom);
						LinearGradientBrush val = new LinearGradientBrush(rectangle2, Color.Transparent, Colour.FillSecondary.Get("Table"), 0f);
						try
						{
							g.Fill((Brush)(object)val, rectangle2);
						}
						finally
						{
							((IDisposable)val)?.Dispose();
						}
					}
					g.Fill(Colour.BgBase.Get("Table"), rectangle);
					g.SetClip(rectangle);
					g.TranslateTransform(0f, -sy);
					foreach (StyleRow show in shows)
					{
						if (show.row.IsColumn)
						{
							PaintTableBgHeader(g, show.row, radius);
							g.ResetTransform();
							g.TranslateTransform(-sFixedR, -sy);
							PaintTableHeader(g, show.row, forecolumn, column_font, radius);
							g.ResetTransform();
							g.TranslateTransform(0f, -sy);
						}
						else
						{
							PaintBgRowFront(g, show);
							PaintBg(g, show.row);
						}
					}
					g.ResetTransform();
					g.TranslateTransform(-sFixedR, -sy);
					foreach (StyleRow show2 in shows)
					{
						foreach (int item in fixedColumnR)
						{
							if (!show2.row.IsColumn)
							{
								PaintItemBg(g, show2.row.cells[item]);
								PaintItemFixed(g, show2.row.cells[item], show2.row.ENABLE, show2.row.ENABLE ? fore : foreEnable, show2.style);
							}
						}
					}
					g.ResetTransform();
					g.TranslateTransform(0f, -sy);
					if (dividers.Length != 0)
					{
						Rectangle[] array = dividers;
						foreach (Rectangle rect2 in array)
						{
							g.Fill((Brush)(object)brush_split, rect2);
						}
					}
					g.ResetTransform();
					if (fixedHeader)
					{
						PaintTableBgHeader(g, rows[0], radius);
						g.TranslateTransform(-sFixedR, 0f);
						PaintTableHeader(g, rows[0], forecolumn, column_font, radius);
					}
					g.ResetTransform();
					g.TranslateTransform(-sFixedR, 0f);
					if (dividerHs.Length != 0)
					{
						Rectangle[] array = dividerHs;
						foreach (Rectangle rect3 in array)
						{
							g.Fill((Brush)(object)brush_split, rect3);
						}
					}
					g.ResetTransform();
					g.ResetClip();
				}
				else
				{
					showFixedColumnR = false;
				}
				return;
			}
			catch
			{
				return;
			}
		}
		showFixedColumnR = false;
	}

	private void PaintCheck(Canvas g, TCellColumn check, ColumnCheck columnCheck)
	{
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected O, but got Unknown
		GraphicsPath val = check.RECT_REAL.RoundPath(check_radius, round: false);
		try
		{
			if (columnCheck.AnimationCheck)
			{
				g.Fill(Colour.BgBase.Get("Checkbox"), val);
				float alpha = 255f * columnCheck.AnimationCheckValue;
				if ((int)columnCheck.CheckState == 2 || ((int)columnCheck.checkStateOld == 2 && !columnCheck.Checked))
				{
					g.Draw(Colour.BorderColor.Get("Checkbox"), check_border, val);
					g.Fill(Helper.ToColor(alpha, Colour.Primary.Get("Checkbox")), PaintBlock(check.RECT_REAL));
					return;
				}
				g.Fill(Helper.ToColor(alpha, Colour.Primary.Get("Checkbox")), val);
				Pen val2 = new Pen(Helper.ToColor(alpha, Colour.BgBase.Get("Checkbox")), check_border * 2f);
				try
				{
					g.DrawLines(val2, PaintArrow(check.RECT_REAL));
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
				if (columnCheck.Checked)
				{
					float num = (float)check.RECT_REAL.Height + (float)check.RECT_REAL.Height * columnCheck.AnimationCheckValue;
					SolidBrush val3 = new SolidBrush(Helper.ToColor(100f * (1f - columnCheck.AnimationCheckValue), Colour.Primary.Get("Checkbox")));
					try
					{
						g.FillEllipse((Brush)(object)val3, new RectangleF((float)check.RECT_REAL.X + ((float)check.RECT_REAL.Width - num) / 2f, (float)check.RECT_REAL.Y + ((float)check.RECT_REAL.Height - num) / 2f, num, num));
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				g.Draw(Colour.Primary.Get("Checkbox"), check_border, val);
				return;
			}
			if ((int)columnCheck.CheckState == 2)
			{
				g.Fill(Colour.BgBase.Get("Checkbox"), val);
				g.Draw(Colour.BorderColor.Get("Checkbox"), check_border, val);
				g.Fill(Colour.Primary.Get("Checkbox"), PaintBlock(check.RECT_REAL));
				return;
			}
			if (columnCheck.Checked)
			{
				g.Fill(Colour.Primary.Get("Checkbox"), val);
				Pen val4 = new Pen(Colour.BgBase.Get("Checkbox"), check_border * 2f);
				try
				{
					g.DrawLines(val4, PaintArrow(check.RECT_REAL));
					return;
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
			g.Fill(Colour.BgBase.Get("Checkbox"), val);
			g.Draw(Colour.BorderColor.Get("Checkbox"), check_border, val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintCheck(Canvas g, TCellCheck check, bool enable)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		GraphicsPath val = check.RECT_REAL.RoundPath(check_radius, round: false);
		try
		{
			if (enable)
			{
				if (check.AnimationCheck)
				{
					g.Fill(Colour.BgBase.Get("Checkbox"), val);
					float alpha = 255f * check.AnimationCheckValue;
					g.Fill(Helper.ToColor(alpha, Colour.Primary.Get("Checkbox")), val);
					Pen val2 = new Pen(Helper.ToColor(alpha, Colour.BgBase.Get("Checkbox")), check_border * 2f);
					try
					{
						g.DrawLines(val2, PaintArrow(check.RECT_REAL));
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
					if (check.Checked)
					{
						float num = (float)check.RECT_REAL.Height + (float)check.RECT_REAL.Height * check.AnimationCheckValue;
						SolidBrush val3 = new SolidBrush(Helper.ToColor(100f * (1f - check.AnimationCheckValue), Colour.Primary.Get("Checkbox")));
						try
						{
							g.FillEllipse((Brush)(object)val3, new RectangleF((float)check.RECT_REAL.X + ((float)check.RECT_REAL.Width - num) / 2f, (float)check.RECT_REAL.Y + ((float)check.RECT_REAL.Height - num) / 2f, num, num));
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
					g.Draw(Colour.Primary.Get("Checkbox"), check_border, val);
					return;
				}
				if (check.Checked)
				{
					g.Fill(Colour.Primary.Get("Checkbox"), val);
					Pen val4 = new Pen(Colour.BgBase.Get("Checkbox"), check_border * 2f);
					try
					{
						g.DrawLines(val4, PaintArrow(check.RECT_REAL));
						return;
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				g.Fill(Colour.BgBase.Get("Checkbox"), val);
				g.Draw(Colour.BorderColor.Get("Checkbox"), check_border, val);
			}
			else
			{
				g.Fill(Colour.FillQuaternary.Get("Checkbox"), val);
				if (check.Checked)
				{
					g.DrawLines(Colour.TextQuaternary.Get("Checkbox"), check_border * 2f, PaintArrow(check.RECT_REAL));
				}
				g.Draw(Colour.BorderColorDisable.Get("Checkbox"), check_border, val);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void PaintRadio(Canvas g, TCellRadio radio, bool enable)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		int height = radio.RECT_REAL.Height;
		if (enable)
		{
			g.FillEllipse(Colour.BgBase.Get("Radio"), radio.RECT_REAL);
			if (radio.AnimationCheck)
			{
				float num = (float)height * 0.3f;
				GraphicsPath val = new GraphicsPath();
				try
				{
					float num2 = (float)height - num * radio.AnimationCheckValue;
					float num3 = num2 / 2f;
					float alpha = 255f * radio.AnimationCheckValue;
					val.AddEllipse(radio.RECT_REAL);
					val.AddEllipse(new RectangleF((float)radio.RECT_REAL.X + num3, (float)radio.RECT_REAL.Y + num3, (float)radio.RECT_REAL.Width - num2, (float)radio.RECT_REAL.Height - num2));
					g.Fill(Helper.ToColor(alpha, Colour.Primary.Get("Radio")), val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				if (radio.Checked)
				{
					float num4 = (float)radio.RECT_REAL.Height + (float)radio.RECT_REAL.Height * radio.AnimationCheckValue;
					float alpha2 = 100f * (1f - radio.AnimationCheckValue);
					g.FillEllipse(Helper.ToColor(alpha2, Colour.Primary.Get("Radio")), new RectangleF((float)radio.RECT_REAL.X + ((float)radio.RECT_REAL.Width - num4) / 2f, (float)radio.RECT_REAL.Y + ((float)radio.RECT_REAL.Height - num4) / 2f, num4, num4));
				}
				g.DrawEllipse(Colour.Primary.Get("Radio"), check_border, radio.RECT_REAL);
			}
			else if (radio.Checked)
			{
				float num5 = (float)height * 0.3f;
				float num6 = num5 / 2f;
				g.DrawEllipse(Color.FromArgb(250, Colour.Primary.Get("Radio")), num5, new RectangleF((float)radio.RECT_REAL.X + num6, (float)radio.RECT_REAL.Y + num6, (float)radio.RECT_REAL.Width - num5, (float)radio.RECT_REAL.Height - num5));
				g.DrawEllipse(Colour.Primary.Get("Radio"), check_border, radio.RECT_REAL);
			}
			else
			{
				g.DrawEllipse(Colour.BorderColor.Get("Radio"), check_border, radio.RECT_REAL);
			}
		}
		else
		{
			g.FillEllipse(Colour.FillQuaternary.Get("Radio"), radio.RECT_REAL);
			if (radio.Checked)
			{
				float num7 = (float)height / 2f;
				float num8 = num7 / 2f;
				g.FillEllipse(Colour.TextQuaternary.Get("Radio"), new RectangleF((float)radio.RECT_REAL.X + num8, (float)radio.RECT_REAL.Y + num8, (float)radio.RECT_REAL.Width - num7, (float)radio.RECT_REAL.Height - num7));
			}
			g.DrawEllipse(Colour.BorderColorDisable.Get("Radio"), check_border, radio.RECT_REAL);
		}
	}

	private void PaintSwitch(Canvas g, TCellSwitch _switch, bool enable)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Expected O, but got Unknown
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Expected O, but got Unknown
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		Color color = Colour.Primary.Get("Switch");
		GraphicsPath val = _switch.RECT_REAL.RoundPath(_switch.RECT_REAL.Height);
		try
		{
			SolidBrush val2 = new SolidBrush(Colour.TextQuaternary.Get("Switch"));
			try
			{
				g.Fill((Brush)(object)val2, val);
				if (_switch.AnimationHover)
				{
					g.Fill(Helper.ToColorN(_switch.AnimationHoverValue, val2.Color), val);
				}
				else if (_switch.ExtraMouseHover)
				{
					g.Fill((Brush)(object)val2, val);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			float num = (int)(2f * Config.Dpi);
			float num2 = num * 2f;
			if (_switch.AnimationCheck)
			{
				float alpha = 255f * _switch.AnimationCheckValue;
				g.Fill(Helper.ToColor(alpha, color), val);
				RectangleF rect = new RectangleF((float)_switch.RECT_REAL.X + num + (float)(_switch.RECT_REAL.Width - _switch.RECT_REAL.Height) * _switch.AnimationCheckValue, (float)_switch.RECT_REAL.Y + num, (float)_switch.RECT_REAL.Height - num2, (float)_switch.RECT_REAL.Height - num2);
				g.FillEllipse(enable ? Colour.BgBase.Get("Switch") : Color.FromArgb(200, Colour.BgBase.Get("Switch")), rect);
				return;
			}
			if (_switch.Checked)
			{
				Color color2 = Colour.PrimaryHover.Get("Switch");
				g.Fill(color, val);
				if (_switch.AnimationHover)
				{
					g.Fill(Helper.ToColorN(_switch.AnimationHoverValue, color2), val);
				}
				else if (_switch.ExtraMouseHover)
				{
					g.Fill(color2, val);
				}
				RectangleF rect2 = new RectangleF((float)_switch.RECT_REAL.X + num + (float)_switch.RECT_REAL.Width - (float)_switch.RECT_REAL.Height, (float)_switch.RECT_REAL.Y + num, (float)_switch.RECT_REAL.Height - num2, (float)_switch.RECT_REAL.Height - num2);
				g.FillEllipse(enable ? Colour.BgBase.Get("Switch") : Color.FromArgb(200, Colour.BgBase.Get("Switch")), rect2);
				if (!_switch.Loading)
				{
					return;
				}
				RectangleF rect3 = new RectangleF(rect2.X + num, rect2.Y + num, rect2.Height - num2, rect2.Height - num2);
				float num3 = (float)_switch.RECT_REAL.Height * 0.1f;
				Pen val3 = new Pen(color, num3);
				try
				{
					LineCap startCap = (LineCap)2;
					val3.EndCap = (LineCap)2;
					val3.StartCap = startCap;
					g.DrawArc(val3, rect3, _switch.LineAngle, _switch.LineWidth * 3.6f);
					return;
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			RectangleF rect4 = new RectangleF((float)_switch.RECT_REAL.X + num, (float)_switch.RECT_REAL.Y + num, (float)_switch.RECT_REAL.Height - num2, (float)_switch.RECT_REAL.Height - num2);
			g.FillEllipse(enable ? Colour.BgBase.Get("Switch") : Color.FromArgb(200, Colour.BgBase.Get("Switch")), rect4);
			if (!_switch.Loading)
			{
				return;
			}
			RectangleF rect5 = new RectangleF(rect4.X + num, rect4.Y + num, rect4.Height - num2, rect4.Height - num2);
			float num4 = (float)_switch.RECT_REAL.Height * 0.1f;
			Pen val4 = new Pen(color, num4);
			try
			{
				LineCap startCap = (LineCap)2;
				val4.EndCap = (LineCap)2;
				val4.StartCap = startCap;
				g.DrawArc(val4, rect5, _switch.LineAngle, _switch.LineWidth * 3.6f);
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private RectangleF PaintBlock(RectangleF rect)
	{
		float num = rect.Height * 0.2f;
		float num2 = num * 2f;
		return new RectangleF(rect.X + num, rect.Y + num, rect.Width - num2, rect.Height - num2);
	}

	private PointF[] PaintArrow(RectangleF rect)
	{
		float num = rect.Height * 0.15f;
		float num2 = rect.Height * 0.2f;
		float num3 = rect.Height * 0.26f;
		return new PointF[3]
		{
			new PointF(rect.X + num, rect.Y + rect.Height / 2f),
			new PointF(rect.X + rect.Width * 0.4f, rect.Y + (rect.Height - num3)),
			new PointF(rect.X + rect.Width - num2, rect.Y + num2)
		};
	}

	private void PaintEmpty(Canvas g, Rectangle rect, int offset)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		string text = EmptyText ?? Localization.Get("NoData", "暂无数据");
		SolidBrush val = new SolidBrush(fore ?? Colour.Text.Get("Table"));
		try
		{
			if (offset > 0)
			{
				rect.Offset(0, offset);
				rect.Height -= offset;
			}
			if (EmptyImage == null)
			{
				g.String(text, ((Control)this).Font, (Brush)(object)val, rect, stringCenter);
				return;
			}
			int num = (int)((float)_gap * Config.Dpi);
			Size size = g.MeasureString(text, ((Control)this).Font);
			Rectangle rect2 = new Rectangle(rect.X + (rect.Width - EmptyImage.Width) / 2, rect.Y + (rect.Height - EmptyImage.Height) / 2 - size.Height, EmptyImage.Width, EmptyImage.Height);
			Rectangle rect3 = new Rectangle(rect.X, rect2.Bottom + num, rect.Width, size.Height);
			g.Image(EmptyImage, rect2);
			g.String(text, ((Control)this).Font, (Brush)(object)val, rect3, stringCenter);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal static StringFormat StringF(Column column)
	{
		if (column.LineBreak)
		{
			return (StringFormat)(column.Align switch
			{
				ColumnAlign.Center => stringCenterN, 
				ColumnAlign.Right => stringRightN, 
				_ => stringLeftN, 
			});
		}
		if (column.Ellipsis)
		{
			return (StringFormat)(column.Align switch
			{
				ColumnAlign.Center => stringCenterEllipsis, 
				ColumnAlign.Right => stringRightEllipsis, 
				_ => stringLeftEllipsis, 
			});
		}
		return (StringFormat)(column.Align switch
		{
			ColumnAlign.Center => stringCenter, 
			ColumnAlign.Right => stringRight, 
			_ => stringLeft, 
		});
	}

	private static StringFormat StringF(ColumnAlign align)
	{
		return (StringFormat)(align switch
		{
			ColumnAlign.Center => stringCenter, 
			ColumnAlign.Right => stringRight, 
			_ => stringLeft, 
		});
	}
}
