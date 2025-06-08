using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public enum NetBiosSuffix : byte
{
	WorkstationService = 0,
	MessengerService = 3,
	DomainMasterBrowser = 27,
	MasterBrowser = 29,
	BrowserServiceElections = 30,
	FileServiceService = 32
}
