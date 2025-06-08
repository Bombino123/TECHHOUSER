using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SharpDX;

public struct Result : IEquatable<Result>
{
	private int _code;

	public static readonly Result Ok = new Result(0);

	public static readonly Result False = new Result(1);

	public static readonly ResultDescriptor Abort = new ResultDescriptor(-2147467260, "General", "E_ABORT", "Operation aborted");

	public static readonly ResultDescriptor AccessDenied = new ResultDescriptor(-2147024891, "General", "E_ACCESSDENIED", "General access denied error");

	public static readonly ResultDescriptor Fail = new ResultDescriptor(-2147467259, "General", "E_FAIL", "Unspecified error");

	public static readonly ResultDescriptor Handle = new ResultDescriptor(-2147024890, "General", "E_HANDLE", "Invalid handle");

	public static readonly ResultDescriptor InvalidArg = new ResultDescriptor(-2147024809, "General", "E_INVALIDARG", "Invalid Arguments");

	public static readonly ResultDescriptor NoInterface = new ResultDescriptor(-2147467262, "General", "E_NOINTERFACE", "No such interface supported");

	public static readonly ResultDescriptor NotImplemented = new ResultDescriptor(-2147467263, "General", "E_NOTIMPL", "Not implemented");

	public static readonly ResultDescriptor OutOfMemory = new ResultDescriptor(-2147024882, "General", "E_OUTOFMEMORY", "Out of memory");

	public static readonly ResultDescriptor InvalidPointer = new ResultDescriptor(-2147467261, "General", "E_POINTER", "Invalid pointer");

	public static readonly ResultDescriptor UnexpectedFailure = new ResultDescriptor(-2147418113, "General", "E_UNEXPECTED", "Catastrophic failure");

	public static readonly ResultDescriptor WaitAbandoned = new ResultDescriptor(128, "General", "WAIT_ABANDONED", "WaitAbandoned");

	public static readonly ResultDescriptor WaitTimeout = new ResultDescriptor(258, "General", "WAIT_TIMEOUT", "WaitTimeout");

	public static readonly ResultDescriptor Pending = new ResultDescriptor(-2147483638, "General", "E_PENDING", "Pending");

	public int Code => _code;

	public bool Success => Code >= 0;

	public bool Failure => Code < 0;

	public Result(int code)
	{
		_code = code;
	}

	public Result(uint code)
	{
		_code = (int)code;
	}

	public static explicit operator int(Result result)
	{
		return result.Code;
	}

	public static explicit operator uint(Result result)
	{
		return (uint)result.Code;
	}

	public static implicit operator Result(int result)
	{
		return new Result(result);
	}

	public static implicit operator Result(uint result)
	{
		return new Result(result);
	}

	public bool Equals(Result other)
	{
		return Code == other.Code;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Result))
		{
			return false;
		}
		return Equals((Result)obj);
	}

	public override int GetHashCode()
	{
		return Code;
	}

	public static bool operator ==(Result left, Result right)
	{
		return left.Code == right.Code;
	}

	public static bool operator !=(Result left, Result right)
	{
		return left.Code != right.Code;
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "HRESULT = 0x{0:X}", new object[1] { _code });
	}

	public void CheckError()
	{
		if (_code < 0)
		{
			throw new SharpDXException(this);
		}
	}

	public static Result GetResultFromException(Exception ex)
	{
		return new Result(Marshal.GetHRForException(ex));
	}

	public static Result GetResultFromWin32Error(int win32Error)
	{
		return (int)((win32Error <= 0) ? win32Error : ((win32Error & 0xFFFF) | 0x70000 | 0x80000000u));
	}
}
