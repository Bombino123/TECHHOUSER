using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

internal class DateTimePropertyConfiguration : PrimitivePropertyConfiguration
{
	public byte? Precision { get; set; }

	public DateTimePropertyConfiguration()
	{
	}

	private DateTimePropertyConfiguration(DateTimePropertyConfiguration source)
		: base(source)
	{
		Precision = source.Precision;
	}

	internal override PrimitivePropertyConfiguration Clone()
	{
		return new DateTimePropertyConfiguration(this);
	}

	protected override void ConfigureProperty(EdmProperty property)
	{
		base.ConfigureProperty(property);
		if (Precision.HasValue)
		{
			property.Precision = Precision;
		}
	}

	internal override void Configure(EdmProperty column, FacetDescription facetDescription)
	{
		base.Configure(column, facetDescription);
		string facetName = facetDescription.FacetName;
		if (facetName != null && facetName == "Precision")
		{
			column.Precision = (facetDescription.IsConstant ? null : (Precision ?? column.Precision));
		}
	}

	internal override void CopyFrom(PrimitivePropertyConfiguration other)
	{
		base.CopyFrom(other);
		if (other is DateTimePropertyConfiguration dateTimePropertyConfiguration)
		{
			Precision = dateTimePropertyConfiguration.Precision;
		}
	}

	internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.FillFrom(other, inCSpace);
		if (other is DateTimePropertyConfiguration dateTimePropertyConfiguration && !Precision.HasValue)
		{
			Precision = dateTimePropertyConfiguration.Precision;
		}
	}

	internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.MakeCompatibleWith(other, inCSpace);
		if (other is DateTimePropertyConfiguration { Precision: not null })
		{
			Precision = null;
		}
	}

	internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
	{
		DateTimePropertyConfiguration dateTimePropertyConfiguration = other as DateTimePropertyConfiguration;
		bool flag = base.IsCompatible(other, inCSpace, out errorMessage);
		bool flag2 = dateTimePropertyConfiguration == null || IsCompatible((DateTimePropertyConfiguration c) => c.Precision, dateTimePropertyConfiguration, ref errorMessage);
		return flag && flag2;
	}
}
