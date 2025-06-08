using System;
using System.Runtime.InteropServices;

namespace dnlib.DotNet.Pdb.Dss;

[ComImport]
[ComVisible(true)]
[Guid("7DAC8207-D3AE-4C75-9B67-92801A497D44")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMetaDataImport
{
	void CloseEnum(IntPtr hEnum);

	void CountEnum(IntPtr hEnum, ref uint pulCount);

	void ResetEnum(IntPtr hEnum, uint ulPos);

	void EnumTypeDefs(IntPtr phEnum, uint[] rTypeDefs, uint cMax, out uint pcTypeDefs);

	void EnumInterfaceImpls(ref IntPtr phEnum, uint td, uint[] rImpls, uint cMax, ref uint pcImpls);

	void EnumTypeRefs(ref IntPtr phEnum, uint[] rTypeRefs, uint cMax, ref uint pcTypeRefs);

	void FindTypeDefByName([In][MarshalAs(UnmanagedType.LPWStr)] string szTypeDef, [In] uint tkEnclosingClass, out uint ptd);

	void GetScopeProps([Out] IntPtr szName, [In] uint cchName, out uint pchName, out Guid pmvid);

	void GetModuleFromScope(out uint pmd);

	unsafe void GetTypeDefProps([In] uint td, [In] ushort* szTypeDef, [In] uint cchTypeDef, [Out] uint* pchTypeDef, [Out] uint* pdwTypeDefFlags, [Out] uint* ptkExtends);

	void GetInterfaceImplProps([In] uint iiImpl, out uint pClass, out uint ptkIface);

	unsafe void GetTypeRefProps([In] uint tr, [Out] uint* ptkResolutionScope, [Out] ushort* szName, [In] uint cchName, [Out] uint* pchName);

	void ResolveTypeRef(uint tr, ref Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppIScope, out uint ptd);

	void EnumMembers([In][Out] ref IntPtr phEnum, [In] uint cl, [Out] uint[] rMembers, [In] uint cMax, out uint pcTokens);

	void EnumMembersWithName([In][Out] ref IntPtr phEnum, [In] uint cl, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [Out] uint[] rMembers, [In] uint cMax, out uint pcTokens);

	void EnumMethods([In][Out] ref IntPtr phEnum, [In] uint cl, [Out] uint[] rMethods, [In] uint cMax, out uint pcTokens);

	void EnumMethodsWithName([In][Out] ref IntPtr phEnum, [In] uint cl, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, uint[] rMethods, [In] uint cMax, out uint pcTokens);

	void EnumFields([In][Out] ref IntPtr phEnum, [In] uint cl, [Out] uint[] rFields, [In] uint cMax, out uint pcTokens);

	void EnumFieldsWithName([In][Out] ref IntPtr phEnum, [In] uint cl, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [Out] uint[] rFields, [In] uint cMax, out uint pcTokens);

	void EnumParams([In][Out] ref IntPtr phEnum, [In] uint mb, [Out] uint[] rParams, [In] uint cMax, out uint pcTokens);

	void EnumMemberRefs([In][Out] ref IntPtr phEnum, [In] uint tkParent, [Out] uint[] rMemberRefs, [In] uint cMax, out uint pcTokens);

	void EnumMethodImpls([In][Out] ref IntPtr phEnum, [In] uint td, [Out] uint[] rMethodBody, [Out] uint[] rMethodDecl, [In] uint cMax, out uint pcTokens);

	void EnumPermissionSets([In][Out] ref IntPtr phEnum, [In] uint tk, [In] uint dwActions, [Out] uint[] rPermission, [In] uint cMax, out uint pcTokens);

	void FindMember([In] uint td, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] IntPtr pvSigBlob, [In] uint cbSigBlob, out uint pmb);

	void FindMethod([In] uint td, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] IntPtr pvSigBlob, [In] uint cbSigBlob, out uint pmb);

	void FindField([In] uint td, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] IntPtr pvSigBlob, [In] uint cbSigBlob, out uint pmb);

	void FindMemberRef([In] uint td, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, [In] IntPtr pvSigBlob, [In] uint cbSigBlob, out uint pmr);

	unsafe void GetMethodProps(uint mb, uint* pClass, [In] ushort* szMethod, uint cchMethod, uint* pchMethod, uint* pdwAttr, [Out] IntPtr* ppvSigBlob, [Out] uint* pcbSigBlob, [Out] uint* pulCodeRVA, [Out] uint* pdwImplFlags);

	void GetMemberRefProps([In] uint mr, out uint ptk, [Out] IntPtr szMember, [In] uint cchMember, out uint pchMember, out IntPtr ppvSigBlob, out uint pbSig);

	void EnumProperties([In][Out] ref IntPtr phEnum, [In] uint td, [Out] uint[] rProperties, [In] uint cMax, out uint pcProperties);

	void EnumEvents([In][Out] ref IntPtr phEnum, [In] uint td, [Out] uint[] rEvents, [In] uint cMax, out uint pcEvents);

	void GetEventProps([In] uint ev, out uint pClass, [Out][MarshalAs(UnmanagedType.LPWStr)] string szEvent, [In] uint cchEvent, out uint pchEvent, out uint pdwEventFlags, out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire, [In][Out] uint[] rmdOtherMethod, [In] uint cMax, out uint pcOtherMethod);

	void EnumMethodSemantics([In][Out] ref IntPtr phEnum, [In] uint mb, [In][Out] uint[] rEventProp, [In] uint cMax, out uint pcEventProp);

	void GetMethodSemantics([In] uint mb, [In] uint tkEventProp, out uint pdwSemanticsFlags);

	void GetClassLayout([In] uint td, out uint pdwPackSize, out IntPtr rFieldOffset, [In] uint cMax, out uint pcFieldOffset, out uint pulClassSize);

	void GetFieldMarshal([In] uint tk, out IntPtr ppvNativeType, out uint pcbNativeType);

	void GetRVA(uint tk, out uint pulCodeRVA, out uint pdwImplFlags);

	void GetPermissionSetProps([In] uint pm, out uint pdwAction, out IntPtr ppvPermission, out uint pcbPermission);

	unsafe void GetSigFromToken([In] uint mdSig, [Out] byte** ppvSig, [Out] uint* pcbSig);

	void GetModuleRefProps([In] uint mur, [Out] IntPtr szName, [In] uint cchName, out uint pchName);

	void EnumModuleRefs([In][Out] ref IntPtr phEnum, [Out] uint[] rModuleRefs, [In] uint cmax, out uint pcModuleRefs);

	void GetTypeSpecFromToken([In] uint typespec, out IntPtr ppvSig, out uint pcbSig);

	void GetNameFromToken([In] uint tk, out IntPtr pszUtf8NamePtr);

	void EnumUnresolvedMethods([In][Out] ref IntPtr phEnum, [Out] uint[] rMethods, [In] uint cMax, out uint pcTokens);

	void GetUserString([In] uint stk, [Out] IntPtr szString, [In] uint cchString, out uint pchString);

	void GetPinvokeMap([In] uint tk, out uint pdwMappingFlags, [Out] IntPtr szImportName, [In] uint cchImportName, out uint pchImportName, out uint pmrImportDLL);

	void EnumSignatures([In][Out] ref IntPtr phEnum, [Out] uint[] rSignatures, [In] uint cmax, out uint pcSignatures);

	void EnumTypeSpecs([In][Out] ref IntPtr phEnum, [Out] uint[] rTypeSpecs, [In] uint cmax, out uint pcTypeSpecs);

	void EnumUserStrings([In][Out] ref IntPtr phEnum, [Out] uint[] rStrings, [In] uint cmax, out uint pcStrings);

	void GetParamForMethodIndex([In] uint md, [In] uint ulParamSeq, out uint ppd);

	void EnumCustomAttributes([In][Out] IntPtr phEnum, [In] uint tk, [In] uint tkType, [Out] uint[] rCustomAttributes, [In] uint cMax, out uint pcCustomAttributes);

	void GetCustomAttributeProps([In] uint cv, out uint ptkObj, out uint ptkType, out IntPtr ppBlob, out uint pcbSize);

	void FindTypeRef([In] uint tkResolutionScope, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, out uint ptr);

	void GetMemberProps(uint mb, out uint pClass, IntPtr szMember, uint cchMember, out uint pchMember, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out IntPtr ppValue, out uint pcchValue);

	void GetFieldProps(uint mb, out uint pClass, IntPtr szField, uint cchField, out uint pchField, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out IntPtr ppValue, out uint pcchValue);

	void GetPropertyProps([In] uint prop, out uint pClass, [Out] IntPtr szProperty, [In] uint cchProperty, out uint pchProperty, out uint pdwPropFlags, out IntPtr ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out IntPtr ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter, out uint pmdGetter, [In][Out] uint[] rmdOtherMethod, [In] uint cMax, out uint pcOtherMethod);

	void GetParamProps([In] uint tk, out uint pmd, out uint pulSequence, [Out] IntPtr szName, [Out] uint cchName, out uint pchName, out uint pdwAttr, out uint pdwCPlusTypeFlag, out IntPtr ppValue, out uint pcchValue);

	void GetCustomAttributeByName([In] uint tkObj, [In][MarshalAs(UnmanagedType.LPWStr)] string szName, out IntPtr ppData, out uint pcbData);

	bool IsValidToken([In] uint tk);

	unsafe void GetNestedClassProps([In] uint tdNestedClass, [Out] uint* ptdEnclosingClass);

	void GetNativeCallConvFromSig([In] IntPtr pvSig, [In] uint cbSig, out uint pCallConv);

	void IsGlobal([In] uint pd, out int pbGlobal);
}
