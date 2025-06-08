using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IDnlibDef : ICodedToken, IMDTokenProvider, IFullName, IHasCustomAttribute
{
}
