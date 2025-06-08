using System.Drawing.Drawing2D;

namespace AntdUI.Svg.Pathing;

public sealed class SvgClosePathSegment : SvgPathSegment
{
	public override void AddToPath(GraphicsPath graphicsPath)
	{
		PathData pathData = graphicsPath.PathData;
		if (pathData.Points.Length == 0)
		{
			return;
		}
		if (!pathData.Points[0].Equals((object?)pathData.Points[pathData.Points.Length - 1]))
		{
			int num = pathData.Points.Length - 1;
			while (num >= 0 && pathData.Types[num] > 0)
			{
				num--;
			}
			if (num < 0)
			{
				num = 0;
			}
			graphicsPath.AddLine(pathData.Points[pathData.Points.Length - 1], pathData.Points[num]);
		}
		graphicsPath.CloseFigure();
	}

	public override string ToString()
	{
		return "z";
	}
}
