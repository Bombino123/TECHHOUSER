using System.Drawing;
using GMap.NET.WindowsForms;

namespace GMap.NET;

internal class GMapMarkerTile : GMapMarker
{
	private static Brush Fill = (Brush)new SolidBrush(Color.FromArgb(155, Color.Blue));

	public GMapMarkerTile(PointLatLng p, int size)
		: base(p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.Size = new Size(size, size);
	}

	public override void OnRender(Graphics g)
	{
		g.FillRectangle(Fill, new Rectangle(base.LocalPosition.X, base.LocalPosition.Y, base.Size.Width, base.Size.Height));
	}
}
