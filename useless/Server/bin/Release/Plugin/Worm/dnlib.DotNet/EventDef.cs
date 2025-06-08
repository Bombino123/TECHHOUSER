using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Threading;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class EventDef : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasSemantic, IFullName, IMemberRef, IOwnerModule, IIsTypeOrMethod, IHasCustomDebugInformation, IMemberDef, IDnlibDef
{
	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected int attributes;

	protected UTF8String name;

	protected ITypeDefOrRef eventType;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	protected MethodDef addMethod;

	protected MethodDef invokeMethod;

	protected MethodDef removeMethod;

	protected IList<MethodDef> otherMethods;

	protected TypeDef declaringType2;

	public MDToken MDToken => new MDToken(Table.Event, rid);

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

	public int HasCustomAttributeTag => 10;

	public int HasSemanticTag => 0;

	public EventAttributes Attributes
	{
		get
		{
			return (EventAttributes)attributes;
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

	public ITypeDefOrRef EventType
	{
		get
		{
			return eventType;
		}
		set
		{
			eventType = value;
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

	public int HasCustomDebugInformationTag => 10;

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

	public MethodDef AddMethod
	{
		get
		{
			if (otherMethods == null)
			{
				InitializeEventMethods();
			}
			return addMethod;
		}
		set
		{
			if (otherMethods == null)
			{
				InitializeEventMethods();
			}
			addMethod = value;
		}
	}

	public MethodDef InvokeMethod
	{
		get
		{
			if (otherMethods == null)
			{
				InitializeEventMethods();
			}
			return invokeMethod;
		}
		set
		{
			if (otherMethods == null)
			{
				InitializeEventMethods();
			}
			invokeMethod = value;
		}
	}

	public MethodDef RemoveMethod
	{
		get
		{
			if (otherMethods == null)
			{
				InitializeEventMethods();
			}
			return removeMethod;
		}
		set
		{
			if (otherMethods == null)
			{
				InitializeEventMethods();
			}
			removeMethod = value;
		}
	}

	public IList<MethodDef> OtherMethods
	{
		get
		{
			if (otherMethods == null)
			{
				InitializeEventMethods();
			}
			return otherMethods;
		}
	}

	public bool IsEmpty
	{
		get
		{
			if (AddMethod == null && removeMethod == null && invokeMethod == null)
			{
				return otherMethods.Count == 0;
			}
			return false;
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public bool HasOtherMethods => OtherMethods.Count > 0;

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
				typeDef?.Events.Remove(this);
				value?.Events.Add(this);
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

	public string FullName => FullNameFactory.EventFullName(declaringType2?.FullName, name, eventType);

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

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => true;

	bool IMemberRef.IsGenericParam => false;

	public bool IsSpecialName
	{
		get
		{
			return ((ushort)attributes & 0x200) != 0;
		}
		set
		{
			ModifyAttributes(value, EventAttributes.SpecialName);
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
			ModifyAttributes(value, EventAttributes.RTSpecialName);
		}
	}

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void InitializeEventMethods()
	{
		theLock.EnterWriteLock();
		try
		{
			if (otherMethods == null)
			{
				InitializeEventMethods_NoLock();
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual void InitializeEventMethods_NoLock()
	{
		otherMethods = new List<MethodDef>();
	}

	protected void ResetMethods()
	{
		otherMethods = null;
	}

	private void ModifyAttributes(bool set, EventAttributes flags)
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
