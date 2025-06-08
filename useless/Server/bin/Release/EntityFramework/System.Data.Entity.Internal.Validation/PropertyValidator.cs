using System.Collections.Generic;
using System.Data.Entity.Validation;

namespace System.Data.Entity.Internal.Validation;

internal class PropertyValidator
{
	private readonly IEnumerable<IValidator> _propertyValidators;

	private readonly string _propertyName;

	public IEnumerable<IValidator> PropertyAttributeValidators => _propertyValidators;

	public string PropertyName => _propertyName;

	public PropertyValidator(string propertyName, IEnumerable<IValidator> propertyValidators)
	{
		_propertyValidators = propertyValidators;
		_propertyName = propertyName;
	}

	public virtual IEnumerable<DbValidationError> Validate(EntityValidationContext entityValidationContext, InternalMemberEntry property)
	{
		List<DbValidationError> list = new List<DbValidationError>();
		foreach (IValidator propertyValidator in _propertyValidators)
		{
			list.AddRange(propertyValidator.Validate(entityValidationContext, property));
		}
		return list;
	}
}
