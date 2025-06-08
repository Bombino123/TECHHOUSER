using System.Drawing;

namespace AntdUI;

public class CellDivider : ICell
{
	public override string? ToString()
	{
		return null;
	}

	public override void PaintBack(Canvas g)
	{
	}

	public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore)
	{
		g.Fill(Colour.Split.Get("Divider"), base.Rect);
	}

	public override Size GetSize(Canvas g, Font font, int gap, int gap2)
	{
		return new Size(0, g.MeasureString("龍Qq", font).Height - gap);
	}

	public override void SetRect(Canvas g, Font font, Rectangle rect, Size size, int gap, int gap2)
	{
		base.Rect = new Rectangle(rect.X + (rect.Width - 1) / 2, rect.Y + (rect.Height - size.Height) / 2, 1, size.Height);
	}
}
