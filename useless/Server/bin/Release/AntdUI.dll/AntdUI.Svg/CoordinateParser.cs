using System.Globalization;

namespace AntdUI.Svg;

internal class CoordinateParser
{
	private enum NumState
	{
		invalid,
		separator,
		prefix,
		integer,
		decPlace,
		fraction,
		exponent,
		expPrefix,
		expValue
	}

	private string _coords;

	private int _pos;

	private NumState _currState = NumState.separator;

	private NumState _newState = NumState.separator;

	private int i;

	private bool _parseWorked = true;

	public int Position => _pos;

	public bool HasMore => _parseWorked;

	public CoordinateParser(string coords)
	{
		_coords = coords;
		if (string.IsNullOrEmpty(_coords))
		{
			_parseWorked = false;
		}
		if (char.IsLetter(coords[0]))
		{
			i++;
		}
	}

	private bool MarkState(bool state)
	{
		_parseWorked = state;
		i++;
		return state;
	}

	public bool TryGetBool(out bool result)
	{
		while (i < _coords.Length && _parseWorked)
		{
			if (_currState == NumState.separator)
			{
				if (IsCoordSeparator(_coords[i]))
				{
					_newState = NumState.separator;
					i++;
					continue;
				}
				if (_coords[i] == '0')
				{
					result = false;
					_newState = NumState.separator;
					_pos = i + 1;
					return MarkState(state: true);
				}
				if (_coords[i] == '1')
				{
					result = true;
					_newState = NumState.separator;
					_pos = i + 1;
					return MarkState(state: true);
				}
				result = false;
				return MarkState(state: false);
			}
			result = false;
			return MarkState(state: false);
		}
		result = false;
		return MarkState(state: false);
	}

	public bool TryGetFloat(out float result)
	{
		while (i < _coords.Length && _parseWorked)
		{
			switch (_currState)
			{
			case NumState.separator:
				if (char.IsNumber(_coords[i]))
				{
					_newState = NumState.integer;
					break;
				}
				if (IsCoordSeparator(_coords[i]))
				{
					_newState = NumState.separator;
					break;
				}
				switch (_coords[i])
				{
				case '.':
					_newState = NumState.decPlace;
					break;
				case '+':
				case '-':
					_newState = NumState.prefix;
					break;
				default:
					_newState = NumState.invalid;
					break;
				}
				break;
			case NumState.prefix:
				if (char.IsNumber(_coords[i]))
				{
					_newState = NumState.integer;
				}
				else if (_coords[i] == '.')
				{
					_newState = NumState.decPlace;
				}
				else
				{
					_newState = NumState.invalid;
				}
				break;
			case NumState.integer:
				if (char.IsNumber(_coords[i]))
				{
					_newState = NumState.integer;
					break;
				}
				if (IsCoordSeparator(_coords[i]))
				{
					_newState = NumState.separator;
					break;
				}
				switch (_coords[i])
				{
				case '.':
					_newState = NumState.decPlace;
					break;
				case 'E':
				case 'e':
					_newState = NumState.exponent;
					break;
				case '+':
				case '-':
					_newState = NumState.prefix;
					break;
				default:
					_newState = NumState.invalid;
					break;
				}
				break;
			case NumState.decPlace:
				if (char.IsNumber(_coords[i]))
				{
					_newState = NumState.fraction;
					break;
				}
				if (IsCoordSeparator(_coords[i]))
				{
					_newState = NumState.separator;
					break;
				}
				switch (_coords[i])
				{
				case 'E':
				case 'e':
					_newState = NumState.exponent;
					break;
				case '+':
				case '-':
					_newState = NumState.prefix;
					break;
				default:
					_newState = NumState.invalid;
					break;
				}
				break;
			case NumState.fraction:
				if (char.IsNumber(_coords[i]))
				{
					_newState = NumState.fraction;
					break;
				}
				if (IsCoordSeparator(_coords[i]))
				{
					_newState = NumState.separator;
					break;
				}
				switch (_coords[i])
				{
				case '.':
					_newState = NumState.decPlace;
					break;
				case 'E':
				case 'e':
					_newState = NumState.exponent;
					break;
				case '+':
				case '-':
					_newState = NumState.prefix;
					break;
				default:
					_newState = NumState.invalid;
					break;
				}
				break;
			case NumState.exponent:
			{
				if (char.IsNumber(_coords[i]))
				{
					_newState = NumState.expValue;
					break;
				}
				if (IsCoordSeparator(_coords[i]))
				{
					_newState = NumState.invalid;
					break;
				}
				char c = _coords[i];
				if (c == '+' || c == '-')
				{
					_newState = NumState.expPrefix;
				}
				else
				{
					_newState = NumState.invalid;
				}
				break;
			}
			case NumState.expPrefix:
				if (char.IsNumber(_coords[i]))
				{
					_newState = NumState.expValue;
				}
				else
				{
					_newState = NumState.invalid;
				}
				break;
			case NumState.expValue:
				if (char.IsNumber(_coords[i]))
				{
					_newState = NumState.expValue;
					break;
				}
				if (IsCoordSeparator(_coords[i]))
				{
					_newState = NumState.separator;
					break;
				}
				switch (_coords[i])
				{
				case '.':
					_newState = NumState.decPlace;
					break;
				case '+':
				case '-':
					_newState = NumState.prefix;
					break;
				default:
					_newState = NumState.invalid;
					break;
				}
				break;
			}
			if (_newState < _currState)
			{
				result = float.Parse(_coords.Substring(_pos, i - _pos), NumberStyles.Float, CultureInfo.InvariantCulture);
				_pos = i;
				_currState = _newState;
				return MarkState(state: true);
			}
			if (_newState != _currState && _currState == NumState.separator)
			{
				_pos = i;
			}
			if (_newState == NumState.invalid)
			{
				result = float.MinValue;
				return MarkState(state: false);
			}
			_currState = _newState;
			i++;
		}
		if (_currState == NumState.separator || !_parseWorked || _pos >= _coords.Length)
		{
			result = float.MinValue;
			return MarkState(state: false);
		}
		result = float.Parse(_coords.Substring(_pos, _coords.Length - _pos), NumberStyles.Float, CultureInfo.InvariantCulture);
		_pos = _coords.Length;
		return MarkState(state: true);
	}

	private static bool IsCoordSeparator(char value)
	{
		switch (value)
		{
		case '\t':
		case '\n':
		case '\r':
		case ' ':
		case ',':
			return true;
		default:
			return false;
		}
	}
}
