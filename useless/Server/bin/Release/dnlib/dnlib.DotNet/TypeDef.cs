using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Threading;
using dnlib.Utils;

namespace dnlib.DotNet;

public abstract class TypeDef : ITypeDefOrRef, ICodedToken, IMDTokenProvider, IHasCustomAttribute, IMemberRefParent, IFullName, IType, IOwnerModule, IGenericParameterProvider, IIsTypeOrMethod, IContainsGenericParameter, ITokenOperand, IMemberRef, IHasDeclSecurity, ITypeOrMethodDef, IHasCustomDebugInformation, IListListener<FieldDef>, IListListener<MethodDef>, IListListener<TypeDef>, IListListener<EventDef>, IListListener<PropertyDef>, IListListener<GenericParam>, IMemberRefResolver, IMemberDef, IDnlibDef
{
	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected ModuleDef module2;

	protected bool module2_isInitialized;

	protected int attributes;

	protected UTF8String name;

	protected UTF8String @namespace;

	protected ITypeDefOrRef baseType;

	protected bool baseType_isInitialized;

	protected LazyList<FieldDef> fields;

	protected LazyList<MethodDef> methods;

	protected LazyList<GenericParam> genericParameters;

	protected IList<InterfaceImpl> interfaces;

	protected IList<DeclSecurity> declSecurities;

	protected ClassLayout classLayout;

	protected bool classLayout_isInitialized;

	protected TypeDef declaringType2;

	protected bool declaringType2_isInitialized;

	protected LazyList<TypeDef> nestedTypes;

	protected LazyList<EventDef> events;

	protected LazyList<PropertyDef> properties;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	private static readonly UTF8String systemString = new UTF8String("System");

	private static readonly UTF8String enumString = new UTF8String("Enum");

	private static readonly UTF8String valueTypeString = new UTF8String("ValueType");

	private static readonly UTF8String multicastDelegateString = new UTF8String("MulticastDelegate");

	public MDToken MDToken => new MDToken(Table.TypeDef, rid);

	public uint Rid
	{
		get
		{
			return rid;
		}
		set
		{
			rid = value;
		}
	}

	public int TypeDefOrRefTag => 0;

	public int HasCustomAttributeTag => 3;

	public int HasDeclSecurityTag => 0;

	public int MemberRefParentTag => 0;

	public int TypeOrMethodDefTag => 0;

	int IGenericParameterProvider.NumberOfGenericParameters => GenericParameters.Count;

	string IType.TypeName => Name;

	public string ReflectionName => FullNameFactory.Name(this, isReflection: true);

	string IType.Namespace => Namespace;

	public string ReflectionNamespace => FullNameFactory.Namespace(this, isReflection: true);

	public string FullName => FullNameFactory.FullName(this, isReflection: false);

	public string ReflectionFullName => FullNameFactory.FullName(this, isReflection: true);

	public string AssemblyQualifiedName => FullNameFactory.AssemblyQualifiedName(this);

	public IAssembly DefinitionAssembly => FullNameFactory.DefinitionAssembly(this);

	public IScope Scope => Module;

	public ITypeDefOrRef ScopeType => this;

	public bool ContainsGenericParameter => false;

	public ModuleDef Module => FullNameFactory.OwnerModule(this);

