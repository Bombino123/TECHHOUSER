using System.Data.Entity.Core.Metadata.Edm;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal abstract class FacetEnabledSchemaElement : SchemaElement
{
	protected SchemaType _type;

	protected string _unresolvedType;

	protected TypeUsageBuilder _typeUsageBuilder;

	internal new Function ParentElement => base.ParentElement as Function;

	internal SchemaType Type => _type;

	internal virtual TypeUsage TypeUsage => _typeUsageBuilder.TypeUsage;

	internal TypeUsageBuilder TypeUsageBuilder => _typeUsageBuilder;

	internal bool HasUserDefinedFacets => _typeUsageBuilder.HasUserDefinedFacets;

	internal string UnresolvedType
	{
		get
		{
			return _unresolvedType;
		}
		set
		{
			_unresolvedType = value;
		}
	}

	internal FacetEnabledSchemaElement(Function parentElement)
		: base(parentElement)
	{
	}

	internal FacetEnabledSchemaElement(SchemaElement parentElement)
		: base(parentElement)
	{
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		if (base.Schema.ResolveTypeName(this, UnresolvedType, out _type) && base.Schema.DataModel == SchemaDataModelOption.ProviderManifestModel && _typeUsageBuilder.HasUserDefinedFacets)
		{
			bool flag = base.Schema.DataModel == SchemaDataModelOption.ProviderManifestModel;
			_typeUsageBuilder.ValidateAndSetTypeUsage((ScalarType)_type, !flag);
		}
	}

	internal void ValidateAndSetTypeUsage(ScalarType scalar)
	{
		_typeUsageBuilder.ValidateAndSetTypeUsage(scalar, complainOnMissingFacet: false);
	}

	internal void ValidateAndSetTypeUsage(EdmType edmType)
	{
		_typeUsageBuilder.ValidateAndSetTypeUsage(edmType, complainOnMissingFacet: false);
	}

	protected override bool HandleAttribute(XmlReader reader)
	{
		if (base.HandleAttribute(reader))
		{
			return true;
		}
		if (_typeUsageBuilder.HandleAttribute(reader))
		{
			return true;
		}
		return false;
	}
}
