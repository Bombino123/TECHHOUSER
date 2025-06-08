using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface ITypeOrMethodDef : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IHasDeclSecurity, IFullName, IMemberRefParent, IMemberRef, IOwnerModule, IIsTypeOrMethod, IGenericParameterProvider
{
	int TypeOrMethodDefTag { get; }

	IList<GenericParam> GenericParameters { get; }

	bool HasGenericParameters { get; }
}
