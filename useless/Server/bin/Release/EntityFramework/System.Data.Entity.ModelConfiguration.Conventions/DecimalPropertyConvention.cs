using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class DecimalPropertyConvention : IConceptualModelConvention<EdmProperty>, IConvention
{
	private readonly byte _precision;

	private readonly byte _scale;

	public DecimalPropertyConvention()
		: this(18, 2)
	{
	}

	public DecimalPropertyConvention(byte precision, byte scale)
	{
		_precision = precision;
		_scale = scale;
	}

	public virtual void Apply(EdmProperty item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (item.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Decimal))
		{
			if (!item.Precision.HasValue)
			{
				item.Precision = _precision;
			}
			if (!item.Scale.HasValue)
			{
				item.Scale = _scale;
			}
		}
	}
}
