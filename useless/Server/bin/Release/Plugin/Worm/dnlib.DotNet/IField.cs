using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IField : ICodedToken, IMDTokenProvider, ITokenOperand, IFullName, IMemberRef, IOwnerModule, IIsTypeOrMethod
{
	FieldSig FieldSig { get; set; }
}
