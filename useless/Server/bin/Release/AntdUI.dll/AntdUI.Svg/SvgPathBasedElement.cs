using System.Drawing;
using System.Drawing.Drawing2D;

namespace AntdUI.Svg;

public abstract class SvgPathBasedElement : SvgVisualElement
{
	public override RectangleF Bounds
	{
		get
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Expected O, but got Unknown
			GraphicsPath val = Path(null);
			if (val != null)
			{
				if (base.Transforms != null && base.Transforms.Count > 0)
				{
					val = (GraphicsPath)val.Clone();
					val.Transform(base.Transforms.GetMatrix());
				}
				return val.GetBounds();
			}
			return default(RectangleF);
		}
	}
}
