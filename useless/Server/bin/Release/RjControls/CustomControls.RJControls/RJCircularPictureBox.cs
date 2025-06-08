using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomControls.RJControls;

internal class RJCircularPictureBox : PictureBox
{
	private int borderSize = 2;

	private Color borderColor = Color.RoyalBlue;

	private Color borderColor2 = Color.HotPink;

	private DashStyle borderLineStyle;

	private DashCap borderCapStyle;

	private float gradientAngle = 50f;

	[Category("RJ Code Advance")]
	public int BorderSize
	{
		get
		{
			return borderSize;
		}
		set
		{
			borderSize = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public Color BorderColor
	{
		get
		{
			return borderColor;
		}
		set
		{
			borderColor = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public Color BorderColor2
	{
		get
		{
			return borderColor2;
		}
		set
		{
			borderColor2 = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public DashStyle BorderLineStyle
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return borderLineStyle;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			borderLineStyle = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public DashCap BorderCapStyle
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return borderCapStyle;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			borderCapStyle = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public float GradientAngle
	{
		get
		{
			return gradientAngle;
		}
		set
		{
			gradientAngle = value;
			((Control)this).Invalidate();
		}
	}

	public RJCircularPictureBox()
	{
		((Control)this).Size = new Size(100, 100);
		((PictureBox)this).SizeMode = (PictureBoxSizeMode)1;
	}

	protected override void OnResize(EventArgs e)
	{
		((PictureBox)this).OnResize(e);
		((Control)this).Size = new Size(((Control)this).Width, ((Control)this).Width);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		((PictureBox)this).OnPaint(pe);
		Graphics graphics = pe.Graphics;
		Rectangle rectangle = Rectangle.Inflate(((Control)this).ClientRectangle, -1, -1);
		Rectangle rectangle2 = Rectangle.Inflate(rectangle, -borderSize, -borderSize);
		int num = ((borderSize <= 0) ? 1 : (borderSize * 3));
		LinearGradientBrush val = new LinearGradientBrush(rectangle2, borderColor, borderColor2, gradientAngle);
		try
		{
			GraphicsPath val2 = new GraphicsPath();
			try
			{
				Pen val3 = new Pen(((Control)this).Parent.BackColor, (float)num);
				try
				{
					Pen val4 = new Pen((Brush)(object)val, (float)borderSize);
					try
					{
						graphics.SmoothingMode = (SmoothingMode)4;
						val4.DashStyle = borderLineStyle;
						val4.DashCap = borderCapStyle;
						val2.AddEllipse(rectangle);
						((Control)this).Region = new Region(val2);
						graphics.DrawEllipse(val3, rectangle);
						if (borderSize > 0)
						{
							graphics.DrawEllipse(val4, rectangle2);
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
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
