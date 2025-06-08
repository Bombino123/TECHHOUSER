using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.DotNet.Writer;
using dnlib.PE;
using dnlib.Threading;
using dnlib.Utils;
using dnlib.W32Resources;

namespace dnlib.DotNet;

public abstract class ModuleDef : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasCustomDebugInformation, IResolutionScope, IFullName, IDisposable, IListListener<TypeDef>, IModule, IScope, ITypeDefFinder, IDnlibDef, ITokenResolver, ISignatureReaderHelper
{
	protected const Characteristics DefaultCharacteristics = Characteristics.ExecutableImage | Characteristics.Bit32Machine;

	protected const DllCharacteristics DefaultDllCharacteristics = DllCharacteristics.DynamicBase | DllCharacteristics.NxCompat | DllCharacteristics.NoSeh | DllCharacteristics.TerminalServerAware;

	protected uint rid;

	private readonly Lock theLock = Lock.Create();

	protected ICorLibTypes corLibTypes;

	protected PdbState pdbState;

	private TypeDefFinder typeDefFinder;

	protected readonly int[] lastUsedRids = new int[64];

	protected ModuleContext context;

	private object tag;

	protected ushort generation;

	protected UTF8String name;

	protected Guid? mvid;

	protected Guid? encId;

	protected Guid? encBaseId;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	protected AssemblyDef assembly;

	protected LazyList<TypeDef> types;

	protected IList<ExportedType> exportedTypes;

	protected RVA nativeEntryPoint;

	protected IManagedEntryPoint managedEntryPoint;

	protected bool nativeAndManagedEntryPoint_initialized;

	protected ResourceCollection resources;

	protected VTableFixups vtableFixups;

	protected bool vtableFixups_isInitialized;

	protected string location;

	protected Win32Resources win32Resources;

	protected bool win32Resources_isInitialized;

	private string runtimeVersion;

	private WinMDStatus? cachedWinMDStatus;

	private string runtimeVersionWinMD;

	private string winMDVersion;

	protected int cor20HeaderFlags;

	public MDToken MDToken => new MDToken(Table.Module, rid);

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

	public int HasCustomAttributeTag => 7;

	public int ResolutionScopeTag => 0;

	public object Tag
	{
		get
		{
			return tag;
		}
		set
		{
			tag = value;
		}
	}

	public ScopeType ScopeType => ScopeType.ModuleDef;

	public string ScopeName => FullName;

