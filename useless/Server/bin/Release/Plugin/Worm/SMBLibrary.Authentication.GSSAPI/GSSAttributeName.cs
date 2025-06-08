using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication.GSSAPI;

[ComVisible(true)]
public enum GSSAttributeName
{
	AccessToken,
	DomainName,
	IsAnonymous,
	IsGuest,
	MachineName,
	OSVersion,
	SessionKey,
	UserName
}
