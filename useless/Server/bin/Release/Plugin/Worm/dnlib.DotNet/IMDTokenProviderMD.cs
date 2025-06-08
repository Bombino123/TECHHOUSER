using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IMDTokenProviderMD : IMDTokenProvider
{
	uint OrigRid { get; }
}
