using System.Collections.Generic;
using System.Text;

namespace dnlib.DotNet;

public struct FullNameFactory
{
	private const uint MaxArrayRank = 100u;

	private const uint MaxMethodGenParamCount = 200u;

	private const string RECURSION_ERROR_RESULT_STRING = "<<<INFRECURSION>>>";

	private const string NULLVALUE = "<<<NULL>>>";

	private readonly StringBuilder sb;

	private readonly bool isReflection;

	private readonly IFullNameFactoryHelper helper;

	private GenericArguments genericArguments;

	private RecursionCounter recursionCounter;

	private const int TYPESIG_NAMESPACE = 1;

	private const int TYPESIG_NAME = 2;

	private const int TYPESIG_ONLY_NAMESPACE = 4;

	private string Result => sb?.ToString();

	public static bool MustUseAssemblyName(ModuleDef module, IType type)
	{
		return MustUseAssemblyName(module, type, allowCorlib: true);
	}

	public static bool MustUseAssemblyName(ModuleDef module, IType type, bool allowCorlib)
	{
		if (type is TypeDef typeDef)
		{
			return typeDef.Module != module;
		}
		if (!(type is TypeRef typeRef))
		{
			return true;
		}
		if (typeRef.ResolutionScope == AssemblyRef.CurrentAssembly)
		{
			return false;
		}
		if (allowCorlib)
		{
			if (!typeRef.DefinitionAssembly.IsCorLib())
			{
				return true;
			}
			return module.Find(typeRef) != null;
		}
		return true;
	}

	public static string FullName(IType type, bool isReflection, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		return FullNameSB(type, isReflection, helper, sb).ToString();
	}

	public static StringBuilder FullNameSB(IType type, bool isReflection, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		if (type is TypeDef typeDef)
		{
			return FullNameSB(typeDef, isReflection, helper, sb);
		}
		if (type is TypeRef typeRef)
		{
			return FullNameSB(typeRef, isReflection, helper, sb);
		}
		if (type is TypeSpec typeSpec)
		{
			return FullNameSB(typeSpec, isReflection, helper, sb);
		}
		if (type is TypeSig typeSig)
		{
			return FullNameSB(typeSig, isReflection, helper, null, null, sb);
		}
		if (type is ExportedType exportedType)
		{
			return FullNameSB(exportedType, isReflection, helper, sb);
		}
		return sb ?? new StringBuilder();
	}

	public static string Name(IType type, bool isReflection, StringBuilder sb)
	{
		return NameSB(type, isReflection, sb).ToString();
	}

	public static StringBuilder NameSB(IType type, bool isReflection, StringBuilder sb)
	{
		if (type is TypeDef typeDef)
		{
			return NameSB(typeDef, isReflection, sb);
		}
		if (type is TypeRef typeRef)
		{
			return NameSB(typeRef, isReflection, sb);
		}
		if (type is TypeSpec typeSpec)
		{
			return NameSB(typeSpec, isReflection, sb);
		}
		if (type is TypeSig typeSig)
		{
			return NameSB(typeSig, isReflection, sb);
		}
		if (type is ExportedType exportedType)
		{
			return NameSB(exportedType, isReflection, sb);
		}
		return sb ?? new StringBuilder();
	}

	public static string Namespace(IType type, bool isReflection, StringBuilder sb)
	{
		return NamespaceSB(type, isReflection, sb).ToString();
	}

	public static StringBuilder NamespaceSB(IType type, bool isReflection, StringBuilder sb)
	{
		if (type is TypeDef typeDef)
		{
			return NamespaceSB(typeDef, isReflection, sb);
		}
		if (type is TypeRef typeRef)
		{
			return NamespaceSB(typeRef, isReflection, sb);
		}
		if (type is TypeSpec typeSpec)
		{
			return NamespaceSB(typeSpec, isReflection, sb);
		}
		if (type is TypeSig typeSig)
		{
			return NamespaceSB(typeSig, isReflection, sb);
		}
		if (type is ExportedType exportedType)
		{
			return NamespaceSB(exportedType, isReflection, sb);
		}
		return sb ?? new StringBuilder();
	}

	public static string AssemblyQualifiedName(IType type, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return AssemblyQualifiedNameSB(type, helper, sb).ToString();
	}

	public static StringBuilder AssemblyQualifiedNameSB(IType type, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		if (type is TypeDef typeDef)
		{
			return AssemblyQualifiedNameSB(typeDef, helper, sb);
		}
		if (type is TypeRef typeRef)
		{
			return AssemblyQualifiedNameSB(typeRef, helper, sb);
		}
		if (type is TypeSpec typeSpec)
		{
			return AssemblyQualifiedNameSB(typeSpec, helper, sb);
		}
		if (type is TypeSig typeSig)
		{
			return AssemblyQualifiedNameSB(typeSig, helper, sb);
		}
		if (type is ExportedType exportedType)
		{
			return AssemblyQualifiedNameSB(exportedType, helper, sb);
		}
		return sb ?? new StringBuilder();
	}

	public static string PropertyFullName(string declaringType, UTF8String name, CallingConventionSig propertySig, IList<TypeSig> typeGenArgs = null, StringBuilder sb = null)
	{
		return PropertyFullNameSB(declaringType, name, propertySig, typeGenArgs, sb).ToString();
	}

	public static StringBuilder PropertyFullNameSB(string declaringType, UTF8String name, CallingConventionSig propertySig, IList<TypeSig> typeGenArgs, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: false, null, sb);
		if (typeGenArgs != null)
		{
			fullNameFactory.genericArguments = new GenericArguments();
			fullNameFactory.genericArguments.PushTypeArgs(typeGenArgs);
		}
		fullNameFactory.CreatePropertyFullName(declaringType, name, propertySig);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string EventFullName(string declaringType, UTF8String name, ITypeDefOrRef typeDefOrRef, IList<TypeSig> typeGenArgs = null, StringBuilder sb = null)
	{
		return EventFullNameSB(declaringType, name, typeDefOrRef, typeGenArgs, sb).ToString();
	}

	public static StringBuilder EventFullNameSB(string declaringType, UTF8String name, ITypeDefOrRef typeDefOrRef, IList<TypeSig> typeGenArgs, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: false, null, sb);
		if (typeGenArgs != null)
		{
			fullNameFactory.genericArguments = new GenericArguments();
			fullNameFactory.genericArguments.PushTypeArgs(typeGenArgs);
		}
		fullNameFactory.CreateEventFullName(declaringType, name, typeDefOrRef);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string FieldFullName(string declaringType, string name, FieldSig fieldSig, IList<TypeSig> typeGenArgs = null, StringBuilder sb = null)
	{
		return FieldFullNameSB(declaringType, name, fieldSig, typeGenArgs, sb).ToString();
	}

