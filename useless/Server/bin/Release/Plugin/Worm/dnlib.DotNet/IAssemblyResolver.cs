using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IAssemblyResolver
{
	AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule);
}
