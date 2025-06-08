using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.MD;

[ComVisible(true)]
public sealed class RawRowEqualityComparer : IEqualityComparer<RawModuleRow>, IEqualityComparer<RawTypeRefRow>, IEqualityComparer<RawTypeDefRow>, IEqualityComparer<RawFieldPtrRow>, IEqualityComparer<RawFieldRow>, IEqualityComparer<RawMethodPtrRow>, IEqualityComparer<RawMethodRow>, IEqualityComparer<RawParamPtrRow>, IEqualityComparer<RawParamRow>, IEqualityComparer<RawInterfaceImplRow>, IEqualityComparer<RawMemberRefRow>, IEqualityComparer<RawConstantRow>, IEqualityComparer<RawCustomAttributeRow>, IEqualityComparer<RawFieldMarshalRow>, IEqualityComparer<RawDeclSecurityRow>, IEqualityComparer<RawClassLayoutRow>, IEqualityComparer<RawFieldLayoutRow>, IEqualityComparer<RawStandAloneSigRow>, IEqualityComparer<RawEventMapRow>, IEqualityComparer<RawEventPtrRow>, IEqualityComparer<RawEventRow>, IEqualityComparer<RawPropertyMapRow>, IEqualityComparer<RawPropertyPtrRow>, IEqualityComparer<RawPropertyRow>, IEqualityComparer<RawMethodSemanticsRow>, IEqualityComparer<RawMethodImplRow>, IEqualityComparer<RawModuleRefRow>, IEqualityComparer<RawTypeSpecRow>, IEqualityComparer<RawImplMapRow>, IEqualityComparer<RawFieldRVARow>, IEqualityComparer<RawENCLogRow>, IEqualityComparer<RawENCMapRow>, IEqualityComparer<RawAssemblyRow>, IEqualityComparer<RawAssemblyProcessorRow>, IEqualityComparer<RawAssemblyOSRow>, IEqualityComparer<RawAssemblyRefRow>, IEqualityComparer<RawAssemblyRefProcessorRow>, IEqualityComparer<RawAssemblyRefOSRow>, IEqualityComparer<RawFileRow>, IEqualityComparer<RawExportedTypeRow>, IEqualityComparer<RawManifestResourceRow>, IEqualityComparer<RawNestedClassRow>, IEqualityComparer<RawGenericParamRow>, IEqualityComparer<RawMethodSpecRow>, IEqualityComparer<RawGenericParamConstraintRow>, IEqualityComparer<RawDocumentRow>, IEqualityComparer<RawMethodDebugInformationRow>, IEqualityComparer<RawLocalScopeRow>, IEqualityComparer<RawLocalVariableRow>, IEqualityComparer<RawLocalConstantRow>, IEqualityComparer<RawImportScopeRow>, IEqualityComparer<RawStateMachineMethodRow>, IEqualityComparer<RawCustomDebugInformationRow>
{
	public static readonly RawRowEqualityComparer Instance = new RawRowEqualityComparer();

	private static int rol(uint val, int shift)
	{
		return (int)((val << shift) | (val >> 32 - shift));
	}

	public bool Equals(RawModuleRow x, RawModuleRow y)
	{
		if (x.Generation == y.Generation && x.Name == y.Name && x.Mvid == y.Mvid && x.EncId == y.EncId)
		{
			return x.EncBaseId == y.EncBaseId;
		}
		return false;
	}

	public int GetHashCode(RawModuleRow obj)
	{
		return obj.Generation + rol(obj.Name, 3) + rol(obj.Mvid, 7) + rol(obj.EncId, 11) + rol(obj.EncBaseId, 15);
	}

	public bool Equals(RawTypeRefRow x, RawTypeRefRow y)
	{
		if (x.ResolutionScope == y.ResolutionScope && x.Name == y.Name)
		{
			return x.Namespace == y.Namespace;
		}
		return false;
	}

	public int GetHashCode(RawTypeRefRow obj)
	{
		return (int)obj.ResolutionScope + rol(obj.Name, 3) + rol(obj.Namespace, 7);
	}

