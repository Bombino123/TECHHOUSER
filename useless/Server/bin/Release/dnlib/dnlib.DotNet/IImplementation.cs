namespace dnlib.DotNet;

public interface IImplementation : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int ImplementationTag { get; }
}