	internal ModuleDef Module2
	{
		get
		{
			if (!module2_isInitialized)
			{
				InitializeModule2();
			}
			return module2;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				module2 = value;
				module2_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	bool IIsTypeOrMethod.IsType => true;

	bool IIsTypeOrMethod.IsMethod => false;

	bool IMemberRef.IsField => false;

	bool IMemberRef.IsTypeSpec => false;

	bool IMemberRef.IsTypeRef => false;

	bool IMemberRef.IsTypeDef => true;

	bool IMemberRef.IsMethodSpec => false;

	bool IMemberRef.IsMethodDef => false;

	bool IMemberRef.IsMemberRef => false;

	bool IMemberRef.IsFieldDef => false;

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => false;

	public TypeAttributes Attributes
	{
		get
		{
			return (TypeAttributes)attributes;
		}
		set
		{
			attributes = (int)value;
		}
	}

	public UTF8String Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public UTF8String Namespace
	{
		get
		{
			return @namespace;
		}
		set
		{
			@namespace = value;
		}
	}

	public ITypeDefOrRef BaseType
	{
		get
		{
			if (!baseType_isInitialized)
			{
				InitializeBaseType();
			}
			return baseType;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				baseType = value;
				baseType_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public IList<FieldDef> Fields
	{
		get
		{
			if (fields == null)
			{
				InitializeFields();
			}
			return fields;
		}
	}

	public IList<MethodDef> Methods
	{
		get
		{
			if (methods == null)
			{
				InitializeMethods();
			}
			return methods;
		}
	}

	public IList<GenericParam> GenericParameters
	{
		get
		{
			if (genericParameters == null)
			{
				InitializeGenericParameters();
			}
			return genericParameters;
		}
	}

	public IList<InterfaceImpl> Interfaces
	{
		get
		{
			if (interfaces == null)
			{
				InitializeInterfaces();
			}
			return interfaces;
		}
	}

	public IList<DeclSecurity> DeclSecurities
	{
		get
		{
			if (declSecurities == null)
			{
				InitializeDeclSecurities();
			}
			return declSecurities;
		}
	}

	public ClassLayout ClassLayout
	{
		get
		{
			if (!classLayout_isInitialized)
			{
				InitializeClassLayout();
			}
			return classLayout;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				classLayout = value;
				classLayout_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public bool HasDeclSecurities => DeclSecurities.Count > 0;

	public TypeDef DeclaringType
	{
		get
		{
			if (!declaringType2_isInitialized)
			{
				InitializeDeclaringType2();
			}
			return declaringType2;
		}
		set
		{
			TypeDef typeDef = DeclaringType2;
			if (typeDef != value)
			{
				typeDef?.NestedTypes.Remove(this);
				value?.NestedTypes.Add(this);
				Module2 = null;
			}
		}
	}

	ITypeDefOrRef IMemberRef.DeclaringType => DeclaringType;

	public TypeDef DeclaringType2
	{
		get
		{
			if (!declaringType2_isInitialized)
			{
				InitializeDeclaringType2();
			}
			return declaringType2;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				declaringType2 = value;
				declaringType2_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public IList<TypeDef> NestedTypes
	{
		get
		{
			if (nestedTypes == null)
			{
				InitializeNestedTypes();
			}
			return nestedTypes;
		}
	}

	public IList<EventDef> Events
	{
		get
		{
			if (events == null)
			{
				InitializeEvents();
			}
			return events;
		}
	}

	public IList<PropertyDef> Properties
	{
		get
		{
			if (properties == null)
			{
				InitializeProperties();
			}
			return properties;
		}
	}

	public CustomAttributeCollection CustomAttributes
	{
		get
		{
			if (customAttributes == null)
			{
				InitializeCustomAttributes();
			}
			return customAttributes;
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public int HasCustomDebugInformationTag => 3;

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	public IList<PdbCustomDebugInfo> CustomDebugInfos
	{
		get
		{
			if (customDebugInfos == null)
			{
				InitializeCustomDebugInfos();
			}
			return customDebugInfos;
		}
	}

	public bool HasFields => Fields.Count > 0;

	public bool HasMethods => Methods.Count > 0;

	public bool HasGenericParameters => GenericParameters.Count > 0;

	public bool HasEvents => Events.Count > 0;

	public bool HasProperties => Properties.Count > 0;

	public bool HasNestedTypes => NestedTypes.Count > 0;

	public bool HasInterfaces => Interfaces.Count > 0;

	public bool HasClassLayout => ClassLayout != null;

	public ushort PackingSize
	{
		get
		{
			return ClassLayout?.PackingSize ?? ushort.MaxValue;
		}
		set
		{
			GetOrCreateClassLayout().PackingSize = value;
		}
	}

	public uint ClassSize
	{
		get
		{
			return (uint)(((int?)ClassLayout?.ClassSize) ?? (-1));
		}
		set
		{
			GetOrCreateClassLayout().ClassSize = value;
		}
	}

	public bool IsValueType
	{
		get
		{
			if ((Attributes & TypeAttributes.ClassSemanticsMask) != 0)
			{
				return false;
			}
			ITypeDefOrRef typeDefOrRef = BaseType;
			if (typeDefOrRef == null)
			{
				return false;
			}
			if (!typeDefOrRef.DefinitionAssembly.IsCorLib())
			{
				return false;
			}
			UTF8String uTF8String;
			UTF8String uTF8String2;
			if (typeDefOrRef is TypeRef typeRef)
			{
				uTF8String = typeRef.Name;
				uTF8String2 = typeRef.Namespace;
			}
			else
			{
				if (!(typeDefOrRef is TypeDef typeDef))
				{
					return false;
				}
				uTF8String = typeDef.Name;
				uTF8String2 = typeDef.Namespace;
			}
			if (uTF8String2 != systemString)
			{
				return false;
			}
			if (uTF8String != valueTypeString && uTF8String != enumString)
			{
				return false;
			}
			if (!DefinitionAssembly.IsCorLib())
			{
				return true;
			}
			if (Name == enumString)
			{
				return !(Namespace == systemString);
			}
			return true;
		}
	}

	public bool IsEnum
	{
		get
		{
			if ((Attributes & TypeAttributes.ClassSemanticsMask) != 0)
			{
				return false;
			}
			ITypeDefOrRef typeDefOrRef = BaseType;
			if (typeDefOrRef == null)
			{
				return false;
			}
			if (!typeDefOrRef.DefinitionAssembly.IsCorLib())
			{
				return false;
			}
			if (typeDefOrRef is TypeRef typeRef)
			{
				if (typeRef.Namespace == systemString)
				{
					return typeRef.Name == enumString;
				}
				return false;
			}
			if (typeDefOrRef is TypeDef typeDef)
			{
				if (typeDef.Namespace == systemString)
				{
					return typeDef.Name == enumString;
				}
				return false;
			}
			return false;
		}
	}

	public bool IsDelegate
	{
		get
		{
			if ((Attributes & (TypeAttributes.ClassSemanticsMask | TypeAttributes.Abstract)) != 0)
			{
				return false;
			}
			ITypeDefOrRef typeDefOrRef = BaseType;
			if (typeDefOrRef == null)
			{
				return false;
			}
			if (!typeDefOrRef.DefinitionAssembly.IsCorLib())
			{
				return false;
			}
			if (typeDefOrRef is TypeRef typeRef)
			{
				if (typeRef.Namespace == systemString)
				{
					return typeRef.Name == multicastDelegateString;
				}
				return false;
			}
			if (typeDefOrRef is TypeDef typeDef)
			{
				if (typeDef.Namespace == systemString)
				{
					return typeDef.Name == multicastDelegateString;
				}
				return false;
			}
			return false;
		}
	}

	public bool IsNested => DeclaringType != null;

	public bool IsPrimitive => this.IsPrimitive();

	public bool IsEquivalent => TIAHelper.IsTypeDefEquivalent(this);

	public TypeAttributes Visibility
	{
		get
		{
			return (TypeAttributes)((uint)attributes & 7u);
		}
		set
		{
			ModifyAttributes(~TypeAttributes.VisibilityMask, value & TypeAttributes.VisibilityMask);
		}
	}

	public bool IsNotPublic => (attributes & 7) == 0;

	public bool IsPublic => (attributes & 7) == 1;

	public bool IsNestedPublic => (attributes & 7) == 2;

	public bool IsNestedPrivate => (attributes & 7) == 3;

	public bool IsNestedFamily => (attributes & 7) == 4;

	public bool IsNestedAssembly => (attributes & 7) == 5;

	public bool IsNestedFamilyAndAssembly => (attributes & 7) == 6;

	public bool IsNestedFamilyOrAssembly => (attributes & 7) == 7;

	public TypeAttributes Layout
	{
		get
		{
			return (TypeAttributes)((uint)attributes & 0x18u);
		}
		set
		{
			ModifyAttributes(~TypeAttributes.LayoutMask, value & TypeAttributes.LayoutMask);
		}
	}

	public bool IsAutoLayout => (attributes & 0x18) == 0;

	public bool IsSequentialLayout => (attributes & 0x18) == 8;

	public bool IsExplicitLayout => (attributes & 0x18) == 16;

	public bool IsInterface
	{
		get
		{
			return (attributes & 0x20) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.ClassSemanticsMask);
		}
	}

	public bool IsClass
	{
		get
		{
			return (attributes & 0x20) == 0;
		}
		set
		{
			ModifyAttributes(!value, TypeAttributes.ClassSemanticsMask);
		}
	}

	public bool IsAbstract
	{
		get
		{
			return (attributes & 0x80) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Abstract);
		}
	}

	public bool IsSealed
	{
		get
		{
			return (attributes & 0x100) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Sealed);
		}
	}

	public bool IsSpecialName
	{
		get
		{
			return (attributes & 0x400) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.SpecialName);
		}
	}

	public bool IsImport
	{
		get
		{
			return (attributes & 0x1000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Import);
		}
	}

	public bool IsSerializable
	{
		get
		{
			return (attributes & 0x2000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Serializable);
		}
	}

	public bool IsWindowsRuntime
	{
		get
		{
			return (attributes & 0x4000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.WindowsRuntime);
		}
	}

	public TypeAttributes StringFormat
	{
		get
		{
			return (TypeAttributes)((uint)attributes & 0x30000u);
		}
		set
		{
			ModifyAttributes(~TypeAttributes.StringFormatMask, value & TypeAttributes.StringFormatMask);
		}
	}

	public bool IsAnsiClass => (attributes & 0x30000) == 0;

	public bool IsUnicodeClass => (attributes & 0x30000) == 65536;

	public bool IsAutoClass => (attributes & 0x30000) == 131072;

	public bool IsCustomFormatClass => (attributes & 0x30000) == 196608;

	public bool IsBeforeFieldInit
	{
		get
		{
			return (attributes & 0x100000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.BeforeFieldInit);
		}
	}

	public bool IsForwarder
	{
		get
		{
			return (attributes & 0x200000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.Forwarder);
		}
	}

	public bool IsRuntimeSpecialName
	{
		get
		{
			return (attributes & 0x800) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.RTSpecialName);
		}
	}

	public bool HasSecurity
	{
		get
		{
			return (attributes & 0x40000) != 0;
		}
		set
		{
			ModifyAttributes(value, TypeAttributes.HasSecurity);
		}
	}

	public bool IsGlobalModuleType
	{
		get
		{
			ModuleDef module = Module;
			if (module != null)
			{
				return module.GlobalType == this;
			}
			return false;
		}
	}

	private void InitializeModule2()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!module2_isInitialized)
			{
				module2 = GetModule2_NoLock();
				module2_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual ModuleDef GetModule2_NoLock()
	{
		return null;
	}

	private void InitializeBaseType()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!baseType_isInitialized)
			{
				baseType = GetBaseType_NoLock();
				baseType_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual ITypeDefOrRef GetBaseType_NoLock()
	{
		return null;
	}

	protected void ResetBaseType()
	{
		baseType_isInitialized = false;
	}

	protected virtual void InitializeFields()
	{
		Interlocked.CompareExchange(ref fields, new LazyList<FieldDef>(this), null);
	}

	protected virtual void InitializeMethods()
	{
		Interlocked.CompareExchange(ref methods, new LazyList<MethodDef>(this), null);
	}

	protected virtual void InitializeGenericParameters()
	{
		Interlocked.CompareExchange(ref genericParameters, new LazyList<GenericParam>(this), null);
	}

	protected virtual void InitializeInterfaces()
	{
		Interlocked.CompareExchange(ref interfaces, new List<InterfaceImpl>(), null);
	}

	protected virtual void InitializeDeclSecurities()
	{
		Interlocked.CompareExchange(ref declSecurities, new List<DeclSecurity>(), null);
	}

	private void InitializeClassLayout()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!classLayout_isInitialized)
			{
				classLayout = GetClassLayout_NoLock();
				classLayout_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	private ClassLayout GetOrCreateClassLayout()
	{
		ClassLayout classLayout = ClassLayout;
		if (classLayout != null)
		{
			return classLayout;
		}
		Interlocked.CompareExchange(ref this.classLayout, new ClassLayoutUser(0, 0u), null);
		return this.classLayout;
	}

	protected virtual ClassLayout GetClassLayout_NoLock()
	{
		return null;
	}

	private void InitializeDeclaringType2()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!declaringType2_isInitialized)
			{
				declaringType2 = GetDeclaringType2_NoLock();
				declaringType2_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual TypeDef GetDeclaringType2_NoLock()
	{
		return null;
	}

	protected virtual void InitializeNestedTypes()
	{
		Interlocked.CompareExchange(ref nestedTypes, new LazyList<TypeDef>(this), null);
	}

	protected virtual void InitializeEvents()
	{
		Interlocked.CompareExchange(ref events, new LazyList<EventDef>(this), null);
	}

	protected virtual void InitializeProperties()
	{
		Interlocked.CompareExchange(ref properties, new LazyList<PropertyDef>(this), null);
	}

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void ModifyAttributes(TypeAttributes andMask, TypeAttributes orMask)
	{
		attributes = (attributes & (int)andMask) | (int)orMask;
	}

	private void ModifyAttributes(bool set, TypeAttributes flags)
	{
		if (set)
		{
			attributes |= (int)flags;
		}
		else
		{
			attributes &= (int)(~flags);
		}
	}

	public IEnumerable<TypeDef> GetTypes()
	{
		return AllTypesHelper.Types(NestedTypes);
	}

	public TypeSig GetEnumUnderlyingType()
	{
		IList<FieldDef> list = Fields;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			FieldDef fieldDef = list[i];
			if (!fieldDef.IsLiteral && !fieldDef.IsStatic)
			{
				FieldSig fieldSig = fieldDef.FieldSig;
				if (fieldSig != null)
				{
					return fieldSig.Type;
				}
			}
		}
		return null;
	}

	public IMemberForwarded Resolve(MemberRef memberRef)
	{
		return Resolve(memberRef, (SigComparerOptions)0u);
	}

	public IMemberForwarded Resolve(MemberRef memberRef, SigComparerOptions options)
	{
		if (memberRef == null)
		{
			return null;
		}
		MethodSig methodSig = memberRef.MethodSig;
		if (methodSig != null)
		{
			return FindMethodCheckBaseType(memberRef.Name, methodSig, options, memberRef.Module);
		}
		FieldSig fieldSig = memberRef.FieldSig;
		if (fieldSig != null)
		{
			return FindFieldCheckBaseType(memberRef.Name, fieldSig, options, memberRef.Module);
		}
		return null;
	}

	public MethodDef FindMethod(UTF8String name, MethodSig sig)
	{
		return FindMethod(name, sig, (SigComparerOptions)0u, null);
	}

	public MethodDef FindMethod(UTF8String name, MethodSig sig, SigComparerOptions options)
	{
		return FindMethod(name, sig, options, null);
	}

	public MethodDef FindMethod(UTF8String name, MethodSig sig, SigComparerOptions options, ModuleDef sourceModule)
	{
		if (UTF8String.IsNull(name) || sig == null)
		{
			return null;
		}
		SigComparer sigComparer = new SigComparer(options, sourceModule);
		bool flag = (options & SigComparerOptions.PrivateScopeMethodIsComparable) != 0;
		IList<MethodDef> list = Methods;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			MethodDef methodDef = list[i];
			if ((flag || !methodDef.IsPrivateScope || sourceModule == Module) && UTF8String.Equals(methodDef.Name, name) && sigComparer.Equals(methodDef.MethodSig, sig))
			{
				return methodDef;
			}
		}
		return null;
	}

	public MethodDef FindMethod(UTF8String name)
	{
		IList<MethodDef> list = Methods;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			MethodDef methodDef = list[i];
			if (UTF8String.Equals(methodDef.Name, name))
			{
				return methodDef;
			}
		}
		return null;
	}

	public IEnumerable<MethodDef> FindMethods(UTF8String name)
	{
		IList<MethodDef> methods = Methods;
		int count = methods.Count;
		for (int i = 0; i < count; i++)
		{
			MethodDef methodDef = methods[i];
			if (UTF8String.Equals(methodDef.Name, name))
			{
				yield return methodDef;
			}
		}
	}

	public MethodDef FindStaticConstructor()
	{
		IList<MethodDef> list = Methods;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			MethodDef methodDef = list[i];
			if (methodDef.IsStaticConstructor)
			{
				return methodDef;
			}
		}
		return null;
	}

	public MethodDef FindOrCreateStaticConstructor()
	{
		MethodDef methodDef = FindStaticConstructor();
		if (methodDef != null)
		{
			return methodDef;
		}
		MethodImplAttributes implFlags = MethodImplAttributes.IL;
		MethodAttributes flags = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
		ModuleDef module = Module;
		methodDef = module.UpdateRowId(new MethodDefUser(MethodDef.StaticConstructorName, MethodSig.CreateStatic(module.CorLibTypes.Void), implFlags, flags));
		CilBody cilBody = new CilBody();
		cilBody.InitLocals = true;
		cilBody.MaxStack = 8;
		cilBody.Instructions.Add(OpCodes.Ret.ToInstruction());
		methodDef.Body = cilBody;
		Methods.Add(methodDef);
		return methodDef;
	}

	public IEnumerable<MethodDef> FindInstanceConstructors()
	{
		IList<MethodDef> methods = Methods;
		int count = methods.Count;
		for (int i = 0; i < count; i++)
		{
			MethodDef methodDef = methods[i];
			if (methodDef.IsInstanceConstructor)
			{
				yield return methodDef;
			}
		}
	}

	public IEnumerable<MethodDef> FindConstructors()
	{
		IList<MethodDef> methods = Methods;
		int count = methods.Count;
		for (int i = 0; i < count; i++)
		{
			MethodDef methodDef = methods[i];
			if (methodDef.IsConstructor)
			{
				yield return methodDef;
			}
		}
	}

	public MethodDef FindDefaultConstructor()
	{
		IList<MethodDef> list = Methods;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			MethodDef methodDef = list[i];
			if (methodDef.IsInstanceConstructor)
			{
				MethodSig methodSig = methodDef.MethodSig;
				if (methodSig != null && methodSig.Params.Count == 0)
				{
					return methodDef;
				}
			}
		}
		return null;
	}

