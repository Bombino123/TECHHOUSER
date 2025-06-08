using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IMDTokenProvider
{
	MDToken MDToken { get; }

	uint Rid { get; set; }
}
