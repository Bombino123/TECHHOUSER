using System;
using System.Runtime.InteropServices;

namespace SMBLibrary.SMB1;

[Flags]
[ComVisible(true)]
public enum SessionSetupAction : ushort
{
	SetupGuest = 1,
	UseLanmanKey = 2
}
