using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace AntdUI;

internal class LayeredFormFloatButton : FormFloatButton, IEventListener
{
	private int BadgeSize = 6;

	private int ShadowXY;

	private bool Loading;

	private ITask? ThreadLoading;

	private readonly StringFormat stringCenter = Helper.SF_NoWrap((StringAlignment)1, (StringAlignment)1);

	private TooltipForm? tooltipForm;

	public override FloatButton.Config config { get; }

	private bool HasLoading
	{
		get
		{
			FloatButton.ConfigBtn[] btns = config.Btns;
			for (int i = 0; i < btns.Length; i++)
			{
				if (btns[i].Loading)
				{
					return true;
				}
			}
			return false;
		}
	}

	public LayeredFormFloatButton(FloatButton.Config _config)
	{
		FloatButton.Config _config2 = _config;
		base._002Ector();
		LayeredFormFloatButton layeredFormFloatButton = this;
		maxalpha = byte.MaxValue;
		config = _config2;
		((Form)this).TopMost = config.TopMost;
		if (!config.TopMost)
		{
			((Control?)(object)config.Form).SetTopMost(((Control)this).Handle);
		}
		((Control)this).Font = (Font)((config.Font == null) ? ((object)((Control)config.Form).Font) : ((object)config.Font));
		Helper.GDI(delegate
		{
			float dpi = Config.Dpi;
			layeredFormFloatButton.BadgeSize = (int)Math.Round((float)layeredFormFloatButton.BadgeSize * dpi);
			_config2.MarginX = (int)Math.Round((float)_config2.MarginX * dpi);
			_config2.MarginY = (int)Math.Round((float)_config2.MarginY * dpi);
			_config2.Size = (int)Math.Round((float)_config2.Size * dpi);
			_config2.Gap = (int)Math.Round((float)_config2.Gap * dpi);
			layeredFormFloatButton.ShadowXY = _config2.Gap / 2;
			int size = _config2.Size;
			int num = size + _config2.Gap;
			int num2 = (int)((float)size * 0.45f);
			int xy = (size - num2) / 2;
			int num3 = 0;
			int num4 = 0;
			if (_config2.Vertical)
			{
				FloatButton.ConfigBtn[] btns = _config2.Btns;
				foreach (FloatButton.ConfigBtn configBtn in btns)
				{
					configBtn.PropertyChanged += layeredFormFloatButton.Notify_PropertyChanged;
					configBtn.rect = new Rectangle(num3, num4, num, num);
					configBtn.rect_read = new Rectangle(num3 + layeredFormFloatButton.ShadowXY, num4 + layeredFormFloatButton.ShadowXY, size, size);
					layeredFormFloatButton.SetIconSize(configBtn, size, xy, num2, dpi);
					num4 += num;
				}
				layeredFormFloatButton.SetSize(size + _config2.Gap, num4);
			}
			else
			{
				FloatButton.ConfigBtn[] btns = _config2.Btns;
				foreach (FloatButton.ConfigBtn configBtn2 in btns)
				{
					configBtn2.PropertyChanged += layeredFormFloatButton.Notify_PropertyChanged;
					configBtn2.rect = new Rectangle(num3, num4, num, num);
					configBtn2.rect_read = new Rectangle(num3 + layeredFormFloatButton.ShadowXY, num4 + layeredFormFloatButton.ShadowXY, size, size);
					layeredFormFloatButton.SetIconSize(configBtn2, size, xy, num2, dpi);
					num3 += num;
				}
				layeredFormFloatButton.SetSize(num3, size + _config2.Gap);
			}
		});
		SetPoint();
		((Control)config.Form).LocationChanged += Form_LSChanged;
		((Control)config.Form).SizeChanged += Form_LSChanged;
	}

