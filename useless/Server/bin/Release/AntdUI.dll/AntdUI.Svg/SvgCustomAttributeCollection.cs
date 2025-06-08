using System;
using System.Collections.Generic;

namespace AntdUI.Svg;

public sealed class SvgCustomAttributeCollection : Dictionary<string, string>
{
	private SvgElement _owner;

	public new string this[string attributeName]
	{
		get
		{
			return base[attributeName];
		}
		set
		{
			if (ContainsKey(attributeName))
			{
				string text = base[attributeName];
				base[attributeName] = value;
				if (text != value)
				{
					OnAttributeChanged(attributeName, value);
				}
			}
			else
			{
				base[attributeName] = value;
				OnAttributeChanged(attributeName, value);
			}
		}
	}

	public event EventHandler<AttributeEventArgs> AttributeChanged;

	public SvgCustomAttributeCollection(SvgElement owner)
	{
		_owner = owner;
	}

	private void OnAttributeChanged(string attribute, object value)
	{
		this.AttributeChanged?.Invoke(_owner, new AttributeEventArgs
		{
			Attribute = attribute,
			Value = value
		});
	}
}
