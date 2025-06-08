using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public class DeclSecurityUser : DeclSecurity
{
	public DeclSecurityUser()
	{
	}

	public DeclSecurityUser(SecurityAction action, IList<SecurityAttribute> securityAttrs)
	{
		base.action = action;
		securityAttributes = securityAttrs;
	}

	public override byte[] GetBlob()
	{
		return null;
	}
}
