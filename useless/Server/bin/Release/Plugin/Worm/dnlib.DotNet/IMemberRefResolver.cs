using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IMemberRefResolver
{
	IMemberForwarded Resolve(MemberRef memberRef);
}
