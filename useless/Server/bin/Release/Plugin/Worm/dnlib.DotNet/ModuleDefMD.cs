using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.DotNet.Pdb.Symbols;
using dnlib.DotNet.Writer;
using dnlib.IO;
using dnlib.PE;
using dnlib.Utils;
using dnlib.W32Resources;

namespace dnlib.DotNet;

[ComVisible(true)]
public sealed class ModuleDefMD : ModuleDefMD2, IInstructionOperandResolver, ITokenResolver, IStringResolver
{
	private MetadataBase metadata;

	private IMethodDecrypter methodDecrypter;

	private IStringDecrypter stringDecrypter;

	private StrongBox<RidList> moduleRidList;

	private SimpleLazyList<ModuleDefMD2> listModuleDefMD;

	private SimpleLazyList<TypeRefMD> listTypeRefMD;

	private SimpleLazyList<TypeDefMD> listTypeDefMD;

	private SimpleLazyList<FieldDefMD> listFieldDefMD;

	private SimpleLazyList<MethodDefMD> listMethodDefMD;

	private SimpleLazyList<ParamDefMD> listParamDefMD;

	private SimpleLazyList2<InterfaceImplMD> listInterfaceImplMD;

	private SimpleLazyList2<MemberRefMD> listMemberRefMD;

	private SimpleLazyList<ConstantMD> listConstantMD;

	private SimpleLazyList<DeclSecurityMD> listDeclSecurityMD;

	private SimpleLazyList<ClassLayoutMD> listClassLayoutMD;

	private SimpleLazyList2<StandAloneSigMD> listStandAloneSigMD;

	private SimpleLazyList<EventDefMD> listEventDefMD;

	private SimpleLazyList<PropertyDefMD> listPropertyDefMD;

	private SimpleLazyList<ModuleRefMD> listModuleRefMD;

	private SimpleLazyList2<TypeSpecMD> listTypeSpecMD;

	private SimpleLazyList<ImplMapMD> listImplMapMD;

	private SimpleLazyList<AssemblyDefMD> listAssemblyDefMD;

	private SimpleLazyList<AssemblyRefMD> listAssemblyRefMD;

	private SimpleLazyList<FileDefMD> listFileDefMD;

	private SimpleLazyList<ExportedTypeMD> listExportedTypeMD;

	private SimpleLazyList<ManifestResourceMD> listManifestResourceMD;

	private SimpleLazyList<GenericParamMD> listGenericParamMD;

	private SimpleLazyList2<MethodSpecMD> listMethodSpecMD;

	private SimpleLazyList2<GenericParamConstraintMD> listGenericParamConstraintMD;

