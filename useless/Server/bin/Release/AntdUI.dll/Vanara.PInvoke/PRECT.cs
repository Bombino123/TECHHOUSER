using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Vanara.PInvoke;

[StructLayout(LayoutKind.Sequential)]
public class PRECT : IEquatable<PRECT>, IEquatable<RECT>, IEquatable<Rectangle>
{
	internal RECT rect;

	public int left
	{
		get
		{
			return rect.left;
		}
		set
		{
			rect.left = value;
		}
	}

	public int top
	{
		get
		{
			return rect.top;
		}
		set
		{
			rect.top = value;
		}
	}

	public int right
	{
		get
		{
			return rect.right;
		}
		set
		{
			rect.right = value;
		}
	}

	public int bottom
	{
		get
		{
			return rect.bottom;
		}
		set
		{
			rect.bottom = value;
		}
	}

	public int X
	{
		get
		{
			return rect.X;
		}
		set
		{
			rect.X = value;
		}
	}

	public int Y
	{
		get
		{
			return rect.Y;
		}
		set
		{
			rect.Y = value;
		}
	}

	public int Height
	{
		get
		{
			return rect.Height;
		}
		set
		{
			rect.Height = value;
		}
	}

	public int Width
	{
		get
		{
			return rect.Width;
		}
		set
		{
			rect.Width = value;
		}
	}

	public Point Location
	{
		get
		{
			return rect.Location;
		}
		set
		{
			rect.Location = value;
		}
	}

	public Size Size
	{
		get
		{
			return rect.Size;
		}
		set
		{
			rect.Size = value;
		}
	}

	public bool IsEmpty => rect.IsEmpty;

	public PRECT()
	{
	}

	public PRECT(int left, int top, int right, int bottom)
	{
		rect = new RECT(left, top, right, bottom);
	}

	public PRECT(Rectangle r)
	{
		rect = new RECT(r);
	}

	[ExcludeFromCodeCoverage]
	private PRECT(RECT r)
	{
		rect = r;
	}

	public static implicit operator Rectangle(PRECT r)
	{
		return r.rect;
	}

	public static implicit operator PRECT(Rectangle? r)
	{
		if (!r.HasValue)
		{
			return null;
		}
		return new PRECT(r.Value);
	}

	public static implicit operator PRECT(Rectangle r)
	{
		return new PRECT(r);
	}

	public static implicit operator PRECT(RECT r)
	{
		return new PRECT(r);
	}

	public static bool operator ==(PRECT r1, PRECT r2)
	{
		if ((object)r1 == r2)
		{
			return true;
		}
		if ((object)r1 == null || (object)r2 == null)
		{
			return false;
		}
		return r1.Equals(r2);
	}

	public static bool operator !=(PRECT r1, PRECT r2)
	{
		return !(r1 == r2);
	}

	public bool Equals(PRECT? r)
	{
		RECT value = rect;
		RECT? obj = r?.rect;
		return value == obj;
	}

	public bool Equals(RECT r)
	{
		return rect.Equals(r);
	}

	public bool Equals(Rectangle r)
	{
		return rect.Equals(r);
	}

	public override bool Equals(object obj)
	{
		return rect.Equals(obj);
	}

	public override int GetHashCode()
	{
		return rect.GetHashCode();
	}

	public override string ToString()
	{
		return rect.ToString();
	}
}
