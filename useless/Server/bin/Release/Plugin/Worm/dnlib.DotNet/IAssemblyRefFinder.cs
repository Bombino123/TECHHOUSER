using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public interface IAssemblyRefFinder
{
	AssemblyRef FindAssemblyRef(TypeRef nonNestedTypeRef);
}
