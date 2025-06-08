namespace SharpDX.Win32;

public class ErrorCodeHelper
{
	public static Result ToResult(ErrorCode errorCode)
	{
		return ToResult((int)errorCode);
	}

	public static Result ToResult(int errorCode)
	{
		return new Result((errorCode <= 0) ? ((uint)errorCode) : (((uint)errorCode & 0xFFFFu) | 0x80070000u));
	}
}
