using System;
using System.Collections.Generic;

namespace dnlib.DotNet.MD;

public sealed class DotNetTableSizes
{
	private bool bigStrings;

	private bool bigGuid;

	private bool bigBlob;

	private bool forceAllBig;

	private TableInfo[] tableInfos;

	internal const int normalMaxTables = 56;

	internal static bool IsSystemTable(Table table)
	{
		return (int)table < 48;
	}

	public void InitializeSizes(bool bigStrings, bool bigGuid, bool bigBlob, IList<uint> systemRowCounts, IList<uint> debugRowCounts)
	{
		InitializeSizes(bigStrings, bigGuid, bigBlob, systemRowCounts, debugRowCounts, forceAllBig: false);
	}

	internal void InitializeSizes(bool bigStrings, bool bigGuid, bool bigBlob, IList<uint> systemRowCounts, IList<uint> debugRowCounts, bool forceAllBig)
	{
		this.bigStrings = bigStrings || forceAllBig;
		this.bigGuid = bigGuid || forceAllBig;
		this.bigBlob = bigBlob || forceAllBig;
		this.forceAllBig = forceAllBig;
		TableInfo[] array = tableInfos;
		foreach (TableInfo tableInfo in array)
		{
			IList<uint> rowCounts = (IsSystemTable(tableInfo.Table) ? systemRowCounts : debugRowCounts);
			int num = 0;
			ColumnInfo[] columns = tableInfo.Columns;
			foreach (ColumnInfo columnInfo in columns)
			{
				columnInfo.Offset = num;
				int num2 = (columnInfo.Size = GetSize(columnInfo.ColumnSize, rowCounts));
				num += num2;
			}
			tableInfo.RowSize = num;
		}
	}

	private int GetSize(ColumnSize columnSize, IList<uint> rowCounts)
	{
		if (0 <= (int)columnSize && (int)columnSize <= 55)
		{
			int num = (int)(columnSize - 0);
			uint num2 = ((num < rowCounts.Count) ? rowCounts[num] : 0u);
			if (!forceAllBig && num2 <= 65535)
			{
				return 2;
			}
			return 4;
		}
		if (72 <= (int)columnSize && (int)columnSize <= 85)
		{
			CodedToken codedToken = columnSize switch
			{
				ColumnSize.TypeDefOrRef => CodedToken.TypeDefOrRef, 
				ColumnSize.HasConstant => CodedToken.HasConstant, 
				ColumnSize.HasCustomAttribute => CodedToken.HasCustomAttribute, 
				ColumnSize.HasFieldMarshal => CodedToken.HasFieldMarshal, 
				ColumnSize.HasDeclSecurity => CodedToken.HasDeclSecurity, 
				ColumnSize.MemberRefParent => CodedToken.MemberRefParent, 
				ColumnSize.HasSemantic => CodedToken.HasSemantic, 
				ColumnSize.MethodDefOrRef => CodedToken.MethodDefOrRef, 
				ColumnSize.MemberForwarded => CodedToken.MemberForwarded, 
				ColumnSize.Implementation => CodedToken.Implementation, 
				ColumnSize.CustomAttributeType => CodedToken.CustomAttributeType, 
				ColumnSize.ResolutionScope => CodedToken.ResolutionScope, 
				ColumnSize.TypeOrMethodDef => CodedToken.TypeOrMethodDef, 
				ColumnSize.HasCustomDebugInformation => CodedToken.HasCustomDebugInformation, 
				_ => throw new InvalidOperationException($"Invalid ColumnSize: {columnSize}"), 
			};
			uint num3 = 0u;
			Table[] tableTypes = codedToken.TableTypes;
			for (int i = 0; i < tableTypes.Length; i++)
			{
				int num4 = (int)tableTypes[i];
				uint num5 = ((num4 < rowCounts.Count) ? rowCounts[num4] : 0u);
				if (num5 > num3)
				{
					num3 = num5;
				}
			}
			uint num6 = num3 << codedToken.Bits;
			if (!forceAllBig && num6 <= 65535)
			{
				return 2;
			}
			return 4;
		}
		switch (columnSize)
		{
		case ColumnSize.Byte:
			return 1;
		case ColumnSize.Int16:
			return 2;
		case ColumnSize.UInt16:
			return 2;
		case ColumnSize.Int32:
			return 4;
		case ColumnSize.UInt32:
			return 4;
		case ColumnSize.Strings:
			if (!forceAllBig && !bigStrings)
			{
				return 2;
			}
			return 4;
		case ColumnSize.GUID:
			if (!forceAllBig && !bigGuid)
			{
				return 2;
			}
			return 4;
		case ColumnSize.Blob:
			if (!forceAllBig && !bigBlob)
			{
				return 2;
			}
			return 4;
		default:
			throw new InvalidOperationException($"Invalid ColumnSize: {columnSize}");
		}
	}

