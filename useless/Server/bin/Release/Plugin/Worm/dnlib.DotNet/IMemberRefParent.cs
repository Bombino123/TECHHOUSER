using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IMemberRefParent : ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName
{
	int MemberRefParentTag { get; }
}
