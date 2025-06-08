using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

public abstract class FileDef : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IImplementation, IFullName, IHasCustomDebugInformation, IManagedEntryPoint
{
	protected uint rid;

	protected int attributes;

	protected UTF8String name;

	protected byte[] hashValue;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.File, rid);

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

	public int HasCustomAttributeTag => 16;

	public int ImplementationTag => 0;

	public FileAttributes Flags
	{
		get
		{
			return (FileAttributes)attributes;
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

	public byte[] HashValue
	{
		get
		{
			return hashValue;
		}
		set
		{
			hashValue = value;
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

	public int HasCustomDebugInformationTag => 16;

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

	public bool ContainsMetadata
	{
		get
		{
			return (attributes & 1) == 0;
		}
		set
		{
			ModifyAttributes(!value, FileAttributes.ContainsNoMetadata);
		}
	}

	public bool ContainsNoMetadata
	{
		get
		{
			return (attributes & 1) != 0;
		}
		set
		{
			ModifyAttributes(value, FileAttributes.ContainsNoMetadata);
		}
	}

	public string FullName => UTF8String.ToSystemStringOrEmpty(name);

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void ModifyAttributes(bool set, FileAttributes flags)
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

	public override string ToString()
	{
		return FullName;
	}
}
