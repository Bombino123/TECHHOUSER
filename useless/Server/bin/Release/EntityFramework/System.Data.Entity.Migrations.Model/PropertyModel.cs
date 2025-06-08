using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Migrations.Model;

public abstract class PropertyModel
{
	private readonly PrimitiveTypeKind _type;

	private TypeUsage _typeUsage;

	public virtual PrimitiveTypeKind Type => _type;

	public TypeUsage TypeUsage => _typeUsage ?? (_typeUsage = BuildTypeUsage());

	public virtual string Name { get; set; }

	public virtual string StoreType { get; set; }

	public virtual int? MaxLength { get; set; }

	public virtual byte? Precision { get; set; }

	public virtual byte? Scale { get; set; }

	public virtual object DefaultValue { get; set; }

	public virtual string DefaultValueSql { get; set; }

	public virtual bool? IsFixedLength { get; set; }

	public virtual bool? IsUnicode { get; set; }

	protected PropertyModel(PrimitiveTypeKind type, TypeUsage typeUsage)
	{
		_type = type;
		_typeUsage = typeUsage;
	}

	private TypeUsage BuildTypeUsage()
	{
		PrimitiveType edmPrimitiveType = PrimitiveType.GetEdmPrimitiveType(Type);
		if (Type == PrimitiveTypeKind.Binary)
		{
			if (MaxLength.HasValue)
			{
				return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, IsFixedLength.GetValueOrDefault(), MaxLength.Value);
			}
			return TypeUsage.CreateBinaryTypeUsage(edmPrimitiveType, IsFixedLength.GetValueOrDefault());
		}
		if (Type == PrimitiveTypeKind.String)
		{
			if (MaxLength.HasValue)
			{
				return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, IsUnicode ?? true, IsFixedLength.GetValueOrDefault(), MaxLength.Value);
			}
			return TypeUsage.CreateStringTypeUsage(edmPrimitiveType, IsUnicode ?? true, IsFixedLength.GetValueOrDefault());
		}
		if (Type == PrimitiveTypeKind.DateTime)
		{
			return TypeUsage.CreateDateTimeTypeUsage(edmPrimitiveType, Precision);
		}
		if (Type == PrimitiveTypeKind.DateTimeOffset)
		{
			return TypeUsage.CreateDateTimeOffsetTypeUsage(edmPrimitiveType, Precision);
		}
		if (Type == PrimitiveTypeKind.Decimal)
		{
			if (Precision.HasValue || Scale.HasValue)
			{
				return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType, Precision ?? 18, Scale.GetValueOrDefault());
			}
			return TypeUsage.CreateDecimalTypeUsage(edmPrimitiveType);
		}
		if (Type != PrimitiveTypeKind.Time)
		{
			return TypeUsage.CreateDefaultTypeUsage(edmPrimitiveType);
		}
		return TypeUsage.CreateTimeTypeUsage(edmPrimitiveType, Precision);
	}

	internal virtual FacetValues ToFacetValues()
	{
		FacetValues facetValues = new FacetValues();
		if (DefaultValue != null)
		{
			facetValues.DefaultValue = DefaultValue;
		}
		if (IsFixedLength.HasValue)
		{
			facetValues.FixedLength = IsFixedLength.Value;
		}
		if (IsUnicode.HasValue)
		{
			facetValues.Unicode = IsUnicode.Value;
		}
		if (MaxLength.HasValue)
		{
			facetValues.MaxLength = MaxLength.Value;
		}
		if (Precision.HasValue)
		{
			facetValues.Precision = Precision.Value;
		}
		if (Scale.HasValue)
		{
			facetValues.Scale = Scale.Value;
		}
		return facetValues;
	}
}
