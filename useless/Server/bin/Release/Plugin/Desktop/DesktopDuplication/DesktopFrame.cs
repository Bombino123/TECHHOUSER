using System.Drawing;

namespace DesktopDuplication;

public class DesktopFrame
{
	public Bitmap DesktopImage { get; internal set; }

	public MovedRegion[] MovedRegions { get; internal set; }

	public Rectangle[] UpdatedRegions { get; internal set; }

	public int AccumulatedFrames { get; internal set; }

	public Point CursorLocation { get; internal set; }

	public bool CursorVisible { get; internal set; }

	public bool ProtectedContentMaskedOut { get; internal set; }

	public bool RectanglesCoalesced { get; internal set; }
}
