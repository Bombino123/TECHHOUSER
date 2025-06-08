using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomControls.RJControls;

public class RJToggleButton : CheckBox
{
	private Color onBackColor = Color.MediumSlateBlue;

	private Color onToggleColor = Color.WhiteSmoke;

	private Color offBackColor = Color.Gray;

	private Color offToggleColor = Color.Gainsboro;

	private bool solidStyle = true;

	[Category("RJ Code Advance")]
	public Color OnBackColor
	{
		get
		{
			return onBackColor;
		}
		set
		{
			onBackColor = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public Color OnToggleColor
	{
		get
		{
			return onToggleColor;
		}
		set
		{
			onToggleColor = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public Color OffBackColor
	{
		get
		{
			return offBackColor;
		}
		set
		{
			offBackColor = value;
			((Control)this).Invalidate();
		}
	}

	[Category("RJ Code Advance")]
	public Color OffToggleColor
	{
		get
		{
			return offToggleColor;
		}
		set
		{
			offToggleColor = value;
			((Control)this).Invalidate();
		}
	}

	[Browsable(false)]
	public override string Text
	{
		get
		{
			return ((ButtonBase)this).Text;
		}
		set
		{
		}
	}

	[Category("RJ Code Advance")]
	[DefaultValue(true)]
	public bool SolidStyle
	{
		get
		{
			return solidStyle;
		}
		set
		{
			solidStyle = value;
			((Control)this).Invalidate();
		}
	}

	public RJToggleButton()
	{
		((Control)this).MinimumSize = new Size(45, 22);
	}

	private GraphicsPath GetFigurePath()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		int num = ((Control)this).Height - 1;
		Rectangle rectangle = new Rectangle(0, 0, num, num);
		Rectangle rectangle2 = new Rectangle(((Control)this).Width - num - 2, 0, num, num);
		GraphicsPath val = new GraphicsPath();
		val.StartFigure();
		val.AddArc(rectangle, 90f, 180f);
		val.AddArc(rectangle2, 270f, 180f);
		val.CloseFigure();
		return val;
	}

	protected override void OnPaint(PaintEventArgs pevent)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		int num = ((Control)this).Height - 5;
		pevent.Graphics.SmoothingMode = (SmoothingMode)4;
		pevent.Graphics.Clear(((Control)this).Parent.BackColor);
		if (((CheckBox)this).Checked)
		{
			if (solidStyle)
			{
				pevent.Graphics.FillPath((Brush)new SolidBrush(onBackColor), GetFigurePath());
			}
			else
			{
				pevent.Graphics.DrawPath(new Pen(onBackColor, 2f), GetFigurePath());
			}
			pevent.Graphics.FillEllipse((Brush)new SolidBrush(onToggleColor), new Rectangle(((Control)this).Width - ((Control)this).Height + 1, 2, num, num));
		}
		else
		{
			if (solidStyle)
			{
				pevent.Graphics.FillPath((Brush)new SolidBrush(offBackColor), GetFigurePath());
			}
			else
			{
				pevent.Graphics.DrawPath(new Pen(offBackColor, 2f), GetFigurePath());
			}
			pevent.Graphics.FillEllipse((Brush)new SolidBrush(offToggleColor), new Rectangle(2, 2, num, num));
		}
	}
}
