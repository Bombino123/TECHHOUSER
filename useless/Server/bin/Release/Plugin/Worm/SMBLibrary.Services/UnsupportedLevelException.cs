using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class UnsupportedLevelException : Exception
{
	private uint m_level;

	public uint Level => m_level;

	public UnsupportedLevelException(uint level)
	{
		m_level = level;
	}
}
