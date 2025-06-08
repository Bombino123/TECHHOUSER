using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Threading;

namespace dnlib.DotNet;

[DebuggerDisplay("{Sequence} {Name}")]
[ComVisible(true)]
public abstract class ParamDef : IHasConstant, ICodedToken, IMDTokenProvider, IHasCustomAttribute, IFullName, IHasFieldMarshal, IHasCustomDebugInformation
{
	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected MethodDef declaringMethod;

	protected int attributes;

	protected ushort sequence;

	protected UTF8String name;

	protected MarshalType marshalType;

	protected bool marshalType_isInitialized;

	protected Constant constant;

	protected bool constant_isInitialized;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.Param, rid);

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

	public int HasConstantTag => 1;

	public int HasCustomAttributeTag => 4;

	public int HasFieldMarshalTag => 1;

	public MethodDef DeclaringMethod
	{
		get
		{
			return declaringMethod;
		}
		internal set
		{
			declaringMethod = value;
		}
	}

	public ParamAttributes Attributes
	{
		get
		{
			return (ParamAttributes)attributes;
		}
		set
		{
			attributes = (int)value;
		}
	}

	public ushort Sequence
	{
		get
		{
			return sequence;
		}
		set
		{
			sequence = value;
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

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public int HasCustomDebugInformationTag => 4;

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

	public bool HasConstant => Constant != null;

	public ElementType ElementType => Constant?.Type ?? ElementType.End;

	public bool HasMarshalType => MarshalType != null;

	public string FullName
	{
		get
		{
			UTF8String uTF8String = name;
			if (UTF8String.IsNullOrEmpty(uTF8String))
			{
				return $"A_{sequence}";
			}
			return uTF8String.String;
		}
	}

	public bool IsIn
	{
		get
		{
			return ((ushort)attributes & 1) != 0;
		}
		set
		{
			ModifyAttributes(value, ParamAttributes.In);
		}
	}

	public bool IsOut
	{
		get
		{
			return ((ushort)attributes & 2) != 0;
		}
		set
		{
			ModifyAttributes(value, ParamAttributes.Out);
		}
	}

	public bool IsLcid
	{
		get
		{
			return ((ushort)attributes & 4) != 0;
		}
		set
		{
			ModifyAttributes(value, ParamAttributes.Lcid);
		}
	}

	public bool IsRetval
	{
		get
		{
			return ((ushort)attributes & 8) != 0;
		}
		set
		{
			ModifyAttributes(value, ParamAttributes.Retval);
		}
	}

	public bool IsOptional
	{
		get
		{
			return ((ushort)attributes & 0x10) != 0;
		}
		set
		{
			ModifyAttributes(value, ParamAttributes.Optional);
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
			ModifyAttributes(value, ParamAttributes.HasDefault);
		}
	}

	public bool HasFieldMarshal
	{
		get
		{
			return ((ushort)attributes & 0x2000) != 0;
		}
		set
		{
			ModifyAttributes(value, ParamAttributes.HasFieldMarshal);
		}
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

	private void ModifyAttributes(bool set, ParamAttributes flags)
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
}