	public bool Equals(RawTypeDefRow x, RawTypeDefRow y)
	{
		if (x.Flags == y.Flags && x.Name == y.Name && x.Namespace == y.Namespace && x.Extends == y.Extends && x.FieldList == y.FieldList)
		{
			return x.MethodList == y.MethodList;
		}
		return false;
	}

	public int GetHashCode(RawTypeDefRow obj)
	{
		return (int)obj.Flags + rol(obj.Name, 3) + rol(obj.Namespace, 7) + rol(obj.Extends, 11) + rol(obj.FieldList, 15) + rol(obj.MethodList, 19);
	}

	public bool Equals(RawFieldPtrRow x, RawFieldPtrRow y)
	{
		return x.Field == y.Field;
	}

	public int GetHashCode(RawFieldPtrRow obj)
	{
		return (int)obj.Field;
	}

	public bool Equals(RawFieldRow x, RawFieldRow y)
	{
		if (x.Flags == y.Flags && x.Name == y.Name)
		{
			return x.Signature == y.Signature;
		}
		return false;
	}

	public int GetHashCode(RawFieldRow obj)
	{
		return obj.Flags + rol(obj.Name, 3) + rol(obj.Signature, 7);
	}

	public bool Equals(RawMethodPtrRow x, RawMethodPtrRow y)
	{
		return x.Method == y.Method;
	}

	public int GetHashCode(RawMethodPtrRow obj)
	{
		return (int)obj.Method;
	}

	public bool Equals(RawMethodRow x, RawMethodRow y)
	{
		if (x.RVA == y.RVA && x.ImplFlags == y.ImplFlags && x.Flags == y.Flags && x.Name == y.Name && x.Signature == y.Signature)
		{
			return x.ParamList == y.ParamList;
		}
		return false;
	}

	public int GetHashCode(RawMethodRow obj)
	{
		return (int)obj.RVA + rol(obj.ImplFlags, 3) + rol(obj.Flags, 7) + rol(obj.Name, 11) + rol(obj.Signature, 15) + rol(obj.ParamList, 19);
	}

	public bool Equals(RawParamPtrRow x, RawParamPtrRow y)
	{
		return x.Param == y.Param;
	}

	public int GetHashCode(RawParamPtrRow obj)
	{
		return (int)obj.Param;
	}

	public bool Equals(RawParamRow x, RawParamRow y)
	{
		if (x.Flags == y.Flags && x.Sequence == y.Sequence)
		{
			return x.Name == y.Name;
		}
		return false;
	}

	public int GetHashCode(RawParamRow obj)
	{
		return obj.Flags + rol(obj.Sequence, 3) + rol(obj.Name, 7);
	}

	public bool Equals(RawInterfaceImplRow x, RawInterfaceImplRow y)
	{
		if (x.Class == y.Class)
		{
			return x.Interface == y.Interface;
		}
		return false;
	}

	public int GetHashCode(RawInterfaceImplRow obj)
	{
		return (int)obj.Class + rol(obj.Interface, 3);
	}

	public bool Equals(RawMemberRefRow x, RawMemberRefRow y)
	{
		if (x.Class == y.Class && x.Name == y.Name)
		{
			return x.Signature == y.Signature;
		}
		return false;
	}

	public int GetHashCode(RawMemberRefRow obj)
	{
		return (int)obj.Class + rol(obj.Name, 3) + rol(obj.Signature, 7);
	}

	public bool Equals(RawConstantRow x, RawConstantRow y)
	{
		if (x.Type == y.Type && x.Padding == y.Padding && x.Parent == y.Parent)
		{
			return x.Value == y.Value;
		}
		return false;
	}

	public int GetHashCode(RawConstantRow obj)
	{
		return obj.Type + rol(obj.Padding, 3) + rol(obj.Parent, 7) + rol(obj.Value, 11);
	}

	public bool Equals(RawCustomAttributeRow x, RawCustomAttributeRow y)
	{
		if (x.Parent == y.Parent && x.Type == y.Type)
		{
			return x.Value == y.Value;
		}
		return false;
	}

	public int GetHashCode(RawCustomAttributeRow obj)
	{
		return (int)obj.Parent + rol(obj.Type, 3) + rol(obj.Value, 7);
	}