	public static StringBuilder FieldFullNameSB(string declaringType, string name, FieldSig fieldSig, IList<TypeSig> typeGenArgs, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: false, null, sb);
		if (typeGenArgs != null)
		{
			fullNameFactory.genericArguments = new GenericArguments();
			fullNameFactory.genericArguments.PushTypeArgs(typeGenArgs);
		}
		fullNameFactory.CreateFieldFullName(declaringType, name, fieldSig);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string MethodFullName(string declaringType, string name, MethodSig methodSig, IList<TypeSig> typeGenArgs = null, IList<TypeSig> methodGenArgs = null, MethodDef gppMethod = null, StringBuilder sb = null)
	{
		return MethodFullNameSB(declaringType, name, methodSig, typeGenArgs, methodGenArgs, gppMethod, sb).ToString();
	}

	public static StringBuilder MethodFullNameSB(string declaringType, string name, MethodSig methodSig, IList<TypeSig> typeGenArgs, IList<TypeSig> methodGenArgs, MethodDef gppMethod, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: false, null, sb);
		if (typeGenArgs != null || methodGenArgs != null)
		{
			fullNameFactory.genericArguments = new GenericArguments();
		}
		if (typeGenArgs != null)
		{
			fullNameFactory.genericArguments.PushTypeArgs(typeGenArgs);
		}
		if (methodGenArgs != null)
		{
			fullNameFactory.genericArguments.PushMethodArgs(methodGenArgs);
		}
		fullNameFactory.CreateMethodFullName(declaringType, name, methodSig, gppMethod);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string MethodBaseSigFullName(MethodBaseSig sig, StringBuilder sb = null)
	{
		return MethodBaseSigFullNameSB(sig, sb).ToString();
	}

	public static StringBuilder MethodBaseSigFullNameSB(MethodBaseSig sig, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: false, null, sb);
		fullNameFactory.CreateMethodFullName(null, null, sig, null);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string MethodBaseSigFullName(string declType, string name, MethodBaseSig sig, MethodDef gppMethod, StringBuilder sb = null)
	{
		return MethodBaseSigFullNameSB(declType, name, sig, gppMethod, sb).ToString();
	}

	public static StringBuilder MethodBaseSigFullNameSB(string declType, string name, MethodBaseSig sig, MethodDef gppMethod, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: false, null, sb);
		fullNameFactory.CreateMethodFullName(declType, name, sig, gppMethod);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string Namespace(TypeRef typeRef, bool isReflection, StringBuilder sb = null)
	{
		return NamespaceSB(typeRef, isReflection, sb).ToString();
	}

