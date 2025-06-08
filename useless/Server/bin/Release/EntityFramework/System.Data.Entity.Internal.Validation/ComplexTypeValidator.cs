using System.Collections.Generic;
using System.Data.Entity.Validation;

namespace System.Data.Entity.Internal.Validation;

internal class ComplexTypeValidator : TypeValidator
{
	public ComplexTypeValidator(IEnumerable<PropertyValidator> propertyValidators, IEnumerable<IValidator> typeLevelValidators)
		: base(propertyValidators, typeLevelValidators)
	{
	}

	public new IEnumerable<DbValidationError> Validate(EntityValidationContext entityValidationContext, InternalPropertyEntry property)
	{
		return base.Validate(entityValidationContext, property);
	}

	protected override void ValidateProperties(EntityValidationContext entityValidationContext, InternalPropertyEntry parentProperty, List<DbValidationError> validationErrors)
	{
		foreach (PropertyValidator propertyValidator in base.PropertyValidators)
		{
			InternalPropertyEntry property = parentProperty.Property(propertyValidator.PropertyName);
			validationErrors.AddRange(propertyValidator.Validate(entityValidationContext, property));
		}
	}
}
