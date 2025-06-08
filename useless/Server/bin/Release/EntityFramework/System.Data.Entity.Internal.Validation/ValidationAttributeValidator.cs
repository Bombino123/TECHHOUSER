using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Data.Entity.Validation;
using System.Linq;

namespace System.Data.Entity.Internal.Validation;

internal class ValidationAttributeValidator : IValidator
{
	private readonly DisplayAttribute _displayAttribute;

	private readonly ValidationAttribute _validationAttribute;

	public ValidationAttributeValidator(ValidationAttribute validationAttribute, DisplayAttribute displayAttribute)
	{
		_validationAttribute = validationAttribute;
		_displayAttribute = displayAttribute;
	}

	public virtual IEnumerable<DbValidationError> Validate(EntityValidationContext entityValidationContext, InternalMemberEntry property)
	{
		if (!AttributeApplicable(entityValidationContext, property))
		{
			return Enumerable.Empty<DbValidationError>();
		}
		ValidationContext externalValidationContext = entityValidationContext.ExternalValidationContext;
		externalValidationContext.SetDisplayName(property, _displayAttribute);
		object value = ((property == null) ? entityValidationContext.InternalEntity.Entity : property.CurrentValue);
		ValidationResult validationResult = null;
		try
		{
			validationResult = _validationAttribute.GetValidationResult(value, externalValidationContext);
		}
		catch (Exception innerException)
		{
			throw new DbUnexpectedValidationException(Strings.DbUnexpectedValidationException_ValidationAttribute(externalValidationContext.DisplayName, _validationAttribute.GetType()), innerException);
		}
		if (validationResult == ValidationResult.Success)
		{
			return Enumerable.Empty<DbValidationError>();
		}
		return DbHelpers.SplitValidationResults(externalValidationContext.MemberName, new ValidationResult[1] { validationResult });
	}

	protected virtual bool AttributeApplicable(EntityValidationContext entityValidationContext, InternalMemberEntry property)
	{
		InternalNavigationEntry internalNavigationEntry = property as InternalNavigationEntry;
		if (_validationAttribute is RequiredAttribute && property != null && property.InternalEntityEntry != null && property.InternalEntityEntry.State != EntityState.Added && property.InternalEntityEntry.State != EntityState.Detached && internalNavigationEntry != null && !internalNavigationEntry.IsLoaded)
		{
			return false;
		}
		return true;
	}
}
