using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration.Utilities;

namespace System.Data.Entity.Internal.Validation;

internal class ValidationProvider
{
	private readonly Dictionary<Type, EntityValidator> _entityValidators;

	private readonly EntityValidatorBuilder _entityValidatorBuilder;

	public ValidationProvider(EntityValidatorBuilder builder = null, AttributeProvider attributeProvider = null)
	{
		_entityValidators = new Dictionary<Type, EntityValidator>();
		_entityValidatorBuilder = builder ?? new EntityValidatorBuilder(attributeProvider ?? new AttributeProvider());
	}

	public virtual EntityValidator GetEntityValidator(InternalEntityEntry entityEntry)
	{
		Type entityType = entityEntry.EntityType;
		EntityValidator value = null;
		if (_entityValidators.TryGetValue(entityType, out value))
		{
			return value;
		}
		value = _entityValidatorBuilder.BuildEntityValidator(entityEntry);
		_entityValidators[entityType] = value;
		return value;
	}

	public virtual PropertyValidator GetPropertyValidator(InternalEntityEntry owningEntity, InternalMemberEntry property)
	{
		EntityValidator entityValidator = GetEntityValidator(owningEntity);
		if (entityValidator == null)
		{
			return null;
		}
		return GetValidatorForProperty(entityValidator, property);
	}

	protected virtual PropertyValidator GetValidatorForProperty(EntityValidator entityValidator, InternalMemberEntry memberEntry)
	{
		if (memberEntry is InternalNestedPropertyEntry internalNestedPropertyEntry)
		{
			if (!(GetValidatorForProperty(entityValidator, internalNestedPropertyEntry.ParentPropertyEntry) is ComplexPropertyValidator { ComplexTypeValidator: not null } complexPropertyValidator))
			{
				return null;
			}
			return complexPropertyValidator.ComplexTypeValidator.GetPropertyValidator(memberEntry.Name);
		}
		return entityValidator.GetPropertyValidator(memberEntry.Name);
	}

	public virtual EntityValidationContext GetEntityValidationContext(InternalEntityEntry entityEntry, IDictionary<object, object> items)
	{
		return new EntityValidationContext(entityEntry, new ValidationContext(entityEntry.Entity, null, items));
	}
}
