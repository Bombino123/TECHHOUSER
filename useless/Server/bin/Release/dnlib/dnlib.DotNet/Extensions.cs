using System;
using System.Collections.Generic;
using System.Reflection;

namespace dnlib.DotNet;

public static class Extensions
{
	internal static string GetName(this AssemblyHashAlgorithm hashAlg)
	{
		return hashAlg switch
		{
			AssemblyHashAlgorithm.MD2 => null, 
			AssemblyHashAlgorithm.MD4 => null, 
			AssemblyHashAlgorithm.MD5 => "MD5", 
			AssemblyHashAlgorithm.SHA1 => "SHA1", 
			AssemblyHashAlgorithm.MAC => null, 
			AssemblyHashAlgorithm.SSL3_SHAMD5 => null, 
			AssemblyHashAlgorithm.HMAC => null, 
			AssemblyHashAlgorithm.TLS1PRF => null, 
			AssemblyHashAlgorithm.HASH_REPLACE_OWF => null, 
			AssemblyHashAlgorithm.SHA_256 => "SHA256", 
			AssemblyHashAlgorithm.SHA_384 => "SHA384", 
			AssemblyHashAlgorithm.SHA_512 => "SHA512", 
			_ => null, 
		};
	}

	public static TypeSig GetFieldType(this FieldSig sig)
	{
		return sig?.Type;
	}

	public static TypeSig GetRetType(this MethodBaseSig sig)
	{
		return sig?.RetType;
	}

	public static IList<TypeSig> GetParams(this MethodBaseSig sig)
	{
		return sig?.Params ?? new List<TypeSig>();
	}

	public static int GetParamCount(this MethodBaseSig sig)
	{
		return sig?.Params.Count ?? 0;
	}

	public static uint GetGenParamCount(this MethodBaseSig sig)
	{
		return sig?.GenParamCount ?? 0;
	}

	public static IList<TypeSig> GetParamsAfterSentinel(this MethodBaseSig sig)
	{
		return sig?.ParamsAfterSentinel;
	}

	public static IList<TypeSig> GetLocals(this LocalSig sig)
	{
		return sig?.Locals ?? new List<TypeSig>();
	}

	public static IList<TypeSig> GetGenericArguments(this GenericInstMethodSig sig)
	{
		return sig?.GenericArguments ?? new List<TypeSig>();
	}

	public static bool GetIsDefault(this CallingConventionSig sig)
	{
		return sig?.IsDefault ?? false;
	}

	public static bool IsPrimitive(this ElementType etype)
	{
		if (etype - 2 <= ElementType.U8 || etype - 24 <= ElementType.Boolean)
		{
			return true;
		}
		return false;
	}

	public static int GetPrimitiveSize(this ElementType etype, int ptrSize = -1)
	{
		switch (etype)
		{
		case ElementType.Boolean:
		case ElementType.I1:
		case ElementType.U1:
			return 1;
		case ElementType.Char:
		case ElementType.I2:
		case ElementType.U2:
			return 2;
		case ElementType.I4:
		case ElementType.U4:
		case ElementType.R4:
			return 4;
		case ElementType.I8:
		case ElementType.U8:
		case ElementType.R8:
			return 8;
		case ElementType.Ptr:
		case ElementType.I:
		case ElementType.U:
		case ElementType.FnPtr:
			return ptrSize;
		default:
			return -1;
		}
	}

