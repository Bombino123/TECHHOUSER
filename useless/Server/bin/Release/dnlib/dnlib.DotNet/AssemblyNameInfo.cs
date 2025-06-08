using System;
using System.Reflection;

namespace dnlib.DotNet;

public sealed class AssemblyNameInfo : IAssembly, IFullName
{
	private AssemblyHashAlgorithm hashAlgId;

	private Version version;

	private AssemblyAttributes flags;

	private PublicKeyBase publicKeyOrToken;

	private UTF8String name;

	private UTF8String culture;

	public AssemblyHashAlgorithm HashAlgId
	{
		get
		{
			return hashAlgId;
		}
		set
		{
			hashAlgId = value;
		}
	}

	public Version Version
	{
		get
		{
			return version;
		}
		set
		{
			version = value;
		}
	}

	public AssemblyAttributes Attributes
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

	public PublicKeyBase PublicKeyOrToken
	{
		get
		{
			return publicKeyOrToken;
		}
		set
		{
			publicKeyOrToken = value;
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

	public string FullName => FullNameToken;

	public string FullNameToken => FullNameFactory.AssemblyFullName(this, withToken: true);

	public bool HasPublicKey
	{
		get
		{
			return (Attributes & AssemblyAttributes.PublicKey) != 0;
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
			return Attributes & AssemblyAttributes.PA_NoPlatform;
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
			return Attributes & AssemblyAttributes.PA_FullMask;
		}
		set
		{
			ModifyAttributes(~AssemblyAttributes.PA_FullMask, value & AssemblyAttributes.PA_FullMask);
		}
	}

	public bool IsProcessorArchitectureNone => (Attributes & AssemblyAttributes.PA_NoPlatform) == 0;

	public bool IsProcessorArchitectureMSIL => (Attributes & AssemblyAttributes.PA_NoPlatform) == AssemblyAttributes.PA_MSIL;

	public bool IsProcessorArchitectureX86 => (Attributes & AssemblyAttributes.PA_NoPlatform) == AssemblyAttributes.PA_x86;

	public bool IsProcessorArchitectureIA64 => (Attributes & AssemblyAttributes.PA_NoPlatform) == AssemblyAttributes.PA_IA64;

	public bool IsProcessorArchitectureX64 => (Attributes & AssemblyAttributes.PA_NoPlatform) == AssemblyAttributes.PA_AMD64;

	public bool IsProcessorArchitectureARM => (Attributes & AssemblyAttributes.PA_NoPlatform) == AssemblyAttributes.PA_ARM;

	public bool IsProcessorArchitectureNoPlatform => (Attributes & AssemblyAttributes.PA_NoPlatform) == AssemblyAttributes.PA_NoPlatform;

	public bool IsProcessorArchitectureSpecified
	{
		get
		{
			return (Attributes & AssemblyAttributes.PA_Specified) != 0;
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
			return (Attributes & AssemblyAttributes.EnableJITcompileTracking) != 0;
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
			return (Attributes & AssemblyAttributes.DisableJITcompileOptimizer) != 0;
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
			return (Attributes & AssemblyAttributes.Retargetable) != 0;
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
			return Attributes & AssemblyAttributes.ContentType_Mask;
		}
		set
		{
			ModifyAttributes(~AssemblyAttributes.ContentType_Mask, value & AssemblyAttributes.ContentType_Mask);
		}
	}

	public bool IsContentTypeDefault => (Attributes & AssemblyAttributes.ContentType_Mask) == 0;

	public bool IsContentTypeWindowsRuntime => (Attributes & AssemblyAttributes.ContentType_Mask) == AssemblyAttributes.ContentType_WindowsRuntime;

	private void ModifyAttributes(AssemblyAttributes andMask, AssemblyAttributes orMask)
	{
		Attributes = (Attributes & andMask) | orMask;
	}

	private void ModifyAttributes(bool set, AssemblyAttributes flags)
	{
		if (set)
		{
			Attributes |= flags;
		}
		else
		{
			Attributes &= ~flags;
		}
	}

	public AssemblyNameInfo()
	{
	}

	public AssemblyNameInfo(string asmFullName)
		: this(ReflectionTypeNameParser.ParseAssemblyRef(asmFullName))
	{
	}

	public AssemblyNameInfo(IAssembly asm)
	{
		if (asm != null)
		{
			hashAlgId = (asm as AssemblyDef)?.HashAlgorithm ?? AssemblyHashAlgorithm.None;
			version = asm.Version ?? new Version(0, 0, 0, 0);
			flags = asm.Attributes;
			publicKeyOrToken = asm.PublicKeyOrToken;
			name = (UTF8String.IsNullOrEmpty(asm.Name) ? UTF8String.Empty : asm.Name);
			culture = (UTF8String.IsNullOrEmpty(asm.Culture) ? UTF8String.Empty : asm.Culture);
		}
	}

	public AssemblyNameInfo(AssemblyName asmName)
	{
		if (asmName != null)
		{
			hashAlgId = (AssemblyHashAlgorithm)asmName.HashAlgorithm;
			version = asmName.Version ?? new Version(0, 0, 0, 0);
			flags = (AssemblyAttributes)asmName.Flags;
			publicKeyOrToken = (PublicKeyBase)(((object)PublicKeyBase.CreatePublicKey(asmName.GetPublicKey())) ?? ((object)PublicKeyBase.CreatePublicKeyToken(asmName.GetPublicKeyToken())));
			name = asmName.Name ?? string.Empty;
			culture = ((asmName.CultureInfo != null && asmName.CultureInfo.Name != null) ? asmName.CultureInfo.Name : string.Empty);
		}
	}

	public override string ToString()
	{
		return FullName;
	}
}
