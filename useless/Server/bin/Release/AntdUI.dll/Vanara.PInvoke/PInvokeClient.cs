using System;

namespace Vanara.PInvoke;

[Flags]
public enum PInvokeClient
{
	None = 0,
	Windows2000 = 1,
	WindowsXP = 3,
	WindowsXP_SP2 = 7,
	WindowsVista = 0xF,
	WindowsVista_SP2 = 0x1F,
	Windows7 = 0x3F,
	Windows8 = 0x7F,
	Windows81 = 0xFF,
	Windows10 = 0x1FF,
	Windows11 = 0x2FF
}
