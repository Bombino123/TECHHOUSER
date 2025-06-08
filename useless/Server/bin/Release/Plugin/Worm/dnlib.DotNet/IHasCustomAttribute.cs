using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IHasCustomAttribute : ICodedToken, IMDTokenProvider
{
	int HasCustomAttributeTag { get; }

	CustomAttributeCollection CustomAttributes { get; }

	bool HasCustomAttributes { get; }
}
