using System.ComponentModel.DataAnnotations;

namespace System.Data.Entity.Internal.Validation;

internal class EntityValidationContext
{
	private readonly InternalEntityEntry _entityEntry;

	public ValidationContext ExternalValidationContext { get; private set; }

	public InternalEntityEntry InternalEntity => _entityEntry;

	public EntityValidationContext(InternalEntityEntry entityEntry, ValidationContext externalValidationContext)
	{
		_entityEntry = entityEntry;
		ExternalValidationContext = externalValidationContext;
	}
}