	public ushort Generation
	{
		get
		{
			return generation;
		}
		set
		{
			generation = value;
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

	public Guid? Mvid
	{
		get
		{
			return mvid;
		}
		set
		{
			mvid = value;
		}
	}

	public Guid? EncId
	{
		get
		{
			return encId;
		}
		set
		{
			encId = value;
		}
	}

	public Guid? EncBaseId
	{
		get
		{
			return encBaseId;
		}
		set
		{
			encBaseId = value;
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

	public int HasCustomDebugInformationTag => 7;

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

	public AssemblyDef Assembly
	{
		get
		{
			return assembly;
		}
		internal set
		{
			assembly = value;
		}
	}

	public IList<TypeDef> Types
	{
		get
		{
			if (types == null)
			{
				InitializeTypes();
			}
			return types;
		}
	}

	public IList<ExportedType> ExportedTypes
	{
		get
		{
			if (exportedTypes == null)
			{
				InitializeExportedTypes();
			}
			return exportedTypes;
		}
	}

	public RVA NativeEntryPoint
	{
		get
		{
			if (!nativeAndManagedEntryPoint_initialized)
			{
				InitializeNativeAndManagedEntryPoint();
			}
			return nativeEntryPoint;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				nativeEntryPoint = value;
				managedEntryPoint = null;
				Cor20HeaderFlags |= ComImageFlags.NativeEntryPoint;
				nativeAndManagedEntryPoint_initialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public IManagedEntryPoint ManagedEntryPoint
	{
		get
		{
			if (!nativeAndManagedEntryPoint_initialized)
			{
				InitializeNativeAndManagedEntryPoint();
			}
			return managedEntryPoint;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				nativeEntryPoint = (RVA)0u;
				managedEntryPoint = value;
				Cor20HeaderFlags &= ~ComImageFlags.NativeEntryPoint;
				nativeAndManagedEntryPoint_initialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public MethodDef EntryPoint
	{
		get
		{
			return ManagedEntryPoint as MethodDef;
		}
		set
		{
			ManagedEntryPoint = value;
		}
	}

	public bool IsNativeEntryPointValid => NativeEntryPoint != (RVA)0u;

	public bool IsManagedEntryPointValid => ManagedEntryPoint != null;

	public bool IsEntryPointValid => EntryPoint != null;

	public ResourceCollection Resources
	{
		get
		{
			if (resources == null)
			{
				InitializeResources();
			}
			return resources;
		}
	}

	public VTableFixups VTableFixups
	{
		get
		{
			if (!vtableFixups_isInitialized)
			{
				InitializeVTableFixups();
			}
			return vtableFixups;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				vtableFixups = value;
				vtableFixups_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public bool HasTypes => Types.Count > 0;

	public bool HasExportedTypes => ExportedTypes.Count > 0;

	public bool HasResources => Resources.Count > 0;

	public string FullName => UTF8String.ToSystemStringOrEmpty(name);

	public string Location
	{
		get
		{
			return location;
		}
		set
		{
			location = value;
		}
	}

	public ICorLibTypes CorLibTypes => corLibTypes;

	private TypeDefFinder TypeDefFinder
	{
		get
		{
			if (typeDefFinder == null)
			{
				Interlocked.CompareExchange(ref typeDefFinder, new TypeDefFinder(Types), null);
			}
			return typeDefFinder;
		}
	}

	public ModuleContext Context
	{
		get
		{
			if (context == null)
			{
				Interlocked.CompareExchange(ref context, new ModuleContext(), null);
			}
			return context;
		}
		set
		{
			context = value ?? new ModuleContext();
		}
	}

	public bool EnableTypeDefFindCache
	{
		get
		{
			return TypeDefFinder.IsCacheEnabled;
		}
		set
		{
			TypeDefFinder.IsCacheEnabled = value;
		}
	}

	public bool IsManifestModule
	{
		get
		{
			AssemblyDef assemblyDef = assembly;
			if (assemblyDef != null)
			{
				return assemblyDef.ManifestModule == this;
			}
			return false;
		}
	}

	public TypeDef GlobalType
	{
		get
		{
			if (Types.Count != 0)
			{
				return Types[0];
			}
			return null;
		}
	}

	public bool? IsCoreLibraryModule { get; set; }

	public Win32Resources Win32Resources
	{
		get
		{
			if (!win32Resources_isInitialized)
			{
				InitializeWin32Resources();
			}
			return win32Resources;
		}
		set
		{
			theLock.EnterWriteLock();
			try
			{
				win32Resources = value;
				win32Resources_isInitialized = true;
			}
			finally
			{
				theLock.ExitWriteLock();
			}
		}
	}

	public PdbState PdbState => pdbState;

	public ModuleKind Kind { get; set; }

	public Characteristics Characteristics { get; set; }

	public DllCharacteristics DllCharacteristics { get; set; }

	public string RuntimeVersion
	{
		get
		{
			return runtimeVersion;
		}
		set
		{
			if (runtimeVersion != value)
			{
				runtimeVersion = value;
				cachedWinMDStatus = null;
				runtimeVersionWinMD = null;
				winMDVersion = null;
			}
		}
	}

	public WinMDStatus WinMDStatus
	{
		get
		{
			WinMDStatus? winMDStatus = cachedWinMDStatus;
			if (winMDStatus.HasValue)
			{
				return winMDStatus.Value;
			}
			winMDStatus = (cachedWinMDStatus = CalculateWinMDStatus(RuntimeVersion));
			return winMDStatus.Value;
		}
	}

	public bool IsWinMD => WinMDStatus != WinMDStatus.None;

	public bool IsManagedWinMD => WinMDStatus == WinMDStatus.Managed;

	public bool IsPureWinMD => WinMDStatus == WinMDStatus.Pure;

	public string RuntimeVersionWinMD
	{
		get
		{
			string text = runtimeVersionWinMD;
			if (text != null)
			{
				return text;
			}
			return runtimeVersionWinMD = CalculateRuntimeVersionWinMD(RuntimeVersion);
		}
	}

	public string WinMDVersion
	{
		get
		{
			string text = winMDVersion;
			if (text != null)
			{
				return text;
			}
			return winMDVersion = CalculateWinMDVersion(RuntimeVersion);
		}
	}

	public bool IsClr10
	{
		get
		{
			string text = RuntimeVersion ?? string.Empty;
			if (!text.StartsWith("v1.0") && !text.StartsWith("v1.x86") && !(text == "retail"))
			{
				return text == "COMPLUS";
			}
			return true;
		}
	}

	public bool IsClr10Exactly
	{
		get
		{
			if (!(RuntimeVersion == "v1.0.3705") && !(RuntimeVersion == "v1.x86ret") && !(RuntimeVersion == "retail"))
			{
				return RuntimeVersion == "COMPLUS";
			}
			return true;
		}
	}

	public bool IsClr11 => (RuntimeVersion ?? string.Empty).StartsWith("v1.1");

	public bool IsClr11Exactly => RuntimeVersion == "v1.1.4322";

	public bool IsClr1x
	{
		get
		{
			if (!IsClr10)
			{
				return IsClr11;
			}
			return true;
		}
	}

	public bool IsClr1xExactly
	{
		get
		{
			if (!IsClr10Exactly)
			{
				return IsClr11Exactly;
			}
			return true;
		}
	}

	public bool IsClr20 => (RuntimeVersion ?? string.Empty).StartsWith("v2.0");

	public bool IsClr20Exactly => RuntimeVersion == "v2.0.50727";

	public bool IsClr40 => (RuntimeVersion ?? string.Empty).StartsWith("v4.0");

	public bool IsClr40Exactly => RuntimeVersion == "v4.0.30319";

	public bool IsEcma2002 => RuntimeVersion == "Standard CLI 2002";

	public bool IsEcma2005 => RuntimeVersion == "Standard CLI 2005";

	public Machine Machine { get; set; }

	public bool IsI386 => Machine.IsI386();

	public bool IsIA64 => Machine == Machine.IA64;

	public bool IsAMD64 => Machine.IsAMD64();

	public bool IsARM => Machine.IsARMNT();

	public bool IsARM64 => Machine.IsARM64();

	public bool IsS390x => Machine.IsS390x();

	public ComImageFlags Cor20HeaderFlags
	{
		get
		{
			return (ComImageFlags)cor20HeaderFlags;
		}
		set
		{
			cor20HeaderFlags = (int)value;
		}
	}

	public uint? Cor20HeaderRuntimeVersion { get; set; }

	public ushort? TablesHeaderVersion { get; set; }

	public bool IsILOnly
	{
		get
		{
			return (cor20HeaderFlags & 1) != 0;
		}
		set
		{
			ModifyComImageFlags(value, ComImageFlags.ILOnly);
		}
	}

	public bool Is32BitRequired
	{
		get
		{
			return (cor20HeaderFlags & 2) != 0;
		}
		set
		{
			ModifyComImageFlags(value, ComImageFlags.Bit32Required);
		}
	}

	public bool IsStrongNameSigned
	{
		get
		{
			return (cor20HeaderFlags & 8) != 0;
		}
		set
		{
			ModifyComImageFlags(value, ComImageFlags.StrongNameSigned);
		}
	}

	public bool HasNativeEntryPoint
	{
		get
		{
			return (cor20HeaderFlags & 0x10) != 0;
		}
		set
		{
			ModifyComImageFlags(value, ComImageFlags.NativeEntryPoint);
		}
	}

	public bool Is32BitPreferred
	{
		get
		{
			return (cor20HeaderFlags & 0x20000) != 0;
		}
		set
		{
			ModifyComImageFlags(value, ComImageFlags.Bit32Preferred);
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

	protected virtual void InitializeTypes()
	{
		Interlocked.CompareExchange(ref types, new LazyList<TypeDef>(this), null);
	}

	protected virtual void InitializeExportedTypes()
	{
		Interlocked.CompareExchange(ref exportedTypes, new List<ExportedType>(), null);
	}

	private void InitializeNativeAndManagedEntryPoint()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!nativeAndManagedEntryPoint_initialized)
			{
				nativeEntryPoint = GetNativeEntryPoint_NoLock();
				managedEntryPoint = GetManagedEntryPoint_NoLock();
				nativeAndManagedEntryPoint_initialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual RVA GetNativeEntryPoint_NoLock()
	{
		return (RVA)0u;
	}

	protected virtual IManagedEntryPoint GetManagedEntryPoint_NoLock()
	{
		return null;
	}

	protected virtual void InitializeResources()
	{
		Interlocked.CompareExchange(ref resources, new ResourceCollection(), null);
	}

	private void InitializeVTableFixups()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!vtableFixups_isInitialized)
			{
				vtableFixups = GetVTableFixups_NoLock();
				vtableFixups_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual VTableFixups GetVTableFixups_NoLock()
	{
		return null;
	}

	private void InitializeWin32Resources()
	{
		theLock.EnterWriteLock();
		try
		{
			if (!win32Resources_isInitialized)
			{
				win32Resources = GetWin32Resources_NoLock();
				win32Resources_isInitialized = true;
			}
		}
		finally
		{
			theLock.ExitWriteLock();
		}
	}

	protected virtual Win32Resources GetWin32Resources_NoLock()
	{
		return null;
	}

	private static WinMDStatus CalculateWinMDStatus(string version)
	{
		if (version == null)
		{
			return WinMDStatus.None;
		}
		if (!version.StartsWith("WindowsRuntime ", StringComparison.Ordinal))
		{
			return WinMDStatus.None;
		}
		if (version.IndexOf(';') >= 0)
		{
			return WinMDStatus.Managed;
		}
		return WinMDStatus.Pure;
	}

	private static string CalculateRuntimeVersionWinMD(string version)
	{
		if (version == null)
		{
			return null;
		}
		if (!version.StartsWith("WindowsRuntime ", StringComparison.Ordinal))
		{
			return null;
		}
		int num = version.IndexOf(';');
		if (num < 0)
		{
			return null;
		}
		string text = version.Substring(num + 1);
		if (text.StartsWith("CLR", StringComparison.OrdinalIgnoreCase))
		{
			text = text.Substring(3);
		}
		return text.TrimStart(new char[1] { ' ' });
	}

	private static string CalculateWinMDVersion(string version)
	{
		if (version == null)
		{
			return null;
		}
		if (!version.StartsWith("WindowsRuntime ", StringComparison.Ordinal))
		{
			return null;
		}
		int num = version.IndexOf(';');
		if (num < 0)
		{
			return version;
		}
		return version.Substring(0, num);
	}

	private void ModifyComImageFlags(bool set, ComImageFlags flags)
	{
		int num;
		int value;
		do
		{
			num = cor20HeaderFlags;
			value = ((!set) ? (num & (int)(~flags)) : (num | (int)flags));
		}
		while (Interlocked.CompareExchange(ref cor20HeaderFlags, value, num) != num);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			TypeDefFinder typeDefFinder = this.typeDefFinder;
			if (typeDefFinder != null)
			{
				typeDefFinder.Dispose();
				this.typeDefFinder = null;
			}
			pdbState?.Dispose();
			pdbState = null;
		}
	}

	public IEnumerable<TypeDef> GetTypes()
	{
		return AllTypesHelper.Types(Types);
	}

	public void AddAsNonNestedType(TypeDef typeDef)
	{
		if (typeDef != null)
		{
			typeDef.DeclaringType = null;
			Types.Add(typeDef);
		}
	}

	public T UpdateRowId<T>(T tableRow) where T : IMDTokenProvider
	{
		if (tableRow != null && tableRow.Rid == 0)
		{
			ref T reference = ref tableRow;
			T val = default(T);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			reference.Rid = GetNextFreeRid(tableRow.MDToken.Table);
		}
		return tableRow;
	}

	public T ForceUpdateRowId<T>(T tableRow) where T : IMDTokenProvider
	{
		if (tableRow != null)
		{
			ref T reference = ref tableRow;
			T val = default(T);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			reference.Rid = GetNextFreeRid(tableRow.MDToken.Table);
		}
		return tableRow;
	}

	private uint GetNextFreeRid(Table table)
	{
		int[] array = lastUsedRids;
		if ((long)table >= (long)array.Length)
		{
			return 0u;
		}
		return (uint)Interlocked.Increment(ref array[(uint)table]) & 0xFFFFFFu;
	}

	public ITypeDefOrRef Import(Type type)
	{
		return new Importer(this).Import(type);
	}

	public TypeSig ImportAsTypeSig(Type type)
	{
		return new Importer(this).ImportAsTypeSig(type);
	}

	public MemberRef Import(FieldInfo fieldInfo)
	{
		return (MemberRef)new Importer(this).Import(fieldInfo);
	}

	public IMethod Import(MethodBase methodBase)
	{
		return new Importer(this).Import(methodBase);
	}

	public IType Import(IType type)
	{
		return new Importer(this).Import(type);
	}

	public TypeRef Import(TypeDef type)
	{
		return (TypeRef)new Importer(this).Import(type);
	}

	public TypeRef Import(TypeRef type)
	{
		return (TypeRef)new Importer(this).Import(type);
	}

	public TypeSpec Import(TypeSpec type)
	{
		return new Importer(this).Import(type);
	}

	public TypeSig Import(TypeSig type)
	{
		return new Importer(this).Import(type);
	}

	public MemberRef Import(IField field)
	{
		return (MemberRef)new Importer(this).Import(field);
	}

	public MemberRef Import(FieldDef field)
	{
		return (MemberRef)new Importer(this).Import(field);
	}

	public IMethod Import(IMethod method)
	{
		return new Importer(this).Import(method);
	}

	public MemberRef Import(MethodDef method)
	{
		return (MemberRef)new Importer(this).Import(method);
	}

	public MethodSpec Import(MethodSpec method)
	{
		return new Importer(this).Import(method);
	}

	public MemberRef Import(MemberRef memberRef)
	{
		return new Importer(this).Import(memberRef);
	}

	public void Write(string filename)
	{
		Write(filename, null);
	}

	public void Write(string filename, ModuleWriterOptions options)
	{
		new ModuleWriter(this, options ?? new ModuleWriterOptions(this)).Write(filename);
	}

	public void Write(Stream dest)
	{
		Write(dest, null);
	}

	public void Write(Stream dest, ModuleWriterOptions options)
	{
		new ModuleWriter(this, options ?? new ModuleWriterOptions(this)).Write(dest);
	}

	public void ResetTypeDefFindCache()
	{
		TypeDefFinder.ResetCache();
	}

	public ResourceData FindWin32ResourceData(ResourceName type, ResourceName name, ResourceName langId)
	{
		return Win32Resources?.Find(type, name, langId);
	}

	public void CreatePdbState(PdbFileKind pdbFileKind)
	{
		SetPdbState(new PdbState(this, pdbFileKind));
	}

	public void SetPdbState(PdbState pdbState)
	{
		if (pdbState == null)
		{
			throw new ArgumentNullException("pdbState");
		}
		if (Interlocked.CompareExchange(ref this.pdbState, pdbState, null) != null)
		{
			throw new InvalidOperationException("PDB file has already been initialized");
		}
	}

	private uint GetCor20RuntimeVersion()
	{
		uint? cor20HeaderRuntimeVersion = Cor20HeaderRuntimeVersion;
		if (cor20HeaderRuntimeVersion.HasValue)
		{
			return cor20HeaderRuntimeVersion.Value;
		}
		if (!IsClr1x)
		{
			return 131077u;
		}
		return 131072u;
	}

	public int GetPointerSize()
	{
		return GetPointerSize(4);
	}

	public int GetPointerSize(int defaultPointerSize)
	{
		return GetPointerSize(defaultPointerSize, defaultPointerSize);
	}

	public int GetPointerSize(int defaultPointerSize, int prefer32bitPointerSize)
	{
		Machine machine = Machine;
		if (machine.Is64Bit())
		{
			return 8;
		}
		if (!machine.IsI386())
		{
			return 4;
		}
		if (GetCor20RuntimeVersion() < 131077)
		{
			return 4;
		}
		ComImageFlags comImageFlags = (ComImageFlags)cor20HeaderFlags;
		if ((comImageFlags & ComImageFlags.ILOnly) == 0)
		{
			return 4;
		}
		return (comImageFlags & (ComImageFlags.Bit32Required | ComImageFlags.Bit32Preferred)) switch
		{
			ComImageFlags.Bit32Required => 4, 
			ComImageFlags.Bit32Required | ComImageFlags.Bit32Preferred => prefer32bitPointerSize, 
			_ => defaultPointerSize, 
		};
	}

	void IListListener<TypeDef>.OnLazyAdd(int index, ref TypeDef value)
	{
		value.Module2 = this;
	}

	void IListListener<TypeDef>.OnAdd(int index, TypeDef value)
	{
		if (value.DeclaringType != null)
		{
			throw new InvalidOperationException("Nested type is already owned by another type. Set DeclaringType to null first.");
		}
		if (value.Module != null)
		{
			throw new InvalidOperationException("Type is already owned by another module. Remove it from that module's type list.");
		}
		value.Module2 = this;
	}

	void IListListener<TypeDef>.OnRemove(int index, TypeDef value)
	{
		value.Module2 = null;
	}

	void IListListener<TypeDef>.OnResize(int index)
	{
	}

	void IListListener<TypeDef>.OnClear()
	{
		foreach (TypeDef item in types.GetEnumerable_NoLock())
		{
			item.Module2 = null;
		}
	}

	public TypeDef Find(string fullName, bool isReflectionName)
	{
		return TypeDefFinder.Find(fullName, isReflectionName);
	}

	public TypeDef Find(TypeRef typeRef)
	{
		return TypeDefFinder.Find(typeRef);
	}

	public TypeDef Find(ITypeDefOrRef typeRef)
	{
		if (typeRef is TypeDef typeDef)
		{
			if (typeDef.Module != this)
			{
				return null;
			}
			return typeDef;
		}
		if (typeRef is TypeRef typeRef2)
		{
			return Find(typeRef2);
		}
		if (!(typeRef is TypeSpec typeSpec))
		{
			return null;
		}
		if (!(typeSpec.TypeSig is TypeDefOrRefSig { TypeDef: var typeDef2 } typeDefOrRefSig))
		{
			return null;
		}
		if (typeDef2 != null)
		{
			if (typeDef2.Module != this)
			{
				return null;
			}
			return typeDef2;
		}
		TypeRef typeRef3 = typeDefOrRefSig.TypeRef;
		if (typeRef3 != null)
		{
			return Find(typeRef3);
		}
		return null;
	}

	public static ModuleContext CreateModuleContext()
	{
		ModuleContext moduleContext = new ModuleContext();
		AssemblyResolver assemblyResolver = new AssemblyResolver(moduleContext);
		Resolver resolver = new Resolver(assemblyResolver);
		moduleContext.AssemblyResolver = assemblyResolver;
		moduleContext.Resolver = resolver;
		assemblyResolver.DefaultModuleContext = moduleContext;
		return moduleContext;
	}

	public virtual void LoadEverything(ICancellationToken cancellationToken = null)
	{
		ModuleLoader.LoadAll(this, cancellationToken);
	}

	public override string ToString()
	{
		return FullName;
	}

	public IMDTokenProvider ResolveToken(MDToken mdToken)
	{
		return ResolveToken(mdToken.Raw, default(GenericParamContext));
	}

	public IMDTokenProvider ResolveToken(MDToken mdToken, GenericParamContext gpContext)
	{
		return ResolveToken(mdToken.Raw, gpContext);
	}

	public IMDTokenProvider ResolveToken(int token)
	{
		return ResolveToken((uint)token, default(GenericParamContext));
	}

	public IMDTokenProvider ResolveToken(int token, GenericParamContext gpContext)
	{
		return ResolveToken((uint)token, gpContext);
	}

	public IMDTokenProvider ResolveToken(uint token)
	{
		return ResolveToken(token, default(GenericParamContext));
	}

	public virtual IMDTokenProvider ResolveToken(uint token, GenericParamContext gpContext)
	{
		return null;
	}

	public IEnumerable<AssemblyRef> GetAssemblyRefs()
	{
		for (uint rid = 1u; ResolveToken(new MDToken(Table.AssemblyRef, rid).Raw) is AssemblyRef assemblyRef; rid++)
		{
			yield return assemblyRef;
		}
	}

	public IEnumerable<ModuleRef> GetModuleRefs()
	{
		for (uint rid = 1u; ResolveToken(new MDToken(Table.ModuleRef, rid).Raw) is ModuleRef moduleRef; rid++)
		{
			yield return moduleRef;
		}
	}

	public IEnumerable<MemberRef> GetMemberRefs()
	{
		return GetMemberRefs(default(GenericParamContext));
	}

	public IEnumerable<MemberRef> GetMemberRefs(GenericParamContext gpContext)
	{
		for (uint rid = 1u; ResolveToken(new MDToken(Table.MemberRef, rid).Raw, gpContext) is MemberRef memberRef; rid++)
		{
			yield return memberRef;
		}
	}

	public IEnumerable<TypeRef> GetTypeRefs()
	{
		for (uint rid = 1u; ResolveToken(new MDToken(Table.TypeRef, rid).Raw) is TypeRef typeRef; rid++)
		{
			yield return typeRef;
		}
	}

	public AssemblyRef GetAssemblyRef(UTF8String simpleName)
	{
		AssemblyRef assemblyRef = null;
		foreach (AssemblyRef assemblyRef2 in GetAssemblyRefs())
		{
			if (!(assemblyRef2.Name != simpleName) && IsGreaterAssemblyRefVersion(assemblyRef, assemblyRef2))
			{
				assemblyRef = assemblyRef2;
			}
		}
		return assemblyRef;
	}

	protected static bool IsGreaterAssemblyRefVersion(AssemblyRef found, AssemblyRef newOne)
	{
		if (found == null)
		{
			return true;
		}
		Version version = found.Version;
		Version version2 = newOne.Version;
		if ((object)version != null)
		{
			if ((object)version2 != null)
			{
				return version2 >= version;
			}
			return false;
		}
		return true;
	}

	ITypeDefOrRef ISignatureReaderHelper.ResolveTypeDefOrRef(uint codedToken, GenericParamContext gpContext)
	{
		if (!CodedToken.TypeDefOrRef.Decode(codedToken, out uint token))
		{
			return null;
		}
		return ResolveToken(token) as ITypeDefOrRef;
	}

	TypeSig ISignatureReaderHelper.ConvertRTInternalAddress(IntPtr address)
	{
		return null;
	}
}
