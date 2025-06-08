namespace dnlib.DotNet;

public interface IField : ICodedToken, IMDTokenProvider, ITokenOperand, IFullName, IMemberRef, IOwnerModule, IIsTypeOrMethod
{
	FieldSig FieldSig { get; set; }
}
