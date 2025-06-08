using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

internal class NotificationFrm : ILayeredFormAnimate
{
	private Font font_title;

	internal Notification.Config config;

	private int shadow_size = 10;

	private readonly StringFormat s_f = Helper.SF_ALL((StringAlignment)1, (StringAlignment)1);

	private readonly StringFormat s_f_left = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private readonly StringFormat s_f_left_left = Helper.SF((StringAlignment)0, (StringAlignment)0);

	private Bitmap? shadow_temp;

	private Rectangle rect_icon;

	private Rectangle rect_title;

	private Rectangle rect_txt;

	private Rectangle rect_close;

	private Rectangle rect_link_text;

	private Rectangle rect_links;

	private ITaskOpacity close_button;

	internal override TAlignFrom Align => config.Align;

	internal override bool ActiveAnimation => false;

	public NotificationFrm(Notification.Config _config)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		config = _config;
		if (config.TopMost)
		{
			Helper.SetTopMost(((Control)this).Handle);
		}
		else
		{
			((Control?)(object)config.Form).SetTopMost(((Control)this).Handle);
		}
		shadow_size = (int)((float)shadow_size * Config.Dpi);
		if (config.Font != null)
		{
			((Control)this).Font = config.Font;
		}
		else if (Config.Font != null)
		{
			((Control)this).Font = Config.Font;
		}
		else
		{
			((Control)this).Font = ((Control)config.Form).Font;
		}
		font_title = (Font)(((object)config.FontTitle) ?? ((object)new Font(((Control)this).Font.FontFamily, ((Control)this).Font.Size * 1.14f, (FontStyle)(((_003F?)config.FontStyleTitle) ?? ((Control)this).Font.Style))));
		((Form)this).Icon = config.Form.Icon;
		Helper.GDI(delegate(Canvas g)
		{
			SetSize(RenderMeasure(g, shadow_size));
		});
		close_button = new ITaskOpacity((ILayeredForm)this);
	}

	protected override void Dispose(bool disposing)
	{
		config.OnClose?.Invoke();
		config.OnClose = null;
		close_button.Dispose();
		base.Dispose(disposing);
	}

	public bool IInit()
	{
		if (SetPosition(config.Form, config.ShowInWindow || Config.ShowInWindowByNotification))
		{
			return true;
		}
		if (config.AutoClose > 0)
		{
			ITask.Run(delegate
			{
				Thread.Sleep(config.AutoClose * 1000);
				CloseMe();
			});
		}
		PlayAnimation();
		return false;
	}

	public override Bitmap PrintBit()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Expected O, but got Unknown
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rect_read = targetRectXY.PaddingRect(((Control)this).Padding, (float)shadow_size);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using (Canvas canvas = Graphics.FromImage((Image)(object)val).High())
		{
			GraphicsPath val2 = DrawShadow(canvas, targetRectXY, rect_read);
			try
			{
				canvas.Fill(Colour.BgElevated.Get("Notification"), val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			if (config.Icon != 0)
			{
				canvas.PaintIcons(config.Icon, rect_icon, "Notification");
			}
			if (config.CloseIcon)
			{
				if (close_button.Animation)
				{
					GraphicsPath val3 = rect_close.RoundPath((int)(4f * Config.Dpi));
					try
					{
						canvas.Fill(Helper.ToColor(close_button.Value, Colour.FillSecondary.Get("Notification")), val3);
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
					canvas.PaintIconClose(rect_close, Colour.Text.Get("Notification"), 0.6f);
				}
				else if (close_button.Switch)
				{
					GraphicsPath val4 = rect_close.RoundPath((int)(4f * Config.Dpi));
					try
					{
						canvas.Fill(Colour.FillSecondary.Get("Notification"), val4);
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
					canvas.PaintIconClose(rect_close, Colour.Text.Get("Notification"), 0.6f);
				}
				else
				{
					canvas.PaintIconClose(rect_close, Colour.TextTertiary.Get("Notification"), 0.6f);
				}
			}
			SolidBrush val5 = new SolidBrush(Colour.TextBase.Get("Notification"));
			try
			{
				canvas.String(config.Title, font_title, (Brush)(object)val5, rect_title, s_f_left);
				canvas.String(config.Text, ((Control)this).Font, (Brush)(object)val5, rect_txt, s_f_left_left);
			}
			finally
			{
				((IDisposable)val5)?.Dispose();
			}
			if (config.Link != null)
			{
				Pen val6 = new Pen(Colour.Primary.Get("Notification"), Config.Dpi);
				try
				{
					canvas.String(config.Link.Text, ((Control)this).Font, Colour.Primary.Get("Notification"), rect_link_text, s_f);
					canvas.DrawLines(val6, TAlignMini.Right.TriangleLines(rect_links));
				}
				finally
				{
					((IDisposable)val6)?.Dispose();
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

	private Size RenderMeasure(Canvas g, int shadow)
	{
		int num = shadow * 2;
		float dpi = Config.Dpi;
		Size size = g.MeasureString(config.Title, font_title, 10000, s_f_left);
		int num2 = (int)((float)config.Padding.Width * dpi);
		int num3 = (int)((float)config.Padding.Height * dpi);
		int num4 = (int)Math.Ceiling(360f * dpi);
		int num5 = (int)(8f * dpi);
		int num6 = (int)Math.Ceiling(22f * dpi);
		if (size.Width > num4)
		{
			num4 = size.Width;
			if (config.CloseIcon)
			{
				num4 += num6 + num5;
			}
		}
		Size size2 = g.MeasureString(config.Text, ((Control)this).Font, num4);
		int num7 = (config.CloseIcon ? (size.Width + num6 + num5) : size.Width);
		int width = size2.Width;
		int num8 = ((width > num7) ? width : num7);
		if (config.Icon == TType.None)
		{
			rect_title = new Rectangle(shadow + num2, shadow + num3, num8, size.Height);
			int num9 = size.Height;
			if (config.CloseIcon)
			{
				rect_close = new Rectangle(rect_title.Right - num6, rect_title.Y, num6, num6);
			}
			rect_txt = new Rectangle(shadow + num2, rect_title.Bottom + num5, rect_title.Width, size2.Height);
			if (size2.Height > 0)
			{
				num9 += size2.Height + num5;
			}
			if (config.Link != null)
			{
				Size size3 = g.MeasureString(config.Link.Text, ((Control)this).Font, 10000, s_f);
				rect_link_text = new Rectangle(rect_title.X, rect_txt.Bottom + num5, size3.Width, size3.Height);
				rect_links = new Rectangle(rect_link_text.Right, rect_link_text.Y, rect_link_text.Height, rect_link_text.Height);
				num9 += size3.Height + num5;
			}
			return new Size(num8 + num2 * 2 + num, num9 + num3 * 2 + num);
		}
		int num10 = (int)Math.Ceiling((float)size.Height * 1.14f);
		int num11 = num10 / 2;
		rect_icon = new Rectangle(shadow + num2, shadow + num3, num10, num10);
		rect_title = new Rectangle(rect_icon.X + rect_icon.Width + num11, shadow + num3, num8, num10);
		int num12 = num10;
		rect_txt = new Rectangle(rect_title.X, rect_title.Bottom + num5, rect_title.Width, size2.Height);
		if (config.CloseIcon)
		{
			rect_close = new Rectangle(rect_title.Right - num6, rect_title.Y, num6, num6);
		}
		if (size2.Height > 0)
		{
			num12 += size2.Height + num5;
		}
		if (config.Link != null)
		{
			Size size4 = g.MeasureString(config.Link.Text, ((Control)this).Font, 10000, s_f);
			rect_link_text = new Rectangle(rect_title.X, rect_txt.Bottom + num5, size4.Width, size4.Height);
			rect_links = new Rectangle(rect_link_text.Right, rect_link_text.Y, rect_link_text.Height, rect_link_text.Height);
			num12 += size4.Height + num5;
		}
		return new Size(num8 + num10 + num11 + num2 * 2 + num, num12 + num3 * 2 + num);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (config.CloseIcon)
		{
			close_button.MaxValue = Colour.FillSecondary.Get("Notification").A;
			close_button.Switch = rect_close.Contains(e.Location);
			SetCursor(close_button.Switch);
			if (close_button.Switch)
			{
				((Control)this).OnMouseMove(e);
				return;
			}
		}
		if (config.Link != null)
		{
			SetCursor(rect_link_text.Contains(e.Location));
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		if (config.Link == null || !rect_link_text.Contains(e.Location) || config.Link.Call())
		{
			if (config.ClickClose)
			{
				CloseMe();
			}
			((Control)this).OnMouseClick(e);
		}
	}
}
