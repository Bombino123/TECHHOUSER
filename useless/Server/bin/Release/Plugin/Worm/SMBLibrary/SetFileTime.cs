using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SMBLibrary;

[ComVisible(true)]
public struct SetFileTime
{
	public bool MustNotChange;

	private DateTime? m_time;

	public DateTime? Time
	{
		get
		{
			if (MustNotChange)
			{
				return null;
			}
			return m_time;
		}
		set
		{
			MustNotChange = false;
			m_time = value;
		}
	}

	public SetFileTime(bool mustNotChange)
	{
		MustNotChange = mustNotChange;
		m_time = null;
	}

	public SetFileTime(DateTime? time)
	{
		MustNotChange = false;
		m_time = time;
	}

	public long ToFileTimeUtc()
	{
		if (MustNotChange)
		{
			return -1L;
		}
		if (!m_time.HasValue)
		{
			return 0L;
		}
		return Time.Value.ToFileTimeUtc();
	}

	public static SetFileTime FromFileTimeUtc(long span)
	{
		if (span > 0)
		{
			return new SetFileTime(DateTime.FromFileTimeUtc(span));
		}
		return span switch
		{
			0L => new SetFileTime(mustNotChange: false), 
			-1L => new SetFileTime(mustNotChange: true), 
			_ => throw new InvalidDataException("Set FILETIME cannot be less than -1"), 
		};
	}

	public static implicit operator DateTime?(SetFileTime setTime)
	{
		return setTime.Time;
	}

	public static implicit operator SetFileTime(DateTime? time)
	{
		return new SetFileTime(time);
	}
}
