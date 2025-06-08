namespace dnlib.DotNet;

public interface ITypeDefOrRef : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IMemberRefParent, IFullName, IType, IOwnerModule, IGenericParameterProvider, IIsTypeOrMethod, IContainsGenericParameter, ITokenOperand, IMemberRef
{
	int TypeDefOrRefTag { get; }
}
