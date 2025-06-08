namespace dnlib.DotNet;

public interface IAssemblyRefFinder
{
	AssemblyRef FindAssemblyRef(TypeRef nonNestedTypeRef);
}