	public bool Equals(RawFieldMarshalRow x, RawFieldMarshalRow y)
	{
		if (x.Parent == y.Parent)
		{
			return x.NativeType == y.NativeType;
		}
		return false;
	}

	public int GetHashCode(RawFieldMarshalRow obj)
	{
		return (int)obj.Parent + rol(obj.NativeType, 3);
	}

	public bool Equals(RawDeclSecurityRow x, RawDeclSecurityRow y)
	{
		if (x.Action == y.Action && x.Parent == y.Parent)
		{
			return x.PermissionSet == y.PermissionSet;
		}
		return false;
	}

	public int GetHashCode(RawDeclSecurityRow obj)
	{
		return obj.Action + rol(obj.Parent, 3) + rol(obj.PermissionSet, 7);
	}

	public bool Equals(RawClassLayoutRow x, RawClassLayoutRow y)
	{
		if (x.PackingSize == y.PackingSize && x.ClassSize == y.ClassSize)
		{
			return x.Parent == y.Parent;
		}
		return false;
	}

	public int GetHashCode(RawClassLayoutRow obj)
	{
		return obj.PackingSize + rol(obj.ClassSize, 3) + rol(obj.Parent, 7);
	}

	public bool Equals(RawFieldLayoutRow x, RawFieldLayoutRow y)
	{
		if (x.OffSet == y.OffSet)
		{
			return x.Field == y.Field;
		}
		return false;
	}

	public int GetHashCode(RawFieldLayoutRow obj)
	{
		return (int)obj.OffSet + rol(obj.Field, 3);
	}

	public bool Equals(RawStandAloneSigRow x, RawStandAloneSigRow y)
	{
		return x.Signature == y.Signature;
	}

	public int GetHashCode(RawStandAloneSigRow obj)
	{
		return (int)obj.Signature;
	}

	public bool Equals(RawEventMapRow x, RawEventMapRow y)
	{
		if (x.Parent == y.Parent)
		{
			return x.EventList == y.EventList;
		}
		return false;
	}

	public int GetHashCode(RawEventMapRow obj)
	{
		return (int)obj.Parent + rol(obj.EventList, 3);
	}

	public bool Equals(RawEventPtrRow x, RawEventPtrRow y)
	{
		return x.Event == y.Event;
	}

	public int GetHashCode(RawEventPtrRow obj)
	{
		return (int)obj.Event;
	}

	public bool Equals(RawEventRow x, RawEventRow y)
	{
		if (x.EventFlags == y.EventFlags && x.Name == y.Name)
		{
			return x.EventType == y.EventType;
		}
		return false;
	}

	public int GetHashCode(RawEventRow obj)
	{
		return obj.EventFlags + rol(obj.Name, 3) + rol(obj.EventType, 7);
	}

	public bool Equals(RawPropertyMapRow x, RawPropertyMapRow y)
	{
		if (x.Parent == y.Parent)
		{
			return x.PropertyList == y.PropertyList;
		}
		return false;
	}

	public int GetHashCode(RawPropertyMapRow obj)
	{
		return (int)obj.Parent + rol(obj.PropertyList, 3);
	}

	public bool Equals(RawPropertyPtrRow x, RawPropertyPtrRow y)
	{
		return x.Property == y.Property;
	}

	public int GetHashCode(RawPropertyPtrRow obj)
	{
		return (int)obj.Property;
	}

	public bool Equals(RawPropertyRow x, RawPropertyRow y)
	{
		if (x.PropFlags == y.PropFlags && x.Name == y.Name)
		{
			return x.Type == y.Type;
		}
		return false;
	}

	public int GetHashCode(RawPropertyRow obj)
	{
		return obj.PropFlags + rol(obj.Name, 3) + rol(obj.Type, 7);
	}

	public bool Equals(RawMethodSemanticsRow x, RawMethodSemanticsRow y)
	{
		if (x.Semantic == y.Semantic && x.Method == y.Method)
		{
			return x.Association == y.Association;
		}
		return false;
	}

	public int GetHashCode(RawMethodSemanticsRow obj)
	{
		return obj.Semantic + rol(obj.Method, 3) + rol(obj.Association, 7);
	}

