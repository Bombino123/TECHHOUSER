using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct3D11;

[Guid("9eb576dd-9f77-4d86-81aa-8bab5fe490e2")]
public class Predicate : Query
{
	public Predicate(Device device, QueryDescription description)
		: base(IntPtr.Zero)
	{
		device.CreatePredicate(description, this);
	}

	public Predicate(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Predicate(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new Predicate(nativePtr);
		}
		return null;
	}
}
