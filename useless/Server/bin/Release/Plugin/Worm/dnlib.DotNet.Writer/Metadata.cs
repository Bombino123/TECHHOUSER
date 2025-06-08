using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.DotNet.Pdb.Portable;
using dnlib.IO;
using dnlib.PE;

namespace dnlib.DotNet.Writer;

[ComVisible(true)]
public abstract class Metadata : IReuseChunk, IChunk, ISignatureWriterHelper, IWriterError, ITokenProvider, ICustomAttributeWriterHelper, IFullNameFactoryHelper, IPortablePdbCustomDebugInfoWriterHelper, IWriterError2
{
	internal sealed class SortedRows<T, TRow> where T : class where TRow : struct
	{
		public struct Info
		{
			public readonly T data;

			public TRow row;

			public Info(T data, ref TRow row)
			{
				this.data = data;
				this.row = row;
			}
		}

		public List<Info> infos = new List<Info>();

		private Dictionary<T, uint> toRid = new Dictionary<T, uint>();

		private bool isSorted;

		public void Add(T data, TRow row)
		{
			if (isSorted)
			{
				throw new ModuleWriterException($"Adding a row after it's been sorted. Table: {row.GetType()}");
			}
			infos.Add(new Info(data, ref row));
			toRid[data] = (uint)(toRid.Count + 1);
		}

		public void Sort(Comparison<Info> comparison)
		{
			infos.Sort(CreateComparison(comparison));
			toRid.Clear();
			for (int i = 0; i < infos.Count; i++)
			{
				toRid[infos[i].data] = (uint)(i + 1);
			}
			isSorted = true;
		}

		private Comparison<Info> CreateComparison(Comparison<Info> comparison)
		{
			return delegate(Info a, Info b)
			{
				int num = comparison(a, b);
				return (num != 0) ? num : toRid[a.data].CompareTo(toRid[b.data]);
			};
		}

		public uint Rid(T data)
		{
			return toRid[data];
		}

		public bool TryGetRid(T data, out uint rid)
		{
			if (data == null)
			{
				rid = 0u;
				return false;
			}
			return toRid.TryGetValue(data, out rid);
		}
	}

	internal sealed class Rows<T> where T : class
	{
		private Dictionary<T, uint> dict = new Dictionary<T, uint>();

		public int Count => dict.Count;

		public bool TryGetRid(T value, out uint rid)
		{
			if (value == null)
			{
				rid = 0u;
				return false;
			}
			return dict.TryGetValue(value, out rid);
		}

		public bool Exists(T value)
		{
			return dict.ContainsKey(value);
		}

		public void Add(T value, uint rid)
		{
			dict.Add(value, rid);
		}

		public uint Rid(T value)
		{
			return dict[value];
		}

		public void SetRid(T value, uint rid)
		{
			dict[value] = rid;
		}
	}

	private struct MethodScopeDebugInfo
	{
		public uint MethodRid;

		public PdbScope Scope;

		public uint ScopeStart;

		public uint ScopeLength;
	}

	private uint length;

	private FileOffset offset;

	private RVA rva;

	private readonly MetadataOptions options;

	private ILogger logger;

	private readonly MetadataErrorContext errorContext;

	private readonly NormalMetadata debugMetadata;

	private readonly bool isStandaloneDebugMetadata;

	internal readonly ModuleDef module;

	internal readonly UniqueChunkList<ByteArrayChunk> constants;

	internal readonly MethodBodyChunks methodBodies;

	internal readonly NetResources netResources;

	internal readonly MetadataHeader metadataHeader;

	internal readonly PdbHeap pdbHeap;

	internal readonly TablesHeap tablesHeap;

	internal readonly StringsHeap stringsHeap;

	internal readonly USHeap usHeap;

	internal readonly GuidHeap guidHeap;

	internal readonly BlobHeap blobHeap;

	internal TypeDef[] allTypeDefs;

	internal readonly Rows<ModuleDef> moduleDefInfos = new Rows<ModuleDef>();

	internal readonly SortedRows<InterfaceImpl, RawInterfaceImplRow> interfaceImplInfos = new SortedRows<InterfaceImpl, RawInterfaceImplRow>();

	internal readonly SortedRows<IHasConstant, RawConstantRow> hasConstantInfos = new SortedRows<IHasConstant, RawConstantRow>();

	internal readonly SortedRows<CustomAttribute, RawCustomAttributeRow> customAttributeInfos = new SortedRows<CustomAttribute, RawCustomAttributeRow>();

	internal readonly SortedRows<IHasFieldMarshal, RawFieldMarshalRow> fieldMarshalInfos = new SortedRows<IHasFieldMarshal, RawFieldMarshalRow>();

	internal readonly SortedRows<DeclSecurity, RawDeclSecurityRow> declSecurityInfos = new SortedRows<DeclSecurity, RawDeclSecurityRow>();

	internal readonly SortedRows<TypeDef, RawClassLayoutRow> classLayoutInfos = new SortedRows<TypeDef, RawClassLayoutRow>();

	internal readonly SortedRows<FieldDef, RawFieldLayoutRow> fieldLayoutInfos = new SortedRows<FieldDef, RawFieldLayoutRow>();

	internal readonly Rows<TypeDef> eventMapInfos = new Rows<TypeDef>();

	internal readonly Rows<TypeDef> propertyMapInfos = new Rows<TypeDef>();

	internal readonly SortedRows<MethodDef, RawMethodSemanticsRow> methodSemanticsInfos = new SortedRows<MethodDef, RawMethodSemanticsRow>();

	internal readonly SortedRows<MethodDef, RawMethodImplRow> methodImplInfos = new SortedRows<MethodDef, RawMethodImplRow>();

	internal readonly Rows<ModuleRef> moduleRefInfos = new Rows<ModuleRef>();

	internal readonly SortedRows<IMemberForwarded, RawImplMapRow> implMapInfos = new SortedRows<IMemberForwarded, RawImplMapRow>();

	internal readonly SortedRows<FieldDef, RawFieldRVARow> fieldRVAInfos = new SortedRows<FieldDef, RawFieldRVARow>();

	internal readonly Rows<AssemblyDef> assemblyInfos = new Rows<AssemblyDef>();

	internal readonly Rows<AssemblyRef> assemblyRefInfos = new Rows<AssemblyRef>();

	internal readonly Rows<FileDef> fileDefInfos = new Rows<FileDef>();

	internal readonly Rows<ExportedType> exportedTypeInfos = new Rows<ExportedType>();

	internal readonly Rows<Resource> manifestResourceInfos = new Rows<Resource>();

	internal readonly SortedRows<TypeDef, RawNestedClassRow> nestedClassInfos = new SortedRows<TypeDef, RawNestedClassRow>();

	internal readonly SortedRows<GenericParam, RawGenericParamRow> genericParamInfos = new SortedRows<GenericParam, RawGenericParamRow>();

	internal readonly SortedRows<GenericParamConstraint, RawGenericParamConstraintRow> genericParamConstraintInfos = new SortedRows<GenericParamConstraint, RawGenericParamConstraintRow>();

	internal readonly Dictionary<MethodDef, MethodBody> methodToBody = new Dictionary<MethodDef, MethodBody>();

	internal readonly Dictionary<MethodDef, NativeMethodBody> methodToNativeBody = new Dictionary<MethodDef, NativeMethodBody>();

	internal readonly Dictionary<EmbeddedResource, DataReaderChunk> embeddedResourceToByteArray = new Dictionary<EmbeddedResource, DataReaderChunk>();

	private readonly Dictionary<FieldDef, ByteArrayChunk> fieldToInitialValue = new Dictionary<FieldDef, ByteArrayChunk>();

	private readonly Rows<PdbDocument> pdbDocumentInfos = new Rows<PdbDocument>();

	private bool methodDebugInformationInfosUsed;

	private readonly SortedRows<PdbScope, RawLocalScopeRow> localScopeInfos = new SortedRows<PdbScope, RawLocalScopeRow>();

	private readonly Rows<PdbLocal> localVariableInfos = new Rows<PdbLocal>();

	private readonly Rows<PdbConstant> localConstantInfos = new Rows<PdbConstant>();

	private readonly Rows<PdbImportScope> importScopeInfos = new Rows<PdbImportScope>();

	private readonly SortedRows<PdbCustomDebugInfo, RawStateMachineMethodRow> stateMachineMethodInfos = new SortedRows<PdbCustomDebugInfo, RawStateMachineMethodRow>();

	private readonly SortedRows<PdbCustomDebugInfo, RawCustomDebugInformationRow> customDebugInfos = new SortedRows<PdbCustomDebugInfo, RawCustomDebugInformationRow>();

	private readonly List<DataWriterContext> binaryWriterContexts = new List<DataWriterContext>();

	private readonly List<SerializerMethodContext> serializerMethodContexts = new List<SerializerMethodContext>();

	private readonly List<MethodDef> exportedMethods = new List<MethodDef>();

	private static readonly double[] eventToProgress = new double[15]
	{
		0.0, 0.00134240009466231, 0.00257484711254305, 0.0762721800615359, 0.196633787905108, 0.207788892253819, 0.270543867900699, 0.451478814851716, 0.451478949929206, 0.454664752528583,
		0.454664887606073, 0.992591810143725, 0.999984331011171, 1.0, 1.0
	};

	private static readonly byte[] constantClassByteArray = new byte[4];

	private static readonly byte[] constantDefaultByteArray = new byte[8];

	private static readonly byte[] directorySeparatorCharUtf8;

	private static readonly char[] directorySeparatorCharArray;

	private const uint HEAP_ALIGNMENT = 4u;

	public ILogger Logger
	{
		get
		{
			return logger;
		}
		set
		{
			logger = value;
		}
	}

	public ModuleDef Module => module;

	public UniqueChunkList<ByteArrayChunk> Constants => constants;

	public MethodBodyChunks MethodBodyChunks => methodBodies;

	public NetResources NetResources => netResources;

	public MetadataHeader MetadataHeader => metadataHeader;

	public TablesHeap TablesHeap => tablesHeap;

	public StringsHeap StringsHeap => stringsHeap;

	public USHeap USHeap => usHeap;

	public GuidHeap GuidHeap => guidHeap;

	public BlobHeap BlobHeap => blobHeap;

	public PdbHeap PdbHeap => pdbHeap;

	public List<MethodDef> ExportedMethods => exportedMethods;

	internal byte[] AssemblyPublicKey { get; set; }

	public FileOffset FileOffset => offset;

	public RVA RVA => rva;

	public bool PreserveTypeRefRids => (options.Flags & MetadataFlags.PreserveTypeRefRids) != 0;

	public bool PreserveTypeDefRids => (options.Flags & MetadataFlags.PreserveTypeDefRids) != 0;

	public bool PreserveFieldRids => (options.Flags & MetadataFlags.PreserveFieldRids) != 0;

	public bool PreserveMethodRids => (options.Flags & MetadataFlags.PreserveMethodRids) != 0;

	public bool PreserveParamRids => (options.Flags & MetadataFlags.PreserveParamRids) != 0;

	public bool PreserveMemberRefRids => (options.Flags & MetadataFlags.PreserveMemberRefRids) != 0;

	public bool PreserveStandAloneSigRids => (options.Flags & MetadataFlags.PreserveStandAloneSigRids) != 0;

	public bool PreserveEventRids => (options.Flags & MetadataFlags.PreserveEventRids) != 0;

	public bool PreservePropertyRids => (options.Flags & MetadataFlags.PreservePropertyRids) != 0;

	public bool PreserveTypeSpecRids => (options.Flags & MetadataFlags.PreserveTypeSpecRids) != 0;

	public bool PreserveMethodSpecRids => (options.Flags & MetadataFlags.PreserveMethodSpecRids) != 0;

