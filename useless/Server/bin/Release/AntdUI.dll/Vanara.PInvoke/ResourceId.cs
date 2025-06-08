using System;
using System.Runtime.InteropServices;

namespace Vanara.PInvoke;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
[PInvokeData("winuser.h")]
public struct ResourceId : IEquatable<string>, IEquatable<IntPtr>, IEquatable<int>, IEquatable<ResourceId>, IHandle
{
	private IntPtr ptr;

	public static readonly ResourceId NULL;

	public int id
	{
		get
		{
			if (!Macros.IS_INTRESOURCE(ptr))
			{
				return 0;
			}
			return (ushort)ptr.ToInt32();
		}
		set
		{
			if (value > 65535 || value <= 0)
			{
				throw new ArgumentOutOfRangeException("id");
			}
			ptr = (IntPtr)(ushort)value;
		}
	}

	public bool IsIntResource => Macros.IS_INTRESOURCE(ptr);

	public static implicit operator int(ResourceId r)
	{
		return r.id;
	}

	public static implicit operator IntPtr(ResourceId r)
	{
		return r.ptr;
	}

	public static implicit operator ResourceId(int resId)
	{
		ResourceId result = default(ResourceId);
		result.id = resId;
		return result;
	}

	public static implicit operator ResourceId(IntPtr p)
	{
		ResourceId result = default(ResourceId);
		result.ptr = p;
		return result;
	}

	public static explicit operator string(ResourceId r)
	{
		return r.ToString();
	}

	public override bool Equals(object obj)
	{
		if (obj != null)
		{
			if (!(obj is string other))
			{
				if (!(obj is int other2))
				{
					if (!(obj is IntPtr other3))
					{
						if (!(obj is ResourceId other4))
						{
							if (obj is IHandle handle)
							{
								return Equals(handle.DangerousGetHandle());
							}
							if (!obj.GetType().IsPrimitive)
							{
								return false;
							}
							try
							{
								return Equals(Convert.ToInt32(obj));
							}
							catch
							{
								return false;
							}
						}
						return Equals(other4);
					}
					return Equals(other3);
				}
				return Equals(other2);
			}
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ptr.GetHashCode();
	}

	public override string? ToString()
	{
		if (!Macros.IS_INTRESOURCE(ptr))
		{
			return Marshal.PtrToStringAuto(ptr);
		}
		return $"#{ptr.ToInt32()}";
	}

	public bool Equals(int other)
	{
		return ptr.ToInt32().Equals(other);
	}

	public bool Equals(string? other)
	{
		return string.Equals(ToString(), other);
	}

	public bool Equals(IntPtr other)
	{
		return ptr.Equals((object?)(nint)other);
	}

	public bool Equals(ResourceId other)
	{
		return string.Equals(other.ToString(), ToString());
	}

	public IntPtr DangerousGetHandle()
	{
		return ptr;
	}
}
