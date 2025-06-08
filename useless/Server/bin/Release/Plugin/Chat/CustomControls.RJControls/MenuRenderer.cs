using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CustomControls.RJControls;

public class MenuRenderer : ToolStripProfessionalRenderer
{
	private Color primaryColor;

	private Color textColor;

	private int arrowThickness;

	public MenuRenderer(bool isMainMenu, Color primaryColor, Color textColor)
		: base((ProfessionalColorTable)(object)new MenuColorTable(isMainMenu, primaryColor))
	{
		this.primaryColor = primaryColor;
		if (isMainMenu)
		{
			arrowThickness = 3;
			if (textColor == Color.Empty)
			{
				this.textColor = Color.Gainsboro;
			}
			else
			{
				this.textColor = textColor;
			}
		}
		else
		{
			arrowThickness = 2;
			if (textColor == Color.Empty)
			{
				this.textColor = Color.DimGray;
			}
			else
			{
				this.textColor = textColor;
			}
		}
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		((ToolStripProfessionalRenderer)this).OnRenderItemText(e);
		((ToolStripItemRenderEventArgs)e).Item.ForeColor = (((ToolStripItemRenderEventArgs)e).Item.Selected ? Color.White : textColor);
	}

	protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		Graphics graphics = e.Graphics;
		Size size = new Size(5, 12);
		Color color = (e.Item.Selected ? Color.White : primaryColor);
		Rectangle rectangle = new Rectangle(e.ArrowRectangle.Location.X, (e.ArrowRectangle.Height - size.Height) / 2, size.Width, size.Height);
		GraphicsPath val = new GraphicsPath();
		try
		{
			Pen val2 = new Pen(color, (float)arrowThickness);
			try
			{
				graphics.SmoothingMode = (SmoothingMode)4;
				val.AddLine(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top + rectangle.Height / 2);
				val.AddLine(rectangle.Right, rectangle.Top + rectangle.Height / 2, rectangle.Left, rectangle.Top + rectangle.Height);
				graphics.DrawPath(val2, val);
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
