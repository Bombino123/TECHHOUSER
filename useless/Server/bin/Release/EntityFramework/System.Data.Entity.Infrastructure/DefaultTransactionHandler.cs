using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Infrastructure;

internal class DefaultTransactionHandler : TransactionHandler
{
	public override string BuildDatabaseInitializationScript()
	{
		return string.Empty;
	}

	public override void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
		if (interceptionContext.Exception != null && interceptionContext.Connection != null && MatchesParentContext(interceptionContext.Connection, interceptionContext))
		{
			interceptionContext.Exception = new CommitFailedException(Strings.CommitFailed, interceptionContext.Exception);
		}
	}
}
