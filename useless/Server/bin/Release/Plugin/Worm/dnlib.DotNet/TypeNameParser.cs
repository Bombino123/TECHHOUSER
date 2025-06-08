using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace dnlib.DotNet;

[ComVisible(true)]
public abstract class TypeNameParser : IDisposable
{
	internal abstract class TSpec
	{
		public readonly ElementType etype;

		protected TSpec(ElementType etype)
		{
			this.etype = etype;
		}
	}

	internal sealed class SZArraySpec : TSpec
	{
		public static readonly SZArraySpec Instance = new SZArraySpec();

		private SZArraySpec()
			: base(ElementType.SZArray)
		{
		}
	}

	internal sealed class ArraySpec : TSpec
	{
		public uint rank;

		public readonly IList<uint> sizes = new List<uint>();

		public readonly IList<int> lowerBounds = new List<int>();

		public ArraySpec()
			: base(ElementType.Array)
		{
		}
	}

	internal sealed class GenericInstSpec : TSpec
	{
		public readonly List<TypeSig> args = new List<TypeSig>();

		public GenericInstSpec()
			: base(ElementType.GenericInst)
		{
		}
	}

	internal sealed class ByRefSpec : TSpec
	{
		public static readonly ByRefSpec Instance = new ByRefSpec();

		private ByRefSpec()
			: base(ElementType.ByRef)
		{
		}
	}

	internal sealed class PtrSpec : TSpec
	{
		public static readonly PtrSpec Instance = new PtrSpec();

		private PtrSpec()
			: base(ElementType.Ptr)
		{
		}
	}

	protected ModuleDef ownerModule;

	private readonly GenericParamContext gpContext;

	private StringReader reader;

	private readonly IAssemblyRefFinder typeNameParserHelper;

	private RecursionCounter recursionCounter;

	public static ITypeDefOrRef ParseReflectionThrow(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper)
	{
		return ParseReflectionThrow(ownerModule, typeFullName, typeNameParserHelper, default(GenericParamContext));
	}

	public static ITypeDefOrRef ParseReflectionThrow(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper, GenericParamContext gpContext)
	{
		using ReflectionTypeNameParser reflectionTypeNameParser = new ReflectionTypeNameParser(ownerModule, typeFullName, typeNameParserHelper, gpContext);
		return reflectionTypeNameParser.Parse();
	}

	public static ITypeDefOrRef ParseReflection(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper)
	{
		return ParseReflection(ownerModule, typeFullName, typeNameParserHelper, default(GenericParamContext));
	}

	public static ITypeDefOrRef ParseReflection(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper, GenericParamContext gpContext)
	{
		try
		{
			return ParseReflectionThrow(ownerModule, typeFullName, typeNameParserHelper, gpContext);
		}
		catch (TypeNameParserException)
		{
			return null;
		}
	}

	public static TypeSig ParseAsTypeSigReflectionThrow(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper)
	{
		return ParseAsTypeSigReflectionThrow(ownerModule, typeFullName, typeNameParserHelper, default(GenericParamContext));
	}

	public static TypeSig ParseAsTypeSigReflectionThrow(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper, GenericParamContext gpContext)
	{
		using ReflectionTypeNameParser reflectionTypeNameParser = new ReflectionTypeNameParser(ownerModule, typeFullName, typeNameParserHelper, gpContext);
		return reflectionTypeNameParser.ParseAsTypeSig();
	}

	public static TypeSig ParseAsTypeSigReflection(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper)
	{
		return ParseAsTypeSigReflection(ownerModule, typeFullName, typeNameParserHelper, default(GenericParamContext));
	}

	public static TypeSig ParseAsTypeSigReflection(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper, GenericParamContext gpContext)
	{
		try
		{
			return ParseAsTypeSigReflectionThrow(ownerModule, typeFullName, typeNameParserHelper, gpContext);
		}
		catch (TypeNameParserException)
		{
			return null;
		}
	}

	protected TypeNameParser(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper)
		: this(ownerModule, typeFullName, typeNameParserHelper, default(GenericParamContext))
	{
	}

	protected TypeNameParser(ModuleDef ownerModule, string typeFullName, IAssemblyRefFinder typeNameParserHelper, GenericParamContext gpContext)
	{
		this.ownerModule = ownerModule;
		reader = new StringReader(typeFullName ?? string.Empty);
		this.typeNameParserHelper = typeNameParserHelper;
		this.gpContext = gpContext;
	}

	internal ITypeDefOrRef Parse()
	{
		return ownerModule.UpdateRowId(ParseAsTypeSig().ToTypeDefOrRef());
	}

	internal abstract TypeSig ParseAsTypeSig();

	protected void RecursionIncrement()
	{
		if (!recursionCounter.Increment())
		{
			throw new TypeNameParserException("Stack overflow");
		}
	}

