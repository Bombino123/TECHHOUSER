using System.Data.Common;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Data.SQLite;

[Serializable]
public sealed class SQLiteException : DbException, ISerializable
{
	private const int FACILITY_SQLITE = 1967;

	private SQLiteErrorCode _errorCode;

	public SQLiteErrorCode ResultCode => _errorCode;

	public override int ErrorCode => (int)_errorCode;

	private SQLiteException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_errorCode = (SQLiteErrorCode)info.GetInt32("errorCode");
		Initialize();
	}

	public SQLiteException(SQLiteErrorCode errorCode, string message)
		: base(GetStockErrorMessage(errorCode, message))
	{
		_errorCode = errorCode;
		Initialize();
	}

	public SQLiteException(string message)
		: this(SQLiteErrorCode.Unknown, message)
	{
	}

	public SQLiteException()
	{
		Initialize();
	}

	public SQLiteException(string message, Exception innerException)
		: base(message, innerException)
	{
		Initialize();
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info?.AddValue("errorCode", _errorCode);
		base.GetObjectData(info, context);
	}

	private void Initialize()
	{
		if (base.HResult == -2147467259)
		{
			int? hResultForErrorCode = GetHResultForErrorCode(ResultCode);
			if (hResultForErrorCode.HasValue)
			{
				base.HResult = hResultForErrorCode.Value;
			}
		}
	}

	private static int MakeHResult(int errorCode, bool success)
	{
		return (errorCode & 0xFFFF) | 0x7AF | ((!success) ? int.MinValue : 0);
	}

	private static int? GetHResultForErrorCode(SQLiteErrorCode errorCode)
	{
		switch (errorCode & SQLiteErrorCode.NonExtendedMask)
		{
		case SQLiteErrorCode.Ok:
			return 0;
		case SQLiteErrorCode.Error:
			return MakeHResult(31, success: false);
		case SQLiteErrorCode.Internal:
			return -2147418113;
		case SQLiteErrorCode.Perm:
			return MakeHResult(5, success: false);
		case SQLiteErrorCode.Abort:
			return -2147467260;
		case SQLiteErrorCode.Busy:
			return MakeHResult(170, success: false);
		case SQLiteErrorCode.Locked:
			return MakeHResult(212, success: false);
		case SQLiteErrorCode.NoMem:
			return MakeHResult(14, success: false);
		case SQLiteErrorCode.ReadOnly:
			return MakeHResult(6009, success: false);
		case SQLiteErrorCode.Interrupt:
			return MakeHResult(1223, success: false);
		case SQLiteErrorCode.IoErr:
			return MakeHResult(1117, success: false);
		case SQLiteErrorCode.Corrupt:
			return MakeHResult(1358, success: false);
		case SQLiteErrorCode.NotFound:
			return MakeHResult(50, success: false);
		case SQLiteErrorCode.Full:
			return MakeHResult(112, success: false);
		case SQLiteErrorCode.CantOpen:
			return MakeHResult(1011, success: false);
		case SQLiteErrorCode.Protocol:
			return MakeHResult(1460, success: false);
		case SQLiteErrorCode.Empty:
			return MakeHResult(4306, success: false);
		case SQLiteErrorCode.Schema:
			return MakeHResult(1931, success: false);
		case SQLiteErrorCode.TooBig:
			return -2147317563;
		case SQLiteErrorCode.Constraint:
			return MakeHResult(8239, success: false);
		case SQLiteErrorCode.Mismatch:
			return MakeHResult(1629, success: false);
		case SQLiteErrorCode.Misuse:
			return MakeHResult(1609, success: false);
		case SQLiteErrorCode.NoLfs:
			return MakeHResult(1606, success: false);
		case SQLiteErrorCode.Auth:
			return MakeHResult(1935, success: false);
		case SQLiteErrorCode.Format:
			return MakeHResult(11, success: false);
		case SQLiteErrorCode.Range:
			return -2147316575;
		case SQLiteErrorCode.NotADb:
			return MakeHResult(1392, success: false);
		case SQLiteErrorCode.Notice:
		case SQLiteErrorCode.Warning:
		case SQLiteErrorCode.Row:
		case SQLiteErrorCode.Done:
			return MakeHResult((int)errorCode, success: true);
		default:
			return null;
		}
	}

	private static string GetErrorString(SQLiteErrorCode errorCode)
	{
		BindingFlags invokeAttr = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
		return typeof(SQLite3).InvokeMember("GetErrorString", invokeAttr, null, null, new object[1] { errorCode }) as string;
	}

	private static string GetStockErrorMessage(SQLiteErrorCode errorCode, string message)
	{
		return HelperMethods.StringFormat(CultureInfo.CurrentCulture, "{0}{1}{2}", GetErrorString(errorCode), Environment.NewLine, message).Trim();
	}

	public override string ToString()
	{
		return HelperMethods.StringFormat(CultureInfo.CurrentCulture, "code = {0} ({1}), message = {2}", _errorCode, (int)_errorCode, base.ToString());
	}
}
