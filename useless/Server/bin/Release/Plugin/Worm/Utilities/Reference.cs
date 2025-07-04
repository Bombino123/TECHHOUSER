using System.Runtime.InteropServices;

namespace Utilities;

[ComVisible(true)]
public class Reference<T> where T : struct
{
	private T m_value;

	public T Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
		}
	}

	public Reference(T value)
	{
		m_value = value;
	}

	public override string ToString()
	{
		return m_value.ToString();
	}

	public static implicit operator T(Reference<T> wrapper)
	{
		return wrapper.Value;
	}

	public static implicit operator Reference<T>(T value)
	{
		return new Reference<T>(value);
	}
}
