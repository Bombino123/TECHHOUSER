using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IHasDeclSecurity : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int HasDeclSecurityTag { get; }

	IList<DeclSecurity> DeclSecurities { get; }

	bool HasDeclSecurities { get; }
}
