using System.Runtime.InteropServices;

namespace SMBLibrary.Client.Authentication;

[ComVisible(true)]
public interface IAuthenticationClient
{
	byte[] InitializeSecurityContext(byte[] securityBlob);

	byte[] GetSessionKey();
}
