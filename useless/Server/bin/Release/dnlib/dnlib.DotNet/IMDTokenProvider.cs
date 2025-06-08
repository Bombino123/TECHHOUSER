namespace dnlib.DotNet;

public interface IMDTokenProvider
{
	MDToken MDToken { get; }

	uint Rid { get; set; }
}
