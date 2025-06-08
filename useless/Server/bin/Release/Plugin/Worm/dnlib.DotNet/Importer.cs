using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace dnlib.DotNet;

[ComVisible(true)]
public struct Importer
{
	private readonly ModuleDef module;

	internal readonly GenericParamContext gpContext;

	private readonly ImportMapper mapper;

	private RecursionCounter recursionCounter;

	private ImporterOptions options;

	private bool TryToUseTypeDefs => (options & ImporterOptions.TryToUseTypeDefs) != 0;

	private bool TryToUseMethodDefs => (options & ImporterOptions.TryToUseMethodDefs) != 0;

	private bool TryToUseFieldDefs => (options & ImporterOptions.TryToUseFieldDefs) != 0;

	private bool TryToUseExistingAssemblyRefs => (options & ImporterOptions.TryToUseExistingAssemblyRefs) != 0;

	private bool FixSignature
	{
		get
		{
			return (options & ImporterOptions.FixSignature) != 0;
		}
		set
		{
			if (value)
			{
				options |= ImporterOptions.FixSignature;
			}
			else
			{
				options &= (ImporterOptions)2147483647;
			}
		}
	}

	public Importer(ModuleDef module)
		: this(module, (ImporterOptions)0, default(GenericParamContext), null)
	{
	}

	public Importer(ModuleDef module, GenericParamContext gpContext)
		: this(module, (ImporterOptions)0, gpContext, null)
	{
	}

	public Importer(ModuleDef module, ImporterOptions options)
		: this(module, options, default(GenericParamContext), null)
	{
	}

	public Importer(ModuleDef module, ImporterOptions options, GenericParamContext gpContext)
		: this(module, options, gpContext, null)
	{
	}

	public Importer(ModuleDef module, ImporterOptions options, GenericParamContext gpContext, ImportMapper mapper)
	{
		this.module = module;
		recursionCounter = default(RecursionCounter);
		this.options = options;
		this.gpContext = gpContext;
		this.mapper = mapper;
	}

	public ITypeDefOrRef Import(Type type)
	{
		return module.UpdateRowId(ImportAsTypeSig(type).ToTypeDefOrRef());
	}

	[Obsolete("Use 'Import(Type)' instead.")]
	public ITypeDefOrRef ImportDeclaringType(Type type)
	{
		return Import(type);
	}

	public ITypeDefOrRef Import(Type type, IList<Type> requiredModifiers, IList<Type> optionalModifiers)
	{
		return module.UpdateRowId(ImportAsTypeSig(type, requiredModifiers, optionalModifiers).ToTypeDefOrRef());
	}

	public TypeSig ImportAsTypeSig(Type type)
	{
		return ImportAsTypeSig(type, null, false);
	}

	private TypeSig ImportAsTypeSig(Type type, Type declaringType, bool? treatAsGenericInst = null)
	{
		if ((object)type == null)
		{
			return null;
		}
		switch ((treatAsGenericInst ?? declaringType.MustTreatTypeAsGenericInstType(type)) ? ElementType.GenericInst : type.GetElementType2())
		{
		case ElementType.Void:
			return module.CorLibTypes.Void;
		case ElementType.Boolean:
			return module.CorLibTypes.Boolean;
		case ElementType.Char:
			return module.CorLibTypes.Char;
		case ElementType.I1:
			return module.CorLibTypes.SByte;
		case ElementType.U1:
			return module.CorLibTypes.Byte;
		case ElementType.I2:
			return module.CorLibTypes.Int16;
		case ElementType.U2:
			return module.CorLibTypes.UInt16;
		case ElementType.I4:
			return module.CorLibTypes.Int32;
		case ElementType.U4:
			return module.CorLibTypes.UInt32;
		case ElementType.I8:
			return module.CorLibTypes.Int64;
		case ElementType.U8:
			return module.CorLibTypes.UInt64;
		case ElementType.R4:
			return module.CorLibTypes.Single;
		case ElementType.R8:
			return module.CorLibTypes.Double;
		case ElementType.String:
			return module.CorLibTypes.String;
		case ElementType.TypedByRef:
			return module.CorLibTypes.TypedReference;
		case ElementType.U:
			return module.CorLibTypes.UIntPtr;
		case ElementType.Object:
			return module.CorLibTypes.Object;
		case ElementType.Ptr:
			return new PtrSig(ImportAsTypeSig(type.GetElementType(), declaringType));
		case ElementType.ByRef:
			return new ByRefSig(ImportAsTypeSig(type.GetElementType(), declaringType));
		case ElementType.SZArray:
			return new SZArraySig(ImportAsTypeSig(type.GetElementType(), declaringType));
		case ElementType.ValueType:
			return new ValueTypeSig(CreateTypeDefOrRef(type));
		case ElementType.Class:
			return new ClassSig(CreateTypeDefOrRef(type));
		case ElementType.Var:
			return new GenericVar((uint)type.GenericParameterPosition, gpContext.Type);
		case ElementType.MVar:
			return new GenericMVar((uint)type.GenericParameterPosition, gpContext.Method);
		case ElementType.I:
			FixSignature = true;
			return module.CorLibTypes.IntPtr;
		case ElementType.Array:
		{
			int[] lowerBounds = new int[type.GetArrayRank()];
			uint[] sizes = Array2.Empty<uint>();
			FixSignature = true;
			return new ArraySig(ImportAsTypeSig(type.GetElementType(), declaringType), (uint)type.GetArrayRank(), sizes, lowerBounds);
		}
		case ElementType.GenericInst:
		{
			Type[] genericArguments = type.GetGenericArguments();
			GenericInstSig genericInstSig = new GenericInstSig(ImportAsTypeSig(type.GetGenericTypeDefinition(), null, false) as ClassOrValueTypeSig, (uint)genericArguments.Length);
			Type[] array = genericArguments;
			foreach (Type type2 in array)
			{
				genericInstSig.GenericArguments.Add(ImportAsTypeSig(type2, declaringType));
			}
			return genericInstSig;
		}
		default:
			return null;
		}
	}

