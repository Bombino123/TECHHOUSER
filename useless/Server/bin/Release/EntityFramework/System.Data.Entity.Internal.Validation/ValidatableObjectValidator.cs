using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Data.Entity.Validation;
using System.Linq;

namespace System.Data.Entity.Internal.Validation;

internal class ValidatableObjectValidator : IValidator
{
	private readonly DisplayAttribute _displayAttribute;

	public ValidatableObjectValidator(DisplayAttribute displayAttribute)
	{
		_displayAttribute = displayAttribute;
	}

	public virtual IEnumerable<DbValidationError> Validate(EntityValidationContext entityValidationContext, InternalMemberEntry property)
	{
		if (property != null && property.CurrentValue == null)
		{
			return Enumerable.Empty<DbValidationError>();
		}
		ValidationContext externalValidationContext = entityValidationContext.ExternalValidationContext;
		externalValidationContext.SetDisplayName(property, _displayAttribute);
		IValidatableObject validatableObject = (IValidatableObject)((property == null) ? entityValidationContext.InternalEntity.Entity : property.CurrentValue);
		IEnumerable<ValidationResult> enumerable = null;
		try
		{
			enumerable = validatableObject.Validate(externalValidationContext);
		}
		catch (Exception innerException)
		{
			throw new DbUnexpectedValidationException(Strings.DbUnexpectedValidationException_IValidatableObject(externalValidationContext.DisplayName, ObjectContextTypeCache.GetObjectType(validatableObject.GetType())), innerException);
		}
		return DbHelpers.SplitValidationResults(externalValidationContext.MemberName, enumerable ?? Enumerable.Empty<ValidationResult>());
	}
}
