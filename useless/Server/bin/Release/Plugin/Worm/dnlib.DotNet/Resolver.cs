using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class Resolver : IResolver, ITypeResolver, IMemberRefResolver
{
	private readonly IAssemblyResolver assemblyResolver;

	private bool projectWinMDRefs = true;

	public bool ProjectWinMDRefs
	{
		get
		{
			return projectWinMDRefs;
		}
		set
		{
			projectWinMDRefs = value;
		}
	}

	public Resolver(IAssemblyResolver assemblyResolver)
	{
		this.assemblyResolver = assemblyResolver ?? throw new ArgumentNullException("assemblyResolver");
	}

	public TypeDef Resolve(TypeRef typeRef, ModuleDef sourceModule)
	{
		if (typeRef == null)
		{
			return null;
		}
		if (ProjectWinMDRefs)
		{
			typeRef = WinMDHelpers.ToCLR(typeRef.Module ?? sourceModule, typeRef) ?? typeRef;
		}
		TypeRef nonNestedTypeRef = TypeRef.GetNonNestedTypeRef(typeRef);
		if (nonNestedTypeRef == null)
		{
			return null;
		}
		IResolutionScope resolutionScope = nonNestedTypeRef.ResolutionScope;
		ModuleDef module = nonNestedTypeRef.Module;
		if (resolutionScope is AssemblyRef assembly)
		{
			AssemblyDef assemblyDef = assemblyResolver.Resolve(assembly, sourceModule ?? module);
			object obj;
			if (assemblyDef != null)
			{
				obj = assemblyDef.Find(typeRef);
				if (obj == null)
				{
					return ResolveExportedType(assemblyDef.Modules, typeRef, sourceModule);
				}
			}
			else
			{
				obj = null;
			}
			return (TypeDef)obj;
		}
		if (resolutionScope is ModuleDef moduleDef)
		{
			return moduleDef.Find(typeRef) ?? ResolveExportedType(new ModuleDef[1] { moduleDef }, typeRef, sourceModule);
		}
		if (resolutionScope is ModuleRef moduleRef)
		{
			if (module == null)
			{
				return null;
			}
			if (default(SigComparer).Equals(moduleRef, module))
			{
				return module.Find(typeRef) ?? ResolveExportedType(new ModuleDef[1] { module }, typeRef, sourceModule);
			}
			AssemblyDef assembly2 = module.Assembly;
			if (assembly2 == null)
			{
				return null;
			}
			ModuleDef moduleDef2 = assembly2.FindModule(moduleRef.Name);
			object obj2;
			if (moduleDef2 != null)
			{
				obj2 = moduleDef2.Find(typeRef);
				if (obj2 == null)
				{
					return ResolveExportedType(new ModuleDef[1] { moduleDef2 }, typeRef, sourceModule);
				}
			}
			else
			{
				obj2 = null;
			}
			return (TypeDef)obj2;
		}
		if (resolutionScope == null)
		{
			return module.Find(typeRef) ?? ResolveExportedType(new ModuleDef[1] { module }, typeRef, sourceModule);
		}
		return null;
	}

	private TypeDef ResolveExportedType(IList<ModuleDef> modules, TypeRef typeRef, ModuleDef sourceModule)
	{
		for (int i = 0; i < 30; i++)
		{
			ExportedType exportedType = FindExportedType(modules, typeRef);
			if (exportedType == null)
			{
				return null;
			}
			AssemblyDef assemblyDef = modules[0].Context.AssemblyResolver.Resolve(exportedType.DefinitionAssembly, sourceModule ?? typeRef.Module);
			if (assemblyDef == null)
			{
				return null;
			}
			TypeDef typeDef = assemblyDef.Find(typeRef);
			if (typeDef != null)
			{
				return typeDef;
			}
			modules = assemblyDef.Modules;
		}
		return null;
	}

	private static ExportedType FindExportedType(IList<ModuleDef> modules, TypeRef typeRef)
	{
		if (typeRef == null)
		{
			return null;
		}
		int count = modules.Count;
		for (int i = 0; i < count; i++)
		{
			IList<ExportedType> exportedTypes = modules[i].ExportedTypes;
			int count2 = exportedTypes.Count;
			for (int j = 0; j < count2; j++)
			{
				ExportedType exportedType = exportedTypes[j];
				if (new SigComparer(SigComparerOptions.DontCompareTypeScope).Equals(exportedType, typeRef))
				{
					return exportedType;
				}
			}
		}
		return null;
	}

	public IMemberForwarded Resolve(MemberRef memberRef)
	{
		if (memberRef == null)
		{
			return null;
		}
		if (ProjectWinMDRefs)
		{
			memberRef = WinMDHelpers.ToCLR(memberRef.Module, memberRef) ?? memberRef;
		}
		IMemberRefParent @class = memberRef.Class;
		if (@class is MethodDef result)
		{
			return result;
		}
		return GetDeclaringType(memberRef, @class)?.Resolve(memberRef);
	}

	private TypeDef GetDeclaringType(MemberRef memberRef, IMemberRefParent parent)
	{
		if (memberRef == null || parent == null)
		{
			return null;
		}
		if (parent is TypeSpec typeSpec)
		{
			parent = typeSpec.ScopeType;
		}
		if (parent is TypeDef result)
		{
			return result;
		}
		if (parent is TypeRef typeRef)
		{
			return Resolve(typeRef, memberRef.Module);
		}
		if (parent is ModuleRef moduleRef)
		{
			ModuleDef module = memberRef.Module;
			if (module == null)
			{
				return null;
			}
			TypeDef typeDef = null;
			if (default(SigComparer).Equals(module, moduleRef))
			{
				typeDef = module.GlobalType;
			}
			AssemblyDef assembly = module.Assembly;
			if (typeDef == null && assembly != null)
			{
				ModuleDef moduleDef = assembly.FindModule(moduleRef.Name);
				if (moduleDef != null)
				{
					typeDef = moduleDef.GlobalType;
				}
			}
			return typeDef;
		}
		if (parent is MethodDef methodDef)
		{
			return methodDef.DeclaringType;
		}
		return null;
	}
}
