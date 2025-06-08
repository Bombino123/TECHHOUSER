using System;
using System.Drawing;

namespace Vanara.PInvoke;

public struct RECT : IEquatable<PRECT>, IEquatable<RECT>, IEquatable<Rectangle>
{
	public int left;

	public int top;

	public int right;

	public int bottom;

	public static readonly RECT Empty;

	public int X
	{
		get
		{
			return left;
		}
		set
		{
			right -= left - value;
			left = value;
		}
	}

	public int Y
	{
		get
		{
			return top;
		}
		set
		{
			bottom -= top - value;
			top = value;
		}
	}

	public int Height
	{
		get
		{
			return bottom - top;
		}
		set
		{
			bottom = value + top;
		}
	}

	public int Width
	{
		get
		{
			return right - left;
		}
		set
		{
			right = value + left;
		}
	}

	public Point Location
	{
		get
		{
			return new Point(left, top);
		}
		set
		{
			X = value.X;
			Y = value.Y;
		}
	}

	public Size Size
	{
		get
		{
			return new Size(Width, Height);
		}
		set
		{
			Width = value.Width;
			Height = value.Height;
		}
	}

	public bool IsEmpty
	{
		get
		{
			if (left == 0 && top == 0 && right == 0)
			{
				return bottom == 0;
			}
			return false;
		}
	}

	public RECT(int left, int top, int right, int bottom)
	{
		this.left = left;
		this.top = top;
		this.right = right;
		this.bottom = bottom;
	}

	public RECT(Rectangle r)
		: this(r.Left, r.Top, r.Right, r.Bottom)
	{
	}

	public static implicit operator Rectangle(RECT r)
	{
		return new Rectangle(r.left, r.top, r.Width, r.Height);
	}

	public static implicit operator RECT(Rectangle r)
	{
		return new RECT(r);
	}

	public static bool operator ==(RECT r1, RECT r2)
	{
		return r1.Equals(r2);
	}

	public static bool operator !=(RECT r1, RECT r2)
	{
		return !r1.Equals(r2);
	}

	public bool Equals(RECT r)
	{
		if (r.left == left && r.top == top && r.right == right)
		{
			return r.bottom == bottom;
		}
		return false;
	}

	public bool Equals(PRECT? r)
	{
		RECT? rECT = r?.rect;
		return Equals(rECT.HasValue ? ((PRECT)rECT.GetValueOrDefault()) : null);
	}

	public bool Equals(Rectangle r)
	{
		if (r.Left == left && r.Top == top && r.Right == right)
		{
			return r.Bottom == bottom;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj != null)
		{
			if (!(obj is RECT r))
			{
				if (!(obj is PRECT r2))
				{
					if (obj is Rectangle r3)
					{
						return Equals(r3);
					}
					return false;
				}
				return Equals(r2);
			}
			return Equals(r);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((Rectangle)this).GetHashCode();
	}

	public override string ToString()
	{
		return $"{{left={left},top={top},right={right},bottom={bottom}}}";
	}
}