	private ITypeDefOrRef TryResolve(TypeRef tr)
	{
		if (!TryToUseTypeDefs || tr == null)
		{
			return tr;
		}
		if (!IsThisModule(tr))
		{
			return tr;
		}
		TypeDef typeDef = tr.Resolve();
		if (typeDef == null || typeDef.Module != module)
		{
			return tr;
		}
		return typeDef;
	}

	private IMethodDefOrRef TryResolveMethod(IMethodDefOrRef mdr)
	{
		if (!TryToUseMethodDefs || mdr == null)
		{
			return mdr;
		}
		if (!(mdr is MemberRef memberRef))
		{
			return mdr;
		}
		if (!memberRef.IsMethodRef)
		{
			return memberRef;
		}
		TypeDef declaringType = GetDeclaringType(memberRef);
		if (declaringType == null)
		{
			return memberRef;
		}
		if (declaringType.Module != module)
		{
			return memberRef;
		}
		IMethodDefOrRef methodDefOrRef = declaringType.ResolveMethod(memberRef);
		return methodDefOrRef ?? memberRef;
	}

	private IField TryResolveField(MemberRef mr)
	{
		if (!TryToUseFieldDefs || mr == null)
		{
			return mr;
		}
		if (!mr.IsFieldRef)
		{
			return mr;
		}
		TypeDef declaringType = GetDeclaringType(mr);
		if (declaringType == null)
		{
			return mr;
		}
		if (declaringType.Module != module)
		{
			return mr;
		}
		IField field = declaringType.ResolveField(mr);
		return field ?? mr;
	}

	private TypeDef GetDeclaringType(MemberRef mr)
	{
		if (mr == null)
		{
			return null;
		}
		if (mr.Class is TypeDef result)
		{
			return result;
		}
		if (TryResolve(mr.Class as TypeRef) is TypeDef result2)
		{
			return result2;
		}
		ModuleRef modRef = mr.Class as ModuleRef;
		if (IsThisModule(modRef))
		{
			return module.GlobalType;
		}
		return null;
	}

	private bool IsThisModule(TypeRef tr)
	{
		if (tr == null)
		{
			return false;
		}
		if (!(tr.GetNonNestedTypeRefScope() is TypeRef typeRef))
		{
			return false;
		}
		if (module == typeRef.ResolutionScope)
		{
			return true;
		}
		if (typeRef.ResolutionScope is ModuleRef modRef)
		{
			return IsThisModule(modRef);
		}
		AssemblyRef b = typeRef.ResolutionScope as AssemblyRef;
		return Equals(module.Assembly, b);
	}

	private bool IsThisModule(ModuleRef modRef)
	{
		if (modRef != null && module.Name == modRef.Name)
		{
			return Equals(module.Assembly, modRef.DefinitionAssembly);
		}
		return false;
	}

