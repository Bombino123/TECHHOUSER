using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Data.SQLite;

internal sealed class SQLiteStreamAdapter : IDisposable
{
	private Stream stream;

	private SQLiteConnectionFlags flags;

	private UnsafeNativeMethods.xSessionInput xInput;

	private UnsafeNativeMethods.xSessionOutput xOutput;

	private bool disposed;

	public SQLiteStreamAdapter(Stream stream, SQLiteConnectionFlags flags)
	{
		this.stream = stream;
		this.flags = flags;
	}

	private SQLiteConnectionFlags GetFlags()
	{
		return flags;
	}

	public UnsafeNativeMethods.xSessionInput GetInputDelegate()
	{
		CheckDisposed();
		if (xInput == null)
		{
			xInput = Input;
		}
		return xInput;
	}

	public UnsafeNativeMethods.xSessionOutput GetOutputDelegate()
	{
		CheckDisposed();
		if (xOutput == null)
		{
			xOutput = Output;
		}
		return xOutput;
	}

	private SQLiteErrorCode Input(IntPtr context, IntPtr pData, ref int nData)
	{
		try
		{
			Stream stream = this.stream;
			if (stream == null)
			{
				return SQLiteErrorCode.Misuse;
			}
			if (nData > 0)
			{
				byte[] array = new byte[nData];
				int num = stream.Read(array, 0, nData);
				if (num > 0 && pData != IntPtr.Zero)
				{
					Marshal.Copy(array, 0, pData, num);
				}
				nData = num;
			}
			return SQLiteErrorCode.Ok;
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(GetFlags()))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "xSessionInput", ex));
				}
			}
			catch
			{
			}
		}
		return SQLiteErrorCode.IoErr_Read;
	}

	private SQLiteErrorCode Output(IntPtr context, IntPtr pData, int nData)
	{
		try
		{
			Stream stream = this.stream;
			if (stream == null)
			{
				return SQLiteErrorCode.Misuse;
			}
			if (nData > 0)
			{
				byte[] array = new byte[nData];
				if (pData != IntPtr.Zero)
				{
					Marshal.Copy(pData, array, 0, nData);
				}
				stream.Write(array, 0, nData);
			}
			stream.Flush();
			return SQLiteErrorCode.Ok;
		}
		catch (Exception ex)
		{
			try
			{
				if (HelperMethods.LogCallbackExceptions(GetFlags()))
				{
					SQLiteLog.LogMessage(-2146233088, HelperMethods.StringFormat(CultureInfo.CurrentCulture, "Caught exception in \"{0}\" method: {1}", "xSessionOutput", ex));
				}
			}
			catch
			{
			}
		}
		return SQLiteErrorCode.IoErr_Write;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void CheckDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(SQLiteStreamAdapter).Name);
		}
	}

	private void Dispose(bool disposing)
	{
		try
		{
			if (!disposed && disposing)
			{
				if (xInput != null)
				{
					xInput = null;
				}
				if (xOutput != null)
				{
					xOutput = null;
				}
				if (stream != null)
				{
					stream = null;
				}
			}
		}
		finally
		{
			disposed = true;
		}
	}

	~SQLiteStreamAdapter()
	{
		Dispose(disposing: false);
	}
}
