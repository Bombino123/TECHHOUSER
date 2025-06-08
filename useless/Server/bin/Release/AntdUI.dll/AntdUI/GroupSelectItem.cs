using System.Collections.Generic;

namespace AntdUI;

public class GroupSelectItem : ISelectItem
{
	private string _title;

	public string Title
	{
		get
		{
			return Localization.GetLangIN(LocalizationTitle, _title);
		}
		set
		{
			_title = value;
		}
	}

	public string? LocalizationTitle { get; set; }

	public IList<object>? Sub { get; set; }

	public object? Tag { get; set; }

	public GroupSelectItem(string title)
	{
		_title = title;
	}

	public override string ToString()
	{
		return Title;
	}
}