	private static bool Equals(IAssembly a, IAssembly b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (Utils.Equals(a.Version, b.Version) && PublicKeyBase.TokenEquals(a.PublicKeyOrToken, b.PublicKeyOrToken) && UTF8String.Equals(a.Name, b.Name))
		{
			return UTF8String.CaseInsensitiveEquals(a.Culture, b.Culture);
		}
		return false;
	}

	private ITypeDefOrRef CreateTypeDefOrRef(Type type)
	{
		ITypeDefOrRef typeDefOrRef = mapper?.Map(type);
		if (typeDefOrRef is TypeSpec)
		{
			throw new InvalidOperationException();
		}
		if (typeDefOrRef is TypeDef result)
		{
			return result;
		}
		if (typeDefOrRef is TypeRef tr)
		{
			return TryResolve(tr);
		}
		if (TryToUseTypeDefs && IsThisModule(type.Module) && module.ResolveToken(type.MetadataToken) is TypeDef result2)
		{
			return result2;
		}
		return TryResolve(CreateTypeRef(type));
	}

	private TypeRef CreateTypeRef(Type type)
	{
		if (!type.IsNested)
		{
			return module.UpdateRowId(new TypeRefUser(module, type.Namespace ?? string.Empty, ReflectionExtensions.Unescape(type.Name) ?? string.Empty, CreateScopeReference(type)));
		}
		type.GetTypeNamespaceAndName_TypeDefOrRef(out var @namespace, out var name);
		return module.UpdateRowId(new TypeRefUser(module, @namespace ?? string.Empty, name ?? string.Empty, CreateTypeRef(type.DeclaringType)));
	}

	private IResolutionScope CreateScopeReference(Type type)
	{
		if ((object)type == null)
		{
			return null;
		}
		AssemblyName name = type.Assembly.GetName();
		AssemblyDef assembly = module.Assembly;
		if (assembly != null && UTF8String.ToSystemStringOrEmpty(assembly.Name).Equals(name.Name, StringComparison.OrdinalIgnoreCase))
		{
			if (UTF8String.ToSystemStringOrEmpty(module.Name).Equals(type.Module.ScopeName, StringComparison.OrdinalIgnoreCase))
			{
				return module;
			}
			return module.UpdateRowId(new ModuleRefUser(module, type.Module.ScopeName));
		}
		byte[] array = name.GetPublicKeyToken();
		if (array == null || array.Length == 0)
		{
			array = null;
		}
		if (TryToUseExistingAssemblyRefs)
		{
			AssemblyRef assemblyRef = module.GetAssemblyRef(name.Name);
			if (assemblyRef != null)
			{
				return assemblyRef;
			}
		}
		return module.UpdateRowId(new AssemblyRefUser(name.Name, name.Version, PublicKeyBase.CreatePublicKeyToken(array), name.CultureInfo?.Name ?? string.Empty));
	}

	public TypeSig ImportAsTypeSig(Type type, IList<Type> requiredModifiers, IList<Type> optionalModifiers)
	{
		return ImportAsTypeSig(type, requiredModifiers, optionalModifiers, null);
	}

	private TypeSig ImportAsTypeSig(Type type, IList<Type> requiredModifiers, IList<Type> optionalModifiers, Type declaringType)
	{
		if ((object)type == null)
		{
			return null;
		}
		if (IsEmpty(requiredModifiers) && IsEmpty(optionalModifiers))
		{
			return ImportAsTypeSig(type, declaringType);
		}
		FixSignature = true;
		TypeSig typeSig = ImportAsTypeSig(type, declaringType);
		if (requiredModifiers != null)
		{
			foreach (Type requiredModifier in requiredModifiers)
			{
				typeSig = new CModReqdSig(Import(requiredModifier), typeSig);
			}
		}
		if (optionalModifiers != null)
		{
			foreach (Type optionalModifier in optionalModifiers)
			{
				typeSig = new CModOptSig(Import(optionalModifier), typeSig);
			}
		}
		return typeSig;
	}

	private static bool IsEmpty<T>(IList<T> list)
	{
		if (list != null)
		{
			return list.Count == 0;
		}
		return true;
	}

	public IMethod Import(MethodBase methodBase)
	{
		return Import(methodBase, forceFixSignature: false);
	}

	public IMethod Import(MethodBase methodBase, bool forceFixSignature)
	{
		FixSignature = false;
		return ImportInternal(methodBase, forceFixSignature);
	}

