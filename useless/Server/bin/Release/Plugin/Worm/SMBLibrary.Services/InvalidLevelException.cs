using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.Services;

[ComVisible(true)]
public class InvalidLevelException : Exception
{
	private uint m_level;

	public uint Level => m_level;

	public InvalidLevelException(uint level)
	{
		m_level = level;
	}
}
