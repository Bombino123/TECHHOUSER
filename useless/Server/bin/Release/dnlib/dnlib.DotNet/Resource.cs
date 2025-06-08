using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

public abstract class Resource : IMDTokenProvider, IHasCustomAttribute, ICodedToken, IHasCustomDebugInformation
{
	private protected uint rid;

	private protected uint? offset;

	private UTF8String name;

	private ManifestResourceAttributes flags;

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

	public uint? Offset
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

	public ManifestResourceAttributes Attributes
	{
		get
		{
			return flags;
		}
		set
		{
			flags = value;
		}
	}

	public abstract ResourceType ResourceType { get; }

	public ManifestResourceAttributes Visibility
	{
		get
		{
			return flags & ManifestResourceAttributes.VisibilityMask;
		}
		set
		{
			flags = (flags & ~ManifestResourceAttributes.VisibilityMask) | (value & ManifestResourceAttributes.VisibilityMask);
		}
	}

	public bool IsPublic => (flags & ManifestResourceAttributes.VisibilityMask) == ManifestResourceAttributes.Public;

	public bool IsPrivate => (flags & ManifestResourceAttributes.VisibilityMask) == ManifestResourceAttributes.Private;

	public int HasCustomAttributeTag => 18;

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

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	protected Resource(UTF8String name, ManifestResourceAttributes flags)
	{
		this.name = name;
		this.flags = flags;
	}
}
