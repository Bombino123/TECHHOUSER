using System;
using System.Threading;

namespace SharpDX;

public abstract class CallbackBase : DisposeBase, ICallbackable, IDisposable
{
	private int refCount = 1;

	IDisposable ICallbackable.Shadow { get; set; }

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Release();
		}
	}

	public int AddReference()
	{
		int num = refCount;
		while (true)
		{
			if (num == 0)
			{
				throw new ObjectDisposedException("Cannot add a reference to a nonreferenced item");
			}
			int num2 = Interlocked.CompareExchange(ref refCount, num + 1, num);
			if (num2 == num)
			{
				break;
			}
			num = num2;
		}
		return num + 1;
	}

	public int Release()
	{
		int num = refCount;
		while (true)
		{
			int num2 = Interlocked.CompareExchange(ref refCount, num - 1, num);
			if (num2 == num)
			{
				break;
			}
			num = num2;
		}
		if (num == 1)
		{
			((ICallbackable)this).Shadow.Dispose();
			((ICallbackable)this).Shadow = null;
		}
		return num - 1;
	}

	public Result QueryInterface(ref Guid guid, out IntPtr comObject)
	{
		ShadowContainer shadowContainer = (ShadowContainer)((ICallbackable)this).Shadow;
		comObject = shadowContainer.Find(guid);
		if (comObject == IntPtr.Zero)
		{
			return Result.NoInterface;
		}
		return Result.Ok;
	}
}
