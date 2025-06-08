using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

internal class StringPropertyConfiguration : LengthPropertyConfiguration
{
	public bool? IsUnicode { get; set; }

	public StringPropertyConfiguration()
	{
	}

	private StringPropertyConfiguration(StringPropertyConfiguration source)
		: base(source)
	{
		IsUnicode = source.IsUnicode;
	}

	internal override PrimitivePropertyConfiguration Clone()
	{
		return new StringPropertyConfiguration(this);
	}

	protected override void ConfigureProperty(EdmProperty property)
	{
		base.ConfigureProperty(property);
		if (IsUnicode.HasValue)
		{
			property.IsUnicode = IsUnicode;
		}
	}

	internal override void Configure(EdmProperty column, FacetDescription facetDescription)
	{
		base.Configure(column, facetDescription);
		string facetName = facetDescription.FacetName;
		if (facetName != null && facetName == "Unicode")
		{
			column.IsUnicode = (facetDescription.IsConstant ? null : (IsUnicode ?? column.IsUnicode));
		}
	}

	internal override void CopyFrom(PrimitivePropertyConfiguration other)
	{
		base.CopyFrom(other);
		if (other is StringPropertyConfiguration stringPropertyConfiguration)
		{
			IsUnicode = stringPropertyConfiguration.IsUnicode;
		}
	}

	internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.FillFrom(other, inCSpace);
		if (other is StringPropertyConfiguration stringPropertyConfiguration && !IsUnicode.HasValue)
		{
			IsUnicode = stringPropertyConfiguration.IsUnicode;
		}
	}

	internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.MakeCompatibleWith(other, inCSpace);
		if (other is StringPropertyConfiguration { IsUnicode: not null })
		{
			IsUnicode = null;
		}
	}

	internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
	{
		StringPropertyConfiguration stringPropertyConfiguration = other as StringPropertyConfiguration;
		bool flag = base.IsCompatible(other, inCSpace, out errorMessage);
		bool flag2 = stringPropertyConfiguration == null || IsCompatible((StringPropertyConfiguration c) => c.IsUnicode, stringPropertyConfiguration, ref errorMessage);
		return flag && flag2;
	}
}
