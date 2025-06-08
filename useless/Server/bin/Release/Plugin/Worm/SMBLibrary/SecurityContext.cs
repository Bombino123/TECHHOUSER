using System.Net;
using System.Runtime.InteropServices;
using SMBLibrary.Authentication.GSSAPI;

namespace SMBLibrary;

[ComVisible(true)]
public class SecurityContext
{
	private string m_userName;

	private string m_machineName;

	private IPEndPoint m_clientEndPoint;

	public GSSContext AuthenticationContext;

	public object AccessToken;

	public string UserName => m_userName;

	public string MachineName => m_machineName;

	public IPEndPoint ClientEndPoint => m_clientEndPoint;

	public SecurityContext(string userName, string machineName, IPEndPoint clientEndPoint, GSSContext authenticationContext, object accessToken)
	{
		m_userName = userName;
		m_machineName = machineName;
		m_clientEndPoint = clientEndPoint;
		AuthenticationContext = authenticationContext;
		AccessToken = accessToken;
	}
}
