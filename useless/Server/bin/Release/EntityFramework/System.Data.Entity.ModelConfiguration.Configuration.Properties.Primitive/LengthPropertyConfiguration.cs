using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;

internal abstract class LengthPropertyConfiguration : PrimitivePropertyConfiguration
{
	public bool? IsFixedLength { get; set; }

	public int? MaxLength { get; set; }

	public bool? IsMaxLength { get; set; }

	protected LengthPropertyConfiguration()
	{
	}

	protected LengthPropertyConfiguration(LengthPropertyConfiguration source)
		: base(source)
	{
		Check.NotNull(source, "source");
		IsFixedLength = source.IsFixedLength;
		MaxLength = source.MaxLength;
		IsMaxLength = source.IsMaxLength;
	}

	protected override void ConfigureProperty(EdmProperty property)
	{
		base.ConfigureProperty(property);
		if (IsFixedLength.HasValue)
		{
			property.IsFixedLength = IsFixedLength;
		}
		if (MaxLength.HasValue)
		{
			property.MaxLength = MaxLength;
		}
		if (IsMaxLength.HasValue)
		{
			property.IsMaxLength = IsMaxLength.Value;
		}
	}

	internal override void Configure(EdmProperty column, FacetDescription facetDescription)
	{
		base.Configure(column, facetDescription);
		switch (facetDescription.FacetName)
		{
		case "FixedLength":
			column.IsFixedLength = (facetDescription.IsConstant ? null : (IsFixedLength ?? column.IsFixedLength));
			break;
		case "MaxLength":
			column.MaxLength = (facetDescription.IsConstant ? null : (MaxLength ?? column.MaxLength));
			column.IsMaxLength = !facetDescription.IsConstant && (IsMaxLength ?? column.IsMaxLength);
			break;
		}
	}

	internal override void CopyFrom(PrimitivePropertyConfiguration other)
	{
		base.CopyFrom(other);
		if (other is LengthPropertyConfiguration lengthPropertyConfiguration)
		{
			IsFixedLength = lengthPropertyConfiguration.IsFixedLength;
			MaxLength = lengthPropertyConfiguration.MaxLength;
			IsMaxLength = lengthPropertyConfiguration.IsMaxLength;
		}
	}

	internal override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.FillFrom(other, inCSpace);
		if (other is LengthPropertyConfiguration lengthPropertyConfiguration)
		{
			if (!IsFixedLength.HasValue)
			{
				IsFixedLength = lengthPropertyConfiguration.IsFixedLength;
			}
			if (!MaxLength.HasValue)
			{
				MaxLength = lengthPropertyConfiguration.MaxLength;
			}
			if (!IsMaxLength.HasValue)
			{
				IsMaxLength = lengthPropertyConfiguration.IsMaxLength;
			}
		}
	}

	internal override void MakeCompatibleWith(PrimitivePropertyConfiguration other, bool inCSpace)
	{
		base.MakeCompatibleWith(other, inCSpace);
		if (other is LengthPropertyConfiguration { IsFixedLength: var isFixedLength } lengthPropertyConfiguration)
		{
			if (isFixedLength.HasValue)
			{
				IsFixedLength = null;
			}
			if (lengthPropertyConfiguration.MaxLength.HasValue)
			{
				MaxLength = null;
			}
			if (lengthPropertyConfiguration.IsMaxLength.HasValue)
			{
				IsMaxLength = null;
			}
		}
	}

	internal override bool IsCompatible(PrimitivePropertyConfiguration other, bool inCSpace, out string errorMessage)
	{
		LengthPropertyConfiguration lengthPropertyConfiguration = other as LengthPropertyConfiguration;
		bool flag = base.IsCompatible(other, inCSpace, out errorMessage);
		bool flag2 = lengthPropertyConfiguration == null || IsCompatible((LengthPropertyConfiguration c) => c.IsFixedLength, lengthPropertyConfiguration, ref errorMessage);
		bool flag3 = lengthPropertyConfiguration == null || IsCompatible((LengthPropertyConfiguration c) => c.IsMaxLength, lengthPropertyConfiguration, ref errorMessage);
		bool flag4 = lengthPropertyConfiguration == null || IsCompatible((LengthPropertyConfiguration c) => c.MaxLength, lengthPropertyConfiguration, ref errorMessage);
		return flag && flag2 && flag3 && flag4;
	}
}
