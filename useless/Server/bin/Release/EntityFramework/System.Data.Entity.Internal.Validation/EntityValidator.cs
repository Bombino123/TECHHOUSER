using System.Collections.Generic;
using System.Data.Entity.Validation;

namespace System.Data.Entity.Internal.Validation;

internal class EntityValidator : TypeValidator
{
	public EntityValidator(IEnumerable<PropertyValidator> propertyValidators, IEnumerable<IValidator> typeLevelValidators)
		: base(propertyValidators, typeLevelValidators)
	{
	}

	public DbEntityValidationResult Validate(EntityValidationContext entityValidationContext)
	{
		IEnumerable<DbValidationError> validationErrors = Validate(entityValidationContext, null);
		return new DbEntityValidationResult(entityValidationContext.InternalEntity, validationErrors);
	}

	protected override void ValidateProperties(EntityValidationContext entityValidationContext, InternalPropertyEntry parentProperty, List<DbValidationError> validationErrors)
	{
		InternalEntityEntry internalEntity = entityValidationContext.InternalEntity;
		foreach (PropertyValidator propertyValidator in base.PropertyValidators)
		{
			validationErrors.AddRange(propertyValidator.Validate(entityValidationContext, internalEntity.Member(propertyValidator.PropertyName)));
		}
	}
}