	private static readonly Dictionary<string, int> preferredCorLibs = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
	{
		{ "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 100 },
		{ "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 90 },
		{ "mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 60 },
		{ "mscorlib, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", 50 },
		{ "mscorlib, Version=5.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", 80 },
		{ "mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", 70 },
		{ "mscorlib, Version=3.5.0.0, Culture=neutral, PublicKeyToken=e92a8b81eba7ceb7", 60 },
		{ "mscorlib, Version=3.5.0.0, Culture=neutral, PublicKeyToken=969db8053d3322ac", 60 },
		{ "mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=969db8053d3322ac", 50 }
	};

	private static readonly string[] corlibs = new string[4] { "System.Private.CoreLib", "System.Runtime", "netstandard", "mscorlib" };

	private static readonly UTF8String systemRuntimeName = new UTF8String("System.Runtime");

	private static readonly UTF8String corefxName = new UTF8String("corefx");

	private static readonly PublicKeyToken contractsPublicKeyToken = new PublicKeyToken("b03f5f7f11d50a3a");

	private MethodExportInfoProvider methodExportInfoProvider;

	public IMethodDecrypter MethodDecrypter
	{
		get
		{
			return methodDecrypter;
		}
		set
		{
			methodDecrypter = value;
		}
	}

	public IStringDecrypter StringDecrypter
	{
		get
		{
			return stringDecrypter;
		}
		set
		{
			stringDecrypter = value;
		}
	}

	public dnlib.DotNet.MD.Metadata Metadata => metadata;

	public TablesStream TablesStream => metadata.TablesStream;

	public StringsStream StringsStream => metadata.StringsStream;

	public BlobStream BlobStream => metadata.BlobStream;

	public GuidStream GuidStream => metadata.GuidStream;

	public USStream USStream => metadata.USStream;

	protected override void InitializeTypes()
	{
		RidList nonNestedClassRidList = Metadata.GetNonNestedClassRidList();
		LazyList<TypeDef, RidList> value = new LazyList<TypeDef, RidList>(nonNestedClassRidList.Count, this, nonNestedClassRidList, (RidList list2, int index) => ResolveTypeDef(list2[index]));
		Interlocked.CompareExchange(ref types, value, null);
	}

	protected override void InitializeExportedTypes()
	{
		RidList exportedTypeRidList = Metadata.GetExportedTypeRidList();
		LazyList<ExportedType, RidList> value = new LazyList<ExportedType, RidList>(exportedTypeRidList.Count, exportedTypeRidList, (RidList list2, int i) => ResolveExportedType(list2[i]));
		Interlocked.CompareExchange(ref exportedTypes, value, null);
	}

	protected override void InitializeResources()
	{
		ResourceCollection value = new ResourceCollection((int)TablesStream.ManifestResourceTable.Rows, null, (object ctx, int i) => CreateResource((uint)(i + 1)));
		Interlocked.CompareExchange(ref resources, value, null);
	}

	protected override Win32Resources GetWin32Resources_NoLock()
	{
		return metadata.PEImage.Win32Resources;
	}

	protected override VTableFixups GetVTableFixups_NoLock()
	{
		ImageDataDirectory vTableFixups = metadata.ImageCor20Header.VTableFixups;
		if (vTableFixups.VirtualAddress == (RVA)0u || vTableFixups.Size == 0)
		{
			return null;
		}
		return new VTableFixups(this);
	}

	public static ModuleDefMD Load(string fileName, ModuleContext context)
	{
		return Load(fileName, new ModuleCreationOptions(context));
	}

	public static ModuleDefMD Load(string fileName, ModuleCreationOptions options = null)
	{
		return Load(MetadataFactory.Load(fileName, options?.Runtime ?? CLRRuntimeReaderKind.CLR), options);
	}

	public static ModuleDefMD Load(byte[] data, ModuleContext context)
	{
		return Load(data, new ModuleCreationOptions(context));
	}

	public static ModuleDefMD Load(byte[] data, ModuleCreationOptions options = null)
	{
		return Load(MetadataFactory.Load(data, options?.Runtime ?? CLRRuntimeReaderKind.CLR), options);
	}

	public static ModuleDefMD Load(Module mod)
	{
		return Load(mod, (ModuleCreationOptions)null, GetImageLayout(mod));
	}

	public static ModuleDefMD Load(Module mod, ModuleContext context)
	{
		return Load(mod, new ModuleCreationOptions(context), GetImageLayout(mod));
	}

	public static ModuleDefMD Load(Module mod, ModuleCreationOptions options)
	{
		return Load(mod, options, GetImageLayout(mod));
	}

	private static ImageLayout GetImageLayout(Module mod)
	{
		string fullyQualifiedName = mod.FullyQualifiedName;
		if (fullyQualifiedName.Length > 0 && fullyQualifiedName[0] == '<' && fullyQualifiedName[fullyQualifiedName.Length - 1] == '>')
		{
			return ImageLayout.File;
		}
		return ImageLayout.Memory;
	}

	public static ModuleDefMD Load(Module mod, ModuleContext context, ImageLayout imageLayout)
	{
		return Load(mod, new ModuleCreationOptions(context), imageLayout);
	}

	private static IntPtr GetModuleHandle(Module mod)
	{
		return Marshal.GetHINSTANCE(mod);
	}

	public static ModuleDefMD Load(Module mod, ModuleCreationOptions options, ImageLayout imageLayout)
	{
		IntPtr moduleHandle = GetModuleHandle(mod);
		if (moduleHandle != IntPtr.Zero && moduleHandle != new IntPtr(-1))
		{
			return Load(moduleHandle, options, imageLayout);
		}
		string fullyQualifiedName = mod.FullyQualifiedName;
		if (string.IsNullOrEmpty(fullyQualifiedName) || fullyQualifiedName[0] == '<')
		{
			throw new InvalidOperationException($"Module {mod} has no HINSTANCE");
		}
		return Load(fullyQualifiedName, options);
	}

	public static ModuleDefMD Load(IntPtr addr)
	{
		return Load(MetadataFactory.Load(addr, CLRRuntimeReaderKind.CLR), null);
	}

	public static ModuleDefMD Load(IntPtr addr, ModuleContext context)
	{
		return Load(MetadataFactory.Load(addr, CLRRuntimeReaderKind.CLR), new ModuleCreationOptions(context));
	}

	public static ModuleDefMD Load(IntPtr addr, ModuleCreationOptions options)
	{
		return Load(MetadataFactory.Load(addr, options?.Runtime ?? CLRRuntimeReaderKind.CLR), options);
	}

	public static ModuleDefMD Load(IPEImage peImage)
	{
		return Load(MetadataFactory.Load(peImage, CLRRuntimeReaderKind.CLR), null);
	}

	public static ModuleDefMD Load(IPEImage peImage, ModuleContext context)
	{
		return Load(MetadataFactory.Load(peImage, CLRRuntimeReaderKind.CLR), new ModuleCreationOptions(context));
	}

	public static ModuleDefMD Load(IPEImage peImage, ModuleCreationOptions options)
	{
		return Load(MetadataFactory.Load(peImage, options?.Runtime ?? CLRRuntimeReaderKind.CLR), options);
	}

	public static ModuleDefMD Load(IntPtr addr, ModuleContext context, ImageLayout imageLayout)
	{
		return Load(MetadataFactory.Load(addr, imageLayout, CLRRuntimeReaderKind.CLR), new ModuleCreationOptions(context));
	}

	public static ModuleDefMD Load(IntPtr addr, ModuleCreationOptions options, ImageLayout imageLayout)
	{
		return Load(MetadataFactory.Load(addr, imageLayout, options?.Runtime ?? CLRRuntimeReaderKind.CLR), options);
	}

	public static ModuleDefMD Load(Stream stream)
	{
		return Load(stream, (ModuleCreationOptions)null);
	}

	public static ModuleDefMD Load(Stream stream, ModuleContext context)
	{
		return Load(stream, new ModuleCreationOptions(context));
	}

	public static ModuleDefMD Load(Stream stream, ModuleCreationOptions options)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (stream.Length > int.MaxValue)
		{
			throw new ArgumentException("Stream is too big");
		}
		byte[] array = new byte[(int)stream.Length];
		stream.Position = 0L;
		if (stream.Read(array, 0, array.Length) != array.Length)
		{
			throw new IOException("Could not read all bytes from the stream");
		}
		return Load(array, options);
	}

	internal static ModuleDefMD Load(MetadataBase metadata, ModuleCreationOptions options)
	{
		return new ModuleDefMD(metadata, options);
	}

	private ModuleDefMD(MetadataBase metadata, ModuleCreationOptions options)
		: base(null, 1u)
	{
		if (options == null)
		{
			options = ModuleCreationOptions.Default;
		}
		this.metadata = metadata;
		context = options.Context;
		Initialize();
		InitializeFromRawRow();
		location = metadata.PEImage.Filename ?? string.Empty;
		base.Kind = GetKind();
		base.Characteristics = Metadata.PEImage.ImageNTHeaders.FileHeader.Characteristics;
		base.DllCharacteristics = Metadata.PEImage.ImageNTHeaders.OptionalHeader.DllCharacteristics;
		base.RuntimeVersion = Metadata.VersionString;
		base.Machine = Metadata.PEImage.ImageNTHeaders.FileHeader.Machine;
		base.Cor20HeaderFlags = Metadata.ImageCor20Header.Flags;
		base.Cor20HeaderRuntimeVersion = (uint)((Metadata.ImageCor20Header.MajorRuntimeVersion << 16) | Metadata.ImageCor20Header.MinorRuntimeVersion);
		base.TablesHeaderVersion = Metadata.TablesStream.Version;
		corLibTypes = new CorLibTypes(this, options.CorLibAssemblyRef ?? FindCorLibAssemblyRef() ?? CreateDefaultCorLibAssemblyRef());
		InitializePdb(options);
	}

	private void InitializePdb(ModuleCreationOptions options)
	{
		if (options != null)
		{
			LoadPdb(CreateSymbolReader(options));
		}
	}

	private SymbolReader CreateSymbolReader(ModuleCreationOptions options)
	{
		if (options.PdbFileOrData != null)
		{
			string text = options.PdbFileOrData as string;
			if (!string.IsNullOrEmpty(text))
			{
				SymbolReader symbolReader = SymbolReaderFactory.Create(options.PdbOptions, metadata, text);
				if (symbolReader != null)
				{
					return symbolReader;
				}
			}
			if (options.PdbFileOrData is byte[] pdbData)
			{
				return SymbolReaderFactory.Create(options.PdbOptions, metadata, pdbData);
			}
			if (options.PdbFileOrData is DataReaderFactory pdbStream)
			{
				return SymbolReaderFactory.Create(options.PdbOptions, metadata, pdbStream);
			}
		}
		if (options.TryToLoadPdbFromDisk)
		{
			return SymbolReaderFactory.CreateFromAssemblyFile(options.PdbOptions, metadata, location ?? string.Empty);
		}
		return null;
	}

	public void LoadPdb(SymbolReader symbolReader)
	{
		if (symbolReader != null)
		{
			if (pdbState != null)
			{
				throw new InvalidOperationException("PDB file has already been initialized");
			}
			if (Interlocked.CompareExchange(ref pdbState, new PdbState(symbolReader, this), null) != null)
			{
				throw new InvalidOperationException("PDB file has already been initialized");
			}
		}
	}

	public void LoadPdb(string pdbFileName)
	{
		LoadPdb(PdbReaderOptions.None, pdbFileName);
	}

	public void LoadPdb(PdbReaderOptions options, string pdbFileName)
	{
		LoadPdb(SymbolReaderFactory.Create(options, metadata, pdbFileName));
	}

	public void LoadPdb(byte[] pdbData)
	{
		LoadPdb(PdbReaderOptions.None, pdbData);
	}

	public void LoadPdb(PdbReaderOptions options, byte[] pdbData)
	{
		LoadPdb(SymbolReaderFactory.Create(options, metadata, pdbData));
	}

	public void LoadPdb(DataReaderFactory pdbStream)
	{
		LoadPdb(PdbReaderOptions.None, pdbStream);
	}

	public void LoadPdb(PdbReaderOptions options, DataReaderFactory pdbStream)
	{
		LoadPdb(SymbolReaderFactory.Create(options, metadata, pdbStream));
	}

	public void LoadPdb()
	{
		LoadPdb(PdbReaderOptions.None);
	}

	public void LoadPdb(PdbReaderOptions options)
	{
		LoadPdb(SymbolReaderFactory.CreateFromAssemblyFile(options, metadata, location ?? string.Empty));
	}

	internal void InitializeCustomDebugInfos(MDToken token, GenericParamContext gpContext, IList<PdbCustomDebugInfo> result)
	{
		pdbState?.InitializeCustomDebugInfos(token, gpContext, result);
	}

	private ModuleKind GetKind()
	{
		if (TablesStream.AssemblyTable.Rows < 1)
		{
			return ModuleKind.NetModule;
		}
		IPEImage pEImage = Metadata.PEImage;
		if ((pEImage.ImageNTHeaders.FileHeader.Characteristics & Characteristics.Dll) != 0)
		{
			return ModuleKind.Dll;
		}
		if (pEImage.ImageNTHeaders.OptionalHeader.Subsystem == Subsystem.WindowsCui)
		{
			return ModuleKind.Console;
		}
		return ModuleKind.Windows;
	}

	private void Initialize()
	{
		TablesStream tablesStream = metadata.TablesStream;
		listModuleDefMD = new SimpleLazyList<ModuleDefMD2>(tablesStream.ModuleTable.Rows, (uint rid2) => (rid2 != 1) ? new ModuleDefMD2(this, rid2) : this);
		listTypeRefMD = new SimpleLazyList<TypeRefMD>(tablesStream.TypeRefTable.Rows, (uint rid2) => new TypeRefMD(this, rid2));
		listTypeDefMD = new SimpleLazyList<TypeDefMD>(tablesStream.TypeDefTable.Rows, (uint rid2) => new TypeDefMD(this, rid2));
		listFieldDefMD = new SimpleLazyList<FieldDefMD>(tablesStream.FieldTable.Rows, (uint rid2) => new FieldDefMD(this, rid2));
		listMethodDefMD = new SimpleLazyList<MethodDefMD>(tablesStream.MethodTable.Rows, (uint rid2) => new MethodDefMD(this, rid2));
		listParamDefMD = new SimpleLazyList<ParamDefMD>(tablesStream.ParamTable.Rows, (uint rid2) => new ParamDefMD(this, rid2));
		listInterfaceImplMD = new SimpleLazyList2<InterfaceImplMD>(tablesStream.InterfaceImplTable.Rows, (uint rid2, GenericParamContext gpContext) => new InterfaceImplMD(this, rid2, gpContext));
		listMemberRefMD = new SimpleLazyList2<MemberRefMD>(tablesStream.MemberRefTable.Rows, (uint rid2, GenericParamContext gpContext) => new MemberRefMD(this, rid2, gpContext));
		listConstantMD = new SimpleLazyList<ConstantMD>(tablesStream.ConstantTable.Rows, (uint rid2) => new ConstantMD(this, rid2));
		listDeclSecurityMD = new SimpleLazyList<DeclSecurityMD>(tablesStream.DeclSecurityTable.Rows, (uint rid2) => new DeclSecurityMD(this, rid2));
		listClassLayoutMD = new SimpleLazyList<ClassLayoutMD>(tablesStream.ClassLayoutTable.Rows, (uint rid2) => new ClassLayoutMD(this, rid2));
		listStandAloneSigMD = new SimpleLazyList2<StandAloneSigMD>(tablesStream.StandAloneSigTable.Rows, (uint rid2, GenericParamContext gpContext) => new StandAloneSigMD(this, rid2, gpContext));
		listEventDefMD = new SimpleLazyList<EventDefMD>(tablesStream.EventTable.Rows, (uint rid2) => new EventDefMD(this, rid2));
		listPropertyDefMD = new SimpleLazyList<PropertyDefMD>(tablesStream.PropertyTable.Rows, (uint rid2) => new PropertyDefMD(this, rid2));
		listModuleRefMD = new SimpleLazyList<ModuleRefMD>(tablesStream.ModuleRefTable.Rows, (uint rid2) => new ModuleRefMD(this, rid2));
		listTypeSpecMD = new SimpleLazyList2<TypeSpecMD>(tablesStream.TypeSpecTable.Rows, (uint rid2, GenericParamContext gpContext) => new TypeSpecMD(this, rid2, gpContext));
		listImplMapMD = new SimpleLazyList<ImplMapMD>(tablesStream.ImplMapTable.Rows, (uint rid2) => new ImplMapMD(this, rid2));
		listAssemblyDefMD = new SimpleLazyList<AssemblyDefMD>(tablesStream.AssemblyTable.Rows, (uint rid2) => new AssemblyDefMD(this, rid2));
		listFileDefMD = new SimpleLazyList<FileDefMD>(tablesStream.FileTable.Rows, (uint rid2) => new FileDefMD(this, rid2));
		listAssemblyRefMD = new SimpleLazyList<AssemblyRefMD>(tablesStream.AssemblyRefTable.Rows, (uint rid2) => new AssemblyRefMD(this, rid2));
		listExportedTypeMD = new SimpleLazyList<ExportedTypeMD>(tablesStream.ExportedTypeTable.Rows, (uint rid2) => new ExportedTypeMD(this, rid2));
		listManifestResourceMD = new SimpleLazyList<ManifestResourceMD>(tablesStream.ManifestResourceTable.Rows, (uint rid2) => new ManifestResourceMD(this, rid2));
		listGenericParamMD = new SimpleLazyList<GenericParamMD>(tablesStream.GenericParamTable.Rows, (uint rid2) => new GenericParamMD(this, rid2));
		listMethodSpecMD = new SimpleLazyList2<MethodSpecMD>(tablesStream.MethodSpecTable.Rows, (uint rid2, GenericParamContext gpContext) => new MethodSpecMD(this, rid2, gpContext));
		listGenericParamConstraintMD = new SimpleLazyList2<GenericParamConstraintMD>(tablesStream.GenericParamConstraintTable.Rows, (uint rid2, GenericParamContext gpContext) => new GenericParamConstraintMD(this, rid2, gpContext));
		for (int i = 0; i < 64; i++)
		{
			MDTable mDTable = TablesStream.Get((Table)i);
			lastUsedRids[i] = (int)(mDTable?.Rows ?? 0);
		}
	}

	private AssemblyRef FindCorLibAssemblyRef()
	{
		uint rows = TablesStream.AssemblyRefTable.Rows;
		AssemblyRef assemblyRef = null;
		int num = int.MinValue;
		for (uint num2 = 1u; num2 <= rows; num2++)
		{
			AssemblyRef assemblyRef2 = ResolveAssemblyRef(num2);
			if (preferredCorLibs.TryGetValue(assemblyRef2.FullName, out var value) && value > num)
			{
				num = value;
				assemblyRef = assemblyRef2;
			}
		}
		if (assemblyRef != null)
		{
			return assemblyRef;
		}
		string[] array = corlibs;
		foreach (string value2 in array)
		{
			for (uint num3 = 1u; num3 <= rows; num3++)
			{
				AssemblyRef assemblyRef3 = ResolveAssemblyRef(num3);
				if (UTF8String.ToSystemStringOrEmpty(assemblyRef3.Name).Equals(value2, StringComparison.OrdinalIgnoreCase) && ModuleDef.IsGreaterAssemblyRefVersion(assemblyRef, assemblyRef3))
				{
					assemblyRef = assemblyRef3;
				}
			}
			if (assemblyRef != null)
			{
				return assemblyRef;
			}
		}
		AssemblyDef assemblyDef = base.Assembly;
		if (assemblyDef != null && (assemblyDef.IsCorLib() || Find("System.Object", isReflectionName: false) != null))
		{
			base.IsCoreLibraryModule = true;
			return UpdateRowId(new AssemblyRefUser(assemblyDef));
		}
		return assemblyRef;
	}

	private AssemblyRef CreateDefaultCorLibAssemblyRef()
	{
		AssemblyRef alternativeCorLibReference = GetAlternativeCorLibReference();
		if (alternativeCorLibReference != null)
		{
			return UpdateRowId(alternativeCorLibReference);
		}
		if (base.IsClr40)
		{
			return UpdateRowId(AssemblyRefUser.CreateMscorlibReferenceCLR40());
		}
		if (base.IsClr20)
		{
			return UpdateRowId(AssemblyRefUser.CreateMscorlibReferenceCLR20());
		}
		if (base.IsClr11)
		{
			return UpdateRowId(AssemblyRefUser.CreateMscorlibReferenceCLR11());
		}
		if (base.IsClr10)
		{
			return UpdateRowId(AssemblyRefUser.CreateMscorlibReferenceCLR10());
		}
		return UpdateRowId(AssemblyRefUser.CreateMscorlibReferenceCLR40());
	}

	private AssemblyRef GetAlternativeCorLibReference()
	{
		foreach (AssemblyRef assemblyRef in GetAssemblyRefs())
		{
			if (IsAssemblyRef(assemblyRef, systemRuntimeName, contractsPublicKeyToken))
			{
				return assemblyRef;
			}
		}
		foreach (AssemblyRef assemblyRef2 in GetAssemblyRefs())
		{
			if (IsAssemblyRef(assemblyRef2, corefxName, contractsPublicKeyToken))
			{
				return assemblyRef2;
			}
		}
		return null;
	}

	private static bool IsAssemblyRef(AssemblyRef asmRef, UTF8String name, PublicKeyToken token)
	{
		if (asmRef.Name != name)
		{
			return false;
		}
		PublicKeyBase publicKeyOrToken = asmRef.PublicKeyOrToken;
		if (publicKeyOrToken == null)
		{
			return false;
		}
		return token.Equals(publicKeyOrToken.Token);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			metadata?.Dispose();
			metadata = null;
		}
	}

	public override IMDTokenProvider ResolveToken(uint token, GenericParamContext gpContext)
	{
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.Module => ResolveModule(num), 
			Table.TypeRef => ResolveTypeRef(num), 
			Table.TypeDef => ResolveTypeDef(num), 
			Table.Field => ResolveField(num), 
			Table.Method => ResolveMethod(num), 
			Table.Param => ResolveParam(num), 
			Table.InterfaceImpl => ResolveInterfaceImpl(num, gpContext), 
			Table.MemberRef => ResolveMemberRef(num, gpContext), 
			Table.Constant => ResolveConstant(num), 
			Table.DeclSecurity => ResolveDeclSecurity(num), 
			Table.ClassLayout => ResolveClassLayout(num), 
			Table.StandAloneSig => ResolveStandAloneSig(num, gpContext), 
			Table.Event => ResolveEvent(num), 
			Table.Property => ResolveProperty(num), 
			Table.ModuleRef => ResolveModuleRef(num), 
			Table.TypeSpec => ResolveTypeSpec(num, gpContext), 
			Table.ImplMap => ResolveImplMap(num), 
			Table.Assembly => ResolveAssembly(num), 
			Table.AssemblyRef => ResolveAssemblyRef(num), 
			Table.File => ResolveFile(num), 
			Table.ExportedType => ResolveExportedType(num), 
			Table.ManifestResource => ResolveManifestResource(num), 
			Table.GenericParam => ResolveGenericParam(num), 
			Table.MethodSpec => ResolveMethodSpec(num, gpContext), 
			Table.GenericParamConstraint => ResolveGenericParamConstraint(num, gpContext), 
			_ => null, 
		};
	}

