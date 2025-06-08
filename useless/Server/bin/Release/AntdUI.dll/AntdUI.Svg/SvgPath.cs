using System.Drawing;
using System.Drawing.Drawing2D;
using AntdUI.Svg.Pathing;

namespace AntdUI.Svg;

public class SvgPath : SvgMarkerElement
{
	private GraphicsPath _path;

	public override string ClassName => "path";

	[SvgAttribute("d", true)]
	public SvgPathSegmentList PathData
	{
		get
		{
			return Attributes.GetAttribute<SvgPathSegmentList>("d");
		}
		set
		{
			Attributes["d"] = value;
			value._owner = this;
			IsPathDirty = true;
		}
	}

	[SvgAttribute("pathLength", true)]
	public float PathLength
	{
		get
		{
			return Attributes.GetAttribute<float>("pathLength");
		}
		set
		{
			Attributes["pathLength"] = value;
		}
	}

	public override RectangleF Bounds => Path(null).GetBounds();

	public override GraphicsPath Path(ISvgRenderer renderer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (_path == null || IsPathDirty)
		{
			_path = new GraphicsPath();
			foreach (SvgPathSegment pathDatum in PathData)
			{
				pathDatum.AddToPath(_path);
			}
			if (_path.PointCount == 0)
			{
				if (PathData.Count > 0)
				{
					SvgPathSegment last = PathData.Last;
					_path.AddLine(last.End, last.End);
					Fill = SvgPaintServer.None;
				}
				else
				{
					_path = null;
				}
			}
			IsPathDirty = false;
		}
		return _path;
	}

	internal void OnPathUpdated()
	{
		IsPathDirty = true;
		OnAttributeChanged(new AttributeEventArgs
		{
			Attribute = "d",
			Value = Attributes.GetAttribute<SvgPathSegmentList>("d")
		});
	}

	public SvgPath()
	{
		SvgPathSegmentList svgPathSegmentList = new SvgPathSegmentList();
		Attributes["d"] = svgPathSegmentList;
		svgPathSegmentList._owner = this;
	}
}
