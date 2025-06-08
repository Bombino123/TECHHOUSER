using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormPopover : ILayeredFormOpacity
{
	private class InRect
	{
		public Popover.TextRow Text { get; set; }

		public Rectangle Rect { get; set; }

		public InRect(Popover.TextRow text, Rectangle rect)
		{
			Text = text;
			Rect = rect;
		}
	}

	private Popover.Config config;

	internal bool topMost;

	private Form? form;

	private Bitmap? tempContent;

	private Rectangle rectTitle;

	private Rectangle rectContent;

	private InRect[]? rectsContent;

	private bool rtext;

	private bool hasmouse;

	private readonly StringFormat stringLeft = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)0);

	private readonly StringFormat stringCenter = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private Bitmap? shadow_temp;

	public override bool MessageEnable => true;

	public override bool MessageCloseSub => true;

	public LayeredFormPopover(Popover.Config _config)
	{
		Popover.Config _config2 = _config;
		base._002Ector();
		LayeredFormPopover layeredFormPopover = this;
		maxalpha = byte.MaxValue;
		config = _config2;
		topMost = config.Control.SetTopMost(((Control)this).Handle);
		((Control)this).Font = config.Font ?? config.Control.Font;
		Helper.GDI(delegate(Canvas g)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Expected O, but got Unknown
			//IL_068b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0692: Expected O, but got Unknown
			//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_03f5: Expected O, but got Unknown
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Expected O, but got Unknown
			float dpi = Config.Dpi;
			int num = (int)Math.Round(8f * dpi);
			int num2 = (int)Math.Round(16f * dpi);
			int num3 = num2 * 2;
			((Control)layeredFormPopover).Padding = new Padding(num2);
			object content = layeredFormPopover.config.Content;
			Control val = (Control)((content is Control) ? content : null);
			if (val != null)
			{
				val.BackColor = Colour.BgElevated.Get("Popover");
				val.ForeColor = Colour.Text.Get("Popover");
				int num5 = (val.Width = (int)Math.Round((float)val.Width * dpi) + 2);
				int num6;
				if (_config2.Title == null)
				{
					num6 = val.Height;
					layeredFormPopover.rectContent = new Rectangle(num2, num2, num5, val.Height);
				}
				else
				{
					Font val2 = new Font(((Control)layeredFormPopover).Font.FontFamily, ((Control)layeredFormPopover).Font.Size, (FontStyle)1);
					try
					{
						Size size = g.MeasureString(layeredFormPopover.config.Title, val2, num5);
						num6 = size.Height + num + val.Height;
						layeredFormPopover.rectTitle = new Rectangle(num2, num2, num5, size.Height + num);
						layeredFormPopover.rectContent = new Rectangle(layeredFormPopover.rectTitle.X, layeredFormPopover.rectTitle.Bottom, num5, num6 - size.Height - num);
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				layeredFormPopover.tempContent = new Bitmap(val.Width, val.Height);
				Helper.DpiAuto(Config.Dpi, val);
				val.Size = new Size(((Image)layeredFormPopover.tempContent).Width, ((Image)layeredFormPopover.tempContent).Height);
				val.DrawToBitmap(layeredFormPopover.tempContent, new Rectangle(0, 0, ((Image)layeredFormPopover.tempContent).Width, ((Image)layeredFormPopover.tempContent).Height));
				layeredFormPopover.SetSize(num5 + num3, num6 + num3);
			}
			else if (layeredFormPopover.config.Content is IList<Popover.TextRow> list)
			{
				layeredFormPopover.rtext = true;
				if (_config2.Title != null)
				{
					Font val3 = new Font(((Control)layeredFormPopover).Font.FontFamily, ((Control)layeredFormPopover).Font.Size, (FontStyle)1);
					try
					{
						Size size2 = g.MeasureString(layeredFormPopover.config.Title, val3);
						List<int[]> list2 = new List<int[]>(list.Count);
						int num7 = 0;
						int num8 = 0;
						foreach (Popover.TextRow item in list)
						{
							if (item.Call != null)
							{
								layeredFormPopover.hasmouse = true;
							}
							Size size3 = g.MeasureString(item.Text, item.Font ?? ((Control)layeredFormPopover).Font);
							int num9 = size3.Width + (int)((float)item.Gap * dpi);
							list2.Add(new int[3]
							{
								num2 + num7,
								num2 + size2.Height + num,
								num9
							});
							if (num8 < size3.Height)
							{
								num8 = size3.Height;
							}
							num7 += num9;
						}
						List<InRect> list3 = new List<InRect>(list2.Count);
						for (int i = 0; i < list2.Count; i++)
						{
							int[] array = list2[i];
							list3.Add(new InRect(list[i], new Rectangle(array[0], array[1], array[2], num8)));
						}
						layeredFormPopover.rectsContent = list3.ToArray();
						int num10 = ((num7 > size2.Width) ? num7 : size2.Width);
						int num11 = size2.Height + num + num8;
						layeredFormPopover.rectTitle = new Rectangle(num2, num2, num10, size2.Height + num);
						layeredFormPopover.rectContent = new Rectangle(layeredFormPopover.rectTitle.X, layeredFormPopover.rectTitle.Bottom, num10, num8);
						layeredFormPopover.SetSize(num10 + num3, num11 + num3);
						return;
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				List<int[]> list4 = new List<int[]>(list.Count);
				int num12 = 0;
				int num13 = 0;
				foreach (Popover.TextRow item2 in list)
				{
					if (item2.Call != null)
					{
						layeredFormPopover.hasmouse = true;
					}
					Size size4 = g.MeasureString(item2.Text, item2.Font ?? ((Control)layeredFormPopover).Font);
					int num14 = size4.Width + (int)((float)item2.Gap * dpi);
					list4.Add(new int[3]
					{
						num2 + num12,
						num2,
						num14
					});
					if (num13 < size4.Height)
					{
						num13 = size4.Height;
					}
					num12 += num14;
				}
				List<InRect> list5 = new List<InRect>(list4.Count);
				for (int j = 0; j < list4.Count; j++)
				{
					int[] array2 = list4[j];
					list5.Add(new InRect(list[j], new Rectangle(array2[0], array2[1], array2[2], num13)));
				}
				layeredFormPopover.rectsContent = list5.ToArray();
				layeredFormPopover.rectContent = new Rectangle(num2, num2, num12, num13);
				layeredFormPopover.SetSize(num12 + num3, num13 + num3);
			}
			else
			{
				layeredFormPopover.rtext = true;
				string text = layeredFormPopover.config.Content.ToString();
				if (_config2.Title != null)
				{
					Font val4 = new Font(((Control)layeredFormPopover).Font.FontFamily, ((Control)layeredFormPopover).Font.Size, (FontStyle)1);
					try
					{
						Size size5 = g.MeasureString(layeredFormPopover.config.Title, val4);
						Size size6 = g.MeasureString(text, ((Control)layeredFormPopover).Font);
						int num15 = ((size6.Width > size5.Width) ? size6.Width : size5.Width);
						int num16 = size5.Height + num + size6.Height;
						layeredFormPopover.rectTitle = new Rectangle(num2, num2, num15, size5.Height + num);
						layeredFormPopover.rectContent = new Rectangle(layeredFormPopover.rectTitle.X, layeredFormPopover.rectTitle.Bottom, num15, num16 - size5.Height - num);
						layeredFormPopover.SetSize(num15 + num3, num16 + num3);
						return;
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				Size size7 = g.MeasureString(text, ((Control)layeredFormPopover).Font);
				int width = size7.Width;
				int height = size7.Height;
				layeredFormPopover.rectContent = new Rectangle(num2, num2, width, height);
				layeredFormPopover.SetSize(width + num3, height + num3);
			}
		});
		if (config.CustomPoint.HasValue)
		{
			_ = config.CustomPoint.Value.Location;
			SetLocation(config.ArrowAlign.AlignPoint(config.CustomPoint.Value.Location, config.CustomPoint.Value.Size, base.TargetRect.Width, base.TargetRect.Height));
			return;
		}
		Point point = config.Control.PointToScreen(Point.Empty);
		if (config.Offset is RectangleF rectangleF)
		{
			SetLocation(config.ArrowAlign.AlignPoint(new Rectangle(point.X + (int)rectangleF.X, point.Y + (int)rectangleF.Y, (int)rectangleF.Width, (int)rectangleF.Height), base.TargetRect.Width, base.TargetRect.Height));
		}
		else if (config.Offset is Rectangle rectangle)
		{
			SetLocation(config.ArrowAlign.AlignPoint(new Rectangle(point.X + rectangle.X, point.Y + rectangle.Y, rectangle.Width, rectangle.Height), base.TargetRect.Width, base.TargetRect.Height));
		}
		else
		{
			SetLocation(config.ArrowAlign.AlignPoint(point, config.Control.Size, base.TargetRect.Width, base.TargetRect.Height));
		}
	}

	public override void LoadOK()
	{
		if (((Control)this).IsHandleCreated)
		{
			object content = config.Content;
			Control control = (Control)((content is Control) ? content : null);
			if (control != null)
			{
				((Control)this).BeginInvoke((Delegate)(Action)delegate
				{
					LoadContent(control);
				});
			}
		}
		if (config.AutoClose > 0)
		{
			ITask.Run(delegate
			{
				Thread.Sleep(config.AutoClose * 1000);
				IClose();
			});
		}
	}

	private void LoadContent(Control control)
	{
		Point location = new Point(base.TargetRect.Location.X + rectContent.X, base.TargetRect.Location.Y + rectContent.Y);
		Size size = new Size(rectContent.Width, rectContent.Height);
		DoubleBufferForm doubleBufferForm = new DoubleBufferForm((Form)(object)this, control);
		((Form)doubleBufferForm).FormBorderStyle = (FormBorderStyle)0;
		((Form)doubleBufferForm).Location = location;
		((Control)doubleBufferForm).MaximumSize = size;
		((Control)doubleBufferForm).MinimumSize = size;
		((Form)doubleBufferForm).Size = size;
		form = (Form?)(object)doubleBufferForm;
		((Component)(object)control).Disposed += Control_Disposed;
		form.Show((IWin32Window)(object)this);
		form.Location = location;
		PARENT = (Control?)(object)form;
		config.OnControlLoad?.Invoke();
	}

	private void Control_Disposed(object? sender, EventArgs e)
	{
		IClose();
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		object content = config.Content;
		Control val = (Control)((content is Control) ? content : null);
		if (val != null)
		{
			if (val.IsDisposed)
			{
				((Component)(object)form)?.Dispose();
				base.OnClosing(e);
				return;
			}
			tempContent = new Bitmap(val.Width, val.Height);
			val.DrawToBitmap(tempContent, new Rectangle(0, 0, ((Image)tempContent).Width, ((Image)tempContent).Height));
			if (form != null)
			{
				form.Location = new Point(-((Control)form).Width * 2, -((Control)form).Height * 2);
			}
		}
		base.OnClosing(e);
	}

	protected override void Dispose(bool disposing)
	{
		Bitmap? obj = shadow_temp;
		if (obj != null)
		{
			((Image)obj).Dispose();
		}
		shadow_temp = null;
		Bitmap? obj2 = tempContent;
		if (obj2 != null)
		{
			((Image)obj2).Dispose();
		}
		tempContent = null;
		object content = config.Content;
		Control val = (Control)((content is Control) ? content : null);
		if (val != null)
		{
			((Component)(object)val).Disposed -= Control_Disposed;
			((Component)(object)val).Dispose();
		}
		config.Content = null;
		((Component)(object)form)?.Dispose();
		base.Dispose(disposing);
	}

	public override Bitmap PrintBit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rectangle = new Rectangle(10, 10, targetRectXY.Width - 20, targetRectXY.Height - 20);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using (Canvas canvas = Graphics.FromImage((Image)(object)val).High())
		{
			GraphicsPath val2 = DrawShadow(canvas, targetRectXY, rectangle);
			try
			{
				SolidBrush val3 = new SolidBrush(Colour.BgElevated.Get("Popover"));
				try
				{
					canvas.Fill((Brush)(object)val3, val2);
					if (config.ArrowAlign != 0)
					{
						canvas.FillPolygon((Brush)(object)val3, config.ArrowAlign.AlignLines(config.ArrowSize, targetRectXY, rectangle));
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
				if (tempContent != null)
				{
					canvas.Image(tempContent, rectContent);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			if (config.Title != null || rtext)
			{
				SolidBrush val4 = new SolidBrush(Colour.Text.Get("Popover"));
				try
				{
					Font val5 = new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size, (FontStyle)1);
					try
					{
						canvas.String(config.Title, val5, (Brush)(object)val4, rectTitle, stringLeft);
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
					if (rtext)
					{
						if (config.Content is IList<Popover.TextRow> list && rectsContent != null)
						{
							for (int i = 0; i < list.Count; i++)
							{
								Popover.TextRow textRow = list[i];
								if (textRow.Fore.HasValue)
								{
									SolidBrush val6 = new SolidBrush(textRow.Fore.Value);
									try
									{
										canvas.String(textRow.Text, textRow.Font ?? ((Control)this).Font, (Brush)(object)val6, rectsContent[i].Rect, stringCenter);
									}
									finally
									{
										((IDisposable)val6)?.Dispose();
									}
								}
								else
								{
									canvas.String(textRow.Text, textRow.Font ?? ((Control)this).Font, (Brush)(object)val4, rectsContent[i].Rect, stringCenter);
								}
							}
						}
						else
						{
							canvas.String(config.Content.ToString(), ((Control)this).Font, (Brush)(object)val4, rectContent, stringLeft);
						}
					}
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
		}
		return val;
	}

	private GraphicsPath DrawShadow(Canvas g, Rectangle rect_client, Rectangle rect_read)
	{
		GraphicsPath val = rect_read.RoundPath((int)((float)config.Radius * Config.Dpi));
		if (Config.ShadowEnabled)
		{
			if (shadow_temp == null || ((Image)shadow_temp).Width != rect_client.Width || ((Image)shadow_temp).Height != rect_client.Height)
			{
				Bitmap? obj = shadow_temp;
				if (obj != null)
				{
					((Image)obj).Dispose();
				}
				shadow_temp = val.PaintShadow(rect_client.Width, rect_client.Height);
			}
			g.Image(shadow_temp, rect_client, 0.2f);
		}
		return val;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (hasmouse && rectsContent != null)
		{
			InRect[] array = rectsContent;
			foreach (InRect inRect in array)
			{
				if (inRect.Text.Call != null && inRect.Rect.Contains(e.Location))
				{
					SetCursor(val: true);
					return;
				}
			}
			SetCursor(val: false);
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		if (hasmouse && rectsContent != null && (int)e.Button == 1048576)
		{
			InRect[] array = rectsContent;
			foreach (InRect inRect in array)
			{
				if (inRect.Text.Call != null && inRect.Rect.Contains(e.Location))
				{
					inRect.Text.Call();
					return;
				}
			}
		}
		((Control)this).OnMouseClick(e);
	}
}
