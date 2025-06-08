using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AntdUI.Svg;

public class SvgFallbackPaintServer : SvgPaintServer
{
	private IEnumerable<SvgPaintServer> _fallbacks;

	private SvgPaintServer _primary;

	public SvgFallbackPaintServer()
	{
	}

	public SvgFallbackPaintServer(SvgPaintServer primary, IEnumerable<SvgPaintServer> fallbacks)
		: this()
	{
		_fallbacks = fallbacks;
		_primary = primary;
	}

	public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		try
		{
			_primary.GetCallback = () => _fallbacks.FirstOrDefault();
			return _primary.GetBrush(styleOwner, renderer, opacity, forStroke);
		}
		finally
		{
			_primary.GetCallback = null;
		}
	}
}
