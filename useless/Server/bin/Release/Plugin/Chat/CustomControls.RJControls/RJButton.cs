using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomControls.RJControls;

public class RJButton : Button
{
	private int borderSize;

	private int borderRadius;

	private Color borderColor = Color.PaleVioletRed;

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
	public int BorderRadius
	{
		get
		{
			return borderRadius;
		}
		set
		{
			borderRadius = value;
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
	public Color BackgroundColor
	{
		get
		{
			return ((Control)this).BackColor;
		}
		set
		{
			((Control)this).BackColor = value;
		}
	}

	[Category("RJ Code Advance")]
	public Color TextColor
	{
		get
		{
			return ((Control)this).ForeColor;
		}
		set
		{
			((Control)this).ForeColor = value;
		}
	}

	public RJButton()
	{
		((ButtonBase)this).FlatStyle = (FlatStyle)0;
		((ButtonBase)this).FlatAppearance.BorderSize = 0;
		((Control)this).Size = new Size(150, 40);
		((Control)this).BackColor = Color.MediumSlateBlue;
		((Control)this).ForeColor = Color.White;
		((Control)this).Resize += Button_Resize;
	}

	private GraphicsPath GetFigurePath(Rectangle rect, int radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		GraphicsPath val = new GraphicsPath();
		float num = (float)radius * 2f;
		val.StartFigure();
		val.AddArc((float)rect.X, (float)rect.Y, num, num, 180f, 90f);
		val.AddArc((float)rect.Right - num, (float)rect.Y, num, num, 270f, 90f);
		val.AddArc((float)rect.Right - num, (float)rect.Bottom - num, num, num, 0f, 90f);
		val.AddArc((float)rect.X, (float)rect.Bottom - num, num, num, 90f, 90f);
		val.CloseFigure();
		return val;
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		((ButtonBase)this).OnPaint(pevent);
		Rectangle clientRectangle = ((Control)this).ClientRectangle;
		Rectangle rect = Rectangle.Inflate(clientRectangle, -borderSize, -borderSize);
		int num = 2;
		if (borderSize > 0)
		{
			num = borderSize;
		}
		if (borderRadius > 2)
		{
			GraphicsPath figurePath = GetFigurePath(clientRectangle, borderRadius);
			try
			{
				GraphicsPath figurePath2 = GetFigurePath(rect, borderRadius - borderSize);
				try
				{
					Pen val = new Pen(((Control)this).Parent.BackColor, (float)num);
					try
					{
						Pen val2 = new Pen(borderColor, (float)borderSize);
						try
						{
							pevent.Graphics.SmoothingMode = (SmoothingMode)4;
							((Control)this).Region = new Region(figurePath);
							pevent.Graphics.DrawPath(val, figurePath);
							if (borderSize >= 1)
							{
								pevent.Graphics.DrawPath(val2, figurePath2);
							}
							return;
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
				finally
				{
					((IDisposable)figurePath2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)figurePath)?.Dispose();
			}
		}
		pevent.Graphics.SmoothingMode = (SmoothingMode)3;
		((Control)this).Region = new Region(clientRectangle);
		if (borderSize >= 1)
		{
			Pen val3 = new Pen(borderColor, (float)borderSize);
			try
			{
				val3.Alignment = (PenAlignment)1;
				pevent.Graphics.DrawRectangle(val3, 0, 0, ((Control)this).Width - 1, ((Control)this).Height - 1);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		((Control)this).OnHandleCreated(e);
		((Control)this).Parent.BackColorChanged += Container_BackColorChanged;
	}

	private void Container_BackColorChanged(object sender, EventArgs e)
	{
		((Control)this).Invalidate();
	}

	private void Button_Resize(object sender, EventArgs e)
	{
		if (borderRadius > ((Control)this).Height)
		{
			borderRadius = ((Control)this).Height;
		}
	}
}