	public ModuleDef ResolveModule(uint rid)
	{
		return listModuleDefMD[rid - 1];
	}

	public TypeRef ResolveTypeRef(uint rid)
	{
		return listTypeRefMD[rid - 1];
	}

	public TypeDef ResolveTypeDef(uint rid)
	{
		return listTypeDefMD[rid - 1];
	}

	public FieldDef ResolveField(uint rid)
	{
		return listFieldDefMD[rid - 1];
	}

	public MethodDef ResolveMethod(uint rid)
	{
		return listMethodDefMD[rid - 1];
	}

	public ParamDef ResolveParam(uint rid)
	{
		return listParamDefMD[rid - 1];
	}

	public InterfaceImpl ResolveInterfaceImpl(uint rid)
	{
		return listInterfaceImplMD[rid - 1, default(GenericParamContext)];
	}

	public InterfaceImpl ResolveInterfaceImpl(uint rid, GenericParamContext gpContext)
	{
		return listInterfaceImplMD[rid - 1, gpContext];
	}

	public MemberRef ResolveMemberRef(uint rid)
	{
		return listMemberRefMD[rid - 1, default(GenericParamContext)];
	}

	public MemberRef ResolveMemberRef(uint rid, GenericParamContext gpContext)
	{
		return listMemberRefMD[rid - 1, gpContext];
	}

