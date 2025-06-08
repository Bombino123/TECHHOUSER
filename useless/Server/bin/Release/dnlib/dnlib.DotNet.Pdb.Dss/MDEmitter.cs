using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;

namespace dnlib.DotNet.Pdb.Dss;

internal sealed class MDEmitter : MetaDataImport, IMetaDataEmit
{
	private readonly dnlib.DotNet.Writer.Metadata metadata;

	private readonly Dictionary<uint, TypeDef> tokenToTypeDef;

	private readonly Dictionary<uint, MethodDef> tokenToMethodDef;

	public MDEmitter(dnlib.DotNet.Writer.Metadata metadata)
	{
		this.metadata = metadata;
		tokenToTypeDef = new Dictionary<uint, TypeDef>(metadata.TablesHeap.TypeDefTable.Rows);
		tokenToMethodDef = new Dictionary<uint, MethodDef>(metadata.TablesHeap.MethodTable.Rows);
		foreach (TypeDef type in metadata.Module.GetTypes())
		{
			if (type == null)
			{
				continue;
			}
			tokenToTypeDef.Add(new MDToken(Table.TypeDef, metadata.GetRid(type)).Raw, type);
			foreach (MethodDef method in type.Methods)
			{
				if (method != null)
				{
					tokenToMethodDef.Add(new MDToken(Table.Method, metadata.GetRid(method)).Raw, method);
				}
			}
		}
	}

	public unsafe override void GetMethodProps(uint mb, uint* pClass, ushort* szMethod, uint cchMethod, uint* pchMethod, uint* pdwAttr, IntPtr* ppvSigBlob, uint* pcbSigBlob, uint* pulCodeRVA, uint* pdwImplFlags)
	{
		if (mb >> 24 != 6)
		{
			throw new ArgumentException();
		}
		MethodDef methodDef = tokenToMethodDef[mb];
		RawMethodRow rawMethodRow = metadata.TablesHeap.MethodTable[mb & 0xFFFFFFu];
		if (pClass != null)
		{
			*pClass = new MDToken(Table.TypeDef, metadata.GetRid(methodDef.DeclaringType)).Raw;
		}
		if (pdwAttr != null)
		{
			*pdwAttr = rawMethodRow.Flags;
		}
		if (ppvSigBlob != null)
		{
			*ppvSigBlob = IntPtr.Zero;
		}
		if (pcbSigBlob != null)
		{
			*pcbSigBlob = 0u;
		}
		if (pulCodeRVA != null)
		{
			*pulCodeRVA = rawMethodRow.RVA;
		}
		if (pdwImplFlags != null)
		{
			*pdwImplFlags = rawMethodRow.ImplFlags;
		}
		string text = methodDef.Name.String ?? string.Empty;
		int num = (int)Math.Min((uint)(text.Length + 1), cchMethod);
		if (szMethod != null)
		{
			int num2 = 0;
			while (num2 < num - 1)
			{
				*szMethod = text[num2];
				num2++;
				szMethod++;
			}
			if (num > 0)
			{
				*szMethod = 0;
			}
		}
		if (pchMethod != null)
		{
			*pchMethod = (uint)num;
		}
	}

	public unsafe override void GetTypeDefProps(uint td, ushort* szTypeDef, uint cchTypeDef, uint* pchTypeDef, uint* pdwTypeDefFlags, uint* ptkExtends)
	{
		if (td >> 24 != 2)
		{
			throw new ArgumentException();
		}
		TypeDef typeDef = tokenToTypeDef[td];
		RawTypeDefRow rawTypeDefRow = metadata.TablesHeap.TypeDefTable[td & 0xFFFFFFu];
		if (pdwTypeDefFlags != null)
		{
			*pdwTypeDefFlags = rawTypeDefRow.Flags;
		}
		if (ptkExtends != null)
		{
			*ptkExtends = rawTypeDefRow.Extends;
		}
		CopyTypeName(typeDef.Namespace, typeDef.Name, szTypeDef, cchTypeDef, pchTypeDef);
	}

