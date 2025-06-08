using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace SMBLibrary.Client;

[ComVisible(true)]
public interface ISMBClient
{
	uint MaxReadSize { get; }

	uint MaxWriteSize { get; }

	bool IsConnected { get; }

	bool Connect(string serverName, SMBTransportType transport);

	bool Connect(IPAddress serverAddress, SMBTransportType transport);

	void Disconnect();

	NTStatus Login(string domainName, string userName, string password);

	NTStatus Login(string domainName, string userName, string password, AuthenticationMethod authenticationMethod);

	NTStatus Logoff();

	List<string> ListShares(out NTStatus status);

	ISMBFileStore TreeConnect(string shareName, out NTStatus status);
}
