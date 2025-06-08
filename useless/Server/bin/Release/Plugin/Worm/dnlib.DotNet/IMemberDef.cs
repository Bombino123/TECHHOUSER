using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IMemberDef : IDnlibDef, ICodedToken, IMDTokenProvider, IFullName, IHasCustomAttribute, IMemberRef, IOwnerModule, IIsTypeOrMethod
{
	new TypeDef DeclaringType { get; }
}