	public bool PreserveStringsOffsets
	{
		get
		{
			return (options.Flags & MetadataFlags.PreserveStringsOffsets) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.PreserveStringsOffsets;
			}
			else
			{
				options.Flags &= ~MetadataFlags.PreserveStringsOffsets;
			}
		}
	}

	public bool PreserveUSOffsets
	{
		get
		{
			return (options.Flags & MetadataFlags.PreserveUSOffsets) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.PreserveUSOffsets;
			}
			else
			{
				options.Flags &= ~MetadataFlags.PreserveUSOffsets;
			}
		}
	}

	public bool PreserveBlobOffsets
	{
		get
		{
			return (options.Flags & MetadataFlags.PreserveBlobOffsets) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.PreserveBlobOffsets;
			}
			else
			{
				options.Flags &= ~MetadataFlags.PreserveBlobOffsets;
			}
		}
	}

	public bool PreserveExtraSignatureData
	{
		get
		{
			return (options.Flags & MetadataFlags.PreserveExtraSignatureData) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.PreserveExtraSignatureData;
			}
			else
			{
				options.Flags &= ~MetadataFlags.PreserveExtraSignatureData;
			}
		}
	}

	public bool KeepOldMaxStack
	{
		get
		{
			return (options.Flags & MetadataFlags.KeepOldMaxStack) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.KeepOldMaxStack;
			}
			else
			{
				options.Flags &= ~MetadataFlags.KeepOldMaxStack;
			}
		}
	}

	public bool AlwaysCreateGuidHeap
	{
		get
		{
			return (options.Flags & MetadataFlags.AlwaysCreateGuidHeap) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.AlwaysCreateGuidHeap;
			}
			else
			{
				options.Flags &= ~MetadataFlags.AlwaysCreateGuidHeap;
			}
		}
	}

	public bool AlwaysCreateStringsHeap
	{
		get
		{
			return (options.Flags & MetadataFlags.AlwaysCreateStringsHeap) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.AlwaysCreateStringsHeap;
			}
			else
			{
				options.Flags &= ~MetadataFlags.AlwaysCreateStringsHeap;
			}
		}
	}

	public bool AlwaysCreateUSHeap
	{
		get
		{
			return (options.Flags & MetadataFlags.AlwaysCreateUSHeap) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.AlwaysCreateUSHeap;
			}
			else
			{
				options.Flags &= ~MetadataFlags.AlwaysCreateUSHeap;
			}
		}
	}

	public bool AlwaysCreateBlobHeap
	{
		get
		{
			return (options.Flags & MetadataFlags.AlwaysCreateBlobHeap) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.AlwaysCreateBlobHeap;
			}
			else
			{
				options.Flags &= ~MetadataFlags.AlwaysCreateBlobHeap;
			}
		}
	}

	public bool RoslynSortInterfaceImpl
	{
		get
		{
			return (options.Flags & MetadataFlags.RoslynSortInterfaceImpl) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.RoslynSortInterfaceImpl;
			}
			else
			{
				options.Flags &= ~MetadataFlags.RoslynSortInterfaceImpl;
			}
		}
	}

	public bool NoMethodBodies
	{
		get
		{
			return (options.Flags & MetadataFlags.NoMethodBodies) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.NoMethodBodies;
			}
			else
			{
				options.Flags &= ~MetadataFlags.NoMethodBodies;
			}
		}
	}

	public bool NoDotNetResources
	{
		get
		{
			return (options.Flags & MetadataFlags.NoDotNetResources) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.NoDotNetResources;
			}
			else
			{
				options.Flags &= ~MetadataFlags.NoDotNetResources;
			}
		}
	}

	public bool NoFieldData
	{
		get
		{
			return (options.Flags & MetadataFlags.NoFieldData) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.NoFieldData;
			}
			else
			{
				options.Flags &= ~MetadataFlags.NoFieldData;
			}
		}
	}

	public bool OptimizeCustomAttributeSerializedTypeNames
	{
		get
		{
			return (options.Flags & MetadataFlags.OptimizeCustomAttributeSerializedTypeNames) != 0;
		}
		set
		{
			if (value)
			{
				options.Flags |= MetadataFlags.OptimizeCustomAttributeSerializedTypeNames;
			}
			else
			{
				options.Flags &= ~MetadataFlags.OptimizeCustomAttributeSerializedTypeNames;
			}
		}
	}

	internal bool KeepFieldRVA { get; set; }

	protected abstract int NumberOfMethods { get; }

	public event EventHandler2<MetadataWriterEventArgs> MetadataEvent;

	public event EventHandler2<MetadataProgressEventArgs> ProgressUpdated;

	public static Metadata Create(ModuleDef module, UniqueChunkList<ByteArrayChunk> constants, MethodBodyChunks methodBodies, NetResources netResources, MetadataOptions options = null, DebugMetadataKind debugKind = DebugMetadataKind.None)
	{
		if (options == null)
		{
			options = new MetadataOptions();
		}
		if ((options.Flags & MetadataFlags.PreserveRids) != 0 && module is ModuleDefMD)
		{
			return new PreserveTokensMetadata(module, constants, methodBodies, netResources, options, debugKind, isStandaloneDebugMetadata: false);
		}
		return new NormalMetadata(module, constants, methodBodies, netResources, options, debugKind, isStandaloneDebugMetadata: false);
	}

	internal Metadata(ModuleDef module, UniqueChunkList<ByteArrayChunk> constants, MethodBodyChunks methodBodies, NetResources netResources, MetadataOptions options, DebugMetadataKind debugKind, bool isStandaloneDebugMetadata)
	{
		this.module = module;
		this.constants = constants;
		this.methodBodies = methodBodies;
		this.netResources = netResources;
		this.options = options ?? new MetadataOptions();
		metadataHeader = new MetadataHeader(isStandaloneDebugMetadata ? this.options.DebugMetadataHeaderOptions : this.options.MetadataHeaderOptions);
		tablesHeap = new TablesHeap(this, isStandaloneDebugMetadata ? this.options.DebugTablesHeapOptions : this.options.TablesHeapOptions);
		stringsHeap = new StringsHeap();
		usHeap = new USHeap();
		guidHeap = new GuidHeap();
		blobHeap = new BlobHeap();
		pdbHeap = new PdbHeap();
		errorContext = new MetadataErrorContext();
		this.isStandaloneDebugMetadata = isStandaloneDebugMetadata;
		switch (debugKind)
		{
		case DebugMetadataKind.Standalone:
			debugMetadata = new NormalMetadata(module, constants, methodBodies, netResources, options, DebugMetadataKind.None, isStandaloneDebugMetadata: true);
			break;
		default:
			throw new ArgumentOutOfRangeException("debugKind");
		case DebugMetadataKind.None:
			break;
		}
	}

	public uint GetRid(ModuleDef module)
	{
		moduleDefInfos.TryGetRid(module, out var rid);
		return rid;
	}

	public abstract uint GetRid(TypeRef tr);

	public abstract uint GetRid(TypeDef td);

	public abstract uint GetRid(FieldDef fd);

	public abstract uint GetRid(MethodDef md);

	public abstract uint GetRid(ParamDef pd);

	public uint GetRid(InterfaceImpl ii)
	{
		interfaceImplInfos.TryGetRid(ii, out var rid);
		return rid;
	}

	public abstract uint GetRid(MemberRef mr);

	public uint GetConstantRid(IHasConstant hc)
	{
		hasConstantInfos.TryGetRid(hc, out var rid);
		return rid;
	}

	public uint GetCustomAttributeRid(CustomAttribute ca)
	{
		customAttributeInfos.TryGetRid(ca, out var rid);
		return rid;
	}

	public uint GetFieldMarshalRid(IHasFieldMarshal hfm)
	{
		fieldMarshalInfos.TryGetRid(hfm, out var rid);
		return rid;
	}

	public uint GetRid(DeclSecurity ds)
	{
		declSecurityInfos.TryGetRid(ds, out var rid);
		return rid;
	}

	public uint GetClassLayoutRid(TypeDef td)
	{
		classLayoutInfos.TryGetRid(td, out var rid);
		return rid;
	}

	public uint GetFieldLayoutRid(FieldDef fd)
	{
		fieldLayoutInfos.TryGetRid(fd, out var rid);
		return rid;
	}

	public abstract uint GetRid(StandAloneSig sas);

	public uint GetEventMapRid(TypeDef td)
	{
		eventMapInfos.TryGetRid(td, out var rid);
		return rid;
	}

	public abstract uint GetRid(EventDef ed);

	public uint GetPropertyMapRid(TypeDef td)
	{
		propertyMapInfos.TryGetRid(td, out var rid);
		return rid;
	}

	public abstract uint GetRid(PropertyDef pd);

	public uint GetMethodSemanticsRid(MethodDef md)
	{
		methodSemanticsInfos.TryGetRid(md, out var rid);
		return rid;
	}

	public uint GetRid(ModuleRef mr)
	{
		moduleRefInfos.TryGetRid(mr, out var rid);
		return rid;
	}

	public abstract uint GetRid(TypeSpec ts);

	public uint GetImplMapRid(IMemberForwarded mf)
	{
		implMapInfos.TryGetRid(mf, out var rid);
		return rid;
	}

	public uint GetFieldRVARid(FieldDef fd)
	{
		fieldRVAInfos.TryGetRid(fd, out var rid);
		return rid;
	}

	public uint GetRid(AssemblyDef asm)
	{
		assemblyInfos.TryGetRid(asm, out var rid);
		return rid;
	}

	public uint GetRid(AssemblyRef asmRef)
	{
		assemblyRefInfos.TryGetRid(asmRef, out var rid);
		return rid;
	}

	public uint GetRid(FileDef fd)
	{
		fileDefInfos.TryGetRid(fd, out var rid);
		return rid;
	}

	public uint GetRid(ExportedType et)
	{
		exportedTypeInfos.TryGetRid(et, out var rid);
		return rid;
	}

	public uint GetManifestResourceRid(Resource resource)
	{
		manifestResourceInfos.TryGetRid(resource, out var rid);
		return rid;
	}

	public uint GetNestedClassRid(TypeDef td)
	{
		nestedClassInfos.TryGetRid(td, out var rid);
		return rid;
	}

	public uint GetRid(GenericParam gp)
	{
		genericParamInfos.TryGetRid(gp, out var rid);
		return rid;
	}

	public abstract uint GetRid(MethodSpec ms);

	public uint GetRid(GenericParamConstraint gpc)
	{
		genericParamConstraintInfos.TryGetRid(gpc, out var rid);
		return rid;
	}

	public uint GetRid(PdbDocument doc)
	{
		if (debugMetadata == null)
		{
			return 0u;
		}
		debugMetadata.pdbDocumentInfos.TryGetRid(doc, out var rid);
		return rid;
	}

	public uint GetRid(PdbScope scope)
	{
		if (debugMetadata == null)
		{
			return 0u;
		}
		debugMetadata.localScopeInfos.TryGetRid(scope, out var rid);
		return rid;
	}

	public uint GetRid(PdbLocal local)
	{
		if (debugMetadata == null)
		{
			return 0u;
		}
		debugMetadata.localVariableInfos.TryGetRid(local, out var rid);
		return rid;
	}

	public uint GetRid(PdbConstant constant)
	{
		if (debugMetadata == null)
		{
			return 0u;
		}
		debugMetadata.localConstantInfos.TryGetRid(constant, out var rid);
		return rid;
	}

	public uint GetRid(PdbImportScope importScope)
	{
		if (debugMetadata == null)
		{
			return 0u;
		}
		debugMetadata.importScopeInfos.TryGetRid(importScope, out var rid);
		return rid;
	}

	public uint GetStateMachineMethodRid(PdbAsyncMethodCustomDebugInfo asyncMethod)
	{
		if (debugMetadata == null)
		{
			return 0u;
		}
		debugMetadata.stateMachineMethodInfos.TryGetRid(asyncMethod, out var rid);
		return rid;
	}

	public uint GetStateMachineMethodRid(PdbIteratorMethodCustomDebugInfo iteratorMethod)
	{
		if (debugMetadata == null)
		{
			return 0u;
		}
		debugMetadata.stateMachineMethodInfos.TryGetRid(iteratorMethod, out var rid);
		return rid;
	}

	public uint GetCustomDebugInfoRid(PdbCustomDebugInfo cdi)
	{
		if (debugMetadata == null)
		{
			return 0u;
		}
		debugMetadata.customDebugInfos.TryGetRid(cdi, out var rid);
		return rid;
	}

	public MethodBody GetMethodBody(MethodDef md)
	{
		if (md == null)
		{
			return null;
		}
		methodToBody.TryGetValue(md, out var value);
		return value;
	}

	public uint GetLocalVarSigToken(MethodDef md)
	{
		return GetMethodBody(md)?.LocalVarSigTok ?? 0;
	}

	public DataReaderChunk GetChunk(EmbeddedResource er)
	{
		if (er == null)
		{
			return null;
		}
		embeddedResourceToByteArray.TryGetValue(er, out var value);
		return value;
	}

	public ByteArrayChunk GetInitialValueChunk(FieldDef fd)
	{
		if (fd == null)
		{
			return null;
		}
		fieldToInitialValue.TryGetValue(fd, out var value);
		return value;
	}

	private ILogger GetLogger()
	{
		return logger ?? DummyLogger.ThrowModuleWriterExceptionOnErrorInstance;
	}

	protected void Error(string message, params object[] args)
	{
		errorContext.Append("Error", ref message, ref args);
		GetLogger().Log(this, LoggerEvent.Error, message, args);
	}

	protected void Warning(string message, params object[] args)
	{
		errorContext.Append("Warning", ref message, ref args);
		GetLogger().Log(this, LoggerEvent.Warning, message, args);
	}

	protected void OnMetadataEvent(MetadataEvent evt)
	{
		errorContext.Event = evt;
		RaiseProgress(evt, 0.0);
		this.MetadataEvent?.Invoke(this, new MetadataWriterEventArgs(this, evt));
	}

	protected void RaiseProgress(MetadataEvent evt, double subProgress)
	{
		subProgress = Math.Min(1.0, Math.Max(0.0, subProgress));
		double num = eventToProgress[(int)evt];
		double num2 = eventToProgress[(int)(evt + 1)];
		double val = num + (num2 - num) * subProgress;
		val = Math.Min(1.0, Math.Max(0.0, val));
		this.ProgressUpdated?.Invoke(this, new MetadataProgressEventArgs(this, val));
	}

	public void CreateTables()
	{
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.BeginCreateTables);
		if (module.Types.Count == 0 || module.Types[0] == null)
		{
			throw new ModuleWriterException("Missing global <Module> type");
		}
		if (module is ModuleDefMD moduleDefMD)
		{
			if (PreserveStringsOffsets)
			{
				stringsHeap.Populate(moduleDefMD.StringsStream);
			}
			if (PreserveUSOffsets)
			{
				usHeap.Populate(moduleDefMD.USStream);
			}
			if (PreserveBlobOffsets)
			{
				blobHeap.Populate(moduleDefMD.BlobStream);
			}
		}
		Create();
	}

	private void UpdateMethodRvas()
	{
		foreach (KeyValuePair<MethodDef, MethodBody> item in methodToBody)
		{
			MethodDef key = item.Key;
			MethodBody value = item.Value;
			uint rid = GetRid(key);
			RawMethodRow rawMethodRow = tablesHeap.MethodTable[rid];
			rawMethodRow = new RawMethodRow((uint)value.RVA, rawMethodRow.ImplFlags, rawMethodRow.Flags, rawMethodRow.Name, rawMethodRow.Signature, rawMethodRow.ParamList);
			tablesHeap.MethodTable[rid] = rawMethodRow;
		}
		foreach (KeyValuePair<MethodDef, NativeMethodBody> item2 in methodToNativeBody)
		{
			MethodDef key2 = item2.Key;
			NativeMethodBody value2 = item2.Value;
			uint rid2 = GetRid(key2);
			RawMethodRow rawMethodRow2 = tablesHeap.MethodTable[rid2];
			rawMethodRow2 = new RawMethodRow((uint)value2.RVA, rawMethodRow2.ImplFlags, rawMethodRow2.Flags, rawMethodRow2.Name, rawMethodRow2.Signature, rawMethodRow2.ParamList);
			tablesHeap.MethodTable[rid2] = rawMethodRow2;
		}
	}

	private void UpdateFieldRvas()
	{
		foreach (KeyValuePair<FieldDef, ByteArrayChunk> item in fieldToInitialValue)
		{
			FieldDef key = item.Key;
			ByteArrayChunk value = item.Value;
			uint rid = fieldRVAInfos.Rid(key);
			RawFieldRVARow rawFieldRVARow = tablesHeap.FieldRVATable[rid];
			rawFieldRVARow = new RawFieldRVARow((uint)value.RVA, rawFieldRVARow.Field);
			tablesHeap.FieldRVATable[rid] = rawFieldRVARow;
		}
	}

	private void Create()
	{
		Initialize();
		allTypeDefs = GetAllTypeDefs();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.AllocateTypeDefRids);
		AllocateTypeDefRids();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.AllocateMemberDefRids);
		AllocateMemberDefRids();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.MemberDefRidsAllocated);
		AddModule(module);
		AddPdbDocuments();
		InitializeMethodDebugInformation();
		InitializeTypeDefsAndMemberDefs();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.MemberDefsInitialized);
		InitializeVTableFixups();
		AddExportedTypes();
		InitializeEntryPoint();
		if (module.Assembly != null)
		{
			AddAssembly(module.Assembly, AssemblyPublicKey);
		}
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.BeforeSortTables);
		SortTables();
		InitializeGenericParamConstraintTable();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.MostTablesSorted);
		WriteTypeDefAndMemberDefCustomAttributesAndCustomDebugInfos();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.MemberDefCustomAttributesWritten);
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.BeginAddResources);
		AddResources(module.Resources);
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.EndAddResources);
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.BeginWriteMethodBodies);
		WriteMethodBodies();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.EndWriteMethodBodies);
		BeforeSortingCustomAttributes();
		InitializeCustomAttributeAndCustomDebugInfoTables();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.OnAllTablesSorted);
		EverythingInitialized();
		OnMetadataEvent(dnlib.DotNet.Writer.MetadataEvent.EndCreateTables);
	}

	private void InitializeTypeDefsAndMemberDefs()
	{
		int num = allTypeDefs.Length;
		int num2 = 0;
		int num3 = 0;
		int num4 = num / 5;
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef typeDef in array)
		{
			using (errorContext.SetSource(typeDef))
			{
				if (num2++ == num4 && num3 < 5)
				{
					RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.MemberDefRidsAllocated, (double)num2 / (double)num);
					num3++;
					num4 = (int)((double)num / 5.0 * (double)(num3 + 1));
				}
				if (typeDef == null)
				{
					Error("TypeDef is null");
					continue;
				}
				uint rid = GetRid(typeDef);
				RawTypeDefRow rawTypeDefRow = tablesHeap.TypeDefTable[rid];
				rawTypeDefRow = new RawTypeDefRow((uint)typeDef.Attributes, stringsHeap.Add(typeDef.Name), stringsHeap.Add(typeDef.Namespace), (typeDef.BaseType != null) ? AddTypeDefOrRef(typeDef.BaseType) : 0u, rawTypeDefRow.FieldList, rawTypeDefRow.MethodList);
				tablesHeap.TypeDefTable[rid] = rawTypeDefRow;
				AddGenericParams(new MDToken(Table.TypeDef, rid), typeDef.GenericParameters);
				AddDeclSecurities(new MDToken(Table.TypeDef, rid), typeDef.DeclSecurities);
				AddInterfaceImpls(rid, typeDef.Interfaces);
				AddClassLayout(typeDef);
				AddNestedType(typeDef, typeDef.DeclaringType);
				IList<FieldDef> fields = typeDef.Fields;
				int count = fields.Count;
				for (int j = 0; j < count; j++)
				{
					FieldDef fieldDef = fields[j];
					if (fieldDef == null)
					{
						Error("Field is null");
						continue;
					}
					using (errorContext.SetSource(fieldDef))
					{
						uint rid2 = GetRid(fieldDef);
						RawFieldRow value = new RawFieldRow((ushort)fieldDef.Attributes, stringsHeap.Add(fieldDef.Name), GetSignature(fieldDef.Signature));
						tablesHeap.FieldTable[rid2] = value;
						AddFieldLayout(fieldDef);
						AddFieldMarshal(new MDToken(Table.Field, rid2), fieldDef);
						AddFieldRVA(fieldDef);
						AddImplMap(new MDToken(Table.Field, rid2), fieldDef);
						AddConstant(new MDToken(Table.Field, rid2), fieldDef);
					}
				}
				IList<MethodDef> methods = typeDef.Methods;
				count = methods.Count;
				for (int k = 0; k < count; k++)
				{
					MethodDef methodDef = methods[k];
					if (methodDef == null)
					{
						Error("Method is null");
						continue;
					}
					using (errorContext.SetSource(methodDef))
					{
						if (methodDef.ExportInfo != null)
						{
							ExportedMethods.Add(methodDef);
						}
						uint rid3 = GetRid(methodDef);
						RawMethodRow rawMethodRow = tablesHeap.MethodTable[rid3];
						rawMethodRow = new RawMethodRow(rawMethodRow.RVA, (ushort)methodDef.ImplAttributes, (ushort)methodDef.Attributes, stringsHeap.Add(methodDef.Name), GetSignature(methodDef.Signature), rawMethodRow.ParamList);
						tablesHeap.MethodTable[rid3] = rawMethodRow;
						AddGenericParams(new MDToken(Table.Method, rid3), methodDef.GenericParameters);
						AddDeclSecurities(new MDToken(Table.Method, rid3), methodDef.DeclSecurities);
						AddImplMap(new MDToken(Table.Method, rid3), methodDef);
						AddMethodImpls(methodDef, methodDef.Overrides);
						IList<ParamDef> paramDefs = methodDef.ParamDefs;
						int count2 = paramDefs.Count;
						for (int l = 0; l < count2; l++)
						{
							ParamDef paramDef = paramDefs[l];
							if (paramDef == null)
							{
								Error("Param is null");
								continue;
							}
							uint rid4 = GetRid(paramDef);
							RawParamRow value2 = new RawParamRow((ushort)paramDef.Attributes, paramDef.Sequence, stringsHeap.Add(paramDef.Name));
							tablesHeap.ParamTable[rid4] = value2;
							AddConstant(new MDToken(Table.Param, rid4), paramDef);
							AddFieldMarshal(new MDToken(Table.Param, rid4), paramDef);
						}
					}
				}
				IList<EventDef> events = typeDef.Events;
				count = events.Count;
				for (int m = 0; m < count; m++)
				{
					EventDef eventDef = events[m];
					if (eventDef == null)
					{
						Error("Event is null");
						continue;
					}
					using (errorContext.SetSource(eventDef))
					{
						uint rid5 = GetRid(eventDef);
						RawEventRow value3 = new RawEventRow((ushort)eventDef.Attributes, stringsHeap.Add(eventDef.Name), AddTypeDefOrRef(eventDef.EventType));
						tablesHeap.EventTable[rid5] = value3;
						AddMethodSemantics(eventDef);
					}
				}
				IList<PropertyDef> properties = typeDef.Properties;
				count = properties.Count;
				for (int n = 0; n < count; n++)
				{
					PropertyDef propertyDef = properties[n];
					if (propertyDef == null)
					{
						Error("Property is null");
						continue;
					}
					using (errorContext.SetSource(propertyDef))
					{
						uint rid6 = GetRid(propertyDef);
						RawPropertyRow value4 = new RawPropertyRow((ushort)propertyDef.Attributes, stringsHeap.Add(propertyDef.Name), GetSignature(propertyDef.Type));
						tablesHeap.PropertyTable[rid6] = value4;
						AddConstant(new MDToken(Table.Property, rid6), propertyDef);
						AddMethodSemantics(propertyDef);
					}
				}
			}
		}
	}

	private void WriteTypeDefAndMemberDefCustomAttributesAndCustomDebugInfos()
	{
		int num = allTypeDefs.Length;
		int num2 = 0;
		int num3 = 0;
		int num4 = num / 5;
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef typeDef in array)
		{
			using (errorContext.SetSource(typeDef))
			{
				if (num2++ == num4 && num3 < 5)
				{
					RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.MostTablesSorted, (double)num2 / (double)num);
					num3++;
					num4 = (int)((double)num / 5.0 * (double)(num3 + 1));
				}
				if (typeDef == null)
				{
					continue;
				}
				if (typeDef.HasCustomAttributes || typeDef.HasCustomDebugInfos)
				{
					uint rid = GetRid(typeDef);
					AddCustomAttributes(Table.TypeDef, rid, typeDef);
					AddCustomDebugInformationList(Table.TypeDef, rid, typeDef);
				}
				IList<FieldDef> fields = typeDef.Fields;
				int count = fields.Count;
				for (int j = 0; j < count; j++)
				{
					FieldDef fieldDef = fields[j];
					if (fieldDef != null && (fieldDef.HasCustomAttributes || fieldDef.HasCustomDebugInfos))
					{
						uint rid = GetRid(fieldDef);
						AddCustomAttributes(Table.Field, rid, fieldDef);
						AddCustomDebugInformationList(Table.Field, rid, fieldDef);
					}
				}
				IList<MethodDef> methods = typeDef.Methods;
				count = methods.Count;
				for (int k = 0; k < count; k++)
				{
					MethodDef methodDef = methods[k];
					if (methodDef == null)
					{
						continue;
					}
					using (errorContext.SetSource(methodDef))
					{
						if (methodDef.HasCustomAttributes)
						{
							uint rid = GetRid(methodDef);
							AddCustomAttributes(Table.Method, rid, methodDef);
						}
						IList<ParamDef> paramDefs = methodDef.ParamDefs;
						int count2 = paramDefs.Count;
						for (int l = 0; l < count2; l++)
						{
							ParamDef paramDef = paramDefs[l];
							if (paramDef != null && (paramDef.HasCustomAttributes || paramDef.HasCustomDebugInfos))
							{
								uint rid = GetRid(paramDef);
								AddCustomAttributes(Table.Param, rid, paramDef);
								AddCustomDebugInformationList(Table.Param, rid, paramDef);
							}
						}
					}
				}
				IList<EventDef> events = typeDef.Events;
				count = events.Count;
				for (int m = 0; m < count; m++)
				{
					EventDef eventDef = events[m];
					if (eventDef != null && (eventDef.HasCustomAttributes || eventDef.HasCustomDebugInfos))
					{
						uint rid = GetRid(eventDef);
						AddCustomAttributes(Table.Event, rid, eventDef);
						AddCustomDebugInformationList(Table.Event, rid, eventDef);
					}
				}
				IList<PropertyDef> properties = typeDef.Properties;
				count = properties.Count;
				for (int n = 0; n < count; n++)
				{
					PropertyDef propertyDef = properties[n];
					if (propertyDef != null && (propertyDef.HasCustomAttributes || propertyDef.HasCustomDebugInfos))
					{
						uint rid = GetRid(propertyDef);
						AddCustomAttributes(Table.Property, rid, propertyDef);
						AddCustomDebugInformationList(Table.Property, rid, propertyDef);
					}
				}
			}
		}
	}

	private void InitializeVTableFixups()
	{
		VTableFixups vTableFixups = module.VTableFixups;
		if (vTableFixups == null || vTableFixups.VTables.Count == 0)
		{
			return;
		}
		using (errorContext.SetSource("vtable fixups"))
		{
			foreach (VTable item in vTableFixups)
			{
				if (item == null)
				{
					Error("VTable is null");
					continue;
				}
				foreach (IMethod item2 in item)
				{
					if (item2 != null)
					{
						AddMDTokenProvider(item2);
					}
				}
			}
		}
	}

	private void AddExportedTypes()
	{
		using (errorContext.SetSource("exported types"))
		{
			IList<ExportedType> exportedTypes = module.ExportedTypes;
			int count = exportedTypes.Count;
			for (int i = 0; i < count; i++)
			{
				AddExportedType(exportedTypes[i]);
			}
		}
	}

	private void InitializeEntryPoint()
	{
		using (errorContext.SetSource("entry point"))
		{
			if (module.ManagedEntryPoint is FileDef file)
			{
				AddFile(file);
			}
		}
	}

	private void SortTables()
	{
		classLayoutInfos.Sort(delegate(SortedRows<TypeDef, RawClassLayoutRow>.Info a, SortedRows<TypeDef, RawClassLayoutRow>.Info b)
		{
			uint parent4 = a.row.Parent;
			return parent4.CompareTo(b.row.Parent);
		});
		hasConstantInfos.Sort(delegate(SortedRows<IHasConstant, RawConstantRow>.Info a, SortedRows<IHasConstant, RawConstantRow>.Info b)
		{
			uint parent3 = a.row.Parent;
			return parent3.CompareTo(b.row.Parent);
		});
		declSecurityInfos.Sort(delegate(SortedRows<DeclSecurity, RawDeclSecurityRow>.Info a, SortedRows<DeclSecurity, RawDeclSecurityRow>.Info b)
		{
			uint parent2 = a.row.Parent;
			return parent2.CompareTo(b.row.Parent);
		});
		fieldLayoutInfos.Sort(delegate(SortedRows<FieldDef, RawFieldLayoutRow>.Info a, SortedRows<FieldDef, RawFieldLayoutRow>.Info b)
		{
			uint field2 = a.row.Field;
			return field2.CompareTo(b.row.Field);
		});
		fieldMarshalInfos.Sort(delegate(SortedRows<IHasFieldMarshal, RawFieldMarshalRow>.Info a, SortedRows<IHasFieldMarshal, RawFieldMarshalRow>.Info b)
		{
			uint parent = a.row.Parent;
			return parent.CompareTo(b.row.Parent);
		});
		fieldRVAInfos.Sort(delegate(SortedRows<FieldDef, RawFieldRVARow>.Info a, SortedRows<FieldDef, RawFieldRVARow>.Info b)
		{
			uint field = a.row.Field;
			return field.CompareTo(b.row.Field);
		});
		implMapInfos.Sort(delegate(SortedRows<IMemberForwarded, RawImplMapRow>.Info a, SortedRows<IMemberForwarded, RawImplMapRow>.Info b)
		{
			uint memberForwarded = a.row.MemberForwarded;
			return memberForwarded.CompareTo(b.row.MemberForwarded);
		});
		methodImplInfos.Sort(delegate(SortedRows<MethodDef, RawMethodImplRow>.Info a, SortedRows<MethodDef, RawMethodImplRow>.Info b)
		{
			uint class2 = a.row.Class;
			return class2.CompareTo(b.row.Class);
		});
		methodSemanticsInfos.Sort(delegate(SortedRows<MethodDef, RawMethodSemanticsRow>.Info a, SortedRows<MethodDef, RawMethodSemanticsRow>.Info b)
		{
			uint association = a.row.Association;
			return association.CompareTo(b.row.Association);
		});
		nestedClassInfos.Sort(delegate(SortedRows<TypeDef, RawNestedClassRow>.Info a, SortedRows<TypeDef, RawNestedClassRow>.Info b)
		{
			uint nestedClass = a.row.NestedClass;
			return nestedClass.CompareTo(b.row.NestedClass);
		});
		genericParamInfos.Sort(delegate(SortedRows<GenericParam, RawGenericParamRow>.Info a, SortedRows<GenericParam, RawGenericParamRow>.Info b)
		{
			if (a.row.Owner != b.row.Owner)
			{
				uint owner = a.row.Owner;
				return owner.CompareTo(b.row.Owner);
			}
			ushort number = a.row.Number;
			return number.CompareTo(b.row.Number);
		});
		interfaceImplInfos.Sort(delegate(SortedRows<InterfaceImpl, RawInterfaceImplRow>.Info a, SortedRows<InterfaceImpl, RawInterfaceImplRow>.Info b)
		{
			uint @class = a.row.Class;
			return @class.CompareTo(b.row.Class);
		});
		tablesHeap.ClassLayoutTable.IsSorted = true;
		tablesHeap.ConstantTable.IsSorted = true;
		tablesHeap.DeclSecurityTable.IsSorted = true;
		tablesHeap.FieldLayoutTable.IsSorted = true;
		tablesHeap.FieldMarshalTable.IsSorted = true;
		tablesHeap.FieldRVATable.IsSorted = true;
		tablesHeap.GenericParamTable.IsSorted = true;
		tablesHeap.ImplMapTable.IsSorted = true;
		tablesHeap.InterfaceImplTable.IsSorted = true;
		tablesHeap.MethodImplTable.IsSorted = true;
		tablesHeap.MethodSemanticsTable.IsSorted = true;
		tablesHeap.NestedClassTable.IsSorted = true;
		tablesHeap.EventMapTable.IsSorted = true;
		tablesHeap.PropertyMapTable.IsSorted = true;
		foreach (SortedRows<TypeDef, RawClassLayoutRow>.Info info in classLayoutInfos.infos)
		{
			tablesHeap.ClassLayoutTable.Create(info.row);
		}
		foreach (SortedRows<IHasConstant, RawConstantRow>.Info info2 in hasConstantInfos.infos)
		{
			tablesHeap.ConstantTable.Create(info2.row);
		}
		foreach (SortedRows<DeclSecurity, RawDeclSecurityRow>.Info info3 in declSecurityInfos.infos)
		{
			tablesHeap.DeclSecurityTable.Create(info3.row);
		}
		foreach (SortedRows<FieldDef, RawFieldLayoutRow>.Info info4 in fieldLayoutInfos.infos)
		{
			tablesHeap.FieldLayoutTable.Create(info4.row);
		}
		foreach (SortedRows<IHasFieldMarshal, RawFieldMarshalRow>.Info info5 in fieldMarshalInfos.infos)
		{
			tablesHeap.FieldMarshalTable.Create(info5.row);
		}
		foreach (SortedRows<FieldDef, RawFieldRVARow>.Info info6 in fieldRVAInfos.infos)
		{
			tablesHeap.FieldRVATable.Create(info6.row);
		}
		foreach (SortedRows<GenericParam, RawGenericParamRow>.Info info7 in genericParamInfos.infos)
		{
			tablesHeap.GenericParamTable.Create(info7.row);
		}
		foreach (SortedRows<IMemberForwarded, RawImplMapRow>.Info info8 in implMapInfos.infos)
		{
			tablesHeap.ImplMapTable.Create(info8.row);
		}
		foreach (SortedRows<InterfaceImpl, RawInterfaceImplRow>.Info info9 in interfaceImplInfos.infos)
		{
			tablesHeap.InterfaceImplTable.Create(info9.row);
		}
		foreach (SortedRows<MethodDef, RawMethodImplRow>.Info info10 in methodImplInfos.infos)
		{
			tablesHeap.MethodImplTable.Create(info10.row);
		}
		foreach (SortedRows<MethodDef, RawMethodSemanticsRow>.Info info11 in methodSemanticsInfos.infos)
		{
			tablesHeap.MethodSemanticsTable.Create(info11.row);
		}
		foreach (SortedRows<TypeDef, RawNestedClassRow>.Info info12 in nestedClassInfos.infos)
		{
			tablesHeap.NestedClassTable.Create(info12.row);
		}
		foreach (SortedRows<InterfaceImpl, RawInterfaceImplRow>.Info info13 in interfaceImplInfos.infos)
		{
			if (info13.data.HasCustomAttributes || info13.data.HasCustomDebugInfos)
			{
				uint rid = interfaceImplInfos.Rid(info13.data);
				AddCustomAttributes(Table.InterfaceImpl, rid, info13.data);
				AddCustomDebugInformationList(Table.InterfaceImpl, rid, info13.data);
			}
		}
		foreach (SortedRows<DeclSecurity, RawDeclSecurityRow>.Info info14 in declSecurityInfos.infos)
		{
			if (info14.data.HasCustomAttributes || info14.data.HasCustomDebugInfos)
			{
				uint rid2 = declSecurityInfos.Rid(info14.data);
				AddCustomAttributes(Table.DeclSecurity, rid2, info14.data);
				AddCustomDebugInformationList(Table.DeclSecurity, rid2, info14.data);
			}
		}
		foreach (SortedRows<GenericParam, RawGenericParamRow>.Info info15 in genericParamInfos.infos)
		{
			if (info15.data.HasCustomAttributes || info15.data.HasCustomDebugInfos)
			{
				uint rid3 = genericParamInfos.Rid(info15.data);
				AddCustomAttributes(Table.GenericParam, rid3, info15.data);
				AddCustomDebugInformationList(Table.GenericParam, rid3, info15.data);
			}
		}
	}

	private void InitializeGenericParamConstraintTable()
	{
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef typeDef in array)
		{
			if (typeDef == null)
			{
				continue;
			}
			using (errorContext.SetSource(typeDef))
			{
				AddGenericParamConstraints(typeDef.GenericParameters);
				IList<MethodDef> methods = typeDef.Methods;
				int count = methods.Count;
				for (int j = 0; j < count; j++)
				{
					MethodDef methodDef = methods[j];
					if (methodDef != null)
					{
						using (errorContext.SetSource(methodDef))
						{
							AddGenericParamConstraints(methodDef.GenericParameters);
						}
					}
				}
			}
		}
		genericParamConstraintInfos.Sort(delegate(SortedRows<GenericParamConstraint, RawGenericParamConstraintRow>.Info a, SortedRows<GenericParamConstraint, RawGenericParamConstraintRow>.Info b)
		{
			uint owner = a.row.Owner;
			return owner.CompareTo(b.row.Owner);
		});
		tablesHeap.GenericParamConstraintTable.IsSorted = true;
		foreach (SortedRows<GenericParamConstraint, RawGenericParamConstraintRow>.Info info in genericParamConstraintInfos.infos)
		{
			tablesHeap.GenericParamConstraintTable.Create(info.row);
		}
		foreach (SortedRows<GenericParamConstraint, RawGenericParamConstraintRow>.Info info2 in genericParamConstraintInfos.infos)
		{
			if (info2.data.HasCustomAttributes || info2.data.HasCustomDebugInfos)
			{
				uint rid = genericParamConstraintInfos.Rid(info2.data);
				AddCustomAttributes(Table.GenericParamConstraint, rid, info2.data);
				AddCustomDebugInformationList(Table.GenericParamConstraint, rid, info2.data);
			}
		}
	}

	private void InitializeCustomAttributeAndCustomDebugInfoTables()
	{
		customAttributeInfos.Sort(delegate(SortedRows<CustomAttribute, RawCustomAttributeRow>.Info a, SortedRows<CustomAttribute, RawCustomAttributeRow>.Info b)
		{
			uint parent2 = a.row.Parent;
			return parent2.CompareTo(b.row.Parent);
		});
		tablesHeap.CustomAttributeTable.IsSorted = true;
		foreach (SortedRows<CustomAttribute, RawCustomAttributeRow>.Info info in customAttributeInfos.infos)
		{
			tablesHeap.CustomAttributeTable.Create(info.row);
		}
		if (debugMetadata == null)
		{
			return;
		}
		debugMetadata.stateMachineMethodInfos.Sort(delegate(SortedRows<PdbCustomDebugInfo, RawStateMachineMethodRow>.Info a, SortedRows<PdbCustomDebugInfo, RawStateMachineMethodRow>.Info b)
		{
			uint moveNextMethod = a.row.MoveNextMethod;
			return moveNextMethod.CompareTo(b.row.MoveNextMethod);
		});
		debugMetadata.tablesHeap.StateMachineMethodTable.IsSorted = true;
		foreach (SortedRows<PdbCustomDebugInfo, RawStateMachineMethodRow>.Info info2 in debugMetadata.stateMachineMethodInfos.infos)
		{
			debugMetadata.tablesHeap.StateMachineMethodTable.Create(info2.row);
		}
		debugMetadata.customDebugInfos.Sort(delegate(SortedRows<PdbCustomDebugInfo, RawCustomDebugInformationRow>.Info a, SortedRows<PdbCustomDebugInfo, RawCustomDebugInformationRow>.Info b)
		{
			uint parent = a.row.Parent;
			return parent.CompareTo(b.row.Parent);
		});
		debugMetadata.tablesHeap.CustomDebugInformationTable.IsSorted = true;
		foreach (SortedRows<PdbCustomDebugInfo, RawCustomDebugInformationRow>.Info info3 in debugMetadata.customDebugInfos.infos)
		{
			debugMetadata.tablesHeap.CustomDebugInformationTable.Create(info3.row);
		}
	}

	private void WriteMethodBodies()
	{
		if (NoMethodBodies)
		{
			return;
		}
		int numberOfMethods = NumberOfMethods;
		int num = 0;
		int num2 = 0;
		int num3 = numberOfMethods / 40;
		NormalMetadata normalMetadata = debugMetadata;
		MethodBodyChunks methodBodyChunks = methodBodies;
		Dictionary<MethodDef, MethodBody> dictionary = methodToBody;
		List<MethodScopeDebugInfo> list;
		List<PdbScope> list2;
		SerializerMethodContext ctx;
		if (normalMetadata == null)
		{
			list = null;
			list2 = null;
			ctx = null;
		}
		else
		{
			list = new List<MethodScopeDebugInfo>();
			list2 = new List<PdbScope>();
			ctx = AllocSerializerMethodContext();
		}
		bool keepOldMaxStack = KeepOldMaxStack;
		MethodBodyWriter methodBodyWriter = new MethodBodyWriter(this);
		TypeDef[] array = allTypeDefs;
		foreach (TypeDef typeDef in array)
		{
			if (typeDef == null)
			{
				continue;
			}
			using (errorContext.SetSource(typeDef))
			{
				IList<MethodDef> methods = typeDef.Methods;
				for (int j = 0; j < methods.Count; j++)
				{
					MethodDef methodDef = methods[j];
					if (methodDef == null)
					{
						continue;
					}
					using (errorContext.SetSource(methodDef))
					{
						if (num++ == num3 && num2 < 40)
						{
							RaiseProgress(dnlib.DotNet.Writer.MetadataEvent.BeginWriteMethodBodies, (double)num / (double)numberOfMethods);
							num2++;
							num3 = (int)((double)numberOfMethods / 40.0 * (double)(num2 + 1));
						}
						uint localVarSigToken = 0u;
						CilBody body = methodDef.Body;
						if (body != null)
						{
							if (body.Instructions.Count != 0 || body.Variables.Count != 0)
							{
								methodBodyWriter.Reset(body, keepOldMaxStack || body.KeepOldMaxStack);
								methodBodyWriter.Write();
								RVA rVA = methodDef.RVA;
								uint metadataBodySize = body.MetadataBodySize;
								MethodBody value = methodBodyChunks.Add(new MethodBody(methodBodyWriter.Code, methodBodyWriter.ExtraSections, methodBodyWriter.LocalVarSigTok), rVA, metadataBodySize);
								dictionary[methodDef] = value;
								localVarSigToken = methodBodyWriter.LocalVarSigTok;
							}
						}
						else
						{
							NativeMethodBody nativeBody = methodDef.NativeBody;
							if (nativeBody != null)
							{
								methodToNativeBody[methodDef] = nativeBody;
							}
							else if (methodDef.MethodBody != null)
							{
								Error("Unsupported method body");
							}
						}
						if (normalMetadata == null)
						{
							continue;
						}
						uint rid = GetRid(methodDef);
						if (body != null)
						{
							PdbMethod pdbMethod = body.PdbMethod;
							if (pdbMethod != null && !IsEmptyRootScope(body, pdbMethod.Scope))
							{
								ctx.SetBody(methodDef);
								list2.Add(pdbMethod.Scope);
								while (list2.Count > 0)
								{
									PdbScope pdbScope = list2[list2.Count - 1];
									list2.RemoveAt(list2.Count - 1);
									list2.AddRange(pdbScope.Scopes);
									uint num4 = ctx.GetOffset(pdbScope.Start);
									uint num5 = ctx.GetOffset(pdbScope.End);
									list.Add(new MethodScopeDebugInfo
									{
										MethodRid = rid,
										Scope = pdbScope,
										ScopeStart = num4,
										ScopeLength = num5 - num4
									});
								}
							}
						}
						AddCustomDebugInformationList(methodDef, rid, localVarSigToken);
					}
				}
			}
		}
		if (normalMetadata != null)
		{
			list.Sort(delegate(MethodScopeDebugInfo a, MethodScopeDebugInfo b)
			{
				int num6 = a.MethodRid.CompareTo(b.MethodRid);
				if (num6 != 0)
				{
					return num6;
				}
				num6 = a.ScopeStart.CompareTo(b.ScopeStart);
				return (num6 != 0) ? num6 : b.ScopeLength.CompareTo(a.ScopeLength);
			});
			foreach (MethodScopeDebugInfo item in list)
			{
				uint rid2 = (uint)(normalMetadata.localScopeInfos.infos.Count + 1);
				RawLocalScopeRow row = new RawLocalScopeRow(item.MethodRid, AddImportScope(item.Scope.ImportScope), (uint)(normalMetadata.tablesHeap.LocalVariableTable.Rows + 1), (uint)(normalMetadata.tablesHeap.LocalConstantTable.Rows + 1), item.ScopeStart, item.ScopeLength);
				normalMetadata.localScopeInfos.Add(item.Scope, row);
				IList<PdbLocal> variables = item.Scope.Variables;
				int count = variables.Count;
				for (int k = 0; k < count; k++)
				{
					PdbLocal local = variables[k];
					AddLocalVariable(local);
				}
				IList<PdbConstant> list3 = item.Scope.Constants;
				count = list3.Count;
				for (int l = 0; l < count; l++)
				{
					PdbConstant constant = list3[l];
					AddLocalConstant(constant);
				}
				AddCustomDebugInformationList(Table.LocalScope, rid2, item.Scope.CustomDebugInfos);
			}
			normalMetadata.tablesHeap.LocalScopeTable.IsSorted = true;
			foreach (SortedRows<PdbScope, RawLocalScopeRow>.Info info in normalMetadata.localScopeInfos.infos)
			{
				normalMetadata.tablesHeap.LocalScopeTable.Create(info.row);
			}
		}
		if (ctx != null)
		{
			Free(ref ctx);
		}
	}

	private static bool IsEmptyRootScope(CilBody cilBody, PdbScope scope)
	{
		if (scope.Variables.Count != 0)
		{
			return false;
		}
		if (scope.Constants.Count != 0)
		{
			return false;
		}
		if (scope.Namespaces.Count != 0)
		{
			return false;
		}
		if (scope.ImportScope != null)
		{
			return false;
		}
		if (scope.Scopes.Count != 0)
		{
			return false;
		}
		if (scope.CustomDebugInfos.Count != 0)
		{
			return false;
		}
		if (scope.End != null)
		{
			return false;
		}
		if (cilBody.Instructions.Count != 0 && cilBody.Instructions[0] != scope.Start)
		{
			return false;
		}
		return true;
	}

	protected static bool IsEmpty<T>(IList<T> list) where T : class
	{
		if (list == null)
		{
			return true;
		}
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (list[i] != null)
			{
				return false;
			}
		}
		return true;
	}

	public MDToken GetToken(object o)
	{
		if (o is IMDTokenProvider { MDToken: var mDToken } iMDTokenProvider)
		{
			return new MDToken(mDToken.Table, AddMDTokenProvider(iMDTokenProvider));
		}
		if (o is string s)
		{
			return new MDToken((Table)112, usHeap.Add(s));
		}
		if (o is MethodSig methodSig)
		{
			return new MDToken(Table.StandAloneSig, AddStandAloneSig(methodSig, methodSig.OriginalToken));
		}
		if (o is FieldSig fieldSig)
		{
			return new MDToken(Table.StandAloneSig, AddStandAloneSig(fieldSig, 0u));
		}
		if (o == null)
		{
			Error("Instruction operand is null");
		}
		else
		{
			Error("Invalid instruction operand");
		}
		return new MDToken((Table)255, 16777215);
	}

	public virtual MDToken GetToken(IList<TypeSig> locals, uint origToken)
	{
		if (locals == null || locals.Count == 0)
		{
			return new MDToken(Table.Module, 0);
		}
		RawStandAloneSigRow row = new RawStandAloneSigRow(GetSignature(new LocalSig(locals, dummy: false)));
		uint rid = tablesHeap.StandAloneSigTable.Add(row);
		return new MDToken(Table.StandAloneSig, rid);
	}

	protected virtual uint AddStandAloneSig(MethodSig methodSig, uint origToken)
	{
		if (methodSig == null)
		{
			Error("StandAloneSig: MethodSig is null");
			return 0u;
		}
		RawStandAloneSigRow row = new RawStandAloneSigRow(GetSignature(methodSig));
		return tablesHeap.StandAloneSigTable.Add(row);
	}

	protected virtual uint AddStandAloneSig(FieldSig fieldSig, uint origToken)
	{
		if (fieldSig == null)
		{
			Error("StandAloneSig: FieldSig is null");
			return 0u;
		}
		RawStandAloneSigRow row = new RawStandAloneSigRow(GetSignature(fieldSig));
		return tablesHeap.StandAloneSigTable.Add(row);
	}

	private uint AddMDTokenProvider(IMDTokenProvider tp)
	{
		if (tp != null)
		{
			switch (tp.MDToken.Table)
			{
			case Table.Module:
				return AddModule((ModuleDef)tp);
			case Table.TypeRef:
				return AddTypeRef((TypeRef)tp);
			case Table.TypeDef:
				return GetRid((TypeDef)tp);
			case Table.Field:
				return GetRid((FieldDef)tp);
			case Table.Method:
				return GetRid((MethodDef)tp);
			case Table.Param:
				return GetRid((ParamDef)tp);
			case Table.MemberRef:
				return AddMemberRef((MemberRef)tp);
			case Table.StandAloneSig:
				return AddStandAloneSig((StandAloneSig)tp);
			case Table.Event:
				return GetRid((EventDef)tp);
			case Table.Property:
				return GetRid((PropertyDef)tp);
			case Table.ModuleRef:
				return AddModuleRef((ModuleRef)tp);
			case Table.TypeSpec:
				return AddTypeSpec((TypeSpec)tp);
			case Table.Assembly:
				return AddAssembly((AssemblyDef)tp, null);
			case Table.AssemblyRef:
				return AddAssemblyRef((AssemblyRef)tp);
			case Table.File:
				return AddFile((FileDef)tp);
			case Table.ExportedType:
				return AddExportedType((ExportedType)tp);
			case Table.MethodSpec:
				return AddMethodSpec((MethodSpec)tp);
			}
		}
		if (tp == null)
		{
			Error("IMDTokenProvider is null");
		}
		else
		{
			Error("Invalid IMDTokenProvider");
		}
		return 0u;
	}

	protected uint AddTypeDefOrRef(ITypeDefOrRef tdr)
	{
		if (tdr == null)
		{
			Error("TypeDefOrRef is null");
			return 0u;
		}
		MDToken token = new MDToken(tdr.MDToken.Table, AddMDTokenProvider(tdr));
		if (!CodedToken.TypeDefOrRef.Encode(token, out var codedToken))
		{
			Error("Can't encode TypeDefOrRef token 0x{0:X8}.", token.Raw);
			return 0u;
		}
		return codedToken;
	}

	protected uint AddResolutionScope(IResolutionScope rs)
	{
		if (rs == null)
		{
			return 0u;
		}
		MDToken token = new MDToken(rs.MDToken.Table, AddMDTokenProvider(rs));
		if (!CodedToken.ResolutionScope.Encode(token, out var codedToken))
		{
			Error("Can't encode ResolutionScope token 0x{0:X8}.", token.Raw);
			return 0u;
		}
		return codedToken;
	}

	protected uint AddMethodDefOrRef(IMethodDefOrRef mdr)
	{
		if (mdr == null)
		{
			Error("MethodDefOrRef is null");
			return 0u;
		}
		MDToken token = new MDToken(mdr.MDToken.Table, AddMDTokenProvider(mdr));
		if (!CodedToken.MethodDefOrRef.Encode(token, out var codedToken))
		{
			Error("Can't encode MethodDefOrRef token 0x{0:X8}.", token.Raw);
			return 0u;
		}
		return codedToken;
	}

	protected uint AddMemberRefParent(IMemberRefParent parent)
	{
		if (parent == null)
		{
			Error("MemberRefParent is null");
			return 0u;
		}
		MDToken token = new MDToken(parent.MDToken.Table, AddMDTokenProvider(parent));
		if (!CodedToken.MemberRefParent.Encode(token, out var codedToken))
		{
			Error("Can't encode MemberRefParent token 0x{0:X8}.", token.Raw);
			return 0u;
		}
		return codedToken;
	}

	protected uint AddImplementation(IImplementation impl)
	{
		if (impl == null)
		{
			Error("Implementation is null");
			return 0u;
		}
		MDToken token = new MDToken(impl.MDToken.Table, AddMDTokenProvider(impl));
		if (!CodedToken.Implementation.Encode(token, out var codedToken))
		{
			Error("Can't encode Implementation token 0x{0:X8}.", token.Raw);
			return 0u;
		}
		return codedToken;
	}

	protected uint AddCustomAttributeType(ICustomAttributeType cat)
	{
		if (cat == null)
		{
			Error("CustomAttributeType is null");
			return 0u;
		}
		MDToken token = new MDToken(cat.MDToken.Table, AddMDTokenProvider(cat));
		if (!CodedToken.CustomAttributeType.Encode(token, out var codedToken))
		{
			Error("Can't encode CustomAttributeType token 0x{0:X8}.", token.Raw);
			return 0u;
		}
		return codedToken;
	}

	protected void AddNestedType(TypeDef nestedType, TypeDef declaringType)
	{
		if (nestedType != null && declaringType != null)
		{
			uint rid = GetRid(nestedType);
			uint rid2 = GetRid(declaringType);
			if (rid != 0 && rid2 != 0)
			{
				RawNestedClassRow row = new RawNestedClassRow(rid, rid2);
				nestedClassInfos.Add(declaringType, row);
			}
		}
	}

	protected uint AddModule(ModuleDef module)
	{
		if (module == null)
		{
			Error("Module is null");
			return 0u;
		}
		if (this.module != module)
		{
			Error("Module '{0}' must be referenced with a ModuleRef, not a ModuleDef.", module);
		}
		if (moduleDefInfos.TryGetRid(module, out var rid))
		{
			return rid;
		}
		RawModuleRow row = new RawModuleRow(module.Generation, stringsHeap.Add(module.Name), guidHeap.Add(module.Mvid), guidHeap.Add(module.EncId), guidHeap.Add(module.EncBaseId));
		rid = tablesHeap.ModuleTable.Add(row);
		moduleDefInfos.Add(module, rid);
		AddCustomAttributes(Table.Module, rid, module);
		AddCustomDebugInformationList(Table.Module, rid, module);
		return rid;
	}

	protected uint AddModuleRef(ModuleRef modRef)
	{
		if (modRef == null)
		{
			Error("ModuleRef is null");
			return 0u;
		}
		if (moduleRefInfos.TryGetRid(modRef, out var rid))
		{
			return rid;
		}
		RawModuleRefRow row = new RawModuleRefRow(stringsHeap.Add(modRef.Name));
		rid = tablesHeap.ModuleRefTable.Add(row);
		moduleRefInfos.Add(modRef, rid);
		AddCustomAttributes(Table.ModuleRef, rid, modRef);
		AddCustomDebugInformationList(Table.ModuleRef, rid, modRef);
		return rid;
	}

	protected uint AddAssemblyRef(AssemblyRef asmRef)
	{
		if (asmRef == null)
		{
			Error("AssemblyRef is null");
			return 0u;
		}
		if (assemblyRefInfos.TryGetRid(asmRef, out var rid))
		{
			return rid;
		}
		Version version = Utils.CreateVersionWithNoUndefinedValues(asmRef.Version);
		RawAssemblyRefRow row = new RawAssemblyRefRow((ushort)version.Major, (ushort)version.Minor, (ushort)version.Build, (ushort)version.Revision, (uint)asmRef.Attributes, blobHeap.Add(PublicKeyBase.GetRawData(asmRef.PublicKeyOrToken)), stringsHeap.Add(asmRef.Name), stringsHeap.Add(asmRef.Culture), blobHeap.Add(asmRef.Hash));
		rid = tablesHeap.AssemblyRefTable.Add(row);
		assemblyRefInfos.Add(asmRef, rid);
		AddCustomAttributes(Table.AssemblyRef, rid, asmRef);
		AddCustomDebugInformationList(Table.AssemblyRef, rid, asmRef);
		return rid;
	}

	protected uint AddAssembly(AssemblyDef asm, byte[] publicKey)
	{
		if (asm == null)
		{
			Error("Assembly is null");
			return 0u;
		}
		if (assemblyInfos.TryGetRid(asm, out var rid))
		{
			return rid;
		}
		AssemblyAttributes assemblyAttributes = asm.Attributes;
		if (publicKey != null)
		{
			assemblyAttributes |= AssemblyAttributes.PublicKey;
		}
		else
		{
			publicKey = PublicKeyBase.GetRawData(asm.PublicKeyOrToken);
		}
		Version version = Utils.CreateVersionWithNoUndefinedValues(asm.Version);
		RawAssemblyRow row = new RawAssemblyRow((uint)asm.HashAlgorithm, (ushort)version.Major, (ushort)version.Minor, (ushort)version.Build, (ushort)version.Revision, (uint)assemblyAttributes, blobHeap.Add(publicKey), stringsHeap.Add(asm.Name), stringsHeap.Add(asm.Culture));
		rid = tablesHeap.AssemblyTable.Add(row);
		assemblyInfos.Add(asm, rid);
		AddDeclSecurities(new MDToken(Table.Assembly, rid), asm.DeclSecurities);
		AddCustomAttributes(Table.Assembly, rid, asm);
		AddCustomDebugInformationList(Table.Assembly, rid, asm);
		return rid;
	}

	protected void AddGenericParams(MDToken token, IList<GenericParam> gps)
	{
		if (gps != null)
		{
			int count = gps.Count;
			for (int i = 0; i < count; i++)
			{
				AddGenericParam(token, gps[i]);
			}
		}
	}

	protected void AddGenericParam(MDToken owner, GenericParam gp)
	{
		if (gp == null)
		{
			Error("GenericParam is null");
			return;
		}
		if (!CodedToken.TypeOrMethodDef.Encode(owner, out var codedToken))
		{
			Error("Can't encode TypeOrMethodDef token 0x{0:X8}.", owner.Raw);
			codedToken = 0u;
		}
		RawGenericParamRow row = new RawGenericParamRow(gp.Number, (ushort)gp.Flags, codedToken, stringsHeap.Add(gp.Name), (gp.Kind != null) ? AddTypeDefOrRef(gp.Kind) : 0u);
		genericParamInfos.Add(gp, row);
	}

	private void AddGenericParamConstraints(IList<GenericParam> gps)
	{
		if (gps == null)
		{
			return;
		}
		int count = gps.Count;
		for (int i = 0; i < count; i++)
		{
			GenericParam genericParam = gps[i];
			if (genericParam != null)
			{
				uint gpRid = genericParamInfos.Rid(genericParam);
				AddGenericParamConstraints(gpRid, genericParam.GenericParamConstraints);
			}
		}
	}

	protected void AddGenericParamConstraints(uint gpRid, IList<GenericParamConstraint> constraints)
	{
		if (constraints != null)
		{
			int count = constraints.Count;
			for (int i = 0; i < count; i++)
			{
				AddGenericParamConstraint(gpRid, constraints[i]);
			}
		}
	}

	protected void AddGenericParamConstraint(uint gpRid, GenericParamConstraint gpc)
	{
		if (gpc == null)
		{
			Error("GenericParamConstraint is null");
			return;
		}
		RawGenericParamConstraintRow row = new RawGenericParamConstraintRow(gpRid, AddTypeDefOrRef(gpc.Constraint));
		genericParamConstraintInfos.Add(gpc, row);
	}

	protected void AddInterfaceImpls(uint typeDefRid, IList<InterfaceImpl> ifaces)
	{
		int count = ifaces.Count;
		for (int i = 0; i < count; i++)
		{
			InterfaceImpl interfaceImpl = ifaces[i];
			if (interfaceImpl != null)
			{
				RawInterfaceImplRow row = new RawInterfaceImplRow(typeDefRid, AddTypeDefOrRef(interfaceImpl.Interface));
				interfaceImplInfos.Add(interfaceImpl, row);
			}
		}
	}

	protected void AddFieldLayout(FieldDef field)
	{
		if (field != null && field.FieldOffset.HasValue)
		{
			uint rid = GetRid(field);
			RawFieldLayoutRow row = new RawFieldLayoutRow(field.FieldOffset.Value, rid);
			fieldLayoutInfos.Add(field, row);
		}
	}

	protected void AddFieldMarshal(MDToken parent, IHasFieldMarshal hfm)
	{
		if (hfm != null && hfm.MarshalType != null)
		{
			MarshalType marshalType = hfm.MarshalType;
			if (!CodedToken.HasFieldMarshal.Encode(parent, out var codedToken))
			{
				Error("Can't encode HasFieldMarshal token 0x{0:X8}.", parent.Raw);
				codedToken = 0u;
			}
			RawFieldMarshalRow row = new RawFieldMarshalRow(codedToken, blobHeap.Add(MarshalBlobWriter.Write(module, marshalType, this, OptimizeCustomAttributeSerializedTypeNames)));
			fieldMarshalInfos.Add(hfm, row);
		}
	}

	protected void AddFieldRVA(FieldDef field)
	{
		if (NoFieldData)
		{
			return;
		}
		if (field.RVA != 0 && KeepFieldRVA)
		{
			uint rid = GetRid(field);
			RawFieldRVARow row = new RawFieldRVARow((uint)field.RVA, rid);
			fieldRVAInfos.Add(field, row);
		}
		else
		{
			if (field == null || field.InitialValue == null)
			{
				return;
			}
			byte[] initialValue = field.InitialValue;
			if (!VerifyFieldSize(field, initialValue.Length))
			{
				Error("Field '{0}' (0x{1:X8}) initial value size != size of field type.", field, field.MDToken.Raw);
			}
			uint rid2 = GetRid(field);
			uint num = 8u;
			if (field.FieldType is TypeDefOrRefSig typeDefOrRefSig)
			{
				ClassLayout classLayout = typeDefOrRefSig.TypeDef?.ClassLayout;
				if (classLayout != null)
				{
					num = Math.Max(num, Utils.RoundToNextPowerOfTwo(classLayout.PackingSize));
				}
			}
			ByteArrayChunk value = constants.Add(new ByteArrayChunk(initialValue, num), num);
			fieldToInitialValue[field] = value;
			RawFieldRVARow row2 = new RawFieldRVARow(0u, rid2);
			fieldRVAInfos.Add(field, row2);
		}
	}

	private static bool VerifyFieldSize(FieldDef field, int size)
	{
		if (field == null)
		{
			return false;
		}
		if (field.FieldSig == null)
		{
			return false;
		}
		return field.GetFieldSize() == size;
	}

	protected void AddImplMap(MDToken parent, IMemberForwarded mf)
	{
		if (mf != null && mf.ImplMap != null)
		{
			ImplMap implMap = mf.ImplMap;
			if (!CodedToken.MemberForwarded.Encode(parent, out var codedToken))
			{
				Error("Can't encode MemberForwarded token 0x{0:X8}.", parent.Raw);
				codedToken = 0u;
			}
			RawImplMapRow row = new RawImplMapRow((ushort)implMap.Attributes, codedToken, stringsHeap.Add(implMap.Name), AddModuleRef(implMap.Module));
			implMapInfos.Add(mf, row);
		}
	}

	protected void AddConstant(MDToken parent, IHasConstant hc)
	{
		if (hc != null && hc.Constant != null)
		{
			Constant constant = hc.Constant;
			if (!CodedToken.HasConstant.Encode(parent, out var codedToken))
			{
				Error("Can't encode HasConstant token 0x{0:X8}.", parent.Raw);
				codedToken = 0u;
			}
			RawConstantRow row = new RawConstantRow((byte)constant.Type, 0, codedToken, blobHeap.Add(GetConstantValueAsByteArray(constant.Type, constant.Value)));
			hasConstantInfos.Add(hc, row);
		}
	}

	private byte[] GetConstantValueAsByteArray(ElementType etype, object o)
	{
		if (o == null)
		{
			if (etype == ElementType.Class)
			{
				return constantClassByteArray;
			}
			Error("Constant is null");
			return constantDefaultByteArray;
		}
		TypeCode typeCode = Type.GetTypeCode(o.GetType());
		switch (typeCode)
		{
		case TypeCode.Boolean:
			VerifyConstantType(etype, ElementType.Boolean);
			return BitConverter.GetBytes((bool)o);
		case TypeCode.Char:
			VerifyConstantType(etype, ElementType.Char);
			return BitConverter.GetBytes((char)o);
		case TypeCode.SByte:
			VerifyConstantType(etype, ElementType.I1);
			return new byte[1] { (byte)(sbyte)o };
		case TypeCode.Byte:
			VerifyConstantType(etype, ElementType.U1);
			return new byte[1] { (byte)o };
		case TypeCode.Int16:
			VerifyConstantType(etype, ElementType.I2);
			return BitConverter.GetBytes((short)o);
		case TypeCode.UInt16:
			VerifyConstantType(etype, ElementType.U2);
			return BitConverter.GetBytes((ushort)o);
		case TypeCode.Int32:
			VerifyConstantType(etype, ElementType.I4);
			return BitConverter.GetBytes((int)o);
		case TypeCode.UInt32:
			VerifyConstantType(etype, ElementType.U4);
			return BitConverter.GetBytes((uint)o);
		case TypeCode.Int64:
			VerifyConstantType(etype, ElementType.I8);
			return BitConverter.GetBytes((long)o);
		case TypeCode.UInt64:
			VerifyConstantType(etype, ElementType.U8);
			return BitConverter.GetBytes((ulong)o);
		case TypeCode.Single:
			VerifyConstantType(etype, ElementType.R4);
			return BitConverter.GetBytes((float)o);
		case TypeCode.Double:
			VerifyConstantType(etype, ElementType.R8);
			return BitConverter.GetBytes((double)o);
		case TypeCode.String:
			VerifyConstantType(etype, ElementType.String);
			return Encoding.Unicode.GetBytes((string)o);
		default:
			Error("Invalid constant type: {0}", typeCode);
			return constantDefaultByteArray;
		}
	}

	private void VerifyConstantType(ElementType realType, ElementType expectedType)
	{
		if (realType != expectedType)
		{
			Error("Constant value's type is the wrong type: {0} != {1}", realType, expectedType);
		}
	}

	protected void AddDeclSecurities(MDToken parent, IList<DeclSecurity> declSecurities)
	{
		if (declSecurities == null)
		{
			return;
		}
		if (!CodedToken.HasDeclSecurity.Encode(parent, out var codedToken))
		{
			Error("Can't encode HasDeclSecurity token 0x{0:X8}.", parent.Raw);
			codedToken = 0u;
		}
		DataWriterContext ctx = AllocBinaryWriterContext();
		int count = declSecurities.Count;
		for (int i = 0; i < count; i++)
		{
			DeclSecurity declSecurity = declSecurities[i];
			if (declSecurity != null)
			{
				RawDeclSecurityRow row = new RawDeclSecurityRow((short)declSecurity.Action, codedToken, blobHeap.Add(DeclSecurityWriter.Write(module, declSecurity.SecurityAttributes, this, OptimizeCustomAttributeSerializedTypeNames, ctx)));
				declSecurityInfos.Add(declSecurity, row);
			}
		}
		Free(ref ctx);
	}

	protected void AddMethodSemantics(EventDef evt)
	{
		if (evt == null)
		{
			Error("Event is null");
			return;
		}
		uint rid = GetRid(evt);
		if (rid != 0)
		{
			MDToken owner = new MDToken(Table.Event, rid);
			AddMethodSemantics(owner, evt.AddMethod, MethodSemanticsAttributes.AddOn);
			AddMethodSemantics(owner, evt.RemoveMethod, MethodSemanticsAttributes.RemoveOn);
			AddMethodSemantics(owner, evt.InvokeMethod, MethodSemanticsAttributes.Fire);
			AddMethodSemantics(owner, evt.OtherMethods, MethodSemanticsAttributes.Other);
		}
	}

	protected void AddMethodSemantics(PropertyDef prop)
	{
		if (prop == null)
		{
			Error("Property is null");
			return;
		}
		uint rid = GetRid(prop);
		if (rid != 0)
		{
			MDToken owner = new MDToken(Table.Property, rid);
			AddMethodSemantics(owner, prop.GetMethods, MethodSemanticsAttributes.Getter);
			AddMethodSemantics(owner, prop.SetMethods, MethodSemanticsAttributes.Setter);
			AddMethodSemantics(owner, prop.OtherMethods, MethodSemanticsAttributes.Other);
		}
	}

	private void AddMethodSemantics(MDToken owner, IList<MethodDef> methods, MethodSemanticsAttributes attrs)
	{
		if (methods != null)
		{
			int count = methods.Count;
			for (int i = 0; i < count; i++)
			{
				AddMethodSemantics(owner, methods[i], attrs);
			}
		}
	}

	private void AddMethodSemantics(MDToken owner, MethodDef method, MethodSemanticsAttributes flags)
	{
		if (method == null)
		{
			return;
		}
		uint rid = GetRid(method);
		if (rid != 0)
		{
			if (!CodedToken.HasSemantic.Encode(owner, out var codedToken))
			{
				Error("Can't encode HasSemantic token 0x{0:X8}.", owner.Raw);
				codedToken = 0u;
			}
			RawMethodSemanticsRow row = new RawMethodSemanticsRow((ushort)flags, rid, codedToken);
			methodSemanticsInfos.Add(method, row);
		}
	}

	private void AddMethodImpls(MethodDef method, IList<MethodOverride> overrides)
	{
		if (overrides == null)
		{
			return;
		}
		if (method.DeclaringType == null)
		{
			Error("Method declaring type is null");
		}
		else if (overrides.Count != 0)
		{
			uint rid = GetRid(method.DeclaringType);
			int count = overrides.Count;
			for (int i = 0; i < count; i++)
			{
				MethodOverride methodOverride = overrides[i];
				RawMethodImplRow row = new RawMethodImplRow(rid, AddMethodDefOrRef(methodOverride.MethodBody), AddMethodDefOrRef(methodOverride.MethodDeclaration));
				methodImplInfos.Add(method, row);
			}
		}
	}

	protected void AddClassLayout(TypeDef type)
	{
		if (type != null && type.ClassLayout != null)
		{
			uint rid = GetRid(type);
			ClassLayout classLayout = type.ClassLayout;
			RawClassLayoutRow row = new RawClassLayoutRow(classLayout.PackingSize, classLayout.ClassSize, rid);
			classLayoutInfos.Add(type, row);
		}
	}

	private void AddResources(IList<Resource> resources)
	{
		if (!NoDotNetResources && resources != null)
		{
			int count = resources.Count;
			for (int i = 0; i < count; i++)
			{
				AddResource(resources[i]);
			}
		}
	}

	private void AddResource(Resource resource)
	{
		if (resource is EmbeddedResource er)
		{
			AddEmbeddedResource(er);
			return;
		}
		if (resource is AssemblyLinkedResource alr)
		{
			AddAssemblyLinkedResource(alr);
			return;
		}
		if (resource is LinkedResource lr)
		{
			AddLinkedResource(lr);
			return;
		}
		if (resource == null)
		{
			Error("Resource is null");
			return;
		}
		Error("Invalid resource type: '{0}'.", resource.GetType());
	}

	private uint AddEmbeddedResource(EmbeddedResource er)
	{
		if (er == null)
		{
			Error("EmbeddedResource is null");
			return 0u;
		}
		if (manifestResourceInfos.TryGetRid(er, out var rid))
		{
			return rid;
		}
		RawManifestResourceRow row = new RawManifestResourceRow(netResources.NextOffset, (uint)er.Attributes, stringsHeap.Add(er.Name), 0u);
		rid = tablesHeap.ManifestResourceTable.Add(row);
		manifestResourceInfos.Add(er, rid);
		embeddedResourceToByteArray[er] = netResources.Add(er.CreateReader());
		AddCustomAttributes(Table.ManifestResource, rid, er);
		AddCustomDebugInformationList(Table.ManifestResource, rid, er);
		return rid;
	}

	private uint AddAssemblyLinkedResource(AssemblyLinkedResource alr)
	{
		if (alr == null)
		{
			Error("AssemblyLinkedResource is null");
			return 0u;
		}
		if (manifestResourceInfos.TryGetRid(alr, out var rid))
		{
			return rid;
		}
		RawManifestResourceRow row = new RawManifestResourceRow(0u, (uint)alr.Attributes, stringsHeap.Add(alr.Name), AddImplementation(alr.Assembly));
		rid = tablesHeap.ManifestResourceTable.Add(row);
		manifestResourceInfos.Add(alr, rid);
		AddCustomAttributes(Table.ManifestResource, rid, alr);
		AddCustomDebugInformationList(Table.ManifestResource, rid, alr);
		return rid;
	}

	private uint AddLinkedResource(LinkedResource lr)
	{
		if (lr == null)
		{
			Error("LinkedResource is null");
			return 0u;
		}
		if (manifestResourceInfos.TryGetRid(lr, out var rid))
		{
			return rid;
		}
		RawManifestResourceRow row = new RawManifestResourceRow(0u, (uint)lr.Attributes, stringsHeap.Add(lr.Name), AddImplementation(lr.File));
		rid = tablesHeap.ManifestResourceTable.Add(row);
		manifestResourceInfos.Add(lr, rid);
		AddCustomAttributes(Table.ManifestResource, rid, lr);
		AddCustomDebugInformationList(Table.ManifestResource, rid, lr);
		return rid;
	}

	protected uint AddFile(FileDef file)
	{
		if (file == null)
		{
			Error("FileDef is null");
			return 0u;
		}
		if (fileDefInfos.TryGetRid(file, out var rid))
		{
			return rid;
		}
		RawFileRow row = new RawFileRow((uint)file.Flags, stringsHeap.Add(file.Name), blobHeap.Add(file.HashValue));
		rid = tablesHeap.FileTable.Add(row);
		fileDefInfos.Add(file, rid);
		AddCustomAttributes(Table.File, rid, file);
		AddCustomDebugInformationList(Table.File, rid, file);
		return rid;
	}

	protected uint AddExportedType(ExportedType et)
	{
		if (et == null)
		{
			Error("ExportedType is null");
			return 0u;
		}
		if (exportedTypeInfos.TryGetRid(et, out var rid))
		{
			return rid;
		}
		exportedTypeInfos.Add(et, 0u);
		RawExportedTypeRow row = new RawExportedTypeRow((uint)et.Attributes, et.TypeDefId, stringsHeap.Add(et.TypeName), stringsHeap.Add(et.TypeNamespace), AddImplementation(et.Implementation));
		rid = tablesHeap.ExportedTypeTable.Add(row);
		exportedTypeInfos.SetRid(et, rid);
		AddCustomAttributes(Table.ExportedType, rid, et);
		AddCustomDebugInformationList(Table.ExportedType, rid, et);
		return rid;
	}

	protected uint GetSignature(TypeSig ts, byte[] extraData)
	{
		byte[] blob;
		if (ts == null)
		{
			Error("TypeSig is null");
			blob = null;
		}
		else
		{
			DataWriterContext ctx = AllocBinaryWriterContext();
			blob = SignatureWriter.Write(this, ts, ctx);
			Free(ref ctx);
		}
		AppendExtraData(ref blob, extraData);
		return blobHeap.Add(blob);
	}

	protected uint GetSignature(CallingConventionSig sig)
	{
		if (sig == null)
		{
			Error("CallingConventionSig is null");
			return 0u;
		}
		DataWriterContext ctx = AllocBinaryWriterContext();
		byte[] blob = SignatureWriter.Write(this, sig, ctx);
		Free(ref ctx);
		AppendExtraData(ref blob, sig.ExtraData);
		return blobHeap.Add(blob);
	}

	private void AppendExtraData(ref byte[] blob, byte[] extraData)
	{
		if (PreserveExtraSignatureData && extraData != null && extraData.Length != 0)
		{
			int num = ((blob != null) ? blob.Length : 0);
			Array.Resize(ref blob, num + extraData.Length);
			Array.Copy(extraData, 0, blob, num, extraData.Length);
		}
	}

	protected void AddCustomAttributes(Table table, uint rid, IHasCustomAttribute hca)
	{
		AddCustomAttributes(table, rid, hca.CustomAttributes);
	}

	private void AddCustomAttributes(Table table, uint rid, CustomAttributeCollection caList)
	{
		MDToken token = new MDToken(table, rid);
		int count = caList.Count;
		for (int i = 0; i < count; i++)
		{
			AddCustomAttribute(token, caList[i]);
		}
	}

	private void AddCustomAttribute(MDToken token, CustomAttribute ca)
	{
		if (ca == null)
		{
			Error("Custom attribute is null");
			return;
		}
		if (!CodedToken.HasCustomAttribute.Encode(token, out var codedToken))
		{
			Error("Can't encode HasCustomAttribute token 0x{0:X8}.", token.Raw);
			codedToken = 0u;
		}
		DataWriterContext ctx = AllocBinaryWriterContext();
		byte[] data = CustomAttributeWriter.Write(this, ca, ctx);
		Free(ref ctx);
		RawCustomAttributeRow row = new RawCustomAttributeRow(codedToken, AddCustomAttributeType(ca.Constructor), blobHeap.Add(data));
		customAttributeInfos.Add(ca, row);
	}

	private void AddCustomDebugInformationList(MethodDef method, uint rid, uint localVarSigToken)
	{
		if (debugMetadata != null)
		{
			SerializerMethodContext ctx = AllocSerializerMethodContext();
			ctx.SetBody(method);
			if (method.CustomDebugInfos.Count != 0)
			{
				AddCustomDebugInformationCore(ctx, Table.Method, rid, method.CustomDebugInfos);
			}
			AddMethodDebugInformation(method, rid, localVarSigToken);
			Free(ref ctx);
		}
	}

	private void AddMethodDebugInformation(MethodDef method, uint rid, uint localVarSigToken)
	{
		CilBody body = method.Body;
		if (body == null)
		{
			return;
		}
		GetSingleDocument(body, out var singleDoc, out var firstDoc, out var hasNoSeqPoints);
		if (hasNoSeqPoints)
		{
			return;
		}
		DataWriterContext ctx = AllocBinaryWriterContext();
		MemoryStream outStream = ctx.OutStream;
		DataWriter writer = ctx.Writer;
		outStream.SetLength(0L);
		outStream.Position = 0L;
		writer.WriteCompressedUInt32(localVarSigToken);
		if (singleDoc == null)
		{
			writer.WriteCompressedUInt32(VerifyGetRid(firstDoc));
		}
		IList<Instruction> instructions = body.Instructions;
		PdbDocument pdbDocument = firstDoc;
		uint num = uint.MaxValue;
		int num2 = -1;
		int num3 = 0;
		uint num4 = 0u;
		Instruction instruction = null;
		int num5 = 0;
		while (num5 < instructions.Count)
		{
			instruction = instructions[num5];
			SequencePoint sequencePoint = instruction.SequencePoint;
			if (sequencePoint != null)
			{
				if (sequencePoint.Document == null)
				{
					Error("PDB document is null");
					return;
				}
				if (pdbDocument != sequencePoint.Document)
				{
					pdbDocument = sequencePoint.Document;
					writer.WriteCompressedUInt32(0u);
					writer.WriteCompressedUInt32(VerifyGetRid(pdbDocument));
				}
				if (num == uint.MaxValue)
				{
					writer.WriteCompressedUInt32(num4);
				}
				else
				{
					writer.WriteCompressedUInt32(num4 - num);
				}
				num = num4;
				if (sequencePoint.StartLine == 16707566 && sequencePoint.EndLine == 16707566)
				{
					writer.WriteCompressedUInt32(0u);
					writer.WriteCompressedUInt32(0u);
				}
				else
				{
					uint num6 = (uint)(sequencePoint.EndLine - sequencePoint.StartLine);
					int value = sequencePoint.EndColumn - sequencePoint.StartColumn;
					writer.WriteCompressedUInt32(num6);
					if (num6 == 0)
					{
						writer.WriteCompressedUInt32((uint)value);
					}
					else
					{
						writer.WriteCompressedInt32(value);
					}
					if (num2 < 0)
					{
						writer.WriteCompressedUInt32((uint)sequencePoint.StartLine);
						writer.WriteCompressedUInt32((uint)sequencePoint.StartColumn);
					}
					else
					{
						writer.WriteCompressedInt32(sequencePoint.StartLine - num2);
						writer.WriteCompressedInt32(sequencePoint.StartColumn - num3);
					}
					num2 = sequencePoint.StartLine;
					num3 = sequencePoint.StartColumn;
				}
			}
			num5++;
			num4 += (uint)instruction.GetSize();
		}
		byte[] data = outStream.ToArray();
		RawMethodDebugInformationRow value2 = new RawMethodDebugInformationRow((singleDoc != null) ? AddPdbDocument(singleDoc) : 0u, debugMetadata.blobHeap.Add(data));
		debugMetadata.tablesHeap.MethodDebugInformationTable[rid] = value2;
		debugMetadata.methodDebugInformationInfosUsed = true;
		Free(ref ctx);
	}

	private uint VerifyGetRid(PdbDocument doc)
	{
		if (!debugMetadata.pdbDocumentInfos.TryGetRid(doc, out var rid))
		{
			Error("PDB document has been removed");
			return 0u;
		}
		return rid;
	}

	private static void GetSingleDocument(CilBody body, out PdbDocument singleDoc, out PdbDocument firstDoc, out bool hasNoSeqPoints)
	{
		IList<Instruction> instructions = body.Instructions;
		int num = 0;
		singleDoc = null;
		firstDoc = null;
		for (int i = 0; i < instructions.Count; i++)
		{
			SequencePoint sequencePoint = instructions[i].SequencePoint;
			if (sequencePoint == null)
			{
				continue;
			}
			PdbDocument document = sequencePoint.Document;
			if (document == null)
			{
				continue;
			}
			if (firstDoc == null)
			{
				firstDoc = document;
			}
			if (singleDoc != document)
			{
				singleDoc = document;
				num++;
				if (num > 1)
				{
					break;
				}
			}
		}
		hasNoSeqPoints = num == 0;
		if (num != 1)
		{
			singleDoc = null;
		}
	}

	protected void AddCustomDebugInformationList(Table table, uint rid, IHasCustomDebugInformation hcdi)
	{
		if (debugMetadata != null && hcdi.CustomDebugInfos.Count != 0)
		{
			SerializerMethodContext ctx = AllocSerializerMethodContext();
			ctx.SetBody(null);
			AddCustomDebugInformationCore(ctx, table, rid, hcdi.CustomDebugInfos);
			Free(ref ctx);
		}
	}

	private void AddCustomDebugInformationList(Table table, uint rid, IList<PdbCustomDebugInfo> cdis)
	{
		if (debugMetadata != null && cdis.Count != 0)
		{
			SerializerMethodContext ctx = AllocSerializerMethodContext();
			ctx.SetBody(null);
			AddCustomDebugInformationCore(ctx, table, rid, cdis);
			Free(ref ctx);
		}
	}

	private void AddCustomDebugInformationCore(SerializerMethodContext serializerMethodContext, Table table, uint rid, IList<PdbCustomDebugInfo> cdis)
	{
		MDToken token = new MDToken(table, rid);
		if (!CodedToken.HasCustomDebugInformation.Encode(token, out var codedToken))
		{
			Error("Couldn't encode HasCustomDebugInformation token 0x{0:X8}.", token.Raw);
			return;
		}
		for (int i = 0; i < cdis.Count; i++)
		{
			PdbCustomDebugInfo pdbCustomDebugInfo = cdis[i];
			if (pdbCustomDebugInfo == null)
			{
				Error("Custom debug info is null");
			}
			else
			{
				AddCustomDebugInformation(serializerMethodContext, token.Raw, codedToken, pdbCustomDebugInfo);
			}
		}
	}

	private void AddCustomDebugInformation(SerializerMethodContext serializerMethodContext, uint token, uint encodedToken, PdbCustomDebugInfo cdi)
	{
		switch (cdi.Kind)
		{
		case PdbCustomDebugInfoKind.SourceServer:
		case PdbCustomDebugInfoKind.UsingGroups:
		case PdbCustomDebugInfoKind.ForwardMethodInfo:
		case PdbCustomDebugInfoKind.ForwardModuleInfo:
		case PdbCustomDebugInfoKind.StateMachineTypeName:
		case PdbCustomDebugInfoKind.DynamicLocals:
		case PdbCustomDebugInfoKind.TupleElementNames:
			Error("Unsupported custom debug info {0}", cdi.Kind);
			break;
		case PdbCustomDebugInfoKind.Unknown:
		case PdbCustomDebugInfoKind.TupleElementNames_PortablePdb:
		case PdbCustomDebugInfoKind.DefaultNamespace:
		case PdbCustomDebugInfoKind.DynamicLocalVariables:
		case PdbCustomDebugInfoKind.EmbeddedSource:
		case PdbCustomDebugInfoKind.SourceLink:
		case PdbCustomDebugInfoKind.CompilationMetadataReferences:
		case PdbCustomDebugInfoKind.CompilationOptions:
		case PdbCustomDebugInfoKind.TypeDefinitionDocuments:
		case PdbCustomDebugInfoKind.EditAndContinueStateMachineStateMap:
		case PdbCustomDebugInfoKind.PrimaryConstructorInformationBlob:
		case PdbCustomDebugInfoKind.StateMachineHoistedLocalScopes:
		case PdbCustomDebugInfoKind.EditAndContinueLocalSlotMap:
		case PdbCustomDebugInfoKind.EditAndContinueLambdaMap:
			AddCustomDebugInformationCore(serializerMethodContext, encodedToken, cdi, cdi.Guid);
			break;
		case PdbCustomDebugInfoKind.AsyncMethod:
			AddCustomDebugInformationCore(serializerMethodContext, encodedToken, cdi, CustomDebugInfoGuids.AsyncMethodSteppingInformationBlob);
			AddStateMachineMethod(cdi, token, ((PdbAsyncMethodCustomDebugInfo)cdi).KickoffMethod);
			break;
		case PdbCustomDebugInfoKind.IteratorMethod:
			AddStateMachineMethod(cdi, token, ((PdbIteratorMethodCustomDebugInfo)cdi).KickoffMethod);
			break;
		default:
			Error("Unknown custom debug info {0}.", cdi.Kind);
			break;
		}
	}

	private void AddStateMachineMethod(PdbCustomDebugInfo cdi, uint moveNextMethodToken, MethodDef kickoffMethod)
	{
		if (kickoffMethod == null)
		{
			Error("KickoffMethod is null");
			return;
		}
		RawStateMachineMethodRow row = new RawStateMachineMethodRow(new MDToken(moveNextMethodToken).Rid, GetRid(kickoffMethod));
		debugMetadata.stateMachineMethodInfos.Add(cdi, row);
	}

	private void AddCustomDebugInformationCore(SerializerMethodContext serializerMethodContext, uint encodedToken, PdbCustomDebugInfo cdi, Guid cdiGuid)
	{
		DataWriterContext ctx = AllocBinaryWriterContext();
		byte[] data = PortablePdbCustomDebugInfoWriter.Write(this, serializerMethodContext, this, cdi, ctx);
		Free(ref ctx);
		RawCustomDebugInformationRow row = new RawCustomDebugInformationRow(encodedToken, debugMetadata.guidHeap.Add(cdiGuid), debugMetadata.blobHeap.Add(data));
		debugMetadata.customDebugInfos.Add(cdi, row);
	}

	private void InitializeMethodDebugInformation()
	{
		if (debugMetadata != null)
		{
			int numberOfMethods = NumberOfMethods;
			for (int i = 0; i < numberOfMethods; i++)
			{
				debugMetadata.tablesHeap.MethodDebugInformationTable.Create(default(RawMethodDebugInformationRow));
			}
		}
	}

	private void AddPdbDocuments()
	{
		if (debugMetadata == null)
		{
			return;
		}
		foreach (PdbDocument document in module.PdbState.Documents)
		{
			AddPdbDocument(document);
		}
	}

	private uint AddPdbDocument(PdbDocument doc)
	{
		if (doc == null)
		{
			Error("PdbDocument is null");
			return 0u;
		}
		if (debugMetadata.pdbDocumentInfos.TryGetRid(doc, out var rid))
		{
			return rid;
		}
		RawDocumentRow row = new RawDocumentRow(GetDocumentNameBlobOffset(doc.Url), debugMetadata.guidHeap.Add(doc.CheckSumAlgorithmId), debugMetadata.blobHeap.Add(doc.CheckSum), debugMetadata.guidHeap.Add(doc.Language));
		rid = debugMetadata.tablesHeap.DocumentTable.Add(row);
		debugMetadata.pdbDocumentInfos.Add(doc, rid);
		AddCustomDebugInformationList(Table.Document, rid, doc.CustomDebugInfos);
		return rid;
	}

	private uint GetDocumentNameBlobOffset(string name)
	{
		if (name == null)
		{
			Error("Document name is null");
			name = string.Empty;
		}
		DataWriterContext ctx = AllocBinaryWriterContext();
		MemoryStream outStream = ctx.OutStream;
		DataWriter writer = ctx.Writer;
		outStream.SetLength(0L);
		outStream.Position = 0L;
		string[] array = name.Split(directorySeparatorCharArray);
		if (array.Length == 1)
		{
			writer.WriteByte(0);
		}
		else
		{
			writer.WriteBytes(directorySeparatorCharUtf8);
		}
		foreach (string s in array)
		{
			uint value = debugMetadata.blobHeap.Add(Encoding.UTF8.GetBytes(s));
			writer.WriteCompressedUInt32(value);
		}
		uint result = debugMetadata.blobHeap.Add(outStream.ToArray());
		Free(ref ctx);
		return result;
	}

	private uint AddImportScope(PdbImportScope scope)
	{
		if (scope == null)
		{
			return 0u;
		}
		if (debugMetadata.importScopeInfos.TryGetRid(scope, out var rid))
		{
			if (rid == 0)
			{
				Error("PdbImportScope has an infinite Parent loop");
			}
			return rid;
		}
		debugMetadata.importScopeInfos.Add(scope, 0u);
		DataWriterContext ctx = AllocBinaryWriterContext();
		MemoryStream outStream = ctx.OutStream;
		DataWriter writer = ctx.Writer;
		outStream.SetLength(0L);
		outStream.Position = 0L;
		ImportScopeBlobWriter.Write(this, this, writer, debugMetadata.blobHeap, scope.Imports);
		byte[] data = outStream.ToArray();
		Free(ref ctx);
		RawImportScopeRow row = new RawImportScopeRow(AddImportScope(scope.Parent), debugMetadata.blobHeap.Add(data));
		rid = debugMetadata.tablesHeap.ImportScopeTable.Add(row);
		debugMetadata.importScopeInfos.SetRid(scope, rid);
		AddCustomDebugInformationList(Table.ImportScope, rid, scope.CustomDebugInfos);
		return rid;
	}

	private void AddLocalVariable(PdbLocal local)
	{
		if (local == null)
		{
			Error("PDB local is null");
			return;
		}
		RawLocalVariableRow row = new RawLocalVariableRow((ushort)local.Attributes, (ushort)local.Index, debugMetadata.stringsHeap.Add(local.Name));
		uint rid = debugMetadata.tablesHeap.LocalVariableTable.Create(row);
		debugMetadata.localVariableInfos.Add(local, rid);
		AddCustomDebugInformationList(Table.LocalVariable, rid, local.CustomDebugInfos);
	}

	private void AddLocalConstant(PdbConstant constant)
	{
		if (constant == null)
		{
			Error("PDB constant is null");
			return;
		}
		DataWriterContext ctx = AllocBinaryWriterContext();
		MemoryStream outStream = ctx.OutStream;
		DataWriter writer = ctx.Writer;
		outStream.SetLength(0L);
		outStream.Position = 0L;
		LocalConstantSigBlobWriter.Write(this, this, writer, constant.Type, constant.Value);
		byte[] data = outStream.ToArray();
		Free(ref ctx);
		RawLocalConstantRow row = new RawLocalConstantRow(debugMetadata.stringsHeap.Add(constant.Name), debugMetadata.blobHeap.Add(data));
		uint rid = debugMetadata.tablesHeap.LocalConstantTable.Create(row);
		debugMetadata.localConstantInfos.Add(constant, rid);
		AddCustomDebugInformationList(Table.LocalConstant, rid, constant.CustomDebugInfos);
	}

	internal void WritePortablePdb(Stream output, uint entryPointToken, out long pdbIdOffset)
	{
		if (debugMetadata == null)
		{
			throw new InvalidOperationException();
		}
		PdbHeap pdbHeap = debugMetadata.PdbHeap;
		pdbHeap.EntryPoint = entryPointToken;
		tablesHeap.GetSystemTableRows(out var mask, pdbHeap.TypeSystemTableRows);
		debugMetadata.tablesHeap.SetSystemTableRows(pdbHeap.TypeSystemTableRows);
		if (!debugMetadata.methodDebugInformationInfosUsed)
		{
			debugMetadata.tablesHeap.MethodDebugInformationTable.Reset();
		}
		pdbHeap.ReferencedTypeSystemTables = mask;
		DataWriter writer = new DataWriter(output);
		debugMetadata.OnBeforeSetOffset();
		debugMetadata.SetOffset((FileOffset)0u, (RVA)0u);
		debugMetadata.GetFileLength();
		debugMetadata.VerifyWriteTo(writer);
		pdbIdOffset = (long)pdbHeap.PdbIdOffset;
	}

	uint ISignatureWriterHelper.ToEncodedToken(ITypeDefOrRef typeDefOrRef)
	{
		return AddTypeDefOrRef(typeDefOrRef);
	}

	void IWriterError.Error(string message)
	{
		Error(message);
	}

	void IWriterError2.Error(string message, params object[] args)
	{
		Error(message, args);
	}

	bool IFullNameFactoryHelper.MustUseAssemblyName(IType type)
	{
		return FullNameFactory.MustUseAssemblyName(module, type, OptimizeCustomAttributeSerializedTypeNames);
	}

	protected virtual void Initialize()
	{
	}

	protected abstract TypeDef[] GetAllTypeDefs();

	protected abstract void AllocateTypeDefRids();

	protected abstract void AllocateMemberDefRids();

	protected abstract uint AddTypeRef(TypeRef tr);

	protected abstract uint AddTypeSpec(TypeSpec ts);

	protected abstract uint AddMemberRef(MemberRef mr);

	protected abstract uint AddStandAloneSig(StandAloneSig sas);

	protected abstract uint AddMethodSpec(MethodSpec ms);

	protected virtual void BeforeSortingCustomAttributes()
	{
	}

	protected virtual void EverythingInitialized()
	{
	}

	bool IReuseChunk.CanReuse(RVA origRva, uint origSize)
	{
		if (length == 0)
		{
			throw new InvalidOperationException();
		}
		return length <= origSize;
	}

	internal void OnBeforeSetOffset()
	{
		stringsHeap.AddOptimizedStringsAndSetReadOnly();
	}

	public void SetOffset(FileOffset offset, RVA rva)
	{
		bool flag = this.offset == (FileOffset)0u;
		this.offset = offset;
		this.rva = rva;
		if (flag)
		{
			blobHeap.SetReadOnly();
			guidHeap.SetReadOnly();
			tablesHeap.SetReadOnly();
			pdbHeap.SetReadOnly();
			tablesHeap.BigStrings = stringsHeap.IsBig;
			tablesHeap.BigBlob = blobHeap.IsBig;
			tablesHeap.BigGuid = guidHeap.IsBig;
			metadataHeader.Heaps = GetHeaps();
		}
		metadataHeader.SetOffset(offset, rva);
		uint fileLength = metadataHeader.GetFileLength();
		offset += fileLength;
		rva += fileLength;
		foreach (IHeap heap in metadataHeader.Heaps)
		{
			offset = offset.AlignUp(4u);
			rva = rva.AlignUp(4u);
			heap.SetOffset(offset, rva);
			fileLength = heap.GetFileLength();
			offset += fileLength;
			rva += fileLength;
		}
		if (!flag && length != rva - this.rva)
		{
			throw new InvalidOperationException();
		}
		length = rva - this.rva;
		if (!isStandaloneDebugMetadata && flag)
		{
			UpdateMethodAndFieldRvas();
		}
	}

	internal void UpdateMethodAndFieldRvas()
	{
		UpdateMethodRvas();
		UpdateFieldRvas();
	}

	private IList<IHeap> GetHeaps()
	{
		List<IHeap> list = new List<IHeap>();
		if (isStandaloneDebugMetadata)
		{
			list.Add(pdbHeap);
			list.Add(tablesHeap);
			if (!stringsHeap.IsEmpty)
			{
				list.Add(stringsHeap);
			}
			if (!usHeap.IsEmpty)
			{
				list.Add(usHeap);
			}
			if (!guidHeap.IsEmpty)
			{
				list.Add(guidHeap);
			}
			if (!blobHeap.IsEmpty)
			{
				list.Add(blobHeap);
			}
		}
		else
		{
			list.Add(tablesHeap);
			if (!stringsHeap.IsEmpty || AlwaysCreateStringsHeap)
			{
				list.Add(stringsHeap);
			}
			if (!usHeap.IsEmpty || AlwaysCreateUSHeap)
			{
				list.Add(usHeap);
			}
			if (!guidHeap.IsEmpty || AlwaysCreateGuidHeap)
			{
				list.Add(guidHeap);
			}
			if (!blobHeap.IsEmpty || AlwaysCreateBlobHeap)
			{
				list.Add(blobHeap);
			}
			list.AddRange(options.CustomHeaps);
			options.RaiseMetadataHeapsAdded(new MetadataHeapsAddedEventArgs(this, list));
		}
		return list;
	}

	public uint GetFileLength()
	{
		return length;
	}

	public uint GetVirtualSize()
	{
		return GetFileLength();
	}

	public uint CalculateAlignment()
	{
		return 0u;
	}

	public void WriteTo(DataWriter writer)
	{
		RVA rVA = rva;
		metadataHeader.VerifyWriteTo(writer);
		rVA += metadataHeader.GetFileLength();
		foreach (IHeap heap in metadataHeader.Heaps)
		{
			writer.WriteZeroes((int)(rVA.AlignUp(4u) - rVA));
			rVA = rVA.AlignUp(4u);
			heap.VerifyWriteTo(writer);
			rVA += heap.GetFileLength();
		}
	}

	protected static List<ParamDef> Sort(IEnumerable<ParamDef> pds)
	{
		List<ParamDef> list = new List<ParamDef>(pds);
		list.Sort(delegate(ParamDef a, ParamDef b)
		{
			if (a == null)
			{
				return -1;
			}
			return (b == null) ? 1 : a.Sequence.CompareTo(b.Sequence);
		});
		return list;
	}

	private DataWriterContext AllocBinaryWriterContext()
	{
		if (binaryWriterContexts.Count == 0)
		{
			return new DataWriterContext();
		}
		DataWriterContext result = binaryWriterContexts[binaryWriterContexts.Count - 1];
		binaryWriterContexts.RemoveAt(binaryWriterContexts.Count - 1);
		return result;
	}

	private void Free(ref DataWriterContext ctx)
	{
		binaryWriterContexts.Add(ctx);
		ctx = null;
	}

	private SerializerMethodContext AllocSerializerMethodContext()
	{
		if (serializerMethodContexts.Count == 0)
		{
			return new SerializerMethodContext(this);
		}
		SerializerMethodContext result = serializerMethodContexts[serializerMethodContexts.Count - 1];
		serializerMethodContexts.RemoveAt(serializerMethodContexts.Count - 1);
		return result;
	}

	private void Free(ref SerializerMethodContext ctx)
	{
		serializerMethodContexts.Add(ctx);
		ctx = null;
	}

	static Metadata()
	{
		Encoding uTF = Encoding.UTF8;
		char directorySeparatorChar = Path.DirectorySeparatorChar;
		directorySeparatorCharUtf8 = uTF.GetBytes(directorySeparatorChar.ToString());
		directorySeparatorCharArray = new char[1] { Path.DirectorySeparatorChar };
	}
}