	public Constant ResolveConstant(uint rid)
	{
		return listConstantMD[rid - 1];
	}

	public DeclSecurity ResolveDeclSecurity(uint rid)
	{
		return listDeclSecurityMD[rid - 1];
	}

	public ClassLayout ResolveClassLayout(uint rid)
	{
		return listClassLayoutMD[rid - 1];
	}

	public StandAloneSig ResolveStandAloneSig(uint rid)
	{
		return listStandAloneSigMD[rid - 1, default(GenericParamContext)];
	}

	public StandAloneSig ResolveStandAloneSig(uint rid, GenericParamContext gpContext)
	{
		return listStandAloneSigMD[rid - 1, gpContext];
	}

	public EventDef ResolveEvent(uint rid)
	{
		return listEventDefMD[rid - 1];
	}

	public PropertyDef ResolveProperty(uint rid)
	{
		return listPropertyDefMD[rid - 1];
	}

	public ModuleRef ResolveModuleRef(uint rid)
	{
		return listModuleRefMD[rid - 1];
	}

	public TypeSpec ResolveTypeSpec(uint rid)
	{
		return listTypeSpecMD[rid - 1, default(GenericParamContext)];
	}

	public TypeSpec ResolveTypeSpec(uint rid, GenericParamContext gpContext)
	{
		return listTypeSpecMD[rid - 1, gpContext];
	}

