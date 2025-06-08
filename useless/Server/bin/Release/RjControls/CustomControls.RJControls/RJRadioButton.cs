using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomControls.RJControls;

internal class RJRadioButton : RadioButton
{
	private Color checkedColor = Color.MediumSlateBlue;

	private Color unCheckedColor = Color.Gray;

	public Color CheckedColor
	{
		get
		{
			return checkedColor;
		}
		set
		{
			checkedColor = value;
			((Control)this).Invalidate();
		}
	}

	public Color UnCheckedColor
	{
		get
		{
			return unCheckedColor;
		}
		set
		{
			unCheckedColor = value;
			((Control)this).Invalidate();
		}
	}

	public RJRadioButton()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).MinimumSize = new Size(0, 21);
		((Control)this).Padding = new Padding(10, 0, 0, 0);
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected O, but got Unknown
		Graphics graphics = pevent.Graphics;
		graphics.SmoothingMode = (SmoothingMode)4;
		float num = 18f;
		float num2 = 12f;
		RectangleF rectangleF = default(RectangleF);
		rectangleF.X = 0.5f;
		rectangleF.Y = ((float)((Control)this).Height - num) / 2f;
		rectangleF.Width = num;
		rectangleF.Height = num;
		RectangleF rectangleF2 = rectangleF;
		rectangleF = default(RectangleF);
		rectangleF.X = rectangleF2.X + (rectangleF2.Width - num2) / 2f;
		rectangleF.Y = ((float)((Control)this).Height - num2) / 2f;
		rectangleF.Width = num2;
		rectangleF.Height = num2;
		RectangleF rectangleF3 = rectangleF;
		Pen val = new Pen(checkedColor, 1.6f);
		try
		{
			SolidBrush val2 = new SolidBrush(checkedColor);
			try
			{
				SolidBrush val3 = new SolidBrush(((Control)this).ForeColor);
				try
				{
					graphics.Clear(((Control)this).BackColor);
					if (((RadioButton)this).Checked)
					{
						graphics.DrawEllipse(val, rectangleF2);
						graphics.FillEllipse((Brush)(object)val2, rectangleF3);
					}
					else
					{
						val.Color = unCheckedColor;
						graphics.DrawEllipse(val, rectangleF2);
					}
					graphics.DrawString(((Control)this).Text, ((Control)this).Font, (Brush)(object)val3, num + 8f, (float)((((Control)this).Height - TextRenderer.MeasureText(((Control)this).Text, ((Control)this).Font).Height) / 2));
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
