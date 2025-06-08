using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IHasSemantic : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName, IMemberRef, IOwnerModule, IIsTypeOrMethod
{
	int HasSemanticTag { get; }
}