	public ImplMap ResolveImplMap(uint rid)
	{
		return listImplMapMD[rid - 1];
	}

	public AssemblyDef ResolveAssembly(uint rid)
	{
		return listAssemblyDefMD[rid - 1];
	}

	public AssemblyRef ResolveAssemblyRef(uint rid)
	{
		return listAssemblyRefMD[rid - 1];
	}

	public FileDef ResolveFile(uint rid)
	{
		return listFileDefMD[rid - 1];
	}

	public ExportedType ResolveExportedType(uint rid)
	{
		return listExportedTypeMD[rid - 1];
	}

	public ManifestResource ResolveManifestResource(uint rid)
	{
		return listManifestResourceMD[rid - 1];
	}

	public GenericParam ResolveGenericParam(uint rid)
	{
		return listGenericParamMD[rid - 1];
	}

	public MethodSpec ResolveMethodSpec(uint rid)
	{
		return listMethodSpecMD[rid - 1, default(GenericParamContext)];
	}

	public MethodSpec ResolveMethodSpec(uint rid, GenericParamContext gpContext)
	{
		return listMethodSpecMD[rid - 1, gpContext];
	}

	public GenericParamConstraint ResolveGenericParamConstraint(uint rid)
	{
		return listGenericParamConstraintMD[rid - 1, default(GenericParamContext)];
	}

	public GenericParamConstraint ResolveGenericParamConstraint(uint rid, GenericParamContext gpContext)
	{
		return listGenericParamConstraintMD[rid - 1, gpContext];
	}

	public ITypeDefOrRef ResolveTypeDefOrRef(uint codedToken)
	{
		return ResolveTypeDefOrRef(codedToken, default(GenericParamContext));
	}

	public ITypeDefOrRef ResolveTypeDefOrRef(uint codedToken, GenericParamContext gpContext)
	{
		if (!CodedToken.TypeDefOrRef.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.TypeDef => ResolveTypeDef(num), 
			Table.TypeRef => ResolveTypeRef(num), 
			Table.TypeSpec => ResolveTypeSpec(num, gpContext), 
			_ => null, 
		};
	}

	public IHasConstant ResolveHasConstant(uint codedToken)
	{
		if (!CodedToken.HasConstant.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.Field => ResolveField(num), 
			Table.Param => ResolveParam(num), 
			Table.Property => ResolveProperty(num), 
			_ => null, 
		};
	}

	public IHasCustomAttribute ResolveHasCustomAttribute(uint codedToken)
	{
		return ResolveHasCustomAttribute(codedToken, default(GenericParamContext));
	}

	public IHasCustomAttribute ResolveHasCustomAttribute(uint codedToken, GenericParamContext gpContext)
	{
		if (!CodedToken.HasCustomAttribute.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.Method => ResolveMethod(num), 
			Table.Field => ResolveField(num), 
			Table.TypeRef => ResolveTypeRef(num), 
			Table.TypeDef => ResolveTypeDef(num), 
			Table.Param => ResolveParam(num), 
			Table.InterfaceImpl => ResolveInterfaceImpl(num, gpContext), 
			Table.MemberRef => ResolveMemberRef(num, gpContext), 
			Table.Module => ResolveModule(num), 
			Table.DeclSecurity => ResolveDeclSecurity(num), 
			Table.Property => ResolveProperty(num), 
			Table.Event => ResolveEvent(num), 
			Table.StandAloneSig => ResolveStandAloneSig(num, gpContext), 
			Table.ModuleRef => ResolveModuleRef(num), 
			Table.TypeSpec => ResolveTypeSpec(num, gpContext), 
			Table.Assembly => ResolveAssembly(num), 
			Table.AssemblyRef => ResolveAssemblyRef(num), 
			Table.File => ResolveFile(num), 
			Table.ExportedType => ResolveExportedType(num), 
			Table.ManifestResource => ResolveManifestResource(num), 
			Table.GenericParam => ResolveGenericParam(num), 
			Table.MethodSpec => ResolveMethodSpec(num, gpContext), 
			Table.GenericParamConstraint => ResolveGenericParamConstraint(num, gpContext), 
			_ => null, 
		};
	}

	public IHasFieldMarshal ResolveHasFieldMarshal(uint codedToken)
	{
		if (!CodedToken.HasFieldMarshal.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.Field => ResolveField(num), 
			Table.Param => ResolveParam(num), 
			_ => null, 
		};
	}

	public IHasDeclSecurity ResolveHasDeclSecurity(uint codedToken)
	{
		if (!CodedToken.HasDeclSecurity.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.TypeDef => ResolveTypeDef(num), 
			Table.Method => ResolveMethod(num), 
			Table.Assembly => ResolveAssembly(num), 
			_ => null, 
		};
	}

	public IMemberRefParent ResolveMemberRefParent(uint codedToken)
	{
		return ResolveMemberRefParent(codedToken, default(GenericParamContext));
	}

	public IMemberRefParent ResolveMemberRefParent(uint codedToken, GenericParamContext gpContext)
	{
		if (!CodedToken.MemberRefParent.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.TypeDef => ResolveTypeDef(num), 
			Table.TypeRef => ResolveTypeRef(num), 
			Table.ModuleRef => ResolveModuleRef(num), 
			Table.Method => ResolveMethod(num), 
			Table.TypeSpec => ResolveTypeSpec(num, gpContext), 
			_ => null, 
		};
	}

	public IHasSemantic ResolveHasSemantic(uint codedToken)
	{
		if (!CodedToken.HasSemantic.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.Event => ResolveEvent(num), 
			Table.Property => ResolveProperty(num), 
			_ => null, 
		};
	}