	public static bool IsValueType(this ElementType etype)
	{
		switch (etype)
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
		case ElementType.ValueType:
		case ElementType.TypedByRef:
		case ElementType.ValueArray:
		case ElementType.I:
		case ElementType.U:
		case ElementType.R:
			return true;
		case ElementType.GenericInst:
			return false;
		default:
			return false;
		}
	}

	public static AssemblyDef Resolve(this IAssemblyResolver self, AssemblyName assembly, ModuleDef sourceModule)
	{
		if (assembly == null)
		{
			return null;
		}
		return self.Resolve(new AssemblyNameInfo(assembly), sourceModule);
	}

	public static AssemblyDef Resolve(this IAssemblyResolver self, string asmFullName, ModuleDef sourceModule)
	{
		if (asmFullName == null)
		{
			return null;
		}
		return self.Resolve(new AssemblyNameInfo(asmFullName), sourceModule);
	}

	public static AssemblyDef ResolveThrow(this IAssemblyResolver self, IAssembly assembly, ModuleDef sourceModule)
	{
		if (assembly == null)
		{
			return null;
		}
		AssemblyDef assemblyDef = self.Resolve(assembly, sourceModule);
		if (assemblyDef != null)
		{
			return assemblyDef;
		}
		throw new AssemblyResolveException($"Could not resolve assembly: {assembly}");
	}

	public static AssemblyDef ResolveThrow(this IAssemblyResolver self, AssemblyName assembly, ModuleDef sourceModule)
	{
		if (assembly == null)
		{
			return null;
		}
		AssemblyDef assemblyDef = self.Resolve(new AssemblyNameInfo(assembly), sourceModule);
		if (assemblyDef != null)
		{
			return assemblyDef;
		}
		throw new AssemblyResolveException($"Could not resolve assembly: {assembly}");
	}

	public static AssemblyDef ResolveThrow(this IAssemblyResolver self, string asmFullName, ModuleDef sourceModule)
	{
		if (asmFullName == null)
		{
			return null;
		}
		AssemblyDef assemblyDef = self.Resolve(new AssemblyNameInfo(asmFullName), sourceModule);
		if (assemblyDef != null)
		{
			return assemblyDef;
		}
		throw new AssemblyResolveException("Could not resolve assembly: " + asmFullName);
	}

	public static bool IsCorLib(this IAssembly asm)
	{
		if (asm is AssemblyDef { ManifestModule: { IsCoreLibraryModule: var isCoreLibraryModule } } && isCoreLibraryModule.HasValue)
		{
			return isCoreLibraryModule.Value;
		}
		if (asm != null && UTF8String.IsNullOrEmpty(asm.Culture))
		{
			string text;
			if (!(text = UTF8String.ToSystemStringOrEmpty(asm.Name)).Equals("mscorlib", StringComparison.OrdinalIgnoreCase) && !text.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase) && !text.Equals("System.Private.CoreLib", StringComparison.OrdinalIgnoreCase) && !text.Equals("netstandard", StringComparison.OrdinalIgnoreCase))
			{
				return text.Equals("corefx", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	public static AssemblyRef ToAssemblyRef(this IAssembly asm)
	{
		if (asm == null)
		{
			return null;
		}
		return new AssemblyRefUser(asm.Name, asm.Version, asm.PublicKeyOrToken, asm.Culture)
		{
			Attributes = asm.Attributes
		};
	}

	public static TypeSig ToTypeSig(this ITypeDefOrRef type, bool resolveToCheckValueType = true)
	{
		if (type == null)
		{
			return null;
		}
		ModuleDef module = type.Module;
		if (module != null)
		{
			CorLibTypeSig corLibTypeSig = module.CorLibTypes.GetCorLibTypeSig(type);
			if (corLibTypeSig != null)
			{
				return corLibTypeSig;
			}
		}
		TypeDef typeDef = type as TypeDef;
		if (typeDef != null)
		{
			return CreateClassOrValueType(type, typeDef.IsValueType);
		}
		if (type is TypeRef typeRef)
		{
			if (resolveToCheckValueType)
			{
				typeDef = typeRef.Resolve();
			}
			return CreateClassOrValueType(type, typeDef?.IsValueType ?? false);
		}
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig;
		}
		return null;
	}

	private static TypeSig CreateClassOrValueType(ITypeDefOrRef type, bool isValueType)
	{
		if (isValueType)
		{
			return new ValueTypeSig(type);
		}
		return new ClassSig(type);
	}

	public static TypeDefOrRefSig TryGetTypeDefOrRefSig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as TypeDefOrRefSig;
		}
		return null;
	}

	public static ClassOrValueTypeSig TryGetClassOrValueTypeSig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as ClassOrValueTypeSig;
		}
		return null;
	}

	public static ValueTypeSig TryGetValueTypeSig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as ValueTypeSig;
		}
		return null;
	}

	public static ClassSig TryGetClassSig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as ClassSig;
		}
		return null;
	}

	public static GenericSig TryGetGenericSig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as GenericSig;
		}
		return null;
	}

	public static GenericVar TryGetGenericVar(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as GenericVar;
		}
		return null;
	}

	public static GenericMVar TryGetGenericMVar(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as GenericMVar;
		}
		return null;
	}

	public static GenericInstSig TryGetGenericInstSig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as GenericInstSig;
		}
		return null;
	}

	public static PtrSig TryGetPtrSig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as PtrSig;
		}
		return null;
	}

	public static ByRefSig TryGetByRefSig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as ByRefSig;
		}
		return null;
	}

	public static ArraySig TryGetArraySig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as ArraySig;
		}
		return null;
	}

	public static SZArraySig TryGetSZArraySig(this ITypeDefOrRef type)
	{
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig.RemovePinnedAndModifiers() as SZArraySig;
		}
		return null;
	}

	public static ITypeDefOrRef GetBaseTypeThrow(this ITypeDefOrRef tdr)
	{
		return tdr.GetBaseType(throwOnResolveFailure: true);
	}

	public static ITypeDefOrRef GetBaseType(this ITypeDefOrRef tdr, bool throwOnResolveFailure = false)
	{
		if (tdr is TypeDef typeDef)
		{
			return typeDef.BaseType;
		}
		if (tdr is TypeRef typeRef)
		{
			return (throwOnResolveFailure ? typeRef.ResolveThrow() : typeRef.Resolve())?.BaseType;
		}
		if (!(tdr is TypeSpec typeSpec))
		{
			return null;
		}
		GenericInstSig genericInstSig = typeSpec.TypeSig.ToGenericInstSig();
		tdr = ((genericInstSig == null) ? typeSpec.TypeSig.ToTypeDefOrRefSig()?.TypeDefOrRef : genericInstSig.GenericType?.TypeDefOrRef);
		if (tdr is TypeDef typeDef2)
		{
			return typeDef2.BaseType;
		}
		if (tdr is TypeRef typeRef2)
		{
			return (throwOnResolveFailure ? typeRef2.ResolveThrow() : typeRef2.Resolve())?.BaseType;
		}
		return null;
	}

	public static TypeDef ResolveTypeDef(this ITypeDefOrRef tdr)
	{
		if (tdr is TypeDef result)
		{
			return result;
		}
		if (tdr is TypeRef typeRef)
		{
			return typeRef.Resolve();
		}
		if (tdr == null)
		{
			return null;
		}
		tdr = tdr.ScopeType;
		if (tdr is TypeDef result2)
		{
			return result2;
		}
		if (tdr is TypeRef typeRef2)
		{
			return typeRef2.Resolve();
		}
		return null;
	}

	public static TypeDef ResolveTypeDefThrow(this ITypeDefOrRef tdr)
	{
		if (tdr is TypeDef result)
		{
			return result;
		}
		if (tdr is TypeRef typeRef)
		{
			return typeRef.ResolveThrow();
		}
		if (tdr == null)
		{
			throw new TypeResolveException("Can't resolve a null pointer");
		}
		tdr = tdr.ScopeType;
		if (tdr is TypeDef result2)
		{
			return result2;
		}
		if (tdr is TypeRef typeRef2)
		{
			return typeRef2.ResolveThrow();
		}
		throw new TypeResolveException($"Could not resolve type: {tdr} ({tdr?.DefinitionAssembly})");
	}

	public static FieldDef ResolveFieldDef(this IField field)
	{
		if (field is FieldDef result)
		{
			return result;
		}
		if (field is MemberRef memberRef)
		{
			return memberRef.ResolveField();
		}
		return null;
	}

	public static FieldDef ResolveFieldDefThrow(this IField field)
	{
		if (field is FieldDef result)
		{
			return result;
		}
		if (field is MemberRef memberRef)
		{
			return memberRef.ResolveFieldThrow();
		}
		throw new MemberRefResolveException($"Could not resolve field: {field}");
	}

	public static MethodDef ResolveMethodDef(this IMethod method)
	{
		if (method is MethodDef result)
		{
			return result;
		}
		if (method is MemberRef memberRef)
		{
			return memberRef.ResolveMethod();
		}
		if (method is MethodSpec methodSpec)
		{
			if (methodSpec.Method is MethodDef result2)
			{
				return result2;
			}
			if (methodSpec.Method is MemberRef memberRef2)
			{
				return memberRef2.ResolveMethod();
			}
		}
		return null;
	}

	public static MethodDef ResolveMethodDefThrow(this IMethod method)
	{
		if (method is MethodDef result)
		{
			return result;
		}
		if (method is MemberRef memberRef)
		{
			return memberRef.ResolveMethodThrow();
		}
		if (method is MethodSpec methodSpec)
		{
			if (methodSpec.Method is MethodDef result2)
			{
				return result2;
			}
			if (methodSpec.Method is MemberRef memberRef2)
			{
				return memberRef2.ResolveMethodThrow();
			}
		}
		throw new MemberRefResolveException($"Could not resolve method: {method}");
	}

	internal static IAssembly GetDefinitionAssembly(this MemberRef mr)
	{
		if (mr == null)
		{
			return null;
		}
		IMemberRefParent @class = mr.Class;
		if (@class is ITypeDefOrRef typeDefOrRef)
		{
			return typeDefOrRef.DefinitionAssembly;
		}
		if (@class is ModuleRef)
		{
			return mr.Module?.Assembly;
		}
		if (@class is MethodDef methodDef)
		{
			return methodDef.DeclaringType?.DefinitionAssembly;
		}
		return null;
	}

	public static IList<TypeSig> GetParams(this IMethod method)
	{
		return method?.MethodSig.GetParams();
	}

	public static int GetParamCount(this IMethod method)
	{
		return method?.MethodSig.GetParamCount() ?? 0;
	}

	public static bool HasParams(this IMethod method)
	{
		return method.GetParamCount() > 0;
	}

	public static TypeSig GetParam(this IMethod method, int index)
	{
		IList<TypeSig> list = method?.MethodSig.GetParams();
		if (list == null || index < 0 || index >= list.Count)
		{
			return null;
		}
		return list[index];
	}

	public static ITypeDefOrRef ToTypeDefOrRef(this TypeSig sig)
	{
		if (sig == null)
		{
			return null;
		}
		if (sig is TypeDefOrRefSig typeDefOrRefSig)
		{
			return typeDefOrRefSig.TypeDefOrRef;
		}
		ModuleDef module = sig.Module;
		if (module == null)
		{
			return new TypeSpecUser(sig);
		}
		return module.UpdateRowId(new TypeSpecUser(sig));
	}

	internal static bool IsPrimitive(this IType tdr)
	{
		if (tdr == null)
		{
			return false;
		}
		if (!tdr.DefinitionAssembly.IsCorLib())
		{
			return false;
		}
		switch (tdr.FullName)
		{
		case "System.Boolean":
		case "System.UIntPtr":
		case "System.Byte":
		case "System.Char":
		case "System.Int64":
		case "System.SByte":
		case "System.Int16":
		case "System.Int32":
		case "System.UInt64":
		case "System.Single":
		case "System.Double":
		case "System.IntPtr":
		case "System.UInt16":
		case "System.UInt32":
			return true;
		default:
			return false;
		}
	}

	public static CorLibTypeSig GetCorLibTypeSig(this ICorLibTypes self, ITypeDefOrRef type)
	{
		CorLibTypeSig corLibTypeSig;
		if (type is TypeDef { DeclaringType: null } typeDef && (corLibTypeSig = self.GetCorLibTypeSig(typeDef.Namespace, typeDef.Name, typeDef.DefinitionAssembly)) != null)
		{
			return corLibTypeSig;
		}
		if (type is TypeRef typeRef && !(typeRef.ResolutionScope is TypeRef) && (corLibTypeSig = self.GetCorLibTypeSig(typeRef.Namespace, typeRef.Name, typeRef.DefinitionAssembly)) != null)
		{
			return corLibTypeSig;
		}
		return null;
	}

	public static CorLibTypeSig GetCorLibTypeSig(this ICorLibTypes self, UTF8String @namespace, UTF8String name, IAssembly defAsm)
	{
		return self.GetCorLibTypeSig(UTF8String.ToSystemStringOrEmpty(@namespace), UTF8String.ToSystemStringOrEmpty(name), defAsm);
	}

	public static CorLibTypeSig GetCorLibTypeSig(this ICorLibTypes self, string @namespace, string name, IAssembly defAsm)
	{
		if (@namespace != "System")
		{
			return null;
		}
		if (defAsm == null || !defAsm.IsCorLib())
		{
			return null;
		}
		return name switch
		{
			"Void" => self.Void, 
			"Boolean" => self.Boolean, 
			"Char" => self.Char, 
			"SByte" => self.SByte, 
			"Byte" => self.Byte, 
			"Int16" => self.Int16, 
			"UInt16" => self.UInt16, 
			"Int32" => self.Int32, 
			"UInt32" => self.UInt32, 
			"Int64" => self.Int64, 
			"UInt64" => self.UInt64, 
			"Single" => self.Single, 
			"Double" => self.Double, 
			"String" => self.String, 
			"TypedReference" => self.TypedReference, 
			"IntPtr" => self.IntPtr, 
			"UIntPtr" => self.UIntPtr, 
			"Object" => self.Object, 
			_ => null, 
		};
	}

	public static void Error(this ILogger logger, object sender, string message)
	{
		logger.Log(sender, LoggerEvent.Error, "{0}", message);
	}

	public static void Error(this ILogger logger, object sender, string message, object arg1)
	{
		logger.Log(sender, LoggerEvent.Error, message, arg1);
	}

	public static void Error(this ILogger logger, object sender, string message, object arg1, object arg2)
	{
		logger.Log(sender, LoggerEvent.Error, message, arg1, arg2);
	}

	public static void Error(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3)
	{
		logger.Log(sender, LoggerEvent.Error, message, arg1, arg2, arg3);
	}

	public static void Error(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3, object arg4)
	{
		logger.Log(sender, LoggerEvent.Error, message, arg1, arg2, arg3, arg4);
	}

	public static void Error(this ILogger logger, object sender, string message, params object[] args)
	{
		logger.Log(sender, LoggerEvent.Error, message, args);
	}

	public static void Warning(this ILogger logger, object sender, string message)
	{
		logger.Log(sender, LoggerEvent.Warning, "{0}", message);
	}

	public static void Warning(this ILogger logger, object sender, string message, object arg1)
	{
		logger.Log(sender, LoggerEvent.Warning, message, arg1);
	}

	public static void Warning(this ILogger logger, object sender, string message, object arg1, object arg2)
	{
		logger.Log(sender, LoggerEvent.Warning, message, arg1, arg2);
	}

	public static void Warning(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3)
	{
		logger.Log(sender, LoggerEvent.Warning, message, arg1, arg2, arg3);
	}

	public static void Warning(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3, object arg4)
	{
		logger.Log(sender, LoggerEvent.Warning, message, arg1, arg2, arg3, arg4);
	}

	public static void Warning(this ILogger logger, object sender, string message, params object[] args)
	{
		logger.Log(sender, LoggerEvent.Warning, message, args);
	}

	public static void Info(this ILogger logger, object sender, string message)
	{
		logger.Log(sender, LoggerEvent.Info, "{0}", message);
	}

	public static void Info(this ILogger logger, object sender, string message, object arg1)
	{
		logger.Log(sender, LoggerEvent.Info, message, arg1);
	}

	public static void Info(this ILogger logger, object sender, string message, object arg1, object arg2)
	{
		logger.Log(sender, LoggerEvent.Info, message, arg1, arg2);
	}

	public static void Info(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3)
	{
		logger.Log(sender, LoggerEvent.Info, message, arg1, arg2, arg3);
	}

	public static void Info(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3, object arg4)
	{
		logger.Log(sender, LoggerEvent.Info, message, arg1, arg2, arg3, arg4);
	}

	public static void Info(this ILogger logger, object sender, string message, params object[] args)
	{
		logger.Log(sender, LoggerEvent.Info, message, args);
	}

	public static void Verbose(this ILogger logger, object sender, string message)
	{
		logger.Log(sender, LoggerEvent.Verbose, "{0}", message);
	}

	public static void Verbose(this ILogger logger, object sender, string message, object arg1)
	{
		logger.Log(sender, LoggerEvent.Verbose, message, arg1);
	}

	public static void Verbose(this ILogger logger, object sender, string message, object arg1, object arg2)
	{
		logger.Log(sender, LoggerEvent.Verbose, message, arg1, arg2);
	}

	public static void Verbose(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3)
	{
		logger.Log(sender, LoggerEvent.Verbose, message, arg1, arg2, arg3);
	}

	public static void Verbose(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3, object arg4)
	{
		logger.Log(sender, LoggerEvent.Verbose, message, arg1, arg2, arg3, arg4);
	}

	public static void Verbose(this ILogger logger, object sender, string message, params object[] args)
	{
		logger.Log(sender, LoggerEvent.Verbose, message, args);
	}

	public static void VeryVerbose(this ILogger logger, object sender, string message)
	{
		logger.Log(sender, LoggerEvent.VeryVerbose, "{0}", message);
	}

	public static void VeryVerbose(this ILogger logger, object sender, string message, object arg1)
	{
		logger.Log(sender, LoggerEvent.VeryVerbose, message, arg1);
	}

	public static void VeryVerbose(this ILogger logger, object sender, string message, object arg1, object arg2)
	{
		logger.Log(sender, LoggerEvent.VeryVerbose, message, arg1, arg2);
	}

	public static void VeryVerbose(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3)
	{
		logger.Log(sender, LoggerEvent.VeryVerbose, message, arg1, arg2, arg3);
	}

	public static void VeryVerbose(this ILogger logger, object sender, string message, object arg1, object arg2, object arg3, object arg4)
	{
		logger.Log(sender, LoggerEvent.VeryVerbose, message, arg1, arg2, arg3, arg4);
	}

	public static void VeryVerbose(this ILogger logger, object sender, string message, params object[] args)
	{
		logger.Log(sender, LoggerEvent.VeryVerbose, message, args);
	}

	public static TypeDef Resolve(this ITypeResolver self, TypeRef typeRef)
	{
		return self.Resolve(typeRef, null);
	}

	public static TypeDef ResolveThrow(this ITypeResolver self, TypeRef typeRef)
	{
		return self.ResolveThrow(typeRef, null);
	}

	public static TypeDef ResolveThrow(this ITypeResolver self, TypeRef typeRef, ModuleDef sourceModule)
	{
		TypeDef typeDef = self.Resolve(typeRef, sourceModule);
		if (typeDef != null)
		{
			return typeDef;
		}
		throw new TypeResolveException($"Could not resolve type: {typeRef} ({typeRef?.DefinitionAssembly})");
	}

	public static IMemberForwarded ResolveThrow(this IMemberRefResolver self, MemberRef memberRef)
	{
		IMemberForwarded memberForwarded = self.Resolve(memberRef);
		if (memberForwarded != null)
		{
			return memberForwarded;
		}
		throw new MemberRefResolveException($"Could not resolve method/field: {memberRef} ({memberRef?.GetDefinitionAssembly()})");
	}

	public static FieldDef ResolveField(this IMemberRefResolver self, MemberRef memberRef)
	{
		return self.Resolve(memberRef) as FieldDef;
	}

	public static FieldDef ResolveFieldThrow(this IMemberRefResolver self, MemberRef memberRef)
	{
		if (self.Resolve(memberRef) is FieldDef result)
		{
			return result;
		}
		throw new MemberRefResolveException($"Could not resolve field: {memberRef} ({memberRef?.GetDefinitionAssembly()})");
	}

	public static MethodDef ResolveMethod(this IMemberRefResolver self, MemberRef memberRef)
	{
		return self.Resolve(memberRef) as MethodDef;
	}

	public static MethodDef ResolveMethodThrow(this IMemberRefResolver self, MemberRef memberRef)
	{
		if (self.Resolve(memberRef) is MethodDef result)
		{
			return result;
		}
		throw new MemberRefResolveException($"Could not resolve method: {memberRef} ({memberRef?.GetDefinitionAssembly()})");
	}

	public static IMDTokenProvider ResolveToken(this ITokenResolver self, uint token)
	{
		return self.ResolveToken(token, default(GenericParamContext));
	}

	public static ITypeDefOrRef GetNonNestedTypeRefScope(this IType type)
	{
		if (type == null)
		{
			return null;
		}
		ITypeDefOrRef scopeType = type.ScopeType;
		TypeRef typeRef = scopeType as TypeRef;
		if (typeRef == null)
		{
			return scopeType;
		}
		for (int i = 0; i < 100; i++)
		{
			if (!(typeRef.ResolutionScope is TypeRef typeRef2))
			{
				return typeRef;
			}
			typeRef = typeRef2;
		}
		return typeRef;
	}

	public static TypeDef FindThrow(this ITypeDefFinder self, TypeRef typeRef)
	{
		TypeDef typeDef = self.Find(typeRef);
		if (typeDef != null)
		{
			return typeDef;
		}
		throw new TypeResolveException($"Could not find type: {typeRef}");
	}

	public static TypeDef FindThrow(this ITypeDefFinder self, string fullName, bool isReflectionName)
	{
		TypeDef typeDef = self.Find(fullName, isReflectionName);
		if (typeDef != null)
		{
			return typeDef;
		}
		throw new TypeResolveException("Could not find type: " + fullName);
	}

	public static TypeDef FindNormal(this ITypeDefFinder self, string fullName)
	{
		return self.Find(fullName, isReflectionName: false);
	}

	public static TypeDef FindNormalThrow(this ITypeDefFinder self, string fullName)
	{
		TypeDef typeDef = self.Find(fullName, isReflectionName: false);
		if (typeDef != null)
		{
			return typeDef;
		}
		throw new TypeResolveException("Could not find type: " + fullName);
	}

	public static TypeDef FindReflection(this ITypeDefFinder self, string fullName)
	{
		return self.Find(fullName, isReflectionName: true);
	}

	public static TypeDef FindReflectionThrow(this ITypeDefFinder self, string fullName)
	{
		TypeDef typeDef = self.Find(fullName, isReflectionName: true);
		if (typeDef != null)
		{
			return typeDef;
		}
		throw new TypeResolveException("Could not find type: " + fullName);
	}

	public static bool TypeExists(this ITypeDefFinder self, TypeRef typeRef)
	{
		return self.Find(typeRef) != null;
	}

	public static bool TypeExists(this ITypeDefFinder self, string fullName, bool isReflectionName)
	{
		return self.Find(fullName, isReflectionName) != null;
	}

	public static bool TypeExistsNormal(this ITypeDefFinder self, string fullName)
	{
		return self.Find(fullName, isReflectionName: false) != null;
	}

	public static bool TypeExistsReflection(this ITypeDefFinder self, string fullName)
	{
		return self.Find(fullName, isReflectionName: true) != null;
	}

	public static TypeSig RemoveModifiers(this TypeSig a)
	{
		if (a == null)
		{
			return null;
		}
		while (a is ModifierSig)
		{
			a = a.Next;
		}
		return a;
	}

	public static TypeSig RemovePinned(this TypeSig a)
	{
		if (!(a is PinnedSig pinnedSig))
		{
			return a;
		}
		return pinnedSig.Next;
	}

	public static TypeSig RemovePinnedAndModifiers(this TypeSig a)
	{
		a = a.RemoveModifiers();
		a = a.RemovePinned();
		a = a.RemoveModifiers();
		return a;
	}

	public static TypeDefOrRefSig ToTypeDefOrRefSig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as TypeDefOrRefSig;
	}

	public static ClassOrValueTypeSig ToClassOrValueTypeSig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as ClassOrValueTypeSig;
	}

	public static ValueTypeSig ToValueTypeSig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as ValueTypeSig;
	}

	public static ClassSig ToClassSig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as ClassSig;
	}

	public static GenericSig ToGenericSig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as GenericSig;
	}

	public static GenericVar ToGenericVar(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as GenericVar;
	}

	public static GenericMVar ToGenericMVar(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as GenericMVar;
	}

	public static GenericInstSig ToGenericInstSig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as GenericInstSig;
	}

	public static PtrSig ToPtrSig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as PtrSig;
	}

	public static ByRefSig ToByRefSig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as ByRefSig;
	}

	public static ArraySig ToArraySig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as ArraySig;
	}

	public static SZArraySig ToSZArraySig(this TypeSig type)
	{
		return type.RemovePinnedAndModifiers() as SZArraySig;
	}

	public static TypeSig GetNext(this TypeSig self)
	{
		return self?.Next;
	}

	public static bool GetIsValueType(this TypeSig self)
	{
		return self?.IsValueType ?? false;
	}

	public static bool GetIsPrimitive(this TypeSig self)
	{
		return self?.IsPrimitive ?? false;
	}

	public static ElementType GetElementType(this TypeSig a)
	{
		return a?.ElementType ?? ElementType.End;
	}

	public static string GetFullName(this TypeSig a)
	{
		if (a != null)
		{
			return a.FullName;
		}
		return string.Empty;
	}

	public static string GetName(this TypeSig a)
	{
		if (a != null)
		{
			return a.TypeName;
		}
		return string.Empty;
	}

	public static string GetNamespace(this TypeSig a)
	{
		if (a != null)
		{
			return a.Namespace;
		}
		return string.Empty;
	}

	public static ITypeDefOrRef TryGetTypeDefOrRef(this TypeSig a)
	{
		return (a.RemovePinnedAndModifiers() as TypeDefOrRefSig)?.TypeDefOrRef;
	}

	public static TypeRef TryGetTypeRef(this TypeSig a)
	{
		return (a.RemovePinnedAndModifiers() as TypeDefOrRefSig)?.TypeRef;
	}

	public static TypeDef TryGetTypeDef(this TypeSig a)
	{
		return (a.RemovePinnedAndModifiers() as TypeDefOrRefSig)?.TypeDef;
	}

	public static TypeSpec TryGetTypeSpec(this TypeSig a)
	{
		return (a.RemovePinnedAndModifiers() as TypeDefOrRefSig)?.TypeSpec;
	}
}
