using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.PE;
using dnlib.Threading;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class FieldDef : IHasConstant, ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName, IHasFieldMarshal, IMemberForwarded, IMemberRef, IOwnerModule, IIsTypeOrMethod, IHasCustomDebugInformation, IField, ITokenOperand, IMemberDef, IDnlibDef
{
	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	protected int attributes;

	protected UTF8String name;

	protected CallingConventionSig signature;

	protected uint? fieldOffset;

	protected bool fieldOffset_isInitialized;

	protected MarshalType marshalType;

	protected bool marshalType_isInitialized;

	protected RVA rva;

	protected bool rva_isInitialized;

	protected byte[] initialValue;

	protected bool initialValue_isInitialized;

	protected ImplMap implMap;

	protected bool implMap_isInitialized;

	protected Constant constant;

	protected bool constant_isInitialized;

	protected TypeDef declaringType2;

	public MDToken MDToken => new MDToken(Table.Field, rid);

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

	public int HasConstantTag => 0;

	public int HasCustomAttributeTag => 1;

	public int HasFieldMarshalTag => 0;

	public int MemberForwardedTag => 0;

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

	public int HasCustomDebugInformationTag => 1;

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

	public FieldAttributes Attributes
	{
		get
		{
			return (FieldAttributes)attributes;
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

	public uint? FieldOffset
	{
		get
		{
			if (!fieldOffset_isInitialized)
			{
				InitializeFieldOffset();
			}
			return fieldOffset;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				fieldOffset = value;
				fieldOffset_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public MarshalType MarshalType
	{
		get
		{
			if (!marshalType_isInitialized)
			{
				InitializeMarshalType();
			}
			return marshalType;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				marshalType = value;
				marshalType_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public RVA RVA
	{
		get
		{
			if (!rva_isInitialized)
			{
				InitializeRVA();
			}
			return rva;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				rva = value;
				rva_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public byte[] InitialValue
	{
		get
		{
			if (!initialValue_isInitialized)
			{
				InitializeInitialValue();
			}
			return initialValue;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				initialValue = value;
				initialValue_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
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

	public Constant Constant
	{
		get
		{
			if (!constant_isInitialized)
			{
				InitializeConstant();
			}
			return constant;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				constant = value;
				constant_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public bool HasImplMap => ImplMap != null;

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
				typeDef?.Fields.Remove(this);
				value?.Fields.Add(this);
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

	public FieldSig FieldSig
	{
		get
		{
			return signature as FieldSig;
		}
		set
		{
			signature = value;
		}
	}

	public ModuleDef Module => declaringType2?.Module;

	bool IIsTypeOrMethod.IsType => false;

	bool IIsTypeOrMethod.IsMethod => false;

	bool IMemberRef.IsField => true;

	bool IMemberRef.IsTypeSpec => false;

	bool IMemberRef.IsTypeRef => false;

	bool IMemberRef.IsTypeDef => false;

	bool IMemberRef.IsMethodSpec => false;

	bool IMemberRef.IsMethodDef => false;

	bool IMemberRef.IsMemberRef => false;

	bool IMemberRef.IsFieldDef => true;

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => false;

	public bool HasLayoutInfo => FieldOffset.HasValue;

	public bool HasConstant => Constant != null;

	public ElementType ElementType => Constant?.Type ?? ElementType.End;

	public bool HasMarshalType => MarshalType != null;

	public TypeSig FieldType
	{
		get
		{
			return FieldSig.GetFieldType();
		}
		set
		{
			FieldSig fieldSig = FieldSig;
			if (fieldSig != null)
			{
				fieldSig.Type = value;
			}
		}
	}

	public FieldAttributes Access
	{
		get
		{
			return (FieldAttributes)attributes & FieldAttributes.FieldAccessMask;
		}
		set
		{
			ModifyAttributes(~FieldAttributes.FieldAccessMask, value & FieldAttributes.FieldAccessMask);
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
			ModifyAttributes(value, FieldAttributes.Static);
		}
	}

	public bool IsInitOnly
	{
		get
		{
			return ((ushort)attributes & 0x20) != 0;
		}
		set
		{
			ModifyAttributes(value, FieldAttributes.InitOnly);
		}
	}

	public bool IsLiteral
	{
		get
		{
			return ((ushort)attributes & 0x40) != 0;
		}
		set
		{
			ModifyAttributes(value, FieldAttributes.Literal);
		}
	}

	public bool IsNotSerialized
	{
		get
		{
			return ((ushort)attributes & 0x80) != 0;
		}
		set
		{
			ModifyAttributes(value, FieldAttributes.NotSerialized);
		}
	}

	public bool IsSpecialName
	{
		get
		{
			return ((ushort)attributes & 0x200) != 0;
		}
		set
		{
			ModifyAttributes(value, FieldAttributes.SpecialName);
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
			ModifyAttributes(value, FieldAttributes.PinvokeImpl);
		}
	}

	public bool IsRuntimeSpecialName
	{
		get
		{
			return ((ushort)attributes & 0x400) != 0;
		}
		set
		{
			ModifyAttributes(value, FieldAttributes.RTSpecialName);
		}
	}

	public bool HasFieldMarshal
	{
		get
		{
			return ((ushort)attributes & 0x1000) != 0;
		}
		set
		{
			ModifyAttributes(value, FieldAttributes.HasFieldMarshal);
		}
	}

	public bool HasDefault
	{
		get
		{
			return ((ushort)attributes & 0x8000) != 0;
		}
		set
		{
			ModifyAttributes(value, FieldAttributes.HasDefault);
		}
	}

	public bool HasFieldRVA
	{
		get
		{
			return ((ushort)attributes & 0x100) != 0;
		}
		set
		{
			ModifyAttributes(value, FieldAttributes.HasFieldRVA);
		}
	}

	public string FullName => FullNameFactory.FieldFullName(declaringType2?.FullName, name, FieldSig);

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void InitializeFieldOffset()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!fieldOffset_isInitialized)
			{
				fieldOffset = GetFieldOffset_NoLock();
				fieldOffset_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual uint? GetFieldOffset_NoLock()
	{
		return null;
	}

	private void InitializeMarshalType()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!marshalType_isInitialized)
			{
				marshalType = GetMarshalType_NoLock();
				marshalType_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual MarshalType GetMarshalType_NoLock()
	{
		return null;
	}

	protected void ResetMarshalType()
	{
		marshalType_isInitialized = false;
	}

	private void InitializeRVA()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!rva_isInitialized)
			{
				rva = GetRVA_NoLock();
				rva_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual RVA GetRVA_NoLock()
	{
		return (RVA)0u;
	}

	protected void ResetRVA()
	{
		rva_isInitialized = false;
	}

	private void InitializeInitialValue()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!initialValue_isInitialized)
			{
				initialValue = GetInitialValue_NoLock();
				initialValue_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual byte[] GetInitialValue_NoLock()
	{
		return null;
	}

	protected void ResetInitialValue()
	{
		initialValue_isInitialized = false;
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

	private void InitializeConstant()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!constant_isInitialized)
			{
				constant = GetConstant_NoLock();
				constant_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual Constant GetConstant_NoLock()
	{
		return null;
	}

	protected void ResetConstant()
	{
		constant_isInitialized = false;
	}

	private void ModifyAttributes(FieldAttributes andMask, FieldAttributes orMask)
	{
		attributes = (attributes & (int)andMask) | (int)orMask;
	}

	private void ModifyAttributes(bool set, FieldAttributes flags)
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

	public uint GetFieldSize()
	{
		if (!GetFieldSize(out var size))
		{
			return 0u;
		}
		return size;
	}

	public bool GetFieldSize(out uint size)
	{
		return GetFieldSize(declaringType2, FieldSig, out size);
	}

	protected bool GetFieldSize(TypeDef declaringType, FieldSig fieldSig, out uint size)
	{
		return GetFieldSize(declaringType, fieldSig, GetPointerSize(declaringType), out size);
	}

	protected bool GetFieldSize(TypeDef declaringType, FieldSig fieldSig, int ptrSize, out uint size)
	{
		size = 0u;
		if (fieldSig == null)
		{
			return false;
		}
		return GetClassSize(declaringType, fieldSig.Type, ptrSize, out size);
	}

	private bool GetClassSize(TypeDef declaringType, TypeSig ts, int ptrSize, out uint size)
	{
		size = 0u;
		ts = ts.RemovePinnedAndModifiers();
		if (ts == null)
		{
			return false;
		}
		int primitiveSize = ts.ElementType.GetPrimitiveSize(ptrSize);
		if (primitiveSize >= 0)
		{
			size = (uint)primitiveSize;
			return true;
		}
		if (!(ts is TypeDefOrRefSig { TypeDef: var typeDef } typeDefOrRefSig))
		{
			return false;
		}
		if (typeDef != null)
		{
			return TypeDef.GetClassSize(typeDef, out size);
		}
		TypeRef typeRef = typeDefOrRefSig.TypeRef;
		if (typeRef != null)
		{
			return TypeDef.GetClassSize(typeRef.Resolve(), out size);
		}
		return false;
	}

	private int GetPointerSize(TypeDef declaringType)
	{
		if (declaringType == null)
		{
			return 4;
		}
		return declaringType.Module?.GetPointerSize() ?? 4;
	}

	public override string ToString()
	{
		return FullName;
	}
}
