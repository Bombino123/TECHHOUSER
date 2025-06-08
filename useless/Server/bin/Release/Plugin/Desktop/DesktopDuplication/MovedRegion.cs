using System.Drawing;

namespace DesktopDuplication;

public struct MovedRegion
{
	public Point Source { get; internal set; }

	public Rectangle Destination { get; internal set; }
}
