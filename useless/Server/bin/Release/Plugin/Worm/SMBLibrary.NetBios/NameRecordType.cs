using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public enum NameRecordType : ushort
{
	NB = 32,
	NBStat
}
