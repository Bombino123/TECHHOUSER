using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IMethodDefOrRef : ICodedToken, IMDTokenProvider, IHasCustomAttribute, ICustomAttributeType, IMethod, ITokenOperand, IFullName, IGenericParameterProvider, IIsTypeOrMethod, IMemberRef, IOwnerModule
{
	int MethodDefOrRefTag { get; }
}
