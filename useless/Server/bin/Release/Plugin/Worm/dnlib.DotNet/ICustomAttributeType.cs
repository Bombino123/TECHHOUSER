using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface ICustomAttributeType : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IMethod, ITokenOperand, IFullName, IGenericParameterProvider, IIsTypeOrMethod, IMemberRef, IOwnerModule
{
	int CustomAttributeTypeTag { get; }
}
