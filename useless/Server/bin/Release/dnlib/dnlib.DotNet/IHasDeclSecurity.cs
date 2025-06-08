using System.Collections.Generic;

namespace dnlib.DotNet;

public interface IHasDeclSecurity : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int HasDeclSecurityTag { get; }

	IList<DeclSecurity> DeclSecurities { get; }

	bool HasDeclSecurities { get; }
}
