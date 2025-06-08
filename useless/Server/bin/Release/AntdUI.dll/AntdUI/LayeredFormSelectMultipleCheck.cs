using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormSelectMultipleCheck : ISelectMultiple, SubLayeredForm
{
	private int MaxCount = 4;

	private int MaxChoiceCount = 4;

	private Size DPadding;

	internal float Radius;

	internal List<object> selectedValue;

	private int r_w;

	private List<ObjectItemCheck> Items;

	private TAlign ArrowAlign;

	private int ArrowSize = 8;

	private StringFormat stringFormatLeft = Helper.SF((StringAlignment)1, (StringAlignment)0);

	internal bool tag1 = true;

	private bool nodata;

	internal ScrollY scrollY;

	internal int select_x;

	private int hoveindex = -1;

	private bool down;

	private LayeredFormSelectMultipleCheck? subForm;

	private int hoveindexold = -1;

	private readonly StringFormat s_f = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private Bitmap? shadow_temp;

	public LayeredFormSelectMultipleCheck(SelectMultiple control, Rectangle rect_read, IList<object> items, string filtertext)
	{
		((Control)control).Parent.SetTopMost(((Control)this).Handle);
		PARENT = (Control?)(object)control;
		scrollY = new ScrollY((ILayeredForm)this);
		MaxCount = control.MaxCount;
		MaxChoiceCount = control.MaxChoiceCount;
		((Control)this).Font = ((Control)control).Font;
		selectedValue = new List<object>(control.SelectedValue.Length);
		selectedValue.AddRange(control.SelectedValue);
		Radius = (int)((float)control.radius * Config.Dpi);
		DPadding = control.DropDownPadding;
		Items = new List<ObjectItemCheck>(items.Count);
		Init((Control)(object)control, control.Placement, control.DropDownArrow, control.ListAutoWidth, rect_read, items, filtertext);
	}

	public LayeredFormSelectMultipleCheck(SelectMultiple control, int sx, LayeredFormSelectMultipleCheck ocontrol, float radius, Rectangle rect_read, IList<object> items, int sel = -1)
	{
		selectedValue = new List<object>(control.SelectedValue.Length);
		selectedValue.AddRange(control.SelectedValue);
		scrollY = new ScrollY((ILayeredForm)this);
		Items = new List<ObjectItemCheck>(items.Count);
		DPadding = control.DropDownPadding;
		InitObj(control, sx, ocontrol, radius, rect_read, items, sel);
	}

	public void Rload(List<object> value)
	{
		selectedValue = new List<object>(value.Count);
		selectedValue.AddRange(value);
		Print();
	}

	private void InitObj(SelectMultiple parent, int sx, LayeredFormSelectMultipleCheck control, float radius, Rectangle rect_read, IList<object> items, int sel)
	{
		((Control)parent).Parent.SetTopMost(((Control)this).Handle);
		select_x = sx;
		PARENT = (Control?)(object)parent;
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

	private void Init(Control control, TAlignFrom Placement, bool ShowArrow, bool ListAutoWidth, Rectangle rect_read, IList<object> items, string? filtertext = null)
	{
		IList<object> items2 = items;
		int y = 10;
		int w = rect_read.Width;
		r_w = w;
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
				w = (r_w = btext + num6 + num5 + height);
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
			if (Items.Count > MaxCount)
			{
				y = 10 + num5 + num8 * MaxCount;
				scrollY.Rect = new Rectangle(w - num2, 10 + num2, 20, num8 * MaxCount);
				scrollY.Show = true;
				scrollY.SetVrSize(num9, scrollY.Rect.Height);
				if (select_y > -1)
				{
					scrollY.val = scrollY.SetValue(select_y - 10 - num3);
				}
			}
			else
			{
				y = 10 + num5 + num9;
			}
		});
		SetSize(h: (filtertext != null && !string.IsNullOrEmpty(filtertext)) ? TextChangeCore(filtertext) : (y + 10), w: w + 20);
		Point point = control.PointToScreen(Point.Empty);
		if (control is LayeredFormSelectMultipleCheck)
		{
			SetLocation(point.X + rect_read.Width, point.Y + rect_read.Y - 10);
		}
		else
		{
			MyPoint(point, control, Placement, ShowArrow, rect_read);
		}
		KeyCall = delegate(Keys keys)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Invalid comparison between Unknown and I4
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Invalid comparison between Unknown and I4
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Invalid comparison between Unknown and I4
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Invalid comparison between Unknown and I4
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e4: Invalid comparison between Unknown and I4
			//IL_0320: Unknown result type (might be due to invalid IL or missing references)
			//IL_0323: Invalid comparison between Unknown and I4
			int num = -1;
			if (PARENT is SelectMultiple selectMultiple)
			{
				num = selectMultiple.select_x;
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
						ObjectItemCheck objectItemCheck = Items[hoveindex];
						if (objectItemCheck.ID != -1)
						{
							OnClick(objectItemCheck);
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
						foreach (ObjectItemCheck item2 in Items)
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
							while (Items[hoveindex].ShowAndID)
							{
								hoveindex++;
								if (hoveindex > Items.Count - 1)
								{
									hoveindex = 0;
								}
							}
						}
						foreach (ObjectItemCheck item3 in Items)
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
							if (PARENT is SelectMultiple selectMultiple2)
							{
								selectMultiple2.select_x--;
							}
							IClose();
							return true;
						}
					}
					else if ((int)keys == 39 && hoveindex > -1)
					{
						ObjectItemCheck objectItemCheck2 = Items[hoveindex];
						if (objectItemCheck2.Sub != null && objectItemCheck2.Sub.Count > 0)
						{
							subForm?.IClose();
							subForm = null;
							OpenDown(objectItemCheck2, objectItemCheck2.Sub, 0);
							if (PARENT is SelectMultiple selectMultiple3)
							{
								selectMultiple3.select_x++;
							}
						}
						return true;
					}
				}
			}
			return false;
		};
	}

	private void MyPoint(Point point, Control control, TAlignFrom Placement, bool ShowArrow, Rectangle rect_read)
	{
		CLocation(point, Placement, ShowArrow, 10, r_w + 20, base.TargetRect.Height, rect_read, ref Inverted, ref ArrowAlign);
	}

	private void ReadList(object value, int i, int width, int item_height, int text_height, int gap, int gap2, int gap_x, int gap_x2, int gap_y, int gap_y2, int sp, ref int item_count, ref int divider_count, ref int y, ref int select_y, bool NoIndex = true)
	{
		if (value is DividerSelectItem)
		{
			divider_count++;
			Items.Add(new ObjectItemCheck(new Rectangle(10 + gap_y, y + (gap_y - sp) / 2, width - gap_y2, sp)));
			y += gap_y;
			return;
		}
		item_count++;
		Rectangle rect = new Rectangle(10 + gap, y, width - gap2, item_height);
		Rectangle rect_text = new Rectangle(rect.X + gap_x, rect.Y + gap_y, rect.Width - gap_x2, text_height);
		if (value is SelectItem selectItem)
		{
			Items.Add(new ObjectItemCheck(selectItem, i, rect, rect_text, gap_x, gap_x2, gap_y, gap_y2)
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
			Items.Add(new ObjectItemCheck(groupSelectItem, i, rect, rect_text, gap_x, gap_x2, gap_y, gap_y2));
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
		Items.Add(new ObjectItemCheck(value, i, rect, rect_text, gap_x, gap_x2, gap_y, gap_y2)
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

	public void FocusItem(ObjectItemCheck item)
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

	public override void TextChange(string val)
	{
		int num = 0;
		if (string.IsNullOrEmpty(val))
		{
			nodata = false;
			foreach (ObjectItemCheck item in Items)
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
			val = val.ToLower();
			int num2 = 0;
			for (int i = 0; i < Items.Count; i++)
			{
				ObjectItemCheck objectItemCheck = Items[i];
				if (objectItemCheck.ID <= -1)
				{
					continue;
				}
				if (objectItemCheck.Contains(val))
				{
					num2++;
					if (objectItemCheck.Text.ToLower() == val)
					{
						objectItemCheck.Hover = true;
						hoveindex = i;
						num++;
					}
					if (!objectItemCheck.Show)
					{
						objectItemCheck.Show = true;
						num++;
					}
				}
				else if (objectItemCheck.Show)
				{
					objectItemCheck.Show = false;
					num++;
				}
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
			int y = 10;
			int w = r_w;
			int list_count = 0;
			Helper.GDI(delegate(Canvas g)
			{
				Size size = g.MeasureString("龍Qq", ((Control)this).Font);
				_ = Config.Dpi;
				int num3 = (int)(4f * Config.Dpi);
				int num4 = (int)((float)DPadding.Height * Config.Dpi);
				int num5 = (int)((float)DPadding.Width * Config.Dpi);
				int num6 = num3 * 2;
				int num7 = num5 * 2;
				int num8 = num4 * 2;
				int height = size.Height;
				int num9 = height + num8;
				y += num3;
				foreach (ObjectItemCheck item2 in Items)
				{
					if (item2.ID > -1 && item2.Show)
					{
						list_count++;
						Rectangle rect = new Rectangle(10 + num3, y, w - num6, num9);
						item2.SetRect(rect_text: new Rectangle(rect.X + num5, rect.Y + num4, rect.Width - num7, height), rect: rect, gap_x: num5, gap_x2: num7, gap_y: num4, gap_y2: num8);
						y += num9;
					}
				}
				int num10 = num9 * list_count;
				if (list_count > MaxCount)
				{
					y = 10 + num6 + num9 * MaxCount;
					scrollY.Rect = new Rectangle(w - num3, 10 + num3, 20, num9 * MaxCount);
					scrollY.Show = true;
					scrollY.SetVrSize(num10, scrollY.Rect.Height);
				}
				else
				{
					y = 10 + num6 + num10;
					scrollY.Show = false;
				}
				y += 10;
				SetSizeH(y);
			});
			sizeH = y;
		}
		SetSizeH(sizeH);
		if (PARENT is SelectMultiple control)
		{
			MyPoint(control);
		}
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
		if (string.IsNullOrEmpty(val))
		{
			nodata = false;
			foreach (ObjectItemCheck item in Items)
			{
				item.Show = true;
			}
		}
		else
		{
			val = val.ToLower();
			int num = 0;
			for (int i = 0; i < Items.Count; i++)
			{
				ObjectItemCheck objectItemCheck = Items[i];
				if (objectItemCheck.ID <= -1)
				{
					continue;
				}
				if (objectItemCheck.Contains(val))
				{
					num++;
					if (objectItemCheck.Text.ToLower() == val)
					{
						objectItemCheck.Hover = true;
						hoveindex = i;
					}
					objectItemCheck.Show = true;
				}
				else
				{
					objectItemCheck.Show = false;
				}
			}
			nodata = num == 0;
		}
		if (nodata)
		{
			return 80;
		}
		scrollY.val = 0f;
		int y = 10;
		int w = r_w;
		int list_count = 0;
		Helper.GDI(delegate(Canvas g)
		{
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			_ = Config.Dpi;
			int num2 = (int)(4f * Config.Dpi);
			int num3 = (int)((float)DPadding.Height * Config.Dpi);
			int num4 = (int)((float)DPadding.Width * Config.Dpi);
			int num5 = num2 * 2;
			int num6 = num4 * 2;
			int num7 = num3 * 2;
			int height = size.Height;
			int num8 = height + num7;
			y += num2;
			foreach (ObjectItemCheck item2 in Items)
			{
				if (item2.ID > -1 && item2.Show)
				{
					list_count++;
					Rectangle rect = new Rectangle(10 + num2, y, w - num5, num8);
					item2.SetRect(rect_text: new Rectangle(rect.X + num4, rect.Y + num3, rect.Width - num6, height), rect: rect, gap_x: num4, gap_x2: num6, gap_y: num3, gap_y2: num7);
					y += num8;
				}
			}
			int num9 = num8 * list_count;
			if (list_count > MaxCount)
			{
				y = 10 + num5 + num8 * MaxCount;
				scrollY.Rect = new Rectangle(w - num2, 10 + num2, 20, num8 * MaxCount);
				scrollY.Show = true;
				scrollY.SetVrSize(num9, scrollY.Rect.Height);
			}
			else
			{
				y = 10 + num5 + num9;
				scrollY.Show = false;
			}
		});
		return y + 10;
	}

	private void MyPoint(SelectMultiple control)
	{
		MyPoint(((Control)control).PointToScreen(Point.Empty), (Control)(object)control, control.Placement, control.DropDownArrow, control.ReadRectangle);
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
			foreach (ObjectItemCheck item in Items)
			{
				if (item.Show && item.Enable && item.ID > -1 && item.Contains(e.Location, 0, (int)scrollY.Value, out var _))
				{
					OnClick(item);
					return;
				}
			}
		}
		down = false;
		((Control)this).OnMouseUp(e);
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	private void OnClick(ObjectItemCheck it)
	{
		if (it.Group && it.Val is GroupSelectItem { Sub: not null } groupSelectItem && groupSelectItem.Sub.Count > 0)
		{
			int num = 0;
			foreach (object item4 in groupSelectItem.Sub)
			{
				object item = ReadValue(item4);
				if (selectedValue.Contains(item))
				{
					num++;
					break;
				}
			}
			if (num > 0)
			{
				foreach (object item5 in groupSelectItem.Sub)
				{
					object item2 = ReadValue(item5);
					if (selectedValue.Contains(item2))
					{
						selectedValue.Remove(item2);
					}
				}
			}
			else
			{
				foreach (object item6 in groupSelectItem.Sub)
				{
					object item3 = ReadValue(item6);
					if (!selectedValue.Contains(item3))
					{
						selectedValue.Add(item3);
					}
				}
			}
		}
		else if (it.Sub == null || it.Sub.Count == 0)
		{
			if (selectedValue.Contains(ReadValue(it.Val)))
			{
				selectedValue.Remove(ReadValue(it.Val));
			}
			else
			{
				if (MaxChoiceCount > 0 && selectedValue.Count >= MaxChoiceCount)
				{
					return;
				}
				selectedValue.Add(ReadValue(it.Val));
			}
		}
		else
		{
			if (selectedValue.Contains(ReadValue(it.Val)))
			{
				selectedValue.Remove(ReadValue(it.Val));
				DelValues(it.Sub);
			}
			else
			{
				selectedValue.Add(ReadValue(it.Val));
				AddValues(it.Sub);
			}
			subForm?.Rload(selectedValue);
		}
		if (PARENT is SelectMultiple selectMultiple)
		{
			selectMultiple.SelectedValue = selectedValue.ToArray();
		}
		down = false;
		Print();
	}

	private void OpenDown(ObjectItemCheck it, IList<object> sub, int tag = -1)
	{
		if (PARENT is SelectMultiple control)
		{
			subForm = new LayeredFormSelectMultipleCheck(control, select_x + 1, this, Radius, new Rectangle(it.Rect.X, (int)((float)it.Rect.Y - scrollY.Value), it.Rect.Width, it.Rect.Height), sub, tag);
			((Form)subForm).Show((IWin32Window)(object)this);
		}
	}

	private object ReadValue(object obj)
	{
		if (obj is SelectItem selectItem)
		{
			return selectItem.Tag;
		}
		return obj;
	}

	private void AddValues(IList<object> sub)
	{
		foreach (object item in sub)
		{
			if (item is SelectItem selectItem)
			{
				if (!selectedValue.Contains(selectItem.Tag))
				{
					selectedValue.Add(selectItem.Tag);
				}
				if (selectItem.Sub != null && selectItem.Sub.Count > 0)
				{
					AddValues(selectItem.Sub);
				}
			}
			else if (!selectedValue.Contains(item))
			{
				selectedValue.Add(item);
			}
		}
	}

	private void DelValues(IList<object> sub)
	{
		foreach (object item in sub)
		{
			if (item is SelectItem selectItem)
			{
				selectedValue.Remove(selectItem.Tag);
				if (selectItem.Sub != null && selectItem.Sub.Count > 0)
				{
					DelValues(selectItem.Sub);
				}
			}
			else
			{
				selectedValue.Remove(item);
			}
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
				ObjectItemCheck objectItemCheck = Items[i];
				if (objectItemCheck.Enable)
				{
					if (objectItemCheck.Contains(e.Location, 0, (int)scrollY.Value, out var change))
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
			if (PARENT is SelectMultiple selectMultiple)
			{
				selectMultiple.select_x = select_x;
			}
			ObjectItemCheck objectItemCheck2 = Items[hoveindex];
			if (objectItemCheck2.Sub != null && objectItemCheck2.Sub.Count > 0 && PARENT != null)
			{
				OpenDown(objectItemCheck2, objectItemCheck2.Sub);
			}
		}
	}

	public override Bitmap PrintBit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rectangle = new Rectangle(10, 10, targetRectXY.Width - 20, targetRectXY.Height - 20);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using (Canvas canvas = Graphics.FromImage((Image)(object)val).High())
		{
			GraphicsPath val2 = rectangle.RoundPath(Radius);
			try
			{
				DrawShadow(canvas, targetRectXY);
				SolidBrush val3 = new SolidBrush(Colour.BgElevated.Get("Select"));
				try
				{
					canvas.Fill((Brush)(object)val3, val2);
					if (ArrowAlign != 0)
					{
						canvas.FillPolygon((Brush)(object)val3, ArrowAlign.AlignLines(ArrowSize, targetRectXY, rectangle));
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				if (nodata)
				{
					string text = Localization.Get("NoData", "暂无数据");
					SolidBrush val4 = new SolidBrush(Color.FromArgb(180, Colour.Text.Get("Select")));
					try
					{
						canvas.String(text, ((Control)this).Font, (Brush)(object)val4, rectangle, s_f);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				else
				{
					canvas.SetClip(val2);
					canvas.TranslateTransform(0f, 0f - scrollY.Value);
					SolidBrush val5 = new SolidBrush(Colour.Text.Get("Select"));
					try
					{
						SolidBrush val6 = new SolidBrush(Colour.FillTertiary.Get("Select"));
						try
						{
							SolidBrush val7 = new SolidBrush(Colour.TextQuaternary.Get("Select"));
							try
							{
								SolidBrush val8 = new SolidBrush(Colour.TextTertiary.Get("Select"));
								try
								{
									SolidBrush val9 = new SolidBrush(Colour.Split.Get("Select"));
									try
									{
										if (Radius > 0f)
										{
											int num = -1;
											for (int i = 0; i < Items.Count; i++)
											{
												ObjectItemCheck objectItemCheck = Items[i];
												if (objectItemCheck == null || !objectItemCheck.Show)
												{
													continue;
												}
												if (selectedValue.Contains(objectItemCheck.Val) || (objectItemCheck.Val is SelectItem selectItem && selectedValue.Contains(selectItem.Tag)))
												{
													if (objectItemCheck.Group)
													{
														DrawItem(canvas, val5, val7, val6, val8, val9, objectItemCheck);
														num = -1;
														continue;
													}
													bool flag = IFNextSelect(i + 1);
													if (num == -1)
													{
														if (flag)
														{
															num = i;
															DrawItemSelect(canvas, val5, val7, val9, objectItemCheck, TL: true, TR: true, BR: false, BL: false);
														}
														else
														{
															DrawItemSelect(canvas, val5, val7, val9, objectItemCheck, TL: true, TR: true, BR: true, BL: true);
														}
													}
													else if (flag)
													{
														DrawItemSelect(canvas, val5, val7, val9, objectItemCheck, TL: false, TR: false, BR: false, BL: false);
													}
													else
													{
														DrawItemSelect(canvas, val5, val7, val9, objectItemCheck, TL: false, TR: false, BR: true, BL: true);
													}
												}
												else
												{
													DrawItem(canvas, val5, val7, val6, val8, val9, objectItemCheck);
													num = -1;
												}
											}
										}
										else
										{
											foreach (ObjectItemCheck item in Items)
											{
												if (item.Show)
												{
													DrawItemR(canvas, val5, val6, val9, item);
												}
											}
										}
									}
									finally
									{
										((IDisposable)val9)?.Dispose();
									}
								}
								finally
								{
									((IDisposable)val8)?.Dispose();
								}
							}
							finally
							{
								((IDisposable)val7)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
					canvas.ResetTransform();
					canvas.ResetClip();
					scrollY.Paint(canvas);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		return val;
	}

	private bool IFNextSelect(int start)
	{
		for (int i = start; i < Items.Count; i++)
		{
			ObjectItemCheck objectItemCheck = Items[i];
			if (objectItemCheck != null && objectItemCheck.Show)
			{
				if (selectedValue.Contains(objectItemCheck.Val) || (objectItemCheck.Val is SelectItem selectItem && selectedValue.Contains(selectItem.Tag)))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	private void DrawItemSelect(Canvas g, SolidBrush brush, SolidBrush subbrush, SolidBrush brush_split, ObjectItemCheck it, bool TL, bool TR, bool BR, bool BL)
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		if (it.ID == -1)
		{
			g.Fill((Brush)(object)brush_split, it.Rect);
			return;
		}
		GraphicsPath val = it.Rect.RoundPath(Radius, TL, TR, BR, BL);
		try
		{
			g.Fill(Colour.PrimaryBg.Get("Select"), val);
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
		if (it.Online.HasValue)
		{
			SolidBrush val2 = new SolidBrush(it.OnlineCustom ?? ((it.Online.GetValueOrDefault() == 1) ? Colour.Success.Get("Select") : Colour.Error.Get("Select")));
			try
			{
				g.FillEllipse((Brush)(object)val2, it.RectOnline);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if (it.has_sub)
		{
			DrawArrow(g, it, Colour.TextBase.Get("Select"));
		}
	}

	private void DrawItem(Canvas g, SolidBrush brush, SolidBrush subbrush, SolidBrush brush_back_hover, SolidBrush brush_fore, SolidBrush brush_split, ObjectItemCheck it)
	{
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Expected O, but got Unknown
		if (it.ID == -1)
		{
			g.Fill((Brush)(object)brush_split, it.Rect);
			return;
		}
		if (it.Group)
		{
			g.String(it.Text, ((Control)this).Font, (Brush)(object)brush_fore, it.RectText, stringFormatLeft);
			return;
		}
		if (it.SubText != null)
		{
			Size size = g.MeasureString(it.Text, ((Control)this).Font);
			Rectangle rect = new Rectangle(it.RectText.X + size.Width, it.RectText.Y, it.RectText.Width - size.Width, it.RectText.Height);
			g.String(it.SubText, ((Control)this).Font, (Brush)(object)subbrush, rect, stringFormatLeft);
		}
		if (MaxChoiceCount > 0 && selectedValue.Count >= MaxChoiceCount)
		{
			DrawTextIcon(g, it, subbrush);
		}
		else
		{
			if (it.Hover)
			{
				GraphicsPath val = it.Rect.RoundPath(Radius);
				try
				{
					g.Fill((Brush)(object)brush_back_hover, val);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			DrawTextIcon(g, it, brush);
		}
		if (it.Online.HasValue)
		{
			SolidBrush val2 = new SolidBrush(it.OnlineCustom ?? ((it.Online.GetValueOrDefault() == 1) ? Colour.Success.Get("Select") : Colour.Error.Get("Select")));
			try
			{
				g.FillEllipse((Brush)(object)val2, it.RectOnline);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if (it.has_sub)
		{
			DrawArrow(g, it, Colour.TextBase.Get("Select"));
		}
	}

	private void DrawItemR(Canvas g, SolidBrush brush, SolidBrush brush_back_hover, SolidBrush brush_split, ObjectItemCheck it)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		if (it.ID == -1)
		{
			g.Fill((Brush)(object)brush_split, it.Rect);
		}
		else if (selectedValue.Contains(it.Val) || (it.Val is SelectItem selectItem && selectedValue.Contains(selectItem.Tag)))
		{
			SolidBrush val = new SolidBrush(Colour.PrimaryBg.Get("Select"));
			try
			{
				g.Fill((Brush)(object)val, it.Rect);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			DrawTextIconSelect(g, it);
		}
		else
		{
			if (it.Hover)
			{
				g.Fill((Brush)(object)brush_back_hover, it.Rect);
			}
			DrawTextIcon(g, it, brush);
		}
		if (it.Online.HasValue)
		{
			SolidBrush val2 = new SolidBrush(it.OnlineCustom ?? ((it.Online.GetValueOrDefault() == 1) ? Colour.Success.Get("Select") : Colour.Error.Get("Select")));
			try
			{
				g.FillEllipse((Brush)(object)val2, it.RectOnline);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		if (it.has_sub)
		{
			DrawArrow(g, it, Colour.TextBase.Get("Select"));
		}
	}

	private void DrawTextIconSelect(Canvas g, ObjectItemCheck it)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		if (it.Enable)
		{
			SolidBrush val = new SolidBrush(Colour.TextBase.Get("Select"));
			try
			{
				g.String(it.Text, ((Control)this).Font, (Brush)(object)val, it.RectText, stringFormatLeft);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		else
		{
			SolidBrush val2 = new SolidBrush(Colour.TextQuaternary.Get("Select"));
			try
			{
				g.String(it.Text, ((Control)this).Font, (Brush)(object)val2, it.RectText, stringFormatLeft);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		DrawIcon(g, it, Colour.TextBase.Get("Select"));
		GraphicsPath val3 = it.RectCheck.RoundPath(Radius / 2f);
		try
		{
			g.Fill(Colour.Primary.Get("Select"), val3);
			Pen val4 = new Pen(Colour.BgBase.Get("Select"), 2.6f * Config.Dpi);
			try
			{
				g.DrawLines(val4, it.RectCheck.CheckArrow());
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

	private void DrawTextIcon(Canvas g, ObjectItemCheck it, SolidBrush brush)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (it.Enable)
		{
			g.String(it.Text, ((Control)this).Font, (Brush)(object)brush, it.RectText, stringFormatLeft);
		}
		else
		{
			SolidBrush val = new SolidBrush(Colour.TextQuaternary.Get("Select"));
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
		GraphicsPath val2 = it.RectCheck.RoundPath(Radius / 2f);
		try
		{
			g.Draw(Colour.BorderColor.Get("Select"), 2f * Config.Dpi, val2);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private void DrawIcon(Canvas g, ObjectItemCheck it, Color color)
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

	private void DrawArrow(Canvas g, ObjectItemCheck item, Color color)
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

	public override void SetValues(object[] value)
	{
		selectedValue = new List<object>(value.Length);
		selectedValue.AddRange(value);
		Print();
	}

	public override void SetValues(List<object> value)
	{
		selectedValue = value;
		Print();
	}

	public override void ClearValues()
	{
		selectedValue = new List<object>(0);
		Print();
	}
}
