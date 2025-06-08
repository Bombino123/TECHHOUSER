using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class PropertyMaxLengthConvention : IConceptualModelConvention<EntityType>, IConvention, IConceptualModelConvention<ComplexType>, IConceptualModelConvention<AssociationType>
{
	private const int DefaultLength = 128;

	private readonly int _length;

	public PropertyMaxLengthConvention()
		: this(128)
	{
	}

	public PropertyMaxLengthConvention(int length)
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
		SetLength(item.DeclaredProperties, item.KeyProperties);
	}

	public virtual void Apply(ComplexType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		SetLength(item.Properties, new List<EdmProperty>());
	}

	private void SetLength(IEnumerable<EdmProperty> properties, ICollection<EdmProperty> keyProperties)
	{
		foreach (EdmProperty property in properties)
		{
			if (property.IsPrimitiveType)
			{
				if (property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String))
				{
					SetStringDefaults(property, keyProperties.Contains(property));
				}
				if (property.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary))
				{
					SetBinaryDefaults(property, keyProperties.Contains(property));
				}
			}
		}
	}

	public virtual void Apply(AssociationType item, DbModel model)
	{
		Check.NotNull(item, "item");
		Check.NotNull(model, "model");
		if (item.Constraint == null)
		{
			return;
		}
		IEnumerable<EdmProperty> source = item.GetOtherEnd(item.Constraint.DependentEnd).GetEntityType().KeyProperties();
		if (source.Count() != item.Constraint.ToProperties.Count)
		{
			return;
		}
		for (int i = 0; i < item.Constraint.ToProperties.Count; i++)
		{
			EdmProperty edmProperty = item.Constraint.ToProperties[i];
			EdmProperty edmProperty2 = source.ElementAt(i);
			if (edmProperty.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String) || edmProperty.PrimitiveType == PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Binary))
			{
				edmProperty.IsUnicode = edmProperty2.IsUnicode;
				edmProperty.IsFixedLength = edmProperty2.IsFixedLength;
				edmProperty.MaxLength = edmProperty2.MaxLength;
				edmProperty.IsMaxLength = edmProperty2.IsMaxLength;
			}
		}
	}

	private void SetStringDefaults(EdmProperty property, bool isKey)
	{
		if (!property.IsUnicode.HasValue)
		{
			property.IsUnicode = true;
		}
		SetBinaryDefaults(property, isKey);
	}

	private void SetBinaryDefaults(EdmProperty property, bool isKey)
	{
		if (!property.IsFixedLength.HasValue)
		{
			property.IsFixedLength = false;
		}
		if (!property.MaxLength.HasValue && !property.IsMaxLength)
		{
			if (isKey || property.IsFixedLength == true)
			{
				property.MaxLength = _length;
			}
			else
			{
				property.IsMaxLength = true;
			}
		}
	}
}
