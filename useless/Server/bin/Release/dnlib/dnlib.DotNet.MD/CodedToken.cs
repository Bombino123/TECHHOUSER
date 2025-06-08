using System;

namespace dnlib.DotNet.MD;

public sealed class CodedToken
{
	public static readonly CodedToken TypeDefOrRef = new CodedToken(2, new Table[3]
	{
		Table.TypeDef,
		Table.TypeRef,
		Table.TypeSpec
	});

	public static readonly CodedToken HasConstant = new CodedToken(2, new Table[3]
	{
		Table.Field,
		Table.Param,
		Table.Property
	});

	public static readonly CodedToken HasCustomAttribute = new CodedToken(5, new Table[24]
	{
		Table.Method,
		Table.Field,
		Table.TypeRef,
		Table.TypeDef,
		Table.Param,
		Table.InterfaceImpl,
		Table.MemberRef,
		Table.Module,
		Table.DeclSecurity,
		Table.Property,
		Table.Event,
		Table.StandAloneSig,
		Table.ModuleRef,
		Table.TypeSpec,
		Table.Assembly,
		Table.AssemblyRef,
		Table.File,
		Table.ExportedType,
		Table.ManifestResource,
		Table.GenericParam,
		Table.GenericParamConstraint,
		Table.MethodSpec,
		Table.Module,
		Table.Module
	});

	public static readonly CodedToken HasFieldMarshal = new CodedToken(1, new Table[2]
	{
		Table.Field,
		Table.Param
	});

	public static readonly CodedToken HasDeclSecurity = new CodedToken(2, new Table[3]
	{
		Table.TypeDef,
		Table.Method,
		Table.Assembly
	});

	public static readonly CodedToken MemberRefParent = new CodedToken(3, new Table[5]
	{
		Table.TypeDef,
		Table.TypeRef,
		Table.ModuleRef,
		Table.Method,
		Table.TypeSpec
	});

	public static readonly CodedToken HasSemantic = new CodedToken(1, new Table[2]
	{
		Table.Event,
		Table.Property
	});

	public static readonly CodedToken MethodDefOrRef = new CodedToken(1, new Table[2]
	{
		Table.Method,
		Table.MemberRef
	});

	public static readonly CodedToken MemberForwarded = new CodedToken(1, new Table[2]
	{
		Table.Field,
		Table.Method
	});

	public static readonly CodedToken Implementation = new CodedToken(2, new Table[3]
	{
		Table.File,
		Table.AssemblyRef,
		Table.ExportedType
	});

	public static readonly CodedToken CustomAttributeType = new CodedToken(3, new Table[5]
	{
		Table.Module,
		Table.Module,
		Table.Method,
		Table.MemberRef,
		Table.Module
	});

	public static readonly CodedToken ResolutionScope = new CodedToken(2, new Table[4]
	{
		Table.Module,
		Table.ModuleRef,
		Table.AssemblyRef,
		Table.TypeRef
	});

	public static readonly CodedToken TypeOrMethodDef = new CodedToken(1, new Table[2]
	{
		Table.TypeDef,
		Table.Method
	});

	public static readonly CodedToken HasCustomDebugInformation = new CodedToken(5, new Table[27]
	{
		Table.Method,
		Table.Field,
		Table.TypeRef,
		Table.TypeDef,
		Table.Param,
		Table.InterfaceImpl,
		Table.MemberRef,
		Table.Module,
		Table.DeclSecurity,
		Table.Property,
		Table.Event,
		Table.StandAloneSig,
		Table.ModuleRef,
		Table.TypeSpec,
		Table.Assembly,
		Table.AssemblyRef,
		Table.File,
		Table.ExportedType,
		Table.ManifestResource,
		Table.GenericParam,
		Table.GenericParamConstraint,
		Table.MethodSpec,
		Table.Document,
		Table.LocalScope,
		Table.LocalVariable,
		Table.LocalConstant,
		Table.ImportScope
	});

	private readonly Table[] tableTypes;

	private readonly int bits;

	private readonly int mask;

	public Table[] TableTypes => tableTypes;

	public int Bits => bits;

	internal CodedToken(int bits, Table[] tableTypes)
	{
		this.bits = bits;
		mask = (1 << bits) - 1;
		this.tableTypes = tableTypes;
	}

	public uint Encode(MDToken token)
	{
		return Encode(token.Raw);
	}

	public uint Encode(uint token)
	{
		Encode(token, out var codedToken);
		return codedToken;
	}

	public bool Encode(MDToken token, out uint codedToken)
	{
		return Encode(token.Raw, out codedToken);
	}

	public bool Encode(uint token, out uint codedToken)
	{
		int num = Array.IndexOf(tableTypes, MDToken.ToTable(token));
		if (num < 0)
		{
			codedToken = uint.MaxValue;
			return false;
		}
		codedToken = (MDToken.ToRID(token) << bits) | (uint)num;
		return true;
	}

	public MDToken Decode2(uint codedToken)
	{
		Decode(codedToken, out uint token);
		return new MDToken(token);
	}

	public uint Decode(uint codedToken)
	{
		Decode(codedToken, out uint token);
		return token;
	}

	public bool Decode(uint codedToken, out MDToken token)
	{
		uint token2;
		bool result = Decode(codedToken, out token2);
		token = new MDToken(token2);
		return result;
	}

	public bool Decode(uint codedToken, out uint token)
	{
		uint num = codedToken >> bits;
		int num2 = (int)(codedToken & mask);
		if (num > 16777215 || num2 >= tableTypes.Length)
		{
			token = 0u;
			return false;
		}
		token = ((uint)tableTypes[num2] << 24) | num;
		return true;
	}
}
