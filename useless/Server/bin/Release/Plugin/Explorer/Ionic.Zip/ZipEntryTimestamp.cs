using System;
using System.Runtime.InteropServices;

namespace Ionic.Zip;

[Flags]
[ComVisible(true)]
public enum ZipEntryTimestamp
{
	None = 0,
	DOS = 1,
	Windows = 2,
	Unix = 4,
	InfoZip1 = 8
}
