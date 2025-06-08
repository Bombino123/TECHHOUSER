using System.Runtime.InteropServices;

namespace SMBLibrary.NetBios;

[ComVisible(true)]
public enum NameServiceOperation : byte
{
	QueryRequest = 0,
	RegistrationRequest = 5,
	ReleaseRequest = 6,
	WackRequest = 7,
	RefreshRequest = 8,
	QueryResponse = 16,
	RegistrationResponse = 21,
	ReleaseResponse = 22,
	WackResponse = 23,
	RefreshResponse = 24
}
