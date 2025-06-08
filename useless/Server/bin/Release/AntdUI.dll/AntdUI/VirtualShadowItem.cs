using System.Drawing;

namespace AntdUI;

public abstract class VirtualShadowItem : VirtualItem
{
	internal Rectangle RECT_S;

	internal ITask? ThreadHover;

	internal float AnimationHoverValue = 0.1f;

	internal bool AnimationHover;

	internal void SetRECTS(int x, int y, int w, int h)
	{
		RECT_S.Width = w;
		RECT_S.Height = h;
		RECT_S.X = x;
		RECT_S.Y = y;
	}
}
