using System;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace AntdUI.Svg;

public struct SvgUnit
{
	private SvgUnitType _type;

	private float _value;

	private bool _isEmpty;

	private float? _deviceValue;

	public static readonly SvgUnit Empty = new SvgUnit(SvgUnitType.User, 0f)
	{
		_isEmpty = true
	};

	public static readonly SvgUnit None = new SvgUnit(SvgUnitType.None, 0f);

	public bool IsEmpty => _isEmpty;

	public bool IsNone => _type == SvgUnitType.None;

	public float Value => _value;

	public SvgUnitType Type => _type;

	public float ToDeviceValue(ISvgRenderer renderer, UnitRenderingType renderType, SvgElement owner)
	{
		if (_deviceValue.HasValue)
		{
			return _deviceValue.Value;
		}
		if (_value == 0f)
		{
			_deviceValue = 0f;
			return _deviceValue.Value;
		}
		int ppi = SvgDocument.Ppi;
		SvgUnitType type = Type;
		float value = Value;
		switch (type)
		{
		case SvgUnitType.Em:
		{
			using (IFontDefn fontDefn2 = GetFont(renderer, owner))
			{
				if (fontDefn2 == null)
				{
					float num = value * 9f;
					_deviceValue = num / 72f * (float)ppi;
				}
				else
				{
					_deviceValue = value * (fontDefn2.SizeInPoints / 72f) * (float)ppi;
				}
			}
			break;
		}
		case SvgUnitType.Ex:
		{
			using (IFontDefn fontDefn = GetFont(renderer, owner))
			{
				if (fontDefn == null)
				{
					float num = value * 9f;
					_deviceValue = num * 0.5f / 72f * (float)ppi;
				}
				else
				{
					_deviceValue = value * 0.5f * (fontDefn.SizeInPoints / 72f) * (float)ppi;
				}
			}
			break;
		}
		case SvgUnitType.Centimeter:
			_deviceValue = value / 2.54f * (float)ppi;
			break;
		case SvgUnitType.Inch:
			_deviceValue = value * (float)ppi;
			break;
		case SvgUnitType.Millimeter:
			_deviceValue = value / 10f / 2.54f * (float)ppi;
			break;
		case SvgUnitType.Pica:
			_deviceValue = value * 12f / 72f * (float)ppi;
			break;
		case SvgUnitType.Point:
			_deviceValue = value / 72f * (float)ppi;
			break;
		case SvgUnitType.Pixel:
			_deviceValue = value;
			break;
		case SvgUnitType.User:
			_deviceValue = value;
			break;
		case SvgUnitType.Percentage:
		{
			ISvgBoundable svgBoundable;
			if (renderer != null)
			{
				svgBoundable = renderer.GetBoundable();
			}
			else
			{
				ISvgBoundable svgBoundable2 = owner?.OwnerDocument;
				svgBoundable = svgBoundable2;
			}
			ISvgBoundable svgBoundable3 = svgBoundable;
			if (svgBoundable3 == null)
			{
				_deviceValue = value;
				break;
			}
			SizeF size = svgBoundable3.Bounds.Size;
			switch (renderType)
			{
			case UnitRenderingType.Horizontal:
				_deviceValue = size.Width / 100f * value;
				break;
			case UnitRenderingType.HorizontalOffset:
				_deviceValue = size.Width / 100f * value + svgBoundable3.Location.X;
				break;
			case UnitRenderingType.Vertical:
				_deviceValue = size.Height / 100f * value;
				break;
			case UnitRenderingType.VerticalOffset:
				_deviceValue = size.Height / 100f * value + svgBoundable3.Location.Y;
				break;
			case UnitRenderingType.Other:
				if (owner.OwnerDocument != null)
				{
					_ = owner.OwnerDocument.ViewBox;
					if (owner.OwnerDocument.ViewBox.Width != 0f && owner.OwnerDocument.ViewBox.Height != 0f)
					{
						_deviceValue = (float)(Math.Sqrt(Math.Pow(owner.OwnerDocument.ViewBox.Width, 2.0) + Math.Pow(owner.OwnerDocument.ViewBox.Height, 2.0)) / Math.Sqrt(2.0) * (double)value / 100.0);
						break;
					}
				}
				_deviceValue = (float)(Math.Sqrt(Math.Pow(size.Width, 2.0) + Math.Pow(size.Height, 2.0)) / Math.Sqrt(2.0) * (double)value / 100.0);
				break;
			}
			break;
		}
		default:
			_deviceValue = value;
			break;
		}
		return _deviceValue.Value;
	}