	public unsafe override void GetNestedClassProps(uint tdNestedClass, uint* ptdEnclosingClass)
	{
		if (tdNestedClass >> 24 != 2)
		{
			throw new ArgumentException();
		}
		TypeDef declaringType = tokenToTypeDef[tdNestedClass].DeclaringType;
		if (ptdEnclosingClass != null)
		{
			if (declaringType == null)
			{
				*ptdEnclosingClass = 0u;
			}
			else
			{
				*ptdEnclosingClass = new MDToken(Table.TypeDef, metadata.GetRid(declaringType)).Raw;
			}
		}
	}

	void IMetaDataEmit.GetTokenFromSig(IntPtr pvSig, uint cbSig, out uint pmsig)
	{
		pmsig = 285212672u;
	}

	void IMetaDataEmit.SetModuleProps(string szName)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.Save(string szFile, uint dwSaveFlags)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SaveToStream(IStream pIStream, uint dwSaveFlags)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.GetSaveSize(int fSave, out uint pdwSaveSize)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineTypeDef(string szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint[] rtkImplements, out uint ptd)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineNestedType(string szTypeDef, uint dwTypeDefFlags, uint tkExtends, uint[] rtkImplements, uint tdEncloser, out uint ptd)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetHandler(object pUnk)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineMethod(uint td, string szName, uint dwMethodFlags, IntPtr pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags, out uint pmd)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineMethodImpl(uint td, uint tkBody, uint tkDecl)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineTypeRefByName(uint tkResolutionScope, string szName, out uint ptr)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineImportType(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint tdImport, IntPtr pAssemEmit, out uint ptr)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineMemberRef(uint tkImport, string szName, IntPtr pvSigBlob, uint cbSigBlob, out uint pmr)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineImportMember(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent, out uint pmr)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint[] rmdOtherMethods, out uint pmdEvent)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetClassLayout(uint td, uint dwPackSize, IntPtr rFieldOffsets, uint ulClassSize)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DeleteClassLayout(uint td)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetFieldMarshal(uint tk, IntPtr pvNativeType, uint cbNativeType)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DeleteFieldMarshal(uint tk)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefinePermissionSet(uint tk, uint dwAction, IntPtr pvPermission, uint cbPermission, out uint ppm)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetRVA(uint md, uint ulRVA)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineModuleRef(string szName, out uint pmur)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetParent(uint mr, uint tk)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.GetTokenFromTypeSpec(IntPtr pvSig, uint cbSig, out uint ptypespec)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SaveToMemory(out IntPtr pbData, uint cbData)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineUserString(string szString, uint cchString, out uint pstk)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DeleteToken(uint tkObj)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, uint[] rtkImplements)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, uint[] rmdOtherMethods)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetPermissionSetProps(uint tk, uint dwAction, IntPtr pvPermission, uint cbPermission, out uint ppm)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DeletePinvokeMap(uint tk)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineCustomAttribute(uint tkOwner, uint tkCtor, IntPtr pCustomAttribute, uint cbCustomAttribute, out uint pcv)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetCustomAttributeValue(uint pcv, IntPtr pCustomAttribute, uint cbCustomAttribute)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineField(uint td, string szName, uint dwFieldFlags, IntPtr pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, out uint pmd)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineProperty(uint td, string szProperty, uint dwPropFlags, IntPtr pvSig, uint cbSig, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, uint mdSetter, uint mdGetter, uint[] rmdOtherMethods, out uint pmdProp)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, out uint ppd)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, uint mdSetter, uint mdGetter, uint[] rmdOtherMethods)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs, out uint pulErrorAttr)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.ApplyEditAndContinue(object pImport)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.TranslateSigWithScope(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport import, IntPtr pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, IntPtr pvTranslatedSig, uint cbTranslatedSigMax, out uint pcbTranslatedSig)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetMethodImplFlags(uint md, uint dwImplFlags)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.SetFieldRVA(uint fd, uint ulRVA)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.Merge(IMetaDataImport pImport, IntPtr pHostMapToken, object pHandler)
	{
		throw new NotImplementedException();
	}

	void IMetaDataEmit.MergeEnd()
	{
		throw new NotImplementedException();
	}
}
