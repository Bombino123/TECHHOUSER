using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.PE;
using dnlib.Threading;
using dnlib.Utils;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class MethodDef : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasDeclSecurity, IFullName, IMemberRefParent, IMethodDefOrRef, ICustomAttributeType, IMethod, ITokenOperand, IGenericParameterProvider, IIsTypeOrMethod, IMemberRef, IOwnerModule, IMemberForwarded, ITypeOrMethodDef, IManagedEntryPoint, IHasCustomDebugInformation, IListListener<GenericParam>, IListListener<ParamDef>, IMemberDef, IDnlibDef
{
	internal static readonly UTF8String StaticConstructorName = ".cctor";

	internal static readonly UTF8String InstanceConstructorName = ".ctor";

	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected ParameterList parameterList;

	protected RVA rva;

	protected int implAttributes;

	protected int attributes;

	protected UTF8String name;

	protected CallingConventionSig signature;

	protected LazyList<ParamDef> paramDefs;

	protected LazyList<GenericParam> genericParameters;

	protected IList<DeclSecurity> declSecurities;

	protected ImplMap implMap;

	protected bool implMap_isInitialized;

	protected MethodBody methodBody;

	protected bool methodBody_isInitialized;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	protected IList<MethodOverride> overrides;

	protected MethodExportInfo exportInfo;

	protected TypeDef declaringType2;

	protected internal static int SEMATTRS_INITD = int.MinValue;

	protected internal int semAttrs;

	public MDToken MDToken => new MDToken(Table.Method, rid);

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

	public int HasCustomAttributeTag => 0;

	public int HasDeclSecurityTag => 1;

	public int MemberRefParentTag => 3;

	public int MethodDefOrRefTag => 0;

	public int MemberForwardedTag => 1;

	public int CustomAttributeTypeTag => 2;

	public int TypeOrMethodDefTag => 1;

	public RVA RVA
	{
		get
		{
			return rva;
		}
		set
		{
			rva = value;
		}
	}

	public MethodImplAttributes ImplAttributes
	{
		get
		{
			return (MethodImplAttributes)implAttributes;
		}
		set
		{
			implAttributes = (int)value;
		}
	}

	public MethodAttributes Attributes
	{
		get
		{
			return (MethodAttributes)attributes;
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

	public CallingConventionSig Signature
	{
		get
		{
			return signature;
		}
		set
		{
			signature = value;
		}
	}

	public IList<ParamDef> ParamDefs
	{
		get
		{
			if (paramDefs == null)
			{
				InitializeParamDefs();
			}
			return paramDefs;
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

	public ImplMap ImplMap
	{
		get
		{
			if (!implMap_isInitialized)
			{
				InitializeImplMap();
			}
			return implMap;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				implMap = value;
				implMap_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public MethodBody MethodBody
	{
		get
		{
			if (!methodBody_isInitialized)
			{
				InitializeMethodBody();
			}
			return methodBody;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				methodBody = value;
				methodBody_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	protected virtual bool CanFreeMethodBody => true;

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

	public int HasCustomDebugInformationTag => 0;

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

	public IList<MethodOverride> Overrides
	{
		get
		{
			if (overrides == null)
			{
				InitializeOverrides();
			}
			return overrides;
		}
	}

	public MethodExportInfo ExportInfo
	{
		get
		{
			return exportInfo;
		}
		set
		{
			exportInfo = value;
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public bool HasDeclSecurities => DeclSecurities.Count > 0;

	public bool HasParamDefs => ParamDefs.Count > 0;

	public TypeDef DeclaringType
	{
		get
		{
			return declaringType2;
		}
		set
		{
			TypeDef typeDef = DeclaringType2;
			if (typeDef != value)
			{
				typeDef?.Methods.Remove(this);
				value?.Methods.Add(this);
			}
		}
	}

	ITypeDefOrRef IMemberRef.DeclaringType => declaringType2;

	public TypeDef DeclaringType2
	{
		get
		{
			return declaringType2;
		}
		set
		{
			declaringType2 = value;
		}
	}

	public ModuleDef Module => declaringType2?.Module;

	bool IIsTypeOrMethod.IsType => false;

	bool IIsTypeOrMethod.IsMethod => true;

	bool IMemberRef.IsField => false;

	bool IMemberRef.IsTypeSpec => false;

	bool IMemberRef.IsTypeRef => false;

	bool IMemberRef.IsTypeDef => false;

	bool IMemberRef.IsMethodSpec => false;

	bool IMemberRef.IsMethodDef => true;

	bool IMemberRef.IsMemberRef => false;

	bool IMemberRef.IsFieldDef => false;

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => false;

	public CilBody Body
	{
		get
		{
			if (!methodBody_isInitialized)
			{
				InitializeMethodBody();
			}
			return methodBody as CilBody;
		}
		set
		{
			MethodBody = value;
		}
	}

	public NativeMethodBody NativeBody
	{
		get
		{
			if (!methodBody_isInitialized)
			{
				InitializeMethodBody();
			}
			return methodBody as NativeMethodBody;
		}
		set
		{
			MethodBody = value;
		}
	}

	public bool HasGenericParameters => GenericParameters.Count > 0;

	public bool HasBody => Body != null;

	public bool HasOverrides => Overrides.Count > 0;

	public bool HasImplMap => ImplMap != null;

	public string FullName => FullNameFactory.MethodFullName(declaringType2?.FullName, name, MethodSig, null, null, this);

	public MethodSig MethodSig
	{
		get
		{
			return signature as MethodSig;
		}
		set
		{
			signature = value;
		}
	}

	public ParameterList Parameters => parameterList;

	int IGenericParameterProvider.NumberOfGenericParameters => (int)(MethodSig?.GenParamCount ?? 0);

	public bool HasThis => MethodSig?.HasThis ?? false;

	public bool ExplicitThis => MethodSig?.ExplicitThis ?? false;

	public CallingConvention CallingConvention
	{
		get
		{
			MethodSig methodSig = MethodSig;
			if (methodSig != null)
			{
				return methodSig.CallingConvention & CallingConvention.Mask;
			}
			return CallingConvention.Default;
		}
	}

	public TypeSig ReturnType
	{
		get
		{
			return MethodSig?.RetType;
		}
		set
		{
			MethodSig methodSig = MethodSig;
			if (methodSig != null)
			{
				methodSig.RetType = value;
			}
		}
	}

	public bool HasReturnType => ReturnType.RemovePinnedAndModifiers().GetElementType() != ElementType.Void;

	public MethodSemanticsAttributes SemanticsAttributes
	{
		get
		{
			if ((semAttrs & SEMATTRS_INITD) == 0)
			{
				InitializeSemanticsAttributes();
			}
			return (MethodSemanticsAttributes)semAttrs;
		}
		set
		{
			semAttrs = (int)value | SEMATTRS_INITD;
		}
	}

	public MethodAttributes Access
	{
		get
		{
			return (MethodAttributes)attributes & MethodAttributes.MemberAccessMask;
		}
		set
		{
			ModifyAttributes(~MethodAttributes.MemberAccessMask, value & MethodAttributes.MemberAccessMask);
		}
	}

	public bool IsCompilerControlled => IsPrivateScope;

	public bool IsPrivateScope => ((ushort)attributes & 7) == 0;

	public bool IsPrivate => ((ushort)attributes & 7) == 1;

	public bool IsFamilyAndAssembly => ((ushort)attributes & 7) == 2;

	public bool IsAssembly => ((ushort)attributes & 7) == 3;

	public bool IsFamily => ((ushort)attributes & 7) == 4;

	public bool IsFamilyOrAssembly => ((ushort)attributes & 7) == 5;

	public bool IsPublic => ((ushort)attributes & 7) == 6;

	public bool IsStatic
	{
		get
		{
			return ((ushort)attributes & 0x10) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.Static);
		}
	}

	public bool IsFinal
	{
		get
		{
			return ((ushort)attributes & 0x20) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.Final);
		}
	}

	public bool IsVirtual
	{
		get
		{
			return ((ushort)attributes & 0x40) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.Virtual);
		}
	}

	public bool IsHideBySig
	{
		get
		{
			return ((ushort)attributes & 0x80) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.HideBySig);
		}
	}

	public bool IsNewSlot
	{
		get
		{
			return ((ushort)attributes & 0x100) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.VtableLayoutMask);
		}
	}

	public bool IsReuseSlot
	{
		get
		{
			return ((ushort)attributes & 0x100) == 0;
		}
		set
		{
			ModifyAttributes(!value, MethodAttributes.VtableLayoutMask);
		}
	}

	public bool IsCheckAccessOnOverride
	{
		get
		{
			return ((ushort)attributes & 0x200) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.CheckAccessOnOverride);
		}
	}

	public bool IsAbstract
	{
		get
		{
			return ((ushort)attributes & 0x400) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.Abstract);
		}
	}

	public bool IsSpecialName
	{
		get
		{
			return ((ushort)attributes & 0x800) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.SpecialName);
		}
	}

	public bool IsPinvokeImpl
	{
		get
		{
			return ((ushort)attributes & 0x2000) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.PinvokeImpl);
		}
	}

	public bool IsUnmanagedExport
	{
		get
		{
			return ((ushort)attributes & 8) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.UnmanagedExport);
		}
	}

	public bool IsRuntimeSpecialName
	{
		get
		{
			return ((ushort)attributes & 0x1000) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.RTSpecialName);
		}
	}

	public bool HasSecurity
	{
		get
		{
			return ((ushort)attributes & 0x4000) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.HasSecurity);
		}
	}

	public bool IsRequireSecObject
	{
		get
		{
			return ((ushort)attributes & 0x8000) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodAttributes.RequireSecObject);
		}
	}

	public MethodImplAttributes CodeType
	{
		get
		{
			return (MethodImplAttributes)implAttributes & MethodImplAttributes.CodeTypeMask;
		}
		set
		{
			ModifyImplAttributes(~MethodImplAttributes.CodeTypeMask, value & MethodImplAttributes.CodeTypeMask);
		}
	}

	public bool IsIL => ((ushort)implAttributes & 3) == 0;

	public bool IsNative => ((ushort)implAttributes & 3) == 1;

	public bool IsOPTIL => ((ushort)implAttributes & 3) == 2;

	public bool IsRuntime => ((ushort)implAttributes & 3) == 3;

	public bool IsUnmanaged
	{
		get
		{
			return ((ushort)implAttributes & 4) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.ManagedMask);
		}
	}

	public bool IsManaged
	{
		get
		{
			return ((ushort)implAttributes & 4) == 0;
		}
		set
		{
			ModifyImplAttributes(!value, MethodImplAttributes.ManagedMask);
		}
	}

	public bool IsForwardRef
	{
		get
		{
			return ((ushort)implAttributes & 0x10) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.ForwardRef);
		}
	}

	public bool IsPreserveSig
	{
		get
		{
			return ((ushort)implAttributes & 0x80) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.PreserveSig);
		}
	}

	public bool IsInternalCall
	{
		get
		{
			return ((ushort)implAttributes & 0x1000) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.InternalCall);
		}
	}

	public bool IsSynchronized
	{
		get
		{
			return ((ushort)implAttributes & 0x20) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.Synchronized);
		}
	}

	public bool IsNoInlining
	{
		get
		{
			return ((ushort)implAttributes & 8) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.NoInlining);
		}
	}

	public bool IsAggressiveInlining
	{
		get
		{
			return ((ushort)implAttributes & 0x100) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.AggressiveInlining);
		}
	}

	public bool IsNoOptimization
	{
		get
		{
			return ((ushort)implAttributes & 0x40) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.NoOptimization);
		}
	}

	public bool IsAggressiveOptimization
	{
		get
		{
			return ((ushort)implAttributes & 0x200) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.AggressiveOptimization);
		}
	}

	public bool HasSecurityMitigations
	{
		get
		{
			return ((ushort)implAttributes & 0x400) != 0;
		}
		set
		{
			ModifyImplAttributes(value, MethodImplAttributes.SecurityMitigations);
		}
	}

	public bool IsSetter
	{
		get
		{
			return (SemanticsAttributes & MethodSemanticsAttributes.Setter) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodSemanticsAttributes.Setter);
		}
	}

	public bool IsGetter
	{
		get
		{
			return (SemanticsAttributes & MethodSemanticsAttributes.Getter) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodSemanticsAttributes.Getter);
		}
	}

	public bool IsOther
	{
		get
		{
			return (SemanticsAttributes & MethodSemanticsAttributes.Other) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodSemanticsAttributes.Other);
		}
	}

	public bool IsAddOn
	{
		get
		{
			return (SemanticsAttributes & MethodSemanticsAttributes.AddOn) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodSemanticsAttributes.AddOn);
		}
	}

	public bool IsRemoveOn
	{
		get
		{
			return (SemanticsAttributes & MethodSemanticsAttributes.RemoveOn) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodSemanticsAttributes.RemoveOn);
		}
	}

	public bool IsFire
	{
		get
		{
			return (SemanticsAttributes & MethodSemanticsAttributes.Fire) != 0;
		}
		set
		{
			ModifyAttributes(value, MethodSemanticsAttributes.Fire);
		}
	}

	public bool IsStaticConstructor
	{
		get
		{
			if (IsRuntimeSpecialName)
			{
				return UTF8String.Equals(name, StaticConstructorName);
			}
			return false;
		}
	}

	public bool IsInstanceConstructor
	{
		get
		{
			if (IsRuntimeSpecialName)
			{
				return UTF8String.Equals(name, InstanceConstructorName);
			}
			return false;
		}
	}

	public bool IsConstructor
	{
		get
		{
			if (!IsStaticConstructor)
			{
				return IsInstanceConstructor;
			}
			return true;
		}
	}

	protected virtual void InitializeParamDefs()
	{
		Interlocked.CompareExchange(ref paramDefs, new LazyList<ParamDef>(this), null);
	}

	protected virtual void InitializeGenericParameters()
	{
		Interlocked.CompareExchange(ref genericParameters, new LazyList<GenericParam>(this), null);
	}

	protected virtual void InitializeDeclSecurities()
	{
		Interlocked.CompareExchange(ref declSecurities, new List<DeclSecurity>(), null);
	}

	private void InitializeImplMap()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!implMap_isInitialized)
			{
				implMap = GetImplMap_NoLock();
				implMap_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual ImplMap GetImplMap_NoLock()
	{
		return null;
	}

	protected void ResetImplMap()
	{
		implMap_isInitialized = false;
	}

	private void InitializeMethodBody()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!methodBody_isInitialized)
			{
				methodBody = GetMethodBody_NoLock();
				methodBody_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	public void FreeMethodBody()
	{
		if (!CanFreeMethodBody || !methodBody_isInitialized)
		{
			return;
		}
		theLock.EnterWriteLock();
		try
		{
			methodBody = null;
			methodBody_isInitialized = false;
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual MethodBody GetMethodBody_NoLock()
	{
		return null;
	}

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	protected virtual void InitializeOverrides()
	{
		Interlocked.CompareExchange(ref overrides, new List<MethodOverride>(), null);
	}

	protected virtual void InitializeSemanticsAttributes()
	{
		semAttrs = 0 | SEMATTRS_INITD;
	}

	private void ModifyAttributes(bool set, MethodSemanticsAttributes flags)
	{
		if ((semAttrs & SEMATTRS_INITD) == 0)
		{
			InitializeSemanticsAttributes();
		}
		if (set)
		{
			semAttrs |= (int)flags;
		}
		else
		{
			semAttrs &= (int)(~(uint)flags);
		}
	}

	private void ModifyAttributes(MethodAttributes andMask, MethodAttributes orMask)
	{
		attributes = (attributes & (int)andMask) | (int)orMask;
	}

	private void ModifyAttributes(bool set, MethodAttributes flags)
	{
		if (set)
		{
			attributes |= (int)flags;
		}
		else
		{
			attributes &= (int)(~(uint)flags);
		}
	}

	private void ModifyImplAttributes(MethodImplAttributes andMask, MethodImplAttributes orMask)
	{
		implAttributes = (implAttributes & (int)andMask) | (int)orMask;
	}

	private void ModifyImplAttributes(bool set, MethodImplAttributes flags)
	{
		if (set)
		{
			implAttributes |= (int)flags;
		}
		else
		{
			implAttributes &= (int)(~(uint)flags);
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

	void IListListener<ParamDef>.OnLazyAdd(int index, ref ParamDef value)
	{
		OnLazyAdd2(index, ref value);
	}

	internal virtual void OnLazyAdd2(int index, ref ParamDef value)
	{
	}

	void IListListener<ParamDef>.OnAdd(int index, ParamDef value)
	{
		if (value.DeclaringMethod != null)
		{
			throw new InvalidOperationException("Param is already owned by another method. Set DeclaringMethod to null first.");
		}
		value.DeclaringMethod = this;
	}

	void IListListener<ParamDef>.OnRemove(int index, ParamDef value)
	{
		value.DeclaringMethod = null;
	}

	void IListListener<ParamDef>.OnResize(int index)
	{
	}

	void IListListener<ParamDef>.OnClear()
	{
		foreach (ParamDef item in paramDefs.GetEnumerable_NoLock())
		{
			item.DeclaringMethod = null;
		}
	}

	public override string ToString()
	{
		return FullName;
	}
}