	private IMethod ImportInternal(MethodBase methodBase)
	{
		return ImportInternal(methodBase, forceFixSignature: false);
	}

	private IMethod ImportInternal(MethodBase methodBase, bool forceFixSignature)
	{
		if ((object)methodBase == null)
		{
			return null;
		}
		if (TryToUseMethodDefs && IsThisModule(methodBase.Module) && !methodBase.IsGenericMethod && ((object)methodBase.DeclaringType == null || !methodBase.DeclaringType.IsGenericType) && module.ResolveToken(methodBase.MetadataToken) is MethodDef result)
		{
			return result;
		}
		if (methodBase.IsGenericButNotGenericMethodDefinition())
		{
			MethodBase methodBase2 = methodBase.Module.ResolveMethod(methodBase.MetadataToken);
			IMethodDefOrRef mdr = ((methodBase.DeclaringType.GetElementType2() != ElementType.GenericInst) ? (ImportInternal(methodBase2) as IMethodDefOrRef) : module.UpdateRowId(new MemberRefUser(module, methodBase.Name, CreateMethodSig(methodBase2), Import(methodBase.DeclaringType))));
			mdr = TryResolveMethod(mdr);
			if (methodBase.ContainsGenericParameters)
			{
				return mdr;
			}
			GenericInstMethodSig sig = CreateGenericInstMethodSig(methodBase);
			MethodSpecUser result2 = module.UpdateRowId(new MethodSpecUser(mdr, sig));
			if (FixSignature)
			{
			}
			return result2;
		}
		IMemberRefParent memberRefParent = (((object)methodBase.DeclaringType != null) ? Import(methodBase.DeclaringType) : GetModuleParent(methodBase.Module));
		if (memberRefParent == null)
		{
			return null;
		}
		MethodBase mb;
		try
		{
			mb = methodBase.Module.ResolveMethod(methodBase.MetadataToken);
		}
		catch (ArgumentException)
		{
			mb = methodBase;
		}
		MethodSig sig2 = CreateMethodSig(mb);
		IMethodDefOrRef mdr2 = module.UpdateRowId(new MemberRefUser(module, methodBase.Name, sig2, memberRefParent));
		mdr2 = TryResolveMethod(mdr2);
		if (FixSignature)
		{
		}
		return mdr2;
	}

	private bool IsThisModule(Module module2)
	{
		if (UTF8String.ToSystemStringOrEmpty(module.Name).Equals(module2.ScopeName, StringComparison.OrdinalIgnoreCase))
		{
			return IsThisAssembly(module2);
		}
		return false;
	}

	private MethodSig CreateMethodSig(MethodBase mb)
	{
		MethodSig methodSig = new MethodSig(GetCallingConvention(mb));
		if (mb is MethodInfo methodInfo)
		{
			methodSig.RetType = ImportAsTypeSig(methodInfo.ReturnParameter, mb.DeclaringType);
		}
		else
		{
			methodSig.RetType = module.CorLibTypes.Void;
		}
		ParameterInfo[] parameters = mb.GetParameters();
		foreach (ParameterInfo p in parameters)
		{
			methodSig.Params.Add(ImportAsTypeSig(p, mb.DeclaringType));
		}
		if (mb.IsGenericMethodDefinition)
		{
			methodSig.GenParamCount = (uint)mb.GetGenericArguments().Length;
		}
		return methodSig;
	}

	private TypeSig ImportAsTypeSig(ParameterInfo p, Type declaringType)
	{
		return ImportAsTypeSig(p.ParameterType, p.GetRequiredCustomModifiers(), p.GetOptionalCustomModifiers(), declaringType);
	}

	private CallingConvention GetCallingConvention(MethodBase mb)
	{
		CallingConvention callingConvention = CallingConvention.Default;
		CallingConventions callingConvention2 = mb.CallingConvention;
		if (mb.IsGenericMethodDefinition)
		{
			callingConvention |= CallingConvention.Generic;
		}
		if ((callingConvention2 & CallingConventions.HasThis) != 0)
		{
			callingConvention |= CallingConvention.HasThis;
		}
		if ((callingConvention2 & CallingConventions.ExplicitThis) != 0)
		{
			callingConvention |= CallingConvention.ExplicitThis;
		}
		switch (callingConvention2 & CallingConventions.Any)
		{
		case CallingConventions.Standard:
			return callingConvention | CallingConvention.Default;
		case CallingConventions.VarArgs:
			return callingConvention | CallingConvention.VarArg;
		default:
			FixSignature = true;
			return callingConvention | CallingConvention.Default;
		}
	}