	public FieldDef FindField(UTF8String name, FieldSig sig)
	{
		return FindField(name, sig, (SigComparerOptions)0u, null);
	}

	public FieldDef FindField(UTF8String name, FieldSig sig, SigComparerOptions options)
	{
		return FindField(name, sig, options, null);
	}

	public FieldDef FindField(UTF8String name, FieldSig sig, SigComparerOptions options, ModuleDef sourceModule)
	{
		if (UTF8String.IsNull(name) || sig == null)
		{
			return null;
		}
		SigComparer sigComparer = new SigComparer(options, sourceModule);
		bool flag = (options & SigComparerOptions.PrivateScopeFieldIsComparable) != 0;
		IList<FieldDef> list = Fields;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			FieldDef fieldDef = list[i];
			if ((flag || !fieldDef.IsPrivateScope || sourceModule == Module) && UTF8String.Equals(fieldDef.Name, name) && sigComparer.Equals(fieldDef.FieldSig, sig))
			{
				return fieldDef;
			}
		}
		return null;
	}

	public FieldDef FindField(UTF8String name)
	{
		IList<FieldDef> list = Fields;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			FieldDef fieldDef = list[i];
			if (UTF8String.Equals(fieldDef.Name, name))
			{
				return fieldDef;
			}
		}
		return null;
	}

	public IEnumerable<FieldDef> FindFields(UTF8String name)
	{
		IList<FieldDef> fields = Fields;
		int count = fields.Count;
		for (int i = 0; i < count; i++)
		{
			FieldDef fieldDef = fields[i];
			if (UTF8String.Equals(fieldDef.Name, name))
			{
				yield return fieldDef;
			}
		}
	}

	public EventDef FindEvent(UTF8String name, IType type)
	{
		return FindEvent(name, type, (SigComparerOptions)0u, null);
	}

	public EventDef FindEvent(UTF8String name, IType type, SigComparerOptions options)
	{
		return FindEvent(name, type, options, null);
	}

	public EventDef FindEvent(UTF8String name, IType type, SigComparerOptions options, ModuleDef sourceModule)
	{
		if (UTF8String.IsNull(name) || type == null)
		{
			return null;
		}
		SigComparer sigComparer = new SigComparer(options, sourceModule);
		IList<EventDef> list = Events;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			EventDef eventDef = list[i];
			if (UTF8String.Equals(eventDef.Name, name) && sigComparer.Equals(eventDef.EventType, type))
			{
				return eventDef;
			}
		}
		return null;
	}

	public EventDef FindEvent(UTF8String name)
	{
		IList<EventDef> list = Events;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			EventDef eventDef = list[i];
			if (UTF8String.Equals(eventDef.Name, name))
			{
				return eventDef;
			}
		}
		return null;
	}

	public IEnumerable<EventDef> FindEvents(UTF8String name)
	{
		IList<EventDef> events = Events;
		int count = events.Count;
		for (int i = 0; i < count; i++)
		{
			EventDef eventDef = events[i];
			if (UTF8String.Equals(eventDef.Name, name))
			{
				yield return eventDef;
			}
		}
	}

	public PropertyDef FindProperty(UTF8String name, CallingConventionSig propSig)
	{
		return FindProperty(name, propSig, (SigComparerOptions)0u, null);
	}

	public PropertyDef FindProperty(UTF8String name, CallingConventionSig propSig, SigComparerOptions options)
	{
		return FindProperty(name, propSig, options, null);
	}

	public PropertyDef FindProperty(UTF8String name, CallingConventionSig propSig, SigComparerOptions options, ModuleDef sourceModule)
	{
		if (UTF8String.IsNull(name) || propSig == null)
		{
			return null;
		}
		SigComparer sigComparer = new SigComparer(options, sourceModule);
		IList<PropertyDef> list = Properties;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			PropertyDef propertyDef = list[i];
			if (UTF8String.Equals(propertyDef.Name, name) && sigComparer.Equals(propertyDef.Type, propSig))
			{
				return propertyDef;
			}
		}
		return null;
	}

	public PropertyDef FindProperty(UTF8String name)
	{
		IList<PropertyDef> list = Properties;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			PropertyDef propertyDef = list[i];
			if (UTF8String.Equals(propertyDef.Name, name))
			{
				return propertyDef;
			}
		}
		return null;
	}

	public IEnumerable<PropertyDef> FindProperties(UTF8String name)
	{
		IList<PropertyDef> properties = Properties;
		int count = properties.Count;
		for (int i = 0; i < count; i++)
		{
			PropertyDef propertyDef = properties[i];
			if (UTF8String.Equals(propertyDef.Name, name))
			{
				yield return propertyDef;
			}
		}
	}

	public MethodDef FindMethodCheckBaseType(UTF8String name, MethodSig sig)
	{
		return FindMethodCheckBaseType(name, sig, (SigComparerOptions)0u, null);
	}

	public MethodDef FindMethodCheckBaseType(UTF8String name, MethodSig sig, SigComparerOptions options)
	{
		return FindMethodCheckBaseType(name, sig, options, null);
	}

	public MethodDef FindMethodCheckBaseType(UTF8String name, MethodSig sig, SigComparerOptions options, ModuleDef sourceModule)
	{
		for (TypeDef typeDef = this; typeDef != null; typeDef = typeDef.BaseType.ResolveTypeDef())
		{
			MethodDef methodDef = typeDef.FindMethod(name, sig, options, sourceModule);
			if (methodDef != null)
			{
				return methodDef;
			}
		}
		return null;
	}

	public MethodDef FindMethodCheckBaseType(UTF8String name)
	{
		for (TypeDef typeDef = this; typeDef != null; typeDef = typeDef.BaseType.ResolveTypeDef())
		{
			MethodDef methodDef = typeDef.FindMethod(name);
			if (methodDef != null)
			{
				return methodDef;
			}
		}
		return null;
	}

	public FieldDef FindFieldCheckBaseType(UTF8String name, FieldSig sig)
	{
		return FindFieldCheckBaseType(name, sig, (SigComparerOptions)0u, null);
	}

	public FieldDef FindFieldCheckBaseType(UTF8String name, FieldSig sig, SigComparerOptions options)
	{
		return FindFieldCheckBaseType(name, sig, options, null);
	}

	public FieldDef FindFieldCheckBaseType(UTF8String name, FieldSig sig, SigComparerOptions options, ModuleDef sourceModule)
	{
		for (TypeDef typeDef = this; typeDef != null; typeDef = typeDef.BaseType.ResolveTypeDef())
		{
			FieldDef fieldDef = typeDef.FindField(name, sig, options, sourceModule);
			if (fieldDef != null)
			{
				return fieldDef;
			}
		}
		return null;
	}

	public FieldDef FindFieldCheckBaseType(UTF8String name)
	{
		for (TypeDef typeDef = this; typeDef != null; typeDef = typeDef.BaseType.ResolveTypeDef())
		{
			FieldDef fieldDef = typeDef.FindField(name);
			if (fieldDef != null)
			{
				return fieldDef;
			}
		}
		return null;
	}

	public EventDef FindEventCheckBaseType(UTF8String name, ITypeDefOrRef eventType)
	{
		for (TypeDef typeDef = this; typeDef != null; typeDef = typeDef.BaseType.ResolveTypeDef())
		{
			EventDef eventDef = typeDef.FindEvent(name, eventType);
			if (eventDef != null)
			{
				return eventDef;
			}
		}
		return null;
	}

	public EventDef FindEventCheckBaseType(UTF8String name)
	{
		for (TypeDef typeDef = this; typeDef != null; typeDef = typeDef.BaseType.ResolveTypeDef())
		{
			EventDef eventDef = typeDef.FindEvent(name);
			if (eventDef != null)
			{
				return eventDef;
			}
		}
		return null;
	}

	public PropertyDef FindPropertyCheckBaseType(UTF8String name, PropertySig sig)
	{
		return FindPropertyCheckBaseType(name, sig, (SigComparerOptions)0u, null);
	}

	public PropertyDef FindPropertyCheckBaseType(UTF8String name, PropertySig sig, SigComparerOptions options)
	{
		return FindPropertyCheckBaseType(name, sig, options, null);
	}

	public PropertyDef FindPropertyCheckBaseType(UTF8String name, PropertySig sig, SigComparerOptions options, ModuleDef sourceModule)
	{
		for (TypeDef typeDef = this; typeDef != null; typeDef = typeDef.BaseType.ResolveTypeDef())
		{
			PropertyDef propertyDef = typeDef.FindProperty(name, sig, options, sourceModule);
			if (propertyDef != null)
			{
				return propertyDef;
			}
		}
		return null;
	}

	public PropertyDef FindPropertyCheckBaseType(UTF8String name)
	{
		for (TypeDef typeDef = this; typeDef != null; typeDef = typeDef.BaseType.ResolveTypeDef())
		{
			PropertyDef propertyDef = typeDef.FindProperty(name);
			if (propertyDef != null)
			{
				return propertyDef;
			}
		}
		return null;
	}

	public void Remove(MethodDef method)
	{
		Remove(method, removeEmptyPropertiesEvents: false);
	}

	public void Remove(MethodDef method, bool removeEmptyPropertiesEvents)
	{
		if (method == null)
		{
			return;
		}
		IList<PropertyDef> list = Properties;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			PropertyDef propertyDef = list[i];
			propertyDef.GetMethods.Remove(method);
			propertyDef.SetMethods.Remove(method);
			propertyDef.OtherMethods.Remove(method);
		}
		IList<EventDef> list2 = Events;
		count = list2.Count;
		for (int j = 0; j < count; j++)
		{
			EventDef eventDef = list2[j];
			if (eventDef.AddMethod == method)
			{
				eventDef.AddMethod = null;
			}
			if (eventDef.RemoveMethod == method)
			{
				eventDef.RemoveMethod = null;
			}
			if (eventDef.InvokeMethod == method)
			{
				eventDef.InvokeMethod = null;
			}
			eventDef.OtherMethods.Remove(method);
		}
		if (removeEmptyPropertiesEvents)
		{
			RemoveEmptyProperties();
			RemoveEmptyEvents();
		}
		Methods.Remove(method);
	}

	private void RemoveEmptyProperties()
	{
		IList<PropertyDef> list = Properties;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].IsEmpty)
			{
				list.RemoveAt(num);
			}
		}
	}

	private void RemoveEmptyEvents()
	{
		IList<EventDef> list = Events;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].IsEmpty)
			{
				list.RemoveAt(num);
			}
		}
	}

	void IListListener<FieldDef>.OnLazyAdd(int index, ref FieldDef value)
	{
		OnLazyAdd2(index, ref value);
	}

	internal virtual void OnLazyAdd2(int index, ref FieldDef value)
	{
	}

	void IListListener<FieldDef>.OnAdd(int index, FieldDef value)
	{
		if (value.DeclaringType != null)
		{
			throw new InvalidOperationException("Field is already owned by another type. Set DeclaringType to null first.");
		}
		value.DeclaringType2 = this;
	}

	void IListListener<FieldDef>.OnRemove(int index, FieldDef value)
	{
		value.DeclaringType2 = null;
	}

	void IListListener<FieldDef>.OnResize(int index)
	{
	}

	void IListListener<FieldDef>.OnClear()
	{
		foreach (FieldDef item in fields.GetEnumerable_NoLock())
		{
			item.DeclaringType2 = null;
		}
	}

	void IListListener<MethodDef>.OnLazyAdd(int index, ref MethodDef value)
	{
		OnLazyAdd2(index, ref value);
	}

	internal virtual void OnLazyAdd2(int index, ref MethodDef value)
	{
	}

	void IListListener<MethodDef>.OnAdd(int index, MethodDef value)
	{
		if (value.DeclaringType != null)
		{
			throw new InvalidOperationException("Method is already owned by another type. Set DeclaringType to null first.");
		}
		value.DeclaringType2 = this;
		value.Parameters.UpdateThisParameterType(this);
	}

	void IListListener<MethodDef>.OnRemove(int index, MethodDef value)
	{
		value.DeclaringType2 = null;
		value.Parameters.UpdateThisParameterType(null);
	}

	void IListListener<MethodDef>.OnResize(int index)
	{
	}

	void IListListener<MethodDef>.OnClear()
	{
		foreach (MethodDef item in methods.GetEnumerable_NoLock())
		{
			item.DeclaringType2 = null;
			item.Parameters.UpdateThisParameterType(null);
		}
	}

	void IListListener<TypeDef>.OnLazyAdd(int index, ref TypeDef value)
	{
	}

	void IListListener<TypeDef>.OnAdd(int index, TypeDef value)
	{
		if (value.DeclaringType != null)
		{
			throw new InvalidOperationException("Nested type is already owned by another type. Set DeclaringType to null first.");
		}
		if (value.Module != null)
		{
			throw new InvalidOperationException("Type is already owned by another module. Remove it from that module's type list.");
		}
		value.DeclaringType2 = this;
	}

	void IListListener<TypeDef>.OnRemove(int index, TypeDef value)
	{
		value.DeclaringType2 = null;
		value.Module2 = null;
	}

	void IListListener<TypeDef>.OnResize(int index)
	{
	}

	void IListListener<TypeDef>.OnClear()
	{
		foreach (TypeDef item in nestedTypes.GetEnumerable_NoLock())
		{
			item.DeclaringType2 = null;
		}
	}

	void IListListener<EventDef>.OnLazyAdd(int index, ref EventDef value)
	{
		OnLazyAdd2(index, ref value);
	}

	internal virtual void OnLazyAdd2(int index, ref EventDef value)
	{
	}

	void IListListener<EventDef>.OnAdd(int index, EventDef value)
	{
		if (value.DeclaringType != null)
		{
			throw new InvalidOperationException("Event is already owned by another type. Set DeclaringType to null first.");
		}
		value.DeclaringType2 = this;
	}

	void IListListener<EventDef>.OnRemove(int index, EventDef value)
	{
		value.DeclaringType2 = null;
	}

	void IListListener<EventDef>.OnResize(int index)
	{
	}

	void IListListener<EventDef>.OnClear()
	{
		foreach (EventDef item in events.GetEnumerable_NoLock())
		{
			item.DeclaringType2 = null;
		}
	}

	void IListListener<PropertyDef>.OnLazyAdd(int index, ref PropertyDef value)
	{
		OnLazyAdd2(index, ref value);
	}

	internal virtual void OnLazyAdd2(int index, ref PropertyDef value)
	{
	}

	void IListListener<PropertyDef>.OnAdd(int index, PropertyDef value)
	{
		if (value.DeclaringType != null)
		{
			throw new InvalidOperationException("Property is already owned by another type. Set DeclaringType to null first.");
		}
		value.DeclaringType2 = this;
	}

	void IListListener<PropertyDef>.OnRemove(int index, PropertyDef value)
	{
		value.DeclaringType2 = null;
	}

	void IListListener<PropertyDef>.OnResize(int index)
	{
	}

	void IListListener<PropertyDef>.OnClear()
	{
		foreach (PropertyDef item in properties.GetEnumerable_NoLock())
		{
			item.DeclaringType2 = null;
		}
	}

	void IListListener<GenericParam>.OnLazyAdd(int index, ref GenericParam value)
	{
		OnLazyAdd2(index, ref value);
	}

	internal virtual void OnLazyAdd2(int index, ref GenericParam value)
	{
	}

	void IListListener<GenericParam>.OnAdd(int index, GenericParam value)
	{
		if (value.Owner != null)
		{
			throw new InvalidOperationException("Generic param is already owned by another type/method. Set Owner to null first.");
		}
		value.Owner = this;
	}

	void IListListener<GenericParam>.OnRemove(int index, GenericParam value)
	{
		value.Owner = null;
	}

	void IListListener<GenericParam>.OnResize(int index)
	{
	}

	void IListListener<GenericParam>.OnClear()
	{
		foreach (GenericParam item in genericParameters.GetEnumerable_NoLock())
		{
			item.Owner = null;
		}
	}

	public IList<FieldDef> GetFields(UTF8String name)
	{
		List<FieldDef> list = new List<FieldDef>();
		IList<FieldDef> list2 = Fields;
		int count = list2.Count;
		for (int i = 0; i < count; i++)
		{
			FieldDef fieldDef = list2[i];
			if (fieldDef.Name == name)
			{
				list.Add(fieldDef);
			}
		}
		return list;
	}

	public FieldDef GetField(UTF8String name)
	{
		IList<FieldDef> list = Fields;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			FieldDef fieldDef = list[i];
			if (fieldDef.Name == name)
			{
				return fieldDef;
			}
		}
		return null;
	}

	internal static bool GetClassSize(TypeDef td, out uint size)
	{
		size = 0u;
		if (td == null)
		{
			return false;
		}
		if (!td.IsValueType)
		{
			return false;
		}
		if (!td.IsSequentialLayout && !td.IsExplicitLayout)
		{
			if (td.Fields.Count != 1)
			{
				return false;
			}
			return td.Fields[0]?.GetFieldSize(out size) ?? false;
		}
		ClassLayout classLayout = td.ClassLayout;
		if (classLayout == null)
		{
			return false;
		}
		uint classSize = classLayout.ClassSize;
		if (classSize != 0)
		{
			size = classSize;
			return true;
		}
		return false;
	}

	protected MethodDef FindMethodImplMethod(IMethodDefOrRef mdr)
	{
		if (mdr is MethodDef result)
		{
			return result;
		}
		if (!(mdr is MemberRef { Class: var memberRefParent } memberRef))
		{
			return null;
		}
		if (memberRefParent is MethodDef result2)
		{
			return result2;
		}
		for (int i = 0; i < 10; i++)
		{
			if (!(memberRefParent is TypeSpec typeSpec))
			{
				break;
			}
			if (!(typeSpec.TypeSig is GenericInstSig { GenericType: not null } genericInstSig))
			{
				return null;
			}
			memberRefParent = genericInstSig.GenericType.TypeDefOrRef;
		}
		TypeDef typeDef = memberRefParent as TypeDef;
		if (typeDef == null && memberRefParent is TypeRef typeRef && Module != null)
		{
			typeDef = Module.Find(typeRef);
		}
		return typeDef?.FindMethod(memberRef.Name, memberRef.MethodSig);
	}

	public override string ToString()
	{
		return FullName;
	}
}
