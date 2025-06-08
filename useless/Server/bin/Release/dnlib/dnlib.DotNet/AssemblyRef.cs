using System;
using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;

namespace dnlib.DotNet;

public abstract class AssemblyRef : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IImplementation, IFullName, IResolutionScope, IHasCustomDebugInformation, IAssembly, IScope
{
	public static readonly AssemblyRef CurrentAssembly = new AssemblyRefUser("<<<CURRENT_ASSEMBLY>>>");

	protected uint rid;

	protected Version version;

	protected int attributes;

	protected PublicKeyBase publicKeyOrToken;

	protected UTF8String name;

	protected UTF8String culture;

	protected byte[] hashValue;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.AssemblyRef, rid);

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

	public int HasCustomAttributeTag => 15;

	public int ImplementationTag => 1;

	public int ResolutionScopeTag => 2;

	public ScopeType ScopeType => ScopeType.AssemblyRef;

	public string ScopeName => FullName;

	public Version Version
	{
		get
		{
			return version;
		}
		set
		{
			version = value ?? throw new ArgumentNullException("value");
		}
	}

	public AssemblyAttributes Attributes
	{
		get
		{
			return (AssemblyAttributes)attributes;
		}
		set
		{
			attributes = (int)value;
		}
	}

	public PublicKeyBase PublicKeyOrToken
	{
		get
		{
			return publicKeyOrToken;
		}
		set
		{
			publicKeyOrToken = value ?? throw new ArgumentNullException("value");
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

	public UTF8String Culture
	{
		get
		{
			return culture;
		}
		set
		{
			culture = value;
		}
	}

	public byte[] Hash
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

	public int HasCustomDebugInformationTag => 15;

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

	public string FullName => FullNameToken;

	public string RealFullName => FullNameFactory.AssemblyFullName(this, withToken: false);

	public string FullNameToken => FullNameFactory.AssemblyFullName(this, withToken: true);

	public bool HasPublicKey
	{
		get
		{
			return (attributes & 1) != 0;
		}
		set
		{
			ModifyAttributes(value, AssemblyAttributes.PublicKey);
		}
	}

	public AssemblyAttributes ProcessorArchitecture
	{
		get
		{
			return (AssemblyAttributes)((uint)attributes & 0x70u);
		}
		set
		{
			ModifyAttributes(~AssemblyAttributes.PA_NoPlatform, value & AssemblyAttributes.PA_NoPlatform);
		}
	}

	public AssemblyAttributes ProcessorArchitectureFull
	{
		get
		{
			return (AssemblyAttributes)((uint)attributes & 0xF0u);
		}
		set
		{
			ModifyAttributes(~AssemblyAttributes.PA_FullMask, value & AssemblyAttributes.PA_FullMask);
		}
	}

	public bool IsProcessorArchitectureNone => (attributes & 0x70) == 0;

	public bool IsProcessorArchitectureMSIL => (attributes & 0x70) == 16;

	public bool IsProcessorArchitectureX86 => (attributes & 0x70) == 32;

	public bool IsProcessorArchitectureIA64 => (attributes & 0x70) == 48;

	public bool IsProcessorArchitectureX64 => (attributes & 0x70) == 64;

	public bool IsProcessorArchitectureARM => (attributes & 0x70) == 80;

	public bool IsProcessorArchitectureNoPlatform => (attributes & 0x70) == 112;

	public bool IsProcessorArchitectureSpecified
	{
		get
		{
			return (attributes & 0x80) != 0;
		}
		set
		{
			ModifyAttributes(value, AssemblyAttributes.PA_Specified);
		}
	}

	public bool EnableJITcompileTracking
	{
		get
		{
			return (attributes & 0x8000) != 0;
		}
		set
		{
			ModifyAttributes(value, AssemblyAttributes.EnableJITcompileTracking);
		}
	}

	public bool DisableJITcompileOptimizer
	{
		get
		{
			return (attributes & 0x4000) != 0;
		}
		set
		{
			ModifyAttributes(value, AssemblyAttributes.DisableJITcompileOptimizer);
		}
	}

	public bool IsRetargetable
	{
		get
		{
			return (attributes & 0x100) != 0;
		}
		set
		{
			ModifyAttributes(value, AssemblyAttributes.Retargetable);
		}
	}

	public AssemblyAttributes ContentType
	{
		get
		{
			return (AssemblyAttributes)((uint)attributes & 0xE00u);
		}
		set
		{
			ModifyAttributes(~AssemblyAttributes.ContentType_Mask, value & AssemblyAttributes.ContentType_Mask);
		}
	}

	public bool IsContentTypeDefault => (attributes & 0xE00) == 0;

	public bool IsContentTypeWindowsRuntime => (attributes & 0xE00) == 512;

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void ModifyAttributes(AssemblyAttributes andMask, AssemblyAttributes orMask)
	{
		attributes = (attributes & (int)andMask) | (int)orMask;
	}

	private void ModifyAttributes(bool set, AssemblyAttributes flags)
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