	private GenericInstMethodSig CreateGenericInstMethodSig(MethodBase mb)
	{
		Type[] genericArguments = mb.GetGenericArguments();
		GenericInstMethodSig genericInstMethodSig = new GenericInstMethodSig(CallingConvention.GenericInst, (uint)genericArguments.Length);
		Type[] array = genericArguments;
		foreach (Type type in array)
		{
			genericInstMethodSig.GenericArguments.Add(ImportAsTypeSig(type));
		}
		return genericInstMethodSig;
	}

	private IMemberRefParent GetModuleParent(Module module2)
	{
		if (!IsThisAssembly(module2))
		{
			return null;
		}
		return module.UpdateRowId(new ModuleRefUser(module, module.Name));
	}

	private bool IsThisAssembly(Module module2)
	{
		AssemblyDef assembly = module.Assembly;
		if (assembly != null)
		{
			return UTF8String.ToSystemStringOrEmpty(assembly.Name).Equals(module2.Assembly.GetName().Name, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public IField Import(FieldInfo fieldInfo)
	{
		return Import(fieldInfo, forceFixSignature: false);
	}

	public IField Import(FieldInfo fieldInfo, bool forceFixSignature)
	{
		FixSignature = false;
		if ((object)fieldInfo == null)
		{
			return null;
		}
		if (TryToUseFieldDefs && IsThisModule(fieldInfo.Module) && ((object)fieldInfo.DeclaringType == null || !fieldInfo.DeclaringType.IsGenericType) && module.ResolveToken(fieldInfo.MetadataToken) is FieldDef result)
		{
			return result;
		}
		IMemberRefParent memberRefParent = (((object)fieldInfo.DeclaringType != null) ? Import(fieldInfo.DeclaringType) : GetModuleParent(fieldInfo.Module));
		if (memberRefParent == null)
		{
			return null;
		}
		FieldInfo fieldInfo2;
		try
		{
			fieldInfo2 = fieldInfo.Module.ResolveField(fieldInfo.MetadataToken);
		}
		catch (ArgumentException)
		{
			fieldInfo2 = fieldInfo;
		}
		FieldSig sig = new FieldSig(ImportAsTypeSig(fieldInfo2.FieldType, fieldInfo2.GetRequiredCustomModifiers(), fieldInfo2.GetOptionalCustomModifiers(), fieldInfo2.DeclaringType));
		MemberRefUser mr = module.UpdateRowId(new MemberRefUser(module, fieldInfo.Name, sig, memberRefParent));
		IField result2 = TryResolveField(mr);
		if (FixSignature)
		{
		}
		return result2;
	}

	public IType Import(IType type)
	{
		if (type == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IType result = ((!(type is TypeDef type2)) ? ((!(type is TypeRef type3)) ? ((!(type is TypeSpec type4)) ? ((IType)((!(type is TypeSig type5)) ? null : Import(type5))) : ((IType)Import(type4))) : Import(type3)) : Import(type2));
		recursionCounter.Decrement();
		return result;
	}

	public ITypeDefOrRef Import(TypeDef type)
	{
		if (type == null)
		{
			return null;
		}
		if (TryToUseTypeDefs && type.Module == module)
		{
			return type;
		}
		ITypeDefOrRef typeDefOrRef = mapper?.Map(type);
		if (typeDefOrRef != null)
		{
			return typeDefOrRef;
		}
		return Import2(type);
	}

	private TypeRef Import2(TypeDef type)
	{
		if (type == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		TypeDef declaringType = type.DeclaringType;
		TypeRef result = ((declaringType == null) ? module.UpdateRowId(new TypeRefUser(module, type.Namespace, type.Name, CreateScopeReference(type.DefinitionAssembly, type.Module))) : module.UpdateRowId(new TypeRefUser(module, type.Namespace, type.Name, Import2(declaringType))));
		recursionCounter.Decrement();
		return result;
	}

	private IResolutionScope CreateScopeReference(IAssembly defAsm, ModuleDef defMod)
	{
		if (defAsm == null)
		{
			return null;
		}
		AssemblyDef assembly = module.Assembly;
		if (defMod != null && defAsm != null && assembly != null && UTF8String.CaseInsensitiveEquals(assembly.Name, defAsm.Name))
		{
			if (UTF8String.CaseInsensitiveEquals(module.Name, defMod.Name))
			{
				return module;
			}
			return module.UpdateRowId(new ModuleRefUser(module, defMod.Name));
		}
		PublicKeyToken publicKeyToken = PublicKeyBase.ToPublicKeyToken(defAsm.PublicKeyOrToken);
		if (PublicKeyBase.IsNullOrEmpty2(publicKeyToken))
		{
			publicKeyToken = null;
		}
		if (TryToUseExistingAssemblyRefs)
		{
			AssemblyRef assemblyRef = module.GetAssemblyRef(defAsm.Name);
			if (assemblyRef != null)
			{
				return assemblyRef;
			}
		}
		return module.UpdateRowId(new AssemblyRefUser(defAsm.Name, defAsm.Version, publicKeyToken, defAsm.Culture)
		{
			Attributes = (defAsm.Attributes & ~AssemblyAttributes.PublicKey)
		});
	}

	public ITypeDefOrRef Import(TypeRef type)
	{
		ITypeDefOrRef typeDefOrRef = mapper?.Map(type);
		if (typeDefOrRef != null)
		{
			return typeDefOrRef;
		}
		return TryResolve(Import2(type));
	}

	private TypeRef Import2(TypeRef type)
	{
		if (type == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		TypeRef declaringType = type.DeclaringType;
		TypeRef result = ((declaringType == null) ? module.UpdateRowId(new TypeRefUser(module, type.Namespace, type.Name, CreateScopeReference(type.DefinitionAssembly, type.Module))) : module.UpdateRowId(new TypeRefUser(module, type.Namespace, type.Name, Import2(declaringType))));
		recursionCounter.Decrement();
		return result;
	}

	public TypeSpec Import(TypeSpec type)
	{
		if (type == null)
		{
			return null;
		}
		return module.UpdateRowId(new TypeSpecUser(Import(type.TypeSig)));
	}

	public TypeSig Import(TypeSig type)
	{
		if (type == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		TypeSig result;
		switch (type.ElementType)
		{
		case ElementType.Void:
			result = module.CorLibTypes.Void;
			break;
		case ElementType.Boolean:
			result = module.CorLibTypes.Boolean;
			break;
		case ElementType.Char:
			result = module.CorLibTypes.Char;
			break;
		case ElementType.I1:
			result = module.CorLibTypes.SByte;
			break;
		case ElementType.U1:
			result = module.CorLibTypes.Byte;
			break;
		case ElementType.I2:
			result = module.CorLibTypes.Int16;
			break;
		case ElementType.U2:
			result = module.CorLibTypes.UInt16;
			break;
		case ElementType.I4:
			result = module.CorLibTypes.Int32;
			break;
		case ElementType.U4:
			result = module.CorLibTypes.UInt32;
			break;
		case ElementType.I8:
			result = module.CorLibTypes.Int64;
			break;
		case ElementType.U8:
			result = module.CorLibTypes.UInt64;
			break;
		case ElementType.R4:
			result = module.CorLibTypes.Single;
			break;
		case ElementType.R8:
			result = module.CorLibTypes.Double;
			break;
		case ElementType.String:
			result = module.CorLibTypes.String;
			break;
		case ElementType.TypedByRef:
			result = module.CorLibTypes.TypedReference;
			break;
		case ElementType.I:
			result = module.CorLibTypes.IntPtr;
			break;
		case ElementType.U:
			result = module.CorLibTypes.UIntPtr;
			break;
		case ElementType.Object:
			result = module.CorLibTypes.Object;
			break;
		case ElementType.Ptr:
			result = new PtrSig(Import(type.Next));
			break;
		case ElementType.ByRef:
			result = new ByRefSig(Import(type.Next));
			break;
		case ElementType.ValueType:
			result = CreateClassOrValueType((type as ClassOrValueTypeSig).TypeDefOrRef, isValueType: true);
			break;
		case ElementType.Class:
			result = CreateClassOrValueType((type as ClassOrValueTypeSig).TypeDefOrRef, isValueType: false);
			break;
		case ElementType.Var:
			result = new GenericVar((type as GenericVar).Number, gpContext.Type);
			break;
		case ElementType.ValueArray:
			result = new ValueArraySig(Import(type.Next), (type as ValueArraySig).Size);
			break;
		case ElementType.FnPtr:
			result = new FnPtrSig(Import((type as FnPtrSig).Signature));
			break;
		case ElementType.SZArray:
			result = new SZArraySig(Import(type.Next));
			break;
		case ElementType.MVar:
			result = new GenericMVar((type as GenericMVar).Number, gpContext.Method);
			break;
		case ElementType.CModReqd:
			result = new CModReqdSig(Import((type as ModifierSig).Modifier), Import(type.Next));
			break;
		case ElementType.CModOpt:
			result = new CModOptSig(Import((type as ModifierSig).Modifier), Import(type.Next));
			break;
		case ElementType.Module:
			result = new ModuleSig((type as ModuleSig).Index, Import(type.Next));
			break;
		case ElementType.Sentinel:
			result = new SentinelSig();
			break;
		case ElementType.Pinned:
			result = new PinnedSig(Import(type.Next));
			break;
		case ElementType.Array:
		{
			ArraySig arraySig = (ArraySig)type;
			List<uint> sizes = new List<uint>(arraySig.Sizes);
			List<int> lowerBounds = new List<int>(arraySig.LowerBounds);
			result = new ArraySig(Import(type.Next), arraySig.Rank, sizes, lowerBounds);
			break;
		}
		case ElementType.GenericInst:
		{
			GenericInstSig genericInstSig = (GenericInstSig)type;
			List<TypeSig> list = new List<TypeSig>(genericInstSig.GenericArguments.Count);
			foreach (TypeSig genericArgument in genericInstSig.GenericArguments)
			{
				list.Add(Import(genericArgument));
			}
			result = new GenericInstSig(Import(genericInstSig.GenericType) as ClassOrValueTypeSig, list);
			break;
		}
		default:
			result = null;
			break;
		}
		recursionCounter.Decrement();
		return result;
	}

	public ITypeDefOrRef Import(ITypeDefOrRef type)
	{
		return (ITypeDefOrRef)Import((IType)type);
	}

	private TypeSig CreateClassOrValueType(ITypeDefOrRef type, bool isValueType)
	{
		CorLibTypeSig corLibTypeSig = module.CorLibTypes.GetCorLibTypeSig(type);
		if (corLibTypeSig != null)
		{
			return corLibTypeSig;
		}
		if (isValueType)
		{
			return new ValueTypeSig(Import(type));
		}
		return new ClassSig(Import(type));
	}

	public CallingConventionSig Import(CallingConventionSig sig)
	{
		if (sig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		Type type = sig.GetType();
		CallingConventionSig result = (((object)type == typeof(MethodSig)) ? Import((MethodSig)sig) : (((object)type == typeof(FieldSig)) ? Import((FieldSig)sig) : (((object)type == typeof(GenericInstMethodSig)) ? Import((GenericInstMethodSig)sig) : (((object)type == typeof(PropertySig)) ? ((CallingConventionSig)Import((PropertySig)sig)) : ((CallingConventionSig)(((object)type != typeof(LocalSig)) ? null : Import((LocalSig)sig)))))));
		recursionCounter.Decrement();
		return result;
	}

	public FieldSig Import(FieldSig sig)
	{
		if (sig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		FieldSig result = new FieldSig(sig.GetCallingConvention(), Import(sig.Type));
		recursionCounter.Decrement();
		return result;
	}

	public MethodSig Import(MethodSig sig)
	{
		if (sig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		MethodSig result = Import(new MethodSig(sig.GetCallingConvention()), sig);
		recursionCounter.Decrement();
		return result;
	}

	private T Import<T>(T sig, T old) where T : MethodBaseSig
	{
		sig.RetType = Import(old.RetType);
		foreach (TypeSig param in old.Params)
		{
			sig.Params.Add(Import(param));
		}
		sig.GenParamCount = old.GenParamCount;
		IList<TypeSig> paramsAfterSentinel = sig.ParamsAfterSentinel;
		if (paramsAfterSentinel != null)
		{
			foreach (TypeSig item in old.ParamsAfterSentinel)
			{
				paramsAfterSentinel.Add(Import(item));
			}
		}
		return sig;
	}

	public PropertySig Import(PropertySig sig)
	{
		if (sig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		PropertySig result = Import(new PropertySig(sig.GetCallingConvention()), sig);
		recursionCounter.Decrement();
		return result;
	}

	public LocalSig Import(LocalSig sig)
	{
		if (sig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		LocalSig localSig = new LocalSig(sig.GetCallingConvention(), (uint)sig.Locals.Count);
		foreach (TypeSig local in sig.Locals)
		{
			localSig.Locals.Add(Import(local));
		}
		recursionCounter.Decrement();
		return localSig;
	}

	public GenericInstMethodSig Import(GenericInstMethodSig sig)
	{
		if (sig == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		GenericInstMethodSig genericInstMethodSig = new GenericInstMethodSig(sig.GetCallingConvention(), (uint)sig.GenericArguments.Count);
		foreach (TypeSig genericArgument in sig.GenericArguments)
		{
			genericInstMethodSig.GenericArguments.Add(Import(genericArgument));
		}
		recursionCounter.Decrement();
		return genericInstMethodSig;
	}

	public IField Import(IField field)
	{
		if (field == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IField result = ((!(field is FieldDef field2)) ? ((!(field is MemberRef memberRef)) ? null : Import(memberRef)) : Import(field2));
		recursionCounter.Decrement();
		return result;
	}

	public IMethod Import(IMethod method)
	{
		if (method == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IMethod result = ((!(method is MethodDef method2)) ? ((!(method is MethodSpec method3)) ? ((IMethod)((!(method is MemberRef memberRef)) ? null : Import(memberRef))) : ((IMethod)Import(method3))) : Import(method2));
		recursionCounter.Decrement();
		return result;
	}

	public IField Import(FieldDef field)
	{
		if (field == null)
		{
			return null;
		}
		if (TryToUseFieldDefs && field.Module == module)
		{
			return field;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IField field2 = mapper?.Map(field);
		if (field2 != null)
		{
			recursionCounter.Decrement();
			return field2;
		}
		MemberRefUser memberRefUser = module.UpdateRowId(new MemberRefUser(module, field.Name));
		memberRefUser.Signature = Import(field.Signature);
		memberRefUser.Class = ImportParent(field.DeclaringType);
		recursionCounter.Decrement();
		return memberRefUser;
	}

	private IMemberRefParent ImportParent(TypeDef type)
	{
		if (type == null)
		{
			return null;
		}
		if (type.IsGlobalModuleType)
		{
			return module.UpdateRowId(new ModuleRefUser(module, type.Module?.Name));
		}
		return Import(type);
	}

	public IMethod Import(MethodDef method)
	{
		if (method == null)
		{
			return null;
		}
		if (TryToUseMethodDefs && method.Module == module)
		{
			return method;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		IMethod method2 = mapper?.Map(method);
		if (method2 != null)
		{
			recursionCounter.Decrement();
			return method2;
		}
		MemberRefUser memberRefUser = module.UpdateRowId(new MemberRefUser(module, method.Name));
		memberRefUser.Signature = Import(method.Signature);
		memberRefUser.Class = ImportParent(method.DeclaringType);
		recursionCounter.Decrement();
		return memberRefUser;
	}

	public MethodSpec Import(MethodSpec method)
	{
		if (method == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		MethodSpecUser methodSpecUser = module.UpdateRowId(new MethodSpecUser((IMethodDefOrRef)Import(method.Method)));
		methodSpecUser.Instantiation = Import(method.Instantiation);
		recursionCounter.Decrement();
		return methodSpecUser;
	}

	public MemberRef Import(MemberRef memberRef)
	{
		if (memberRef == null)
		{
			return null;
		}
		if (!recursionCounter.Increment())
		{
			return null;
		}
		MemberRef memberRef2 = mapper?.Map(memberRef);
		if (memberRef2 != null)
		{
			recursionCounter.Decrement();
			return memberRef2;
		}
		MemberRef memberRef3 = module.UpdateRowId(new MemberRefUser(module, memberRef.Name));
		memberRef3.Signature = Import(memberRef.Signature);
		memberRef3.Class = Import(memberRef.Class);
		if (memberRef3.Class == null)
		{
			memberRef3 = null;
		}
		recursionCounter.Decrement();
		return memberRef3;
	}

	private IMemberRefParent Import(IMemberRefParent parent)
	{
		if (parent is ITypeDefOrRef typeDefOrRef)
		{
			if (typeDefOrRef is TypeDef { IsGlobalModuleType: not false } typeDef)
			{
				return module.UpdateRowId(new ModuleRefUser(module, typeDef.Module?.Name));
			}
			return Import(typeDefOrRef);
		}
		if (parent is ModuleRef moduleRef)
		{
			return module.UpdateRowId(new ModuleRefUser(module, moduleRef.Name));
		}
		if (parent is MethodDef { DeclaringType: var declaringType } methodDef)
		{
			if (declaringType != null && declaringType.Module == module)
			{
				return methodDef;
			}
			return null;
		}
		return null;
	}
}
