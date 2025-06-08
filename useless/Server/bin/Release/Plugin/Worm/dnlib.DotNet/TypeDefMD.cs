using System.Collections.Generic;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Utils;

namespace dnlib.DotNet;

internal sealed class TypeDefMD : TypeDef, IMDTokenProviderMD, IMDTokenProvider
{
	private readonly struct MethodOverrideTokens
	{
		public readonly uint MethodBodyToken;

		public readonly uint MethodDeclarationToken;

		public MethodOverrideTokens(uint methodBodyToken, uint methodDeclarationToken)
		{
			MethodBodyToken = methodBodyToken;
			MethodDeclarationToken = methodDeclarationToken;
		}
	}

	private readonly ModuleDefMD readerModule;

	private readonly uint origRid;

	private readonly uint extendsCodedToken;

	private Dictionary<uint, IList<MethodOverrideTokens>> methodRidToOverrides;

	internal ModuleDefMD ReaderModule => readerModule;

	public uint OrigRid => origRid;

	private TypeDef DeclaringType2_NoLock
	{
		get
		{
			if (!declaringType2_isInitialized)
			{
				declaringType2 = GetDeclaringType2_NoLock();
				declaringType2_isInitialized = true;
			}
			return declaringType2;
		}
	}

	protected override ITypeDefOrRef GetBaseType_NoLock()
	{
		return readerModule.ResolveTypeDefOrRef(extendsCodedToken, new GenericParamContext(this));
	}

	protected override void InitializeFields()
	{
		RidList fieldRidList = readerModule.Metadata.GetFieldRidList(origRid);
		LazyList<FieldDef, RidList> value = new LazyList<FieldDef, RidList>(fieldRidList.Count, this, fieldRidList, (RidList list2, int index) => readerModule.ResolveField(list2[index]));
		Interlocked.CompareExchange(ref fields, value, null);
	}

	protected override void InitializeMethods()
	{
		RidList methodRidList = readerModule.Metadata.GetMethodRidList(origRid);
		LazyList<MethodDef, RidList> value = new LazyList<MethodDef, RidList>(methodRidList.Count, this, methodRidList, (RidList list2, int index) => readerModule.ResolveMethod(list2[index]));
		Interlocked.CompareExchange(ref methods, value, null);
	}

	protected override void InitializeGenericParameters()
	{
		RidList genericParamRidList = readerModule.Metadata.GetGenericParamRidList(Table.TypeDef, origRid);
		LazyList<GenericParam, RidList> value = new LazyList<GenericParam, RidList>(genericParamRidList.Count, this, genericParamRidList, (RidList list2, int index) => readerModule.ResolveGenericParam(list2[index]));
		Interlocked.CompareExchange(ref genericParameters, value, null);
	}

	protected override void InitializeInterfaces()
	{
		RidList interfaceImplRidList = readerModule.Metadata.GetInterfaceImplRidList(origRid);
		LazyList<InterfaceImpl, RidList> value = new LazyList<InterfaceImpl, RidList>(interfaceImplRidList.Count, interfaceImplRidList, (RidList list2, int index) => readerModule.ResolveInterfaceImpl(list2[index], new GenericParamContext(this)));
		Interlocked.CompareExchange(ref interfaces, value, null);
	}

	protected override void InitializeDeclSecurities()
	{
		RidList declSecurityRidList = readerModule.Metadata.GetDeclSecurityRidList(Table.TypeDef, origRid);
		LazyList<DeclSecurity, RidList> value = new LazyList<DeclSecurity, RidList>(declSecurityRidList.Count, declSecurityRidList, (RidList list2, int index) => readerModule.ResolveDeclSecurity(list2[index]));
		Interlocked.CompareExchange(ref declSecurities, value, null);
	}

	protected override ClassLayout GetClassLayout_NoLock()
	{
		return readerModule.ResolveClassLayout(readerModule.Metadata.GetClassLayoutRid(origRid));
	}