	public IMethodDefOrRef ResolveMethodDefOrRef(uint codedToken)
	{
		return ResolveMethodDefOrRef(codedToken, default(GenericParamContext));
	}

	public IMethodDefOrRef ResolveMethodDefOrRef(uint codedToken, GenericParamContext gpContext)
	{
		if (!CodedToken.MethodDefOrRef.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.Method => ResolveMethod(num), 
			Table.MemberRef => ResolveMemberRef(num, gpContext), 
			_ => null, 
		};
	}

	public IMemberForwarded ResolveMemberForwarded(uint codedToken)
	{
		if (!CodedToken.MemberForwarded.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.Field => ResolveField(num), 
			Table.Method => ResolveMethod(num), 
			_ => null, 
		};
	}

	public IImplementation ResolveImplementation(uint codedToken)
	{
		if (!CodedToken.Implementation.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.File => ResolveFile(num), 
			Table.AssemblyRef => ResolveAssemblyRef(num), 
			Table.ExportedType => ResolveExportedType(num), 
			_ => null, 
		};
	}

	public ICustomAttributeType ResolveCustomAttributeType(uint codedToken)
	{
		return ResolveCustomAttributeType(codedToken, default(GenericParamContext));
	}

	public ICustomAttributeType ResolveCustomAttributeType(uint codedToken, GenericParamContext gpContext)
	{
		if (!CodedToken.CustomAttributeType.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.Method => ResolveMethod(num), 
			Table.MemberRef => ResolveMemberRef(num, gpContext), 
			_ => null, 
		};
	}

