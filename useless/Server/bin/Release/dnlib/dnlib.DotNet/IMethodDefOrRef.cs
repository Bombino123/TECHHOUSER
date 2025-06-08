namespace dnlib.DotNet;

public interface IMethodDefOrRef : ICodedToken, IMDTokenProvider, IHasCustomAttribute, ICustomAttributeType, IMethod, ITokenOperand, IFullName, IGenericParameterProvider, IIsTypeOrMethod, IMemberRef, IOwnerModule
{
	int MethodDefOrRefTag { get; }
}
