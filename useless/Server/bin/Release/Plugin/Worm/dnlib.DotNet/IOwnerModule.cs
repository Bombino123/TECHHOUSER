using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IOwnerModule
{
	ModuleDef Module { get; }
}
