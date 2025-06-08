using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI;

internal class MessageFrm : ILayeredFormAnimate
{
	internal Message.Config config;

	private int shadow_size = 10;

	private bool loading;

	private bool loadingend = true;

	private int AnimationLoadingValue;

	private ITask? ThreadLoading;

	private readonly StringFormat s_f_left = Helper.SF_ALL((StringAlignment)1, (StringAlignment)0);

	private Bitmap? shadow_temp;

	private Rectangle rect_icon;

	private Rectangle rect_loading;

	private Rectangle rect_txt;

	internal override TAlignFrom Align => config.Align;

	internal override bool ActiveAnimation => false;

	public MessageFrm(Message.Config _config)
	{
		config = _config;
		((Control?)(object)config.Form).SetTopMost(((Control)this).Handle);
		shadow_size = (int)((float)shadow_size * Config.Dpi);
		loading = _config.Call != null;
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
		((Form)this).Icon = config.Form.Icon;
		Helper.GDI(delegate(Canvas g)
		{
			SetSize(RenderMeasure(g, shadow_size));
		});
	}

	public bool IInit()
	{
		if (SetPosition(config.Form, config.ShowInWindow || Config.ShowInWindowByMessage))
		{
			return true;
		}
		if (loading)
		{
			ThreadLoading = new ITask((Control)(object)this, delegate(int i)
			{
				AnimationLoadingValue = i;
				Print();
				return loading;
			}, 20, 360, 10);
		}
		loadingend = false;
		ITask.Run(delegate
		{
			if (config.Call != null)
			{
				Message.Config obj = config;
				obj.refresh = (Action)Delegate.Combine(obj.refresh, (Action)delegate
				{
					if (IRefresh())
					{
						loadingend = true;
					}
				});
				try
				{
					config.Call(config);
				}
				catch
				{
				}
				loading = false;
				ThreadLoading?.Dispose();
				if (IRefresh())
				{
					loadingend = true;
				}
			}
		}, delegate
		{
			loadingend = true;
			if (config.AutoClose > 0)
			{
				Thread.Sleep(config.AutoClose * 1000);
				CloseMe();
			}
		});
		PlayAnimation();
		return false;
	}

	private bool IRefresh()
	{
		int width = base.TargetRect.Width;
		if (((Control)this).IsHandleCreated)
		{
			Helper.GDI(delegate(Canvas g)
			{
				SetSize(RenderMeasure(g, shadow_size));
			});
			DisposeAnimation();
			SetPositionCenter(width);
			return false;
		}
		return true;
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		if (loadingend && config.ClickClose)
		{
			CloseMe();
		}
		((Control)this).OnMouseClick(e);
	}

	public override Bitmap PrintBit()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		Rectangle targetRectXY = base.TargetRectXY;
		Rectangle rect_read = targetRectXY.PaddingRect(((Control)this).Padding, (float)shadow_size);
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using Canvas canvas = Graphics.FromImage((Image)(object)val).High();
		GraphicsPath val2 = DrawShadow(canvas, targetRectXY, rect_read);
		try
		{
			canvas.Fill(Colour.BgElevated.Get("Message"), val2);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		if (loading)
		{
			float num = 3f * Config.Dpi;
			canvas.DrawEllipse(Colour.Fill.Get("Message"), num, rect_loading);
			Pen val3 = new Pen(Colour.Primary.Get("Message"), num);
			try
			{
				LineCap startCap = (LineCap)2;
				val3.EndCap = (LineCap)2;
				val3.StartCap = startCap;
				canvas.DrawArc(val3, rect_loading, AnimationLoadingValue, 100f);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		else if (config.Icon != 0)
		{
			canvas.PaintIcons(config.Icon, rect_icon, "Message");
		}
		SolidBrush val4 = new SolidBrush(Colour.TextBase.Get("Message"));
		try
		{
			canvas.String(config.Text, ((Control)this).Font, (Brush)(object)val4, rect_txt, s_f_left);
			return val;
		}
		finally
		{
			((IDisposable)val4)?.Dispose();
		}
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
		Size size = g.MeasureString(config.Text, ((Control)this).Font, 10000, s_f_left);
		int num2 = (int)((float)config.Padding.Width * dpi);
		int num3 = (int)((float)config.Padding.Height * dpi);
		int num4 = (int)(8f * dpi);
		int num5 = size.Height + num3 * 2;
		if (loading)
		{
			int num6 = (int)((float)size.Height * 0.86f);
			rect_icon = new Rectangle(shadow + num2, shadow + (num5 - num6) / 2, num6, num6);
			rect_txt = new Rectangle(rect_icon.Right + num4, shadow, size.Width, num5);
			int num7 = (int)((float)num6 * 0.86f);
			rect_loading = new Rectangle(rect_icon.X + (rect_icon.Width - num7) / 2, rect_icon.Y + (rect_icon.Height - num7) / 2, num7, num7);
			return new Size(size.Width + num6 + num4 + num2 * 2 + num, num5 + num);
		}
		if (config.Icon == TType.None)
		{
			rect_txt = new Rectangle(shadow + num2, shadow, size.Width, num5);
			return new Size(size.Width + num2 * 2 + num, num5 + num);
		}
		int num8 = (int)((float)size.Height * 0.86f);
		rect_icon = new Rectangle(shadow + num2, shadow + (num5 - num8) / 2, num8, num8);
		rect_txt = new Rectangle(rect_icon.Right + num4, shadow, size.Width, num5);
		return new Size(size.Width + num8 + num4 + num2 * 2 + num, num5 + num);
	}

	protected override void Dispose(bool disposing)
	{
		ThreadLoading?.Dispose();
		base.Dispose(disposing);
	}
}
