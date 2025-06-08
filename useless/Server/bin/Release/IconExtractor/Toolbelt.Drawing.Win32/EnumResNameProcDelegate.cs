using System;

namespace Toolbelt.Drawing.Win32;

internal delegate bool EnumResNameProcDelegate(IntPtr hModule, RT lpszType, IntPtr lpszName, IntPtr lParam);
