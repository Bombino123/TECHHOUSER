using System.Data.Common;

namespace System.Data.Entity.Infrastructure.Interception;

public interface IDbConnectionInterceptor : IDbInterceptor
{
	void BeginningTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext);

	void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext);

	void Closing(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

	void Closed(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

	void ConnectionStringGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

	void ConnectionStringGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

	void ConnectionStringSetting(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext);

	void ConnectionStringSet(DbConnection connection, DbConnectionPropertyInterceptionContext<string> interceptionContext);

	void ConnectionTimeoutGetting(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext);

	void ConnectionTimeoutGot(DbConnection connection, DbConnectionInterceptionContext<int> interceptionContext);

	void DatabaseGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

	void DatabaseGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

	void DataSourceGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

	void DataSourceGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

	void Disposing(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

	void Disposed(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

	void EnlistingTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext);

	void EnlistedTransaction(DbConnection connection, EnlistTransactionInterceptionContext interceptionContext);

	void Opening(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

	void Opened(DbConnection connection, DbConnectionInterceptionContext interceptionContext);

	void ServerVersionGetting(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

	void ServerVersionGot(DbConnection connection, DbConnectionInterceptionContext<string> interceptionContext);

	void StateGetting(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext);

	void StateGot(DbConnection connection, DbConnectionInterceptionContext<ConnectionState> interceptionContext);
}
