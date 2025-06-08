using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IResolutionScope : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int ResolutionScopeTag { get; }
}
