using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IIsTypeOrMethod
{
	bool IsType { get; }

	bool IsMethod { get; }
}
