using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormSelectMultiple : ISelectMultiple
{
	private int MaxCount = 4;

	private int MaxChoiceCount = 4;

	private Size DPadding;

	internal float Radius;

	internal List<object> selectedValue;

	private int r_w;

	private List<ObjectItem> Items;

	private ObjectItemSearch[]? ItemsSearch;

	private TAlign ArrowAlign;

	private int ArrowSize = 8;

	private StringFormat stringFormatLeft = Helper.SF((StringAlignment)1, (StringAlignment)0);

	internal bool tag1 = true;

	private bool nodata;

	internal ScrollY scrollY;

	private int hoveindex = -1;

	private bool down;

	private readonly StringFormat s_f = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private Bitmap? shadow_temp;

	public LayeredFormSelectMultiple(SelectMultiple control, Rectangle rect_read, IList<object> items, string filtertext)
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
		Items = new List<ObjectItem>(items.Count);
		Init(control, control.Placement, control.DropDownArrow, control.ListAutoWidth, rect_read, items, filtertext);
	}

	private void Init(SelectMultiple control, TAlignFrom Placement, bool ShowArrow, bool ListAutoWidth, Rectangle rect_read, IList<object> items, string? filtertext = null)
	{
		IList<object> items2 = items;
		int y = 10;
		int w = rect_read.Width;
		r_w = w;
		Helper.GDI(delegate(Canvas g)
		{
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			int sp = (int)Config.Dpi;
			int num = (int)(4f * Config.Dpi);
			int num2 = (int)((float)DPadding.Height * Config.Dpi);
			int num3 = (int)((float)DPadding.Width * Config.Dpi);
			int num4 = num * 2;
			int num5 = num3 * 2;
			int num6 = num2 * 2;
			int height = size.Height;
			int num7 = height + num6;
			y += num;
			if (ListAutoWidth)
			{
				int btext = size.Width + num5;
				bool ui_online = false;
				bool ui_icon = false;
				bool ui_arrow = false;
				foreach (object item in items2)
				{
					InitReadList(g, item, ref btext, ref ui_online, ref ui_icon, ref ui_arrow);
				}
				if (ui_icon || ui_online)
				{
					btext = ((ui_icon && ui_online) ? (btext + (height + num6)) : ((!ui_icon) ? (btext + num2) : (btext + height)));
				}
				w = (r_w = btext + num5 + num4 + num6);
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
				ReadList(items2[i], i, w, num7, height, num, num4, num3, num5, num2, num6, sp, ref item_count, ref divider_count, ref y, ref select_y);
			}
			int num8 = num7 * item_count + num2 * divider_count;
			if (Items.Count > MaxCount)
			{
				y = 10 + num4 + num7 * MaxCount;
				scrollY.Rect = new Rectangle(w - num, 10 + num, 20, num7 * MaxCount);
				scrollY.Show = true;
				scrollY.SetVrSize(num8, scrollY.Rect.Height);
				if (select_y > -1)
				{
					scrollY.val = scrollY.SetValue(select_y - 10 - num2);
				}
			}
			else
			{
				y = 10 + num4 + num8;
			}
		});
		SetSize(h: (filtertext != null && !string.IsNullOrEmpty(filtertext)) ? TextChangeCore(filtertext) : (y + 10), w: w + 20);
		Point point = ((Control)control).PointToScreen(Point.Empty);
		MyPoint(point, (Control)(object)control, Placement, ShowArrow, rect_read);
		KeyCall = delegate(Keys keys)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Invalid comparison between Unknown and I4
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Invalid comparison between Unknown and I4
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Invalid comparison between Unknown and I4
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Invalid comparison between Unknown and I4
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
					if (objectItem.ID != -1)
					{
						OnClick(objectItem);
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
						while (Items[hoveindex].ShowAndID)
						{
							hoveindex++;
							if (hoveindex > Items.Count - 1)
							{
								hoveindex = 0;
							}
						}
					}
					foreach (ObjectItem item3 in Items)
					{
						item3.Hover = false;
					}
					FocusItem(Items[hoveindex]);
					return true;
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

	public override void TextChange(string val)
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
			foreach (ObjectItem item in Items)
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

	private void OnClick(ObjectItem it)
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
		else if (selectedValue.Contains(ReadValue(it.Val)))
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
		if (PARENT is SelectMultiple selectMultiple)
		{
			selectMultiple.SelectedValue = selectedValue.ToArray();
		}
		down = false;
		Print();
	}

	private object ReadValue(object obj)
	{
		if (obj is SelectItem selectItem)
		{
			return selectItem.Tag;
		}
		return obj;
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
												ObjectItem objectItem = Items[i];
												if (objectItem == null || !objectItem.Show)
												{
													continue;
												}
												if (selectedValue.Contains(objectItem.Val) || (objectItem.Val is SelectItem selectItem && selectedValue.Contains(selectItem.Tag)))
												{
													if (objectItem.Group)
													{
														DrawItem(canvas, val5, val7, val6, val8, val9, objectItem);
														num = -1;
														continue;
													}
													bool flag = IFNextSelect(i + 1);
													if (num == -1)
													{
														if (flag)
														{
															num = i;
															DrawItemSelect(canvas, val5, val7, val9, objectItem, TL: true, TR: true, BR: false, BL: false);
														}
														else
														{
															DrawItemSelect(canvas, val5, val7, val9, objectItem, TL: true, TR: true, BR: true, BL: true);
														}
													}
													else if (flag)
													{
														DrawItemSelect(canvas, val5, val7, val9, objectItem, TL: false, TR: false, BR: false, BL: false);
													}
													else
													{
														DrawItemSelect(canvas, val5, val7, val9, objectItem, TL: false, TR: false, BR: true, BL: true);
													}
												}
												else
												{
													DrawItem(canvas, val5, val7, val6, val8, val9, objectItem);
													num = -1;
												}
											}
										}
										else
										{
											foreach (ObjectItem item in Items)
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
			ObjectItem objectItem = Items[i];
			if (objectItem != null && objectItem.Show)
			{
				if (selectedValue.Contains(objectItem.Val) || (objectItem.Val is SelectItem selectItem && selectedValue.Contains(selectItem.Tag)))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	private void DrawItemSelect(Canvas g, SolidBrush brush, SolidBrush subbrush, SolidBrush brush_split, ObjectItem it, bool TL, bool TR, bool BR, bool BL)
	{
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
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
		g.PaintIconCore(new Rectangle(it.Rect.Right - it.Rect.Height, it.Rect.Y, it.Rect.Height, it.Rect.Height), SvgDb.IcoSuccessGhost, Colour.Primary.Get("Select"), 0.46f);
		if (!it.Online.HasValue)
		{
			return;
		}
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

	private void DrawItem(Canvas g, SolidBrush brush, SolidBrush subbrush, SolidBrush brush_back_hover, SolidBrush brush_fore, SolidBrush brush_split, ObjectItem it)
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
		if (!it.Online.HasValue)
		{
			return;
		}
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

	private void DrawItemR(Canvas g, SolidBrush brush, SolidBrush brush_back_hover, SolidBrush brush_split, ObjectItem it)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
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
			g.PaintIconCore(new Rectangle(it.Rect.Right - it.Rect.Height, it.Rect.Y, it.Rect.Height, it.Rect.Height), SvgDb.IcoSuccessGhost, Colour.Primary.Get("Select"), 0.46f);
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
	}

	private void DrawTextIconSelect(Canvas g, ObjectItem it)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
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
	}

	private void DrawTextIcon(Canvas g, ObjectItem it, SolidBrush brush)
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
