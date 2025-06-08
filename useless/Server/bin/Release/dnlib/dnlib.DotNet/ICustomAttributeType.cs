namespace dnlib.DotNet;

public interface ICustomAttributeType : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IMethod, ITokenOperand, IFullName, IGenericParameterProvider, IIsTypeOrMethod, IMemberRef, IOwnerModule
{
	int CustomAttributeTypeTag { get; }
}
