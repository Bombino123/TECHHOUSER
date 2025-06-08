namespace dnlib.DotNet;

public interface IMemberRefParent : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int MemberRefParentTag { get; }
}