	public static StringBuilder NamespaceSB(TypeRef typeRef, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateNamespace(typeRef, onlyNamespace: true);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string Name(TypeRef typeRef, bool isReflection, StringBuilder sb = null)
	{
		return NameSB(typeRef, isReflection, sb).ToString();
	}

	public static StringBuilder NameSB(TypeRef typeRef, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateName(typeRef);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string FullName(TypeRef typeRef, bool isReflection, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return FullNameSB(typeRef, isReflection, helper, sb).ToString();
	}

	public static StringBuilder FullNameSB(TypeRef typeRef, bool isReflection, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, helper, sb);
		fullNameFactory.CreateFullName(typeRef);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string AssemblyQualifiedName(TypeRef typeRef, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return AssemblyQualifiedNameSB(typeRef, helper, sb).ToString();
	}

	public static StringBuilder AssemblyQualifiedNameSB(TypeRef typeRef, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: true, helper, sb);
		fullNameFactory.CreateAssemblyQualifiedName(typeRef);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static IAssembly DefinitionAssembly(TypeRef typeRef)
	{
		return default(FullNameFactory).GetDefinitionAssembly(typeRef);
	}

	public static IScope Scope(TypeRef typeRef)
	{
		return default(FullNameFactory).GetScope(typeRef);
	}

	public static ModuleDef OwnerModule(TypeRef typeRef)
	{
		return default(FullNameFactory).GetOwnerModule(typeRef);
	}

	public static string Namespace(TypeDef typeDef, bool isReflection, StringBuilder sb = null)
	{
		return NamespaceSB(typeDef, isReflection, sb).ToString();
	}

	public static StringBuilder NamespaceSB(TypeDef typeDef, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateNamespace(typeDef, onlyNamespace: true);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string Name(TypeDef typeDef, bool isReflection, StringBuilder sb = null)
	{
		return NameSB(typeDef, isReflection, sb).ToString();
	}

	public static StringBuilder NameSB(TypeDef typeDef, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateName(typeDef);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string FullName(TypeDef typeDef, bool isReflection, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return FullNameSB(typeDef, isReflection, helper, sb).ToString();
	}

	public static StringBuilder FullNameSB(TypeDef typeDef, bool isReflection, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, helper, sb);
		fullNameFactory.CreateFullName(typeDef);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string AssemblyQualifiedName(TypeDef typeDef, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return AssemblyQualifiedNameSB(typeDef, helper, sb).ToString();
	}

	public static StringBuilder AssemblyQualifiedNameSB(TypeDef typeDef, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: true, helper, sb);
		fullNameFactory.CreateAssemblyQualifiedName(typeDef);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static IAssembly DefinitionAssembly(TypeDef typeDef)
	{
		return default(FullNameFactory).GetDefinitionAssembly(typeDef);
	}

	public static ModuleDef OwnerModule(TypeDef typeDef)
	{
		return default(FullNameFactory).GetOwnerModule(typeDef);
	}

	public static string Namespace(TypeSpec typeSpec, bool isReflection, StringBuilder sb = null)
	{
		return NamespaceSB(typeSpec, isReflection, sb).ToString();
	}

	public static StringBuilder NamespaceSB(TypeSpec typeSpec, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateNamespace(typeSpec, onlyNamespace: true);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string Name(TypeSpec typeSpec, bool isReflection, StringBuilder sb = null)
	{
		return NameSB(typeSpec, isReflection, sb).ToString();
	}

	public static StringBuilder NameSB(TypeSpec typeSpec, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateName(typeSpec);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string FullName(TypeSpec typeSpec, bool isReflection, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return FullNameSB(typeSpec, isReflection, helper, sb).ToString();
	}

	public static StringBuilder FullNameSB(TypeSpec typeSpec, bool isReflection, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, helper, sb);
		fullNameFactory.CreateFullName(typeSpec);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string AssemblyQualifiedName(TypeSpec typeSpec, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return AssemblyQualifiedNameSB(typeSpec, helper, sb).ToString();
	}

	public static StringBuilder AssemblyQualifiedNameSB(TypeSpec typeSpec, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: true, helper, sb);
		fullNameFactory.CreateAssemblyQualifiedName(typeSpec);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static IAssembly DefinitionAssembly(TypeSpec typeSpec)
	{
		return default(FullNameFactory).GetDefinitionAssembly(typeSpec);
	}

	public static ITypeDefOrRef ScopeType(TypeSpec typeSpec)
	{
		return default(FullNameFactory).GetScopeType(typeSpec);
	}

	public static IScope Scope(TypeSpec typeSpec)
	{
		return default(FullNameFactory).GetScope(typeSpec);
	}

	public static ModuleDef OwnerModule(TypeSpec typeSpec)
	{
		return default(FullNameFactory).GetOwnerModule(typeSpec);
	}

	public static string Namespace(TypeSig typeSig, bool isReflection, StringBuilder sb = null)
	{
		return NamespaceSB(typeSig, isReflection, sb).ToString();
	}

	public static StringBuilder NamespaceSB(TypeSig typeSig, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateNamespace(typeSig, onlyNamespace: true);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string Name(TypeSig typeSig, bool isReflection, StringBuilder sb = null)
	{
		return NameSB(typeSig, isReflection, sb).ToString();
	}

	public static StringBuilder NameSB(TypeSig typeSig, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateName(typeSig);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string FullName(TypeSig typeSig, bool isReflection, IFullNameFactoryHelper helper = null, IList<TypeSig> typeGenArgs = null, IList<TypeSig> methodGenArgs = null, StringBuilder sb = null)
	{
		return FullNameSB(typeSig, isReflection, helper, typeGenArgs, methodGenArgs, sb).ToString();
	}

	public static StringBuilder FullNameSB(TypeSig typeSig, bool isReflection, IFullNameFactoryHelper helper, IList<TypeSig> typeGenArgs, IList<TypeSig> methodGenArgs, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, helper, sb);
		if (typeGenArgs != null || methodGenArgs != null)
		{
			fullNameFactory.genericArguments = new GenericArguments();
		}
		if (typeGenArgs != null)
		{
			fullNameFactory.genericArguments.PushTypeArgs(typeGenArgs);
		}
		if (methodGenArgs != null)
		{
			fullNameFactory.genericArguments.PushMethodArgs(methodGenArgs);
		}
		fullNameFactory.CreateFullName(typeSig);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string AssemblyQualifiedName(TypeSig typeSig, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return AssemblyQualifiedNameSB(typeSig, helper, sb).ToString();
	}

	public static StringBuilder AssemblyQualifiedNameSB(TypeSig typeSig, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: true, helper, sb);
		fullNameFactory.CreateAssemblyQualifiedName(typeSig);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static IAssembly DefinitionAssembly(TypeSig typeSig)
	{
		return default(FullNameFactory).GetDefinitionAssembly(typeSig);
	}

	public static IScope Scope(TypeSig typeSig)
	{
		return default(FullNameFactory).GetScope(typeSig);
	}

	public static ITypeDefOrRef ScopeType(TypeSig typeSig)
	{
		return default(FullNameFactory).GetScopeType(typeSig);
	}

	public static ModuleDef OwnerModule(TypeSig typeSig)
	{
		return default(FullNameFactory).GetOwnerModule(typeSig);
	}

	public static string Namespace(ExportedType exportedType, bool isReflection, StringBuilder sb = null)
	{
		return NamespaceSB(exportedType, isReflection, sb).ToString();
	}

	public static StringBuilder NamespaceSB(ExportedType exportedType, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateNamespace(exportedType, onlyNamespace: true);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string Name(ExportedType exportedType, bool isReflection, StringBuilder sb = null)
	{
		return NameSB(exportedType, isReflection, sb).ToString();
	}

	public static StringBuilder NameSB(ExportedType exportedType, bool isReflection, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, null, sb);
		fullNameFactory.CreateName(exportedType);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string FullName(ExportedType exportedType, bool isReflection, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return FullNameSB(exportedType, isReflection, helper, sb).ToString();
	}

	public static StringBuilder FullNameSB(ExportedType exportedType, bool isReflection, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection, helper, sb);
		fullNameFactory.CreateFullName(exportedType);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static string AssemblyQualifiedName(ExportedType exportedType, IFullNameFactoryHelper helper = null, StringBuilder sb = null)
	{
		return AssemblyQualifiedNameSB(exportedType, helper, sb).ToString();
	}

	public static StringBuilder AssemblyQualifiedNameSB(ExportedType exportedType, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: true, helper, sb);
		fullNameFactory.CreateAssemblyQualifiedName(exportedType);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	public static IAssembly DefinitionAssembly(ExportedType exportedType)
	{
		return default(FullNameFactory).GetDefinitionAssembly(exportedType);
	}

	public static ITypeDefOrRef ScopeType(ExportedType exportedType)
	{
		return default(FullNameFactory).GetScopeType(exportedType);
	}

	public static IScope Scope(ExportedType exportedType)
	{
		return default(FullNameFactory).GetScope(exportedType);
	}

	public static ModuleDef OwnerModule(ExportedType exportedType)
	{
		return default(FullNameFactory).GetOwnerModule(exportedType);
	}

	public static string AssemblyFullName(IAssembly assembly, bool withToken, StringBuilder sb = null)
	{
		return AssemblyFullNameSB(assembly, withToken, sb).ToString();
	}

	public static StringBuilder AssemblyFullNameSB(IAssembly assembly, bool withToken, StringBuilder sb = null)
	{
		FullNameFactory fullNameFactory = new FullNameFactory(isReflection: false, null, sb);
		fullNameFactory.CreateAssemblyFullName(assembly, withToken);
		return fullNameFactory.sb ?? new StringBuilder();
	}

	private FullNameFactory(bool isReflection, IFullNameFactoryHelper helper, StringBuilder sb)
	{
		this.sb = sb ?? new StringBuilder();
		this.isReflection = isReflection;
		this.helper = helper;
		genericArguments = null;
		recursionCounter = default(RecursionCounter);
	}

	private bool MustUseAssemblyName(IType type)
	{
		if (helper == null)
		{
			return true;
		}
		return helper.MustUseAssemblyName(GetDefinitionType(type));
	}

	private IType GetDefinitionType(IType type)
	{
		if (!recursionCounter.Increment())
		{
			return type;
		}
		if (type is TypeSpec typeSpec)
		{
			type = typeSpec.TypeSig;
		}
		if (type is TypeSig typeSig)
		{
			type = ((!(typeSig is TypeDefOrRefSig typeDefOrRefSig)) ? ((!(typeSig is GenericInstSig genericInstSig)) ? GetDefinitionType(typeSig.Next) : GetDefinitionType(genericInstSig.GenericType)) : GetDefinitionType(typeDefOrRefSig.TypeDefOrRef));
		}
		recursionCounter.Decrement();
		return type;
	}

	private void CreateFullName(ITypeDefOrRef typeDefOrRef)
	{
		if (typeDefOrRef is TypeRef)
		{
			CreateFullName((TypeRef)typeDefOrRef);
		}
		else if (typeDefOrRef is TypeDef)
		{
			CreateFullName((TypeDef)typeDefOrRef);
		}
		else if (typeDefOrRef is TypeSpec)
		{
			CreateFullName((TypeSpec)typeDefOrRef);
		}
		else
		{
			sb.Append("<<<NULL>>>");
		}
	}

	private void CreateNamespace(ITypeDefOrRef typeDefOrRef, bool onlyNamespace)
	{
		if (typeDefOrRef is TypeRef)
		{
			CreateNamespace((TypeRef)typeDefOrRef, onlyNamespace);
		}
		else if (typeDefOrRef is TypeDef)
		{
			CreateNamespace((TypeDef)typeDefOrRef, onlyNamespace);
		}
		else if (typeDefOrRef is TypeSpec)
		{
			CreateNamespace((TypeSpec)typeDefOrRef, onlyNamespace);
		}
		else
		{
			sb.Append("<<<NULL>>>");
		}
	}

	private void CreateName(ITypeDefOrRef typeDefOrRef)
	{
		if (typeDefOrRef is TypeRef)
		{
			CreateName((TypeRef)typeDefOrRef);
		}
		else if (typeDefOrRef is TypeDef)
		{
			CreateName((TypeDef)typeDefOrRef);
		}
		else if (typeDefOrRef is TypeSpec)
		{
			CreateName((TypeSpec)typeDefOrRef);
		}
		else
		{
			sb.Append("<<<NULL>>>");
		}
	}

	private void CreateAssemblyQualifiedName(ITypeDefOrRef typeDefOrRef)
	{
		if (typeDefOrRef is TypeRef)
		{
			CreateAssemblyQualifiedName((TypeRef)typeDefOrRef);
		}
		else if (typeDefOrRef is TypeDef)
		{
			CreateAssemblyQualifiedName((TypeDef)typeDefOrRef);
		}
		else if (typeDefOrRef is TypeSpec)
		{
			CreateAssemblyQualifiedName((TypeSpec)typeDefOrRef);
		}
		else
		{
			sb.Append("<<<NULL>>>");
		}
	}

	private void CreateAssemblyQualifiedName(TypeRef typeRef)
	{
		if (typeRef == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		if (!recursionCounter.Increment())
		{
			sb.Append("<<<INFRECURSION>>>");
			return;
		}
		CreateFullName(typeRef);
		if (MustUseAssemblyName(typeRef))
		{
			sb.Append(", ");
			CreateAssemblyFullName(GetDefinitionAssembly(typeRef), useToken: true);
		}
		recursionCounter.Decrement();
	}

	private void CreateFullName(TypeRef typeRef)
	{
		if (typeRef == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		if (!recursionCounter.Increment())
		{
			sb.Append("<<<INFRECURSION>>>");
			return;
		}
		if (typeRef.ResolutionScope is TypeRef typeRef2)
		{
			CreateFullName(typeRef2);
			AddNestedTypeSeparator();
		}
		if (AddNamespace(typeRef.Namespace, onlyNamespace: false))
		{
			sb.Append('.');
		}
		AddName(typeRef.Name);
		recursionCounter.Decrement();
	}

	private void CreateNamespace(TypeRef typeRef, bool onlyNamespace)
	{
		if (typeRef == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			AddNamespace(typeRef.Namespace, onlyNamespace);
		}
	}

	private void CreateName(TypeRef typeRef)
	{
		if (typeRef == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			AddName(typeRef.Name);
		}
	}

	private void CreateAssemblyQualifiedName(TypeDef typeDef)
	{
		if (typeDef == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		if (!recursionCounter.Increment())
		{
			sb.Append("<<<INFRECURSION>>>");
			return;
		}
		CreateFullName(typeDef);
		if (MustUseAssemblyName(typeDef))
		{
			sb.Append(", ");
			CreateAssemblyFullName(GetDefinitionAssembly(typeDef), useToken: true);
		}
		recursionCounter.Decrement();
	}

	private void CreateFullName(TypeDef typeDef)
	{
		if (typeDef == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		if (!recursionCounter.Increment())
		{
			sb.Append("<<<INFRECURSION>>>");
			return;
		}
		TypeDef declaringType = typeDef.DeclaringType;
		if (declaringType != null)
		{
			CreateFullName(declaringType);
			AddNestedTypeSeparator();
		}
		if (AddNamespace(typeDef.Namespace, onlyNamespace: false))
		{
			sb.Append('.');
		}
		AddName(typeDef.Name);
		recursionCounter.Decrement();
	}

	private void CreateNamespace(TypeDef typeDef, bool onlyNamespace)
	{
		if (typeDef == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			AddNamespace(typeDef.Namespace, onlyNamespace);
		}
	}

	private void CreateName(TypeDef typeDef)
	{
		if (typeDef == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			AddName(typeDef.Name);
		}
	}

	private void CreateAssemblyQualifiedName(TypeSpec typeSpec)
	{
		if (typeSpec == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			CreateAssemblyQualifiedName(typeSpec.TypeSig);
		}
	}

	private void CreateFullName(TypeSpec typeSpec)
	{
		if (typeSpec == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			CreateFullName(typeSpec.TypeSig);
		}
	}

	private void CreateNamespace(TypeSpec typeSpec, bool onlyNamespace)
	{
		if (typeSpec == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			CreateNamespace(typeSpec.TypeSig, onlyNamespace);
		}
	}

	private void CreateName(TypeSpec typeSpec)
	{
		if (typeSpec == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			CreateName(typeSpec.TypeSig);
		}
	}

	private void CreateAssemblyQualifiedName(TypeSig typeSig)
	{
		if (typeSig == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		if (!recursionCounter.Increment())
		{
			sb.Append("<<<INFRECURSION>>>");
			return;
		}
		CreateFullName(typeSig);
		if (MustUseAssemblyName(typeSig))
		{
			sb.Append(", ");
			CreateAssemblyFullName(GetDefinitionAssembly(typeSig), useToken: true);
		}
		recursionCounter.Decrement();
	}

	private void CreateFullName(TypeSig typeSig)
	{
		CreateTypeSigName(typeSig, 3);
	}

	private void CreateNamespace(TypeSig typeSig, bool onlyNamespace)
	{
		CreateTypeSigName(typeSig, 1 | (onlyNamespace ? 4 : 0));
	}

	private void CreateName(TypeSig typeSig)
	{
		CreateTypeSigName(typeSig, 2);
	}

	private TypeSig ReplaceGenericArg(TypeSig typeSig)
	{
		if (genericArguments == null)
		{
			return typeSig;
		}
		TypeSig typeSig2 = genericArguments.Resolve(typeSig);
		if (typeSig2 != typeSig)
		{
			genericArguments = null;
		}
		return typeSig2;
	}

	private void CreateTypeSigName(TypeSig typeSig, int flags)
	{
		if (typeSig == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		if (!recursionCounter.Increment())
		{
			sb.Append("<<<INFRECURSION>>>");
			return;
		}
		GenericArguments genericArguments = this.genericArguments;
		typeSig = ReplaceGenericArg(typeSig);
		bool flag = (flags & 1) != 0;
		bool flag2 = (flags & 2) != 0;
		switch (typeSig.ElementType)
		{
		case ElementType.Void:
		case ElementType.Boolean:
		case ElementType.Char:
		case ElementType.I1:
		case ElementType.U1:
		case ElementType.I2:
		case ElementType.U2:
		case ElementType.I4:
		case ElementType.U4:
		case ElementType.I8:
		case ElementType.U8:
		case ElementType.R4:
		case ElementType.R8:
		case ElementType.String:
		case ElementType.ValueType:
		case ElementType.Class:
		case ElementType.TypedByRef:
		case ElementType.I:
		case ElementType.U:
		case ElementType.Object:
			if (flag && flag2)
			{
				CreateFullName(((TypeDefOrRefSig)typeSig).TypeDefOrRef);
			}
			else if (flag)
			{
				CreateNamespace(((TypeDefOrRefSig)typeSig).TypeDefOrRef, (flags & 4) != 0);
			}
			else if (flag2)
			{
				CreateName(((TypeDefOrRefSig)typeSig).TypeDefOrRef);
			}
			break;
		case ElementType.Ptr:
			CreateTypeSigName(typeSig.Next, flags);
			if (flag2)
			{
				sb.Append('*');
			}
			break;
		case ElementType.ByRef:
			CreateTypeSigName(typeSig.Next, flags);
			if (flag2)
			{
				sb.Append('&');
			}
			break;
		case ElementType.Array:
		{
			CreateTypeSigName(typeSig.Next, flags);
			if (!flag2)
			{
				break;
			}
			ArraySig arraySig = (ArraySig)typeSig;
			sb.Append('[');
			uint num = arraySig.Rank;
			if (num > 100)
			{
				num = 100u;
			}
			switch (num)
			{
			case 0u:
				sb.Append("<RANK0>");
				break;
			case 1u:
				sb.Append('*');
				break;
			default:
			{
				for (int i = 0; i < (int)num; i++)
				{
					if (i != 0)
					{
						sb.Append(',');
					}
					if (isReflection)
					{
						continue;
					}
					int num2 = ((i < arraySig.LowerBounds.Count) ? arraySig.LowerBounds[i] : int.MinValue);
					uint num3 = ((i < arraySig.Sizes.Count) ? arraySig.Sizes[i] : uint.MaxValue);
					if (num2 != int.MinValue)
					{
						sb.Append(num2);
						sb.Append("..");
						if (num3 != uint.MaxValue)
						{
							sb.Append(num2 + (int)num3 - 1);
						}
						else
						{
							sb.Append('.');
						}
					}
				}
				break;
			}
			}
			sb.Append(']');
			break;
		}
		case ElementType.SZArray:
			CreateTypeSigName(typeSig.Next, flags);
			if (flag2)
			{
				sb.Append("[]");
			}
			break;
		case ElementType.CModReqd:
			CreateTypeSigName(typeSig.Next, flags);
			if (!isReflection && flag2)
			{
				sb.Append(" modreq(");
				if (flag)
				{
					CreateFullName(((ModifierSig)typeSig).Modifier);
				}
				else
				{
					CreateName(((ModifierSig)typeSig).Modifier);
				}
				sb.Append(")");
			}
			break;
		case ElementType.CModOpt:
			CreateTypeSigName(typeSig.Next, flags);
			if (!isReflection && flag2)
			{
				sb.Append(" modopt(");
				if (flag)
				{
					CreateFullName(((ModifierSig)typeSig).Modifier);
				}
				else
				{
					CreateName(((ModifierSig)typeSig).Modifier);
				}
				sb.Append(")");
			}
			break;
		case ElementType.Pinned:
			CreateTypeSigName(typeSig.Next, flags);
			break;
		case ElementType.ValueArray:
			CreateTypeSigName(typeSig.Next, flags);
			if (flag2)
			{
				ValueArraySig valueArraySig = (ValueArraySig)typeSig;
				sb.Append(" ValueArray(");
				sb.Append(valueArraySig.Size);
				sb.Append(')');
			}
			break;
		case ElementType.Module:
			CreateTypeSigName(typeSig.Next, flags);
			if (flag2)
			{
				ModuleSig moduleSig = (ModuleSig)typeSig;
				sb.Append(" Module(");
				sb.Append(moduleSig.Index);
				sb.Append(')');
			}
			break;
		case ElementType.GenericInst:
		{
			GenericInstSig genericInstSig = (GenericInstSig)typeSig;
			IList<TypeSig> list = genericInstSig.GenericArguments;
			CreateTypeSigName(genericInstSig.GenericType, flags);
			if (!(flag && flag2))
			{
				break;
			}
			if (isReflection)
			{
				sb.Append('[');
				int num4 = -1;
				int count = list.Count;
				for (int j = 0; j < count; j++)
				{
					TypeSig typeSig2 = list[j];
					num4++;
					if (num4 != 0)
					{
						sb.Append(',');
					}
					bool num5 = MustUseAssemblyName(typeSig2);
					if (num5)
					{
						sb.Append('[');
					}
					CreateFullName(typeSig2);
					if (num5)
					{
						sb.Append(", ");
						CreateAssemblyFullName(GetDefinitionAssembly(typeSig2), useToken: true, escapeClosingBracket: true);
						sb.Append(']');
					}
				}
				sb.Append(']');
				break;
			}
			sb.Append('<');
			int num6 = -1;
			int count2 = list.Count;
			for (int k = 0; k < count2; k++)
			{
				TypeSig typeSig3 = list[k];
				num6++;
				if (num6 != 0)
				{
					sb.Append(',');
				}
				CreateFullName(typeSig3);
			}
			sb.Append('>');
			break;
		}
		case ElementType.Var:
		case ElementType.MVar:
			if (flag2)
			{
				GenericSig genericSig = (GenericSig)typeSig;
				GenericParam genericParam = genericSig.GenericParam;
				if (genericParam == null || !AddName(genericParam.Name))
				{
					sb.Append(genericSig.IsMethodVar ? "!!" : "!");
					sb.Append(genericSig.Number);
				}
			}
			break;
		case ElementType.FnPtr:
			if (flag2)
			{
				if (isReflection)
				{
					sb.Append("(fnptr)");
				}
				else
				{
					CreateMethodFullName(null, null, ((FnPtrSig)typeSig).MethodSig, null);
				}
			}
			break;
		}
		this.genericArguments = genericArguments;
		recursionCounter.Decrement();
	}

	private void CreateAssemblyQualifiedName(ExportedType exportedType)
	{
		if (exportedType == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		if (!recursionCounter.Increment())
		{
			sb.Append("<<<INFRECURSION>>>");
			return;
		}
		CreateFullName(exportedType);
		if (MustUseAssemblyName(exportedType))
		{
			sb.Append(", ");
			CreateAssemblyFullName(GetDefinitionAssembly(exportedType), useToken: true);
		}
		recursionCounter.Decrement();
	}

	private void CreateFullName(ExportedType exportedType)
	{
		if (exportedType == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		if (!recursionCounter.Increment())
		{
			sb.Append("<<<INFRECURSION>>>");
			return;
		}
		if (exportedType.Implementation is ExportedType exportedType2)
		{
			CreateFullName(exportedType2);
			AddNestedTypeSeparator();
		}
		if (AddNamespace(exportedType.TypeNamespace, onlyNamespace: false))
		{
			sb.Append('.');
		}
		AddName(exportedType.TypeName);
		recursionCounter.Decrement();
	}

	private void CreateNamespace(ExportedType exportedType, bool onlyNamespace)
	{
		if (exportedType == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			AddNamespace(exportedType.TypeNamespace, onlyNamespace);
		}
	}

	private void CreateName(ExportedType exportedType)
	{
		if (exportedType == null)
		{
			sb.Append("<<<NULL>>>");
		}
		else
		{
			AddName(exportedType.TypeName);
		}
	}

	private static string EscapeAssemblyName(UTF8String asmSimpleName)
	{
		return EscapeAssemblyName(UTF8String.ToSystemString(asmSimpleName));
	}

	private static string EscapeAssemblyName(string asmSimpleName)
	{
		if (asmSimpleName.IndexOf(']') < 0)
		{
			return asmSimpleName;
		}
		StringBuilder stringBuilder = new StringBuilder(asmSimpleName.Length);
		foreach (char c in asmSimpleName)
		{
			if (c == ']')
			{
				stringBuilder.Append('\\');
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private void AddNestedTypeSeparator()
	{
		if (isReflection)
		{
			sb.Append('+');
		}
		else
		{
			sb.Append('/');
		}
	}

	private bool AddNamespace(UTF8String @namespace, bool onlyNamespace)
	{
		if (UTF8String.IsNullOrEmpty(@namespace))
		{
			return false;
		}
		if (onlyNamespace && isReflection)
		{
			sb.Append(@namespace.String);
		}
		else
		{
			AddIdentifier(@namespace.String);
		}
		return true;
	}

	private bool AddName(UTF8String name)
	{
		if (UTF8String.IsNullOrEmpty(name))
		{
			return false;
		}
		AddIdentifier(name.String);
		return true;
	}

	private void CreateAssemblyFullName(IAssembly assembly, bool useToken, bool escapeClosingBracket = false)
	{
		if (assembly == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		string text = UTF8String.ToSystemStringOrEmpty(assembly.Name);
		foreach (char c in text)
		{
			if (c == ',' || c == '=' || (escapeClosingBracket && c == ']'))
			{
				sb.Append('\\');
			}
			sb.Append(c);
		}
		if ((object)assembly.Version != null)
		{
			sb.Append(", Version=");
			sb.Append(Utils.CreateVersionWithNoUndefinedValues(assembly.Version));
		}
		if ((object)assembly.Culture != null)
		{
			sb.Append(", Culture=");
			if (UTF8String.IsNullOrEmpty(assembly.Culture))
			{
				sb.Append("neutral");
			}
			else
			{
				sb.Append(escapeClosingBracket ? EscapeAssemblyName(assembly.Culture) : assembly.Culture.String);
			}
		}
		PublicKeyBase publicKeyBase = assembly.PublicKeyOrToken;
		if (useToken)
		{
			publicKeyBase = PublicKeyBase.ToPublicKeyToken(publicKeyBase);
		}
		sb.Append(", ");
		sb.Append((publicKeyBase == null || publicKeyBase is PublicKeyToken) ? "PublicKeyToken=" : "PublicKey=");
		sb.Append((publicKeyBase == null) ? "null" : publicKeyBase.ToString());
		if (assembly.IsRetargetable)
		{
			sb.Append(", Retargetable=Yes");
		}
		if (assembly.IsContentTypeWindowsRuntime)
		{
			sb.Append(", ContentType=WindowsRuntime");
		}
	}

	private void AddIdentifier(string id)
	{
		if (isReflection)
		{
			foreach (char c in id)
			{
				switch (c)
				{
				case '&':
				case '*':
				case '+':
				case ',':
				case '[':
				case '\\':
				case ']':
					sb.Append('\\');
					break;
				}
				sb.Append(c);
			}
		}
		else
		{
			sb.Append(id);
		}
	}

	private IAssembly GetDefinitionAssembly(ITypeDefOrRef typeDefOrRef)
	{
		if (typeDefOrRef is TypeRef typeRef)
		{
			return GetDefinitionAssembly(typeRef);
		}
		if (typeDefOrRef is TypeDef typeDef)
		{
			return GetDefinitionAssembly(typeDef);
		}
		if (typeDefOrRef is TypeSpec typeSpec)
		{
			return GetDefinitionAssembly(typeSpec);
		}
		return null;
	}

	private IScope GetScope(ITypeDefOrRef typeDefOrRef)
	{
		if (typeDefOrRef is TypeRef typeRef)
		{
			return GetScope(typeRef);
		}
		if (typeDefOrRef is TypeDef typeDef)
		{
			return typeDef.Scope;
		}
		if (typeDefOrRef is TypeSpec typeSpec)
		{
			return GetScope(typeSpec);
		}
		return null;
	}

	private ITypeDefOrRef GetScopeType(ITypeDefOrRef typeDefOrRef)
	{
		if (typeDefOrRef is TypeRef result)
		{
			return result;
		}
		if (typeDefOrRef is TypeDef result2)
		{
			return result2;
		}
		if (typeDefOrRef is TypeSpec typeSpec)
		{
			return GetScopeType(typeSpec);
		}
		return null;
	}

	private ModuleDef GetOwnerModule(ITypeDefOrRef typeDefOrRef)
	{
		if (typeDefOrRef is TypeRef typeRef)
		{
			return GetOwnerModule(typeRef);
		}
		if (typeDefOrRef is TypeDef typeDef)
		{
			return GetOwnerModule(typeDef);
		}
		if (typeDefOrRef is TypeSpec typeSpec)
		{
			return GetOwnerModule(typeSpec);
		}
		return null;
	}

	private IAssembly GetDefinitionAssembly(TypeRef typeRef)
	{
		if (typeRef == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IResolutionScope resolutionScope = typeRef.ResolutionScope;
		IAssembly result = ((resolutionScope == null) ? null : ((resolutionScope is TypeRef) ? GetDefinitionAssembly((TypeRef)resolutionScope) : ((resolutionScope is AssemblyRef) ? ((IAssembly)(AssemblyRef)resolutionScope) : ((IAssembly)((resolutionScope is ModuleRef) ? GetOwnerModule(typeRef)?.Assembly : ((!(resolutionScope is ModuleDef)) ? null : ((ModuleDef)resolutionScope).Assembly))))));
		recursionCounter.Decrement();
		return result;
	}

	private IScope GetScope(TypeRef typeRef)
	{
		if (typeRef == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IResolutionScope resolutionScope = typeRef.ResolutionScope;
		IScope result = ((resolutionScope == null) ? null : ((!(resolutionScope is TypeRef typeRef2)) ? ((!(resolutionScope is AssemblyRef assemblyRef)) ? ((!(resolutionScope is ModuleRef moduleRef)) ? ((IScope)((!(resolutionScope is ModuleDef moduleDef)) ? null : moduleDef)) : ((IScope)moduleRef)) : assemblyRef) : GetScope(typeRef2)));
		recursionCounter.Decrement();
		return result;
	}

	private ModuleDef GetOwnerModule(TypeRef typeRef)
	{
		return typeRef?.Module;
	}

	private IAssembly GetDefinitionAssembly(TypeDef typeDef)
	{
		return GetOwnerModule(typeDef)?.Assembly;
	}

	private ModuleDef GetOwnerModule(TypeDef typeDef)
	{
		if (typeDef == null)
		{
			return null;
		}
		ModuleDef result = null;
		for (int i = recursionCounter.Counter; i < 100; i++)
		{
			TypeDef declaringType = typeDef.DeclaringType;
			if (declaringType == null)
			{
				result = typeDef.Module2;
				break;
			}
			typeDef = declaringType;
		}
		return result;
	}

	private IAssembly GetDefinitionAssembly(TypeSpec typeSpec)
	{
		if (typeSpec == null)
		{
			return null;
		}
		return GetDefinitionAssembly(typeSpec.TypeSig);
	}

	private IScope GetScope(TypeSpec typeSpec)
	{
		if (typeSpec == null)
		{
			return null;
		}
		return GetScope(typeSpec.TypeSig);
	}

	private ITypeDefOrRef GetScopeType(TypeSpec typeSpec)
	{
		if (typeSpec == null)
		{
			return null;
		}
		return GetScopeType(typeSpec.TypeSig);
	}

	private ModuleDef GetOwnerModule(TypeSpec typeSpec)
	{
		if (typeSpec == null)
		{
			return null;
		}
		return GetOwnerModule(typeSpec.TypeSig);
	}

	private IAssembly GetDefinitionAssembly(TypeSig typeSig)
	{
		if (typeSig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		GenericArguments genericArguments = this.genericArguments;
		typeSig = ReplaceGenericArg(typeSig);
		IAssembly result;
		switch (typeSig.ElementType)
		{
		case ElementType.Void:
		case ElementType.Boolean:
		case ElementType.Char:
		case ElementType.I1:
		case ElementType.U1:
		case ElementType.I2:
		case ElementType.U2:
		case ElementType.I4:
		case ElementType.U4:
		case ElementType.I8:
		case ElementType.U8:
		case ElementType.R4:
		case ElementType.R8:
		case ElementType.String:
		case ElementType.ValueType:
		case ElementType.Class:
		case ElementType.TypedByRef:
		case ElementType.I:
		case ElementType.U:
		case ElementType.Object:
			result = GetDefinitionAssembly(((TypeDefOrRefSig)typeSig).TypeDefOrRef);
			break;
		case ElementType.Ptr:
		case ElementType.ByRef:
		case ElementType.Array:
		case ElementType.ValueArray:
		case ElementType.SZArray:
		case ElementType.CModReqd:
		case ElementType.CModOpt:
		case ElementType.Module:
		case ElementType.Pinned:
			result = GetDefinitionAssembly(typeSig.Next);
			break;
		case ElementType.GenericInst:
			result = GetDefinitionAssembly(((GenericInstSig)typeSig).GenericType?.TypeDefOrRef);
			break;
		default:
			result = null;
			break;
		}
		this.genericArguments = genericArguments;
		recursionCounter.Decrement();
		return result;
	}

	private ITypeDefOrRef GetScopeType(TypeSig typeSig)
	{
		if (typeSig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		GenericArguments genericArguments = this.genericArguments;
		typeSig = ReplaceGenericArg(typeSig);
		ITypeDefOrRef result;
		switch (typeSig.ElementType)
		{
		case ElementType.Void:
		case ElementType.Boolean:
		case ElementType.Char:
		case ElementType.I1:
		case ElementType.U1:
		case ElementType.I2:
		case ElementType.U2:
		case ElementType.I4:
		case ElementType.U4:
		case ElementType.I8:
		case ElementType.U8:
		case ElementType.R4:
		case ElementType.R8:
		case ElementType.String:
		case ElementType.ValueType:
		case ElementType.Class:
		case ElementType.TypedByRef:
		case ElementType.I:
		case ElementType.U:
		case ElementType.Object:
			result = GetScopeType(((TypeDefOrRefSig)typeSig).TypeDefOrRef);
			break;
		case ElementType.Ptr:
		case ElementType.ByRef:
		case ElementType.Array:
		case ElementType.ValueArray:
		case ElementType.SZArray:
		case ElementType.CModReqd:
		case ElementType.CModOpt:
		case ElementType.Module:
		case ElementType.Pinned:
			result = GetScopeType(typeSig.Next);
			break;
		case ElementType.GenericInst:
			result = GetScopeType(((GenericInstSig)typeSig).GenericType?.TypeDefOrRef);
			break;
		default:
			result = null;
			break;
		}
		this.genericArguments = genericArguments;
		recursionCounter.Decrement();
		return result;
	}

	private IScope GetScope(TypeSig typeSig)
	{
		if (typeSig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		GenericArguments genericArguments = this.genericArguments;
		typeSig = ReplaceGenericArg(typeSig);
		IScope result;
		switch (typeSig.ElementType)
		{
		case ElementType.Void:
		case ElementType.Boolean:
		case ElementType.Char:
		case ElementType.I1:
		case ElementType.U1:
		case ElementType.I2:
		case ElementType.U2:
		case ElementType.I4:
		case ElementType.U4:
		case ElementType.I8:
		case ElementType.U8:
		case ElementType.R4:
		case ElementType.R8:
		case ElementType.String:
		case ElementType.ValueType:
		case ElementType.Class:
		case ElementType.TypedByRef:
		case ElementType.I:
		case ElementType.U:
		case ElementType.Object:
			result = GetScope(((TypeDefOrRefSig)typeSig).TypeDefOrRef);
			break;
		case ElementType.Ptr:
		case ElementType.ByRef:
		case ElementType.Array:
		case ElementType.ValueArray:
		case ElementType.SZArray:
		case ElementType.CModReqd:
		case ElementType.CModOpt:
		case ElementType.Module:
		case ElementType.Pinned:
			result = GetScope(typeSig.Next);
			break;
		case ElementType.GenericInst:
			result = GetScope(((GenericInstSig)typeSig).GenericType?.TypeDefOrRef);
			break;
		default:
			result = null;
			break;
		}
		this.genericArguments = genericArguments;
		recursionCounter.Decrement();
		return result;
	}

	private ModuleDef GetOwnerModule(TypeSig typeSig)
	{
		if (typeSig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		GenericArguments genericArguments = this.genericArguments;
		typeSig = ReplaceGenericArg(typeSig);
		ModuleDef result;
		switch (typeSig.ElementType)
		{
		case ElementType.Void:
		case ElementType.Boolean:
		case ElementType.Char:
		case ElementType.I1:
		case ElementType.U1:
		case ElementType.I2:
		case ElementType.U2:
		case ElementType.I4:
		case ElementType.U4:
		case ElementType.I8:
		case ElementType.U8:
		case ElementType.R4:
		case ElementType.R8:
		case ElementType.String:
		case ElementType.ValueType:
		case ElementType.Class:
		case ElementType.TypedByRef:
		case ElementType.I:
		case ElementType.U:
		case ElementType.Object:
			result = GetOwnerModule(((TypeDefOrRefSig)typeSig).TypeDefOrRef);
			break;
		case ElementType.Ptr:
		case ElementType.ByRef:
		case ElementType.Array:
		case ElementType.ValueArray:
		case ElementType.SZArray:
		case ElementType.CModReqd:
		case ElementType.CModOpt:
		case ElementType.Module:
		case ElementType.Pinned:
			result = GetOwnerModule(typeSig.Next);
			break;
		case ElementType.GenericInst:
			result = GetOwnerModule(((GenericInstSig)typeSig).GenericType?.TypeDefOrRef);
			break;
		default:
			result = null;
			break;
		}
		this.genericArguments = genericArguments;
		recursionCounter.Decrement();
		return result;
	}

	private IAssembly GetDefinitionAssembly(ExportedType exportedType)
	{
		if (exportedType == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IImplementation implementation = exportedType.Implementation;
		IAssembly result = ((!(implementation is ExportedType exportedType2)) ? ((!(implementation is AssemblyRef assemblyRef)) ? ((IAssembly)((!(implementation is FileDef)) ? null : GetOwnerModule(exportedType)?.Assembly)) : ((IAssembly)assemblyRef)) : GetDefinitionAssembly(exportedType2));
		recursionCounter.Decrement();
		return result;
	}

	private ITypeDefOrRef GetScopeType(ExportedType exportedType)
	{
		return null;
	}

	private IScope GetScope(ExportedType exportedType)
	{
		if (exportedType == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IImplementation implementation = exportedType.Implementation;
		IScope result;
		if (implementation is ExportedType exportedType2)
		{
			result = GetScope(exportedType2);
		}
		else if (implementation is AssemblyRef assemblyRef)
		{
			result = assemblyRef;
		}
		else if (implementation is FileDef fileDef)
		{
			ModuleDef ownerModule = GetOwnerModule(exportedType);
			ModuleRefUser moduleRefUser = new ModuleRefUser(ownerModule, fileDef.Name);
			ownerModule?.UpdateRowId(moduleRefUser);
			result = moduleRefUser;
		}
		else
		{
			result = null;
		}
		recursionCounter.Decrement();
		return result;
	}

	private ModuleDef GetOwnerModule(ExportedType exportedType)
	{
		return exportedType?.Module;
	}

	private void CreateFieldFullName(string declaringType, string name, FieldSig fieldSig)
	{
		CreateFullName(fieldSig?.Type);
		sb.Append(' ');
		if (declaringType != null)
		{
			sb.Append(declaringType);
			sb.Append("::");
		}
		if (name != null)
		{
			sb.Append(name);
		}
	}

	private void CreateMethodFullName(string declaringType, string name, MethodBaseSig methodSig, MethodDef gppMethod)
	{
		if (methodSig == null)
		{
			sb.Append("<<<NULL>>>");
			return;
		}
		CreateFullName(methodSig.RetType);
		sb.Append(' ');
		if (declaringType != null)
		{
			sb.Append(declaringType);
			sb.Append("::");
		}
		if (name != null)
		{
			sb.Append(name);
		}
		if (methodSig.Generic)
		{
			sb.Append('<');
			uint num = methodSig.GenParamCount;
			if (num > 200)
			{
				num = 200u;
			}
			for (uint num2 = 0u; num2 < num; num2++)
			{
				if (num2 != 0)
				{
					sb.Append(',');
				}
				CreateFullName(new GenericMVar(num2, gppMethod));
			}
			sb.Append('>');
		}
		sb.Append('(');
		int num3 = PrintMethodArgList(methodSig.Params, hasPrintedArgs: false, isAfterSentinel: false);
		PrintMethodArgList(methodSig.ParamsAfterSentinel, num3 > 0, isAfterSentinel: true);
		sb.Append(')');
	}

	private int PrintMethodArgList(IList<TypeSig> args, bool hasPrintedArgs, bool isAfterSentinel)
	{
		if (args == null)
		{
			return 0;
		}
		if (isAfterSentinel)
		{
			if (hasPrintedArgs)
			{
				sb.Append(',');
			}
			sb.Append("...");
			hasPrintedArgs = true;
		}
		int num = 0;
		int count = args.Count;
		for (int i = 0; i < count; i++)
		{
			TypeSig typeSig = args[i];
			num++;
			if (hasPrintedArgs)
			{
				sb.Append(',');
			}
			CreateFullName(typeSig);
			hasPrintedArgs = true;
		}
		return num;
	}

	private void CreatePropertyFullName(string declaringType, UTF8String name, CallingConventionSig propertySig)
	{
		CreateMethodFullName(declaringType, UTF8String.ToSystemString(name), propertySig as MethodBaseSig, null);
	}

	private void CreateEventFullName(string declaringType, UTF8String name, ITypeDefOrRef typeDefOrRef)
	{
		CreateFullName(typeDefOrRef);
		sb.Append(' ');
		if (declaringType != null)
		{
			sb.Append(declaringType);
			sb.Append("::");
		}
		if (!UTF8String.IsNull(name))
		{
			sb.Append(UTF8String.ToSystemString(name));
		}
	}

	public override string ToString()
	{
		return Result;
	}
}
