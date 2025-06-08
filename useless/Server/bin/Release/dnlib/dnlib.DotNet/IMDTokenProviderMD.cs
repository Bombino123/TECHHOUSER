namespace dnlib.DotNet;

public interface IMDTokenProviderMD : IMDTokenProvider
{
	uint OrigRid { get; }
}
