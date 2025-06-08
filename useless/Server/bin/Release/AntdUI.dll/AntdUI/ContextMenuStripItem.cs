using System.Drawing;

namespace AntdUI;

public class ContextMenuStripItem : IContextMenuStripItem
{
	private string _text;

	private string? subText;

	public string? ID { get; set; }

	public string Text
	{
		get
		{
			return Localization.GetLangIN(LocalizationText, _text, new string[2] { "{id}", ID });
		}
		set
		{
			_text = value;
		}
	}

	public string? LocalizationText { get; set; }

	public string? SubText
	{
		get
		{
			return Localization.GetLangI(LocalizationSubText, subText, new string[2] { "{id}", ID });
		}
		set
		{
			subText = value;
		}
	}

	public string? LocalizationSubText { get; set; }

	public Color? Fore { get; set; }

	public Bitmap? Icon { get; set; }

	public string? IconSvg { get; set; }

	public bool Enabled { get; set; } = true;


	public bool Checked { get; set; }

	public IContextMenuStripItem[]? Sub { get; set; }

	public object? Tag { get; set; }

	public ContextMenuStripItem(string text)
	{
		_text = text;
	}

	public ContextMenuStripItem(string text, string subtext)
	{
		_text = text;
		subText = subtext;
	}
}