	protected override TypeDef GetDeclaringType2_NoLock()
	{
		if (!readerModule.TablesStream.TryReadNestedClassRow(readerModule.Metadata.GetNestedClassRid(origRid), out var row))
		{
			return null;
		}
		return readerModule.ResolveTypeDef(row.EnclosingClass);
	}

	protected override void InitializeEvents()
	{
		uint eventMapRid = readerModule.Metadata.GetEventMapRid(origRid);
		RidList eventRidList = readerModule.Metadata.GetEventRidList(eventMapRid);
		LazyList<EventDef, RidList> value = new LazyList<EventDef, RidList>(eventRidList.Count, this, eventRidList, (RidList list2, int index) => readerModule.ResolveEvent(list2[index]));
		Interlocked.CompareExchange(ref events, value, null);
	}

	protected override void InitializeProperties()
	{
		uint propertyMapRid = readerModule.Metadata.GetPropertyMapRid(origRid);
		RidList propertyRidList = readerModule.Metadata.GetPropertyRidList(propertyMapRid);
		LazyList<PropertyDef, RidList> value = new LazyList<PropertyDef, RidList>(propertyRidList.Count, this, propertyRidList, (RidList list2, int index) => readerModule.ResolveProperty(list2[index]));
		Interlocked.CompareExchange(ref properties, value, null);
	}

	protected override void InitializeNestedTypes()
	{
		RidList nestedClassRidList = readerModule.Metadata.GetNestedClassRidList(origRid);
		LazyList<TypeDef, RidList> value = new LazyList<TypeDef, RidList>(nestedClassRidList.Count, this, nestedClassRidList, (RidList list2, int index) => readerModule.ResolveTypeDef(list2[index]));
		Interlocked.CompareExchange(ref nestedTypes, value, null);
	}

	protected override void InitializeCustomAttributes()
	{
		RidList list3 = readerModule.Metadata.GetCustomAttributeRidList(Table.TypeDef, origRid);
		CustomAttributeCollection value = new CustomAttributeCollection(list3.Count, list3, (object list2, int index) => readerModule.ReadCustomAttribute(list3[index]));
		Interlocked.CompareExchange(ref customAttributes, value, null);
	}

	protected override void InitializeCustomDebugInfos()
	{
		List<PdbCustomDebugInfo> list = new List<PdbCustomDebugInfo>();
		readerModule.InitializeCustomDebugInfos(new MDToken(base.MDToken.Table, origRid), new GenericParamContext(this), list);
		Interlocked.CompareExchange(ref customDebugInfos, list, null);
	}

	protected override ModuleDef GetModule2_NoLock()
	{
		if (DeclaringType2_NoLock == null)
		{
			return readerModule;
		}
		return null;
	}

	public TypeDefMD(ModuleDefMD readerModule, uint rid)
	{
		origRid = rid;
		base.rid = rid;
		this.readerModule = readerModule;
		readerModule.TablesStream.TryReadTypeDefRow(origRid, out var row);
		extendsCodedToken = row.Extends;
		attributes = (int)row.Flags;
		name = readerModule.StringsStream.ReadNoNull(row.Name);
		@namespace = readerModule.StringsStream.ReadNoNull(row.Namespace);
	}

	internal IList<MethodOverride> GetMethodOverrides(MethodDefMD method, GenericParamContext gpContext)
	{
		if (method == null)
		{
			return new List<MethodOverride>();
		}
		if (methodRidToOverrides == null)
		{
			InitializeMethodOverrides();
		}
		if (methodRidToOverrides.TryGetValue(method.OrigRid, out var value))
		{
			List<MethodOverride> list = new List<MethodOverride>(value.Count);
			for (int i = 0; i < value.Count; i++)
			{
				MethodOverrideTokens methodOverrideTokens = value[i];
				IMethodDefOrRef methodBody = (IMethodDefOrRef)readerModule.ResolveToken(methodOverrideTokens.MethodBodyToken, gpContext);
				IMethodDefOrRef methodDeclaration = (IMethodDefOrRef)readerModule.ResolveToken(methodOverrideTokens.MethodDeclarationToken, gpContext);
				list.Add(new MethodOverride(methodBody, methodDeclaration));
			}
			return list;
		}
		return new List<MethodOverride>();
	}

