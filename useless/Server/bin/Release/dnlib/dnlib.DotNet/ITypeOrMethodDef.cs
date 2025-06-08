using System.Collections.Generic;

namespace dnlib.DotNet;

public interface ITypeOrMethodDef : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IHasDeclSecurity, IFullName, IMemberRefParent, IMemberRef, IOwnerModule, IIsTypeOrMethod, IGenericParameterProvider
{
	int TypeOrMethodDefTag { get; }

	IList<GenericParam> GenericParameters { get; }

	bool HasGenericParameters { get; }
}
