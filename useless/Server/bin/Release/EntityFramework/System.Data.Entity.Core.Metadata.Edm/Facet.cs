using System.Data.Entity.Utilities;
using System.Diagnostics;

namespace System.Data.Entity.Core.Metadata.Edm;

[DebuggerDisplay("{Name,nq}={Value}")]
public class Facet : MetadataItem
{
	private readonly FacetDescription _facetDescription;

	private readonly object _value;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.Facet;

	public FacetDescription Description => _facetDescription;

	[MetadataProperty(PrimitiveTypeKind.String, false)]
	public virtual string Name => _facetDescription.FacetName;

	[MetadataProperty(BuiltInTypeKind.EdmType, false)]
	public EdmType FacetType => _facetDescription.FacetType;

	[MetadataProperty(typeof(object), false)]
	public virtual object Value => _value;

	internal override string Identity => _facetDescription.FacetName;

	public bool IsUnbounded => Value == EdmConstants.UnboundedValue;

	internal Facet()
	{
	}

	private Facet(FacetDescription facetDescription, object value)
		: base(MetadataFlags.Readonly)
	{
		Check.NotNull(facetDescription, "facetDescription");
		_facetDescription = facetDescription;
		_value = value;
	}

	internal static Facet Create(FacetDescription facetDescription, object value)
	{
		return Create(facetDescription, value, bypassKnownValues: false);
	}

	internal static Facet Create(FacetDescription facetDescription, object value, bool bypassKnownValues)
	{
		if (!bypassKnownValues)
		{
			if (value == null)
			{
				return facetDescription.NullValueFacet;
			}
			if (object.Equals(facetDescription.DefaultValue, value))
			{
				return facetDescription.DefaultValueFacet;
			}
			if (facetDescription.FacetType.Identity == "Edm.Boolean")
			{
				bool value2 = (bool)value;
				return facetDescription.GetBooleanFacet(value2);
			}
		}
		Facet facet = new Facet(facetDescription, value);
		if (value != null && !Helper.IsUnboundedFacetValue(facet) && !Helper.IsVariableFacetValue(facet) && facet.FacetType.ClrType != null)
		{
			value.GetType();
		}
		return facet;
	}

	public override string ToString()
	{
		return Name;
	}
}
