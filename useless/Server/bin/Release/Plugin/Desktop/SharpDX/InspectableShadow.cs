using System;
using System.Runtime.InteropServices;

namespace SharpDX;

internal class InspectableShadow : ComObjectShadow
{
	public class InspectableProviderVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private unsafe delegate int GetIidsDelegate(IntPtr thisPtr, int* iidCount, IntPtr* iids);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetRuntimeClassNameDelegate(IntPtr thisPtr, IntPtr className);

		private enum TrustLevel
		{
			BaseTrust,
			PartialTrust,
			FullTrust
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetTrustLevelDelegate(IntPtr thisPtr, IntPtr trustLevel);

		public InspectableProviderVtbl()
			: base(3)
		{
			AddMethod(new GetIidsDelegate(GetIids));
			AddMethod(new GetRuntimeClassNameDelegate(GetRuntimeClassName));
			AddMethod(new GetTrustLevelDelegate(GetTrustLevel));
		}

		private unsafe static int GetIids(IntPtr thisPtr, int* iidCount, IntPtr* iids)
		{
			try
			{
				ShadowContainer shadowContainer = (ShadowContainer)((IInspectable)CppObjectShadow.ToShadow<InspectableShadow>(thisPtr).Callback).Shadow;
				int num = shadowContainer.Guids.Length;
				iids = (IntPtr*)(void*)Marshal.AllocCoTaskMem(IntPtr.Size * num);
				*iidCount = num;
				for (int i = 0; i < num; i++)
				{
					iids[i] = shadowContainer.Guids[i];
				}
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int GetRuntimeClassName(IntPtr thisPtr, IntPtr className)
		{
			try
			{
				_ = (IInspectable)CppObjectShadow.ToShadow<InspectableShadow>(thisPtr).Callback;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int GetTrustLevel(IntPtr thisPtr, IntPtr trustLevel)
		{
			try
			{
				_ = (IInspectable)CppObjectShadow.ToShadow<InspectableShadow>(thisPtr).Callback;
				Marshal.WriteInt32(trustLevel, 2);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly InspectableProviderVtbl Vtbl = new InspectableProviderVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(IInspectable callback)
	{
		return CppObject.ToCallbackPtr<IInspectable>(callback);
	}
}