	public IResolutionScope ResolveResolutionScope(uint codedToken)
	{
		if (!CodedToken.ResolutionScope.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		Table table = MDToken.ToTable(token);
		switch (table)
		{
		case Table.TypeRef:
			if (table != Table.TypeRef)
			{
				break;
			}
			return ResolveTypeRef(num);
		case Table.Module:
			return ResolveModule(num);
		case Table.ModuleRef:
			return ResolveModuleRef(num);
		case Table.AssemblyRef:
			return ResolveAssemblyRef(num);
		}
		return null;
	}

	public ITypeOrMethodDef ResolveTypeOrMethodDef(uint codedToken)
	{
		if (!CodedToken.TypeOrMethodDef.Decode(codedToken, out uint token))
		{
			return null;
		}
		uint num = MDToken.ToRID(token);
		return MDToken.ToTable(token) switch
		{
			Table.TypeDef => ResolveTypeDef(num), 
			Table.Method => ResolveMethod(num), 
			_ => null, 
		};
	}

	public CallingConventionSig ReadSignature(uint sig)
	{
		return SignatureReader.ReadSig(this, sig, default(GenericParamContext));
	}

	public CallingConventionSig ReadSignature(uint sig, GenericParamContext gpContext)
	{
		return SignatureReader.ReadSig(this, sig, gpContext);
	}

	public TypeSig ReadTypeSignature(uint sig)
	{
		return SignatureReader.ReadTypeSig(this, sig, default(GenericParamContext));
	}

	public TypeSig ReadTypeSignature(uint sig, GenericParamContext gpContext)
	{
		return SignatureReader.ReadTypeSig(this, sig, gpContext);
	}

	public TypeSig ReadTypeSignature(uint sig, out byte[] extraData)
	{
		return SignatureReader.ReadTypeSig(this, sig, default(GenericParamContext), out extraData);
	}

	public TypeSig ReadTypeSignature(uint sig, GenericParamContext gpContext, out byte[] extraData)
	{
		return SignatureReader.ReadTypeSig(this, sig, gpContext, out extraData);
	}

	internal MarshalType ReadMarshalType(Table table, uint rid, GenericParamContext gpContext)
	{
		if (!TablesStream.TryReadFieldMarshalRow(Metadata.GetFieldMarshalRid(table, rid), out var row))
		{
			return null;
		}
		return MarshalBlobReader.Read(this, row.NativeType, gpContext);
	}

	public CilBody ReadCilBody(IList<Parameter> parameters, RVA rva)
	{
		return ReadCilBody(parameters, rva, default(GenericParamContext));
	}

	public CilBody ReadCilBody(IList<Parameter> parameters, RVA rva, GenericParamContext gpContext)
	{
		if (rva == (RVA)0u)
		{
			return new CilBody();
		}
		FileOffset fileOffset = metadata.PEImage.ToFileOffset(rva);
		if (fileOffset == (FileOffset)0u)
		{
			return new CilBody();
		}
		DataReader reader = metadata.PEImage.CreateReader();
		reader.Position = (uint)fileOffset;
		return MethodBodyReader.CreateCilBody(this, reader, parameters, gpContext, base.Context);
	}

	internal TypeDef GetOwnerType(FieldDefMD field)
	{
		return ResolveTypeDef(Metadata.GetOwnerTypeOfField(field.OrigRid));
	}

	internal TypeDef GetOwnerType(MethodDefMD method)
	{
		return ResolveTypeDef(Metadata.GetOwnerTypeOfMethod(method.OrigRid));
	}

	internal TypeDef GetOwnerType(EventDefMD evt)
	{
		return ResolveTypeDef(Metadata.GetOwnerTypeOfEvent(evt.OrigRid));
	}

	internal TypeDef GetOwnerType(PropertyDefMD property)
	{
		return ResolveTypeDef(Metadata.GetOwnerTypeOfProperty(property.OrigRid));
	}

	internal ITypeOrMethodDef GetOwner(GenericParamMD gp)
	{
		return ResolveTypeOrMethodDef(Metadata.GetOwnerOfGenericParam(gp.OrigRid));
	}

	internal GenericParam GetOwner(GenericParamConstraintMD gpc)
	{
		return ResolveGenericParam(Metadata.GetOwnerOfGenericParamConstraint(gpc.OrigRid));
	}

	internal MethodDef GetOwner(ParamDefMD pd)
	{
		return ResolveMethod(Metadata.GetOwnerOfParam(pd.OrigRid));
	}

	internal ModuleDefMD ReadModule(uint fileRid, AssemblyDef owner)
	{
		FileDef fileDef = ResolveFile(fileRid);
		if (fileDef == null)
		{
			return null;
		}
		if (!fileDef.ContainsMetadata)
		{
			return null;
		}
		string validFilename = GetValidFilename(GetBaseDirectoryOfImage(), UTF8String.ToSystemString(fileDef.Name));
		if (validFilename == null)
		{
			return null;
		}
		ModuleDefMD moduleDefMD;
		try
		{
			moduleDefMD = Load(validFilename);
		}
		catch
		{
			moduleDefMD = null;
		}
		if (moduleDefMD != null)
		{
			moduleDefMD.context = context;
			AssemblyDef assemblyDef = moduleDefMD.Assembly;
			if (assemblyDef != null && assemblyDef != owner)
			{
				assemblyDef.Modules.Remove(moduleDefMD);
			}
		}
		return moduleDefMD;
	}

	internal RidList GetModuleRidList()
	{
		if (moduleRidList == null)
		{
			InitializeModuleList();
		}
		return moduleRidList.Value;
	}

	private void InitializeModuleList()
	{
		if (moduleRidList != null)
		{
			return;
		}
		uint rows = TablesStream.FileTable.Rows;
		List<uint> list = new List<uint>((int)rows);
		string baseDirectoryOfImage = GetBaseDirectoryOfImage();
		for (uint num = 1u; num <= rows; num++)
		{
			FileDef fileDef = ResolveFile(num);
			if (fileDef != null && fileDef.ContainsMetadata && GetValidFilename(baseDirectoryOfImage, UTF8String.ToSystemString(fileDef.Name)) != null)
			{
				list.Add(num);
			}
		}
		Interlocked.CompareExchange(ref moduleRidList, new StrongBox<RidList>(RidList.Create(list)), null);
	}

	private static string GetValidFilename(string baseDir, string name)
	{
		if (baseDir == null)
		{
			return null;
		}
		try
		{
			if (name.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
			{
				return null;
			}
			string text = Path.Combine(baseDir, name);
			if (text != Path.GetFullPath(text))
			{
				return null;
			}
			if (!File.Exists(text))
			{
				return null;
			}
			return text;
		}
		catch
		{
			return null;
		}
	}

	private string GetBaseDirectoryOfImage()
	{
		string text = base.Location;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		try
		{
			return Path.GetDirectoryName(text);
		}
		catch (IOException)
		{
		}
		catch (ArgumentException)
		{
		}
		return null;
	}

	private Resource CreateResource(uint rid)
	{
		if (!TablesStream.TryReadManifestResourceRow(rid, out var row))
		{
			return new EmbeddedResource(UTF8String.Empty, Array2.Empty<byte>(), (ManifestResourceAttributes)0u)
			{
				Rid = rid
			};
		}
		if (!CodedToken.Implementation.Decode(row.Implementation, out MDToken token))
		{
			return new EmbeddedResource(UTF8String.Empty, Array2.Empty<byte>(), (ManifestResourceAttributes)0u)
			{
				Rid = rid
			};
		}
		ManifestResource manifestResource = ResolveManifestResource(rid);
		if (manifestResource == null)
		{
			return new EmbeddedResource(UTF8String.Empty, Array2.Empty<byte>(), (ManifestResourceAttributes)0u)
			{
				Rid = rid
			};
		}
		if (token.Rid == 0)
		{
			if (TryCreateResourceStream(manifestResource.Offset, out var dataReaderFactory, out var resourceOffset, out var resourceLength))
			{
				return new EmbeddedResourceMD(this, manifestResource, dataReaderFactory, resourceOffset, resourceLength);
			}
			return new EmbeddedResourceMD(this, manifestResource, Array2.Empty<byte>());
		}
		if (manifestResource.Implementation is FileDef file)
		{
			return new LinkedResourceMD(this, manifestResource, file);
		}
		if (manifestResource.Implementation is AssemblyRef asmRef)
		{
			return new AssemblyLinkedResourceMD(this, manifestResource, asmRef);
		}
		return new EmbeddedResourceMD(this, manifestResource, Array2.Empty<byte>());
	}

	[HandleProcessCorruptedStateExceptions]
	[SecurityCritical]
	private bool TryCreateResourceStream(uint offset, out DataReaderFactory dataReaderFactory, out uint resourceOffset, out uint resourceLength)
	{
		dataReaderFactory = null;
		resourceOffset = 0u;
		resourceLength = 0u;
		try
		{
			IPEImage pEImage = metadata.PEImage;
			ImageDataDirectory imageDataDirectory = metadata.ImageCor20Header.Resources;
			if (imageDataDirectory.VirtualAddress == (RVA)0u || imageDataDirectory.Size == 0)
			{
				return false;
			}
			DataReader dataReader = pEImage.CreateReader();
			uint num = (uint)pEImage.ToFileOffset(imageDataDirectory.VirtualAddress);
			if (num == 0 || (ulong)((long)num + (long)offset) > 4294967295uL)
			{
				return false;
			}
			if ((ulong)((long)offset + 4L) > (ulong)imageDataDirectory.Size)
			{
				return false;
			}
			if ((ulong)((long)num + (long)offset + 4) > (ulong)dataReader.Length)
			{
				return false;
			}
			dataReader.Position = num + offset;
			resourceLength = dataReader.ReadUInt32();
			resourceOffset = dataReader.Position;
			if (resourceLength == 0 || (ulong)((long)dataReader.Position + (long)resourceLength) > (ulong)dataReader.Length)
			{
				return false;
			}
			if ((ulong)((long)dataReader.Position - (long)num + resourceLength - 1) >= (ulong)imageDataDirectory.Size)
			{
				return false;
			}
			if (pEImage.MayHaveInvalidAddresses)
			{
				DataReader dataReader2 = pEImage.CreateReader((FileOffset)dataReader.Position, resourceLength);
				while (dataReader2.Position < dataReader2.Length)
				{
					dataReader2.ReadByte();
					dataReader2.Position += Math.Min(dataReader2.BytesLeft, 4096u);
				}
				dataReader2.Position = dataReader2.Length - 1;
				dataReader2.ReadByte();
			}
			dataReaderFactory = pEImage.DataReaderFactory;
			return true;
		}
		catch (IOException)
		{
		}
		catch (AccessViolationException)
		{
		}
		return false;
	}

	public CustomAttribute ReadCustomAttribute(uint caRid)
	{
		return ReadCustomAttribute(caRid, default(GenericParamContext));
	}

	public CustomAttribute ReadCustomAttribute(uint caRid, GenericParamContext gpContext)
	{
		if (!TablesStream.TryReadCustomAttributeRow(caRid, out var row))
		{
			return null;
		}
		return CustomAttributeReader.Read(this, ResolveCustomAttributeType(row.Type, gpContext), row.Value, gpContext);
	}

	public byte[] ReadDataAt(RVA rva, int size)
	{
		if (size < 0)
		{
			return null;
		}
		DataReader dataReader = Metadata.PEImage.CreateReader(rva, (uint)size);
		if (dataReader.Length < size)
		{
			return null;
		}
		return dataReader.ReadBytes(size);
	}

	public RVA GetNativeEntryPoint()
	{
		dnlib.DotNet.MD.ImageCor20Header imageCor20Header = Metadata.ImageCor20Header;
		if ((imageCor20Header.Flags & ComImageFlags.NativeEntryPoint) == 0)
		{
			return (RVA)0u;
		}
		return (RVA)imageCor20Header.EntryPointToken_or_RVA;
	}

	public IManagedEntryPoint GetManagedEntryPoint()
	{
		dnlib.DotNet.MD.ImageCor20Header imageCor20Header = Metadata.ImageCor20Header;
		if ((imageCor20Header.Flags & ComImageFlags.NativeEntryPoint) != 0)
		{
			return null;
		}
		return ResolveToken(imageCor20Header.EntryPointToken_or_RVA) as IManagedEntryPoint;
	}

	internal FieldDefMD ReadField(uint rid)
	{
		return new FieldDefMD(this, rid);
	}

	internal MethodDefMD ReadMethod(uint rid)
	{
		return new MethodDefMD(this, rid);
	}

	internal EventDefMD ReadEvent(uint rid)
	{
		return new EventDefMD(this, rid);
	}

	internal PropertyDefMD ReadProperty(uint rid)
	{
		return new PropertyDefMD(this, rid);
	}

	internal ParamDefMD ReadParam(uint rid)
	{
		return new ParamDefMD(this, rid);
	}

	internal GenericParamMD ReadGenericParam(uint rid)
	{
		return new GenericParamMD(this, rid);
	}

	internal GenericParamConstraintMD ReadGenericParamConstraint(uint rid)
	{
		return new GenericParamConstraintMD(this, rid, default(GenericParamContext));
	}

	internal GenericParamConstraintMD ReadGenericParamConstraint(uint rid, GenericParamContext gpContext)
	{
		return new GenericParamConstraintMD(this, rid, gpContext);
	}

	internal dnlib.DotNet.Emit.MethodBody ReadMethodBody(MethodDefMD method, RVA rva, MethodImplAttributes implAttrs, GenericParamContext gpContext)
	{
		IMethodDecrypter methodDecrypter = this.methodDecrypter;
		if (methodDecrypter != null && methodDecrypter.GetMethodBody(method.OrigRid, rva, method.Parameters, gpContext, out var methodBody))
		{
			if (methodBody is CilBody body)
			{
				return InitializeBodyFromPdb(method, body);
			}
			return methodBody;
		}
		if (rva == (RVA)0u)
		{
			return null;
		}
		return (implAttrs & MethodImplAttributes.CodeTypeMask) switch
		{
			MethodImplAttributes.IL => InitializeBodyFromPdb(method, ReadCilBody(method.Parameters, rva, gpContext)), 
			MethodImplAttributes.Native => new NativeMethodBody(rva), 
			_ => null, 
		};
	}

	private CilBody InitializeBodyFromPdb(MethodDefMD method, CilBody body)
	{
		pdbState?.InitializeMethodBody(this, method, body);
		return body;
	}

	internal void InitializeCustomDebugInfos(MethodDefMD method, CilBody body, IList<PdbCustomDebugInfo> customDebugInfos)
	{
		if (body != null)
		{
			pdbState?.InitializeCustomDebugInfos(method, body, customDebugInfos);
		}
	}

	public string ReadUserString(uint token)
	{
		IStringDecrypter stringDecrypter = this.stringDecrypter;
		if (stringDecrypter != null)
		{
			string text = stringDecrypter.ReadUserString(token);
			if (text != null)
			{
				return text;
			}
		}
		return USStream.ReadNoNull(token & 0xFFFFFFu);
	}

	internal MethodExportInfo GetExportInfo(uint methodRid)
	{
		if (methodExportInfoProvider == null)
		{
			InitializeMethodExportInfoProvider();
		}
		return methodExportInfoProvider.GetMethodExportInfo(100663296 + methodRid);
	}

	private void InitializeMethodExportInfoProvider()
	{
		Interlocked.CompareExchange(ref methodExportInfoProvider, new MethodExportInfoProvider(this), null);
	}

	public void NativeWrite(string filename)
	{
		NativeWrite(filename, null);
	}

	public void NativeWrite(string filename, NativeModuleWriterOptions options)
	{
		new NativeModuleWriter(this, options ?? new NativeModuleWriterOptions(this, optimizeImageSize: true)).Write(filename);
	}

	public void NativeWrite(Stream dest)
	{
		NativeWrite(dest, null);
	}

	public void NativeWrite(Stream dest, NativeModuleWriterOptions options)
	{
		new NativeModuleWriter(this, options ?? new NativeModuleWriterOptions(this, optimizeImageSize: true)).Write(dest);
	}

	public byte[] ReadBlob(uint token)
	{
		uint num = MDToken.ToRID(token);
		switch (MDToken.ToTable(token))
		{
		case Table.Field:
		{
			if (TablesStream.TryReadFieldRow(num, out var row12))
			{
				return BlobStream.Read(row12.Signature);
			}
			break;
		}
		case Table.Method:
		{
			if (TablesStream.TryReadMethodRow(num, out var row4))
			{
				return BlobStream.Read(row4.Signature);
			}
			break;
		}
		case Table.MemberRef:
		{
			if (TablesStream.TryReadMemberRefRow(num, out var row8))
			{
				return BlobStream.Read(row8.Signature);
			}
			break;
		}
		case Table.Constant:
		{
			if (TablesStream.TryReadConstantRow(num, out var row14))
			{
				return BlobStream.Read(row14.Value);
			}
			break;
		}
		case Table.CustomAttribute:
		{
			if (TablesStream.TryReadCustomAttributeRow(num, out var row10))
			{
				return BlobStream.Read(row10.Value);
			}
			break;
		}
		case Table.FieldMarshal:
		{
			if (TablesStream.TryReadFieldMarshalRow(num, out var row6))
			{
				return BlobStream.Read(row6.NativeType);
			}
			break;
		}
		case Table.DeclSecurity:
		{
			if (TablesStream.TryReadDeclSecurityRow(num, out var row2))
			{
				return BlobStream.Read(row2.PermissionSet);
			}
			break;
		}
		case Table.StandAloneSig:
		{
			if (TablesStream.TryReadStandAloneSigRow(num, out var row13))
			{
				return BlobStream.Read(row13.Signature);
			}
			break;
		}
		case Table.Property:
		{
			if (TablesStream.TryReadPropertyRow(num, out var row11))
			{
				return BlobStream.Read(row11.Type);
			}
			break;
		}
		case Table.TypeSpec:
		{
			if (TablesStream.TryReadTypeSpecRow(num, out var row9))
			{
				return BlobStream.Read(row9.Signature);
			}
			break;
		}
		case Table.Assembly:
		{
			if (TablesStream.TryReadAssemblyRow(num, out var row7))
			{
				return BlobStream.Read(row7.PublicKey);
			}
			break;
		}
		case Table.AssemblyRef:
		{
			if (TablesStream.TryReadAssemblyRefRow(num, out var row5))
			{
				return BlobStream.Read(row5.PublicKeyOrToken);
			}
			break;
		}
		case Table.File:
		{
			if (TablesStream.TryReadFileRow(num, out var row3))
			{
				return BlobStream.Read(row3.HashValue);
			}
			break;
		}
		case Table.MethodSpec:
		{
			if (TablesStream.TryReadMethodSpecRow(num, out var row))
			{
				return BlobStream.Read(row.Instantiation);
			}
			break;
		}
		}
		return null;
	}
}