	public bool Equals(RawMethodImplRow x, RawMethodImplRow y)
	{
		if (x.Class == y.Class && x.MethodBody == y.MethodBody)
		{
			return x.MethodDeclaration == y.MethodDeclaration;
		}
		return false;
	}

	public int GetHashCode(RawMethodImplRow obj)
	{
		return (int)obj.Class + rol(obj.MethodBody, 3) + rol(obj.MethodDeclaration, 7);
	}

	public bool Equals(RawModuleRefRow x, RawModuleRefRow y)
	{
		return x.Name == y.Name;
	}

	public int GetHashCode(RawModuleRefRow obj)
	{
		return (int)obj.Name;
	}

	public bool Equals(RawTypeSpecRow x, RawTypeSpecRow y)
	{
		return x.Signature == y.Signature;
	}

	public int GetHashCode(RawTypeSpecRow obj)
	{
		return (int)obj.Signature;
	}

	public bool Equals(RawImplMapRow x, RawImplMapRow y)
	{
		if (x.MappingFlags == y.MappingFlags && x.MemberForwarded == y.MemberForwarded && x.ImportName == y.ImportName)
		{
			return x.ImportScope == y.ImportScope;
		}
		return false;
	}

	public int GetHashCode(RawImplMapRow obj)
	{
		return obj.MappingFlags + rol(obj.MemberForwarded, 3) + rol(obj.ImportName, 7) + rol(obj.ImportScope, 11);
	}

	public bool Equals(RawFieldRVARow x, RawFieldRVARow y)
	{
		if (x.RVA == y.RVA)
		{
			return x.Field == y.Field;
		}
		return false;
	}

	public int GetHashCode(RawFieldRVARow obj)
	{
		return (int)obj.RVA + rol(obj.Field, 3);
	}

	public bool Equals(RawENCLogRow x, RawENCLogRow y)
	{
		if (x.Token == y.Token)
		{
			return x.FuncCode == y.FuncCode;
		}
		return false;
	}

	public int GetHashCode(RawENCLogRow obj)
	{
		return (int)obj.Token + rol(obj.FuncCode, 3);
	}

	public bool Equals(RawENCMapRow x, RawENCMapRow y)
	{
		return x.Token == y.Token;
	}

	public int GetHashCode(RawENCMapRow obj)
	{
		return (int)obj.Token;
	}

	public bool Equals(RawAssemblyRow x, RawAssemblyRow y)
	{
		if (x.HashAlgId == y.HashAlgId && x.MajorVersion == y.MajorVersion && x.MinorVersion == y.MinorVersion && x.BuildNumber == y.BuildNumber && x.RevisionNumber == y.RevisionNumber && x.Flags == y.Flags && x.PublicKey == y.PublicKey && x.Name == y.Name)
		{
			return x.Locale == y.Locale;
		}
		return false;
	}

	public int GetHashCode(RawAssemblyRow obj)
	{
		return (int)obj.HashAlgId + rol(obj.MajorVersion, 3) + rol(obj.MinorVersion, 7) + rol(obj.BuildNumber, 11) + rol(obj.RevisionNumber, 15) + rol(obj.Flags, 19) + rol(obj.PublicKey, 23) + rol(obj.Name, 27) + rol(obj.Locale, 31);
	}

	public bool Equals(RawAssemblyProcessorRow x, RawAssemblyProcessorRow y)
	{
		return x.Processor == y.Processor;
	}

	public int GetHashCode(RawAssemblyProcessorRow obj)
	{
		return (int)obj.Processor;
	}

	public bool Equals(RawAssemblyOSRow x, RawAssemblyOSRow y)
	{
		if (x.OSPlatformId == y.OSPlatformId && x.OSMajorVersion == y.OSMajorVersion)
		{
			return x.OSMinorVersion == y.OSMinorVersion;
		}
		return false;
	}

	public int GetHashCode(RawAssemblyOSRow obj)
	{
		return (int)obj.OSPlatformId + rol(obj.OSMajorVersion, 3) + rol(obj.OSMinorVersion, 7);
	}

