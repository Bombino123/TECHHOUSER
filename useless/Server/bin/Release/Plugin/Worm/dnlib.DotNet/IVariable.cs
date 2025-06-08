using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IVariable
{
	TypeSig Type { get; }

	int Index { get; }

	string Name { get; set; }
}
