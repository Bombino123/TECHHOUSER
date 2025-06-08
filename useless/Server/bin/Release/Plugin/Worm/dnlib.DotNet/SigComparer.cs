using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public struct SigComparer
{
	private const SigComparerOptions SigComparerOptions_DontSubstituteGenericParameters = (SigComparerOptions)1024u;

	private const int HASHCODE_MAGIC_GLOBAL_TYPE = 1654396648;

	private const int HASHCODE_MAGIC_NESTED_TYPE = -1049070942;

	private const int HASHCODE_MAGIC_ET_MODULE = -299744851;

	private const int HASHCODE_MAGIC_ET_VALUEARRAY = -674970533;

	private const int HASHCODE_MAGIC_ET_GENERICINST = -2050514639;

	private const int HASHCODE_MAGIC_ET_VAR = 1288450097;

	private const int HASHCODE_MAGIC_ET_MVAR = -990598495;

	private const int HASHCODE_MAGIC_ET_ARRAY = -96331531;

	private const int HASHCODE_MAGIC_ET_SZARRAY = 871833535;

	private const int HASHCODE_MAGIC_ET_BYREF = -634749586;

	private const int HASHCODE_MAGIC_ET_PTR = 1976400808;

	private const int HASHCODE_MAGIC_ET_SENTINEL = 68439620;

	private RecursionCounter recursionCounter;

	private SigComparerOptions options;

	private GenericArguments genericArguments;

	private readonly ModuleDef sourceModule;

	private bool DontCompareTypeScope => (options & SigComparerOptions.DontCompareTypeScope) != 0;

	private bool CompareMethodFieldDeclaringType => (options & SigComparerOptions.CompareMethodFieldDeclaringType) != 0;

	private bool ComparePropertyDeclaringType => (options & SigComparerOptions.ComparePropertyDeclaringType) != 0;

	private bool CompareEventDeclaringType => (options & SigComparerOptions.CompareEventDeclaringType) != 0;

	private bool CompareSentinelParams => (options & SigComparerOptions.CompareSentinelParams) != 0;

	private bool CompareAssemblyPublicKeyToken => (options & SigComparerOptions.CompareAssemblyPublicKeyToken) != 0;

	private bool CompareAssemblyVersion => (options & SigComparerOptions.CompareAssemblyVersion) != 0;

	private bool CompareAssemblyLocale => (options & SigComparerOptions.CompareAssemblyLocale) != 0;

	private bool TypeRefCanReferenceGlobalType => (options & SigComparerOptions.TypeRefCanReferenceGlobalType) != 0;

	private bool DontCompareReturnType => (options & SigComparerOptions.DontCompareReturnType) != 0;

	private bool DontSubstituteGenericParameters => (options & (SigComparerOptions)1024u) != 0;

	private bool CaseInsensitiveTypeNamespaces => (options & SigComparerOptions.CaseInsensitiveTypeNamespaces) != 0;

	private bool CaseInsensitiveTypeNames => (options & SigComparerOptions.CaseInsensitiveTypeNames) != 0;

	private bool CaseInsensitiveMethodFieldNames => (options & SigComparerOptions.CaseInsensitiveMethodFieldNames) != 0;

	private bool CaseInsensitivePropertyNames => (options & SigComparerOptions.CaseInsensitivePropertyNames) != 0;

	private bool CaseInsensitiveEventNames => (options & SigComparerOptions.CaseInsensitiveEventNames) != 0;

	private bool PrivateScopeFieldIsComparable => (options & SigComparerOptions.PrivateScopeFieldIsComparable) != 0;

	private bool PrivateScopeMethodIsComparable => (options & SigComparerOptions.PrivateScopeMethodIsComparable) != 0;

	private bool RawSignatureCompare => (options & SigComparerOptions.RawSignatureCompare) != 0;

	private bool IgnoreModifiers => (options & SigComparerOptions.IgnoreModifiers) != 0;

	private bool MscorlibIsNotSpecial => (options & SigComparerOptions.MscorlibIsNotSpecial) != 0;

	private bool DontProjectWinMDRefs => (options & SigComparerOptions.DontProjectWinMDRefs) != 0;

	private bool DontCheckTypeEquivalence => (options & SigComparerOptions.DontCheckTypeEquivalence) != 0;

	private bool IgnoreMultiDimensionalArrayLowerBoundsAndSizes => (options & SigComparerOptions.IgnoreMultiDimensionalArrayLowerBoundsAndSizes) != 0;

	private bool ReferenceCompareForMemberDefsInSameModule => (options & SigComparerOptions.ReferenceCompareForMemberDefsInSameModule) != 0;

	public SigComparer(SigComparerOptions options)
		: this(options, null)
	{
	}

	public SigComparer(SigComparerOptions options, ModuleDef sourceModule)
	{
		recursionCounter = default(RecursionCounter);
		this.options = options;
		genericArguments = null;
		this.sourceModule = sourceModule;
	}

	private int GetHashCode_FnPtr_SystemIntPtr()
	{
		return GetHashCode_TypeNamespace("System") + GetHashCode_TypeName("IntPtr");
	}

	private bool Equals_Names(bool caseInsensitive, UTF8String a, UTF8String b)
	{
		if (caseInsensitive)
		{
			return UTF8String.ToSystemStringOrEmpty(a).Equals(UTF8String.ToSystemStringOrEmpty(b), StringComparison.OrdinalIgnoreCase);
		}
		return UTF8String.Equals(a, b);
	}

	private bool Equals_Names(bool caseInsensitive, string a, string b)
	{
		if (caseInsensitive)
		{
			return (a ?? string.Empty).Equals(b ?? string.Empty, StringComparison.OrdinalIgnoreCase);
		}
		return (a ?? string.Empty) == (b ?? string.Empty);
	}

	private int GetHashCode_Name(bool caseInsensitive, string a)
	{
		if (caseInsensitive)
		{
			return (a ?? string.Empty).ToUpperInvariant().GetHashCode();
		}
		return (a ?? string.Empty).GetHashCode();
	}

	private bool Equals_TypeNamespaces(UTF8String a, UTF8String b)
	{
		return Equals_Names(CaseInsensitiveTypeNamespaces, a, b);
	}

	private bool Equals_TypeNamespaces(UTF8String a, string b)
	{
		return Equals_Names(CaseInsensitiveTypeNamespaces, UTF8String.ToSystemStringOrEmpty(a), b);
	}

	private int GetHashCode_TypeNamespace(UTF8String a)
	{
		return GetHashCode_Name(CaseInsensitiveTypeNamespaces, UTF8String.ToSystemStringOrEmpty(a));
	}

	private int GetHashCode_TypeNamespace(string a)
	{
		return GetHashCode_Name(CaseInsensitiveTypeNamespaces, a);
	}

	private bool Equals_TypeNames(UTF8String a, UTF8String b)
	{
		return Equals_Names(CaseInsensitiveTypeNames, a, b);
	}

	private bool Equals_TypeNames(UTF8String a, string b)
	{
		return Equals_Names(CaseInsensitiveTypeNames, UTF8String.ToSystemStringOrEmpty(a), b);
	}

	private int GetHashCode_TypeName(UTF8String a)
	{
		return GetHashCode_Name(CaseInsensitiveTypeNames, UTF8String.ToSystemStringOrEmpty(a));
	}

	private int GetHashCode_TypeName(string a)
	{
		return GetHashCode_Name(CaseInsensitiveTypeNames, a);
	}

	private bool Equals_MethodFieldNames(UTF8String a, UTF8String b)
	{
		return Equals_Names(CaseInsensitiveMethodFieldNames, a, b);
	}

	private bool Equals_MethodFieldNames(UTF8String a, string b)
	{
		return Equals_Names(CaseInsensitiveMethodFieldNames, UTF8String.ToSystemStringOrEmpty(a), b);
	}

	private int GetHashCode_MethodFieldName(UTF8String a)
	{
		return GetHashCode_Name(CaseInsensitiveMethodFieldNames, UTF8String.ToSystemStringOrEmpty(a));
	}

	private int GetHashCode_MethodFieldName(string a)
	{
		return GetHashCode_Name(CaseInsensitiveMethodFieldNames, a);
	}

	private bool Equals_PropertyNames(UTF8String a, UTF8String b)
	{
		return Equals_Names(CaseInsensitivePropertyNames, a, b);
	}

	private bool Equals_PropertyNames(UTF8String a, string b)
	{
		return Equals_Names(CaseInsensitivePropertyNames, UTF8String.ToSystemStringOrEmpty(a), b);
	}

	private int GetHashCode_PropertyName(UTF8String a)
	{
		return GetHashCode_Name(CaseInsensitivePropertyNames, UTF8String.ToSystemStringOrEmpty(a));
	}

	private int GetHashCode_PropertyName(string a)
	{
		return GetHashCode_Name(CaseInsensitivePropertyNames, a);
	}

	private bool Equals_EventNames(UTF8String a, UTF8String b)
	{
		return Equals_Names(CaseInsensitiveEventNames, a, b);
	}

	private bool Equals_EventNames(UTF8String a, string b)
	{
		return Equals_Names(CaseInsensitiveEventNames, UTF8String.ToSystemStringOrEmpty(a), b);
	}

	private int GetHashCode_EventName(UTF8String a)
	{
		return GetHashCode_Name(CaseInsensitiveEventNames, UTF8String.ToSystemStringOrEmpty(a));
	}

	private int GetHashCode_EventName(string a)
	{
		return GetHashCode_Name(CaseInsensitiveEventNames, a);
	}

	private SigComparerOptions ClearOptions(SigComparerOptions flags)
	{
		SigComparerOptions result = options;
		options &= ~flags;
		return result;
	}

	private SigComparerOptions SetOptions(SigComparerOptions flags)
	{
		SigComparerOptions result = options;
		options |= flags;
		return result;
	}

	private void RestoreOptions(SigComparerOptions oldFlags)
	{
		options = oldFlags;
	}

	private void InitializeGenericArguments()
	{
		if (genericArguments == null)
		{
			genericArguments = new GenericArguments();
		}
	}

	private static GenericInstSig GetGenericInstanceType(IMemberRefParent parent)
	{
		if (!(parent is TypeSpec typeSpec))
		{
			return null;
		}
		return typeSpec.TypeSig.RemoveModifiers() as GenericInstSig;
	}

	private bool Equals(IAssembly aAsm, IAssembly bAsm, TypeRef b)
	{
		if (Equals(aAsm, bAsm))
		{
			return true;
		}
		TypeDef typeDef = b.Resolve(sourceModule);
		if (typeDef != null)
		{
			return Equals(aAsm, typeDef.Module.Assembly);
		}
		return false;
	}

	private bool Equals(IAssembly aAsm, IAssembly bAsm, ExportedType b)
	{
		if (Equals(aAsm, bAsm))
		{
			return true;
		}
		TypeDef typeDef = b.Resolve();
		if (typeDef != null)
		{
			return Equals(aAsm, typeDef.Module.Assembly);
		}
		return false;
	}

	private bool Equals(IAssembly aAsm, TypeRef a, IAssembly bAsm, TypeRef b)
	{
		if (Equals(aAsm, bAsm))
		{
			return true;
		}
		TypeDef typeDef = a.Resolve(sourceModule);
		TypeDef typeDef2 = b.Resolve(sourceModule);
		if (typeDef != null && typeDef2 != null)
		{
			return Equals(typeDef.Module.Assembly, typeDef2.Module.Assembly);
		}
		return false;
	}

	private bool Equals(IAssembly aAsm, ExportedType a, IAssembly bAsm, ExportedType b)
	{
		if (Equals(aAsm, bAsm))
		{
			return true;
		}
		TypeDef typeDef = a.Resolve();
		TypeDef typeDef2 = b.Resolve();
		if (typeDef != null && typeDef2 != null)
		{
			return Equals(typeDef.Module.Assembly, typeDef2.Module.Assembly);
		}
		return false;
	}

	private bool Equals(IAssembly aAsm, TypeRef a, IAssembly bAsm, ExportedType b)
	{
		if (Equals(aAsm, bAsm))
		{
			return true;
		}
		TypeDef typeDef = a.Resolve(sourceModule);
		TypeDef typeDef2 = b.Resolve();
		if (typeDef != null && typeDef2 != null)
		{
			return Equals(typeDef.Module.Assembly, typeDef2.Module.Assembly);
		}
		return false;
	}

	private bool Equals(TypeDef a, IModule bMod, TypeRef b)
	{
		if (Equals(a.Module, bMod) && Equals(a.DefinitionAssembly, b.DefinitionAssembly))
		{
			return true;
		}
		TypeDef typeDef = b.Resolve(sourceModule);
		if (typeDef == null)
		{
			return false;
		}
		if (!DontCheckTypeEquivalence && TIAHelper.Equivalent(a, typeDef))
		{
			return true;
		}
		if (Equals(a.Module, typeDef.Module))
		{
			return Equals(a.DefinitionAssembly, typeDef.DefinitionAssembly);
		}
		return false;
	}

	private bool Equals(TypeDef a, FileDef bFile, ExportedType b)
	{
		if (Equals(a.Module, bFile) && Equals(a.DefinitionAssembly, b.DefinitionAssembly))
		{
			return true;
		}
		TypeDef typeDef = b.Resolve();
		if (typeDef != null && Equals(a.Module, typeDef.Module))
		{
			return Equals(a.DefinitionAssembly, typeDef.DefinitionAssembly);
		}
		return false;
	}

	private bool TypeDefScopeEquals(TypeDef a, TypeDef b)
	{
		if (a == null || b == null)
		{
			return false;
		}
		if (!DontCheckTypeEquivalence && TIAHelper.Equivalent(a, b))
		{
			return true;
		}
		return Equals(a.Module, b.Module);
	}

	private bool Equals(TypeRef a, IModule ma, TypeRef b, IModule mb)
	{
		if (Equals(ma, mb) && Equals(a.DefinitionAssembly, b.DefinitionAssembly))
		{
			return true;
		}
		TypeDef typeDef = a.Resolve(sourceModule);
		TypeDef typeDef2 = b.Resolve(sourceModule);
		if (typeDef != null && typeDef2 != null && Equals(typeDef.Module, typeDef2.Module))
		{
			return Equals(typeDef.DefinitionAssembly, typeDef2.DefinitionAssembly);
		}
		return false;
	}

	private bool Equals(TypeRef a, IModule ma, ExportedType b, FileDef fb)
	{
		if (Equals(ma, fb) && Equals(a.DefinitionAssembly, b.DefinitionAssembly))
		{
			return true;
		}
		TypeDef typeDef = a.Resolve(sourceModule);
		TypeDef typeDef2 = b.Resolve();
		if (typeDef != null && typeDef2 != null && Equals(typeDef.Module, typeDef2.Module))
		{
			return Equals(typeDef.DefinitionAssembly, typeDef2.DefinitionAssembly);
		}
		return false;
	}

	private bool Equals(Assembly aAsm, IAssembly bAsm, TypeRef b)
	{
		if (Equals(bAsm, aAsm))
		{
			return true;
		}
		TypeDef typeDef = b.Resolve(sourceModule);
		if (typeDef != null)
		{
			return Equals(typeDef.Module.Assembly, aAsm);
		}
		return false;
	}

	private bool Equals(Assembly aAsm, IAssembly bAsm, ExportedType b)
	{
		if (Equals(bAsm, aAsm))
		{
			return true;
		}
		TypeDef typeDef = b.Resolve();
		if (typeDef != null)
		{
			return Equals(typeDef.Module.Assembly, aAsm);
		}
		return false;
	}

	private bool Equals(Type a, IModule bMod, TypeRef b)
	{
		if (Equals(bMod, a.Module) && Equals(b.DefinitionAssembly, a.Assembly))
		{
			return true;
		}
		TypeDef typeDef = b.Resolve(sourceModule);
		if (typeDef != null && Equals(typeDef.Module, a.Module))
		{
			return Equals(typeDef.DefinitionAssembly, a.Assembly);
		}
		return false;
	}

	private bool Equals(Type a, FileDef bFile, ExportedType b)
	{
		if (Equals(bFile, a.Module) && Equals(b.DefinitionAssembly, a.Assembly))
		{
			return true;
		}
		TypeDef typeDef = b.Resolve();
		if (typeDef != null && Equals(typeDef.Module, a.Module))
		{
			return Equals(typeDef.DefinitionAssembly, a.Assembly);
		}
		return false;
	}

	public bool Equals(IMemberRef a, IMemberRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((a is IType a2 && b is IType b2) ? Equals(a2, b2) : ((a is IField field && b is IField field2 && field.IsField && field2.IsField) ? Equals(field, field2) : ((a is IMethod a3 && b is IMethod b3) ? Equals(a3, b3) : ((a is PropertyDef a4 && b is PropertyDef b4) ? Equals(a4, b4) : (a is EventDef a5 && b is EventDef b5 && Equals(a5, b5))))));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(IMemberRef a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result = ((!(a is IType a2)) ? ((!(a is IField a3)) ? ((!(a is IMethod a4)) ? ((!(a is PropertyDef a5)) ? ((a is EventDef a6) ? GetHashCode(a6) : 0) : GetHashCode(a5)) : GetHashCode(a4)) : GetHashCode(a3)) : GetHashCode(a2));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(ITypeDefOrRef a, ITypeDefOrRef b)
	{
		return Equals((IType)a, (IType)b);
	}

	public int GetHashCode(ITypeDefOrRef a)
	{
		return GetHashCode((IType)a);
	}

	public bool Equals(IType a, IType b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		TypeDef typeDef;
		TypeDef typeDef2;
		TypeRef typeRef;
		TypeRef typeRef2;
		TypeSpec typeSpec;
		TypeSpec typeSpec2;
		TypeSig typeSig;
		TypeSig typeSig2;
		ExportedType exportedType;
		ExportedType exportedType2;
		bool result = ((((typeDef = a as TypeDef) != null) & ((typeDef2 = b as TypeDef) != null)) ? Equals(typeDef, typeDef2) : ((((typeRef = a as TypeRef) != null) & ((typeRef2 = b as TypeRef) != null)) ? Equals(typeRef, typeRef2) : ((((typeSpec = a as TypeSpec) != null) & ((typeSpec2 = b as TypeSpec) != null)) ? Equals(typeSpec, typeSpec2) : ((((typeSig = a as TypeSig) != null) & ((typeSig2 = b as TypeSig) != null)) ? Equals(typeSig, typeSig2) : ((((exportedType = a as ExportedType) != null) & ((exportedType2 = b as ExportedType) != null)) ? Equals(exportedType, exportedType2) : ((typeDef != null && typeRef2 != null) ? Equals(typeDef, typeRef2) : ((typeRef != null && typeDef2 != null) ? Equals(typeDef2, typeRef) : ((typeDef != null && typeSpec2 != null) ? Equals(typeDef, typeSpec2) : ((typeSpec != null && typeDef2 != null) ? Equals(typeDef2, typeSpec) : ((typeDef != null && typeSig2 != null) ? Equals(typeDef, typeSig2) : ((typeSig != null && typeDef2 != null) ? Equals(typeDef2, typeSig) : ((typeDef != null && exportedType2 != null) ? Equals(typeDef, exportedType2) : ((exportedType != null && typeDef2 != null) ? Equals(typeDef2, exportedType) : ((typeRef != null && typeSpec2 != null) ? Equals(typeRef, typeSpec2) : ((typeSpec != null && typeRef2 != null) ? Equals(typeRef2, typeSpec) : ((typeRef != null && typeSig2 != null) ? Equals(typeRef, typeSig2) : ((typeSig != null && typeRef2 != null) ? Equals(typeRef2, typeSig) : ((typeRef != null && exportedType2 != null) ? Equals(typeRef, exportedType2) : ((exportedType != null && typeRef2 != null) ? Equals(typeRef2, exportedType) : ((typeSpec != null && typeSig2 != null) ? Equals(typeSpec, typeSig2) : ((typeSig != null && typeSpec2 != null) ? Equals(typeSpec2, typeSig) : ((typeSpec != null && exportedType2 != null) ? Equals(typeSpec, exportedType2) : ((exportedType != null && typeSpec2 != null) ? Equals(typeSpec2, exportedType) : ((typeSig != null && exportedType2 != null) ? Equals(typeSig, exportedType2) : (exportedType != null && typeSig2 != null && Equals(typeSig2, exportedType))))))))))))))))))))))))));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(IType a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result = ((!(a is TypeDef a2)) ? ((!(a is TypeRef a3)) ? ((!(a is TypeSpec a4)) ? ((!(a is TypeSig a5)) ? ((a is ExportedType a6) ? GetHashCode(a6) : 0) : GetHashCode(a5)) : GetHashCode(a4)) : GetHashCode(a3)) : GetHashCode(a2));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(TypeRef a, TypeDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeDef a, TypeRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool flag;
		if (!DontProjectWinMDRefs)
		{
			TypeRef typeRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			b = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b) ?? b;
			if (typeRef != null)
			{
				flag = Equals(typeRef, b);
				goto IL_019f;
			}
		}
		IResolutionScope resolutionScope = b.ResolutionScope;
		if (!Equals_TypeNames(a.Name, b.Name) || !Equals_TypeNamespaces(a.Namespace, b.Namespace))
		{
			flag = false;
		}
		else if (resolutionScope is TypeRef b2)
		{
			flag = Equals(a.DeclaringType, b2);
		}
		else if (a.DeclaringType != null)
		{
			flag = false;
		}
		else if (DontCompareTypeScope)
		{
			flag = true;
		}
		else if (resolutionScope is IModule bMod)
		{
			flag = Equals(a, bMod, b);
		}
		else if (resolutionScope is AssemblyRef bAsm)
		{
			ModuleDef module = a.Module;
			flag = module != null && Equals(module.Assembly, bAsm, b);
			if (!flag && !DontCheckTypeEquivalence)
			{
				TypeDef b3 = b.Resolve();
				flag = TypeDefScopeEquals(a, b3);
			}
		}
		else
		{
			flag = false;
		}
		if (flag && !TypeRefCanReferenceGlobalType && a.IsGlobalModuleType)
		{
			flag = false;
		}
		goto IL_019f;
		IL_019f:
		recursionCounter.Decrement();
		return flag;
	}

	public bool Equals(ExportedType a, TypeDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeDef a, ExportedType b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool flag;
		if (!DontProjectWinMDRefs)
		{
			TypeRef typeRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			b = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b) ?? b;
			if (typeRef != null)
			{
				flag = Equals(typeRef, b);
				goto IL_019f;
			}
		}
		IImplementation implementation = b.Implementation;
		if (!Equals_TypeNames(a.Name, b.TypeName) || !Equals_TypeNamespaces(a.Namespace, b.TypeNamespace))
		{
			flag = false;
		}
		else if (implementation is ExportedType b2)
		{
			flag = Equals(a.DeclaringType, b2);
		}
		else if (a.DeclaringType != null)
		{
			flag = false;
		}
		else if (DontCompareTypeScope)
		{
			flag = true;
		}
		else
		{
			if (implementation is FileDef bFile)
			{
				flag = Equals(a, bFile, b);
			}
			else if (implementation is AssemblyRef bAsm)
			{
				ModuleDef module = a.Module;
				flag = module != null && Equals(module.Assembly, bAsm, b);
			}
			else
			{
				flag = false;
			}
			if (!flag && !DontCheckTypeEquivalence)
			{
				TypeDef b3 = b.Resolve();
				flag = TypeDefScopeEquals(a, b3);
			}
		}
		if (flag && !TypeRefCanReferenceGlobalType && a.IsGlobalModuleType)
		{
			flag = false;
		}
		goto IL_019f;
		IL_019f:
		recursionCounter.Decrement();
		return flag;
	}

	public bool Equals(TypeSpec a, TypeDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeDef a, TypeSpec b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return Equals(a, b.TypeSig);
	}

	public bool Equals(TypeSig a, TypeDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeDef a, TypeSig b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(b is TypeDefOrRefSig typeDefOrRefSig)) ? ((b is ModifierSig || b is PinnedSig) && Equals(a, b.Next)) : Equals((IType)a, (IType)typeDefOrRefSig.TypeDefOrRef));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(TypeSpec a, TypeRef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeRef a, TypeSpec b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return Equals(a, b.TypeSig);
	}

	public bool Equals(ExportedType a, TypeRef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeRef a, ExportedType b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
			b = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b) ?? b;
		}
		bool result = Equals_TypeNames(a.Name, b.TypeName) && Equals_TypeNamespaces(a.Namespace, b.TypeNamespace) && EqualsScope(a, b);
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(TypeSig a, TypeRef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeRef a, TypeSig b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(b is TypeDefOrRefSig typeDefOrRefSig)) ? ((b is ModifierSig || b is PinnedSig) && Equals(a, b.Next)) : Equals((IType)a, (IType)typeDefOrRefSig.TypeDefOrRef));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(TypeSig a, TypeSpec b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeSpec a, TypeSig b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return Equals(a.TypeSig, b);
	}

	public bool Equals(ExportedType a, TypeSpec b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeSpec a, ExportedType b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return Equals(a.TypeSig, b);
	}

	public bool Equals(ExportedType a, TypeSig b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeSig a, ExportedType b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(a is TypeDefOrRefSig typeDefOrRefSig)) ? ((a is ModifierSig || a is PinnedSig) && Equals(a.Next, b)) : Equals(typeDefOrRefSig.TypeDefOrRef, b));
		recursionCounter.Decrement();
		return result;
	}

	private int GetHashCodeGlobalType()
	{
		return 1654396648;
	}

	public bool Equals(TypeRef a, TypeRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
			b = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b) ?? b;
		}
		bool result = Equals_TypeNames(a.Name, b.Name) && Equals_TypeNamespaces(a.Namespace, b.Namespace) && EqualsResolutionScope(a, b);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(TypeRef a)
	{
		if (a == null)
		{
			if (!TypeRefCanReferenceGlobalType)
			{
				return 0;
			}
			return GetHashCodeGlobalType();
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
		}
		int hashCode_TypeName = GetHashCode_TypeName(a.Name);
		if (a.ResolutionScope is TypeRef)
		{
			return hashCode_TypeName + -1049070942;
		}
		return hashCode_TypeName + GetHashCode_TypeNamespace(a.Namespace);
	}

	public bool Equals(ExportedType a, ExportedType b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
			b = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b) ?? b;
		}
		bool result = Equals_TypeNames(a.TypeName, b.TypeName) && Equals_TypeNamespaces(a.TypeNamespace, b.TypeNamespace) && EqualsImplementation(a, b);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(ExportedType a)
	{
		if (a == null)
		{
			if (!TypeRefCanReferenceGlobalType)
			{
				return 0;
			}
			return GetHashCodeGlobalType();
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
		}
		int hashCode_TypeName = GetHashCode_TypeName(a.TypeName);
		if (a.Implementation is ExportedType)
		{
			return hashCode_TypeName + -1049070942;
		}
		return hashCode_TypeName + GetHashCode_TypeNamespace(a.TypeNamespace);
	}

	public bool Equals(TypeDef a, TypeDef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (ReferenceCompareForMemberDefsInSameModule && InSameModule(a, b))
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (!DontProjectWinMDRefs)
		{
			TypeRef typeRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			TypeRef typeRef2 = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b);
			if (typeRef != null || typeRef2 != null)
			{
				IType type = typeRef;
				IType a2 = type ?? a;
				type = typeRef2;
				result = Equals(a2, type ?? b);
				goto IL_0114;
			}
		}
		result = Equals_TypeNames(a.Name, b.Name) && Equals_TypeNamespaces(a.Namespace, b.Namespace) && Equals(a.DeclaringType, b.DeclaringType) && (DontCompareTypeScope || TypeDefScopeEquals(a, b));
		goto IL_0114;
		IL_0114:
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(TypeDef a)
	{
		if (a == null || a.IsGlobalModuleType)
		{
			return GetHashCodeGlobalType();
		}
		if (!DontProjectWinMDRefs)
		{
			TypeRef typeRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			if (typeRef != null)
			{
				return GetHashCode(typeRef);
			}
		}
		int hashCode_TypeName = GetHashCode_TypeName(a.Name);
		if (a.DeclaringType != null)
		{
			return hashCode_TypeName + -1049070942;
		}
		return hashCode_TypeName + GetHashCode_TypeNamespace(a.Namespace);
	}

	public bool Equals(TypeSpec a, TypeSpec b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals(a.TypeSig, b.TypeSig);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(TypeSpec a)
	{
		if (a == null)
		{
			return 0;
		}
		return GetHashCode(a.TypeSig);
	}

	private bool EqualsResolutionScope(TypeRef a, TypeRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		IResolutionScope resolutionScope = a.ResolutionScope;
		IResolutionScope resolutionScope2 = b.ResolutionScope;
		if (resolutionScope == resolutionScope2)
		{
			return true;
		}
		if (resolutionScope == null || resolutionScope2 == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool flag = true;
		TypeRef a2;
		TypeRef b2;
		bool flag2;
		IModule ma;
		IModule mb;
		AssemblyRef assemblyRef;
		AssemblyRef assemblyRef2;
		if (((a2 = resolutionScope as TypeRef) != null) | ((b2 = resolutionScope2 as TypeRef) != null))
		{
			flag2 = Equals(a2, b2);
			flag = false;
		}
		else if (DontCompareTypeScope)
		{
			flag2 = true;
		}
		else if (((ma = resolutionScope as IModule) != null) & ((mb = resolutionScope2 as IModule) != null))
		{
			flag2 = Equals(a, ma, b, mb);
		}
		else if (((assemblyRef = resolutionScope as AssemblyRef) != null) & ((assemblyRef2 = resolutionScope2 as AssemblyRef) != null))
		{
			flag2 = Equals(assemblyRef, a, assemblyRef2, b);
		}
		else if (assemblyRef != null && resolutionScope2 is ModuleRef)
		{
			ModuleDef module = b.Module;
			flag2 = module != null && Equals(module.Assembly, b, assemblyRef, a);
		}
		else if (assemblyRef2 != null && resolutionScope is ModuleRef)
		{
			ModuleDef module2 = a.Module;
			flag2 = module2 != null && Equals(module2.Assembly, a, assemblyRef2, b);
		}
		else if (assemblyRef != null && resolutionScope2 is ModuleDef moduleDef)
		{
			flag2 = Equals(moduleDef.Assembly, assemblyRef, a);
		}
		else if (assemblyRef2 != null && resolutionScope is ModuleDef moduleDef2)
		{
			flag2 = Equals(moduleDef2.Assembly, assemblyRef2, b);
		}
		else
		{
			flag2 = false;
			flag = false;
		}
		if (!flag2 && flag && !DontCheckTypeEquivalence)
		{
			TypeDef typeDef = a.Resolve();
			TypeDef typeDef2 = b.Resolve();
			if (typeDef != null && typeDef2 != null)
			{
				flag2 = TypeDefScopeEquals(typeDef, typeDef2);
			}
		}
		recursionCounter.Decrement();
		return flag2;
	}

	private bool EqualsImplementation(ExportedType a, ExportedType b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		IImplementation implementation = a.Implementation;
		IImplementation implementation2 = b.Implementation;
		if (implementation == implementation2)
		{
			return true;
		}
		if (implementation == null || implementation2 == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool flag = true;
		ExportedType a2;
		ExportedType b2;
		bool flag2;
		FileDef fileDef;
		FileDef fileDef2;
		AssemblyRef assemblyRef;
		AssemblyRef assemblyRef2;
		if (((a2 = implementation as ExportedType) != null) | ((b2 = implementation2 as ExportedType) != null))
		{
			flag2 = Equals(a2, b2);
			flag = false;
		}
		else if (DontCompareTypeScope)
		{
			flag2 = true;
		}
		else if (((fileDef = implementation as FileDef) != null) & ((fileDef2 = implementation2 as FileDef) != null))
		{
			flag2 = Equals(fileDef, fileDef2);
		}
		else if (((assemblyRef = implementation as AssemblyRef) != null) & ((assemblyRef2 = implementation2 as AssemblyRef) != null))
		{
			flag2 = Equals(assemblyRef, a, assemblyRef2, b);
		}
		else if (fileDef != null && assemblyRef2 != null)
		{
			flag2 = Equals(a.DefinitionAssembly, assemblyRef2, b);
		}
		else if (fileDef2 != null && assemblyRef != null)
		{
			flag2 = Equals(b.DefinitionAssembly, assemblyRef, a);
		}
		else
		{
			flag2 = false;
			flag = false;
		}
		if (!flag2 && flag && !DontCheckTypeEquivalence)
		{
			TypeDef typeDef = a.Resolve();
			TypeDef typeDef2 = b.Resolve();
			if (typeDef != null && typeDef2 != null)
			{
				flag2 = TypeDefScopeEquals(typeDef, typeDef2);
			}
		}
		recursionCounter.Decrement();
		return flag2;
	}

	private bool EqualsScope(TypeRef a, ExportedType b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		IResolutionScope resolutionScope = a.ResolutionScope;
		IImplementation implementation = b.Implementation;
		if (resolutionScope == implementation)
		{
			return true;
		}
		if (resolutionScope == null || implementation == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool flag = true;
		TypeRef a2;
		ExportedType b2;
		bool flag2;
		IModule module;
		FileDef fileDef;
		AssemblyRef assemblyRef;
		AssemblyRef assemblyRef2;
		if (((a2 = resolutionScope as TypeRef) != null) | ((b2 = implementation as ExportedType) != null))
		{
			flag2 = Equals(a2, b2);
			flag = false;
		}
		else if (DontCompareTypeScope)
		{
			flag2 = true;
		}
		else if (((module = resolutionScope as IModule) != null) & ((fileDef = implementation as FileDef) != null))
		{
			flag2 = Equals(a, module, b, fileDef);
		}
		else if (((assemblyRef = resolutionScope as AssemblyRef) != null) & ((assemblyRef2 = implementation as AssemblyRef) != null))
		{
			flag2 = Equals(assemblyRef, a, assemblyRef2, b);
		}
		else if (module != null && assemblyRef2 != null)
		{
			flag2 = Equals(a.DefinitionAssembly, assemblyRef2, b);
		}
		else if (fileDef != null && assemblyRef != null)
		{
			flag2 = Equals(b.DefinitionAssembly, assemblyRef, a);
		}
		else
		{
			flag = false;
			flag2 = false;
		}
		if (!flag2 && flag && !DontCheckTypeEquivalence)
		{
			TypeDef typeDef = a.Resolve();
			TypeDef typeDef2 = b.Resolve();
			if (typeDef != null && typeDef2 != null)
			{
				flag2 = TypeDefScopeEquals(typeDef, typeDef2);
			}
		}
		recursionCounter.Decrement();
		return flag2;
	}

	private bool Equals(FileDef a, FileDef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return UTF8String.CaseInsensitiveEquals(a.Name, b.Name);
	}

	private bool Equals(IModule a, FileDef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return UTF8String.CaseInsensitiveEquals(a.Name, b.Name);
	}

	internal bool Equals(IModule a, IModule b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!MscorlibIsNotSpecial && IsCorLib(a) && IsCorLib(b))
		{
			return true;
		}
		return UTF8String.CaseInsensitiveEquals(a.Name, b.Name);
	}

	private static bool IsCorLib(ModuleDef a)
	{
		if (a != null && a.IsManifestModule)
		{
			return a.Assembly.IsCorLib();
		}
		return false;
	}

	private static bool IsCorLib(IModule a)
	{
		if (a is ModuleDef { IsManifestModule: not false } moduleDef)
		{
			return moduleDef.Assembly.IsCorLib();
		}
		return false;
	}

	private static bool IsCorLib(Module a)
	{
		if ((object)a != null && (object)a.Assembly.ManifestModule == a)
		{
			return (object)a.Assembly == typeof(void).Assembly;
		}
		return false;
	}

	private static bool IsCorLib(IAssembly a)
	{
		return a.IsCorLib();
	}

	private static bool IsCorLib(Assembly a)
	{
		return (object)a == typeof(void).Assembly;
	}

	private bool Equals(ModuleDef a, ModuleDef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!MscorlibIsNotSpecial && IsCorLib(a) && IsCorLib(b))
		{
			return true;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals((IModule)a, (IModule)b) && Equals(a.Assembly, b.Assembly);
		recursionCounter.Decrement();
		return result;
	}

	private bool Equals(IAssembly a, IAssembly b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!MscorlibIsNotSpecial && IsCorLib(a) && IsCorLib(b))
		{
			return true;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = UTF8String.CaseInsensitiveEquals(a.Name, b.Name) && (!CompareAssemblyPublicKeyToken || PublicKeyBase.TokenEquals(a.PublicKeyOrToken, b.PublicKeyOrToken)) && (!CompareAssemblyVersion || Utils.Equals(a.Version, b.Version)) && (!CompareAssemblyLocale || Utils.LocaleEquals(a.Culture, b.Culture));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(TypeSig a, TypeSig b)
	{
		if (IgnoreModifiers)
		{
			a = a.RemoveModifiers();
			b = b.RemoveModifiers();
		}
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
			b = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b) ?? b;
		}
		bool result;
		if (a.ElementType != b.ElementType)
		{
			result = false;
		}
		else
		{
			switch (a.ElementType)
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
			case ElementType.TypedByRef:
			case ElementType.I:
			case ElementType.U:
			case ElementType.Object:
			case ElementType.Sentinel:
				result = true;
				break;
			case ElementType.Ptr:
			case ElementType.ByRef:
			case ElementType.SZArray:
			case ElementType.Pinned:
				result = Equals(a.Next, b.Next);
				break;
			case ElementType.Array:
			{
				ArraySig arraySig = a as ArraySig;
				ArraySig arraySig2 = b as ArraySig;
				result = arraySig.Rank == arraySig2.Rank && (IgnoreMultiDimensionalArrayLowerBoundsAndSizes || (Equals(arraySig.Sizes, arraySig2.Sizes) && Equals(arraySig.LowerBounds, arraySig2.LowerBounds))) && Equals(a.Next, b.Next);
				break;
			}
			case ElementType.ValueType:
			case ElementType.Class:
				result = ((!RawSignatureCompare) ? Equals((IType)(a as ClassOrValueTypeSig).TypeDefOrRef, (IType)(b as ClassOrValueTypeSig).TypeDefOrRef) : TokenEquals((a as ClassOrValueTypeSig).TypeDefOrRef, (b as ClassOrValueTypeSig).TypeDefOrRef));
				break;
			case ElementType.Var:
			case ElementType.MVar:
				result = (a as GenericSig).Number == (b as GenericSig).Number;
				break;
			case ElementType.GenericInst:
			{
				GenericInstSig genericInstSig = (GenericInstSig)a;
				GenericInstSig genericInstSig2 = (GenericInstSig)b;
				if (RawSignatureCompare)
				{
					ClassOrValueTypeSig genericType = genericInstSig.GenericType;
					result = TokenEquals(b: genericInstSig2.GenericType?.TypeDefOrRef, a: genericType?.TypeDefOrRef) && Equals(genericInstSig.GenericArguments, genericInstSig2.GenericArguments);
				}
				else
				{
					result = Equals(genericInstSig.GenericType, genericInstSig2.GenericType) && Equals(genericInstSig.GenericArguments, genericInstSig2.GenericArguments);
				}
				break;
			}
			case ElementType.FnPtr:
				result = Equals((a as FnPtrSig).Signature, (b as FnPtrSig).Signature);
				break;
			case ElementType.CModReqd:
			case ElementType.CModOpt:
				result = ((!RawSignatureCompare) ? (Equals((IType)(a as ModifierSig).Modifier, (IType)(b as ModifierSig).Modifier) && Equals(a.Next, b.Next)) : (TokenEquals((a as ModifierSig).Modifier, (b as ModifierSig).Modifier) && Equals(a.Next, b.Next)));
				break;
			case ElementType.ValueArray:
				result = (a as ValueArraySig).Size == (b as ValueArraySig).Size && Equals(a.Next, b.Next);
				break;
			case ElementType.Module:
				result = (a as ModuleSig).Index == (b as ModuleSig).Index && Equals(a.Next, b.Next);
				break;
			default:
				result = false;
				break;
			}
		}
		recursionCounter.Decrement();
		return result;
	}

	private static bool TokenEquals(ITypeDefOrRef a, ITypeDefOrRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		return a.MDToken == b.MDToken;
	}

	public int GetHashCode(TypeSig a)
	{
		return GetHashCode(a, substituteGenericParameters: true);
	}

	private int GetHashCode(TypeSig a, bool substituteGenericParameters)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		if (substituteGenericParameters && genericArguments != null)
		{
			TypeSig typeSig = a;
			a = genericArguments.Resolve(a);
			substituteGenericParameters = typeSig == a;
		}
		int result;
		switch (a.ElementType)
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
			result = GetHashCode((IType)(a as TypeDefOrRefSig).TypeDefOrRef);
			break;
		case ElementType.Sentinel:
			result = 68439620;
			break;
		case ElementType.Ptr:
			result = 1976400808 + GetHashCode(a.Next, substituteGenericParameters);
			break;
		case ElementType.ByRef:
			result = -634749586 + GetHashCode(a.Next, substituteGenericParameters);
			break;
		case ElementType.SZArray:
			result = 871833535 + GetHashCode(a.Next, substituteGenericParameters);
			break;
		case ElementType.CModReqd:
		case ElementType.CModOpt:
		case ElementType.Pinned:
			result = GetHashCode(a.Next, substituteGenericParameters);
			break;
		case ElementType.Array:
		{
			ArraySig arraySig = (ArraySig)a;
			result = -96331531 + (int)arraySig.Rank + GetHashCode(arraySig.Next, substituteGenericParameters);
			break;
		}
		case ElementType.Var:
			result = (int)(1288450097 + (a as GenericVar).Number);
			break;
		case ElementType.MVar:
			result = -990598495 + (int)(a as GenericMVar).Number;
			break;
		case ElementType.GenericInst:
		{
			GenericInstSig genericInstSig = (GenericInstSig)a;
			result = -2050514639;
			result += GetHashCode(genericInstSig.GenericType, substituteGenericParameters);
			result += GetHashCode(genericInstSig.GenericArguments, substituteGenericParameters);
			break;
		}
		case ElementType.FnPtr:
			result = GetHashCode_FnPtr_SystemIntPtr();
			break;
		case ElementType.ValueArray:
			result = -674970533 + (int)(a as ValueArraySig).Size + GetHashCode(a.Next, substituteGenericParameters);
			break;
		case ElementType.Module:
			result = -299744851 + (int)(a as ModuleSig).Index + GetHashCode(a.Next, substituteGenericParameters);
			break;
		default:
			result = 0;
			break;
		}
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(IList<TypeSig> a, IList<TypeSig> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (a.Count != b.Count)
		{
			result = false;
		}
		else
		{
			int i;
			for (i = 0; i < a.Count && Equals(a[i], b[i]); i++)
			{
			}
			result = i == a.Count;
		}
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(IList<TypeSig> a)
	{
		return GetHashCode(a, substituteGenericParameters: true);
	}

	private int GetHashCode(IList<TypeSig> a, bool substituteGenericParameters)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		uint num = 0u;
		for (int i = 0; i < a.Count; i++)
		{
			num += (uint)GetHashCode(a[i], substituteGenericParameters);
			num = (num << 13) | (num >> 19);
		}
		recursionCounter.Decrement();
		return (int)num;
	}

	private bool Equals(IList<uint> a, IList<uint> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	private bool Equals(IList<int> a, IList<int> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	public bool Equals(CallingConventionSig a, CallingConventionSig b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (a.GetCallingConvention() != b.GetCallingConvention())
		{
			result = false;
		}
		else
		{
			switch (a.GetCallingConvention() & CallingConvention.Mask)
			{
			case CallingConvention.Default:
			case CallingConvention.C:
			case CallingConvention.StdCall:
			case CallingConvention.ThisCall:
			case CallingConvention.FastCall:
			case CallingConvention.VarArg:
			case CallingConvention.Property:
			case CallingConvention.Unmanaged:
			case CallingConvention.NativeVarArg:
			{
				MethodBaseSig methodBaseSig = a as MethodBaseSig;
				MethodBaseSig methodBaseSig2 = b as MethodBaseSig;
				result = methodBaseSig != null && methodBaseSig2 != null && Equals(methodBaseSig, methodBaseSig2);
				break;
			}
			case CallingConvention.Field:
			{
				FieldSig fieldSig = a as FieldSig;
				FieldSig fieldSig2 = b as FieldSig;
				result = fieldSig != null && fieldSig2 != null && Equals(fieldSig, fieldSig2);
				break;
			}
			case CallingConvention.LocalSig:
			{
				LocalSig localSig = a as LocalSig;
				LocalSig localSig2 = b as LocalSig;
				result = localSig != null && localSig2 != null && Equals(localSig, localSig2);
				break;
			}
			case CallingConvention.GenericInst:
			{
				GenericInstMethodSig genericInstMethodSig = a as GenericInstMethodSig;
				GenericInstMethodSig genericInstMethodSig2 = b as GenericInstMethodSig;
				result = genericInstMethodSig != null && genericInstMethodSig2 != null && Equals(genericInstMethodSig, genericInstMethodSig2);
				break;
			}
			default:
				result = false;
				break;
			}
		}
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(CallingConventionSig a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result;
		switch (a.GetCallingConvention() & CallingConvention.Mask)
		{
		case CallingConvention.Default:
		case CallingConvention.C:
		case CallingConvention.StdCall:
		case CallingConvention.ThisCall:
		case CallingConvention.FastCall:
		case CallingConvention.VarArg:
		case CallingConvention.Property:
		case CallingConvention.Unmanaged:
		case CallingConvention.NativeVarArg:
			result = ((a is MethodBaseSig a4) ? GetHashCode(a4) : 0);
			break;
		case CallingConvention.Field:
			result = ((a is FieldSig a3) ? GetHashCode(a3) : 0);
			break;
		case CallingConvention.LocalSig:
			result = ((a is LocalSig a5) ? GetHashCode(a5) : 0);
			break;
		case CallingConvention.GenericInst:
			result = ((a is GenericInstMethodSig a2) ? GetHashCode(a2) : 0);
			break;
		default:
			result = GetHashCode_CallingConvention(a);
			break;
		}
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(MethodBaseSig a, MethodBaseSig b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = a.GetCallingConvention() == b.GetCallingConvention() && (DontCompareReturnType || Equals(a.RetType, b.RetType)) && Equals(a.Params, b.Params) && (!a.Generic || a.GenParamCount == b.GenParamCount) && (!CompareSentinelParams || Equals(a.ParamsAfterSentinel, b.ParamsAfterSentinel));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(MethodBaseSig a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int num = GetHashCode_CallingConvention(a) + GetHashCode(a.Params);
		if (!DontCompareReturnType)
		{
			num += GetHashCode(a.RetType);
		}
		if (a.Generic)
		{
			num += GetHashCode_ElementType_MVar((int)a.GenParamCount);
		}
		if (CompareSentinelParams)
		{
			num += GetHashCode(a.ParamsAfterSentinel);
		}
		recursionCounter.Decrement();
		return num;
	}

	private int GetHashCode_CallingConvention(CallingConventionSig a)
	{
		return GetHashCode(a.GetCallingConvention());
	}

	private int GetHashCode(CallingConvention a)
	{
		switch (a & CallingConvention.Mask)
		{
		case CallingConvention.Default:
		case CallingConvention.C:
		case CallingConvention.StdCall:
		case CallingConvention.ThisCall:
		case CallingConvention.FastCall:
		case CallingConvention.VarArg:
		case CallingConvention.Field:
		case CallingConvention.Property:
		case CallingConvention.Unmanaged:
		case CallingConvention.GenericInst:
		case CallingConvention.NativeVarArg:
			return (int)(a & ~(CallingConvention.Mask | CallingConvention.ReservedByCLR));
		default:
			return (int)a;
		}
	}

	public bool Equals(FieldSig a, FieldSig b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = a.GetCallingConvention() == b.GetCallingConvention() && Equals(a.Type, b.Type);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(FieldSig a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result = GetHashCode_CallingConvention(a) + GetHashCode(a.Type);
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(LocalSig a, LocalSig b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = a.GetCallingConvention() == b.GetCallingConvention() && Equals(a.Locals, b.Locals);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(LocalSig a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result = GetHashCode_CallingConvention(a) + GetHashCode(a.Locals);
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(GenericInstMethodSig a, GenericInstMethodSig b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = a.GetCallingConvention() == b.GetCallingConvention() && Equals(a.GenericArguments, b.GenericArguments);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(GenericInstMethodSig a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result = GetHashCode_CallingConvention(a) + GetHashCode(a.GenericArguments);
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(IMethod a, IMethod b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		MethodDef methodDef;
		MethodDef methodDef2;
		MemberRef memberRef;
		MemberRef memberRef2;
		bool result = ((((methodDef = a as MethodDef) != null) & ((methodDef2 = b as MethodDef) != null)) ? Equals(methodDef, methodDef2) : ((((memberRef = a as MemberRef) != null) & ((memberRef2 = b as MemberRef) != null)) ? Equals(memberRef, memberRef2) : ((a is MethodSpec a2 && b is MethodSpec b2) ? Equals(a2, b2) : ((methodDef != null && memberRef2 != null) ? Equals(methodDef, memberRef2) : (memberRef != null && methodDef2 != null && Equals(methodDef2, memberRef))))));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(IMethod a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result = ((!(a is MethodDef a2)) ? ((!(a is MemberRef a3)) ? ((a is MethodSpec a4) ? GetHashCode(a4) : 0) : GetHashCode(a3)) : GetHashCode(a2));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(MemberRef a, MethodDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(MethodDef a, MemberRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (!DontProjectWinMDRefs)
		{
			MemberRef memberRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			b = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b) ?? b;
			if (memberRef != null)
			{
				result = Equals(memberRef, b);
				goto IL_00f3;
			}
		}
		result = (PrivateScopeMethodIsComparable || !a.IsPrivateScope) && Equals_MethodFieldNames(a.Name, b.Name) && Equals(a.Signature, b.Signature) && (!CompareMethodFieldDeclaringType || Equals(a.DeclaringType, b.Class));
		goto IL_00f3;
		IL_00f3:
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(MethodDef a, MethodDef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (ReferenceCompareForMemberDefsInSameModule && InSameModule(a, b))
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (!DontProjectWinMDRefs)
		{
			MemberRef memberRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			MemberRef memberRef2 = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b);
			if (memberRef != null || memberRef2 != null)
			{
				IMethod method = memberRef;
				IMethod a2 = method ?? a;
				method = memberRef2;
				result = Equals(a2, method ?? b);
				goto IL_0107;
			}
		}
		result = Equals_MethodFieldNames(a.Name, b.Name) && Equals(a.Signature, b.Signature) && (!CompareMethodFieldDeclaringType || Equals(a.DeclaringType, b.DeclaringType));
		goto IL_0107;
		IL_0107:
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(MethodDef a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!DontProjectWinMDRefs)
		{
			MemberRef memberRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			if (memberRef != null)
			{
				return GetHashCode(memberRef);
			}
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int num = GetHashCode_MethodFieldName(a.Name) + GetHashCode(a.Signature);
		if (CompareMethodFieldDeclaringType)
		{
			num += GetHashCode(a.DeclaringType);
		}
		recursionCounter.Decrement();
		return num;
	}

	public bool Equals(MemberRef a, MemberRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
			b = WinMDHelpers.ToCLR(b.Module ?? sourceModule, b) ?? b;
		}
		bool result = Equals_MethodFieldNames(a.Name, b.Name) && Equals(a.Signature, b.Signature) && (!CompareMethodFieldDeclaringType || Equals(a.Class, b.Class));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(MemberRef a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
		}
		int hashCode_MethodFieldName = GetHashCode_MethodFieldName(a.Name);
		GenericInstSig genericInstanceType;
		if (CompareMethodFieldDeclaringType && !DontSubstituteGenericParameters && (genericInstanceType = GetGenericInstanceType(a.Class)) != null)
		{
			InitializeGenericArguments();
			genericArguments.PushTypeArgs(genericInstanceType.GenericArguments);
			hashCode_MethodFieldName += GetHashCode(a.Signature);
			genericArguments.PopTypeArgs();
		}
		else
		{
			hashCode_MethodFieldName += GetHashCode(a.Signature);
		}
		if (CompareMethodFieldDeclaringType)
		{
			hashCode_MethodFieldName += GetHashCode(a.Class);
		}
		recursionCounter.Decrement();
		return hashCode_MethodFieldName;
	}

	public bool Equals(MethodSpec a, MethodSpec b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals(a.Method, b.Method) && Equals(a.Instantiation, b.Instantiation);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(MethodSpec a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		GenericInstMethodSig genericInstMethodSig = a.GenericInstMethodSig;
		if (genericInstMethodSig != null)
		{
			InitializeGenericArguments();
			genericArguments.PushMethodArgs(genericInstMethodSig.GenericArguments);
		}
		int hashCode = GetHashCode(a.Method);
		if (genericInstMethodSig != null)
		{
			genericArguments.PopMethodArgs();
		}
		recursionCounter.Decrement();
		return hashCode;
	}

	private bool Equals(IMemberRefParent a, IMemberRefParent b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		ModuleRef moduleRef;
		ModuleRef moduleRef2;
		if (a is ITypeDefOrRef a2 && b is ITypeDefOrRef b2)
		{
			result = Equals((IType)a2, (IType)b2);
		}
		else if (!(((moduleRef = a as ModuleRef) != null) & ((moduleRef2 = b as ModuleRef) != null)))
		{
			result = ((a is MethodDef a3 && b is MethodDef b3) ? Equals(a3, b3) : ((moduleRef2 != null && a is TypeDef a4) ? EqualsGlobal(a4, moduleRef2) : (moduleRef != null && b is TypeDef a5 && EqualsGlobal(a5, moduleRef))));
		}
		else
		{
			ModuleDef module = moduleRef.Module;
			ModuleDef module2 = moduleRef2.Module;
			result = Equals((IModule)moduleRef, (IModule)moduleRef2) && Equals(module?.Assembly, module2?.Assembly);
		}
		recursionCounter.Decrement();
		return result;
	}

	private int GetHashCode(IMemberRefParent a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result = ((!(a is ITypeDefOrRef a2)) ? ((a is ModuleRef) ? GetHashCodeGlobalType() : ((a is MethodDef methodDef) ? GetHashCode(methodDef.DeclaringType) : 0)) : GetHashCode((IType)a2));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(IField a, IField b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		FieldDef fieldDef;
		FieldDef fieldDef2;
		MemberRef memberRef;
		MemberRef memberRef2;
		bool result = ((((fieldDef = a as FieldDef) != null) & ((fieldDef2 = b as FieldDef) != null)) ? Equals(fieldDef, fieldDef2) : ((((memberRef = a as MemberRef) != null) & ((memberRef2 = b as MemberRef) != null)) ? Equals(memberRef, memberRef2) : ((fieldDef != null && memberRef2 != null) ? Equals(fieldDef, memberRef2) : (fieldDef2 != null && memberRef != null && Equals(fieldDef2, memberRef)))));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(IField a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result = ((!(a is FieldDef a2)) ? ((a is MemberRef a3) ? GetHashCode(a3) : 0) : GetHashCode(a2));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(MemberRef a, FieldDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(FieldDef a, MemberRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = (PrivateScopeFieldIsComparable || !a.IsPrivateScope) && Equals_MethodFieldNames(a.Name, b.Name) && Equals(a.Signature, b.Signature) && (!CompareMethodFieldDeclaringType || Equals(a.DeclaringType, b.Class));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(FieldDef a, FieldDef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (ReferenceCompareForMemberDefsInSameModule && InSameModule(a, b))
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals_MethodFieldNames(a.Name, b.Name) && Equals(a.Signature, b.Signature) && (!CompareMethodFieldDeclaringType || Equals(a.DeclaringType, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(FieldDef a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int num = GetHashCode_MethodFieldName(a.Name) + GetHashCode(a.Signature);
		if (CompareMethodFieldDeclaringType)
		{
			num += GetHashCode(a.DeclaringType);
		}
		recursionCounter.Decrement();
		return num;
	}

	public bool Equals(PropertyDef a, PropertyDef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (ReferenceCompareForMemberDefsInSameModule && InSameModule(a, b))
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals_PropertyNames(a.Name, b.Name) && Equals(a.Type, b.Type) && (!ComparePropertyDeclaringType || Equals(a.DeclaringType, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(PropertyDef a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		PropertySig propertySig = a.PropertySig;
		int num = GetHashCode_PropertyName(a.Name) + GetHashCode(propertySig?.RetType);
		if (ComparePropertyDeclaringType)
		{
			num += GetHashCode(a.DeclaringType);
		}
		recursionCounter.Decrement();
		return num;
	}

	public bool Equals(EventDef a, EventDef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (ReferenceCompareForMemberDefsInSameModule && InSameModule(a, b))
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals_EventNames(a.Name, b.Name) && Equals((IType)a.EventType, (IType)b.EventType) && (!CompareEventDeclaringType || Equals(a.DeclaringType, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(EventDef a)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int num = GetHashCode_EventName(a.Name) + GetHashCode((IType)a.EventType);
		if (CompareEventDeclaringType)
		{
			num += GetHashCode(a.DeclaringType);
		}
		recursionCounter.Decrement();
		return num;
	}

	private bool EqualsGlobal(TypeDef a, ModuleRef b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = a.IsGlobalModuleType && Equals(a.Module, b) && Equals(a.DefinitionAssembly, GetAssembly(b.Module));
		recursionCounter.Decrement();
		return result;
	}

	private static AssemblyDef GetAssembly(ModuleDef module)
	{
		return module?.Assembly;
	}

	public bool Equals(Type a, IType b)
	{
		return Equals(b, a);
	}

	public bool Equals(IType a, Type b)
	{
		if (a == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(a is TypeDef a2)) ? ((!(a is TypeRef a3)) ? ((!(a is TypeSpec a4)) ? ((!(a is TypeSig a5)) ? (a is ExportedType a6 && Equals(a6, b)) : Equals(a5, b)) : Equals(a4, b)) : Equals(a3, b)) : Equals(a2, b));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(Type a, TypeDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeDef a, Type b)
	{
		if (a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return a.IsGlobalModuleType;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (!DontProjectWinMDRefs)
		{
			TypeRef typeRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			if (typeRef != null)
			{
				result = Equals(typeRef, b);
				goto IL_00da;
			}
		}
		result = !b.HasElementType && Equals_TypeNames(a.Name, ReflectionExtensions.Unescape(b.Name)) && Equals_TypeNamespaces(a.Namespace, b) && EnclosingTypeEquals(a.DeclaringType, b.DeclaringType) && (DontCompareTypeScope || Equals(a.Module, b.Module));
		goto IL_00da;
		IL_00da:
		recursionCounter.Decrement();
		return result;
	}

	private bool EnclosingTypeEquals(TypeDef a, Type b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		return Equals(a, b);
	}

	public bool Equals(Type a, TypeRef b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeRef a, Type b)
	{
		if (a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
		}
		IResolutionScope resolutionScope = a.ResolutionScope;
		bool result = b.IsTypeDef() && Equals_TypeNames(a.Name, ReflectionExtensions.Unescape(b.Name)) && Equals_TypeNamespaces(a.Namespace, b) && ((!(resolutionScope is TypeRef a2)) ? (!b.IsNested && (DontCompareTypeScope || ((!(resolutionScope is IModule bMod)) ? (resolutionScope is AssemblyRef bAsm && Equals(b.Assembly, bAsm, a)) : Equals(b, bMod, a)))) : Equals(a2, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	private bool Equals_TypeNamespaces(UTF8String a, Type b)
	{
		if (b.IsNested)
		{
			return true;
		}
		return Equals_TypeNamespaces(a, b.Namespace);
	}

	public bool Equals(Type a, TypeSpec b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeSpec a, Type b)
	{
		if (a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return false;
		}
		return Equals(a.TypeSig, b);
	}

	public bool Equals(Type a, TypeSig b)
	{
		return Equals(b, a);
	}

	public bool Equals(TypeSig a, Type b)
	{
		return Equals(a, b, null, false);
	}

	private bool Equals(ITypeDefOrRef a, Type b, Type declaringType)
	{
		if (a is TypeSpec typeSpec)
		{
			return Equals(typeSpec.TypeSig, b, declaringType);
		}
		return Equals(a, b);
	}

	private static bool IsFnPtrElementType(Type a)
	{
		if ((object)a == null || !a.HasElementType)
		{
			return false;
		}
		Type elementType = a.GetElementType();
		if ((object)elementType == null || elementType.HasElementType)
		{
			return false;
		}
		if ((object)elementType != typeof(IntPtr))
		{
			return false;
		}
		if (!a.FullName.StartsWith("(fnptr)"))
		{
			return false;
		}
		return true;
	}

	private bool Equals(TypeSig a, Type b, Type declaringType, bool? treatAsGenericInst = null)
	{
		if (a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool flag = treatAsGenericInst ?? declaringType.MustTreatTypeAsGenericInstType(b);
		if (genericArguments != null)
		{
			a = genericArguments.Resolve(a);
		}
		bool result;
		switch (a.ElementType)
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
		case ElementType.TypedByRef:
		case ElementType.I:
		case ElementType.U:
		case ElementType.Object:
			result = Equals(((TypeDefOrRefSig)a).TypeDefOrRef, b, declaringType);
			break;
		case ElementType.Ptr:
			if (!b.IsPointer)
			{
				result = false;
			}
			else if (IsFnPtrElementType(b))
			{
				a = a.Next.RemoveModifiers();
				result = a != null && a.ElementType == ElementType.FnPtr;
			}
			else
			{
				result = Equals(a.Next, b.GetElementType(), declaringType);
			}
			break;
		case ElementType.ByRef:
			if (!b.IsByRef)
			{
				result = false;
			}
			else if (IsFnPtrElementType(b))
			{
				a = a.Next.RemoveModifiers();
				result = a != null && a.ElementType == ElementType.FnPtr;
			}
			else
			{
				result = Equals(a.Next, b.GetElementType(), declaringType);
			}
			break;
		case ElementType.SZArray:
			if (!b.IsArray || !b.IsSZArray())
			{
				result = false;
			}
			else if (IsFnPtrElementType(b))
			{
				a = a.Next.RemoveModifiers();
				result = a != null && a.ElementType == ElementType.FnPtr;
			}
			else
			{
				result = Equals(a.Next, b.GetElementType(), declaringType);
			}
			break;
		case ElementType.Pinned:
			result = Equals(a.Next, b, declaringType);
			break;
		case ElementType.Array:
			result = b.IsArray && !b.IsSZArray() && (a as ArraySig).Rank == b.GetArrayRank() && ((!IsFnPtrElementType(b)) ? Equals(a.Next, b.GetElementType(), declaringType) : ((a = a.Next.RemoveModifiers()) != null && a.ElementType == ElementType.FnPtr));
			break;
		case ElementType.ValueType:
		case ElementType.Class:
			result = Equals((a as ClassOrValueTypeSig).TypeDefOrRef, b, declaringType);
			break;
		case ElementType.Var:
			result = b.IsGenericParameter && b.GenericParameterPosition == (a as GenericSig).Number && (object)b.DeclaringMethod == null;
			break;
		case ElementType.MVar:
			result = b.IsGenericParameter && b.GenericParameterPosition == (a as GenericSig).Number && (object)b.DeclaringMethod != null;
			break;
		case ElementType.GenericInst:
		{
			if ((!b.IsGenericType || b.IsGenericTypeDefinition) && !flag)
			{
				result = false;
				break;
			}
			GenericInstSig genericInstSig = (GenericInstSig)a;
			result = Equals(genericInstSig.GenericType, b.GetGenericTypeDefinition(), null, false) && Equals(genericInstSig.GenericArguments, b.GetGenericArguments(), declaringType);
			break;
		}
		case ElementType.CModReqd:
		case ElementType.CModOpt:
			result = Equals(a.Next, b, declaringType);
			break;
		case ElementType.FnPtr:
			result = (object)b == typeof(IntPtr);
			break;
		default:
			result = false;
			break;
		}
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(Type a, ExportedType b)
	{
		return Equals(b, a);
	}

	public bool Equals(ExportedType a, Type b)
	{
		if (a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
		}
		IImplementation implementation = a.Implementation;
		bool result = b.IsTypeDef() && Equals_TypeNames(a.TypeName, ReflectionExtensions.Unescape(b.Name)) && Equals_TypeNamespaces(a.TypeNamespace, b) && ((!(implementation is ExportedType a2)) ? (!b.IsNested && (DontCompareTypeScope || ((!(implementation is FileDef bFile)) ? (implementation is AssemblyRef bAsm && Equals(b.Assembly, bAsm, a)) : Equals(b, bFile, a)))) : Equals(a2, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(Type a)
	{
		return GetHashCode(a, treatAsGenericInst: false);
	}

	public int GetHashCode(Type a, bool treatAsGenericInst)
	{
		return GetHashCode(a, null, treatAsGenericInst);
	}

	private int GetHashCode(Type a, Type declaringType, bool? treatAsGenericInst = null)
	{
		if ((object)a == null)
		{
			return GetHashCode_TypeDef(a);
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int result;
		switch ((treatAsGenericInst ?? declaringType.MustTreatTypeAsGenericInstType(a)) ? ElementType.GenericInst : a.GetElementType2())
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
			result = GetHashCode_TypeDef(a);
			break;
		case ElementType.FnPtr:
			result = GetHashCode_FnPtr_SystemIntPtr();
			break;
		case ElementType.Sentinel:
			result = 68439620;
			break;
		case ElementType.Ptr:
			result = 1976400808 + (IsFnPtrElementType(a) ? GetHashCode_FnPtr_SystemIntPtr() : GetHashCode(a.GetElementType(), declaringType));
			break;
		case ElementType.ByRef:
			result = -634749586 + (IsFnPtrElementType(a) ? GetHashCode_FnPtr_SystemIntPtr() : GetHashCode(a.GetElementType(), declaringType));
			break;
		case ElementType.SZArray:
			result = 871833535 + (IsFnPtrElementType(a) ? GetHashCode_FnPtr_SystemIntPtr() : GetHashCode(a.GetElementType(), declaringType));
			break;
		case ElementType.CModReqd:
		case ElementType.CModOpt:
		case ElementType.Pinned:
			result = GetHashCode(a.GetElementType(), declaringType);
			break;
		case ElementType.Array:
			result = -96331531 + a.GetArrayRank() + (IsFnPtrElementType(a) ? GetHashCode_FnPtr_SystemIntPtr() : GetHashCode(a.GetElementType(), declaringType));
			break;
		case ElementType.Var:
			result = 1288450097 + a.GenericParameterPosition;
			break;
		case ElementType.MVar:
			result = -990598495 + a.GenericParameterPosition;
			break;
		case ElementType.GenericInst:
			result = -2050514639 + GetHashCode(a.GetGenericTypeDefinition(), treatAsGenericInst: false) + GetHashCode(a.GetGenericArguments(), declaringType);
			break;
		default:
			result = 0;
			break;
		}
		recursionCounter.Decrement();
		return result;
	}

	private int GetHashCode(IList<Type> a, Type declaringType)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		uint num = 0u;
		for (int i = 0; i < a.Count; i++)
		{
			num += (uint)GetHashCode(a[i], declaringType);
			num = (num << 13) | (num >> 19);
		}
		recursionCounter.Decrement();
		return (int)num;
	}

	private static int GetHashCode_ElementType_MVar(int numGenericParams)
	{
		return GetHashCode(numGenericParams, -990598495);
	}

	private static int GetHashCode(int numGenericParams, int etypeHashCode)
	{
		uint num = 0u;
		for (int i = 0; i < numGenericParams; i++)
		{
			num += (uint)(etypeHashCode + i);
			num = (num << 13) | (num >> 19);
		}
		return (int)num;
	}

	public int GetHashCode_TypeDef(Type a)
	{
		if ((object)a == null)
		{
			return GetHashCodeGlobalType();
		}
		int hashCode_TypeName = GetHashCode_TypeName(ReflectionExtensions.Unescape(a.Name));
		if (a.IsNested)
		{
			return hashCode_TypeName + -1049070942;
		}
		return hashCode_TypeName + GetHashCode_TypeNamespace(a.Namespace);
	}

	private bool Equals(IList<TypeSig> a, IList<Type> b, Type declaringType)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (a.Count != b.Count)
		{
			result = false;
		}
		else
		{
			int i;
			for (i = 0; i < a.Count && Equals(a[i], b[i], declaringType); i++)
			{
			}
			result = i == a.Count;
		}
		recursionCounter.Decrement();
		return result;
	}

	private bool Equals(ModuleDef a, Module b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!MscorlibIsNotSpecial && IsCorLib(a) && IsCorLib(b))
		{
			return true;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals((IModule)a, b) && Equals(a.Assembly, b.Assembly);
		recursionCounter.Decrement();
		return result;
	}

	private bool Equals(FileDef a, Module b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		return UTF8String.ToSystemStringOrEmpty(a.Name).Equals(b.Name, StringComparison.OrdinalIgnoreCase);
	}

	private bool Equals(IModule a, Module b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!MscorlibIsNotSpecial && IsCorLib(a) && IsCorLib(b))
		{
			return true;
		}
		return UTF8String.ToSystemStringOrEmpty(a.Name).Equals(b.ScopeName, StringComparison.OrdinalIgnoreCase);
	}

	private bool Equals(IAssembly a, Assembly b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!MscorlibIsNotSpecial && IsCorLib(a) && IsCorLib(b))
		{
			return true;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		AssemblyName name = b.GetName();
		bool result = UTF8String.ToSystemStringOrEmpty(a.Name).Equals(name.Name, StringComparison.OrdinalIgnoreCase) && (!CompareAssemblyPublicKeyToken || PublicKeyBase.TokenEquals(a.PublicKeyOrToken, new PublicKeyToken(name.GetPublicKeyToken()))) && (!CompareAssemblyVersion || Utils.Equals(a.Version, name.Version)) && (!CompareAssemblyLocale || Utils.LocaleEquals(a.Culture, name.CultureInfo.Name));
		recursionCounter.Decrement();
		return result;
	}

	private bool DeclaringTypeEquals(IMethod a, MethodBase b)
	{
		if (!CompareMethodFieldDeclaringType)
		{
			return true;
		}
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(a is MethodDef a2)) ? ((!(a is MemberRef a3)) ? (a is MethodSpec a4 && DeclaringTypeEquals(a4, b)) : DeclaringTypeEquals(a3, b)) : DeclaringTypeEquals(a2, b));
		recursionCounter.Decrement();
		return result;
	}

	private bool DeclaringTypeEquals(MethodDef a, MethodBase b)
	{
		if (!CompareMethodFieldDeclaringType)
		{
			return true;
		}
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		return Equals(a.DeclaringType, b.DeclaringType);
	}

	private bool DeclaringTypeEquals(MemberRef a, MethodBase b)
	{
		if (!CompareMethodFieldDeclaringType)
		{
			return true;
		}
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		return Equals(a.Class, b.DeclaringType, b.Module);
	}

	private bool DeclaringTypeEquals(MethodSpec a, MethodBase b)
	{
		if (!CompareMethodFieldDeclaringType)
		{
			return true;
		}
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		return DeclaringTypeEquals(a.Method, b);
	}

	public bool Equals(MethodBase a, IMethod b)
	{
		return Equals(b, a);
	}

	public bool Equals(IMethod a, MethodBase b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(a is MethodDef a2)) ? ((!(a is MemberRef a3)) ? (a is MethodSpec a4 && Equals(a4, b)) : Equals(a3, b)) : Equals(a2, b));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(MethodBase a, MethodDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(MethodDef a, MethodBase b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (!DontProjectWinMDRefs)
		{
			MemberRef memberRef = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a);
			if (memberRef != null)
			{
				result = Equals(memberRef, b);
				goto IL_00ff;
			}
		}
		MethodSig methodSig = a.MethodSig;
		result = Equals_MethodFieldNames(a.Name, b.Name) && methodSig != null && ((methodSig.Generic && b.IsGenericMethodDefinition && b.IsGenericMethod) || (!methodSig.Generic && !b.IsGenericMethodDefinition && !b.IsGenericMethod)) && Equals(methodSig, b) && (!CompareMethodFieldDeclaringType || Equals(a.DeclaringType, b.DeclaringType));
		goto IL_00ff;
		IL_00ff:
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(MethodBase a, MethodSig b)
	{
		return Equals(b, a);
	}

	public bool Equals(MethodSig a, MethodBase b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!CompareMethodFieldDeclaringType && b.DeclaringType.IsGenericButNotGenericTypeDefinition())
		{
			MethodBase methodBase = b;
			b = b.Module.ResolveMethod(b.MetadataToken);
			if (b.IsGenericButNotGenericMethodDefinition())
			{
				b = ((MethodInfo)b).MakeGenericMethod(methodBase.GetGenericArguments());
			}
		}
		bool result = Equals(a.GetCallingConvention(), b) && (DontCompareReturnType || ReturnTypeEquals(a.RetType, b)) && Equals(a.Params, b.GetParameters(), b.DeclaringType) && (!a.Generic || a.GenParamCount == b.GetGenericArguments().Length);
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(MethodBase a, MemberRef b)
	{
		return Equals(b, a);
	}

	public bool Equals(MemberRef a, MethodBase b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!DontProjectWinMDRefs)
		{
			a = WinMDHelpers.ToCLR(a.Module ?? sourceModule, a) ?? a;
		}
		bool flag;
		if (b.IsGenericMethod && !b.IsGenericMethodDefinition)
		{
			flag = a.IsMethodRef && a.MethodSig.Generic;
			SigComparerOptions oldFlags = ClearOptions(SigComparerOptions.CompareMethodFieldDeclaringType);
			SetOptions((SigComparerOptions)1024u);
			flag = flag && Equals(a, b.Module.ResolveMethod(b.MetadataToken));
			RestoreOptions(oldFlags);
			flag = flag && DeclaringTypeEquals(a, b) && GenericMethodArgsEquals((int)a.MethodSig.GenParamCount, b.GetGenericArguments());
		}
		else
		{
			MethodSig methodSig = a.MethodSig;
			flag = Equals_MethodFieldNames(a.Name, b.Name) && methodSig != null && ((methodSig.Generic && b.IsGenericMethodDefinition && b.IsGenericMethod) || (!methodSig.Generic && !b.IsGenericMethodDefinition && !b.IsGenericMethod));
			GenericInstSig genericInstanceType;
			if (CompareMethodFieldDeclaringType && !DontSubstituteGenericParameters && (genericInstanceType = GetGenericInstanceType(a.Class)) != null)
			{
				InitializeGenericArguments();
				genericArguments.PushTypeArgs(genericInstanceType.GenericArguments);
				flag = flag && Equals(methodSig, b);
				genericArguments.PopTypeArgs();
			}
			else
			{
				flag = flag && Equals(methodSig, b);
			}
			flag = flag && (!CompareMethodFieldDeclaringType || Equals(a.Class, b.DeclaringType, b.Module));
		}
		recursionCounter.Decrement();
		return flag;
	}

	private static bool GenericMethodArgsEquals(int numMethodArgs, IList<Type> methodGenArgs)
	{
		if (numMethodArgs != methodGenArgs.Count)
		{
			return false;
		}
		for (int i = 0; i < numMethodArgs; i++)
		{
			if (methodGenArgs[i].GetElementType2() != ElementType.MVar)
			{
				return false;
			}
		}
		return true;
	}

	private bool Equals(IMemberRefParent a, Type b, Module bModule)
	{
		if (a == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(a is ITypeDefOrRef a2)) ? ((!(a is ModuleRef { Module: var module } moduleRef)) ? ((!(a is MethodDef methodDef)) ? ((object)b == null && a is TypeDef typeDef && typeDef.IsGlobalModuleType) : Equals(methodDef.DeclaringType, b)) : ((object)b == null && Equals(moduleRef, bModule) && Equals(module?.Assembly, bModule.Assembly))) : Equals(a2, b));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(MethodBase a, MethodSpec b)
	{
		return Equals(b, a);
	}

	public bool Equals(MethodSpec a, MethodBase b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool num = b.IsGenericMethod && !b.IsGenericMethodDefinition;
		SigComparerOptions oldFlags = ClearOptions(SigComparerOptions.CompareMethodFieldDeclaringType);
		SetOptions((SigComparerOptions)1024u);
		bool num2 = num && Equals(a.Method, b.Module.ResolveMethod(b.MetadataToken));
		RestoreOptions(oldFlags);
		bool num3 = num2 && DeclaringTypeEquals(a.Method, b);
		GenericInstMethodSig genericInstMethodSig = a.GenericInstMethodSig;
		bool result = num3 && genericInstMethodSig != null && Equals(genericInstMethodSig.GenericArguments, b.GetGenericArguments(), b.DeclaringType);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(MethodBase a)
	{
		if ((object)a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int num = GetHashCode_MethodFieldName(a.Name) + GetHashCode_MethodSig(a);
		if (CompareMethodFieldDeclaringType)
		{
			num += GetHashCode(a.DeclaringType);
		}
		recursionCounter.Decrement();
		return num;
	}

	private int GetHashCode_MethodSig(MethodBase a)
	{
		if ((object)a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		if (!CompareMethodFieldDeclaringType && a.DeclaringType.IsGenericButNotGenericTypeDefinition())
		{
			MethodBase methodBase = a;
			a = a.Module.ResolveMethod(a.MetadataToken);
			if (methodBase.IsGenericButNotGenericMethodDefinition())
			{
				a = ((MethodInfo)a).MakeGenericMethod(methodBase.GetGenericArguments());
			}
		}
		int num = GetHashCode_CallingConvention(a.CallingConvention, a.IsGenericMethod) + GetHashCode(a.GetParameters(), a.DeclaringType);
		if (!DontCompareReturnType)
		{
			num += GetHashCode_ReturnType(a);
		}
		if (a.IsGenericMethod)
		{
			num += GetHashCode_ElementType_MVar(a.GetGenericArguments().Length);
		}
		recursionCounter.Decrement();
		return num;
	}

	private int GetHashCode(IList<ParameterInfo> a, Type declaringType)
	{
		if (a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		uint num = 0u;
		for (int i = 0; i < a.Count; i++)
		{
			num += (uint)GetHashCode(a[i], declaringType);
			num = (num << 13) | (num >> 19);
		}
		recursionCounter.Decrement();
		return (int)num;
	}

	private int GetHashCode_ReturnType(MethodBase a)
	{
		if (a is MethodInfo methodInfo)
		{
			return GetHashCode(methodInfo.ReturnParameter, a.DeclaringType);
		}
		return GetHashCode(typeof(void));
	}

	private int GetHashCode(ParameterInfo a, Type declaringType)
	{
		return GetHashCode(a.ParameterType, declaringType);
	}

	private static bool Equals(CallingConvention a, MethodBase b)
	{
		CallingConventions callingConvention = b.CallingConvention;
		if ((a & CallingConvention.Generic) != 0 != b.IsGenericMethod)
		{
			return false;
		}
		if ((a & CallingConvention.HasThis) != 0 != ((callingConvention & CallingConventions.HasThis) != 0))
		{
			return false;
		}
		if ((a & CallingConvention.ExplicitThis) != 0 != ((callingConvention & CallingConventions.ExplicitThis) != 0))
		{
			return false;
		}
		CallingConvention callingConvention2 = a & CallingConvention.Mask;
		switch (callingConvention & CallingConventions.Any)
		{
		case CallingConventions.Standard:
			if (callingConvention2 == CallingConvention.VarArg || callingConvention2 == CallingConvention.NativeVarArg)
			{
				return false;
			}
			break;
		case CallingConventions.VarArgs:
			if (callingConvention2 != CallingConvention.VarArg && callingConvention2 != CallingConvention.NativeVarArg)
			{
				return false;
			}
			break;
		}
		return true;
	}

	private static int GetHashCode_CallingConvention(CallingConventions a, bool isGeneric)
	{
		CallingConvention callingConvention = CallingConvention.Default;
		if (isGeneric)
		{
			callingConvention |= CallingConvention.Generic;
		}
		if ((a & CallingConventions.HasThis) != 0)
		{
			callingConvention |= CallingConvention.HasThis;
		}
		if ((a & CallingConventions.ExplicitThis) != 0)
		{
			callingConvention |= CallingConvention.ExplicitThis;
		}
		return (int)callingConvention;
	}

	private bool ReturnTypeEquals(TypeSig a, MethodBase b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(b is MethodInfo methodInfo)) ? (b is ConstructorInfo && IsSystemVoid(a)) : Equals(a, methodInfo.ReturnParameter, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	private static bool IsSystemVoid(TypeSig a)
	{
		return a.RemovePinnedAndModifiers().GetElementType() == ElementType.Void;
	}

	private bool Equals(IList<TypeSig> a, IList<ParameterInfo> b, Type declaringType)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (a.Count != b.Count)
		{
			result = false;
		}
		else
		{
			int i;
			for (i = 0; i < a.Count && Equals(a[i], b[i], declaringType); i++)
			{
			}
			result = i == a.Count;
		}
		recursionCounter.Decrement();
		return result;
	}

	private bool Equals(TypeSig a, ParameterInfo b, Type declaringType)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		TypeSig aAfterModifiers;
		bool result = ModifiersEquals(a, b.GetRequiredCustomModifiers(), b.GetOptionalCustomModifiers(), out aAfterModifiers) && Equals(aAfterModifiers, b.ParameterType, declaringType);
		recursionCounter.Decrement();
		return result;
	}

	private bool ModifiersEquals(TypeSig a, IList<Type> reqMods2, IList<Type> optMods2, out TypeSig aAfterModifiers)
	{
		aAfterModifiers = a;
		if (!(a is ModifierSig))
		{
			if (reqMods2.Count == 0)
			{
				return optMods2.Count == 0;
			}
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		List<ITypeDefOrRef> list = new List<ITypeDefOrRef>(reqMods2.Count);
		List<ITypeDefOrRef> list2 = new List<ITypeDefOrRef>(optMods2.Count);
		while (aAfterModifiers is ModifierSig modifierSig)
		{
			if (modifierSig is CModOptSig)
			{
				list2.Add(modifierSig.Modifier);
			}
			else
			{
				list.Add(modifierSig.Modifier);
			}
			aAfterModifiers = aAfterModifiers.Next;
		}
		list2.Reverse();
		list.Reverse();
		bool result = list.Count == reqMods2.Count && list2.Count == optMods2.Count && ModifiersEquals(list, reqMods2) && ModifiersEquals(list2, optMods2);
		recursionCounter.Decrement();
		return result;
	}

	private bool ModifiersEquals(IList<ITypeDefOrRef> a, IList<Type> b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result;
		if (a.Count != b.Count)
		{
			result = false;
		}
		else
		{
			int i;
			for (i = 0; i < b.Count && Equals(a[i], b[i]); i++)
			{
			}
			result = i == b.Count;
		}
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(FieldInfo a, IField b)
	{
		return Equals(b, a);
	}

	public bool Equals(IField a, FieldInfo b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = ((!(a is FieldDef a2)) ? (a is MemberRef a3 && Equals(a3, b)) : Equals(a2, b));
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(FieldInfo a, FieldDef b)
	{
		return Equals(b, a);
	}

	public bool Equals(FieldDef a, FieldInfo b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals_MethodFieldNames(a.Name, b.Name) && Equals(a.FieldSig, b) && (!CompareMethodFieldDeclaringType || Equals(a.DeclaringType, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	private bool Equals(FieldSig a, FieldInfo b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		if (!CompareMethodFieldDeclaringType && b.DeclaringType.IsGenericButNotGenericTypeDefinition())
		{
			b = b.Module.ResolveField(b.MetadataToken);
		}
		TypeSig aAfterModifiers;
		bool result = ModifiersEquals(a.Type, b.GetRequiredCustomModifiers(), b.GetOptionalCustomModifiers(), out aAfterModifiers) && Equals(aAfterModifiers, b.FieldType, b.DeclaringType);
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(FieldInfo a, MemberRef b)
	{
		return Equals(b, a);
	}

	public bool Equals(MemberRef a, FieldInfo b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool flag = Equals_MethodFieldNames(a.Name, b.Name);
		GenericInstSig genericInstanceType;
		if (CompareMethodFieldDeclaringType && !DontSubstituteGenericParameters && (genericInstanceType = GetGenericInstanceType(a.Class)) != null)
		{
			InitializeGenericArguments();
			genericArguments.PushTypeArgs(genericInstanceType.GenericArguments);
			flag = flag && Equals(a.FieldSig, b);
			genericArguments.PopTypeArgs();
		}
		else
		{
			flag = flag && Equals(a.FieldSig, b);
		}
		flag = flag && (!CompareMethodFieldDeclaringType || Equals(a.Class, b.DeclaringType, b.Module));
		recursionCounter.Decrement();
		return flag;
	}

	public int GetHashCode(FieldInfo a)
	{
		if ((object)a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int num = GetHashCode_MethodFieldName(a.Name) + GetHashCode_FieldSig(a);
		if (CompareMethodFieldDeclaringType)
		{
			num += GetHashCode(a.DeclaringType);
		}
		recursionCounter.Decrement();
		return num;
	}

	private int GetHashCode_FieldSig(FieldInfo a)
	{
		if ((object)a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		if (!CompareMethodFieldDeclaringType && a.DeclaringType.IsGenericButNotGenericTypeDefinition())
		{
			a = a.Module.ResolveField(a.MetadataToken);
		}
		int result = GetHashCode_CallingConvention((CallingConventions)0, isGeneric: false) + GetHashCode(a.FieldType, a.DeclaringType);
		recursionCounter.Decrement();
		return result;
	}

	public bool Equals(PropertyDef a, PropertyInfo b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals_PropertyNames(a.Name, b.Name) && Equals(a.PropertySig, b) && (!ComparePropertyDeclaringType || Equals(a.DeclaringType, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	private bool Equals(PropertySig a, PropertyInfo b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		TypeSig aAfterModifiers;
		bool result = ModifiersEquals(a.RetType, b.GetRequiredCustomModifiers(), b.GetOptionalCustomModifiers(), out aAfterModifiers) && Equals(aAfterModifiers, b.PropertyType, b.DeclaringType);
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(PropertyInfo a)
	{
		if ((object)a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int num = GetHashCode_PropertyName(a.Name) + GetHashCode(a.PropertyType, a.DeclaringType);
		if (ComparePropertyDeclaringType)
		{
			num += GetHashCode(a.DeclaringType);
		}
		recursionCounter.Decrement();
		return num;
	}

	public bool Equals(EventDef a, EventInfo b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || (object)b == null)
		{
			return false;
		}
		if (!recursionCounter.Increment())
		{
			return false;
		}
		bool result = Equals_EventNames(a.Name, b.Name) && Equals(a.EventType, b.EventHandlerType, b.DeclaringType) && (!CompareEventDeclaringType || Equals(a.DeclaringType, b.DeclaringType));
		recursionCounter.Decrement();
		return result;
	}

	public int GetHashCode(EventInfo a)
	{
		if ((object)a == null)
		{
			return 0;
		}
		if (!recursionCounter.Increment())
		{
			return 0;
		}
		int num = GetHashCode_EventName(a.Name) + GetHashCode(a.EventHandlerType, a.DeclaringType);
		if (CompareEventDeclaringType)
		{
			num += GetHashCode(a.DeclaringType);
		}
		recursionCounter.Decrement();
		return num;
	}

	public override string ToString()
	{
		return $"{recursionCounter} - {options}";
	}

	private static bool InSameModule(IOwnerModule a, IOwnerModule b)
	{
		ModuleDef module = a.Module;
		if (module != null)
		{
			return module == b.Module;
		}
		return false;
	}
}
