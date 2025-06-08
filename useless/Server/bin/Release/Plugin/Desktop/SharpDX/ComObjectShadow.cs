using System;
using System.Runtime.InteropServices;

namespace SharpDX;

internal abstract class ComObjectShadow : CppObjectShadow
{
	internal class ComObjectVtbl : CppObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int QueryInterfaceDelegate(IntPtr thisObject, IntPtr guid, out IntPtr output);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int AddRefDelegate(IntPtr thisObject);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate int ReleaseDelegate(IntPtr thisObject);

		public ComObjectVtbl(int numberOfCallbackMethods)
			: base(numberOfCallbackMethods + 3)
		{
			AddMethod(new QueryInterfaceDelegate(QueryInterfaceImpl));
			AddMethod(new AddRefDelegate(AddRefImpl));
			AddMethod(new ReleaseDelegate(ReleaseImpl));
		}

		protected unsafe static int QueryInterfaceImpl(IntPtr thisObject, IntPtr guid, out IntPtr output)
		{
			ComObjectShadow comObjectShadow = CppObjectShadow.ToShadow<ComObjectShadow>(thisObject);
			if (comObjectShadow == null)
			{
				output = IntPtr.Zero;
				return Result.NoInterface.Code;
			}
			return comObjectShadow.QueryInterfaceImpl(ref *(Guid*)(void*)guid, out output);
		}

		protected static int AddRefImpl(IntPtr thisObject)
		{
			return CppObjectShadow.ToShadow<ComObjectShadow>(thisObject)?.AddRefImpl() ?? 0;
		}

		protected static int ReleaseImpl(IntPtr thisObject)
		{
			return CppObjectShadow.ToShadow<ComObjectShadow>(thisObject)?.ReleaseImpl() ?? 0;
		}
	}

	public static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

	protected int QueryInterfaceImpl(ref Guid guid, out IntPtr output)
	{
		ComObjectShadow comObjectShadow = (ComObjectShadow)((ShadowContainer)base.Callback.Shadow).FindShadow(guid);
		if (comObjectShadow != null)
		{
			((IUnknown)base.Callback).AddReference();
			output = comObjectShadow.NativePointer;
			return Result.Ok.Code;
		}
		output = IntPtr.Zero;
		return Result.NoInterface.Code;
	}

	protected virtual int AddRefImpl()
	{
		return ((IUnknown)base.Callback).AddReference();
	}

	protected virtual int ReleaseImpl()
	{
		return ((IUnknown)base.Callback).Release();
	}
}
