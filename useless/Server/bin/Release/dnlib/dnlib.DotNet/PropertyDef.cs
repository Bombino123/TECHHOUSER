using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Threading;

namespace dnlib.DotNet;

public abstract class PropertyDef : IHasConstant, ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName, IHasSemantic, IMemberRef, IOwnerModule, IIsTypeOrMethod, IHasCustomDebugInformation, IMemberDef, IDnlibDef
{
	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected int attributes;

	protected UTF8String name;

	protected CallingConventionSig type;

	protected Constant constant;

	protected bool constant_isInitialized;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	protected IList<MethodDef> getMethods;

	protected IList<MethodDef> setMethods;

	protected IList<MethodDef> otherMethods;

	protected TypeDef declaringType2;

	public MDToken MDToken => new MDToken(Table.Property, rid);

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

	public int HasConstantTag => 2;

	public int HasCustomAttributeTag => 9;

	public int HasSemanticTag => 1;

	public PropertyAttributes Attributes
	{
		get
		{
			return (PropertyAttributes)attributes;
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

	public CallingConventionSig Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
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

	public int HasCustomDebugInformationTag => 9;

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

	public MethodDef GetMethod
	{
		get
		{
			if (otherMethods == null)
			{
				InitializePropertyMethods();
			}
			if (getMethods.Count != 0)
			{
				return getMethods[0];
			}
			return null;
		}
		set
		{
			if (otherMethods == null)
			{
				InitializePropertyMethods();
			}
			if (value == null)
			{
				getMethods.Clear();
			}
			else if (getMethods.Count == 0)
			{
				getMethods.Add(value);
			}
			else
			{
				getMethods[0] = value;
			}
		}
	}

	public MethodDef SetMethod
	{
		get
		{
			if (otherMethods == null)
			{
				InitializePropertyMethods();
			}
			if (setMethods.Count != 0)
			{
				return setMethods[0];
			}
			return null;
		}
		set
		{
			if (otherMethods == null)
			{
				InitializePropertyMethods();
			}
			if (value == null)
			{
				setMethods.Clear();
			}
			else if (setMethods.Count == 0)
			{
				setMethods.Add(value);
			}
			else
			{
				setMethods[0] = value;
			}
		}
	}

	public IList<MethodDef> GetMethods
	{
		get
		{
			if (otherMethods == null)
			{
				InitializePropertyMethods();
			}
			return getMethods;
		}
	}

	public IList<MethodDef> SetMethods
	{
		get
		{
			if (otherMethods == null)
			{
				InitializePropertyMethods();
			}
			return setMethods;
		}
	}

	public IList<MethodDef> OtherMethods
	{
		get
		{
			if (otherMethods == null)
			{
				InitializePropertyMethods();
			}
			return otherMethods;
		}
	}

	public bool IsEmpty
	{
		get
		{
			if (GetMethods.Count == 0 && setMethods.Count == 0)
			{
				return otherMethods.Count == 0;
			}
			return false;
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public bool HasOtherMethods => OtherMethods.Count > 0;

	public bool HasConstant => Constant != null;

	public ElementType ElementType => Constant?.Type ?? ElementType.End;

	public PropertySig PropertySig
	{
		get
		{
			return type as PropertySig;
		}
		set
		{
			type = value;
		}
	}

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
				typeDef?.Properties.Remove(this);
				value?.Properties.Add(this);
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

	public string FullName => FullNameFactory.PropertyFullName(declaringType2?.FullName, name, type);

	bool IIsTypeOrMethod.IsType => false;

	bool IIsTypeOrMethod.IsMethod => false;

	bool IMemberRef.IsField => false;

	bool IMemberRef.IsTypeSpec => false;

	bool IMemberRef.IsTypeRef => false;

	bool IMemberRef.IsTypeDef => false;

	bool IMemberRef.IsMethodSpec => false;

	bool IMemberRef.IsMethodDef => false;

	bool IMemberRef.IsMemberRef => false;

	bool IMemberRef.IsFieldDef => false;

	bool IMemberRef.IsPropertyDef => true;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => false;

	public bool IsSpecialName
	{
		get
		{
			return ((ushort)attributes & 0x200) != 0;
		}
		set
		{
			ModifyAttributes(value, PropertyAttributes.SpecialName);
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
			ModifyAttributes(value, PropertyAttributes.RTSpecialName);
		}
	}

	public bool HasDefault
	{
		get
		{
			return ((ushort)attributes & 0x1000) != 0;
		}
		set
		{
			ModifyAttributes(value, PropertyAttributes.HasDefault);
		}
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

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void InitializePropertyMethods()
	{
		theLock.EnterWriteLock();
		try
		{
			if (otherMethods == null)
			{
				InitializePropertyMethods_NoLock();
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual void InitializePropertyMethods_NoLock()
	{
		getMethods = new List<MethodDef>();
		setMethods = new List<MethodDef>();
		otherMethods = new List<MethodDef>();
	}

	protected void ResetMethods()
	{
		otherMethods = null;
	}

	private void ModifyAttributes(bool set, PropertyAttributes flags)
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

	public override string ToString()
	{
		return FullName;
	}
}
