using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class SqlCePropertyMaxLengthConvention : IConceptualModelConvention<EntityType>, IConvention, IConceptualModelConvention<ComplexType>
{
	private const int DefaultLength = 4000;

	private readonly int _length;

	public SqlCePropertyMaxLengthConvention()
		: this(4000)
	{
	}

	public SqlCePropertyMaxLengthConvention(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length", Strings.InvalidMaxLengthSize);
		}
		_length = length;
	}

	public virtual void Apply(EntityType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		DbProviderInfo providerInfo = model.ProviderInfo;
		if (providerInfo != null && providerInfo.IsSqlCe())
		{
			SetLength(item.DeclaredProperties);
		}
	}

	public virtual void Apply(ComplexType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		DbProviderInfo providerInfo = model.ProviderInfo;
		if (providerInfo != null && providerInfo.IsSqlCe())
		{
			SetLength(item.Properties);
		}
	}

	private void SetLength(IEnumerable<EdmProperty> properties)
	{
		foreach (EdmProperty property in properties)
		{
			if (property.IsPrimitiveType && (property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String) || property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary)))
			{
				SetDefaults(property);
			}
		}
	}

	private void SetDefaults(EdmProperty property)
	{
		if (!property.MaxLength.HasValue && !property.IsMaxLength)
		{
			property.MaxLength = _length;
		}
	}
}
