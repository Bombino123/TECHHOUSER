using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal class TypeElement : SchemaType
{
	private readonly PrimitiveType _primitiveType = new PrimitiveType();

	private readonly List<FacetDescriptionElement> _facetDescriptions = new List<FacetDescriptionElement>();

	public override string Name
	{
		get
		{
			return _primitiveType.Name;
		}
		set
		{
			_primitiveType.Name = value;
		}
	}

	public PrimitiveType PrimitiveType => _primitiveType;

	public IEnumerable<FacetDescription> FacetDescriptions
	{
		get
		{
			foreach (FacetDescriptionElement facetDescription in _facetDescriptions)
			{
				yield return facetDescription.FacetDescription;
			}
		}
	}

	public TypeElement(Schema parent)
		: base(parent)
	{
		_primitiveType.NamespaceName = base.Schema.Namespace;
	}

	protected override bool HandleElement(XmlReader reader)
	{
		if (base.HandleElement(reader))
		{
			return true;
		}
		if (CanHandleElement(reader, "FacetDescriptions"))
		{
			SkipThroughElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "Precision"))
		{
			HandlePrecisionElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "Scale"))
		{
			HandleScaleElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "MaxLength"))
		{
			HandleMaxLengthElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "Unicode"))
		{
			HandleUnicodeElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "FixedLength"))
		{
			HandleFixedLengthElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "SRID"))
		{
			HandleSridElement(reader);
			return true;
		}
		if (CanHandleElement(reader, "IsStrict"))
		{
			HandleIsStrictElement(reader);
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
		if (SchemaElement.CanHandleAttribute(reader, "PrimitiveTypeKind"))
		{
			HandlePrimitiveTypeKindAttribute(reader);
			return true;
		}
		return false;
	}

	private void HandlePrecisionElement(XmlReader reader)
	{
		ByteFacetDescriptionElement byteFacetDescriptionElement = new ByteFacetDescriptionElement(this, "Precision");
		byteFacetDescriptionElement.Parse(reader);
		_facetDescriptions.Add(byteFacetDescriptionElement);
	}

	private void HandleScaleElement(XmlReader reader)
	{
		ByteFacetDescriptionElement byteFacetDescriptionElement = new ByteFacetDescriptionElement(this, "Scale");
		byteFacetDescriptionElement.Parse(reader);
		_facetDescriptions.Add(byteFacetDescriptionElement);
	}

	private void HandleMaxLengthElement(XmlReader reader)
	{
		IntegerFacetDescriptionElement integerFacetDescriptionElement = new IntegerFacetDescriptionElement(this, "MaxLength");
		integerFacetDescriptionElement.Parse(reader);
		_facetDescriptions.Add(integerFacetDescriptionElement);
	}

	private void HandleUnicodeElement(XmlReader reader)
	{
		BooleanFacetDescriptionElement booleanFacetDescriptionElement = new BooleanFacetDescriptionElement(this, "Unicode");
		booleanFacetDescriptionElement.Parse(reader);
		_facetDescriptions.Add(booleanFacetDescriptionElement);
	}

	private void HandleFixedLengthElement(XmlReader reader)
	{
		BooleanFacetDescriptionElement booleanFacetDescriptionElement = new BooleanFacetDescriptionElement(this, "FixedLength");
		booleanFacetDescriptionElement.Parse(reader);
		_facetDescriptions.Add(booleanFacetDescriptionElement);
	}

	private void HandleSridElement(XmlReader reader)
	{
		SridFacetDescriptionElement sridFacetDescriptionElement = new SridFacetDescriptionElement(this, "SRID");
		sridFacetDescriptionElement.Parse(reader);
		_facetDescriptions.Add(sridFacetDescriptionElement);
	}

	private void HandleIsStrictElement(XmlReader reader)
	{
		BooleanFacetDescriptionElement booleanFacetDescriptionElement = new BooleanFacetDescriptionElement(this, "IsStrict");
		booleanFacetDescriptionElement.Parse(reader);
		_facetDescriptions.Add(booleanFacetDescriptionElement);
	}

	private void HandlePrimitiveTypeKindAttribute(XmlReader reader)
	{
		string value = reader.Value;
		try
		{
			_primitiveType.PrimitiveTypeKind = (PrimitiveTypeKind)Enum.Parse(typeof(PrimitiveTypeKind), value);
			_primitiveType.BaseType = MetadataItem.EdmProviderManifest.GetPrimitiveType(_primitiveType.PrimitiveTypeKind);
		}
		catch (ArgumentException)
		{
			AddError(ErrorCode.InvalidPrimitiveTypeKind, EdmSchemaErrorSeverity.Error, Strings.InvalidPrimitiveTypeKind(value));
		}
	}

	internal override void ResolveTopLevelNames()
	{
		base.ResolveTopLevelNames();
		foreach (FacetDescriptionElement facetDescription in _facetDescriptions)
		{
			try
			{
				facetDescription.CreateAndValidateFacetDescription(Name);
			}
			catch (ArgumentException ex)
			{
				AddError(ErrorCode.InvalidFacetInProviderManifest, EdmSchemaErrorSeverity.Error, ex.Message);
			}
		}
	}

	internal override void Validate()
	{
		base.Validate();
		if (ValidateSufficientFacets())
		{
			ValidateInterFacetConsistency();
		}
	}

	private bool ValidateInterFacetConsistency()
	{
		if (PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Decimal)
		{
			FacetDescription facet = Helper.GetFacet(FacetDescriptions, "Precision");
			FacetDescription facet2 = Helper.GetFacet(FacetDescriptions, "Scale");
			if (facet.MaxValue.Value < facet2.MaxValue.Value)
			{
				AddError(ErrorCode.BadPrecisionAndScale, EdmSchemaErrorSeverity.Error, Strings.BadPrecisionAndScale(facet.MaxValue.Value, facet2.MaxValue.Value));
				return false;
			}
		}
		return true;
	}

	private bool ValidateSufficientFacets()
	{
		if (!(_primitiveType.BaseType is PrimitiveType primitiveType))
		{
			return false;
		}
		bool flag = false;
		foreach (FacetDescription facetDescription in primitiveType.FacetDescriptions)
		{
			if (Helper.GetFacet(FacetDescriptions, facetDescription.FacetName) == null)
			{
				AddError(ErrorCode.RequiredFacetMissing, EdmSchemaErrorSeverity.Error, Strings.MissingFacetDescription(PrimitiveType.Name, PrimitiveType.PrimitiveTypeKind, facetDescription.FacetName));
				flag = true;
			}
		}
		return !flag;
	}
}
