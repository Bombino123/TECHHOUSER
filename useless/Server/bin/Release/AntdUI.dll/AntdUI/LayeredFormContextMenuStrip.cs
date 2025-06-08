using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormContextMenuStrip : ILayeredFormOpacity, SubLayeredForm
{
	private class InRect
	{
		public ContextMenuStripItem? Tag { get; set; }

		public bool Hover { get; set; }

		public bool Down { get; set; }

		public Rectangle RectT { get; set; }

		public Rectangle RectIcon { get; set; }

		public Rectangle RectCheck { get; set; }

		public Rectangle RectSub { get; set; }

		public Rectangle Rect { get; set; }

		public int y { get; set; }

		public int h { get; set; }

		public InRect(ContextMenuStripItem tag, int _y, int _h)
		{
			Tag = tag;
			y = _y;
			h = _h;
		}

		public InRect(int _y, int _h)
		{
			y = _y;
			h = _h;
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
	}

	private ContextMenuStrip.Config config;

	private Font FontSub;

	private float radius;

	private ScrollY scrollY;

	private bool has_subtext;

	private InRect[] rectsContent;

	private readonly StringFormat stringLeft = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)0);

	private readonly StringFormat stringRight = Helper.SF_Ellipsis((StringAlignment)1, (StringAlignment)2);

	private Bitmap? shadow_temp;

	private int select_index = -1;

	private int oldSub = -1;

	private ManualResetEvent? resetEvent;

	private LayeredFormContextMenuStrip? subForm;

	public override bool MessageEnable => true;

	public override bool MessageCloseSub => true;

	public override bool MessageClickMe => false;

	public LayeredFormContextMenuStrip(ContextMenuStrip.Config _config)
	{
		PARENT = _config.Control;
		if (_config.TopMost)
		{
			Helper.SetTopMost(((Control)this).Handle);
			base.MessageCloseMouseLeave = true;
		}
		else
		{
			_config.Control.SetTopMost(((Control)this).Handle);
		}
		Point point = _config.Location ?? Control.MousePosition;
		maxalpha = 250;
		config = _config;
		((Control)this).Font = config.Font ?? config.Control.Font;
		FontSub = ((Control)this).Font;
		rectsContent = Init(config.Items);
		scrollY = new ScrollY((ILayeredForm)this);
		switch (config.Align)
		{
		case TAlign.BL:
		case TAlign.LB:
			point.X -= base.TargetRect.Width;
			point.Offset(10, -10);
			break;
		case TAlign.TL:
		case TAlign.LT:
			point.X -= base.TargetRect.Width;
			point.Y -= base.TargetRect.Height;
			point.Offset(10, 10);
			break;
		case TAlign.Left:
			point.X -= base.TargetRect.Width;
			point.Y -= base.TargetRect.Height / 2;
			point.Offset(10, 0);
			break;
		case TAlign.Right:
			point.Y -= base.TargetRect.Height / 2;
			point.Offset(-10, 0);
			break;
		case TAlign.Top:
			point.X -= base.TargetRect.Width / 2;
			point.Y -= base.TargetRect.Height;
			point.Offset(0, 10);
			break;
		case TAlign.Bottom:
			point.X -= base.TargetRect.Width / 2;
			point.Offset(0, -10);
			break;
		case TAlign.TR:
		case TAlign.RT:
			point.Y -= base.TargetRect.Height;
			point.Offset(-10, 10);
			break;
		default:
			point.Offset(-10, -10);
			break;
		}
		Init(point);
		KeyCall = delegate(Keys keys)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Invalid comparison between Unknown and I4
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Invalid comparison between Unknown and I4
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Invalid comparison between Unknown and I4
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Invalid comparison between Unknown and I4
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Invalid comparison between Unknown and I4
			if ((int)keys == 27)
			{
				IClose();
				return true;
			}
			if ((int)keys == 13)
			{
				if (select_index > -1)
				{
					InRect it = rectsContent[select_index];
					if (ClickItem(it))
					{
						return true;
					}
				}
			}
			else
			{
				if ((int)keys == 38)
				{
					select_index--;
					if (select_index < 0)
					{
						select_index = rectsContent.Length - 1;
					}
					while (rectsContent[select_index].Tag == null)
					{
						select_index--;
						if (select_index < 0)
						{
							select_index = rectsContent.Length - 1;
						}
					}
					InRect[] array = rectsContent;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Hover = false;
					}
					FocusItem(rectsContent[select_index]);
					return true;
				}
				if ((int)keys == 40)
				{
					if (select_index == -1)
					{
						select_index = 0;
					}
					else
					{
						select_index++;
						if (select_index > rectsContent.Length - 1)
						{
							select_index = 0;
						}
					}
					while (rectsContent[select_index].Tag == null)
					{
						select_index++;
						if (select_index > rectsContent.Length - 1)
						{
							select_index = 0;
						}
					}
					InRect[] array = rectsContent;
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Hover = false;
					}
					FocusItem(rectsContent[select_index]);
					return true;
				}
				if ((int)keys == 37)
				{
					IClose();
					return true;
				}
				if ((int)keys == 39)
				{
					if (select_index > -1)
					{
						InRect inRect = rectsContent[select_index];
						if (inRect.Tag != null && inRect.Tag.Sub != null && inRect.Tag.Sub.Length != 0)
						{
							if (subForm == null)
							{
								subForm = new LayeredFormContextMenuStrip(config, this, new Point(base.TargetRect.X + (inRect.Rect.X + inRect.Rect.Width) - 20, base.TargetRect.Y + inRect.Rect.Y - 20 - (scrollY.Show ? ((int)scrollY.Value) : 0)), inRect.Tag.Sub);
								((Form)subForm).Show((IWin32Window)(object)this);
							}
							else
							{
								subForm?.IClose();
								subForm = null;
							}
							return true;
						}
					}
					return true;
				}
			}
			return false;
		};
	}

	public LayeredFormContextMenuStrip(ContextMenuStrip.Config _config, LayeredFormContextMenuStrip parent, Point point, IContextMenuStripItem[] subs)
	{
		PARENT = (Control?)(object)parent;
		maxalpha = 250;
		config = _config;
		((Control)this).Font = config.Font ?? config.Control.Font;
		FontSub = ((Control)this).Font;
		if (_config.TopMost)
		{
			Helper.SetTopMost(((Control)this).Handle);
		}
		else
		{
			_config.Control.SetTopMost(((Control)this).Handle);
		}
		rectsContent = Init(subs);
		scrollY = new ScrollY((ILayeredForm)this);
		Init(point);
	}

	private void Init(Point point)
	{
		Rectangle workingArea = Screen.FromPoint(point).WorkingArea;
		if (point.X < workingArea.X)
		{
			point.X = workingArea.X;
		}
		else if (point.X > workingArea.X + workingArea.Width - base.TargetRect.Width)
		{
			point.X = workingArea.X + workingArea.Width - base.TargetRect.Width;
		}
		if (base.TargetRect.Height > workingArea.Height)
		{
			int num = rectsContent[0].y / 2 / 2;
			int height = base.TargetRect.Height;
			int height2 = workingArea.Height;
			scrollY.Rect = new Rectangle(base.TargetRect.Width - num - scrollY.SIZE, 10 + num, scrollY.SIZE, height2 - 20 - num * 2);
			scrollY.Show = true;
			scrollY.SetVrSize(height, height2);
			SetSizeH(height2);
		}
		if (point.Y < workingArea.Y)
		{
			point.Y = workingArea.Y;
		}
		else if (point.Y > workingArea.Y + workingArea.Height - base.TargetRect.Height)
		{
			point.Y = workingArea.Y + workingArea.Height - base.TargetRect.Height;
		}
		SetLocation(point);
	}

	private InRect[] Init(IContextMenuStripItem[] Items)
	{
		IContextMenuStripItem[] Items2 = Items;
		return Helper.GDI(delegate(Canvas g)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Expected O, but got Unknown
			float dpi = Config.Dpi;
			radius = (int)((float)config.Radius * dpi);
			int num = (int)Math.Round(1f * dpi);
			int num2 = (int)Math.Round(8f * dpi);
			int num3 = num2 / 2;
			int num4 = (int)Math.Round(16f * dpi);
			int num5 = num4 * 2;
			((Control)this).Padding = new Padding(num4);
			List<InRect> list = new List<InRect>(Items2.Length);
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			int num9 = 0;
			int num10 = 0;
			IContextMenuStripItem[] array = Items2;
			foreach (IContextMenuStripItem contextMenuStripItem in array)
			{
				if (contextMenuStripItem is ContextMenuStripItem contextMenuStripItem2)
				{
					if (!has_subtext && contextMenuStripItem2.SubText != null)
					{
						has_subtext = true;
					}
					Size size = g.MeasureString(contextMenuStripItem2.Text + contextMenuStripItem2.SubText, ((Control)this).Font);
					int width = size.Width;
					int height = size.Height;
					int num11 = height + num2;
					if (num10 == 0 && contextMenuStripItem2.Sub != null && contextMenuStripItem2.Sub.Length != 0)
					{
						num10 = height;
					}
					if (num8 == 0 && (contextMenuStripItem2.Icon != null || contextMenuStripItem2.IconSvg != null))
					{
						num8 = (int)((float)height * 0.68f);
					}
					if (num9 == 0 && contextMenuStripItem2.Checked)
					{
						num9 = (int)((float)height * 0.8f);
					}
					if (width > num6)
					{
						num6 = width;
					}
					list.Add(new InRect(contextMenuStripItem2, num4 + num7, num11));
					num7 += num11 + num3;
				}
				else if (contextMenuStripItem is ContextMenuStripItemDivider)
				{
					list.Add(new InRect(num4 + num7, num2));
					num7 += num2;
				}
			}
			if (has_subtext)
			{
				FontSub = new Font(FontSub.FontFamily, FontSub.Size * 0.8f);
			}
			int num12 = ((num8 > 0 || num9 > 0) ? ((num8 > 0 && num9 > 0) ? (num8 + num9 + num3 * 3) : ((num8 <= 0) ? (num9 + num3) : (num8 + num3))) : 0);
			int num13 = num2 * 2;
			int num14 = num4 + num12;
			num6 += num12;
			int num15 = num6 + num10 + num5 + num13;
			foreach (InRect item in list)
			{
				if (item.Tag == null)
				{
					item.Rect = new Rectangle(10, item.y + (item.h - num) / 2, num15 - 20, num);
				}
				else
				{
					item.Rect = new Rectangle(num4, item.y, num6 + num10 + num13, item.h);
					if (item.Tag.Sub != null && item.Tag.Sub.Length != 0)
					{
						item.RectSub = new Rectangle(item.Rect.Right - num3 - num10, item.y + (item.h - num10) / 2, num10, num10);
					}
					int num16 = num4 + num3;
					if (num9 > 0)
					{
						if (item.Tag.Checked)
						{
							item.RectCheck = new Rectangle(num16 + num3, item.y + (item.h - num9) / 2, num9, num9);
						}
						num16 += num9 + num2;
						item.RectT = new Rectangle(num14 + num2, item.y, num6 - num12 - num3, item.h);
					}
					else
					{
						item.RectT = new Rectangle(num14 + num2, item.y, num6 - num12, item.h);
					}
					if ((num8 > 0 && item.Tag.Icon != null) || item.Tag.IconSvg != null)
					{
						item.RectIcon = new Rectangle(num16 + num3, item.y + (item.h - num8) / 2, num8, num8);
					}
				}
			}
			SetSize(num15, num7 - num3 + num5);
			return list.ToArray();
		});
	}

	protected override void OnDeactivate(EventArgs e)
	{
		IClose();
		subForm?.IClose();
		subForm = null;
		((Form)this).OnDeactivate(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (has_subtext)
		{
			FontSub.Dispose();
		}
		subForm?.IClose();
		subForm = null;
		resetEvent?.WaitDispose();
		resetEvent = null;
		base.Dispose(disposing);
	}

	public override Bitmap PrintBit()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Expected O, but got Unknown
		//IL_0491: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Expected O, but got Unknown
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Expected O, but got Unknown
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Expected O, but got Unknown
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rect_read = new Rectangle(10, 10, targetRectXY.Width - 20, targetRectXY.Height - 20);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using Canvas canvas = Graphics.FromImage((Image)(object)val).High();
		GraphicsPath val2 = DrawShadow(canvas, targetRectXY, rect_read);
		try
		{
			canvas.Fill(Colour.BgElevated.Get("ContextMenuStrip"), val2);
			SolidBrush val3 = new SolidBrush(Colour.Text.Get("ContextMenuStrip"));
			try
			{
				SolidBrush val4 = new SolidBrush(Colour.Split.Get("ContextMenuStrip"));
				try
				{
					SolidBrush val5 = new SolidBrush(Colour.TextSecondary.Get("ContextMenuStrip"));
					try
					{
						SolidBrush val6 = new SolidBrush(Colour.TextQuaternary.Get("ContextMenuStrip"));
						try
						{
							if (scrollY.Show)
							{
								canvas.SetClip(val2);
								canvas.TranslateTransform(0f, 0f - scrollY.Value);
							}
							InRect[] array = rectsContent;
							foreach (InRect inRect in array)
							{
								if (inRect.Tag == null)
								{
									canvas.Fill((Brush)(object)val4, inRect.Rect);
									continue;
								}
								if (inRect.Hover)
								{
									GraphicsPath val7 = inRect.Rect.RoundPath(radius);
									try
									{
										canvas.Fill(Colour.PrimaryBg.Get("ContextMenuStrip"), val7);
									}
									finally
									{
										((IDisposable)val7)?.Dispose();
									}
								}
								if (inRect.Tag.Enabled)
								{
									canvas.String(inRect.Tag.SubText, FontSub, (Brush)(object)val5, inRect.RectT, stringRight);
									if (inRect.Tag.Fore.HasValue)
									{
										SolidBrush val8 = new SolidBrush(inRect.Tag.Fore.Value);
										try
										{
											canvas.String(inRect.Tag.Text, ((Control)this).Font, (Brush)(object)val8, inRect.RectT, stringLeft);
										}
										finally
										{
											((IDisposable)val8)?.Dispose();
										}
									}
									else
									{
										canvas.String(inRect.Tag.Text, ((Control)this).Font, (Brush)(object)val3, inRect.RectT, stringLeft);
									}
									if (inRect.Tag.Sub != null && inRect.Tag.Sub.Length != 0)
									{
										Pen val9 = new Pen(Colour.TextSecondary.Get("ContextMenuStrip"), 2f * Config.Dpi);
										try
										{
											LineCap startCap = (LineCap)2;
											val9.EndCap = (LineCap)2;
											val9.StartCap = startCap;
											canvas.DrawLines(val9, TAlignMini.Right.TriangleLines(inRect.RectSub));
										}
										finally
										{
											((IDisposable)val9)?.Dispose();
										}
									}
									if (inRect.Tag.Checked)
									{
										Pen val10 = new Pen(Colour.Primary.Get("ContextMenuStrip"), 3f * Config.Dpi);
										try
										{
											canvas.DrawLines(val10, PaintArrow(inRect.RectCheck));
										}
										finally
										{
											((IDisposable)val10)?.Dispose();
										}
									}
									if (inRect.Tag.IconSvg != null)
									{
										Bitmap val11 = inRect.Tag.IconSvg.SvgToBmp(inRect.RectIcon.Width, inRect.RectIcon.Height, inRect.Tag.Fore ?? val3.Color);
										try
										{
											if (val11 != null)
											{
												canvas.Image(val11, inRect.RectIcon);
											}
										}
										finally
										{
											((IDisposable)val11)?.Dispose();
										}
									}
									else if (inRect.Tag.Icon != null)
									{
										canvas.Image(inRect.Tag.Icon, inRect.RectIcon);
									}
									continue;
								}
								canvas.String(inRect.Tag.SubText, FontSub, (Brush)(object)val6, inRect.RectT, stringRight);
								canvas.String(inRect.Tag.Text, ((Control)this).Font, (Brush)(object)val6, inRect.RectT, stringLeft);
								if (inRect.Tag.Sub != null && inRect.Tag.Sub.Length != 0)
								{
									Pen val12 = new Pen(Colour.TextQuaternary.Get("ContextMenuStrip"), 2f * Config.Dpi);
									try
									{
										LineCap startCap = (LineCap)2;
										val12.EndCap = (LineCap)2;
										val12.StartCap = startCap;
										canvas.DrawLines(val12, TAlignMini.Right.TriangleLines(inRect.RectSub));
									}
									finally
									{
										((IDisposable)val12)?.Dispose();
									}
								}
								if (inRect.Tag.Checked)
								{
									Pen val13 = new Pen(Colour.Primary.Get("ContextMenuStrip"), 3f * Config.Dpi);
									try
									{
										canvas.DrawLines(val13, PaintArrow(inRect.RectCheck));
									}
									finally
									{
										((IDisposable)val13)?.Dispose();
									}
								}
								if (inRect.Tag.IconSvg != null)
								{
									Bitmap val14 = inRect.Tag.IconSvg.SvgToBmp(inRect.RectIcon.Width, inRect.RectIcon.Height, val6.Color);
									try
									{
										if (val14 != null)
										{
											canvas.Image(val14, inRect.RectIcon);
										}
									}
									finally
									{
										((IDisposable)val14)?.Dispose();
									}
								}
								else if (inRect.Tag.Icon != null)
								{
									canvas.Image(inRect.Tag.Icon, inRect.RectIcon);
								}
							}
							canvas.ResetTransform();
							canvas.ResetClip();
							scrollY.Paint(canvas);
							return val;
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

	internal PointF[] PaintArrow(Rectangle rect)
	{
		float num = (float)rect.Height * 0.15f;
		float num2 = (float)rect.Height * 0.2f;
		float num3 = (float)rect.Height * 0.26f;
		return new PointF[3]
		{
			new PointF((float)rect.X + num, rect.Y + rect.Height / 2),
			new PointF((float)rect.X + (float)rect.Width * 0.4f, (float)rect.Y + ((float)rect.Height - num3)),
			new PointF((float)(rect.X + rect.Width) - num2, (float)rect.Y + num2)
		};
	}

	private GraphicsPath DrawShadow(Canvas g, Rectangle rect_client, Rectangle rect_read)
	{
		GraphicsPath val = rect_read.RoundPath(radius);
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

	protected override void OnMouseDown(MouseEventArgs e)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		if (!scrollY.MouseDown(e.Location))
		{
			return;
		}
		OnTouchDown(e.X, e.Y);
		select_index = -1;
		if ((int)e.Button == 1048576)
		{
			int num = (scrollY.Show ? ((int)scrollY.Value) : 0);
			for (int i = 0; i < rectsContent.Length; i++)
			{
				InRect inRect = rectsContent[i];
				if (inRect.Tag != null && inRect.Tag.Enabled && inRect.Rect.Contains(e.X, e.Y + num))
				{
					select_index = i;
					inRect.Down = true;
					((Control)this).OnMouseDown(e);
					return;
				}
			}
		}
		((Control)this).OnMouseDown(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (scrollY.MouseUp(e.Location) && OnTouchUp())
		{
			int num = (scrollY.Show ? ((int)scrollY.Value) : 0);
			InRect[] array = rectsContent;
			foreach (InRect it in array)
			{
				if (!it.Down)
				{
					continue;
				}
				if (it.Rect.Contains(e.X, e.Y + num) && it.Tag != null && (it.Tag.Sub == null || it.Tag.Sub.Length == 0))
				{
					IClose();
					LayeredFormContextMenuStrip layeredFormContextMenuStrip = this;
					while (layeredFormContextMenuStrip.PARENT is LayeredFormContextMenuStrip layeredFormContextMenuStrip2 && layeredFormContextMenuStrip.PARENT != layeredFormContextMenuStrip2)
					{
						layeredFormContextMenuStrip2.IClose();
						layeredFormContextMenuStrip = layeredFormContextMenuStrip2;
					}
					resetEvent = new ManualResetEvent(initialState: false);
					ITask.Run(delegate
					{
						if (!Config.Animation || !resetEvent.Wait(close: false))
						{
							if (config.CallSleep > 0)
							{
								Thread.Sleep(config.CallSleep);
							}
							config.Control.BeginInvoke((Delegate)(Action)delegate
							{
								config.Call(it.Tag);
							});
						}
					});
				}
				it.Down = false;
				return;
			}
		}
		((Control)this).OnMouseUp(e);
	}

	private bool ClickItem(InRect it)
	{
		InRect it2 = it;
		if (it2.Tag != null)
		{
			if (it2.Tag.Sub == null || it2.Tag.Sub.Length == 0)
			{
				IClose();
				LayeredFormContextMenuStrip layeredFormContextMenuStrip = this;
				while (layeredFormContextMenuStrip.PARENT is LayeredFormContextMenuStrip layeredFormContextMenuStrip2)
				{
					layeredFormContextMenuStrip2.IClose();
					layeredFormContextMenuStrip = layeredFormContextMenuStrip2;
				}
				resetEvent = new ManualResetEvent(initialState: false);
				ITask.Run(delegate
				{
					if (!resetEvent.Wait(close: false))
					{
						if (config.CallSleep > 0)
						{
							Thread.Sleep(config.CallSleep);
						}
						config.Control.BeginInvoke((Delegate)(Action)delegate
						{
							config.Call(it2.Tag);
						});
					}
				});
			}
			else if (subForm == null)
			{
				subForm = new LayeredFormContextMenuStrip(config, this, new Point(base.TargetRect.X + (it2.Rect.X + it2.Rect.Width) - 20, base.TargetRect.Y + it2.Rect.Y - 20 - (scrollY.Show ? ((int)scrollY.Value) : 0)), it2.Tag.Sub);
				((Form)subForm).Show((IWin32Window)(object)this);
			}
			else
			{
				subForm?.IClose();
				subForm = null;
			}
			return true;
		}
		return false;
	}

	private void FocusItem(InRect it)
	{
		if (it.SetHover(val: true))
		{
			if (scrollY.Show)
			{
				scrollY.Value = it.Rect.Y - it.Rect.Height;
			}
			Print();
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (scrollY.MouseMove(e.Location) && OnTouchMove(e.X, e.Y))
		{
			int num = 0;
			int num2 = -1;
			int num3 = (scrollY.Show ? ((int)scrollY.Value) : 0);
			for (int i = 0; i < rectsContent.Length; i++)
			{
				InRect inRect = rectsContent[i];
				if (inRect.Tag == null)
				{
					continue;
				}
				if (inRect.Tag.Enabled)
				{
					bool flag = inRect.Rect.Contains(e.X, e.Y + num3);
					if (flag)
					{
						num2 = i;
					}
					if (inRect.Hover != flag)
					{
						inRect.Hover = flag;
						num++;
					}
				}
				else if (inRect.Hover)
				{
					inRect.Hover = false;
					num++;
				}
			}
			SetCursor(num2 > 0);
			if (num > 0)
			{
				Print();
			}
			select_index = num2;
			if (num2 > -1)
			{
				SetCursor(val: true);
				if (oldSub == num2)
				{
					return;
				}
				InRect inRect2 = rectsContent[num2];
				oldSub = num2;
				subForm?.IClose();
				subForm = null;
				if (inRect2.Tag != null && inRect2.Tag.Sub != null && inRect2.Tag.Sub.Length != 0)
				{
					subForm = new LayeredFormContextMenuStrip(config, this, new Point(base.TargetRect.X + (inRect2.Rect.X + inRect2.Rect.Width) - 20, base.TargetRect.Y + inRect2.Rect.Y - 20 - (scrollY.Show ? ((int)scrollY.Value) : 0)), inRect2.Tag.Sub);
					((Form)subForm).Show((IWin32Window)(object)this);
				}
			}
			else
			{
				oldSub = -1;
				subForm?.IClose();
				subForm = null;
				SetCursor(val: false);
			}
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		scrollY.MouseWheel(e.Delta);
		base.OnMouseWheel(e);
	}

	protected override bool OnTouchScrollY(int value)
	{
		return scrollY.MouseWheel(value);
	}

	ILayeredForm? SubLayeredForm.SubForm()
	{
		return subForm;
	}
}