	private void Notify_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (sender == null || e.PropertyName == null)
		{
			return;
		}
		if (e.PropertyName == "Loading")
		{
			bool hasLoading = HasLoading;
			if (Loading == hasLoading)
			{
				return;
			}
			Loading = hasLoading;
			if (hasLoading)
			{
				ThreadLoading = new ITask((Control)(object)this, delegate(int i)
				{
					FloatButton.ConfigBtn[] btns = config.Btns;
					foreach (FloatButton.ConfigBtn configBtn in btns)
					{
						if (configBtn.Loading)
						{
							configBtn.AnimationLoadingValue = i;
						}
					}
					Print();
					return Loading;
				}, 10, 360, 6, delegate
				{
					Print();
				});
			}
			else
			{
				ThreadLoading?.Dispose();
				ThreadLoading = null;
				Print();
			}
		}
		else
		{
			Print();
		}
	}

	private bool SetPoint()
	{
		if (config.Control == null)
		{
			Point location = config.Form.Location;
			SetPoint(location.X, location.Y, ((Control)config.Form).Width, ((Control)config.Form).Height);
		}
		else
		{
			if (config.Control.IsDisposed)
			{
				IClose();
				return false;
			}
			Point point = config.Control.PointToScreen(Point.Empty);
			SetPoint(point.X, point.Y, config.Control.Width, config.Control.Height);
		}
		return true;
	}

	private void SetPoint(int x, int y, int w, int h)
	{
		switch (config.Align)
		{
		case TAlign.TL:
		case TAlign.LT:
			SetLocation(new Point(x + config.MarginY, y + config.MarginY));
			break;
		case TAlign.Top:
			SetLocation(new Point(x + (w - base.TargetRect.Width) / 2, y + config.MarginY));
			break;
		case TAlign.TR:
		case TAlign.RT:
			SetLocation(new Point(x + w - config.MarginX - base.TargetRect.Width, y + config.MarginY));
			break;
		case TAlign.Left:
			SetLocation(new Point(x + config.MarginY, y + (h - base.TargetRect.Height) / 2));
			break;
		case TAlign.Right:
			SetLocation(new Point(x + w - config.MarginX - base.TargetRect.Width, y + (h - base.TargetRect.Height) / 2));
			break;
		case TAlign.BL:
		case TAlign.LB:
			SetLocation(new Point(x + config.MarginY, y + h - config.MarginY - base.TargetRect.Height));
			break;
		case TAlign.Bottom:
			SetLocation(new Point(x + (w - base.TargetRect.Width) / 2, y + h - config.MarginY - base.TargetRect.Height));
			break;
		default:
			SetLocation(new Point(x + w - config.MarginX - base.TargetRect.Width, y + h - config.MarginY - base.TargetRect.Height));
			break;
		}
	}

	private void SetIconSize(FloatButton.ConfigBtn it, int size, int xy, int icon_size, float dpi)
	{
		if (it.IconSize.HasValue)
		{
			if (it.IconSize.Value.Width == it.IconSize.Value.Height)
			{
				int num = (int)((float)it.IconSize.Value.Width * dpi);
				it.rect_icon = new Rectangle(it.rect_read.X + (size - num) / 2, it.rect_read.Y + (size - num) / 2, num, num);
			}
			else
			{
				int num2 = (int)((float)it.IconSize.Value.Width * dpi);
				int num3 = (int)((float)it.IconSize.Value.Height * dpi);
				it.rect_icon = new Rectangle(it.rect_read.X + (size - num2) / 2, it.rect_read.Y + (size - num3) / 2, num2, num3);
			}
		}
		else
		{
			it.rect_icon = new Rectangle(it.rect_read.X + xy, it.rect_read.Y + xy, icon_size, icon_size);
		}
	}

	private void Form_LSChanged(object? sender, EventArgs e)
	{
		if (SetPoint())
		{
			Print();
		}
	}

	public override Bitmap PrintBit()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		Rectangle targetRectXY = base.TargetRectXY;
		Bitmap val = new Bitmap(targetRectXY.Width, targetRectXY.Height);
		using Canvas canvas = Graphics.FromImage((Image)(object)val).High();
		FloatButton.ConfigBtn[] btns = config.Btns;
		foreach (FloatButton.ConfigBtn configBtn in btns)
		{
			GraphicsPath val2 = DrawShadow(canvas, configBtn);
			try
			{
				if (configBtn.Loading)
				{
					Color color;
					Color color2;
					switch (configBtn.Type)
					{
					case TTypeMini.Primary:
						color = Colour.Primary.Get("FloatButton");
						color2 = Colour.PrimaryColor.Get("FloatButton");
						break;
					case TTypeMini.Success:
						color = Colour.Success.Get("FloatButton");
						color2 = Colour.SuccessColor.Get("FloatButton");
						break;
					case TTypeMini.Error:
						color = Colour.Error.Get("FloatButton");
						color2 = Colour.ErrorColor.Get("FloatButton");
						break;
					case TTypeMini.Warn:
						color = Colour.Warning.Get("FloatButton");
						color2 = Colour.WarningColor.Get("FloatButton");
						break;
					case TTypeMini.Info:
						color = Colour.Info.Get("FloatButton");
						color2 = Colour.InfoColor.Get("FloatButton");
						break;
					default:
						color = Colour.BgElevated.Get("FloatButton");
						color2 = Colour.Text.Get("FloatButton");
						break;
					}
					if (configBtn.Fore.HasValue)
					{
						color2 = configBtn.Fore.Value;
					}
					canvas.Fill(color, val2);
					float num = (float)configBtn.rect_read.Height * 0.06f;
					Pen val3 = new Pen(Colour.Fill.Get("FloatButton"), num);
					try
					{
						Pen val4 = new Pen(color2, val3.Width);
						try
						{
							canvas.DrawEllipse(val3, configBtn.rect_icon);
							LineCap startCap = (LineCap)2;
							val4.EndCap = (LineCap)2;
							val4.StartCap = startCap;
							canvas.DrawArc(val4, configBtn.rect_icon, configBtn.AnimationLoadingValue, configBtn.LoadingValue * 360f);
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
				else
				{
					Color color3;
					Color color4;
					Color color5;
					if (configBtn.Enabled)
					{
						switch (configBtn.Type)
						{
						case TTypeMini.Primary:
							color3 = Colour.Primary.Get("FloatButton");
							color4 = Colour.PrimaryHover.Get("FloatButton");
							color5 = Colour.PrimaryColor.Get("FloatButton");
							break;
						case TTypeMini.Success:
							color3 = Colour.Success.Get("FloatButton");
							color4 = Colour.SuccessHover.Get("FloatButton");
							color5 = Colour.SuccessColor.Get("FloatButton");
							break;
						case TTypeMini.Error:
							color3 = Colour.Error.Get("FloatButton");
							color4 = Colour.ErrorHover.Get("FloatButton");
							color5 = Colour.ErrorColor.Get("FloatButton");
							break;
						case TTypeMini.Warn:
							color3 = Colour.Warning.Get("FloatButton");
							color4 = Colour.WarningHover.Get("FloatButton");
							color5 = Colour.WarningColor.Get("FloatButton");
							break;
						case TTypeMini.Info:
							color3 = Colour.Info.Get("FloatButton");
							color4 = Colour.InfoHover.Get("FloatButton");
							color5 = Colour.InfoColor.Get("FloatButton");
							break;
						default:
							color3 = Colour.BgElevated.Get("FloatButton");
							color4 = Colour.FillSecondary.Get("FloatButton");
							color5 = Colour.Text.Get("FloatButton");
							break;
						}
						if (configBtn.Fore.HasValue)
						{
							color5 = configBtn.Fore.Value;
						}
					}
					else
					{
						color3 = (color4 = Colour.FillTertiary.Get("FloatButton"));
						color5 = Colour.TextQuaternary.Get("FloatButton");
					}
					canvas.Fill(color3, val2);
					if (configBtn.hover)
					{
						canvas.Fill(color4, val2);
					}
					if (configBtn.IconSvg != null)
					{
						canvas.GetImgExtend(configBtn.IconSvg, configBtn.rect_icon, color5);
					}
					else if (configBtn.Icon != null)
					{
						canvas.Image(configBtn.Icon, configBtn.rect_icon);
					}
					else
					{
						canvas.String(configBtn.Text, ((Control)this).Font, color5, configBtn.rect_read, stringCenter);
					}
				}
				configBtn.PaintBadge(((Control)this).Font, configBtn.rect_read, canvas);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		return val;
	}

	private GraphicsPath DrawShadow(Canvas g, FloatButton.ConfigBtn it)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		bool round = it.Round;
		float radius = (round ? ((float)it.rect_read.Height) : ((float)it.Radius * Config.Dpi));
		GraphicsPath result = it.rect_read.RoundPath(radius, round);
		if (Config.ShadowEnabled && it.Enabled)
		{
			if (it.shadow_temp == null || ((Image)it.shadow_temp).Width != it.rect.Width || ((Image)it.shadow_temp).Height != it.rect.Height)
			{
				Bitmap? shadow_temp = it.shadow_temp;
				if (shadow_temp != null)
				{
					((Image)shadow_temp).Dispose();
				}
				GraphicsPath val = new Rectangle(ShadowXY, ShadowXY, it.rect_read.Width, it.rect_read.Height).RoundPath(radius, round);
				try
				{
					it.shadow_temp = val.PaintShadow(it.rect.Width, it.rect.Height, 14);
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			ImageAttributes val2 = new ImageAttributes();
			try
			{
				ColorMatrix val3 = new ColorMatrix
				{
					Matrix33 = 0.2f
				};
				val2.SetColorMatrix(val3, (ColorMatrixFlag)0, (ColorAdjustType)1);
				g.Image((Image)(object)it.shadow_temp, new Rectangle(it.rect.X, it.rect.Y + 6, it.rect.Width, it.rect.Height), 0, 0, it.rect.Width, it.rect.Height, (GraphicsUnit)2, val2);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		return result;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		int num = 0;
		int num2 = 0;
		FloatButton.ConfigBtn[] btns = config.Btns;
		foreach (FloatButton.ConfigBtn configBtn in btns)
		{
			if (configBtn.Enabled && !configBtn.Loading && configBtn.rect.Contains(e.Location))
			{
				num2++;
				if (configBtn.hover)
				{
					continue;
				}
				configBtn.hover = true;
				num++;
				if (configBtn.Tooltip != null)
				{
					Rectangle targetRect = base.TargetRect;
					Rectangle rect = new Rectangle(targetRect.X + configBtn.rect.X, targetRect.Y + configBtn.rect.Y, configBtn.rect.Width, configBtn.rect.Height);
					if (tooltipForm == null)
					{
						tooltipForm = new TooltipForm((Control)(object)config.Form, rect, configBtn.Tooltip, new TooltipConfig
						{
							Font = ((Control)this).Font,
							ArrowAlign = config.Align.AlignMiniReverse(config.Vertical)
						});
						((Form)tooltipForm).Show((IWin32Window)(object)this);
					}
					else
					{
						tooltipForm.SetText(rect, configBtn.Tooltip);
					}
				}
			}
			else if (configBtn.hover)
			{
				configBtn.hover = false;
				num++;
			}
		}
		SetCursor(num2 > 0);
		if (num > 0)
		{
			Print();
		}
		((Control)this).OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		tooltipForm?.IClose();
		tooltipForm = null;
		int num = 0;
		FloatButton.ConfigBtn[] btns = config.Btns;
		foreach (FloatButton.ConfigBtn configBtn in btns)
		{
			if (configBtn.hover)
			{
				configBtn.hover = false;
				num++;
			}
		}
		SetCursor(val: false);
		if (num > 0)
		{
			Print();
		}
		((Control)this).OnMouseLeave(e);
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)e.Button == 1048576)
		{
			FloatButton.ConfigBtn[] btns = config.Btns;
			foreach (FloatButton.ConfigBtn configBtn in btns)
			{
				if (configBtn.Enabled && !configBtn.Loading && configBtn.rect.Contains(e.Location))
				{
					config.Call(configBtn);
					return;
				}
			}
		}
		((Control)this).OnMouseClick(e);
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		((Control)(object)this).AddListener();
	}

	protected override void Dispose(bool disposing)
	{
		ThreadLoading?.Dispose();
		((Control)config.Form).LocationChanged -= Form_LSChanged;
		((Control)config.Form).SizeChanged -= Form_LSChanged;
		FloatButton.ConfigBtn[] btns = config.Btns;
		for (int i = 0; i < btns.Length; i++)
		{
			btns[i].PropertyChanged -= Notify_PropertyChanged;
		}
		base.Dispose(disposing);
	}

	public void HandleEvent(EventType id, object? tag)
	{
		if (id == EventType.THEME)
		{
			Print();
		}
	}
}