	private IFontDefn GetFont(ISvgRenderer renderer, SvgElement owner)
	{
		return owner?.Parents.OfType<SvgVisualElement>().FirstOrDefault()?.GetFont(renderer);
	}

	public SvgUnit ToPercentage()
	{
		return Type switch
		{
			SvgUnitType.Percentage => this, 
			SvgUnitType.User => new SvgUnit(SvgUnitType.Percentage, Value * 100f), 
			_ => throw new NotImplementedException(), 
		};
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj.GetType() == typeof(SvgUnit)))
		{
			return false;
		}
		SvgUnit svgUnit = (SvgUnit)obj;
		if (svgUnit.Value == Value)
		{
			return svgUnit.Type == Type;
		}
		return false;
	}

	public bool Equals(SvgUnit other)
	{
		if (_type == other._type)
		{
			return _value == other._value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return 0 + 1000000007 * _type.GetHashCode() + 1000000009 * _value.GetHashCode() + 1000000021 * _isEmpty.GetHashCode() + 1000000033 * _deviceValue.GetHashCode();
	}

	public static bool operator ==(SvgUnit lhs, SvgUnit rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(SvgUnit lhs, SvgUnit rhs)
	{
		return !(lhs == rhs);
	}

	public override string ToString()
	{
		string text = string.Empty;
		switch (Type)
		{
		case SvgUnitType.None:
			return "none";
		case SvgUnitType.Pixel:
			text = "px";
			break;
		case SvgUnitType.Point:
			text = "pt";
			break;
		case SvgUnitType.Inch:
			text = "in";
			break;
		case SvgUnitType.Centimeter:
			text = "cm";
			break;
		case SvgUnitType.Millimeter:
			text = "mm";
			break;
		case SvgUnitType.Percentage:
			text = "%";
			break;
		case SvgUnitType.Em:
			text = "em";
			break;
		}
		return Value.ToString(CultureInfo.InvariantCulture) + text;
	}

	public static implicit operator float(SvgUnit value)
	{
		return value.ToDeviceValue(null, UnitRenderingType.Other, null);
	}

	public static implicit operator SvgUnit(float value)
	{
		return new SvgUnit(value);
	}

	public SvgUnit(SvgUnitType type, float value)
	{
		_isEmpty = false;
		_type = type;
		_value = value;
		_deviceValue = null;
	}

	public SvgUnit(float value)
	{
		_isEmpty = false;
		_value = value;
		_type = SvgUnitType.User;
		_deviceValue = null;
	}

	public static PointF GetDevicePoint(SvgUnit x, SvgUnit y, ISvgRenderer renderer, SvgElement owner)
	{
		return new PointF(x.ToDeviceValue(renderer, UnitRenderingType.Horizontal, owner), y.ToDeviceValue(renderer, UnitRenderingType.Vertical, owner));
	}

	public static PointF GetDevicePointOffset(SvgUnit x, SvgUnit y, ISvgRenderer renderer, SvgElement owner)
	{
		return new PointF(x.ToDeviceValue(renderer, UnitRenderingType.HorizontalOffset, owner), y.ToDeviceValue(renderer, UnitRenderingType.VerticalOffset, owner));
	}

	public static SizeF GetDeviceSize(SvgUnit width, SvgUnit height, ISvgRenderer renderer, SvgElement owner)
	{
		return new SizeF(width.ToDeviceValue(renderer, UnitRenderingType.HorizontalOffset, owner), height.ToDeviceValue(renderer, UnitRenderingType.VerticalOffset, owner));
	}
}
