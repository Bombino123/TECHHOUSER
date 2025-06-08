using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormMenuDown : ILayeredFormOpacityDown, SubLayeredForm
{
	internal class OMenuItem
	{
		public MenuItem Val { get; set; }

		public MenuItemCollection Sub { get; set; }

		internal bool has_sub { get; set; }

		public Rectangle RectIcon { get; set; }

		public bool Hover { get; set; }

		public bool Show { get; set; }

		internal Rectangle arr_rect { get; set; }

		public Rectangle Rect { get; set; }

		public Rectangle RectText { get; set; }

		public OMenuItem(MenuItem _val, Rectangle rect, int gap_y, Rectangle rect_text)
		{
			Sub = _val.Sub;
			if (_val.CanExpand)
			{
				has_sub = true;
			}
			Rect = rect;
			if (_val.HasIcon)
			{
				RectIcon = new Rectangle(rect_text.X, rect_text.Y, rect_text.Height, rect_text.Height);
				RectText = new Rectangle(rect_text.X + gap_y + rect_text.Height, rect_text.Y, rect_text.Width - rect_text.Height - gap_y, rect_text.Height);
			}
			else
			{
				RectText = rect_text;
			}
			arr_rect = new Rectangle(Rect.Right - Rect.Height - gap_y, Rect.Y, Rect.Height, Rect.Height);
			Show = true;
			Val = _val;
		}

		internal bool SetHover(bool val)
		{
			bool result = false;
			if (val)
			{
				if (!Hover)
				{
					result = true;
				}
				Hover = true;
			}
			else
			{
				if (Hover)
				{
					result = true;
				}
				Hover = false;
			}
			return result;
		}

		internal bool Contains(int x, int y, out bool change)
		{
			if (Rect.Contains(x, y))
			{
				change = SetHover(val: true);
				return true;
			}
			change = SetHover(val: false);
			return false;
		}
	}

	internal float Radius;

	private TAMode Theme;

	private bool isdark;

	private List<OMenuItem> Items;

	private Color? backColor;

	private Color? BackHover;

	private Color? BackActive;

	private Color? foreColor;

	private Color? ForeActive;

	private ScrollY? scrollY;

	private LayeredFormMenuDown? subForm;

	private StringFormat stringFormatLeft = Helper.SF((StringAlignment)1, (StringAlignment)0);

	private bool nodata;

	internal int select_x;

	private int hoveindex = -1;

	private int hoveindexold = -1;

	private readonly StringFormat s_f = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private Bitmap? shadow_temp;

	public LayeredFormMenuDown(Menu control, int radius, Rectangle rect_read, MenuItemCollection items)
	{
		base.MessageCloseMouseLeave = true;
		Theme = control.Theme;
		isdark = Config.IsDark || control.Theme == TAMode.Dark;
		((Control)control).Parent.SetTopMost(((Control)this).Handle);
		PARENT = (Control?)(object)control;
		select_x = 0;
		((Control)this).Font = ((Control)control).Font;
		if (control.ShowSubBack)
		{
			backColor = ((Control)control).BackColor;
		}
		foreColor = control.ForeColor;
		ForeActive = control.ForeActive;
		BackHover = control.BackHover;
		BackActive = control.BackActive;
		Radius = (int)((float)radius * Config.Dpi);
		Items = new List<OMenuItem>(items.Count);
		Init((Control)(object)control, rect_read, items);
	}

	public LayeredFormMenuDown(Menu parent, int sx, LayeredFormMenuDown control, float radius, Rectangle rect_read, MenuItemCollection items)
	{
		Theme = parent.Theme;
		isdark = Config.IsDark || parent.Theme == TAMode.Dark;
		((Control)parent).Parent.SetTopMost(((Control)this).Handle);
		select_x = sx;
		PARENT = (Control?)(object)parent;
		((Control)this).Font = ((Control)control).Font;
		backColor = control.backColor;
		foreColor = control.foreColor;
		ForeActive = control.ForeActive;
		BackHover = control.BackHover;
		BackActive = control.BackActive;
		Radius = radius;
		((Component)(object)control).Disposed += delegate
		{
			((Component)(object)this).Dispose();
		};
		Items = new List<OMenuItem>(items.Count);
		Init((Control)(object)control, rect_read, items);
	}

	public ILayeredForm? SubForm()
	{
		return subForm;
	}

	private void Init(Control control, Rectangle rect_read, MenuItemCollection items)
	{
		MenuItemCollection items2 = items;
		int y = 10;
		int w = rect_read.Width;
		OMenuItem oMenuItem = null;
		Helper.GDI(delegate(Canvas g)
		{
			Size size = g.MeasureString("龍Qq", ((Control)this).Font);
			int num2 = (int)(4f * Config.Dpi);
			int num3 = (int)(5f * Config.Dpi);
			int num4 = (int)(12f * Config.Dpi);
			int num5 = num2 * 2;
			int num6 = num4 * 2;
			int num7 = num3 * 2;
			int height = size.Height;
			int num8 = height + num7;
			y += num2;
			int num9 = size.Width + num6;
			bool flag = false;
			bool flag2 = false;
			foreach (MenuItem item in items2)
			{
				if (item.Text != null)
				{
					Size size2 = g.MeasureString(item.Text, ((Control)this).Font);
					if (size2.Width > num9)
					{
						num9 = size2.Width;
					}
				}
				if (item.HasIcon)
				{
					flag = true;
				}
				if (item.CanExpand)
				{
					flag2 = true;
				}
			}
			if (flag)
			{
				num9 = ((!flag) ? (num9 + num3) : (num9 + height));
			}
			if (flag2)
			{
				num9 += num7;
			}
			w = num9 + num6 + num5;
			foreach (MenuItem item2 in items2)
			{
				Rectangle rect = new Rectangle(10 + num2, y, w - num5, num8);
				OMenuItem oMenuItem3 = new OMenuItem(rect_text: new Rectangle(rect.X + num4, rect.Y + num3, rect.Width - num6, height), _val: item2, rect: rect, gap_y: num3);
				Items.Add(oMenuItem3);
				if (item2.Select)
				{
					oMenuItem = oMenuItem3;
				}
				y += num8;
			}
			int num10 = num8 * items2.Count;
			y = 10 + num7 + num10;
		});
		int h = y + 10;
		if (control is LayeredFormMenuDown)
		{
			Point point = control.PointToScreen(Point.Empty);
			InitPoint(point.X + rect_read.Width, point.Y + rect_read.Y - 10, w + 20, h);
		}
		else if (control is Menu { Mode: TMenuMode.Horizontal })
		{
			InitPoint(rect_read.X - 10, Config.ShadowEnabled ? rect_read.Bottom : (rect_read.Bottom - 10), w + 20, h);
		}
		else
		{
			InitPoint(Config.ShadowEnabled ? rect_read.Right : (rect_read.Right - 10), rect_read.Y, w + 20, h);
		}
		KeyCall = delegate(Keys keys)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Invalid comparison between Unknown and I4
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Invalid comparison between Unknown and I4
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Invalid comparison between Unknown and I4
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Invalid comparison between Unknown and I4
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Invalid comparison between Unknown and I4
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_024a: Invalid comparison between Unknown and I4
			int num = -1;
			if (PARENT is Menu menu2)
			{
				num = menu2.select_x;
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
						OMenuItem it = Items[hoveindex];
						if (OnClick(it))
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
						foreach (OMenuItem item3 in Items)
						{
							item3.Hover = false;
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
						foreach (OMenuItem item4 in Items)
						{
							item4.Hover = false;
						}
						FocusItem(Items[hoveindex]);
						return true;
					}
					if ((int)keys == 37)
					{
						if (num > 0)
						{
							if (PARENT is Menu menu3)
							{
								menu3.select_x--;
							}
							IClose();
						}
						return true;
					}
					if ((int)keys == 39)
					{
						if (hoveindex > -1)
						{
							OMenuItem oMenuItem2 = Items[hoveindex];
							if (oMenuItem2.Sub != null && oMenuItem2.Sub.Count > 0)
							{
								subForm?.IClose();
								subForm = null;
								OpenDown(oMenuItem2);
								if (PARENT is Menu menu4)
								{
									menu4.select_x++;
								}
							}
						}
						return true;
					}
				}
			}
			return false;
		};
		if (oMenuItem != null)
		{
			FocusItem(oMenuItem, print: false);
		}
	}

	private void InitPoint(int x, int y, int w, int h)
	{
		Rectangle workingArea = Screen.FromPoint(new Point(x, y)).WorkingArea;
		if (x < workingArea.X)
		{
			x = workingArea.X;
		}
		else if (x > workingArea.X + workingArea.Width - w)
		{
			x = workingArea.X + workingArea.Width - w;
		}
		if (h > workingArea.Height)
		{
			int num = (int)(4f * Config.Dpi);
			int num2 = h;
			int height = workingArea.Height;
			scrollY = new ScrollY((ILayeredForm)this);
			scrollY.Rect = new Rectangle(w - num - scrollY.SIZE, 10 + num, scrollY.SIZE, height - 20 - num * 2);
			scrollY.Show = true;
			scrollY.SetVrSize(num2, height);
			h = height;
		}
		if (y < workingArea.Y)
		{
			y = workingArea.Y;
		}
		else if (y > workingArea.Y + workingArea.Height - h)
		{
			y = workingArea.Y + workingArea.Height - h;
		}
		SetLocation(x, y);
		SetSize(w, h);
	}

	public void FocusItem(OMenuItem it, bool print = true)
	{
		if (it.SetHover(val: true))
		{
			if (scrollY != null && scrollY.Show)
			{
				scrollY.Value = it.Rect.Y - it.Rect.Height;
			}
			if (print)
			{
				Print();
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		int num = ((scrollY != null && scrollY.Show) ? ((int)scrollY.Value) : 0);
		foreach (OMenuItem item in Items)
		{
			if (item.Show && item.Val.Enabled && item.Contains(e.X, e.Y + num, out var _) && OnClick(item))
			{
				return;
			}
		}
		((Control)this).OnMouseUp(e);
	}

	private bool OnClick(OMenuItem it)
	{
		if (it.Sub == null || it.Sub.Count == 0)
		{
			if (PARENT is Menu menu)
			{
				menu.DropDownChange(it.Val);
			}
			IClose();
			return true;
		}
		if (subForm == null)
		{
			OpenDown(it);
		}
		else
		{
			subForm?.IClose();
			subForm = null;
		}
		return false;
	}

	private void OpenDown(OMenuItem it)
	{
		if (PARENT is Menu parent)
		{
			subForm = new LayeredFormMenuDown(parent, select_x + 1, this, Radius, new Rectangle(it.Rect.X, it.Rect.Y, it.Rect.Width, it.Rect.Height), it.Sub);
			((Form)subForm).Show((IWin32Window)(object)this);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (RunAnimation)
		{
			return;
		}
		((Control)this).OnMouseMove(e);
		hoveindex = -1;
		int num = ((scrollY != null && scrollY.Show) ? ((int)scrollY.Value) : 0);
		int num2 = 0;
		for (int i = 0; i < Items.Count; i++)
		{
			OMenuItem oMenuItem = Items[i];
			if (oMenuItem.Show && oMenuItem.Val.Enabled)
			{
				if (oMenuItem.Contains(e.X, e.Y + num, out var change))
				{
					hoveindex = i;
				}
				if (change)
				{
					num2++;
				}
			}
		}
		if (num2 > 0)
		{
			Print();
		}
		if (hoveindexold == hoveindex)
		{
			return;
		}
		hoveindexold = hoveindex;
		subForm?.IClose();
		subForm = null;
		if (hoveindex > -1)
		{
			if (PARENT is Menu menu)
			{
				menu.select_x = select_x;
			}
			OMenuItem oMenuItem2 = Items[hoveindex];
			if (oMenuItem2.Sub != null && oMenuItem2.Sub.Count > 0 && PARENT != null)
			{
				OpenDown(oMenuItem2);
			}
		}
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		scrollY?.MouseWheel(e.Delta);
		base.OnMouseWheel(e);
	}

	public override Bitmap PrintBit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rectangle = new Rectangle(10, 10, targetRectXY.Width - 20, targetRectXY.Height - 20);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using (Canvas canvas = Graphics.FromImage((Image)(object)val).HighLay(text: true))
		{
			GraphicsPath val2 = rectangle.RoundPath(Radius);
			try
			{
				DrawShadow(canvas, targetRectXY);
				canvas.Fill(backColor ?? Colour.BgElevated.Get("Menu", Theme), val2);
				if (nodata)
				{
					string text = Localization.Get("NoData", "暂无数据");
					SolidBrush val3 = new SolidBrush(Color.FromArgb(180, Colour.Text.Get("Menu", Theme)));
					try
					{
						canvas.String(text, ((Control)this).Font, (Brush)(object)val3, rectangle, s_f);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				else
				{
					canvas.SetClip(rectangle);
					if (scrollY != null && scrollY.Show)
					{
						canvas.TranslateTransform(0f, 0f - scrollY.Value);
					}
					if (foreColor.HasValue)
					{
						SolidBrush val4 = new SolidBrush(foreColor.Value);
						try
						{
							foreach (OMenuItem item in Items)
							{
								if (item.Show)
								{
									DrawItem(canvas, val4, item);
								}
							}
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
					}
					else
					{
						SolidBrush val5 = new SolidBrush(Colour.Text.Get("Menu", Theme));
						try
						{
							foreach (OMenuItem item2 in Items)
							{
								if (item2.Show)
								{
									DrawItem(canvas, val5, item2);
								}
							}
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					canvas.ResetClip();
					canvas.ResetTransform();
					scrollY?.Paint(canvas);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		return val;
	}

	private void DrawItem(Canvas g, SolidBrush brush, OMenuItem it)
	{
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Expected O, but got Unknown
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		if (it.Val.Enabled)
		{
			if (isdark)
			{
				if (it.Val.Select)
				{
					GraphicsPath val = it.Rect.RoundPath(Radius);
					try
					{
						g.Fill(BackActive ?? Colour.Primary.Get("Menu", Theme), val);
					}
					finally
					{
						((IDisposable)val)?.Dispose();
					}
					SolidBrush val2 = new SolidBrush(ForeActive ?? Colour.TextBase.Get("Menu", Theme));
					try
					{
						g.String(it.Val.Text, it.Val.Font ?? ((Control)this).Font, (Brush)(object)val2, it.RectText, stringFormatLeft);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
					PaintIcon(g, it, brush.Color);
				}
				else
				{
					if (it.Hover)
					{
						GraphicsPath val3 = it.Rect.RoundPath(Radius);
						try
						{
							g.Fill(BackHover ?? Colour.FillTertiary.Get("Menu", Theme), val3);
						}
						finally
						{
							((IDisposable)val3)?.Dispose();
						}
					}
					g.String(it.Val.Text, it.Val.Font ?? ((Control)this).Font, (Brush)(object)brush, it.RectText, stringFormatLeft);
					PaintIcon(g, it, brush.Color);
				}
			}
			else
			{
				if (it.Val.Select)
				{
					GraphicsPath val4 = it.Rect.RoundPath(Radius);
					try
					{
						g.Fill(BackActive ?? Colour.PrimaryBg.Get("Menu", Theme), val4);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
					SolidBrush val5 = new SolidBrush(ForeActive ?? Colour.TextBase.Get("Menu", Theme));
					try
					{
						g.String(it.Val.Text, it.Val.Font ?? ((Control)this).Font, (Brush)(object)val5, it.RectText, stringFormatLeft);
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
				}
				else
				{
					if (it.Hover)
					{
						GraphicsPath val6 = it.Rect.RoundPath(Radius);
						try
						{
							g.Fill(BackHover ?? Colour.FillTertiary.Get("Menu", Theme), val6);
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
					g.String(it.Val.Text, it.Val.Font ?? ((Control)this).Font, (Brush)(object)brush, it.RectText, stringFormatLeft);
				}
				PaintIcon(g, it, brush.Color);
			}
		}
		else
		{
			if (it.Val.Select)
			{
				if (isdark)
				{
					GraphicsPath val7 = it.Rect.RoundPath(Radius);
					try
					{
						g.Fill(BackActive ?? Colour.Primary.Get("Menu", Theme), val7);
					}
					finally
					{
						((IDisposable)val7)?.Dispose();
					}
				}
				else
				{
					GraphicsPath val8 = it.Rect.RoundPath(Radius);
					try
					{
						g.Fill(BackActive ?? Colour.PrimaryBg.Get("Menu", Theme), val8);
					}
					finally
					{
						((IDisposable)val8)?.Dispose();
					}
				}
			}
			SolidBrush val9 = new SolidBrush(Colour.TextQuaternary.Get("Menu", Theme));
			try
			{
				g.String(it.Val.Text, it.Val.Font ?? ((Control)this).Font, (Brush)(object)val9, it.RectText, stringFormatLeft);
			}
			finally
			{
				((IDisposable)val9)?.Dispose();
			}
			PaintIcon(g, it, brush.Color);
		}
		if (it.has_sub)
		{
			PaintArrow(g, it, brush.Color);
		}
	}

	private void PaintIcon(Canvas g, OMenuItem it, Color fore)
	{
		if (it.Val.Icon != null)
		{
			g.Image(it.Val.Icon, it.RectIcon);
		}
		else if (it.Val.IconSvg != null)
		{
			g.GetImgExtend(it.Val.IconSvg, it.RectIcon, fore);
		}
	}

	private void PaintArrow(Canvas g, OMenuItem item, Color color)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		int num = item.arr_rect.Width / 2;
		g.TranslateTransform(item.arr_rect.X + num, item.arr_rect.Y + num);
		g.RotateTransform(-90f);
		Pen val = new Pen(color, 2f);
		try
		{
			LineCap startCap = (LineCap)2;
			val.EndCap = (LineCap)2;
			val.StartCap = startCap;
			g.DrawLines(val, new Rectangle(-num, -num, item.arr_rect.Width, item.arr_rect.Height).TriangleLines(-1f, 0.2f));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		g.ResetTransform();
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
}
