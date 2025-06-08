using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using dnlib.DotNet.MD;
using dnlib.DotNet.Pdb;
using dnlib.Utils;

namespace dnlib.DotNet;

[DebuggerDisplay("{Name.String}")]
public abstract class GenericParam : IHasCustomAttribute, ICodedToken, IMDTokenProvider, IHasCustomDebugInformation, IMemberDef, IDnlibDef, IFullName, IMemberRef, IOwnerModule, IIsTypeOrMethod, IListListener<GenericParamConstraint>
{
	protected uint rid;

	protected ITypeOrMethodDef owner;

	protected ushort number;

	protected int attributes;

	protected UTF8String name;

	protected ITypeDefOrRef kind;

	protected LazyList<GenericParamConstraint> genericParamConstraints;

	protected CustomAttributeCollection customAttributes;

	protected IList<PdbCustomDebugInfo> customDebugInfos;

	public MDToken MDToken => new MDToken(Table.GenericParam, rid);

	public uint Rid
	{
		get
		{
			return rid;
		}
		set
		{
			rid = value;
		}
	}

	public int HasCustomAttributeTag => 19;

	public ITypeOrMethodDef Owner
	{
		get
		{
			return owner;
		}
		internal set
		{
			owner = value;
		}
	}

	public TypeDef DeclaringType => owner as TypeDef;

	ITypeDefOrRef IMemberRef.DeclaringType => owner as TypeDef;

	public MethodDef DeclaringMethod => owner as MethodDef;

	public ushort Number
	{
		get
		{
			return number;
		}
		set
		{
			number = value;
		}
	}

	public GenericParamAttributes Flags
	{
		get
		{
			return (GenericParamAttributes)attributes;
		}
		set
		{
			attributes = (int)value;
		}
	}

	public UTF8String Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public ITypeDefOrRef Kind
	{
		get
		{
			return kind;
		}
		set
		{
			kind = value;
		}
	}

	public IList<GenericParamConstraint> GenericParamConstraints
	{
		get
		{
			if (genericParamConstraints == null)
			{
				InitializeGenericParamConstraints();
			}
			return genericParamConstraints;
		}
	}

	public CustomAttributeCollection CustomAttributes
	{
		get
		{
			if (customAttributes == null)
			{
				InitializeCustomAttributes();
			}
			return customAttributes;
		}
	}

	public bool HasCustomAttributes => CustomAttributes.Count > 0;

	public int HasCustomDebugInformationTag => 19;

	public bool HasCustomDebugInfos => CustomDebugInfos.Count > 0;

	public IList<PdbCustomDebugInfo> CustomDebugInfos
	{
		get
		{
			if (customDebugInfos == null)
			{
				InitializeCustomDebugInfos();
			}
			return customDebugInfos;
		}
	}

	public bool HasGenericParamConstraints => GenericParamConstraints.Count > 0;

	public ModuleDef Module => owner?.Module;

	public string FullName => UTF8String.ToSystemStringOrEmpty(name);

	bool IIsTypeOrMethod.IsType => false;

	bool IIsTypeOrMethod.IsMethod => false;

	bool IMemberRef.IsField => false;

	bool IMemberRef.IsTypeSpec => false;

	bool IMemberRef.IsTypeRef => false;

	bool IMemberRef.IsTypeDef => false;

	bool IMemberRef.IsMethodSpec => false;

	bool IMemberRef.IsMethodDef => false;

	bool IMemberRef.IsMemberRef => false;

	bool IMemberRef.IsFieldDef => false;

	bool IMemberRef.IsPropertyDef => false;

	bool IMemberRef.IsEventDef => false;

	bool IMemberRef.IsGenericParam => true;

	public GenericParamAttributes Variance
	{
		get
		{
			return (GenericParamAttributes)attributes & GenericParamAttributes.VarianceMask;
		}
		set
		{
			ModifyAttributes(~GenericParamAttributes.VarianceMask, value & GenericParamAttributes.VarianceMask);
		}
	}

	public bool IsNonVariant => Variance == GenericParamAttributes.NonVariant;

	public bool IsCovariant => Variance == GenericParamAttributes.Covariant;

	public bool IsContravariant => Variance == GenericParamAttributes.Contravariant;

	public GenericParamAttributes SpecialConstraint
	{
		get
		{
			return (GenericParamAttributes)attributes & GenericParamAttributes.SpecialConstraintMask;
		}
		set
		{
			ModifyAttributes(~GenericParamAttributes.SpecialConstraintMask, value & GenericParamAttributes.SpecialConstraintMask);
		}
	}

	public bool HasNoSpecialConstraint => ((ushort)attributes & 0x1C) == 0;

	public bool HasReferenceTypeConstraint
	{
		get
		{
			return ((ushort)attributes & 4) != 0;
		}
		set
		{
			ModifyAttributes(value, GenericParamAttributes.ReferenceTypeConstraint);
		}
	}

	public bool HasNotNullableValueTypeConstraint
	{
		get
		{
			return ((ushort)attributes & 8) != 0;
		}
		set
		{
			ModifyAttributes(value, GenericParamAttributes.NotNullableValueTypeConstraint);
		}
	}

	public bool HasDefaultConstructorConstraint
	{
		get
		{
			return ((ushort)attributes & 0x10) != 0;
		}
		set
		{
			ModifyAttributes(value, GenericParamAttributes.DefaultConstructorConstraint);
		}
	}

	protected virtual void InitializeGenericParamConstraints()
	{
		Interlocked.CompareExchange(ref genericParamConstraints, new LazyList<GenericParamConstraint>(this), null);
	}

	protected virtual void InitializeCustomAttributes()
	{
		Interlocked.CompareExchange(ref customAttributes, new CustomAttributeCollection(), null);
	}

	protected virtual void InitializeCustomDebugInfos()
	{
		Interlocked.CompareExchange(ref customDebugInfos, new List<PdbCustomDebugInfo>(), null);
	}

	private void ModifyAttributes(GenericParamAttributes andMask, GenericParamAttributes orMask)
	{
		attributes = (attributes & (int)andMask) | (int)orMask;
	}

	private void ModifyAttributes(bool set, GenericParamAttributes flags)
	{
		if (set)
		{
			attributes |= (int)flags;
		}
		else
		{
			attributes &= (int)(~(uint)flags);
		}
	}

	void IListListener<GenericParamConstraint>.OnLazyAdd(int index, ref GenericParamConstraint value)
	{
		OnLazyAdd2(index, ref value);
	}

	internal virtual void OnLazyAdd2(int index, ref GenericParamConstraint value)
	{
	}

	void IListListener<GenericParamConstraint>.OnAdd(int index, GenericParamConstraint value)
	{
		if (value.Owner != null)
		{
			throw new InvalidOperationException("Generic param constraint is already owned by another generic param. Set Owner to null first.");
		}
		value.Owner = this;
	}

	void IListListener<GenericParamConstraint>.OnRemove(int index, GenericParamConstraint value)
	{
		value.Owner = null;
	}

	void IListListener<GenericParamConstraint>.OnResize(int index)
	{
	}

	void IListListener<GenericParamConstraint>.OnClear()
	{
		foreach (GenericParamConstraint item in genericParamConstraints.GetEnumerable_NoLock())
		{
			item.Owner = null;
		}
	}

	public override string ToString()
	{
		ITypeOrMethodDef typeOrMethodDef = owner;
		if (typeOrMethodDef is TypeDef)
		{
			return $"!{number}";
		}
		if (typeOrMethodDef is MethodDef)
		{
			return $"!!{number}";
		}
		return $"??{number}";
	}
}
