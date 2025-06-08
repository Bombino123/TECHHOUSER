using System.Runtime.InteropServices;

namespace SMBLibrary.SMB2;

[ComVisible(true)]
public enum SMB2Dialect : ushort
{
	SMB202 = 514,
	SMB210 = 528,
	SMB300 = 768,
	SMB302 = 770,
	SMB311 = 785,
	SMB2xx = 767
}
