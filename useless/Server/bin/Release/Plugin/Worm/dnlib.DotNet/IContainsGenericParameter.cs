using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IContainsGenericParameter
{
	bool ContainsGenericParameter { get; }
}
