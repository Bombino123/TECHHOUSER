using System;
using System.Collections.Generic;

namespace AntdUI.Svg;

public sealed class SvgAttributeCollection : Dictionary<string, object>
{
	private SvgElement _owner;

	public new object this[string attributeName]
	{
		get
		{
			return GetInheritedAttribute<object>(attributeName);
		}
		set
		{
			if (ContainsKey(attributeName))
			{
				object a = base[attributeName];
				if (TryUnboxedCheck(a, value))
				{
					base[attributeName] = value;
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

	public SvgAttributeCollection(SvgElement owner)
	{
		_owner = owner;
	}

	public TAttributeType GetAttribute<TAttributeType>(string attributeName)
	{
		if (ContainsKey(attributeName) && base[attributeName] != null)
		{
			return (TAttributeType)base[attributeName];
		}
		return GetAttribute(attributeName, default(TAttributeType));
	}

	public T GetAttribute<T>(string attributeName, T defaultValue)
	{
		if (ContainsKey(attributeName) && base[attributeName] != null)
		{
			return (T)base[attributeName];
		}
		return defaultValue;
	}

	public TAttributeType GetInheritedAttribute<TAttributeType>(string attributeName)
	{
		if (ContainsKey(attributeName) && !IsInheritValue(base[attributeName]))
		{
			TAttributeType obj = (TAttributeType)base[attributeName];
			if (obj is SvgDeferredPaintServer svgDeferredPaintServer)
			{
				svgDeferredPaintServer.EnsureServer(_owner);
			}
			return obj;
		}
		if (_owner.Parent != null)
		{
			object obj2 = _owner.Parent.Attributes[attributeName];
			if (obj2 != null)
			{
				return (TAttributeType)obj2;
			}
		}
		return default(TAttributeType);
	}

	private bool IsInheritValue(object value)
	{
		if (value != null && (!(value is SvgFontWeight) || (SvgFontWeight)value != 0) && (!(value is SvgTextAnchor) || (SvgTextAnchor)value != 0) && (!(value is SvgFontVariant) || (SvgFontVariant)value != SvgFontVariant.Inherit) && (!(value is SvgTextDecoration) || (SvgTextDecoration)value != 0) && (!(value is XmlSpaceHandling) || (XmlSpaceHandling)value != XmlSpaceHandling.inherit) && (!(value is SvgOverflow) || (SvgOverflow)value != SvgOverflow.Inherit) && (!(value is SvgColourServer) || (SvgColourServer)value != SvgColourServer.Inherit) && (!(value is SvgShapeRendering) || (SvgShapeRendering)value != 0) && (!(value is SvgTextRendering) || (SvgTextRendering)value != 0) && (!(value is SvgImageRendering) || (SvgImageRendering)value != 0))
		{
			if (value is string)
			{
				return ((string)value).ToLower() == "inherit";
			}
			return false;
		}
		return true;
	}

	private bool TryUnboxedCheck(object a, object b)
	{
		if (IsValueType(a))
		{
			if (a is SvgUnit)
			{
				return UnboxAndCheck<SvgUnit>(a, b);
			}
			if (a is bool)
			{
				return UnboxAndCheck<bool>(a, b);
			}
			if (a is int)
			{
				return UnboxAndCheck<int>(a, b);
			}
			if (a is float)
			{
				return UnboxAndCheck<float>(a, b);
			}
			if (a is SvgViewBox)
			{
				return UnboxAndCheck<SvgViewBox>(a, b);
			}
			return true;
		}
		return a != b;
	}

	private bool UnboxAndCheck<T>(object a, object b)
	{
		return !((T)a).Equals((T)b);
	}

	private bool IsValueType(object obj)
	{
		return obj?.GetType().IsValueType ?? false;
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
