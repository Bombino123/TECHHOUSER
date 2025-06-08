using System;
using System.Runtime.InteropServices;

namespace GMap.NET.Internals;

public sealed class FastReaderWriterLock : IDisposable
{
	private static class NativeMethods
	{
		[DllImport("Kernel32", ExactSpelling = true)]
		internal static extern void AcquireSRWLockExclusive(ref IntPtr srw);

		[DllImport("Kernel32", ExactSpelling = true)]
		internal static extern void AcquireSRWLockShared(ref IntPtr srw);

		[DllImport("Kernel32", ExactSpelling = true)]
		internal static extern void InitializeSRWLock(out IntPtr srw);

		[DllImport("Kernel32", ExactSpelling = true)]
		internal static extern void ReleaseSRWLockExclusive(ref IntPtr srw);

		[DllImport("Kernel32", ExactSpelling = true)]
		internal static extern void ReleaseSRWLockShared(ref IntPtr srw);
	}

	private IntPtr _lockSRW = IntPtr.Zero;

	private FastResourceLock _pLock;

	private static readonly bool UseNativeSRWLock = Stuff.IsRunningOnVistaOrLater() && IntPtr.Size == 4;

	public FastReaderWriterLock()
	{
		if (UseNativeSRWLock)
		{
			NativeMethods.InitializeSRWLock(out _lockSRW);
		}
		else
		{
			_pLock = new FastResourceLock();
		}
	}

	~FastReaderWriterLock()
	{
		Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (_pLock != null)
		{
			_pLock.Dispose();
			_pLock = null;
		}
	}

	public void AcquireReaderLock()
	{
		if (UseNativeSRWLock)
		{
			NativeMethods.AcquireSRWLockShared(ref _lockSRW);
		}
		else
		{
			_pLock.AcquireShared();
		}
	}

	public void ReleaseReaderLock()
	{
		if (UseNativeSRWLock)
		{
			NativeMethods.ReleaseSRWLockShared(ref _lockSRW);
		}
		else
		{
			_pLock.ReleaseShared();
		}
	}

	public void AcquireWriterLock()
	{
		try
		{
			if (UseNativeSRWLock)
			{
				NativeMethods.AcquireSRWLockExclusive(ref _lockSRW);
			}
			else
			{
				_pLock.AcquireExclusive();
			}
		}
		catch (Exception)
		{
		}
	}

	public void ReleaseWriterLock()
	{
		try
		{
			if (UseNativeSRWLock)
			{
				NativeMethods.ReleaseSRWLockExclusive(ref _lockSRW);
			}
			else
			{
				_pLock.ReleaseExclusive();
			}
		}
		catch (Exception)
		{
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
