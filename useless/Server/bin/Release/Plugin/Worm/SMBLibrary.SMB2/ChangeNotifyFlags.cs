using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[Flags]
[ComVisible(true)]
public enum ChangeNotifyFlags : ushort
{
	WatchTree = 1
}
