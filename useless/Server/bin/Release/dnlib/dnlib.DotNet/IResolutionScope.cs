namespace dnlib.DotNet;

public interface IResolutionScope : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int ResolutionScopeTag { get; }
}
