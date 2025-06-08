using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IManagedEntryPoint : ICodedToken, IMDTokenProvider
{
}
