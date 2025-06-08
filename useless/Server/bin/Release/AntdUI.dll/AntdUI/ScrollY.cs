using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntdUI;

public class ScrollY
{
	private IControl? control;

	private Action Invalidate;

	public bool Back = true;

	public bool Gap = true;

	private bool show;

	public Rectangle Rect;

	public RectangleF Slider;

	internal float val;

	public bool ShowDown;

	private bool hover;

	public bool Show
	{
		get
		{
			return show;
		}
		set
		{
			if (show != value)
			{
				show = value;
				if (!value)
				{
					val = 0f;
				}
			}
		}
	}

	public float Value
	{
		get
		{
			return val;
		}
		set
		{
			float num = SetValue(value);
			if (val != num)
			{
				val = num;
				Invalidate();
			}
		}
	}

	public float VrValue { get; set; }

	public float VrValueI { get; set; }

	public int Height { get; set; }

	public int SIZE { get; set; } = 20;


	public bool ShowX { get; set; }

	private bool Hover
	{
		get
		{
			return hover;
		}
		set
		{
			if (hover != value)
			{
				hover = value;
				Invalidate();
			}
		}
	}

	public ScrollY(IControl _control)
	{
		IControl _control2 = _control;
		base._002Ector();
		Invalidate = delegate
		{
			((Control)_control2).Invalidate();
		};
		control = _control2;
	}

	public ScrollY(FlowLayoutPanel _control)
	{
		FlowLayoutPanel _control2 = _control;
		base._002Ector();
		ScrollY scrollY = this;
		SIZE = SystemInformation.VerticalScrollBarWidth;
		Invalidate = delegate
		{
			((Control)_control2).Invalidate(scrollY.Rect);
		};
	}

	public ScrollY(Control _control)
	{
		Control _control2 = _control;
		base._002Ector();
		Invalidate = delegate
		{
			_control2.Invalidate();
		};
	}

	public ScrollY(ILayeredForm _form)
	{
		ILayeredForm _form2 = _form;
		base._002Ector();
		Invalidate = delegate
		{
			_form2.Print();
		};
		Gap = (Back = false);
	}

	public float SetValue(float value)
	{
		if (value < 0f)
		{
			return 0f;
		}
		if (value > VrValueI)
		{
			return VrValueI;
		}
		return value;
	}

	public void SetVrSize(float len, int height)
	{
		Height = height;
		if (len > 0f && len > (float)height)
		{
			if (ShowX)
			{
				len += (float)SIZE;
			}
			VrValueI = len - (float)height;
			VrValue = len;
			Show = VrValue > (float)height;
			if (Show && val > len - (float)height)
			{
				Value = len - (float)height;
			}
		}
		else
		{
			float vrValue = (VrValueI = 0f);
			VrValue = vrValue;
			Show = false;
		}
	}

	public void SetVrSize(float len)
	{
		SetVrSize(len, Rect.Height);
	}

	public virtual void SizeChange(Rectangle rect)
	{
		Rect = new Rectangle(rect.Right - SIZE, rect.Y, SIZE, rect.Height);
	}

	public virtual void Paint(Canvas g)
	{
		Paint(g, Colour.TextBase.Get("ScrollBar"));
	}

	public virtual void Paint(Canvas g, Color baseColor)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		if (!Show)
		{
			return;
		}
		if (Back && IsPaintScroll())
		{
			SolidBrush val = new SolidBrush(Color.FromArgb(10, baseColor));
			try
			{
				if (ShowX)
				{
					g.Fill((Brush)(object)val, new Rectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height - SIZE));
				}
				else
				{
					g.Fill((Brush)(object)val, Rect);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		float num = (float)Rect.Height / VrValue * (float)Rect.Height;
		if (num < (float)SIZE)
		{
			num = SIZE;
		}
		if (Gap)
		{
			num -= 12f;
		}
		float num2 = ((this.val == 0f) ? 0f : (this.val / (VrValue - (float)Rect.Height) * ((float)(ShowX ? (Rect.Height - SIZE) : Rect.Height) - num)));
		if (Hover)
		{
			Slider = new RectangleF(Rect.X + 6, (float)Rect.Y + num2, 8f, num);
		}
		else
		{
			Slider = new RectangleF(Rect.X + 7, (float)Rect.Y + num2, 6f, num);
		}
		if (Gap)
		{
			if (Slider.Y < 6f)
			{
				Slider.Y = 6f;
			}
			else if (Slider.Y > (float)(ShowX ? (Rect.Height - SIZE) : Rect.Height) - num - 6f)
			{
				Slider.Y = (float)(ShowX ? (Rect.Height - SIZE) : Rect.Height) - num - 6f;
			}
		}
		GraphicsPath val2 = Slider.RoundPath(Slider.Width);
		try
		{
			g.Fill(Color.FromArgb(141, baseColor), val2);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private bool IsPaintScroll()
	{
		if (Config.ScrollBarHide)
		{
			return hover;
		}
		return true;
	}

	public virtual bool MouseDown(Point e)
	{
		if (Show && Rect.Contains(e))
		{
			if (!Slider.Contains(e))
			{
				float num = ((float)e.Y - Slider.Height / 2f) / (float)Rect.Height;
				Value = num * VrValue;
			}
			ShowDown = true;
			return false;
		}
		return true;
	}

	public virtual bool MouseDown(Point e, Action<float> cal)
	{
		if (Show && Rect.Contains(e))
		{
			if (!Slider.Contains(e))
			{
				float num = ((float)e.Y - Slider.Height / 2f) / (float)Rect.Height;
				float num2 = val;
				Value = num * VrValue;
				if (num2 != val)
				{
					cal(val);
				}
			}
			ShowDown = true;
			return false;
		}
		return true;
	}

	public virtual bool MouseUp(Point e)
	{
		ShowDown = false;
		return true;
	}

	public virtual bool MouseMove(Point e)
	{
		if (ShowDown)
		{
			Hover = true;
			float num = ((float)e.Y - Slider.Height / 2f) / (float)Rect.Height;
			Value = num * VrValue;
			return false;
		}
		if (Show && Rect.Contains(e))
		{
			Hover = true;
			control?.SetCursor(val: false);
			return false;
		}
		Hover = false;
		return true;
	}

	public virtual bool MouseMove(Point e, Action<float> cal)
	{
		if (ShowDown)
		{
			Hover = true;
			float num = ((float)e.Y - Slider.Height / 2f) / (float)Rect.Height;
			float num2 = val;
			Value = num * VrValue;
			if (num2 != val)
			{
				cal(val);
			}
			return false;
		}
		if (Show && Rect.Contains(e))
		{
			Hover = true;
			control?.SetCursor(val: false);
			return false;
		}
		Hover = false;
		return true;
	}

	public bool MouseWheel(int delta)
	{
		if (Show && delta != 0)
		{
			Value -= delta;
			return true;
		}
		return false;
	}

	public void Leave()
	{
		Hover = false;
	}
}
