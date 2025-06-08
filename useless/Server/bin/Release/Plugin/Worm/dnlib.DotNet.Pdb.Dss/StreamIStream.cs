using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class StreamIStream : IStream
{
	private enum STREAM_SEEK
	{
		SET,
		CUR,
		END
	}

	private enum STATFLAG
	{
		DEFAULT,
		NONAME,
		NOOPEN
	}

	private enum STGTY
	{
		STORAGE = 1,
		STREAM,
		LOCKBYTES,
		PROPERTY
	}

	private readonly Stream stream;

	private readonly string name;

	private const int STG_E_INVALIDFUNCTION = -2147287039;

	public StreamIStream(Stream stream)
		: this(stream, string.Empty)
	{
	}

	public StreamIStream(Stream stream, string name)
	{
		this.stream = stream ?? throw new ArgumentNullException("stream");
		this.name = name ?? string.Empty;
	}

	public void Clone(out IStream ppstm)
	{
		Marshal.ThrowExceptionForHR(-2147287039);
		throw new Exception();
	}

	public void Commit(int grfCommitFlags)
	{
		stream.Flush();
	}

	public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
	{
		if (cb > int.MaxValue)
		{
			cb = 2147483647L;
		}
		else if (cb < 0)
		{
			cb = 0L;
		}
		int num = (int)cb;
		if (stream.Position + num < num || stream.Position + num > stream.Length)
		{
			num = (int)(stream.Length - Math.Min(stream.Position, stream.Length));
		}
		byte[] array = new byte[num];
		Read(array, num, pcbRead);
		if (pcbRead != IntPtr.Zero)
		{
			Marshal.WriteInt64(pcbRead, Marshal.ReadInt32(pcbRead));
		}
		pstm.Write(array, array.Length, pcbWritten);
		if (pcbWritten != IntPtr.Zero)
		{
			Marshal.WriteInt64(pcbWritten, Marshal.ReadInt32(pcbWritten));
		}
	}

	public void LockRegion(long libOffset, long cb, int dwLockType)
	{
		Marshal.ThrowExceptionForHR(-2147287039);
	}

	public void Read(byte[] pv, int cb, IntPtr pcbRead)
	{
		if (cb < 0)
		{
			cb = 0;
		}
		cb = stream.Read(pv, 0, cb);
		if (pcbRead != IntPtr.Zero)
		{
			Marshal.WriteInt32(pcbRead, cb);
		}
	}

	public void Revert()
	{
	}

	public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
	{
		switch ((STREAM_SEEK)dwOrigin)
		{
		case STREAM_SEEK.SET:
			stream.Position = dlibMove;
			break;
		case STREAM_SEEK.CUR:
			stream.Position += dlibMove;
			break;
		case STREAM_SEEK.END:
			stream.Position = stream.Length + dlibMove;
			break;
		}
		if (plibNewPosition != IntPtr.Zero)
		{
			Marshal.WriteInt64(plibNewPosition, stream.Position);
		}
	}

	public void SetSize(long libNewSize)
	{
		stream.SetLength(libNewSize);
	}

	public void Stat(out STATSTG pstatstg, int grfStatFlag)
	{
		STATSTG sTATSTG = default(STATSTG);
		sTATSTG.cbSize = stream.Length;
		sTATSTG.clsid = Guid.Empty;
		sTATSTG.grfLocksSupported = 0;
		sTATSTG.grfMode = 2;
		sTATSTG.grfStateBits = 0;
		if ((grfStatFlag & 1) == 0)
		{
			sTATSTG.pwcsName = name;
		}
		sTATSTG.reserved = 0;
		sTATSTG.type = 2;
		pstatstg = sTATSTG;
	}

	public void UnlockRegion(long libOffset, long cb, int dwLockType)
	{
		Marshal.ThrowExceptionForHR(-2147287039);
	}

	public void Write(byte[] pv, int cb, IntPtr pcbWritten)
	{
		stream.Write(pv, 0, cb);
		if (pcbWritten != IntPtr.Zero)
		{
			Marshal.WriteInt32(pcbWritten, cb);
		}
	}
}
