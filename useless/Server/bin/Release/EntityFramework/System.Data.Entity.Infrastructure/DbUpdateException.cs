using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Linq;
using System.Runtime.Serialization;

namespace System.Data.Entity.Infrastructure;

[Serializable]
public class DbUpdateException : DataException
{
	[NonSerialized]
	private readonly InternalContext _internalContext;

	private readonly bool _involvesIndependentAssociations;

	public IEnumerable<DbEntityEntry> Entries
	{
		get
		{
			UpdateException ex = base.InnerException as UpdateException;
			if (_involvesIndependentAssociations || _internalContext == null || ex == null || ex.StateEntries == null)
			{
				return Enumerable.Empty<DbEntityEntry>();
			}
			return ex.StateEntries.Select((ObjectStateEntry e) => new DbEntityEntry(new InternalEntityEntry(_internalContext, new StateEntryAdapter(e))));
		}
	}

	internal DbUpdateException(InternalContext internalContext, UpdateException innerException, bool involvesIndependentAssociations)
		: base(involvesIndependentAssociations ? Strings.DbContext_IndependentAssociationUpdateException : innerException.Message, innerException)
	{
		_internalContext = internalContext;
		_involvesIndependentAssociations = involvesIndependentAssociations;
	}

	public DbUpdateException()
	{
	}

	public DbUpdateException(string message)
		: base(message)
	{
	}

	public DbUpdateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected DbUpdateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_involvesIndependentAssociations = info.GetBoolean("InvolvesIndependentAssociations");
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("InvolvesIndependentAssociations", _involvesIndependentAssociations);
	}
}
