using System.Drawing;

namespace AntdUI.Chat;

internal class CacheFont
{
	private Rectangle _rect;

	public int i { get; set; }

	public string text { get; set; }

	public Rectangle rect
	{
		get
		{
			return _rect;
		}
		set
		{
			_rect = value;
		}
	}

	public bool emoji { get; set; }

	public bool retun { get; set; }

	public int width { get; set; }

	public GraphemeSplitter.STRE_TYPE type { get; set; }

	public int? imageHeight { get; set; }

	public CacheFont(string _text, bool _emoji, int _width, GraphemeSplitter.STRE_TYPE Type)
	{
		text = _text;
		emoji = _emoji;
		width = _width;
		type = Type;
	}

	internal void SetOffset(Point point)
	{
		_rect.Offset(point);
	}
}
