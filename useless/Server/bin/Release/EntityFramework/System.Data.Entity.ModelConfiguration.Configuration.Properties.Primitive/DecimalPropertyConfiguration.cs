using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

internal class DecimalPropertyConfiguration : PrimitivePropertyConfiguration
{
	public byte? Precision { get; set; }

	public byte? Scale { get; set; }

	public DecimalPropertyConfiguration()
	{
	}

	private DecimalPropertyConfiguration(DecimalPropertyConfiguration source)
		: base(source)
	{
		Precision = source.Precision;
		Scale = source.Scale;
	}

	internal override PrimitivePropertyConfiguration Clone()
	{
		return new DecimalPropertyConfiguration(this);
	}

	protected override void ConfigureProperty(EdmProperty property)
	{
		base.ConfigureProperty(property);
		if (Precision.HasValue)
		{
			property.Precision = Precision;
		}
		if (Scale.HasValue)
		{
			property.Scale = Scale;
		}
	}

	internal override void Configure(EdmProperty column, FacetDescription facetDescription)
	{
		base.Configure(column, facetDescription);
		switch (facetDescription.FacetName)
		{
		case "Precision":
			column.Precision = (facetDescription.IsConstant ? null : (Precision ?? column.Precision));
			break;
		case "Scale":
			column.Scale = (facetDescription.IsConstant ? null : (Scale ?? column.Scale));
			break;
		}
	}

	internal override void CopyFrom(PrimitivePropertyConfiguration other)
	{
		base.CopyFrom(other);
		if (other is DecimalPropertyConfiguration decimalPropertyConfiguration)
		{
			Precision = decimalPropertyConfiguration.Precision;
			Scale = decimalPropertyConfiguration.Scale;
		}
	}

	internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.FillFrom(other, inCSpace);
		if (other is DecimalPropertyConfiguration decimalPropertyConfiguration)
		{
			if (!Precision.HasValue)
			{
				Precision = decimalPropertyConfiguration.Precision;
			}
			if (!Scale.HasValue)
			{
				Scale = decimalPropertyConfiguration.Scale;
			}
		}
	}

	internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.MakeCompatibleWith(other, inCSpace);
		if (other is DecimalPropertyConfiguration { Precision: var precision } decimalPropertyConfiguration)
		{
			if (precision.HasValue)
			{
				Precision = null;
			}
			if (decimalPropertyConfiguration.Scale.HasValue)
			{
				Scale = null;
			}
		}
	}

	internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
	{
		DecimalPropertyConfiguration decimalPropertyConfiguration = other as DecimalPropertyConfiguration;
		bool flag = base.IsCompatible(other, inCSpace, out errorMessage);
		bool flag2 = decimalPropertyConfiguration == null || IsCompatible((DecimalPropertyConfiguration c) => c.Precision, decimalPropertyConfiguration, ref errorMessage);
		bool flag3 = decimalPropertyConfiguration == null || IsCompatible((DecimalPropertyConfiguration c) => c.Scale, decimalPropertyConfiguration, ref errorMessage);
		return flag && flag2 && flag3;
	}
}