	public bool Equals(RawAssemblyRefRow x, RawAssemblyRefRow y)
	{
		if (x.MajorVersion == y.MajorVersion && x.MinorVersion == y.MinorVersion && x.BuildNumber == y.BuildNumber && x.RevisionNumber == y.RevisionNumber && x.Flags == y.Flags && x.PublicKeyOrToken == y.PublicKeyOrToken && x.Name == y.Name && x.Locale == y.Locale)
		{
			return x.HashValue == y.HashValue;
		}
		return false;
	}

	public int GetHashCode(RawAssemblyRefRow obj)
	{
		return obj.MajorVersion + rol(obj.MinorVersion, 3) + rol(obj.BuildNumber, 7) + rol(obj.RevisionNumber, 11) + rol(obj.Flags, 15) + rol(obj.PublicKeyOrToken, 19) + rol(obj.Name, 23) + rol(obj.Locale, 27) + rol(obj.HashValue, 31);
	}

	public bool Equals(RawAssemblyRefProcessorRow x, RawAssemblyRefProcessorRow y)
	{
		if (x.Processor == y.Processor)
		{
			return x.AssemblyRef == y.AssemblyRef;
		}
		return false;
	}

	public int GetHashCode(RawAssemblyRefProcessorRow obj)
	{
		return (int)obj.Processor + rol(obj.AssemblyRef, 3);
	}

	public bool Equals(RawAssemblyRefOSRow x, RawAssemblyRefOSRow y)
	{
		if (x.OSPlatformId == y.OSPlatformId && x.OSMajorVersion == y.OSMajorVersion && x.OSMinorVersion == y.OSMinorVersion)
		{
			return x.AssemblyRef == y.AssemblyRef;
		}
		return false;
	}

	public int GetHashCode(RawAssemblyRefOSRow obj)
	{
		return (int)obj.OSPlatformId + rol(obj.OSMajorVersion, 3) + rol(obj.OSMinorVersion, 7) + rol(obj.AssemblyRef, 11);
	}

	public bool Equals(RawFileRow x, RawFileRow y)
	{
		if (x.Flags == y.Flags && x.Name == y.Name)
		{
			return x.HashValue == y.HashValue;
		}
		return false;
	}

	public int GetHashCode(RawFileRow obj)
	{
		return (int)obj.Flags + rol(obj.Name, 3) + rol(obj.HashValue, 7);
	}

	public bool Equals(RawExportedTypeRow x, RawExportedTypeRow y)
	{
		if (x.Flags == y.Flags && x.TypeDefId == y.TypeDefId && x.TypeName == y.TypeName && x.TypeNamespace == y.TypeNamespace)
		{
			return x.Implementation == y.Implementation;
		}
		return false;
	}

	public int GetHashCode(RawExportedTypeRow obj)
	{
		return (int)obj.Flags + rol(obj.TypeDefId, 3) + rol(obj.TypeName, 7) + rol(obj.TypeNamespace, 11) + rol(obj.Implementation, 15);
	}

	public bool Equals(RawManifestResourceRow x, RawManifestResourceRow y)
	{
		if (x.Offset == y.Offset && x.Flags == y.Flags && x.Name == y.Name)
		{
			return x.Implementation == y.Implementation;
		}
		return false;
	}

	public int GetHashCode(RawManifestResourceRow obj)
	{
		return (int)obj.Offset + rol(obj.Flags, 3) + rol(obj.Name, 7) + rol(obj.Implementation, 11);
	}

	public bool Equals(RawNestedClassRow x, RawNestedClassRow y)
	{
		if (x.NestedClass == y.NestedClass)
		{
			return x.EnclosingClass == y.EnclosingClass;
		}
		return false;
	}

	public int GetHashCode(RawNestedClassRow obj)
	{
		return (int)obj.NestedClass + rol(obj.EnclosingClass, 3);
	}

	public bool Equals(RawGenericParamRow x, RawGenericParamRow y)
	{
		if (x.Number == y.Number && x.Flags == y.Flags && x.Owner == y.Owner && x.Name == y.Name)
		{
			return x.Kind == y.Kind;
		}
		return false;
	}

	public int GetHashCode(RawGenericParamRow obj)
	{
		return obj.Number + rol(obj.Flags, 3) + rol(obj.Owner, 7) + rol(obj.Name, 11) + rol(obj.Kind, 15);
	}

