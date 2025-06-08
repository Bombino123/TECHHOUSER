using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using AntdUI.Design;

namespace AntdUI;

[Description("Collapse 折叠面板")]
[ToolboxItem(true)]
[DefaultProperty("Items")]
[DefaultEvent("ExpandChanged")]
[Designer(typeof(CollapseDesigner))]
public class Collapse : IControl
{
	internal class CollapseDesigner : ParentControlDesigner
	{
		public Collapse Control => (Collapse)(object)((ControlDesigner)this).Control;

		protected override bool GetHitTest(Point point)
		{
			Point point2 = ((Control)Control).PointToClient(point);
			foreach (CollapseItem item in Control.Items)
			{
				if (item.Contains(point2.X, point2.Y))
				{
					return true;
				}
			}
			return ((ControlDesigner)this).GetHitTest(point);
		}
	}

	private Color? fore;

	private Color? headerBg;

	private float borderWidth = 1f;

	private Color? borderColor;

	private int radius = 6;

	private int _gap;

	private CollapseItemCollection? items;

	private StringFormat s_l = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

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
			}
		}
	}

	[Description("折叠面板头部背景")]
	[Category("外观")]
	[DefaultValue(null)]
	[Editor(typeof(ColorEditor), typeof(UITypeEditor))]
	public Color? HeaderBg
	{
		get
		{
			return headerBg;
		}
		set
		{
			if (!(headerBg == value))
			{
				headerBg = value;
				((Control)this).Invalidate();
			}
		}
	}

	private Size headerPadding { get; set; } = new Size(16, 12);


	[Description("折叠面板头部内边距")]
	[Category("外观")]
	[DefaultValue(typeof(Size), "16, 12")]
	public Size HeaderPadding
	{
		get
		{
			return headerPadding;
		}
		set
		{
			if (!(headerPadding == value))
			{
				headerPadding = value;
				LoadLayout();
			}
		}
	}

	private Size contentPadding { get; set; } = new Size(16, 16);


	[Description("折叠面板内容内边距")]
	[Category("外观")]
	[DefaultValue(typeof(Size), "16, 16")]
	public Size ContentPadding
	{
		get
		{
			return contentPadding;
		}
		set
		{
			if (!(contentPadding == value))
			{
				contentPadding = value;
				LoadLayout();
			}
		}
	}

	[Description("边框宽度")]
	[Category("边框")]
	[DefaultValue(1f)]
	public float BorderWidth
	{
		get
		{
			return borderWidth;
		}
		set
		{
			if (borderWidth != value)
			{
				borderWidth = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("边框颜色")]
	[Category("边框")]
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
			}
		}
	}

	[Description("圆角")]
	[Category("外观")]
	[DefaultValue(6)]
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
				radius = value;
				((Control)this).Invalidate();
			}
		}
	}

	[Description("间距")]
	[Category("外观")]
	[DefaultValue(0)]
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
				LoadLayout();
			}
		}
	}

	[Description("只保持一个展开")]
	[Category("外观")]
	[DefaultValue(false)]
	public bool Unique { get; set; }

	public override Rectangle DisplayRectangle => ((Control)this).ClientRectangle.PaddingRect(((Control)this).Margin, ((Control)this).Padding);

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("集合")]
	[Category("数据")]
	[DefaultValue(null)]
	public CollapseItemCollection Items
	{
		get
		{
			if (items == null)
			{
				items = new CollapseItemCollection(this);
			}
			return items;
		}
		set
		{
			items = value.BindData(this);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ControlCollection Controls => ((Control)this).Controls;

	[Description("Expand 属性值更改时发生")]
	[Category("行为")]
	public event CollapseExpandEventHandler? ExpandChanged;

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		LoadLayout(r: false);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		LoadLayout(r: false);
		((Control)this).OnSizeChanged(e);
	}

	internal void UniqueOne(CollapseItem item)
	{
		if (!Unique || items == null)
		{
			return;
		}
		foreach (CollapseItem item2 in items)
		{
			if (item2 != item)
			{
				item2.Expand = false;
			}
		}
	}

	internal void LoadLayout(bool r = true)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Control)this).IsHandleCreated || items == null)
		{
			return;
		}
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		if (clientRectangle.Width > 0 && clientRectangle.Height > 0)
		{
			Rectangle rect = clientRectangle.DeflateRect(((Control)this).Margin);
			LoadLayout(rect, items);
			if (r)
			{
				((Control)this).Invalidate();
			}
		}
	}

	internal void LoadLayout(Rectangle rect, CollapseItemCollection items)
	{
		Size size = Helper.GDI((Canvas g) => g.MeasureString("龍Qq", ((Control)this).Font));
		int num = (int)((float)_gap * Config.Dpi);
		int num2 = (int)((float)HeaderPadding.Width * Config.Dpi);
		int num3 = (int)((float)HeaderPadding.Height * Config.Dpi);
		int num4 = (int)((float)ContentPadding.Width * Config.Dpi);
		int num5 = (int)((float)ContentPadding.Height * Config.Dpi);
		int num6 = 0;
		int num7 = size.Height + num3 * 2;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		foreach (CollapseItem item in items)
		{
			if (item.Full)
			{
				num8++;
			}
		}
		if (num8 > 0)
		{
			foreach (CollapseItem item2 in items)
			{
				if (!item2.Full)
				{
					num9 += num7 + num;
					if (item2.ExpandThread)
					{
						num9 += (int)((float)(num5 * 2 + ((Control)item2).Height) * item2.ExpandProg);
					}
					else if (item2.Expand)
					{
						num9 += num5 * 2 + ((Control)item2).Height;
					}
				}
			}
			num10 = (rect.Height - num9) / num8;
		}
		foreach (CollapseItem item3 in items)
		{
			int num11 = rect.Y + num6;
			item3.RectTitle = new Rectangle(rect.X, num11, rect.Width, num7);
			item3.RectArrow = new Rectangle(rect.X + num2, num11 + num3, size.Height, size.Height);
			item3.RectText = new Rectangle(rect.X + num2 + size.Height + num3 / 2, num11 + num3, rect.Width - (num2 * 2 - size.Height - num3 / 2), size.Height);
			Rectangle rectangle;
			if (item3.Full)
			{
				if (item3.ExpandThread)
				{
					rectangle = (item3.Rect = new Rectangle(rect.X, num11, rect.Width, num7 + (int)((float)(num10 - num7) * item3.ExpandProg)));
				}
				else if (item3.Expand)
				{
					rectangle = (item3.Rect = new Rectangle(rect.X, num11, rect.Width, num10));
					item3.RectCcntrol = new Rectangle(rect.X + num4, num11 + num7 + num5, rect.Width - num4 * 2, num10 - (num7 + num5 * 2));
					item3.SetSize();
				}
				else
				{
					rectangle = (item3.Rect = item3.RectTitle);
				}
			}
			else if (item3.ExpandThread)
			{
				rectangle = (item3.Rect = new Rectangle(rect.X, num11, rect.Width, num7 + (int)((float)(num5 * 2 + ((Control)item3).Height) * item3.ExpandProg)));
			}
			else if (item3.Expand)
			{
				item3.RectCcntrol = new Rectangle(rect.X + num4, num11 + num7 + num5, rect.Width - num4 * 2, ((Control)item3).Height);
				rectangle = (item3.Rect = new Rectangle(rect.X, num11, rect.Width, num7 + num5 * 2 + ((Control)item3).Height));
				item3.SetSize();
			}
			else
			{
				rectangle = (item3.Rect = item3.RectTitle);
			}
			num6 += rectangle.Height + num;
		}
	}

	internal void OnExpandChanged(CollapseItem value, bool expand)
	{
		this.ExpandChanged?.Invoke(this, new CollapseExpandEventArgs(value, expand));
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		if (items == null || items.Count == 0)
		{
			((Control)this).OnPaint(e);
			return;
		}
		Canvas canvas = e.Graphics.High();
		float num = (float)radius * Config.Dpi;
		SolidBrush val = new SolidBrush(fore ?? Colour.Text.Get("Collapse"));
		try
		{
			SolidBrush val2 = new SolidBrush(headerBg ?? Colour.FillQuaternary.Get("Collapse"));
			try
			{
				if (borderWidth > 0f)
				{
					Pen val3 = new Pen(borderColor ?? Colour.BorderColor.Get("Collapse"), borderWidth * Config.Dpi);
					try
					{
						Pen val4 = new Pen(val.Color, 1.2f * Config.Dpi);
						try
						{
							LineCap startCap = (LineCap)2;
							val3.EndCap = (LineCap)2;
							val3.StartCap = startCap;
							if (items.Count == 1 || _gap > 0)
							{
								foreach (CollapseItem item in items)
								{
									if (item.Expand)
									{
										GraphicsPath val5 = item.Rect.RoundPath(num);
										try
										{
											canvas.Draw(val3, val5);
										}
										finally
										{
											((IDisposable)val5)?.Dispose();
										}
										GraphicsPath val6 = item.RectTitle.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
										try
										{
											canvas.Fill((Brush)(object)val2, val6);
											canvas.Draw(val3, val6);
										}
										finally
										{
											((IDisposable)val6)?.Dispose();
										}
									}
									else
									{
										GraphicsPath val7 = item.RectTitle.RoundPath(num);
										try
										{
											canvas.Fill((Brush)(object)val2, val7);
											canvas.Draw(val3, val7);
										}
										finally
										{
											((IDisposable)val7)?.Dispose();
										}
									}
									PaintItem(canvas, item, val, val4);
								}
							}
							else
							{
								for (int i = 0; i < items.Count; i++)
								{
									CollapseItem collapseItem = items[i];
									if (i == 0)
									{
										if (collapseItem.Expand)
										{
											GraphicsPath val8 = collapseItem.Rect.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
											try
											{
												canvas.Draw(val3, val8);
											}
											finally
											{
												((IDisposable)val8)?.Dispose();
											}
											GraphicsPath val9 = collapseItem.RectTitle.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
											try
											{
												canvas.Fill((Brush)(object)val2, val9);
												canvas.Draw(val3, val9);
											}
											finally
											{
												((IDisposable)val9)?.Dispose();
											}
										}
										else
										{
											GraphicsPath val10 = collapseItem.RectTitle.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
											try
											{
												canvas.Fill((Brush)(object)val2, val10);
												canvas.Draw(val3, val10);
											}
											finally
											{
												((IDisposable)val10)?.Dispose();
											}
										}
										PaintItem(canvas, collapseItem, val, val4);
										continue;
									}
									if (i == items.Count - 1)
									{
										if (collapseItem.Expand)
										{
											GraphicsPath val11 = collapseItem.Rect.RoundPath(num, TL: false, TR: false, BR: true, BL: true);
											try
											{
												canvas.Draw(val3, val11);
											}
											finally
											{
												((IDisposable)val11)?.Dispose();
											}
											canvas.Fill((Brush)(object)val2, collapseItem.RectTitle);
											canvas.Draw(val3, collapseItem.RectTitle);
										}
										else
										{
											GraphicsPath val12 = collapseItem.RectTitle.RoundPath(num, TL: false, TR: false, BR: true, BL: true);
											try
											{
												canvas.Fill((Brush)(object)val2, val12);
												canvas.Draw(val3, val12);
											}
											finally
											{
												((IDisposable)val12)?.Dispose();
											}
										}
										PaintItem(canvas, collapseItem, val, val4);
										continue;
									}
									if (collapseItem.Expand)
									{
										canvas.Draw(val3, collapseItem.Rect);
										canvas.Fill((Brush)(object)val2, collapseItem.RectTitle);
										canvas.Draw(val3, collapseItem.RectTitle);
									}
									else
									{
										GraphicsPath val13 = collapseItem.RectTitle.RoundPath(num, TL: false, TR: false, BR: true, BL: true);
										try
										{
											canvas.Fill((Brush)(object)val2, collapseItem.RectTitle);
											canvas.Draw(val3, collapseItem.RectTitle);
										}
										finally
										{
											((IDisposable)val13)?.Dispose();
										}
									}
									PaintItem(canvas, collapseItem, val, val4);
								}
							}
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
				else if (items.Count == 1 || _gap > 0)
				{
					foreach (CollapseItem item2 in items)
					{
						if (item2.Expand)
						{
							GraphicsPath val14 = item2.RectTitle.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
							try
							{
								canvas.Fill((Brush)(object)val2, val14);
							}
							finally
							{
								((IDisposable)val14)?.Dispose();
							}
						}
						else
						{
							GraphicsPath val15 = item2.RectTitle.RoundPath(num);
							try
							{
								canvas.Fill((Brush)(object)val2, val15);
							}
							finally
							{
								((IDisposable)val15)?.Dispose();
							}
						}
						PaintItem(canvas, item2, val);
					}
				}
				else
				{
					for (int j = 0; j < items.Count; j++)
					{
						CollapseItem collapseItem2 = items[j];
						if (j == 0)
						{
							if (collapseItem2.Expand)
							{
								GraphicsPath val16 = collapseItem2.RectTitle.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
								try
								{
									canvas.Fill((Brush)(object)val2, val16);
								}
								finally
								{
									((IDisposable)val16)?.Dispose();
								}
							}
							else
							{
								GraphicsPath val17 = collapseItem2.RectTitle.RoundPath(num, TL: true, TR: true, BR: false, BL: false);
								try
								{
									canvas.Fill((Brush)(object)val2, val17);
								}
								finally
								{
									((IDisposable)val17)?.Dispose();
								}
							}
							PaintItem(canvas, collapseItem2, val);
							continue;
						}
						if (j == items.Count - 1)
						{
							if (collapseItem2.Expand)
							{
								canvas.Fill((Brush)(object)val2, collapseItem2.RectTitle);
							}
							else
							{
								GraphicsPath val18 = collapseItem2.RectTitle.RoundPath(num, TL: false, TR: false, BR: true, BL: true);
								try
								{
									canvas.Fill((Brush)(object)val2, val18);
								}
								finally
								{
									((IDisposable)val18)?.Dispose();
								}
							}
							PaintItem(canvas, collapseItem2, val);
							continue;
						}
						if (collapseItem2.Expand)
						{
							canvas.Fill((Brush)(object)val2, collapseItem2.RectTitle);
						}
						else
						{
							GraphicsPath val19 = collapseItem2.RectTitle.RoundPath(num, TL: false, TR: false, BR: true, BL: true);
							try
							{
								canvas.Fill((Brush)(object)val2, collapseItem2.RectTitle);
							}
							finally
							{
								((IDisposable)val19)?.Dispose();
							}
						}
						PaintItem(canvas, collapseItem2, val);
					}
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
		this.PaintBadge(canvas);
		((Control)this).OnPaint(e);
	}

	private void PaintItem(Canvas g, CollapseItem item, SolidBrush fore, Pen pen_arr)
	{
		if (item.ExpandThread)
		{
			PaintArrow(g, item, pen_arr, -90f + 90f * item.ExpandProg);
		}
		else if (item.Expand)
		{
			g.DrawLines(pen_arr, item.RectArrow.TriangleLines(-1f, 0.56f));
		}
		else
		{
			PaintArrow(g, item, pen_arr, -90f);
		}
		g.String(((Control)item).Text, ((Control)this).Font, (Brush)(object)fore, item.RectText, s_l);
	}

	private void PaintItem(Canvas g, CollapseItem item, SolidBrush fore)
	{
		if (item.ExpandThread)
		{
			PaintArrow(g, item, fore, -90f + 90f * item.ExpandProg);
		}
		else if (item.Expand)
		{
			g.FillPolygon((Brush)(object)fore, item.RectArrow.TriangleLines(-1f, 0.56f));
		}
		else
		{
			PaintArrow(g, item, fore, -90f);
		}
		g.String(((Control)item).Text, ((Control)this).Font, (Brush)(object)fore, item.RectText, s_l);
	}

	private void PaintArrow(Canvas g, CollapseItem item, Pen pen, float rotate)
	{
		Rectangle rectArrow = item.RectArrow;
		int num = rectArrow.Width / 2;
		g.TranslateTransform(rectArrow.X + num, rectArrow.Y + num);
		g.RotateTransform(rotate);
		g.DrawLines(pen, new Rectangle(-num, -num, rectArrow.Width, rectArrow.Height).TriangleLines(-1f, 0.56f));
		g.ResetTransform();
	}

	private void PaintArrow(Canvas g, CollapseItem item, SolidBrush brush, float rotate)
	{
		Rectangle rectArrow = item.RectArrow;
		int num = rectArrow.Width / 2;
		g.TranslateTransform(rectArrow.X + num, rectArrow.Y + num);
		g.RotateTransform(rotate);
		g.FillPolygon((Brush)(object)brush, new Rectangle(-num, -num, rectArrow.Width, rectArrow.Height).TriangleLines(-1f, 0.56f));
		g.ResetTransform();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		foreach (CollapseItem item in items)
		{
			if (item.Contains(e.X, e.Y))
			{
				item.MDown = true;
				return;
			}
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		foreach (CollapseItem item in items)
		{
			if (item.MDown)
			{
				if (item.Contains(e.X, e.Y))
				{
					item.Expand = !item.Expand;
				}
				item.MDown = false;
				return;
			}
		}
		((Control)this).OnMouseUp(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		foreach (CollapseItem item in items)
		{
			if (item.Contains(e.X, e.Y))
			{
				SetCursor(val: true);
				return;
			}
		}
		SetCursor(val: false);
		((Control)this).OnMouseMove(e);
	}
}
