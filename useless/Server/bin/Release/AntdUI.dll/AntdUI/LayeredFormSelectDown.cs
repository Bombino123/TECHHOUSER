using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormSelectDown : ILayeredFormOpacityDown, SubLayeredForm
{
	private int MaxCount;

	private Size DPadding;

	internal float Radius;

	private bool ClickEnd;

	private object? selectedValue;

	private int r_w;

	private List<ObjectItem> Items;

	private ObjectItemSearch[]? ItemsSearch;

	private string keyid;

	private TAlign ArrowAlign;

	private int ArrowSize = 8;

	private LayeredFormSelectDown? subForm;

	private StringFormat stringFormatLeft = Helper.SF((StringAlignment)1, (StringAlignment)0);

	internal bool tag1 = true;

	private bool nodata;

	internal ScrollY scrollY;

	internal int select_x;

	private int hoveindex = -1;

	private bool down;

	private int hoveindexold = -1;

	private readonly StringFormat s_f = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private Bitmap? shadow_temp;

	public LayeredFormSelectDown(Select control, IList<object> items, string filtertext)
	{
		keyid = "Select";
		((Control)control).Parent.SetTopMost(((Control)this).Handle);
		PARENT = (Control?)(object)control;
		ClickEnd = control.ClickEnd;
		select_x = 0;
		scrollY = new ScrollY((ILayeredForm)this);
		MaxCount = control.MaxCount;
		((Control)this).Font = ((Control)control).Font;
		selectedValue = control.SelectedValue;
		Radius = (int)(((float?)control.DropDownRadius) ?? ((float)control.radius * Config.Dpi));
		DPadding = control.DropDownPadding;
		Items = new List<ObjectItem>(items.Count);
		Init((Control)(object)control, control.Placement, control.DropDownArrow, control.ListAutoWidth, control.ReadRectangle, items, filtertext);
	}

	public LayeredFormSelectDown(Dropdown control, int radius, IList<object> items)
	{
		keyid = "Dropdown";
		((Control)control).Parent.SetTopMost(((Control)this).Handle);
		PARENT = (Control?)(object)control;
		ClickEnd = control.ClickEnd;
		base.MessageCloseMouseLeave = control.Trigger == Trigger.Hover;
		select_x = 0;
		scrollY = new ScrollY((ILayeredForm)this);
		MaxCount = control.MaxCount;
		((Control)this).Font = ((Control)control).Font;
		selectedValue = control.SelectedValue;
		Radius = (int)(((float?)control.DropDownRadius) ?? ((float)radius * Config.Dpi));
		DPadding = control.DropDownPadding;
		Items = new List<ObjectItem>(items.Count);
		Init((Control)(object)control, control.Placement, control.DropDownArrow, control.ListAutoWidth, control.ReadRectangle, items);
	}

	public LayeredFormSelectDown(Tabs control, int radius, IList<object> items, object? sValue, Rectangle rect_ctls)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected I4, but got Unknown
		keyid = "Tabs";
		base.MessageCloseMouseLeave = true;
		((Control)control).Parent.SetTopMost(((Control)this).Handle);
		PARENT = (Control?)(object)control;
		ClickEnd = false;
		select_x = 0;
		scrollY = new ScrollY((ILayeredForm)this);
		MaxCount = 7;
		((Control)this).Font = ((Control)control).Font;
		selectedValue = sValue;
		Radius = (int)((float)radius * Config.Dpi);
		DPadding = new Size(12, 5);
		Items = new List<ObjectItem>(items.Count);
		TabAlignment alignment = control.Alignment;
		Init((Control)(object)control, (alignment - 1) switch
		{
			0 => TAlignFrom.TR, 
			1 => TAlignFrom.TL, 
			2 => TAlignFrom.TR, 
			_ => TAlignFrom.BR, 
		}, ShowArrow: false, ListAutoWidth: true, rect_ctls, items);
	}

	public LayeredFormSelectDown(Table control, ICell cell, Rectangle rect, IList<object> items)
	{
		keyid = "Table";
		((Control)this).Tag = cell;
		base.MessageCloseMouseLeave = true;
		((Control)control).Parent.SetTopMost(((Control)this).Handle);
		PARENT = (Control?)(object)control;
		ClickEnd = cell.DropDownClickEnd;
		select_x = 0;
		scrollY = new ScrollY((ILayeredForm)this);
		MaxCount = cell.DropDownMaxCount;
		((Control)this).Font = ((Control)control).Font;
		selectedValue = cell.DropDownValue;
		Radius = (int)(((float?)cell.DropDownRadius) ?? ((float)control.Radius * Config.Dpi));
		DPadding = cell.DropDownPadding;
		Items = new List<ObjectItem>(items.Count);
		Init((Control)(object)control, cell.DropDownPlacement, cell.DropDownArrow, ListAutoWidth: true, rect, items, "");
	}

	public LayeredFormSelectDown(Select control, int sx, LayeredFormSelectDown ocontrol, float radius, Rectangle rect_read, IList<object> items, int sel = -1)
	{
		keyid = "Select";
		ClickEnd = control.ClickEnd;
		selectedValue = control.SelectedValue;
		scrollY = new ScrollY((ILayeredForm)this);
		DPadding = control.DropDownPadding;
		Items = new List<ObjectItem>(items.Count);
		InitObj((Control)(object)control, sx, ocontrol, radius, rect_read, items, sel);
	}

	public LayeredFormSelectDown(Dropdown control, int sx, LayeredFormSelectDown ocontrol, float radius, Rectangle rect_read, IList<object> items, int sel = -1)
	{
		keyid = "Dropdown";
		ClickEnd = control.ClickEnd;
		scrollY = new ScrollY((ILayeredForm)this);
		DPadding = control.DropDownPadding;
		Items = new List<ObjectItem>(items.Count);
		InitObj((Control)(object)control, sx, ocontrol, radius, rect_read, items, sel);
	}

	private void InitObj(Control parent, int sx, LayeredFormSelectDown control, float radius, Rectangle rect_read, IList<object> items, int sel)
	{
		parent.Parent.SetTopMost(((Control)this).Handle);
		select_x = sx;
		PARENT = parent;
		((Control)this).Font = ((Control)control).Font;
		Radius = radius;
		((Component)(object)control).Disposed += delegate
		{
			((Component)(object)this).Dispose();
		};
		Init((Control)(object)control, TAlignFrom.BL, ShowArrow: false, ListAutoWidth: true, rect_read, items);
		if (sel > -1)
		{
			try
			{
				hoveindex = sel;
				Items[hoveindex].SetHover(val: true);
			}
			catch
			{
			}
		}
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	private void Init(Control control, TAlignFrom Placement, bool ShowArrow, bool ListAutoWidth, Rectangle rect_read, IList<object> items, string? filtertext = null)
	{
		IList<object> items2 = items;
		Control control2 = control;
		int y = 10;
		int w = rect_read.Width;
		r_w = w;
		Point point = control2.PointToScreen(Point.Empty);
		Helper.GDI(delegate(Canvas g)
		{
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			int sp = (int)Config.Dpi;
			int num2 = (int)(4f * Config.Dpi);
			int num3 = (int)((float)DPadding.Height * Config.Dpi);
			int num4 = (int)((float)DPadding.Width * Config.Dpi);
			int num5 = num2 * 2;
			int num6 = num4 * 2;
			int num7 = num3 * 2;
			int height = size.Height;
			int num8 = height + num7;
			y += num2;
			if (ListAutoWidth)
			{
				int btext = size.Width + num6;
				bool ui_online = false;
				bool ui_icon = false;
				bool ui_arrow = false;
				foreach (object item in items2)
				{
					InitReadList(g, item, ref btext, ref ui_online, ref ui_icon, ref ui_arrow);
				}
				if (ui_icon || ui_online)
				{
					btext = ((ui_icon && ui_online) ? (btext + (height + num7)) : ((!ui_icon) ? (btext + num3) : (btext + height)));
				}
				if (ui_arrow)
				{
					btext += num7;
				}
				w = (r_w = btext + num6 + num5);
			}
			else
			{
				stringFormatLeft.Trimming = (StringTrimming)3;
			}
			stringFormatLeft.FormatFlags = (StringFormatFlags)4096;
			int select_y = -1;
			int item_count = 0;
			int divider_count = 0;
			for (int i = 0; i < items2.Count; i++)
			{
				ReadList(items2[i], i, w, num8, height, num2, num5, num4, num6, num3, num7, sp, ref item_count, ref divider_count, ref y, ref select_y);
			}
			int num9 = num8 * item_count + num3 * divider_count;
			if (MaxCount > 0)
			{
				if (Items.Count > MaxCount)
				{
					y = 10 + num5 + num8 * MaxCount;
					scrollY.Rect = new Rectangle(w - num2, 10 + num2, 20, num8 * MaxCount);
					scrollY.Show = true;
					scrollY.SetVrSize(num9, scrollY.Rect.Height);
					if (select_y > -1)
					{
						scrollY.val = scrollY.SetValue(select_y - 10 - num2);
					}
				}
				else
				{
					y = 10 + num5 + num9;
				}
			}
			else
			{
				Rectangle workingArea = Screen.FromPoint(point).WorkingArea;
				int num10 = ((!ShowArrow) ? (point.Y + control2.Height + 20 + num5) : (point.Y + control2.Height + 20 + ArrowSize + num5));
				int num11 = 10 + num5 + num9;
				if (num11 > workingArea.Height - point.Y)
				{
					MaxCount = (int)Math.Floor((double)(workingArea.Height - num10) / ((double)num8 * 1.0)) - 1;
					if (MaxCount < 1)
					{
						MaxCount = 1;
					}
					y = 10 + num5 + num8 * MaxCount;
					scrollY.Rect = new Rectangle(w - num2, 10 + num2, 20, num8 * MaxCount);
					scrollY.Show = true;
					scrollY.SetVrSize(num9, scrollY.Rect.Height);
					if (select_y > -1)
					{
						scrollY.val = scrollY.SetValue(select_y - 10 - num2);
					}
				}
				else
				{
					y = num11;
				}
			}
		});
		SetSize(h: (filtertext != null && !string.IsNullOrEmpty(filtertext)) ? TextChangeCore(filtertext) : (y + 10), w: w + 20);
		if (control2 is LayeredFormSelectDown)
		{
			SetLocation(point.X + rect_read.Width, point.Y + rect_read.Y - 10);
		}
		else
		{
			MyPoint(point, Placement, ShowArrow, rect_read);
		}
		KeyCall = delegate(Keys keys)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Invalid comparison between Unknown and I4
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Invalid comparison between Unknown and I4
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Invalid comparison between Unknown and I4
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Invalid comparison between Unknown and I4
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Invalid comparison between Unknown and I4
			//IL_036b: Unknown result type (might be due to invalid IL or missing references)
			//IL_036e: Invalid comparison between Unknown and I4
			int num = -1;
			if (PARENT is Select select)
			{
				num = select.select_x;
			}
			else if (PARENT is Dropdown dropdown)
			{
				num = dropdown.select_x;
			}
			if (select_x == num)
			{
				if ((int)keys == 27)
				{
					IClose();
					return true;
				}
				if (nodata)
				{
					return false;
				}
				if ((int)keys == 13)
				{
					if (hoveindex > -1)
					{
						ObjectItem objectItem = Items[hoveindex];
						if (objectItem.ID != -1 && OnClick(objectItem))
						{
							return true;
						}
					}
				}
				else
				{
					if ((int)keys == 38)
					{
						hoveindex--;
						if (hoveindex < 0)
						{
							hoveindex = Items.Count - 1;
						}
						while (Items[hoveindex].ShowAndID)
						{
							hoveindex--;
							if (hoveindex < 0)
							{
								hoveindex = Items.Count - 1;
							}
						}
						foreach (ObjectItem item2 in Items)
						{
							item2.Hover = false;
						}
						FocusItem(Items[hoveindex]);
						return true;
					}
					if ((int)keys == 40)
					{
						if (hoveindex == -1)
						{
							hoveindex = 0;
						}
						else
						{
							hoveindex++;
							if (hoveindex > Items.Count - 1)
							{
								hoveindex = 0;
							}
						}
						while (Items[hoveindex].ShowAndID)
						{
							hoveindex++;
							if (hoveindex > Items.Count - 1)
							{
								hoveindex = 0;
							}
						}
						foreach (ObjectItem item3 in Items)
						{
							item3.Hover = false;
						}
						FocusItem(Items[hoveindex]);
						return true;
					}
					if ((int)keys == 37)
					{
						if (num > 0)
						{
							if (PARENT is Select select2)
							{
								select2.select_x--;
							}
							else if (PARENT is Dropdown dropdown2)
							{
								dropdown2.select_x--;
							}
							IClose();
							return true;
						}
					}
					else if ((int)keys == 39 && hoveindex > -1)
					{
						ObjectItem objectItem2 = Items[hoveindex];
						if (objectItem2.Sub != null && objectItem2.Sub.Count > 0)
						{
							subForm?.IClose();
							subForm = null;
							OpenDown(objectItem2, objectItem2.Sub, 0);
							if (PARENT is Select select3)
							{
								select3.select_x++;
							}
							else if (PARENT is Dropdown dropdown3)
							{
								dropdown3.select_x++;
							}
						}
						return true;
					}
				}
			}
			return false;
		};
	}

	private void MyPoint(Point point, TAlignFrom Placement, bool ShowArrow, Rectangle rect_read)
	{
		CLocation(point, Placement, ShowArrow, 10, r_w + 20, base.TargetRect.Height, rect_read, ref Inverted, ref ArrowAlign);
	}

	private void ReadList(object value, int i, int width, int item_height, int text_height, int gap, int gap2, int gap_x, int gap_x2, int gap_y, int gap_y2, int sp, ref int item_count, ref int divider_count, ref int y, ref int select_y, bool NoIndex = true)
	{
		if (value is DividerSelectItem)
		{
			divider_count++;
			Items.Add(new ObjectItem(new Rectangle(10 + gap_y, y + (gap_y - sp) / 2, width - gap_y2, sp)));
			y += gap_y;
			return;
		}
		item_count++;
		Rectangle rect = new Rectangle(10 + gap, y, width - gap2, item_height);
		Rectangle rect_text = new Rectangle(rect.X + gap_x, rect.Y + gap_y, rect.Width - gap_x2, text_height);
		if (value is SelectItem selectItem)
		{
			Items.Add(new ObjectItem(selectItem, i, rect, rect_text, gap_x, gap_x2, gap_y, gap_y2)
			{
				NoIndex = NoIndex
			});
			if (selectedValue == selectItem.Tag)
			{
				select_y = y;
			}
			y += item_height;
			return;
		}
		if (value is GroupSelectItem { Sub: not null } groupSelectItem && groupSelectItem.Sub.Count > 0)
		{
			Items.Add(new ObjectItem(groupSelectItem, i, rect, rect_text));
			if (selectedValue == value)
			{
				select_y = y;
			}
			y += item_height;
			{
				foreach (object item in groupSelectItem.Sub)
				{
					ReadList(item, i, width, item_height, text_height, gap, gap2, gap_x, gap_x2, gap_y, gap_y2, sp, ref item_count, ref divider_count, ref y, ref select_y, NoIndex: false);
				}
				return;
			}
		}
		Items.Add(new ObjectItem(value, i, rect, rect_text)
		{
			NoIndex = NoIndex
		});
		if (selectedValue == value)
		{
			select_y = y;
		}
		y += item_height;
	}

	private void InitReadList(Canvas g, object obj, ref int btext, ref bool ui_online, ref bool ui_icon, ref bool ui_arrow)
	{
		if (obj is SelectItem selectItem)
		{
			string text = selectItem.Text + selectItem.SubText;
			Size size = g.MeasureString(text, ((Control)this).Font);
			if (size.Width > btext)
			{
				btext = size.Width;
			}
			if (selectItem.Online > -1)
			{
				ui_online = true;
			}
			if (selectItem.Icon != null)
			{
				ui_icon = true;
			}
			else if (selectItem.IconSvg != null)
			{
				ui_icon = true;
			}
			if (selectItem.Sub != null && selectItem.Sub.Count > 0)
			{
				ui_arrow = true;
			}
			return;
		}
		if (obj is GroupSelectItem { Sub: not null } groupSelectItem && groupSelectItem.Sub.Count > 0)
		{
			foreach (object item in groupSelectItem.Sub)
			{
				InitReadList(g, item, ref btext, ref ui_online, ref ui_icon, ref ui_arrow);
			}
			return;
		}
		if (obj is DividerSelectItem)
		{
			return;
		}
		string text2 = obj.ToString();
		if (text2 != null)
		{
			Size size2 = g.MeasureString(text2, ((Control)this).Font);
			if (size2.Width > btext)
			{
				btext = size2.Width;
			}
		}
	}

	public void FocusItem(ObjectItem item)
	{
		if (item.SetHover(val: true))
		{
			if (scrollY.Show)
			{
				scrollY.Value = item.Rect.Y - item.Rect.Height;
			}
			Print();
		}
	}

	internal void TextChange(string val)
	{
		ItemsSearch = null;
		int num = 0;
		if (string.IsNullOrEmpty(val))
		{
			nodata = false;
			foreach (ObjectItem item in Items)
			{
				if (!item.Show)
				{
					item.Show = true;
					num++;
				}
			}
		}
		else
		{
			int num2 = 0;
			List<ObjectItemSearch> list = new List<ObjectItemSearch>(Items.Count);
			for (int i = 0; i < Items.Count; i++)
			{
				ObjectItem objectItem = Items[i];
				if (objectItem.ID <= -1)
				{
					continue;
				}
				bool select;
				int num3 = objectItem.Contains(val, out select);
				if (num3 > 0)
				{
					list.Add(new ObjectItemSearch(num3, objectItem));
					num2++;
					if (select)
					{
						objectItem.Hover = true;
						hoveindex = i;
						num++;
					}
					if (!objectItem.Show)
					{
						objectItem.Show = true;
						num++;
					}
				}
				else if (objectItem.Show)
				{
					objectItem.Show = false;
					num++;
				}
			}
			if (list.Count > 0)
			{
				list.Sort((ObjectItemSearch x, ObjectItemSearch y) => -x.Weight.CompareTo(y.Weight));
				ItemsSearch = list.ToArray();
			}
			nodata = num2 == 0;
		}
		if (num <= 0)
		{
			return;
		}
		int sizeH;
		if (nodata)
		{
			sizeH = 80;
			SetSizeH(sizeH);
		}
		else
		{
			scrollY.val = 0f;
			int y2 = 10;
			int w = r_w;
			int list_count = 0;
			Helper.GDI(delegate(Canvas g)
			{
				Size size = g.MeasureString("龍Qq", ((Control)this).Font);
				_ = Config.Dpi;
				int gap = (int)(4f * Config.Dpi);
				int gap_y = (int)((float)DPadding.Height * Config.Dpi);
				int gap_x = (int)((float)DPadding.Width * Config.Dpi);
				int gap2 = gap * 2;
				int gap_x2 = gap_x * 2;
				int gap_y2 = gap_y * 2;
				int text_height = size.Height;
				int item_height = text_height + gap_y2;
				y2 += gap;
				ForEach(delegate(ObjectItem it)
				{
					if (it.ID > -1 && it.Show)
					{
						int num5 = list_count;
						list_count = num5 + 1;
						Rectangle rect = new Rectangle(10 + gap, y2, w - gap2, item_height);
						it.SetRect(rect_text: new Rectangle(rect.X + gap_x, rect.Y + gap_y, rect.Width - gap_x2, text_height), rect: rect, gap_x: gap_x, gap_x2: gap_x2, gap_y: gap_y, gap_y2: gap_y2);
						y2 += item_height;
					}
				});
				int num4 = item_height * list_count;
				if (list_count > MaxCount)
				{
					y2 = 10 + gap2 + item_height * MaxCount;
					scrollY.Rect = new Rectangle(w - gap, 10 + gap, 20, item_height * MaxCount);
					scrollY.Show = true;
					scrollY.SetVrSize(num4, scrollY.Rect.Height);
				}
				else
				{
					y2 = 10 + gap2 + num4;
					scrollY.Show = false;
				}
				y2 += 10;
				SetSizeH(y2);
			});
			sizeH = y2;
		}
		SetSizeH(sizeH);
		MyPoint();
		Bitmap? obj = shadow_temp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		shadow_temp = null;
		Print();
	}

	internal void TextChange(string val, IList<object> items)
	{
		ItemsSearch = null;
		int select_y = -1;
		int y3 = 0;
		int item_count = 0;
		int divider_count = 0;
		Items.Clear();
		for (int i = 0; i < items.Count; i++)
		{
			ReadList(items[i], i, 20, 10, 10, 0, 0, 0, 0, 0, 0, 1, ref item_count, ref divider_count, ref y3, ref select_y);
		}
		int num = 0;
		if (string.IsNullOrEmpty(val))
		{
			nodata = false;
			foreach (ObjectItem item in Items)
			{
				if (!item.Show)
				{
					item.Show = true;
					num++;
				}
			}
		}
		else
		{
			int num2 = 0;
			List<ObjectItemSearch> list = new List<ObjectItemSearch>(Items.Count);
			for (int j = 0; j < Items.Count; j++)
			{
				ObjectItem objectItem = Items[j];
				if (objectItem.ID <= -1)
				{
					continue;
				}
				bool select;
				int num3 = objectItem.Contains(val, out select);
				if (num3 > 0)
				{
					list.Add(new ObjectItemSearch(num3, objectItem));
					num2++;
					if (select)
					{
						objectItem.Hover = true;
						hoveindex = j;
						num++;
					}
					if (!objectItem.Show)
					{
						objectItem.Show = true;
						num++;
					}
				}
				else if (objectItem.Show)
				{
					objectItem.Show = false;
					num++;
				}
			}
			if (list.Count > 0)
			{
				list.Sort((ObjectItemSearch x, ObjectItemSearch y) => -x.Weight.CompareTo(y.Weight));
				ItemsSearch = list.ToArray();
			}
			nodata = num2 == 0;
		}
		if (num <= 0)
		{
			return;
		}
		int sizeH;
		if (nodata)
		{
			sizeH = 80;
			SetSizeH(sizeH);
		}
		else
		{
			scrollY.val = 0f;
			int y2 = 10;
			int w = r_w;
			int list_count = 0;
			Helper.GDI(delegate(Canvas g)
			{
				Size size = g.MeasureString("龍Qq", ((Control)this).Font);
				_ = Config.Dpi;
				int gap = (int)(4f * Config.Dpi);
				int gap_y = (int)((float)DPadding.Height * Config.Dpi);
				int gap_x = (int)((float)DPadding.Width * Config.Dpi);
				int gap2 = gap * 2;
				int gap_x2 = gap_x * 2;
				int gap_y2 = gap_y * 2;
				int text_height = size.Height;
				int item_height = text_height + gap_y2;
				y2 += gap;
				ForEach(delegate(ObjectItem it)
				{
					if (it.ID > -1 && it.Show)
					{
						int num5 = list_count;
						list_count = num5 + 1;
						Rectangle rect = new Rectangle(10 + gap, y2, w - gap2, item_height);
						it.SetRect(rect_text: new Rectangle(rect.X + gap_x, rect.Y + gap_y, rect.Width - gap_x2, text_height), rect: rect, gap_x: gap_x, gap_x2: gap_x2, gap_y: gap_y, gap_y2: gap_y2);
						y2 += item_height;
					}
				});
				int num4 = item_height * list_count;
				if (list_count > MaxCount)
				{
					y2 = 10 + gap2 + item_height * MaxCount;
					scrollY.Rect = new Rectangle(w - gap, 10 + gap, 20, item_height * MaxCount);
					scrollY.Show = true;
					scrollY.SetVrSize(num4, scrollY.Rect.Height);
				}
				else
				{
					y2 = 10 + gap2 + num4;
					scrollY.Show = false;
				}
				y2 += 10;
				SetSizeH(y2);
			});
			sizeH = y2;
		}
		SetSizeH(sizeH);
		MyPoint();
		Bitmap? obj = shadow_temp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		shadow_temp = null;
		Print();
	}

	internal int TextChangeCore(string val)
	{
		ItemsSearch = null;
		if (string.IsNullOrEmpty(val))
		{
			nodata = false;
			foreach (ObjectItem item in Items)
			{
				item.Show = true;
			}
		}
		else
		{
			int num = 0;
			List<ObjectItemSearch> list = new List<ObjectItemSearch>(Items.Count);
			for (int i = 0; i < Items.Count; i++)
			{
				ObjectItem objectItem = Items[i];
				if (objectItem.ID <= -1)
				{
					continue;
				}
				bool select;
				int num2 = objectItem.Contains(val, out select);
				if (num2 > 0)
				{
					list.Add(new ObjectItemSearch(num2, objectItem));
					num++;
					if (select)
					{
						objectItem.Hover = true;
						hoveindex = i;
					}
					objectItem.Show = true;
				}
				else
				{
					objectItem.Show = false;
				}
			}
			if (list.Count > 0)
			{
				list.Sort((ObjectItemSearch x, ObjectItemSearch y) => -x.Weight.CompareTo(y.Weight));
				ItemsSearch = list.ToArray();
			}
			nodata = num == 0;
		}
		if (nodata)
		{
			return 80;
		}
		scrollY.val = 0f;
		int y2 = 10;
		int w = r_w;
		int list_count = 0;
		Helper.GDI(delegate(Canvas g)
		{
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			_ = Config.Dpi;
			int gap = (int)(4f * Config.Dpi);
			int gap_y = (int)((float)DPadding.Height * Config.Dpi);
			int gap_x = (int)((float)DPadding.Width * Config.Dpi);
			int gap2 = gap * 2;
			int gap_x2 = gap_x * 2;
			int gap_y2 = gap_y * 2;
			int text_height = size.Height;
			int item_height = text_height + gap_y2;
			y2 += gap;
			ForEach(delegate(ObjectItem it)
			{
				if (it.ID > -1 && it.Show)
				{
					int num4 = list_count;
					list_count = num4 + 1;
					Rectangle rect = new Rectangle(10 + gap, y2, w - gap2, item_height);
					it.SetRect(rect_text: new Rectangle(rect.X + gap_x, rect.Y + gap_y, rect.Width - gap_x2, text_height), rect: rect, gap_x: gap_x, gap_x2: gap_x2, gap_y: gap_y, gap_y2: gap_y2);
					y2 += item_height;
				}
			});
			int num3 = item_height * list_count;
			if (list_count > MaxCount)
			{
				y2 = 10 + gap2 + item_height * MaxCount;
				scrollY.Rect = new Rectangle(w - gap, 10 + gap, 20, item_height * MaxCount);
				scrollY.Show = true;
				scrollY.SetVrSize(num3, scrollY.Rect.Height);
			}
			else
			{
				y2 = 10 + gap2 + num3;
				scrollY.Show = false;
			}
		});
		return y2 + 10;
	}

	private void ForEach(Action<ObjectItem> action)
	{
		if (ItemsSearch == null)
		{
			foreach (ObjectItem item in Items)
			{
				action(item);
			}
			return;
		}
		ObjectItemSearch[] itemsSearch = ItemsSearch;
		foreach (ObjectItemSearch objectItemSearch in itemsSearch)
		{
			action(objectItemSearch.Value);
		}
	}

	private void MyPoint()
	{
		if (PARENT is Select select)
		{
			MyPoint(((Control)select).PointToScreen(Point.Empty), select.Placement, select.DropDownArrow, select.ReadRectangle);
		}
		else if (PARENT is Dropdown dropdown)
		{
			MyPoint(((Control)dropdown).PointToScreen(Point.Empty), dropdown.Placement, dropdown.DropDownArrow, dropdown.ReadRectangle);
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (scrollY.MouseDown(e.Location))
		{
			OnTouchDown(e.X, e.Y);
			down = true;
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (scrollY.MouseUp(e.Location) && OnTouchUp() && down)
		{
			if (RunAnimation)
			{
				return;
			}
			foreach (ObjectItem item in Items)
			{
				if (item.Show && item.Enable && item.ID > -1 && item.Contains(e.Location, 0, (int)scrollY.Value, out var _) && OnClick(item))
				{
					return;
				}
			}
		}
		down = false;
		((Control)this).OnMouseUp(e);
	}

	private bool OnClick(ObjectItem it)
	{
		if (!ClickEnd || it.Sub == null || it.Sub.Count == 0)
		{
			selectedValue = it.Val;
			OnCall(it);
			down = false;
			IClose();
			return true;
		}
		if (subForm == null)
		{
			OpenDown(it, it.Sub);
		}
		else
		{
			subForm?.IClose();
			subForm = null;
		}
		return false;
	}

	private void OnCall(ObjectItem it)
	{
		if (PARENT is Select select)
		{
			if (select_x == 0 && it.NoIndex)
			{
				if (select.DropDownChange())
				{
					select.DropDownChange(it.ID);
				}
				else
				{
					select.DropDownChange(select_x, it.ID, it.Val);
				}
			}
			else
			{
				select.DropDownChange(select_x, it.ID, it.Val);
			}
		}
		else if (PARENT is Dropdown dropdown)
		{
			dropdown.DropDownChange(it.Val);
		}
		else if (PARENT is Tabs tabs)
		{
			tabs.SelectedIndex = it.ID;
		}
		else if (((Control)this).Tag is ICell cell)
		{
			cell.DropDownValueChanged?.Invoke(it.Val);
		}
	}

	private void OpenDown(ObjectItem it, IList<object> sub, int tag = -1)
	{
		if (PARENT is Select control)
		{
			subForm = new LayeredFormSelectDown(control, select_x + 1, this, Radius, new Rectangle(it.Rect.X, (int)((float)it.Rect.Y - scrollY.Value), it.Rect.Width, it.Rect.Height), sub, tag);
			((Form)subForm).Show((IWin32Window)(object)this);
		}
		else if (PARENT is Dropdown control2)
		{
			subForm = new LayeredFormSelectDown(control2, select_x + 1, this, Radius, new Rectangle(it.Rect.X, (int)((float)it.Rect.Y - scrollY.Value), it.Rect.Width, it.Rect.Height), sub, tag);
			((Form)subForm).Show((IWin32Window)(object)this);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		hoveindex = -1;
		if (scrollY.MouseMove(e.Location) && OnTouchMove(e.X, e.Y))
		{
			int num = 0;
			for (int i = 0; i < Items.Count; i++)
			{
				ObjectItem objectItem = Items[i];
				if (objectItem.Enable)
				{
					if (objectItem.Contains(e.Location, 0, (int)scrollY.Value, out var change))
					{
						hoveindex = i;
					}
					if (change)
					{
						num++;
					}
				}
			}
			if (num > 0)
			{
				Print();
			}
		}
		((Control)this).OnMouseMove(e);
		if (hoveindexold == hoveindex)
		{
			return;
		}
		hoveindexold = hoveindex;
		subForm?.IClose();
		subForm = null;
		if (hoveindex > -1)
		{
			if (PARENT is Select select)
			{
				select.select_x = select_x;
			}
			else if (PARENT is Dropdown dropdown)
			{
				dropdown.select_x = select_x;
			}
			ObjectItem objectItem2 = Items[hoveindex];
			if (objectItem2.Sub != null && objectItem2.Sub.Count > 0 && PARENT != null)
			{
				OpenDown(objectItem2, objectItem2.Sub);
			}
		}
	}

	public override Bitmap PrintBit()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Expected O, but got Unknown
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Expected O, but got Unknown
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rectangle = new Rectangle(10, 10, targetRectXY.Width - 20, targetRectXY.Height - 20);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		Canvas g = Graphics.FromImage((Image)(object)val).High();
		try
		{
			GraphicsPath val2 = rectangle.RoundPath(Radius);
			try
			{
				DrawShadow(g, targetRectXY);
				SolidBrush val3 = new SolidBrush(Colour.BgElevated.Get(keyid));
				try
				{
					g.Fill((Brush)(object)val3, val2);
					if (ArrowAlign != 0)
					{
						g.FillPolygon((Brush)(object)val3, ArrowAlign.AlignLines(ArrowSize, targetRectXY, rectangle));
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				if (nodata)
				{
					string text = Localization.Get("NoData", "暂无数据");
					SolidBrush val4 = new SolidBrush(Color.FromArgb(180, Colour.Text.Get(keyid)));
					try
					{
						g.String(text, ((Control)this).Font, (Brush)(object)val4, rectangle, s_f);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				else
				{
					g.SetClip(val2);
					g.TranslateTransform(0f, 0f - scrollY.Value);
					SolidBrush brush = new SolidBrush(Colour.Text.Get(keyid));
					try
					{
						SolidBrush brush_back_hover = new SolidBrush(Colour.FillTertiary.Get(keyid));
						try
						{
							SolidBrush brush_sub = new SolidBrush(Colour.TextQuaternary.Get(keyid));
							try
							{
								SolidBrush brush_fore = new SolidBrush(Colour.TextTertiary.Get(keyid));
								try
								{
									SolidBrush brush_split = new SolidBrush(Colour.Split.Get(keyid));
									try
									{
										ForEach(delegate(ObjectItem it)
										{
											if (it.Show)
											{
												DrawItem(g, brush, brush_sub, brush_back_hover, brush_fore, brush_split, it);
											}
										});
									}
									finally
									{
										if (brush_split != null)
										{
											((IDisposable)brush_split).Dispose();
										}
									}
								}
								finally
								{
									if (brush_fore != null)
									{
										((IDisposable)brush_fore).Dispose();
									}
								}
							}
							finally
							{
								if (brush_sub != null)
								{
									((IDisposable)brush_sub).Dispose();
								}
							}
						}
						finally
						{
							if (brush_back_hover != null)
							{
								((IDisposable)brush_back_hover).Dispose();
							}
						}
					}
					finally
					{
						if (brush != null)
						{
							((IDisposable)brush).Dispose();
						}
					}
					g.ResetTransform();
					g.ResetClip();
					scrollY.Paint(g);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			if (g != null)
			{
				g.Dispose();
			}
		}
		return val;
	}

	private void DrawItem(Canvas g, SolidBrush brush, SolidBrush subbrush, SolidBrush brush_back_hover, SolidBrush brush_fore, SolidBrush brush_split, ObjectItem it)
	{
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Expected O, but got Unknown
		if (it.ID == -1)
		{
			g.Fill((Brush)(object)brush_split, it.Rect);
		}
		else if (it.Group)
		{
			g.String(it.Text, ((Control)this).Font, (Brush)(object)brush_fore, it.RectText, stringFormatLeft);
		}
		else if (selectedValue == it.Val || (it.Val is SelectItem selectItem && selectItem.Tag == selectedValue))
		{
			GraphicsPath val = it.Rect.RoundPath(Radius);
			try
			{
				g.Fill(Colour.PrimaryBg.Get(keyid), val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			if (it.SubText != null)
			{
				Size size = g.MeasureString(it.Text, ((Control)this).Font);
				Rectangle rect = new Rectangle(it.RectText.X + size.Width, it.RectText.Y, it.RectText.Width - size.Width, it.RectText.Height);
				g.String(it.SubText, ((Control)this).Font, (Brush)(object)subbrush, rect, stringFormatLeft);
			}
			DrawTextIconSelect(g, it);
		}
		else
		{
			if (it.Hover)
			{
				GraphicsPath val2 = it.Rect.RoundPath(Radius);
				try
				{
					g.Fill((Brush)(object)brush_back_hover, val2);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			if (it.SubText != null)
			{
				Size size2 = g.MeasureString(it.Text, ((Control)this).Font);
				Rectangle rect2 = new Rectangle(it.RectText.X + size2.Width, it.RectText.Y, it.RectText.Width - size2.Width, it.RectText.Height);
				g.String(it.SubText, ((Control)this).Font, (Brush)(object)subbrush, rect2, stringFormatLeft);
			}
			DrawTextIcon(g, it, brush);
		}
		if (it.Online.HasValue)
		{
			Color color = it.OnlineCustom ?? ((it.Online.GetValueOrDefault() == 1) ? Colour.Success.Get(keyid) : Colour.Error.Get(keyid));
			SolidBrush val3 = new SolidBrush(it.Enable ? color : Color.FromArgb(Colour.TextQuaternary.Get(keyid).A, color));
			try
			{
				g.FillEllipse((Brush)(object)val3, it.RectOnline);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		if (it.has_sub)
		{
			DrawArrow(g, it, Colour.TextBase.Get(keyid));
		}
	}

	private void DrawTextIconSelect(Canvas g, ObjectItem it)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		Font val = new Font(((Control)this).Font, (FontStyle)1);
		try
		{
			if (it.Enable)
			{
				SolidBrush val2 = new SolidBrush(Colour.TextBase.Get(keyid));
				try
				{
					g.String(it.Text, val, (Brush)(object)val2, it.RectText, stringFormatLeft);
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			else
			{
				SolidBrush val3 = new SolidBrush(Colour.TextQuaternary.Get(keyid));
				try
				{
					g.String(it.Text, val, (Brush)(object)val3, it.RectText, stringFormatLeft);
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		DrawIcon(g, it, Colour.TextBase.Get(keyid));
	}

	private void DrawTextIcon(Canvas g, ObjectItem it, SolidBrush brush)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		if (it.Enable)
		{
			g.String(it.Text, ((Control)this).Font, (Brush)(object)brush, it.RectText, stringFormatLeft);
		}
		else
		{
			SolidBrush val = new SolidBrush(Colour.TextQuaternary.Get(keyid));
			try
			{
				g.String(it.Text, ((Control)this).Font, (Brush)(object)val, it.RectText, stringFormatLeft);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		DrawIcon(g, it, brush.Color);
	}

	private void DrawIcon(Canvas g, ObjectItem it, Color color)
	{
		if (it.IconSvg != null)
		{
			Bitmap imgExtend = SvgExtend.GetImgExtend(it.IconSvg, it.RectIcon, color);
			try
			{
				if (imgExtend != null)
				{
					if (it.Enable)
					{
						g.Image(imgExtend, it.RectIcon);
					}
					else
					{
						g.Image(imgExtend, it.RectIcon, 0.25f);
					}
					return;
				}
			}
			finally
			{
				((IDisposable)imgExtend)?.Dispose();
			}
		}
		if (it.Icon != null)
		{
			if (it.Enable)
			{
				g.Image(it.Icon, it.RectIcon);
			}
			else
			{
				g.Image(it.Icon, it.RectIcon, 0.25f);
			}
		}
	}

	private void DrawArrow(Canvas g, ObjectItem item, Color color)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		int num = item.RectArrow.Width / 2;
		g.TranslateTransform(item.RectArrow.X + num, item.RectArrow.Y + num);
		g.RotateTransform(-90f);
		Pen val = new Pen(color, 2f);
		try
		{
			LineCap startCap = (LineCap)2;
			val.EndCap = (LineCap)2;
			val.StartCap = startCap;
			g.DrawLines(val, new Rectangle(-num, -num, item.RectArrow.Width, item.RectArrow.Height).TriangleLines(-1f, 0.2f));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		g.ResetTransform();
		g.TranslateTransform(0f, 0f - scrollY.Value);
	}

	private void DrawShadow(Canvas g, Rectangle rect)
	{
		if (!Config.ShadowEnabled)
		{
			return;
		}
		if (shadow_temp == null)
		{
			Bitmap? obj = shadow_temp;
			if (obj != null)
			{
				((Image)obj).Dispose();
			}
			GraphicsPath val = new Rectangle(10, 10, rect.Width - 20, rect.Height - 20).RoundPath(Radius);
			try
			{
				shadow_temp = val.PaintShadow(rect.Width, rect.Height);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		g.Image(shadow_temp, rect, 0.2f);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (!RunAnimation)
		{
			scrollY.MouseWheel(e.Delta);
			base.OnMouseWheel(e);
		}
	}

	protected override bool OnTouchScrollY(int value)
	{
		return scrollY.MouseWheel(value);
	}
}
