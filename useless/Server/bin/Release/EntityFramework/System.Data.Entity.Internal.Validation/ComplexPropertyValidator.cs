using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace System.Data.Entity.Internal.Validation;

internal class ComplexPropertyValidator : PropertyValidator
{
	private readonly ComplexTypeValidator _complexTypeValidator;

	public ComplexTypeValidator ComplexTypeValidator => _complexTypeValidator;

	public ComplexPropertyValidator(string propertyName, IEnumerable<IValidator> propertyValidators, ComplexTypeValidator complexTypeValidator)
		: base(propertyName, propertyValidators)
	{
		_complexTypeValidator = complexTypeValidator;
	}

	public override IEnumerable<DbValidationError> Validate(EntityValidationContext entityValidationContext, InternalMemberEntry property)
	{
		List<DbValidationError> list = new List<DbValidationError>();
		list.AddRange(base.Validate(entityValidationContext, property));
		if (!list.Any() && property.CurrentValue != null && _complexTypeValidator != null)
		{
			list.AddRange(_complexTypeValidator.Validate(entityValidationContext, (InternalPropertyEntry)property));
		}
		return list;
	}
}
