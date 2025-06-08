using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using dnlib.IO;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class DataReaderIStream : IStream, IDisposable
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

	private readonly DataReaderFactory dataReaderFactory;

	private DataReader reader;

	private readonly string name;

	private const int STG_E_INVALIDFUNCTION = -2147287039;

	private const int STG_E_CANTSAVE = -2147286781;

	public DataReaderIStream(DataReaderFactory dataReaderFactory)
		: this(dataReaderFactory, dataReaderFactory.CreateReader(), string.Empty)
	{
	}

	private DataReaderIStream(DataReaderFactory dataReaderFactory, DataReader reader, string name)
	{
		this.dataReaderFactory = dataReaderFactory ?? throw new ArgumentNullException("dataReaderFactory");
		this.reader = reader;
		this.name = name ?? string.Empty;
	}

	public void Clone(out IStream ppstm)
	{
		ppstm = new DataReaderIStream(dataReaderFactory, reader, name);
	}

	public void Commit(int grfCommitFlags)
	{
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
		if ((ulong)((long)reader.Position + (long)(uint)num) > (ulong)reader.Length)
		{
			num = (int)(reader.Length - Math.Min(reader.Position, reader.Length));
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
		cb = (int)Math.Min(reader.BytesLeft, (uint)cb);
		reader.ReadBytes(pv, 0, cb);
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
			reader.Position = (uint)dlibMove;
			break;
		case STREAM_SEEK.CUR:
			reader.Position = (uint)(reader.Position + dlibMove);
			break;
		case STREAM_SEEK.END:
			reader.Position = (uint)(reader.Length + dlibMove);
			break;
		}
		if (plibNewPosition != IntPtr.Zero)
		{
			Marshal.WriteInt64(plibNewPosition, reader.Position);
		}
	}

	public void SetSize(long libNewSize)
	{
		Marshal.ThrowExceptionForHR(-2147287039);
	}

	public void Stat(out STATSTG pstatstg, int grfStatFlag)
	{
		STATSTG sTATSTG = default(STATSTG);
		sTATSTG.cbSize = reader.Length;
		sTATSTG.clsid = Guid.Empty;
		sTATSTG.grfLocksSupported = 0;
		sTATSTG.grfMode = 0;
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
		Marshal.ThrowExceptionForHR(-2147286781);
	}

	public void Dispose()
	{
	}
}
