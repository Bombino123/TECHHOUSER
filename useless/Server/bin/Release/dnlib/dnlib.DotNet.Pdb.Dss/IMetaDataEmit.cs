using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("BA3FEE4C-ECB9-4E41-83B7-183FA41CD859")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMetaDataEmit
{
	void SetModuleProps([In][MarshalAs(UnmanagedType.LPWStr)] string szName);

	void Save([In][MarshalAs(UnmanagedType.LPWStr)] string szFile, [In] uint dwSaveFlags);

	void SaveToStream([In] IStream pIStream, [In] uint dwSaveFlags);

	void GetSaveSize([In] int fSave, out uint pdwSaveSize);

	void DefineTypeDef([In][MarshalAs(UnmanagedType.LPWStr)] string szTypeDef, [In] uint dwTypeDefFlags, [In] uint tkExtends, [In] uint[] rtkImplements, out uint ptd);

	void DefineNestedType([In][MarshalAs(UnmanagedType.LPWStr)] string szTypeDef, [In] uint dwTypeDefFlags, [In] uint tkExtends, [In] uint[] rtkImplements, [In] uint tdEncloser, out uint ptd);

	void SetHandler([In][MarshalAs(UnmanagedType.IUnknown)] object pUnk);

	void DefineMethod(uint td, [MarshalAs(UnmanagedType.LPWStr)] string szName, uint dwMethodFlags, [In] IntPtr pvSigBlob, [In] uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags, out uint pmd);

	void DefineMethodImpl([In] uint td, [In] uint tkBody, [In] uint tkDecl);

	void DefineTypeRefByName([In] uint tkResolutionScope, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, out uint ptr);

	void DefineImportType([In] IntPtr pAssemImport, [In] IntPtr pbHashValue, [In] uint cbHashValue, [In] IMetaDataImport pImport, [In] uint tdImport, [In] IntPtr pAssemEmit, out uint ptr);

	void DefineMemberRef([In] uint tkImport, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] IntPtr pvSigBlob, [In] uint cbSigBlob, out uint pmr);

	void DefineImportMember([In] IntPtr pAssemImport, [In] IntPtr pbHashValue, [In] uint cbHashValue, [In] IMetaDataImport pImport, [In] uint mbMember, [In] IntPtr pAssemEmit, [In] uint tkParent, out uint pmr);

	void DefineEvent([In] uint td, [In][MarshalAs(UnmanagedType.LPWStr)] string szEvent, [In] uint dwEventFlags, [In] uint tkEventType, [In] uint mdAddOn, [In] uint mdRemoveOn, [In] uint mdFire, [In] uint[] rmdOtherMethods, out uint pmdEvent);

	void SetClassLayout([In] uint td, [In] uint dwPackSize, [In] IntPtr rFieldOffsets, [In] uint ulClassSize);

	void DeleteClassLayout([In] uint td);

	void SetFieldMarshal([In] uint tk, [In] IntPtr pvNativeType, [In] uint cbNativeType);

	void DeleteFieldMarshal([In] uint tk);

	void DefinePermissionSet([In] uint tk, [In] uint dwAction, [In] IntPtr pvPermission, [In] uint cbPermission, out uint ppm);

	void SetRVA([In] uint md, [In] uint ulRVA);

	void GetTokenFromSig([In] IntPtr pvSig, [In] uint cbSig, out uint pmsig);

	void DefineModuleRef([In][MarshalAs(UnmanagedType.LPWStr)] string szName, out uint pmur);

	void SetParent([In] uint mr, [In] uint tk);

	void GetTokenFromTypeSpec([In] IntPtr pvSig, [In] uint cbSig, out uint ptypespec);

	void SaveToMemory(out IntPtr pbData, [In] uint cbData);

	void DefineUserString([In][MarshalAs(UnmanagedType.LPWStr)] string szString, [In] uint cchString, out uint pstk);

	void DeleteToken([In] uint tkObj);

	void SetMethodProps([In] uint md, [In] uint dwMethodFlags, [In] uint ulCodeRVA, [In] uint dwImplFlags);

	void SetTypeDefProps([In] uint td, [In] uint dwTypeDefFlags, [In] uint tkExtends, [In] uint[] rtkImplements);

	void SetEventProps([In] uint ev, [In] uint dwEventFlags, [In] uint tkEventType, [In] uint mdAddOn, [In] uint mdRemoveOn, [In] uint mdFire, [In] uint[] rmdOtherMethods);

	void SetPermissionSetProps([In] uint tk, [In] uint dwAction, [In] IntPtr pvPermission, [In] uint cbPermission, out uint ppm);

	void DefinePinvokeMap([In] uint tk, [In] uint dwMappingFlags, [In][MarshalAs(UnmanagedType.LPWStr)] string szImportName, [In] uint mrImportDLL);

	void SetPinvokeMap([In] uint tk, [In] uint dwMappingFlags, [In][MarshalAs(UnmanagedType.LPWStr)] string szImportName, [In] uint mrImportDLL);

	void DeletePinvokeMap([In] uint tk);

	void DefineCustomAttribute([In] uint tkOwner, [In] uint tkCtor, [In] IntPtr pCustomAttribute, [In] uint cbCustomAttribute, out uint pcv);

	void SetCustomAttributeValue([In] uint pcv, [In] IntPtr pCustomAttribute, [In] uint cbCustomAttribute);

	void DefineField(uint td, [MarshalAs(UnmanagedType.LPWStr)] string szName, uint dwFieldFlags, [In] IntPtr pvSigBlob, [In] uint cbSigBlob, [In] uint dwCPlusTypeFlag, [In] IntPtr pValue, [In] uint cchValue, out uint pmd);

	void DefineProperty([In] uint td, [In][MarshalAs(UnmanagedType.LPWStr)] string szProperty, [In] uint dwPropFlags, [In] IntPtr pvSig, [In] uint cbSig, [In] uint dwCPlusTypeFlag, [In] IntPtr pValue, [In] uint cchValue, [In] uint mdSetter, [In] uint mdGetter, [In] uint[] rmdOtherMethods, out uint pmdProp);

	void DefineParam([In] uint md, [In] uint ulParamSeq, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] uint dwParamFlags, [In] uint dwCPlusTypeFlag, [In] IntPtr pValue, [In] uint cchValue, out uint ppd);

	void SetFieldProps([In] uint fd, [In] uint dwFieldFlags, [In] uint dwCPlusTypeFlag, [In] IntPtr pValue, [In] uint cchValue);

	void SetPropertyProps([In] uint pr, [In] uint dwPropFlags, [In] uint dwCPlusTypeFlag, [In] IntPtr pValue, [In] uint cchValue, [In] uint mdSetter, [In] uint mdGetter, [In] uint[] rmdOtherMethods);

	void SetParamProps([In] uint pd, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] uint dwParamFlags, [In] uint dwCPlusTypeFlag, [Out] IntPtr pValue, [In] uint cchValue);

	void DefineSecurityAttributeSet([In] uint tkObj, [In] IntPtr rSecAttrs, [In] uint cSecAttrs, out uint pulErrorAttr);

	void ApplyEditAndContinue([In][MarshalAs(UnmanagedType.IUnknown)] object pImport);

	void TranslateSigWithScope([In] IntPtr pAssemImport, [In] IntPtr pbHashValue, [In] uint cbHashValue, [In] IMetaDataImport import, [In] IntPtr pbSigBlob, [In] uint cbSigBlob, [In] IntPtr pAssemEmit, [In] IMetaDataEmit emit, [Out] IntPtr pvTranslatedSig, uint cbTranslatedSigMax, out uint pcbTranslatedSig);

	void SetMethodImplFlags([In] uint md, uint dwImplFlags);

	void SetFieldRVA([In] uint fd, [In] uint ulRVA);

	void Merge([In] IMetaDataImport pImport, [In] IntPtr pHostMapToken, [In][MarshalAs(UnmanagedType.IUnknown)] object pHandler);

	void MergeEnd();
}
