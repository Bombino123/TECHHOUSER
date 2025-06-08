using System.Collections.Generic;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Runtime.Serialization;

namespace System.Data.Entity.Validation;

[Serializable]
public class DbEntityValidationException : DataException
{
	private IList<DbEntityValidationResult> _entityValidationResults;

	public IEnumerable<DbEntityValidationResult> EntityValidationErrors => _entityValidationResults;

	public DbEntityValidationException()
		: this(Strings.DbEntityValidationException_ValidationFailed)
	{
	}

	public DbEntityValidationException(string message)
		: this(message, Enumerable.Empty<DbEntityValidationResult>())
	{
	}

	public DbEntityValidationException(string message, IEnumerable<DbEntityValidationResult> entityValidationResults)
		: base(message)
	{
		Check.NotNull(entityValidationResults, "entityValidationResults");
		InititializeValidationResults(entityValidationResults);
	}

	public DbEntityValidationException(string message, Exception innerException)
		: this(message, Enumerable.Empty<DbEntityValidationResult>(), innerException)
	{
	}

	public DbEntityValidationException(string message, IEnumerable<DbEntityValidationResult> entityValidationResults, Exception innerException)
		: base(message, innerException)
	{
		Check.NotNull(entityValidationResults, "entityValidationResults");
		InititializeValidationResults(entityValidationResults);
	}

	protected DbEntityValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_entityValidationResults = (List<DbEntityValidationResult>)info.GetValue("EntityValidationErrors", typeof(List<DbEntityValidationResult>));
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("EntityValidationErrors", _entityValidationResults);
	}

	private void InititializeValidationResults(IEnumerable<DbEntityValidationResult> entityValidationResults)
	{
		_entityValidationResults = ((entityValidationResults == null) ? new List<DbEntityValidationResult>() : entityValidationResults.ToList());
	}
}
