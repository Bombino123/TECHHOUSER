using System.Data.Common;

namespace System.Data.Entity.Infrastructure.Interception;

public interface IDbTransactionInterceptor : IDbInterceptor
{
	void ConnectionGetting(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext);

	void ConnectionGot(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext);

	void IsolationLevelGetting(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext);

	void IsolationLevelGot(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext);

	void Committing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

	void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

	void Disposing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

	void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

	void RollingBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);

	void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext);
}
