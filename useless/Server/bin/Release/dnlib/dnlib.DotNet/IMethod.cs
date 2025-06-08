namespace dnlib.DotNet;

public interface IMethod : ICodedToken, IMDTokenProvider, ITokenOperand, IFullName, IGenericParameterProvider, IIsTypeOrMethod, IMemberRef, IOwnerModule
{
	MethodSig MethodSig { get; set; }
}
