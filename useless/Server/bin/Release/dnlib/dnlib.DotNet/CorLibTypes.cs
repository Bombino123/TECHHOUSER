namespace dnlib.DotNet;

public sealed class CorLibTypes : ICorLibTypes
{
	private readonly ModuleDef module;

	private CorLibTypeSig typeVoid;

	private CorLibTypeSig typeBoolean;

	private CorLibTypeSig typeChar;

	private CorLibTypeSig typeSByte;

	private CorLibTypeSig typeByte;

	private CorLibTypeSig typeInt16;

	private CorLibTypeSig typeUInt16;

	private CorLibTypeSig typeInt32;

	private CorLibTypeSig typeUInt32;

	private CorLibTypeSig typeInt64;

	private CorLibTypeSig typeUInt64;

	private CorLibTypeSig typeSingle;

	private CorLibTypeSig typeDouble;

	private CorLibTypeSig typeString;

	private CorLibTypeSig typeTypedReference;

	private CorLibTypeSig typeIntPtr;

	private CorLibTypeSig typeUIntPtr;

	private CorLibTypeSig typeObject;

	private readonly AssemblyRef corLibAssemblyRef;

	public CorLibTypeSig Void => typeVoid;

	public CorLibTypeSig Boolean => typeBoolean;

	public CorLibTypeSig Char => typeChar;

	public CorLibTypeSig SByte => typeSByte;

	public CorLibTypeSig Byte => typeByte;

	public CorLibTypeSig Int16 => typeInt16;

	public CorLibTypeSig UInt16 => typeUInt16;

	public CorLibTypeSig Int32 => typeInt32;

	public CorLibTypeSig UInt32 => typeUInt32;

	public CorLibTypeSig Int64 => typeInt64;

	public CorLibTypeSig UInt64 => typeUInt64;

	public CorLibTypeSig Single => typeSingle;

	public CorLibTypeSig Double => typeDouble;

	public CorLibTypeSig String => typeString;

	public CorLibTypeSig TypedReference => typeTypedReference;

	public CorLibTypeSig IntPtr => typeIntPtr;

	public CorLibTypeSig UIntPtr => typeUIntPtr;

	public CorLibTypeSig Object => typeObject;

	public AssemblyRef AssemblyRef => corLibAssemblyRef;

	public CorLibTypes(ModuleDef module)
		: this(module, null)
	{
	}

	public CorLibTypes(ModuleDef module, AssemblyRef corLibAssemblyRef)
	{
		this.module = module;
		this.corLibAssemblyRef = corLibAssemblyRef ?? CreateCorLibAssemblyRef();
		Initialize();
	}

	private AssemblyRef CreateCorLibAssemblyRef()
	{
		return module.UpdateRowId(AssemblyRefUser.CreateMscorlibReferenceCLR20());
	}

	private void Initialize()
	{
		bool isCorLib = module.Assembly.IsCorLib();
		typeVoid = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Void"), ElementType.Void);
		typeBoolean = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Boolean"), ElementType.Boolean);
		typeChar = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Char"), ElementType.Char);
		typeSByte = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "SByte"), ElementType.I1);
		typeByte = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Byte"), ElementType.U1);
		typeInt16 = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Int16"), ElementType.I2);
		typeUInt16 = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "UInt16"), ElementType.U2);
		typeInt32 = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Int32"), ElementType.I4);
		typeUInt32 = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "UInt32"), ElementType.U4);
		typeInt64 = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Int64"), ElementType.I8);
		typeUInt64 = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "UInt64"), ElementType.U8);
		typeSingle = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Single"), ElementType.R4);
		typeDouble = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Double"), ElementType.R8);
		typeString = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "String"), ElementType.String);
		typeTypedReference = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "TypedReference"), ElementType.TypedByRef);
		typeIntPtr = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "IntPtr"), ElementType.I);
		typeUIntPtr = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "UIntPtr"), ElementType.U);
		typeObject = new CorLibTypeSig(CreateCorLibTypeRef(isCorLib, "Object"), ElementType.Object);
	}

	private ITypeDefOrRef CreateCorLibTypeRef(bool isCorLib, string name)
	{
		TypeRefUser typeRefUser = new TypeRefUser(module, "System", name, corLibAssemblyRef);
		if (isCorLib)
		{
			TypeDef typeDef = module.Find(typeRefUser);
			if (typeDef != null)
			{
				return typeDef;
			}
		}
		return module.UpdateRowId(typeRefUser);
	}

	public TypeRef GetTypeRef(string @namespace, string name)
	{
		return module.UpdateRowId(new TypeRefUser(module, @namespace, name, corLibAssemblyRef));
	}
}