	public bool Equals(RawMethodSpecRow x, RawMethodSpecRow y)
	{
		if (x.Method == y.Method)
		{
			return x.Instantiation == y.Instantiation;
		}
		return false;
	}

	public int GetHashCode(RawMethodSpecRow obj)
	{
		return (int)obj.Method + rol(obj.Instantiation, 3);
	}

	public bool Equals(RawGenericParamConstraintRow x, RawGenericParamConstraintRow y)
	{
		if (x.Owner == y.Owner)
		{
			return x.Constraint == y.Constraint;
		}
		return false;
	}

	public int GetHashCode(RawGenericParamConstraintRow obj)
	{
		return (int)obj.Owner + rol(obj.Constraint, 3);
	}

	public bool Equals(RawDocumentRow x, RawDocumentRow y)
	{
		if (x.Name == y.Name && x.HashAlgorithm == y.HashAlgorithm && x.Hash == y.Hash)
		{
			return x.Language == y.Language;
		}
		return false;
	}

	public int GetHashCode(RawDocumentRow obj)
	{
		return (int)obj.Name + rol(obj.HashAlgorithm, 3) + rol(obj.Hash, 7) + rol(obj.Language, 11);
	}

	public bool Equals(RawMethodDebugInformationRow x, RawMethodDebugInformationRow y)
	{
		if (x.Document == y.Document)
		{
			return x.SequencePoints == y.SequencePoints;
		}
		return false;
	}

	public int GetHashCode(RawMethodDebugInformationRow obj)
	{
		return (int)obj.Document + rol(obj.SequencePoints, 3);
	}

	public bool Equals(RawLocalScopeRow x, RawLocalScopeRow y)
	{
		if (x.Method == y.Method && x.ImportScope == y.ImportScope && x.VariableList == y.VariableList && x.ConstantList == y.ConstantList && x.StartOffset == y.StartOffset)
		{
			return x.Length == y.Length;
		}
		return false;
	}

	public int GetHashCode(RawLocalScopeRow obj)
	{
		return (int)obj.Method + rol(obj.ImportScope, 3) + rol(obj.VariableList, 7) + rol(obj.ConstantList, 11) + rol(obj.StartOffset, 15) + rol(obj.Length, 19);
	}

	public bool Equals(RawLocalVariableRow x, RawLocalVariableRow y)
	{
		if (x.Attributes == y.Attributes && x.Index == y.Index)
		{
			return x.Name == y.Name;
		}
		return false;
	}

	public int GetHashCode(RawLocalVariableRow obj)
	{
		return obj.Attributes + rol(obj.Index, 3) + rol(obj.Name, 7);
	}

	public bool Equals(RawLocalConstantRow x, RawLocalConstantRow y)
	{
		if (x.Name == y.Name)
		{
			return x.Signature == y.Signature;
		}
		return false;
	}

	public int GetHashCode(RawLocalConstantRow obj)
	{
		return (int)obj.Name + rol(obj.Signature, 3);
	}

	public bool Equals(RawImportScopeRow x, RawImportScopeRow y)
	{
		if (x.Parent == y.Parent)
		{
			return x.Imports == y.Imports;
		}
		return false;
	}

	public int GetHashCode(RawImportScopeRow obj)
	{
		return (int)obj.Parent + rol(obj.Imports, 3);
	}

	public bool Equals(RawStateMachineMethodRow x, RawStateMachineMethodRow y)
	{
		if (x.MoveNextMethod == y.MoveNextMethod)
		{
			return x.KickoffMethod == y.KickoffMethod;
		}
		return false;
	}

	public int GetHashCode(RawStateMachineMethodRow obj)
	{
		return (int)obj.MoveNextMethod + rol(obj.KickoffMethod, 3);
	}

	public bool Equals(RawCustomDebugInformationRow x, RawCustomDebugInformationRow y)
	{
		if (x.Parent == y.Parent && x.Kind == y.Kind)
		{
			return x.Value == y.Value;
		}
		return false;
	}

	public int GetHashCode(RawCustomDebugInformationRow obj)
	{
		return (int)obj.Parent + rol(obj.Kind, 3) + rol(obj.Value, 7);
	}
}
