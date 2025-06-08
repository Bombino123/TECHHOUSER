using System.Collections.Generic;

namespace dnlib.DotNet;

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
