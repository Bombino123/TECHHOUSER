using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal.Validation;
using System.Data.Entity.Validation;
using System.Linq;

namespace System.Data.Entity.Internal;

internal abstract class InternalMemberEntry
{
	private readonly InternalEntityEntry _internalEntityEntry;

	private readonly MemberEntryMetadata _memberMetadata;

	public virtual string Name => _memberMetadata.MemberName;

	public abstract object CurrentValue { get; set; }

	public virtual InternalEntityEntry InternalEntityEntry => _internalEntityEntry;

	public virtual MemberEntryMetadata EntryMetadata => _memberMetadata;

	protected InternalMemberEntry(InternalEntityEntry internalEntityEntry, MemberEntryMetadata memberMetadata)
	{
		_internalEntityEntry = internalEntityEntry;
		_memberMetadata = memberMetadata;
	}

	public virtual IEnumerable<DbValidationError> GetValidationErrors()
	{
		ValidationProvider validationProvider = InternalEntityEntry.InternalContext.ValidationProvider;
		PropertyValidator propertyValidator = validationProvider.GetPropertyValidator(_internalEntityEntry, this);
		if (propertyValidator == null)
		{
			return Enumerable.Empty<DbValidationError>();
		}
		return propertyValidator.Validate(validationProvider.GetEntityValidationContext(_internalEntityEntry, null), this);
	}

	public abstract DbMemberEntry CreateDbMemberEntry();

	public abstract DbMemberEntry<TEntity, TProperty> CreateDbMemberEntry<TEntity, TProperty>() where TEntity : class;
}
