using System.Collections.Generic;
using System.Drawing;

namespace AntdUI;

public class SelectItem : ISelectItem
{
	private string _text;

	private string? subText;

	public int? Online { get; set; }

	public Color? OnlineCustom { get; set; }

	public Image? Icon { get; set; }

	public string? IconSvg { get; set; }

	public string Text
	{
		get
		{
			return Localization.GetLangIN(LocalizationText, _text, new string[2]
			{
				"{id}",
				Tag.ToString()
			});
		}
		set
		{
			_text = value;
		}
	}

	public string? LocalizationText { get; set; }

	public bool Enable { get; set; } = true;


	public string? SubText
	{
		get
		{
			return Localization.GetLangI(LocalizationSubText, subText, new string[2]
			{
				"{id}",
				Tag.ToString()
			});
		}
		set
		{
			subText = value;
		}
	}

	public string? LocalizationSubText { get; set; }

	public IList<object>? Sub { get; set; }

	public object Tag { get; set; }

	public Color? TagFore { get; set; }

	public Color? TagBack { get; set; }

	public string? TagBackExtend { get; set; }

	public SelectItem(int online, Image? ico, string text, object tag)
		: this(text, tag)
	{
		Online = online;
		Icon = ico;
	}

	public SelectItem(int online, Image? ico, object tag)
		: this(tag)
	{
		Online = online;
		Icon = ico;
	}

	public SelectItem(Image? ico, string text, object tag)
		: this(text, tag)
	{
		Icon = ico;
	}

	public SelectItem(Image? ico, string text)
		: this(text)
	{
		Icon = ico;
	}

	public SelectItem(Image? ico, object tag)
		: this(tag)
	{
		Icon = ico;
	}

	public SelectItem(int online, string text, object tag)
		: this(text, tag)
	{
		Online = online;
	}

	public SelectItem(int online, object tag)
		: this(tag)
	{
		Online = online;
	}

	public SelectItem(object tag)
	{
		_text = tag.ToString() ?? string.Empty;
		Tag = tag;
	}

	public SelectItem(string text, object tag)
	{
		_text = text;
		Tag = tag;
	}

	public override string ToString()
	{
		return Text;
	}
}
