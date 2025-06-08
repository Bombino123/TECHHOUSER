using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpDX.Win32;

[Guid("0000000c-0000-0000-C000-000000000046")]
public class ComStream : ComStreamBase, IStream, IStreamBase, IUnknown, ICallbackable, IDisposable
{
	public ComStream(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator ComStream(IntPtr nativePtr)
	{
		if (!(nativePtr == IntPtr.Zero))
		{
			return new ComStream(nativePtr);
		}
		return null;
	}

	public unsafe long Seek(long dlibMove, SeekOrigin dwOrigin)
	{
		long result = default(long);
		((Result)((delegate* unmanaged[Stdcall]<void*, long, uint, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)5 * (nint)sizeof(void*))))(_nativePointer, dlibMove, (uint)dwOrigin, &result)).CheckError();
		return result;
	}

	public unsafe void SetSize(long libNewSize)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, long, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)6 * (nint)sizeof(void*))))(_nativePointer, libNewSize)).CheckError();
	}

	public unsafe long CopyTo(IStream stmRef, long cb, out long cbWrittenRef)
	{
		IntPtr zero = IntPtr.Zero;
		zero = CppObject.ToCallbackPtr<IStream>(stmRef);
		Result result2;
		long result = default(long);
		fixed (long* ptr = &cbWrittenRef)
		{
			void* ptr2 = ptr;
			result2 = ((delegate* unmanaged[Stdcall]<void*, void*, long, void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)7 * (nint)sizeof(void*))))(_nativePointer, (void*)zero, cb, &result, ptr2);
		}
		result2.CheckError();
		return result;
	}

	public unsafe void Commit(CommitFlags grfCommitFlags)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)8 * (nint)sizeof(void*))))(_nativePointer, (int)grfCommitFlags)).CheckError();
	}

	public unsafe void Revert()
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)9 * (nint)sizeof(void*))))(_nativePointer)).CheckError();
	}

	public unsafe void LockRegion(long libOffset, long cb, LockType dwLockType)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, long, long, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)10 * (nint)sizeof(void*))))(_nativePointer, libOffset, cb, (int)dwLockType)).CheckError();
	}

	public unsafe void UnlockRegion(long libOffset, long cb, LockType dwLockType)
	{
		((Result)((delegate* unmanaged[Stdcall]<void*, long, long, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)11 * (nint)sizeof(void*))))(_nativePointer, libOffset, cb, (int)dwLockType)).CheckError();
	}

	public unsafe StorageStatistics GetStatistics(StorageStatisticsFlags grfStatFlag)
	{
		StorageStatistics.__Native @ref = default(StorageStatistics.__Native);
		StorageStatistics result = default(StorageStatistics);
		Result result2 = ((delegate* unmanaged[Stdcall]<void*, void*, int, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)12 * (nint)sizeof(void*))))(_nativePointer, &@ref, (int)grfStatFlag);
		result.__MarshalFrom(ref @ref);
		result2.CheckError();
		return result;
	}

	public unsafe IStream Clone()
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)13 * (nint)sizeof(void*))))(_nativePointer, &zero);
		IStream result2 = ((!(zero != IntPtr.Zero)) ? null : new ComStream(zero));
		result.CheckError();
		return result2;
	}
}
