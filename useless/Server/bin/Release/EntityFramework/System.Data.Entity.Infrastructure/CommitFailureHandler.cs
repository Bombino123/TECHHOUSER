using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Infrastructure;

public class CommitFailureHandler : TransactionHandler
{
	private readonly HashSet<TransactionRow> _rowsToDelete = new HashSet<TransactionRow>();

	private readonly Func<DbConnection, TransactionContext> _transactionContextFactory;

	protected internal TransactionContext TransactionContext { get; private set; }

	protected Dictionary<DbTransaction, TransactionRow> Transactions { get; private set; }

	protected virtual int PruningLimit => 20;

	public CommitFailureHandler()
		: this((DbConnection c) => new TransactionContext(c))
	{
	}

	public CommitFailureHandler(Func<DbConnection, TransactionContext> transactionContextFactory)
	{
		Check.NotNull(transactionContextFactory, "transactionContextFactory");
		_transactionContextFactory = transactionContextFactory;
		Transactions = new Dictionary<DbTransaction, TransactionRow>();
	}

	protected virtual IDbExecutionStrategy GetExecutionStrategy()
	{
		return null;
	}

	public override void Initialize(ObjectContext context)
	{
		base.Initialize(context);
		DbConnection storeConnection = ((EntityConnection)base.ObjectContext.Connection).StoreConnection;
		Initialize(storeConnection);
	}

	public override void Initialize(DbContext context, DbConnection connection)
	{
		base.Initialize(context, connection);
		Initialize(connection);
	}

	private void Initialize(DbConnection connection)
	{
		DbContextInfo currentInfo = DbContextInfo.CurrentInfo;
		DbContextInfo.CurrentInfo = null;
		try
		{
			TransactionContext = _transactionContextFactory(connection);
			if (TransactionContext != null)
			{
				TransactionContext.Configuration.LazyLoadingEnabled = false;
				TransactionContext.Configuration.AutoDetectChangesEnabled = false;
				TransactionContext.Database.Initialize(force: false);
			}
		}
		finally
		{
			DbContextInfo.CurrentInfo = currentInfo;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && disposing && TransactionContext != null)
		{
			if (_rowsToDelete.Any())
			{
				try
				{
					PruneTransactionHistory(force: true, useExecutionStrategy: false);
				}
				catch (Exception)
				{
				}
			}
			TransactionContext.Dispose();
		}
		base.Dispose(disposing);
	}

	public override string BuildDatabaseInitializationScript()
	{
		if (TransactionContext != null)
		{
			IEnumerable<MigrationStatement> migrationStatements = TransactionContextInitializer<TransactionContext>.GenerateMigrationStatements(TransactionContext);
			StringBuilder stringBuilder = new StringBuilder();
			MigratorScriptingDecorator.BuildSqlScript(migrationStatements, stringBuilder);
			return stringBuilder.ToString();
		}
		return null;
	}

	public override void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
	{
		if (TransactionContext == null || !MatchesParentContext(connection, interceptionContext) || interceptionContext.Result == null)
		{
			return;
		}
		Guid transactionId = Guid.NewGuid();
		bool flag = false;
		bool flag2 = false;
		ObjectContext objectContext = ((IObjectContextAdapter)TransactionContext).ObjectContext;
		((EntityConnection)objectContext.Connection).UseStoreTransaction(interceptionContext.Result);
		while (!flag)
		{
			TransactionRow transactionRow = new TransactionRow
			{
				Id = transactionId,
				CreationTime = DateTime.Now
			};
			Transactions.Add(interceptionContext.Result, transactionRow);
			TransactionContext.Transactions.Add(transactionRow);
			try
			{
				objectContext.SaveChangesInternal(SaveOptions.AcceptAllChangesAfterSave, executeInExistingTransaction: true);
				flag = true;
			}
			catch (UpdateException)
			{
				Transactions.Remove(interceptionContext.Result);
				TransactionContext.Entry(transactionRow).State = EntityState.Detached;
				if (flag2)
				{
					throw;
				}
				try
				{
					if (TransactionContext.Transactions.AsNoTracking().WithExecutionStrategy(new DefaultExecutionStrategy()).FirstOrDefault((TransactionRow t) => t.Id == transactionId) != null)
					{
						transactionId = Guid.NewGuid();
						continue;
					}
					throw;
				}
				catch (EntityCommandExecutionException)
				{
					TransactionContext.Database.Initialize(force: true);
					flag2 = true;
				}
			}
		}
	}

