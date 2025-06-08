using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface ITypeDefOrRef : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IMemberRefParent, IFullName, IType, IOwnerModule, IGenericParameterProvider, IIsTypeOrMethod, IContainsGenericParameter, ITokenOperand, IMemberRef
{
	int TypeDefOrRefTag { get; }
}
