using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IImplementation : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int ImplementationTag { get; }
}
