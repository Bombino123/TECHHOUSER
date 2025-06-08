using System;
using System.Runtime.InteropServices;

namespace Vestris.ResourceLib;

public class ResourceId
{
	private IntPtr _name = IntPtr.Zero;

	public IntPtr Id
	{
		get
		{
			return _name;
		}
		set
		{
			_name = (IsIntResource(value) ? value : Marshal.StringToHGlobalUni(Marshal.PtrToStringUni(value)));
		}
	}

	public string TypeName
	{
		get
		{
			if (!IsIntResource())
			{
				return Name;
			}
			return ResourceType.ToString();
		}
	}

	public Kernel32.ResourceTypes ResourceType
	{
		get
		{
			if (IsIntResource())
			{
				return (Kernel32.ResourceTypes)(int)_name;
			}
			throw new InvalidCastException($"Resource {Name} is not of built-in type.");
		}
		set
		{
			_name = (IntPtr)(int)value;
		}
	}

	public string Name
	{
		get
		{
			if (!IsIntResource())
			{
				return Marshal.PtrToStringUni(_name);
			}
			return _name.ToString();
		}
		set
		{
			_name = Marshal.StringToHGlobalUni(value);
		}
	}

	public ResourceId(IntPtr value)
	{
		Id = value;
	}

	public ResourceId(uint value)
	{
		Id = new IntPtr(value);
	}

	public ResourceId(Kernel32.ResourceTypes value)
	{
		Id = (IntPtr)(int)value;
	}

	public ResourceId(string value)
	{
		Name = value;
	}

	public bool IsIntResource()
	{
		return IsIntResource(_name);
	}

	internal static bool IsIntResource(IntPtr value)
	{
		return value.ToInt64() <= 65535;
	}

	public override string ToString()
	{
		return Name;
	}

	public override int GetHashCode()
	{
		if (!IsIntResource())
		{
			return Name.GetHashCode();
		}
		return Id.ToInt32();
	}

	public override bool Equals(object obj)
	{
		if (obj is ResourceId && obj == this)
		{
			return true;
		}
		if (obj is ResourceId && (obj as ResourceId).GetHashCode() == GetHashCode())
		{
			return true;
		}
		return false;
	}
}
