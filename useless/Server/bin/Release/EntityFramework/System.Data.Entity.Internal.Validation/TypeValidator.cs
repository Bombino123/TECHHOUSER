using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;

namespace System.Data.Entity.Internal.Validation;

internal abstract class TypeValidator
{
	private readonly IEnumerable<IValidator> _typeLevelValidators;

	private readonly IEnumerable<PropertyValidator> _propertyValidators;

	public IEnumerable<IValidator> TypeLevelValidators => _typeLevelValidators;

	public IEnumerable<PropertyValidator> PropertyValidators => _propertyValidators;

	public TypeValidator(IEnumerable<PropertyValidator> propertyValidators, IEnumerable<IValidator> typeLevelValidators)
	{
		_typeLevelValidators = typeLevelValidators;
		_propertyValidators = propertyValidators;
	}

	protected IEnumerable<DbValidationError> Validate(EntityValidationContext entityValidationContext, InternalPropertyEntry property)
	{
		List<DbValidationError> list = new List<DbValidationError>();
		ValidateProperties(entityValidationContext, property, list);
		if (!list.Any())
		{
			foreach (IValidator typeLevelValidator in _typeLevelValidators)
			{
				list.AddRange(typeLevelValidator.Validate(entityValidationContext, property));
			}
		}
		return list;
	}

	protected abstract void ValidateProperties(EntityValidationContext entityValidationContext, InternalPropertyEntry parentProperty, List<DbValidationError> validationErrors);

	public PropertyValidator GetPropertyValidator(string name)
	{
		return _propertyValidators.SingleOrDefault((PropertyValidator v) => v.PropertyName == name);
	}
}
