namespace dnlib.DotNet;

public interface IHasSemantic : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName, IMemberRef, IOwnerModule, IIsTypeOrMethod
{
	int HasSemanticTag { get; }
}
