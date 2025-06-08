using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IHasConstant : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int HasConstantTag { get; }

	Constant Constant { get; set; }
}