	protected void RecursionDecrement()
	{
		recursionCounter.Decrement();
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
			if (reader != null)
			{
				reader.Dispose();
			}
			reader = null;
		}
	}

	internal GenericSig ReadGenericSig()
	{
		Verify(ReadChar() == 33, "Expected '!'");
		if (PeekChar() == 33)
		{
			ReadChar();
			return new GenericMVar(ReadUInt32(), gpContext.Method);
		}
		return new GenericVar(ReadUInt32(), gpContext.Type);
	}

	internal TypeSig CreateTypeSig(IList<TSpec> tspecs, TypeSig currentSig)
	{
		int count = tspecs.Count;
		for (int i = 0; i < count; i++)
		{
			TSpec tSpec = tspecs[i];
			switch (tSpec.etype)
			{
			case ElementType.SZArray:
				currentSig = new SZArraySig(currentSig);
				break;
			case ElementType.Array:
			{
				ArraySpec arraySpec = (ArraySpec)tSpec;
				currentSig = new ArraySig(currentSig, arraySpec.rank, arraySpec.sizes, arraySpec.lowerBounds);
				break;
			}
			case ElementType.GenericInst:
			{
				GenericInstSpec genericInstSpec = (GenericInstSpec)tSpec;
				currentSig = new GenericInstSig(currentSig as ClassOrValueTypeSig, genericInstSpec.args);
				break;
			}
			case ElementType.ByRef:
				currentSig = new ByRefSig(currentSig);
				break;
			case ElementType.Ptr:
				currentSig = new PtrSig(currentSig);
				break;
			default:
				Verify(b: false, "Unknown TSpec");
				break;
			}
		}
		return currentSig;
	}

	protected TypeRef ReadTypeRefAndNestedNoAssembly(char nestedChar)
	{
		TypeRef typeRef = ReadTypeRefNoAssembly();
		while (true)
		{
			SkipWhite();
			if (PeekChar() != nestedChar)
			{
				break;
			}
			ReadChar();
			TypeRef typeRef2 = ReadTypeRefNoAssembly();
			typeRef2.ResolutionScope = typeRef;
			typeRef = typeRef2;
		}
		return typeRef;
	}

	protected TypeRef ReadTypeRefNoAssembly()
	{
		GetNamespaceAndName(ReadId(ignoreWhiteSpace: false, ignoreEqualSign: false), out var ns, out var name);
		return ownerModule.UpdateRowId(new TypeRefUser(ownerModule, ns, name));
	}

	private static void GetNamespaceAndName(string fullName, out string ns, out string name)
	{
		int num = fullName.LastIndexOf('.');
		if (num < 0)
		{
			ns = string.Empty;
			name = fullName;
		}
		else
		{
			ns = fullName.Substring(0, num);
			name = fullName.Substring(num + 1);
		}
	}

	internal TypeSig ToTypeSig(ITypeDefOrRef type)
	{
		if (type is TypeDef typeDef)
		{
			return ToTypeSig(typeDef, typeDef.IsValueType);
		}
		if (type is TypeRef typeRef)
		{
			return ToTypeSig(typeRef, IsValueType(typeRef));
		}
		if (type is TypeSpec typeSpec)
		{
			return typeSpec.TypeSig;
		}
		Verify(b: false, "Unknown type");
		return null;
	}

	private static TypeSig ToTypeSig(ITypeDefOrRef type, bool isValueType)
	{
		if (!isValueType)
		{
			return new ClassSig(type);
		}
		return new ValueTypeSig(type);
	}

	internal AssemblyRef FindAssemblyRef(TypeRef nonNestedTypeRef)
	{
		AssemblyRef assemblyRef = null;
		if (nonNestedTypeRef != null && typeNameParserHelper != null)
		{
			assemblyRef = typeNameParserHelper.FindAssemblyRef(nonNestedTypeRef);
		}
		if (assemblyRef != null)
		{
			return assemblyRef;
		}
		AssemblyDef assembly = ownerModule.Assembly;
		if (assembly != null)
		{
			return ownerModule.UpdateRowId(assembly.ToAssemblyRef());
		}
		return AssemblyRef.CurrentAssembly;
	}

	internal bool IsValueType(TypeRef typeRef)
	{
		return typeRef?.IsValueType ?? false;
	}

	internal static void Verify(bool b, string msg)
	{
		if (!b)
		{
			throw new TypeNameParserException(msg);
		}
	}

	internal void SkipWhite()
	{
		while (true)
		{
			int num = PeekChar();
			if (num != -1 && char.IsWhiteSpace((char)num))
			{
				ReadChar();
				continue;
			}
			break;
		}
	}

	internal uint ReadUInt32()
	{
		SkipWhite();
		bool b = false;
		uint num = 0u;
		while (true)
		{
			int num2 = PeekChar();
			if (num2 == -1 || num2 < 48 || num2 > 57)
			{
				break;
			}
			ReadChar();
			int num3 = (int)(num * 10) + (num2 - 48);
			Verify((uint)num3 >= num, "Integer overflow");
			num = (uint)num3;
			b = true;
		}
		Verify(b, "Expected an integer");
		return num;
	}

	internal int ReadInt32()
	{
		SkipWhite();
		bool flag = false;
		if (PeekChar() == 45)
		{
			flag = true;
			ReadChar();
		}
		uint num = ReadUInt32();
		if (flag)
		{
			Verify(num <= 2147483648u, "Integer overflow");
			return (int)(0 - num);
		}
		Verify(num <= int.MaxValue, "Integer overflow");
		return (int)num;
	}

	internal string ReadId()
	{
		return ReadId(ignoreWhiteSpace: true, ignoreEqualSign: true);
	}

	internal string ReadId(bool ignoreWhiteSpace, bool ignoreEqualSign)
	{
		SkipWhite();
		StringBuilder stringBuilder = new StringBuilder();
		int idChar;
		while ((idChar = GetIdChar(ignoreWhiteSpace, ignoreEqualSign)) != -1)
		{
			stringBuilder.Append((char)idChar);
		}
		Verify(stringBuilder.Length > 0, "Expected an id");
		return stringBuilder.ToString();
	}

	protected int PeekChar()
	{
		return reader.Peek();
	}

	protected int ReadChar()
	{
		return reader.Read();
	}

	internal abstract int GetIdChar(bool ignoreWhiteSpace, bool ignoreEqualSign);
}
