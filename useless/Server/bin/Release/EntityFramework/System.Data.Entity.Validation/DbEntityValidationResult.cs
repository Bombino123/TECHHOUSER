using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Validation;

[Serializable]
public class DbEntityValidationResult
{
	[NonSerialized]
	private readonly InternalEntityEntry _entry;

	private readonly List<DbValidationError> _validationErrors;

	public DbEntityEntry Entry
	{
		get
		{
			if (_entry == null)
			{
				return null;
			}
			return new DbEntityEntry(_entry);
		}
	}

	public ICollection<DbValidationError> ValidationErrors => _validationErrors;

	public bool IsValid => !_validationErrors.Any();

	public DbEntityValidationResult(DbEntityEntry entry, IEnumerable<DbValidationError> validationErrors)
	{
		Check.NotNull(entry, "entry");
		Check.NotNull(validationErrors, "validationErrors");
		_entry = entry.InternalEntry;
		_validationErrors = validationErrors.ToList();
	}

	internal DbEntityValidationResult(InternalEntityEntry entry, IEnumerable<DbValidationError> validationErrors)
	{
		_entry = entry;
		_validationErrors = validationErrors.ToList();
	}
}
