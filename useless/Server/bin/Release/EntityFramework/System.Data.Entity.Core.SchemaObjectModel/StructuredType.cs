using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal abstract class StructuredType : SchemaType
{
	private enum HowDefined
	{
		NotDefined,
		AsMember
	}

	private bool? _baseTypeResolveResult;

	private string _unresolvedBaseType;

	private bool _isAbstract;

	private SchemaElementLookUpTable<SchemaElement> _namedMembers;

	private ISchemaElementLookUpTable<StructuredProperty> _properties;

	public StructuredType BaseType { get; private set; }

	public ISchemaElementLookUpTable<StructuredProperty> Properties
	{
		get
		{
			if (_properties == null)
			{
				_properties = new FilteredSchemaElementLookUpTable<StructuredProperty, SchemaElement>(NamedMembers);
			}
			return _properties;
		}
	}

	protected SchemaElementLookUpTable<SchemaElement> NamedMembers
	{
		get
		{
			if (_namedMembers == null)
			{
				_namedMembers = new SchemaElementLookUpTable<SchemaElement>();
			}
			return _namedMembers;
		}
	}

	public virtual bool IsTypeHierarchyRoot => BaseType == null;

	public bool IsAbstract => _isAbstract;

	protected string UnresolvedBaseType
	{
		get
		{
			return _unresolvedBaseType;
		}
		set
		{
			_unresolvedBaseType = value;
		}
	}

	public StructuredProperty FindProperty(string name)
	{
		StructuredProperty structuredProperty = Properties.LookUpEquivalentKey(name);
		if (structuredProperty != null)
		{
			return structuredProperty;
		}
		if (IsTypeHierarchyRoot)
		{
			return null;
		}
		return BaseType.FindProperty(name);
	}

	public bool IsOfType(StructuredType baseType)
	{
		StructuredType structuredType = this;
		while (structuredType != null && structuredType != baseType)
		{
			structuredType = structuredType.BaseType;
		}
		return structuredType == baseType;
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		TryResolveBaseType();
		foreach (SchemaElement namedMember in NamedMembers)
		{
			namedMember.ResolveTopLevelNames();
		}
	}

	internal override void Validate()
	{
		base.Validate();
		foreach (SchemaElement namedMember in NamedMembers)
		{
			if (BaseType != null)
			{
				string text = null;
				if (HowDefined.AsMember == BaseType.DefinesMemberName(namedMember.Name, out var definingType, out var _))
				{
					text = Strings.DuplicateMemberName(namedMember.Name, FQName, definingType.FQName);
				}
				if (text != null)
				{
					namedMember.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, text);
				}
			}
			namedMember.Validate();
		}
	}

	protected StructuredType(Schema parentElement)
		: base(parentElement)
	{
	}

	protected void AddMember(SchemaElement newMember)
	{
		if (!string.IsNullOrEmpty(newMember.Name))
		{
			if (base.Schema.DataModel != SchemaDataModelOption.ProviderDataModel && Utils.CompareNames(newMember.Name, Name) == 0)
			{
				newMember.AddError(ErrorCode.BadProperty, EdmSchemaErrorSeverity.Error, Strings.InvalidMemberNameMatchesTypeName(newMember.Name, FQName));
			}
			NamedMembers.Add(newMember, doNotAddErrorForEmptyName: true, Strings.PropertyNameAlreadyDefinedDuplicate);
		}
	}

	private HowDefined DefinesMemberName(string name, out StructuredType definingType, out SchemaElement definingMember)
	{
		if (NamedMembers.ContainsKey(name))
		{
			definingType = this;
			definingMember = NamedMembers[name];
			return HowDefined.AsMember;
		}
		definingMember = NamedMembers.LookUpEquivalentKey(name);
		if (IsTypeHierarchyRoot)
		{
			definingType = null;
			definingMember = null;
			return HowDefined.NotDefined;
		}
		return BaseType.DefinesMemberName(name, out definingType, out definingMember);
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "Property"))
		{
			HandlePropertyElement(reader);
			return true;
		}
		return false;
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "BaseType"))
		{
			HandleBaseTypeAttribute(reader);
			return true;
		}
		if (SchemaElement.CanHandleAttribute(reader, "Abstract"))
		{
			HandleAbstractAttribute(reader);
			return true;
		}
		return false;
	}

	private bool TryResolveBaseType()
	{
		if (_baseTypeResolveResult.HasValue)
		{
			return _baseTypeResolveResult.Value;
		}
		if (BaseType != null)
		{
			_baseTypeResolveResult = true;
			return _baseTypeResolveResult.Value;
		}
		if (UnresolvedBaseType == null)
		{
			_baseTypeResolveResult = true;
			return _baseTypeResolveResult.Value;
		}
		if (!base.Schema.ResolveTypeName(this, UnresolvedBaseType, out var type))
		{
			_baseTypeResolveResult = false;
			return _baseTypeResolveResult.Value;
		}
		BaseType = type as StructuredType;
		if (BaseType == null)
		{
			AddError(ErrorCode.InvalidBaseType, EdmSchemaErrorSeverity.Error, Strings.InvalidBaseTypeForStructuredType(UnresolvedBaseType, FQName));
			_baseTypeResolveResult = false;
			return _baseTypeResolveResult.Value;
		}
		if (CheckForInheritanceCycle())
		{
			BaseType = null;
			AddError(ErrorCode.CycleInTypeHierarchy, EdmSchemaErrorSeverity.Error, Strings.CycleInTypeHierarchy(FQName));
			_baseTypeResolveResult = false;
			return _baseTypeResolveResult.Value;
		}
		_baseTypeResolveResult = true;
		return true;
	}

	private void HandleBaseTypeAttribute(XmlReader reader)
	{
		if (Utils.GetDottedName(base.Schema, reader, out var name))
		{
			UnresolvedBaseType = name;
		}
	}

	private void HandleAbstractAttribute(XmlReader reader)
	{
		HandleBoolAttribute(reader, ref _isAbstract);
	}

	private void HandlePropertyElement(XmlReader reader)
	{
		StructuredProperty structuredProperty = new StructuredProperty(this);
		structuredProperty.Parse(reader);
		AddMember(structuredProperty);
	}

	private bool CheckForInheritanceCycle()
	{
		StructuredType baseType;
		StructuredType structuredType = (baseType = BaseType);
		do
		{
			structuredType = structuredType.BaseType;
			if (baseType == structuredType)
			{
				return true;
			}
			if (baseType == null)
			{
				return false;
			}
			baseType = baseType.BaseType;
			if (structuredType != null)
			{
				structuredType = structuredType.BaseType;
			}
		}
		while (structuredType != null);
		return false;
	}
}