	public TableInfo[] CreateTables(byte majorVersion, byte minorVersion)
	{
		int maxPresentTables;
		return CreateTables(majorVersion, minorVersion, out maxPresentTables);
	}

	public TableInfo[] CreateTables(byte majorVersion, byte minorVersion, out int maxPresentTables)
	{
		maxPresentTables = ((majorVersion == 1 && minorVersion == 0) ? 42 : 56);
		TableInfo[] array = new TableInfo[56]
		{
			new TableInfo(Table.Module, "Module", new ColumnInfo[5]
			{
				new ColumnInfo(0, "Generation", ColumnSize.UInt16),
				new ColumnInfo(1, "Name", ColumnSize.Strings),
				new ColumnInfo(2, "Mvid", ColumnSize.GUID),
				new ColumnInfo(3, "EncId", ColumnSize.GUID),
				new ColumnInfo(4, "EncBaseId", ColumnSize.GUID)
			}),
			new TableInfo(Table.TypeRef, "TypeRef", new ColumnInfo[3]
			{
				new ColumnInfo(0, "ResolutionScope", ColumnSize.ResolutionScope),
				new ColumnInfo(1, "Name", ColumnSize.Strings),
				new ColumnInfo(2, "Namespace", ColumnSize.Strings)
			}),
			new TableInfo(Table.TypeDef, "TypeDef", new ColumnInfo[6]
			{
				new ColumnInfo(0, "Flags", ColumnSize.UInt32),
				new ColumnInfo(1, "Name", ColumnSize.Strings),
				new ColumnInfo(2, "Namespace", ColumnSize.Strings),
				new ColumnInfo(3, "Extends", ColumnSize.TypeDefOrRef),
				new ColumnInfo(4, "FieldList", ColumnSize.Field),
				new ColumnInfo(5, "MethodList", ColumnSize.Method)
			}),
			new TableInfo(Table.FieldPtr, "FieldPtr", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Field", ColumnSize.Field)
			}),
			new TableInfo(Table.Field, "Field", new ColumnInfo[3]
			{
				new ColumnInfo(0, "Flags", ColumnSize.UInt16),
				new ColumnInfo(1, "Name", ColumnSize.Strings),
				new ColumnInfo(2, "Signature", ColumnSize.Blob)
			}),
			new TableInfo(Table.MethodPtr, "MethodPtr", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Method", ColumnSize.Method)
			}),
			new TableInfo(Table.Method, "Method", new ColumnInfo[6]
			{
				new ColumnInfo(0, "RVA", ColumnSize.UInt32),
				new ColumnInfo(1, "ImplFlags", ColumnSize.UInt16),
				new ColumnInfo(2, "Flags", ColumnSize.UInt16),
				new ColumnInfo(3, "Name", ColumnSize.Strings),
				new ColumnInfo(4, "Signature", ColumnSize.Blob),
				new ColumnInfo(5, "ParamList", ColumnSize.Param)
			}),
			new TableInfo(Table.ParamPtr, "ParamPtr", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Param", ColumnSize.Param)
			}),
			new TableInfo(Table.Param, "Param", new ColumnInfo[3]
			{
				new ColumnInfo(0, "Flags", ColumnSize.UInt16),
				new ColumnInfo(1, "Sequence", ColumnSize.UInt16),
				new ColumnInfo(2, "Name", ColumnSize.Strings)
			}),
			new TableInfo(Table.InterfaceImpl, "InterfaceImpl", new ColumnInfo[2]
			{
				new ColumnInfo(0, "Class", ColumnSize.TypeDef),
				new ColumnInfo(1, "Interface", ColumnSize.TypeDefOrRef)
			}),
			new TableInfo(Table.MemberRef, "MemberRef", new ColumnInfo[3]
			{
				new ColumnInfo(0, "Class", ColumnSize.MemberRefParent),
				new ColumnInfo(1, "Name", ColumnSize.Strings),
				new ColumnInfo(2, "Signature", ColumnSize.Blob)
			}),
			new TableInfo(Table.Constant, "Constant", new ColumnInfo[4]
			{
				new ColumnInfo(0, "Type", ColumnSize.Byte),
				new ColumnInfo(1, "Padding", ColumnSize.Byte),
				new ColumnInfo(2, "Parent", ColumnSize.HasConstant),
				new ColumnInfo(3, "Value", ColumnSize.Blob)
			}),
			new TableInfo(Table.CustomAttribute, "CustomAttribute", new ColumnInfo[3]
			{
				new ColumnInfo(0, "Parent", ColumnSize.HasCustomAttribute),
				new ColumnInfo(1, "Type", ColumnSize.CustomAttributeType),
				new ColumnInfo(2, "Value", ColumnSize.Blob)
			}),
			new TableInfo(Table.FieldMarshal, "FieldMarshal", new ColumnInfo[2]
			{
				new ColumnInfo(0, "Parent", ColumnSize.HasFieldMarshal),
				new ColumnInfo(1, "NativeType", ColumnSize.Blob)
			}),
			new TableInfo(Table.DeclSecurity, "DeclSecurity", new ColumnInfo[3]
			{
				new ColumnInfo(0, "Action", ColumnSize.Int16),
				new ColumnInfo(1, "Parent", ColumnSize.HasDeclSecurity),
				new ColumnInfo(2, "PermissionSet", ColumnSize.Blob)
			}),
			new TableInfo(Table.ClassLayout, "ClassLayout", new ColumnInfo[3]
			{
				new ColumnInfo(0, "PackingSize", ColumnSize.UInt16),
				new ColumnInfo(1, "ClassSize", ColumnSize.UInt32),
				new ColumnInfo(2, "Parent", ColumnSize.TypeDef)
			}),
			new TableInfo(Table.FieldLayout, "FieldLayout", new ColumnInfo[2]
			{
				new ColumnInfo(0, "OffSet", ColumnSize.UInt32),
				new ColumnInfo(1, "Field", ColumnSize.Field)
			}),
			new TableInfo(Table.StandAloneSig, "StandAloneSig", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Signature", ColumnSize.Blob)
			}),
			new TableInfo(Table.EventMap, "EventMap", new ColumnInfo[2]
			{
				new ColumnInfo(0, "Parent", ColumnSize.TypeDef),
				new ColumnInfo(1, "EventList", ColumnSize.Event)
			}),
			new TableInfo(Table.EventPtr, "EventPtr", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Event", ColumnSize.Event)
			}),
			new TableInfo(Table.Event, "Event", new ColumnInfo[3]
			{
				new ColumnInfo(0, "EventFlags", ColumnSize.UInt16),
				new ColumnInfo(1, "Name", ColumnSize.Strings),
				new ColumnInfo(2, "EventType", ColumnSize.TypeDefOrRef)
			}),
			new TableInfo(Table.PropertyMap, "PropertyMap", new ColumnInfo[2]
			{
				new ColumnInfo(0, "Parent", ColumnSize.TypeDef),
				new ColumnInfo(1, "PropertyList", ColumnSize.Property)
			}),
			new TableInfo(Table.PropertyPtr, "PropertyPtr", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Property", ColumnSize.Property)
			}),
			new TableInfo(Table.Property, "Property", new ColumnInfo[3]
			{
				new ColumnInfo(0, "PropFlags", ColumnSize.UInt16),
				new ColumnInfo(1, "Name", ColumnSize.Strings),
				new ColumnInfo(2, "Type", ColumnSize.Blob)
			}),
			new TableInfo(Table.MethodSemantics, "MethodSemantics", new ColumnInfo[3]
			{
				new ColumnInfo(0, "Semantic", ColumnSize.UInt16),
				new ColumnInfo(1, "Method", ColumnSize.Method),
				new ColumnInfo(2, "Association", ColumnSize.HasSemantic)
			}),
			new TableInfo(Table.MethodImpl, "MethodImpl", new ColumnInfo[3]
			{
				new ColumnInfo(0, "Class", ColumnSize.TypeDef),
				new ColumnInfo(1, "MethodBody", ColumnSize.MethodDefOrRef),
				new ColumnInfo(2, "MethodDeclaration", ColumnSize.MethodDefOrRef)
			}),
			new TableInfo(Table.ModuleRef, "ModuleRef", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Name", ColumnSize.Strings)
			}),
			new TableInfo(Table.TypeSpec, "TypeSpec", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Signature", ColumnSize.Blob)
			}),
			new TableInfo(Table.ImplMap, "ImplMap", new ColumnInfo[4]
			{
				new ColumnInfo(0, "MappingFlags", ColumnSize.UInt16),
				new ColumnInfo(1, "MemberForwarded", ColumnSize.MemberForwarded),
				new ColumnInfo(2, "ImportName", ColumnSize.Strings),
				new ColumnInfo(3, "ImportScope", ColumnSize.ModuleRef)
			}),
			new TableInfo(Table.FieldRVA, "FieldRVA", new ColumnInfo[2]
			{
				new ColumnInfo(0, "RVA", ColumnSize.UInt32),
				new ColumnInfo(1, "Field", ColumnSize.Field)
			}),
			new TableInfo(Table.ENCLog, "ENCLog", new ColumnInfo[2]
			{
				new ColumnInfo(0, "Token", ColumnSize.UInt32),
				new ColumnInfo(1, "FuncCode", ColumnSize.UInt32)
			}),
			new TableInfo(Table.ENCMap, "ENCMap", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Token", ColumnSize.UInt32)
			}),
			new TableInfo(Table.Assembly, "Assembly", new ColumnInfo[9]
			{
				new ColumnInfo(0, "HashAlgId", ColumnSize.UInt32),
				new ColumnInfo(1, "MajorVersion", ColumnSize.UInt16),
				new ColumnInfo(2, "MinorVersion", ColumnSize.UInt16),
				new ColumnInfo(3, "BuildNumber", ColumnSize.UInt16),
				new ColumnInfo(4, "RevisionNumber", ColumnSize.UInt16),
				new ColumnInfo(5, "Flags", ColumnSize.UInt32),
				new ColumnInfo(6, "PublicKey", ColumnSize.Blob),
				new ColumnInfo(7, "Name", ColumnSize.Strings),
				new ColumnInfo(8, "Locale", ColumnSize.Strings)
			}),
			new TableInfo(Table.AssemblyProcessor, "AssemblyProcessor", new ColumnInfo[1]
			{
				new ColumnInfo(0, "Processor", ColumnSize.UInt32)
			}),
			new TableInfo(Table.AssemblyOS, "AssemblyOS", new ColumnInfo[3]
			{
				new ColumnInfo(0, "OSPlatformId", ColumnSize.UInt32),
				new ColumnInfo(1, "OSMajorVersion", ColumnSize.UInt32),
				new ColumnInfo(2, "OSMinorVersion", ColumnSize.UInt32)
			}),
			new TableInfo(Table.AssemblyRef, "AssemblyRef", new ColumnInfo[9]
			{
				new ColumnInfo(0, "MajorVersion", ColumnSize.UInt16),
				new ColumnInfo(1, "MinorVersion", ColumnSize.UInt16),
				new ColumnInfo(2, "BuildNumber", ColumnSize.UInt16),
				new ColumnInfo(3, "RevisionNumber", ColumnSize.UInt16),
				new ColumnInfo(4, "Flags", ColumnSize.UInt32),
				new ColumnInfo(5, "PublicKeyOrToken", ColumnSize.Blob),
				new ColumnInfo(6, "Name", ColumnSize.Strings),
				new ColumnInfo(7, "Locale", ColumnSize.Strings),
				new ColumnInfo(8, "HashValue", ColumnSize.Blob)
			}),
			new TableInfo(Table.AssemblyRefProcessor, "AssemblyRefProcessor", new ColumnInfo[2]
			{
				new ColumnInfo(0, "Processor", ColumnSize.UInt32),
				new ColumnInfo(1, "AssemblyRef", ColumnSize.AssemblyRef)
			}),
			new TableInfo(Table.AssemblyRefOS, "AssemblyRefOS", new ColumnInfo[4]
			{
				new ColumnInfo(0, "OSPlatformId", ColumnSize.UInt32),
				new ColumnInfo(1, "OSMajorVersion", ColumnSize.UInt32),
				new ColumnInfo(2, "OSMinorVersion", ColumnSize.UInt32),
				new ColumnInfo(3, "AssemblyRef", ColumnSize.AssemblyRef)
			}),
			new TableInfo(Table.File, "File", new ColumnInfo[3]
			{
				new ColumnInfo(0, "Flags", ColumnSize.UInt32),
				new ColumnInfo(1, "Name", ColumnSize.Strings),
				new ColumnInfo(2, "HashValue", ColumnSize.Blob)
			}),
			new TableInfo(Table.ExportedType, "ExportedType", new ColumnInfo[5]
			{
				new ColumnInfo(0, "Flags", ColumnSize.UInt32),
				new ColumnInfo(1, "TypeDefId", ColumnSize.UInt32),
				new ColumnInfo(2, "TypeName", ColumnSize.Strings),
				new ColumnInfo(3, "TypeNamespace", ColumnSize.Strings),
				new ColumnInfo(4, "Implementation", ColumnSize.Implementation)
			}),
			new TableInfo(Table.ManifestResource, "ManifestResource", new ColumnInfo[4]
			{
				new ColumnInfo(0, "Offset", ColumnSize.UInt32),
				new ColumnInfo(1, "Flags", ColumnSize.UInt32),
				new ColumnInfo(2, "Name", ColumnSize.Strings),
				new ColumnInfo(3, "Implementation", ColumnSize.Implementation)
			}),
			new TableInfo(Table.NestedClass, "NestedClass", new ColumnInfo[2]
			{
				new ColumnInfo(0, "NestedClass", ColumnSize.TypeDef),
				new ColumnInfo(1, "EnclosingClass", ColumnSize.TypeDef)
			}),
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null
		};
		if (majorVersion == 1 && minorVersion == 1)
		{
			array[42] = new TableInfo(Table.GenericParam, "GenericParam", new ColumnInfo[5]
			{
				new ColumnInfo(0, "Number", ColumnSize.UInt16),
				new ColumnInfo(1, "Flags", ColumnSize.UInt16),
				new ColumnInfo(2, "Owner", ColumnSize.TypeOrMethodDef),
				new ColumnInfo(3, "Name", ColumnSize.Strings),
				new ColumnInfo(4, "Kind", ColumnSize.TypeDefOrRef)
			});
		}
		else
		{
			array[42] = new TableInfo(Table.GenericParam, "GenericParam", new ColumnInfo[4]
			{
				new ColumnInfo(0, "Number", ColumnSize.UInt16),
				new ColumnInfo(1, "Flags", ColumnSize.UInt16),
				new ColumnInfo(2, "Owner", ColumnSize.TypeOrMethodDef),
				new ColumnInfo(3, "Name", ColumnSize.Strings)
			});
		}
		array[43] = new TableInfo(Table.MethodSpec, "MethodSpec", new ColumnInfo[2]
		{
			new ColumnInfo(0, "Method", ColumnSize.MethodDefOrRef),
			new ColumnInfo(1, "Instantiation", ColumnSize.Blob)
		});
		array[44] = new TableInfo(Table.GenericParamConstraint, "GenericParamConstraint", new ColumnInfo[2]
		{
			new ColumnInfo(0, "Owner", ColumnSize.GenericParam),
			new ColumnInfo(1, "Constraint", ColumnSize.TypeDefOrRef)
		});
		array[45] = new TableInfo((Table)45, string.Empty, new ColumnInfo[0]);
		array[46] = new TableInfo((Table)46, string.Empty, new ColumnInfo[0]);
		array[47] = new TableInfo((Table)47, string.Empty, new ColumnInfo[0]);
		array[48] = new TableInfo(Table.Document, "Document", new ColumnInfo[4]
		{
			new ColumnInfo(0, "Name", ColumnSize.Blob),
			new ColumnInfo(1, "HashAlgorithm", ColumnSize.GUID),
			new ColumnInfo(2, "Hash", ColumnSize.Blob),
			new ColumnInfo(3, "Language", ColumnSize.GUID)
		});
		array[49] = new TableInfo(Table.MethodDebugInformation, "MethodDebugInformation", new ColumnInfo[2]
		{
			new ColumnInfo(0, "Document", ColumnSize.Document),
			new ColumnInfo(1, "SequencePoints", ColumnSize.Blob)
		});
		array[50] = new TableInfo(Table.LocalScope, "LocalScope", new ColumnInfo[6]
		{
			new ColumnInfo(0, "Method", ColumnSize.Method),
			new ColumnInfo(1, "ImportScope", ColumnSize.ImportScope),
			new ColumnInfo(2, "VariableList", ColumnSize.LocalVariable),
			new ColumnInfo(3, "ConstantList", ColumnSize.LocalConstant),
			new ColumnInfo(4, "StartOffset", ColumnSize.UInt32),
			new ColumnInfo(5, "Length", ColumnSize.UInt32)
		});
		array[51] = new TableInfo(Table.LocalVariable, "LocalVariable", new ColumnInfo[3]
		{
			new ColumnInfo(0, "Attributes", ColumnSize.UInt16),
			new ColumnInfo(1, "Index", ColumnSize.UInt16),
			new ColumnInfo(2, "Name", ColumnSize.Strings)
		});
		array[52] = new TableInfo(Table.LocalConstant, "LocalConstant", new ColumnInfo[2]
		{
			new ColumnInfo(0, "Name", ColumnSize.Strings),
			new ColumnInfo(1, "Signature", ColumnSize.Blob)
		});
		array[53] = new TableInfo(Table.ImportScope, "ImportScope", new ColumnInfo[2]
		{
			new ColumnInfo(0, "Parent", ColumnSize.ImportScope),
			new ColumnInfo(1, "Imports", ColumnSize.Blob)
		});
		array[54] = new TableInfo(Table.StateMachineMethod, "StateMachineMethod", new ColumnInfo[2]
		{
			new ColumnInfo(0, "MoveNextMethod", ColumnSize.Method),
			new ColumnInfo(1, "KickoffMethod", ColumnSize.Method)
		});
		array[55] = new TableInfo(Table.CustomDebugInformation, "CustomDebugInformation", new ColumnInfo[3]
		{
			new ColumnInfo(0, "Parent", ColumnSize.HasCustomDebugInformation),
			new ColumnInfo(1, "Kind", ColumnSize.GUID),
			new ColumnInfo(2, "Value", ColumnSize.Blob)
		});
		return tableInfos = array;
	}
}