	private void InitializeMethodOverrides()
	{
		Dictionary<uint, IList<MethodOverrideTokens>> dictionary = new Dictionary<uint, IList<MethodOverrideTokens>>();
		RidList methodImplRidList = readerModule.Metadata.GetMethodImplRidList(origRid);
		for (int i = 0; i < methodImplRidList.Count; i++)
		{
			if (!readerModule.TablesStream.TryReadMethodImplRow(methodImplRidList[i], out var row))
			{
				continue;
			}
			IMethodDefOrRef methodDefOrRef = readerModule.ResolveMethodDefOrRef(row.MethodBody);
			IMethodDefOrRef methodDefOrRef2 = readerModule.ResolveMethodDefOrRef(row.MethodDeclaration);
			if (methodDefOrRef == null || methodDefOrRef2 == null)
			{
				continue;
			}
			MethodDef methodDef = FindMethodImplMethod(methodDefOrRef);
			if (methodDef != null && methodDef.DeclaringType == this)
			{
				uint key = methodDef.Rid;
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = new List<MethodOverrideTokens>());
				}
				value.Add(new MethodOverrideTokens(methodDefOrRef.MDToken.Raw, methodDefOrRef2.MDToken.Raw));
			}
		}
		Interlocked.CompareExchange(ref methodRidToOverrides, dictionary, null);
	}

	internal void InitializeMethodSemanticsAttributes()
	{
		uint propertyMapRid = readerModule.Metadata.GetPropertyMapRid(origRid);
		RidList propertyRidList = readerModule.Metadata.GetPropertyRidList(propertyMapRid);
		for (int i = 0; i < propertyRidList.Count; i++)
		{
			RidList methodSemanticsRidList = readerModule.Metadata.GetMethodSemanticsRidList(Table.Property, propertyRidList[i]);
			for (int j = 0; j < methodSemanticsRidList.Count; j++)
			{
				if (readerModule.TablesStream.TryReadMethodSemanticsRow(methodSemanticsRidList[j], out var row))
				{
					MethodDef methodDef = readerModule.ResolveMethod(row.Method);
					if (methodDef != null)
					{
						Interlocked.CompareExchange(ref methodDef.semAttrs, row.Semantic | MethodDef.SEMATTRS_INITD, 0);
					}
				}
			}
		}
		propertyMapRid = readerModule.Metadata.GetEventMapRid(origRid);
		propertyRidList = readerModule.Metadata.GetEventRidList(propertyMapRid);
		for (int k = 0; k < propertyRidList.Count; k++)
		{
			RidList methodSemanticsRidList2 = readerModule.Metadata.GetMethodSemanticsRidList(Table.Event, propertyRidList[k]);
			for (int l = 0; l < methodSemanticsRidList2.Count; l++)
			{
				if (readerModule.TablesStream.TryReadMethodSemanticsRow(methodSemanticsRidList2[l], out var row2))
				{
					MethodDef methodDef2 = readerModule.ResolveMethod(row2.Method);
					if (methodDef2 != null)
					{
						Interlocked.CompareExchange(ref methodDef2.semAttrs, row2.Semantic | MethodDef.SEMATTRS_INITD, 0);
					}
				}
			}
		}
	}

	internal void InitializeProperty(PropertyDefMD prop, out IList<MethodDef> getMethods, out IList<MethodDef> setMethods, out IList<MethodDef> otherMethods)
	{
		getMethods = new List<MethodDef>();
		setMethods = new List<MethodDef>();
		otherMethods = new List<MethodDef>();
		if (prop == null)
		{
			return;
		}
		RidList methodSemanticsRidList = readerModule.Metadata.GetMethodSemanticsRidList(Table.Property, prop.OrigRid);
		for (int i = 0; i < methodSemanticsRidList.Count; i++)
		{
			if (!readerModule.TablesStream.TryReadMethodSemanticsRow(methodSemanticsRidList[i], out var row))
			{
				continue;
			}
			MethodDef methodDef = readerModule.ResolveMethod(row.Method);
			if (methodDef == null || methodDef.DeclaringType != prop.DeclaringType)
			{
				continue;
			}
			switch ((MethodSemanticsAttributes)row.Semantic)
			{
			case MethodSemanticsAttributes.Setter:
				if (!setMethods.Contains(methodDef))
				{
					setMethods.Add(methodDef);
				}
				break;
			case MethodSemanticsAttributes.Getter:
				if (!getMethods.Contains(methodDef))
				{
					getMethods.Add(methodDef);
				}
				break;
			case MethodSemanticsAttributes.Other:
				if (!otherMethods.Contains(methodDef))
				{
					otherMethods.Add(methodDef);
				}
				break;
			}
		}
	}

	internal void InitializeEvent(EventDefMD evt, out MethodDef addMethod, out MethodDef invokeMethod, out MethodDef removeMethod, out IList<MethodDef> otherMethods)
	{
		addMethod = null;
		invokeMethod = null;
		removeMethod = null;
		otherMethods = new List<MethodDef>();
		if (evt == null)
		{
			return;
		}
		RidList methodSemanticsRidList = readerModule.Metadata.GetMethodSemanticsRidList(Table.Event, evt.OrigRid);
		for (int i = 0; i < methodSemanticsRidList.Count; i++)
		{
			if (!readerModule.TablesStream.TryReadMethodSemanticsRow(methodSemanticsRidList[i], out var row))
			{
				continue;
			}
			MethodDef methodDef = readerModule.ResolveMethod(row.Method);
			if (methodDef == null || methodDef.DeclaringType != evt.DeclaringType)
			{
				continue;
			}
			switch ((MethodSemanticsAttributes)row.Semantic)
			{
			case MethodSemanticsAttributes.AddOn:
				if (addMethod == null)
				{
					addMethod = methodDef;
				}
				break;
			case MethodSemanticsAttributes.RemoveOn:
				if (removeMethod == null)
				{
					removeMethod = methodDef;
				}
				break;
			case MethodSemanticsAttributes.Fire:
				if (invokeMethod == null)
				{
					invokeMethod = methodDef;
				}
				break;
			case MethodSemanticsAttributes.Other:
				if (!otherMethods.Contains(methodDef))
				{
					otherMethods.Add(methodDef);
				}
				break;
			}
		}
	}

	internal override void OnLazyAdd2(int index, ref FieldDef value)
	{
		if (value.DeclaringType != this)
		{
			value = readerModule.ForceUpdateRowId(readerModule.ReadField(value.Rid).InitializeAll());
			value.DeclaringType2 = this;
		}
	}

	internal override void OnLazyAdd2(int index, ref MethodDef value)
	{
		if (value.DeclaringType != this)
		{
			value = readerModule.ForceUpdateRowId(readerModule.ReadMethod(value.Rid).InitializeAll());
			value.DeclaringType2 = this;
			value.Parameters.UpdateThisParameterType(this);
		}
	}

	internal override void OnLazyAdd2(int index, ref EventDef value)
	{
		if (value.DeclaringType != this)
		{
			value = readerModule.ForceUpdateRowId(readerModule.ReadEvent(value.Rid).InitializeAll());
			value.DeclaringType2 = this;
		}
	}

	internal override void OnLazyAdd2(int index, ref PropertyDef value)
	{
		if (value.DeclaringType != this)
		{
			value = readerModule.ForceUpdateRowId(readerModule.ReadProperty(value.Rid).InitializeAll());
			value.DeclaringType2 = this;
		}
	}

	internal override void OnLazyAdd2(int index, ref GenericParam value)
	{
		if (value.Owner != this)
		{
			value = readerModule.ForceUpdateRowId(readerModule.ReadGenericParam(value.Rid).InitializeAll());
			value.Owner = this;
		}
	}
}
