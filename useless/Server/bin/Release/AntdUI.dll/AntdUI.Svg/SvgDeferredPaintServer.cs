using System.Drawing;
using System.Linq;

namespace AntdUI.Svg;

public class SvgDeferredPaintServer : SvgPaintServer
{
	private bool _serverLoaded;

	private SvgPaintServer _concreteServer;

	public SvgDocument Document { get; set; }

	public string DeferredId { get; set; }

	public SvgDeferredPaintServer()
	{
	}

	public SvgDeferredPaintServer(SvgDocument document, string id)
	{
		Document = document;
		DeferredId = id;
	}

	public void EnsureServer(SvgElement styleOwner)
	{
		if (_serverLoaded)
		{
			return;
		}
		if (DeferredId == "currentColor" && styleOwner != null)
		{
			SvgElement svgElement = (from e in styleOwner.ParentsAndSelf.OfType<SvgElement>()
				where e.Color != SvgPaintServer.None && e.Color != SvgColourServer.NotSet && e.Color != SvgColourServer.Inherit && e.Color != SvgPaintServer.None
				select e).FirstOrDefault();
			_concreteServer = ((svgElement == null) ? SvgColourServer.NotSet : svgElement.Color);
		}
		else
		{
			_concreteServer = Document.IdManager.GetElementById(DeferredId) as SvgPaintServer;
		}
		_serverLoaded = true;
	}

	public override Brush GetBrush(SvgVisualElement styleOwner, ISvgRenderer renderer, float opacity, bool forStroke = false)
	{
		EnsureServer(styleOwner);
		return _concreteServer.GetBrush(styleOwner, renderer, opacity, forStroke);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SvgDeferredPaintServer svgDeferredPaintServer))
		{
			return false;
		}
		if (Document == svgDeferredPaintServer.Document)
		{
			return DeferredId == svgDeferredPaintServer.DeferredId;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Document == null || DeferredId == null)
		{
			return 0;
		}
		return Document.GetHashCode() ^ DeferredId.GetHashCode();
	}

	public override string ToString()
	{
		if (DeferredId == "currentColor")
		{
			return DeferredId;
		}
		return $"url({DeferredId})";
	}

	public static T TryGet<T>(SvgPaintServer server, SvgElement parent) where T : SvgPaintServer
	{
		if (!(server is SvgDeferredPaintServer svgDeferredPaintServer))
		{
			return server as T;
		}
		svgDeferredPaintServer.EnsureServer(parent);
		return svgDeferredPaintServer._concreteServer as T;
	}
}
