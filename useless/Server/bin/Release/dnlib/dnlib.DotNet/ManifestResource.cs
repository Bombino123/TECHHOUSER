using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

[DebuggerDisplay("{Offset} {Name.String} {Implementation}")]
public abstract class ManifestResource : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasCustomDebugInformation
{
	protected uint rid;

	protected uint offset;

	protected int attributes;

	protected UTF8String name;

	protected IImplementation implementation;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.ManifestResource, rid);

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

	public int HasCustomAttributeTag => 18;

	public uint Offset
	{
		get
		{
			return offset;
		}
		set
		{
			offset = value;
		}
	}

	public ManifestResourceAttributes Flags
	{
		get
		{
			return (ManifestResourceAttributes)attributes;
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

	public IImplementation Implementation
	{
		get
		{
			return implementation;
		}
		set
		{
			implementation = value;
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

	public int HasCustomDebugInformationTag => 18;

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

	public ManifestResourceAttributes Visibility
	{
		get
		{
			return (ManifestResourceAttributes)((uint)attributes & 7u);
		}
		set
		{
			ModifyAttributes(~ManifestResourceAttributes.VisibilityMask, value & ManifestResourceAttributes.VisibilityMask);
		}
	}

	public bool IsPublic => (attributes & 7) == 1;

	public bool IsPrivate => (attributes & 7) == 2;

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void ModifyAttributes(ManifestResourceAttributes andMask, ManifestResourceAttributes orMask)
	{
		attributes = (attributes & (int)andMask) | (int)orMask;
	}
}