	public override void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
		if (TransactionContext == null || (interceptionContext.Connection != null && !MatchesParentContext(interceptionContext.Connection, interceptionContext)) || !Transactions.TryGetValue(transaction, out var transactionRow))
		{
			return;
		}
		Transactions.Remove(transaction);
		if (interceptionContext.Exception != null)
		{
			TransactionRow transactionRow2 = null;
			bool suspended = DbExecutionStrategy.Suspended;
			try
			{
				DbExecutionStrategy.Suspended = false;
				IDbExecutionStrategy executionStrategy = GetExecutionStrategy() ?? DbProviderServices.GetExecutionStrategy(interceptionContext.Connection);
				transactionRow2 = TransactionContext.Transactions.AsNoTracking().WithExecutionStrategy(executionStrategy).SingleOrDefault((TransactionRow t) => t.Id == transactionRow.Id);
			}
			catch (EntityCommandExecutionException)
			{
			}
			finally
			{
				DbExecutionStrategy.Suspended = suspended;
			}
			if (transactionRow2 != null)
			{
				interceptionContext.Exception = null;
				PruneTransactionHistory(transactionRow);
			}
			else
			{
				TransactionContext.Entry(transactionRow).State = EntityState.Detached;
			}
		}
		else
		{
			PruneTransactionHistory(transactionRow);
		}
	}

	public override void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
		if (TransactionContext != null && (interceptionContext.Connection == null || MatchesParentContext(interceptionContext.Connection, interceptionContext)) && Transactions.TryGetValue(transaction, out var value))
		{
			Transactions.Remove(transaction);
			TransactionContext.Entry(value).State = EntityState.Detached;
		}
	}

	public override void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
	{
		RolledBack(transaction, interceptionContext);
	}

	public virtual void ClearTransactionHistory()
	{
		foreach (TransactionRow transaction in TransactionContext.Transactions)
		{
			MarkTransactionForPruning(transaction);
		}
		PruneTransactionHistory(force: true, useExecutionStrategy: true);
	}

	public Task ClearTransactionHistoryAsync()
	{
		return ClearTransactionHistoryAsync(CancellationToken.None);
	}

	public virtual async Task ClearTransactionHistoryAsync(CancellationToken cancellationToken)
	{
		await TransactionContext.Transactions.ForEachAsync(MarkTransactionForPruning, cancellationToken).WithCurrentCulture();
		await PruneTransactionHistoryAsync(force: true, useExecutionStrategy: true, cancellationToken).WithCurrentCulture();
	}

	protected virtual void MarkTransactionForPruning(TransactionRow transaction)
	{
		Check.NotNull(transaction, "transaction");
		if (!_rowsToDelete.Contains(transaction))
		{
			_rowsToDelete.Add(transaction);
		}
	}

	public void PruneTransactionHistory()
	{
		PruneTransactionHistory(force: true, useExecutionStrategy: true);
	}

	public Task PruneTransactionHistoryAsync()
	{
		return PruneTransactionHistoryAsync(CancellationToken.None);
	}

	public Task PruneTransactionHistoryAsync(CancellationToken cancellationToken)
	{
		return PruneTransactionHistoryAsync(force: true, useExecutionStrategy: true, cancellationToken);
	}

	protected virtual void PruneTransactionHistory(bool force, bool useExecutionStrategy)
	{
		if (_rowsToDelete.Count <= 0 || (!force && _rowsToDelete.Count <= PruningLimit))
		{
			return;
		}
		foreach (TransactionRow item in TransactionContext.Transactions.ToList())
		{
			if (_rowsToDelete.Contains(item))
			{
				TransactionContext.Transactions.Remove(item);
			}
		}
		ObjectContext objectContext = ((IObjectContextAdapter)TransactionContext).ObjectContext;
		try
		{
			objectContext.SaveChangesInternal(SaveOptions.None, !useExecutionStrategy);
			_rowsToDelete.Clear();
		}
		finally
		{
			objectContext.AcceptAllChanges();
		}
	}

	protected virtual async Task PruneTransactionHistoryAsync(bool force, bool useExecutionStrategy, CancellationToken cancellationToken)
	{
		if (_rowsToDelete.Count <= 0 || (!force && _rowsToDelete.Count <= PruningLimit))
		{
			return;
		}
		foreach (TransactionRow item in TransactionContext.Transactions.ToList())
		{
			if (_rowsToDelete.Contains(item))
			{
				TransactionContext.Transactions.Remove(item);
			}
		}
		ObjectContext objectContext = ((IObjectContextAdapter)TransactionContext).ObjectContext;
		try
		{
			await ((IObjectContextAdapter)TransactionContext).ObjectContext.SaveChangesInternalAsync(SaveOptions.None, !useExecutionStrategy, cancellationToken).WithCurrentCulture();
			_rowsToDelete.Clear();
		}
		finally
		{
			objectContext.AcceptAllChanges();
		}
	}

	private void PruneTransactionHistory(TransactionRow transaction)
	{
		MarkTransactionForPruning(transaction);
		try
		{
			PruneTransactionHistory(force: false, useExecutionStrategy: false);
		}
		catch (DataException)
		{
		}
	}

	public static CommitFailureHandler FromContext(DbContext context)
	{
		Check.NotNull(context, "context");
		return FromContext(((IObjectContextAdapter)context).ObjectContext);
	}

	public static CommitFailureHandler FromContext(ObjectContext context)
	{
		Check.NotNull(context, "context");
		return context.TransactionHandler as CommitFailureHandler;
	}
}
