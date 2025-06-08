using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.DotNet.Writer;
using dnlib.Utils;

namespace dnlib.DotNet;

public abstract class AssemblyDef : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasDeclSecurity, IFullName, IHasCustomDebugInformation, IAssembly, IListListener<ModuleDef>, ITypeDefFinder, IDnlibDef
{
	protected uint rid;

	protected AssemblyHashAlgorithm hashAlgorithm;

	protected Version version;

	protected int attributes;

	protected PublicKey publicKey;

	protected UTF8String name;

	protected UTF8String culture;

	protected IList<DeclSecurity> declSecurities;

	protected LazyList<ModuleDef> modules;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.Assembly, rid);

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

	public int HasCustomAttributeTag => 14;

	public int HasDeclSecurityTag => 2;

	public AssemblyHashAlgorithm HashAlgorithm
	{
		get
		{
			return hashAlgorithm;
		}
		set
		{
			hashAlgorithm = value;
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

	public PublicKey PublicKey
	{
		get
		{
			return publicKey;
		}
		set
		{
			publicKey = value ?? new PublicKey();
		}
	}

	public PublicKeyToken PublicKeyToken => publicKey.Token;

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

	public PublicKeyBase PublicKeyOrToken => publicKey;

	public string FullName => GetFullNameWithPublicKeyToken();

	public string FullNameToken => GetFullNameWithPublicKeyToken();

	public IList<ModuleDef> Modules
	{
		get
		{
			if (modules == null)
			{
				InitializeModules();
			}
			return modules;
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

	public int HasCustomDebugInformationTag => 14;

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

	public bool HasDeclSecurities => DeclSecurities.Count > 0;

	public bool HasModules => Modules.Count > 0;

	public ModuleDef ManifestModule
	{
		get
		{
			if (Modules.Count != 0)
			{
				return Modules[0];
			}
			return null;
		}
	}

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

	protected virtual void InitializeDeclSecurities()
	{
		Interlocked.CompareExchange(ref declSecurities, new List<DeclSecurity>(), null);
	}

	protected virtual void InitializeModules()
	{
		Interlocked.CompareExchange(ref modules, new LazyList<ModuleDef>(this), null);
	}

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

	public ModuleDef FindModule(UTF8String name)
	{
		IList<ModuleDef> list = Modules;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			ModuleDef moduleDef = list[i];
			if (moduleDef != null && UTF8String.CaseInsensitiveEquals(moduleDef.Name, name))
			{
				return moduleDef;
			}
		}
		return null;
	}

	public static AssemblyDef Load(string fileName, ModuleContext context)
	{
		return Load(fileName, new ModuleCreationOptions(context));
	}

	public static AssemblyDef Load(string fileName, ModuleCreationOptions options = null)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		ModuleDef moduleDef = null;
		try
		{
			moduleDef = ModuleDefMD.Load(fileName, options);
			return moduleDef.Assembly ?? throw new BadImageFormatException(fileName + " is only a .NET module, not a .NET assembly. Use ModuleDef.Load().");
		}
		catch
		{
			moduleDef?.Dispose();
			throw;
		}
	}

	public static AssemblyDef Load(byte[] data, ModuleContext context)
	{
		return Load(data, new ModuleCreationOptions(context));
	}

	public static AssemblyDef Load(byte[] data, ModuleCreationOptions options = null)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		ModuleDef moduleDef = null;
		try
		{
			moduleDef = ModuleDefMD.Load(data, options);
			return moduleDef.Assembly ?? throw new BadImageFormatException(moduleDef.ToString() + " is only a .NET module, not a .NET assembly. Use ModuleDef.Load().");
		}
		catch
		{
			moduleDef?.Dispose();
			throw;
		}
	}

	public static AssemblyDef Load(IntPtr addr, ModuleContext context)
	{
		return Load(addr, new ModuleCreationOptions(context));
	}

	public static AssemblyDef Load(IntPtr addr, ModuleCreationOptions options = null)
	{
		if (addr == IntPtr.Zero)
		{
			throw new ArgumentNullException("addr");
		}
		ModuleDef moduleDef = null;
		try
		{
			moduleDef = ModuleDefMD.Load(addr, options);
			return moduleDef.Assembly ?? throw new BadImageFormatException($"{moduleDef.ToString()} (addr: {addr.ToInt64():X8}) is only a .NET module, not a .NET assembly. Use ModuleDef.Load().");
		}
		catch
		{
			moduleDef?.Dispose();
			throw;
		}
	}

	public static AssemblyDef Load(Stream stream, ModuleContext context)
	{
		return Load(stream, new ModuleCreationOptions(context));
	}

	public static AssemblyDef Load(Stream stream, ModuleCreationOptions options = null)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		ModuleDef moduleDef = null;
		try
		{
			moduleDef = ModuleDefMD.Load(stream, options);
			return moduleDef.Assembly ?? throw new BadImageFormatException(moduleDef.ToString() + " is only a .NET module, not a .NET assembly. Use ModuleDef.Load().");
		}
		catch
		{
			moduleDef?.Dispose();
			throw;
		}
	}

	public string GetFullNameWithPublicKey()
	{
		return FullNameFactory.AssemblyFullName(this, withToken: false);
	}

	public string GetFullNameWithPublicKeyToken()
	{
		return FullNameFactory.AssemblyFullName(this, withToken: true);
	}

	public TypeDef Find(string fullName, bool isReflectionName)
	{
		IList<ModuleDef> list = Modules;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			ModuleDef moduleDef = list[i];
			if (moduleDef != null)
			{
				TypeDef typeDef = moduleDef.Find(fullName, isReflectionName);
				if (typeDef != null)
				{
					return typeDef;
				}
			}
		}
		return null;
	}

	public TypeDef Find(TypeRef typeRef)
	{
		IList<ModuleDef> list = Modules;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			ModuleDef moduleDef = list[i];
			if (moduleDef != null)
			{
				TypeDef typeDef = moduleDef.Find(typeRef);
				if (typeDef != null)
				{
					return typeDef;
				}
			}
		}
		return null;
	}

	public void Write(string filename, ModuleWriterOptions options = null)
	{
		ManifestModule.Write(filename, options);
	}

	public void Write(Stream dest, ModuleWriterOptions options = null)
	{
		ManifestModule.Write(dest, options);
	}

	public bool IsFriendAssemblyOf(AssemblyDef targetAsm)
	{
		if (targetAsm == null)
		{
			return false;
		}
		if (this == targetAsm)
		{
			return true;
		}
		if (PublicKeyBase.IsNullOrEmpty2(publicKey) != PublicKeyBase.IsNullOrEmpty2(targetAsm.PublicKey))
		{
			return false;
		}
		foreach (CustomAttribute item in targetAsm.CustomAttributes.FindAll("System.Runtime.CompilerServices.InternalsVisibleToAttribute"))
		{
			if (item.ConstructorArguments.Count != 1)
			{
				continue;
			}
			CAArgument cAArgument = ((item.ConstructorArguments.Count == 0) ? default(CAArgument) : item.ConstructorArguments[0]);
			if (cAArgument.Type.GetElementType() != ElementType.String)
			{
				continue;
			}
			UTF8String uTF8String = cAArgument.Value as UTF8String;
			if (UTF8String.IsNull(uTF8String))
			{
				continue;
			}
			AssemblyNameInfo assemblyNameInfo = new AssemblyNameInfo(uTF8String);
			if (assemblyNameInfo.Name != name)
			{
				continue;
			}
			if (!PublicKeyBase.IsNullOrEmpty2(publicKey))
			{
				if (!PublicKey.Equals(assemblyNameInfo.PublicKeyOrToken as PublicKey))
				{
					continue;
				}
			}
			else if (!PublicKeyBase.IsNullOrEmpty2(assemblyNameInfo.PublicKeyOrToken))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	public void UpdateOrCreateAssemblySignatureKeyAttribute(StrongNamePublicKey identityPubKey, StrongNameKey identityKey, StrongNamePublicKey signaturePubKey)
	{
		ModuleDef manifestModule = ManifestModule;
		if (manifestModule == null)
		{
			return;
		}
		CustomAttribute customAttribute = null;
		for (int i = 0; i < CustomAttributes.Count; i++)
		{
			CustomAttribute customAttribute2 = CustomAttributes[i];
			if (!(customAttribute2.TypeFullName != "System.Reflection.AssemblySignatureKeyAttribute"))
			{
				CustomAttributes.RemoveAt(i);
				i--;
				if (customAttribute == null)
				{
					customAttribute = customAttribute2;
				}
			}
		}
		if (IsValidAssemblySignatureKeyAttribute(customAttribute))
		{
			customAttribute.NamedArguments.Clear();
		}
		else
		{
			customAttribute = CreateAssemblySignatureKeyAttribute();
		}
		string s = StrongNameKey.CreateCounterSignatureAsString(identityPubKey, identityKey, signaturePubKey);
		customAttribute.ConstructorArguments[0] = new CAArgument(manifestModule.CorLibTypes.String, new UTF8String(signaturePubKey.ToString()));
		customAttribute.ConstructorArguments[1] = new CAArgument(manifestModule.CorLibTypes.String, new UTF8String(s));
		CustomAttributes.Add(customAttribute);
	}

	private bool IsValidAssemblySignatureKeyAttribute(CustomAttribute ca)
	{
		if (Settings.IsThreadSafe)
		{
			return false;
		}
		if (ca == null)
		{
			return false;
		}
		ICustomAttributeType constructor = ca.Constructor;
		if (constructor == null)
		{
			return false;
		}
		MethodSig methodSig = constructor.MethodSig;
		if (methodSig == null || methodSig.Params.Count != 2)
		{
			return false;
		}
		if (methodSig.Params[0].GetElementType() != ElementType.String)
		{
			return false;
		}
		if (methodSig.Params[1].GetElementType() != ElementType.String)
		{
			return false;
		}
		if (ca.ConstructorArguments.Count != 2)
		{
			return false;
		}
		return true;
	}

	private CustomAttribute CreateAssemblySignatureKeyAttribute()
	{
		ModuleDef manifestModule = ManifestModule;
		TypeRefUser @class = manifestModule.UpdateRowId(new TypeRefUser(manifestModule, "System.Reflection", "AssemblySignatureKeyAttribute", manifestModule.CorLibTypes.AssemblyRef));
		MethodSig sig = MethodSig.CreateInstance(manifestModule.CorLibTypes.Void, manifestModule.CorLibTypes.String, manifestModule.CorLibTypes.String);
		return new CustomAttribute(manifestModule.UpdateRowId(new MemberRefUser(manifestModule, MethodDef.InstanceConstructorName, sig, @class)))
		{
			ConstructorArguments = 
			{
				new CAArgument(manifestModule.CorLibTypes.String, UTF8String.Empty),
				new CAArgument(manifestModule.CorLibTypes.String, UTF8String.Empty)
			}
		};
	}

	public virtual bool TryGetOriginalTargetFrameworkAttribute(out string framework, out Version version, out string profile)
	{
		framework = null;
		version = null;
		profile = null;
		return false;
	}

	void IListListener<ModuleDef>.OnLazyAdd(int index, ref ModuleDef module)
	{
		_ = module;
	}

	void IListListener<ModuleDef>.OnAdd(int index, ModuleDef module)
	{
		if (module != null)
		{
			if (module.Assembly != null)
			{
				throw new InvalidOperationException("Module already has an assembly. Remove it from that assembly before adding it to this assembly.");
			}
			module.Assembly = this;
		}
	}

	void IListListener<ModuleDef>.OnRemove(int index, ModuleDef module)
	{
		if (module != null)
		{
			module.Assembly = null;
		}
	}

	void IListListener<ModuleDef>.OnResize(int index)
	{
	}

	void IListListener<ModuleDef>.OnClear()
	{
		foreach (ModuleDef item in modules.GetEnumerable_NoLock())
		{
			if (item != null)
			{
				item.Assembly = null;
			}
		}
	}

	public override string ToString()
	{
		return FullName;
	}
}
