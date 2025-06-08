using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class NullResolver : IAssemblyResolver, IResolver, ITypeResolver, IMemberRefResolver
{
	public static readonly NullResolver Instance = new NullResolver();

	private NullResolver()
	{
	}

	public AssemblyDef Resolve(IAssembly assembly, ModuleDef sourceModule)
	{
		return null;
	}

	public TypeDef Resolve(TypeRef typeRef, ModuleDef sourceModule)
	{
		return null;
	}

	public IMemberForwarded Resolve(MemberRef memberRef)
	{
		return null;
	}
}
