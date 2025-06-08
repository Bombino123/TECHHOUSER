namespace dnlib.DotNet;

public interface IMemberForwarded : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName, IMemberRef, IOwnerModule, IIsTypeOrMethod
{
	int MemberForwardedTag { get; }

	ImplMap ImplMap { get; set; }

	bool HasImplMap { get; }
}
