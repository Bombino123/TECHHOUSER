using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Internal;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Transactions;
using System.Xml.Linq;

namespace System.Data.Entity.Infrastructure;

internal class TransactionContextInitializer<TContext> : IDatabaseInitializer<TContext> where TContext : TransactionContext
{
	public void InitializeDatabase(TContext context)
	{
		EntityConnection entityConnection = (EntityConnection)((IObjectContextAdapter)context).ObjectContext.Connection;
		if (entityConnection.State != ConnectionState.Open || entityConnection.CurrentTransaction == null)
		{
			return;
		}
		try
		{
			using (new TransactionScope(TransactionScopeOption.Suppress))
			{
				context.Transactions.AsNoTracking().WithExecutionStrategy(new DefaultExecutionStrategy()).Count();
			}
		}
		catch (EntityException)
		{
			DbContextInfo currentInfo = DbContextInfo.CurrentInfo;
			DbContextInfo.CurrentInfo = null;
			try
			{
				IEnumerable<MigrationStatement> migrationStatements = GenerateMigrationStatements(context);
				DbMigrator dbMigrator = new DbMigrator(context.InternalContext.MigrationsConfiguration, context, DatabaseExistenceState.Exists, calledByCreateDatabase: true);
				using (new TransactionScope(TransactionScopeOption.Suppress))
				{
					dbMigrator.ExecuteStatements(migrationStatements, entityConnection.CurrentTransaction.StoreTransaction);
				}
			}
			finally
			{
				DbContextInfo.CurrentInfo = currentInfo;
			}
		}
	}

	internal static IEnumerable<MigrationStatement> GenerateMigrationStatements(TransactionContext context)
	{
		if (DbConfiguration.DependencyResolver.GetService<Func<MigrationSqlGenerator>>(context.InternalContext.ProviderName) != null)
		{
			MigrationSqlGenerator sqlGenerator = context.InternalContext.MigrationsConfiguration.GetSqlGenerator(context.InternalContext.ProviderName);
			DbConnection connection = context.Database.Connection;
			XDocument model = new DbModelBuilder().Build(connection).GetModel();
			CreateTableOperation createTableOperation = (CreateTableOperation)new EdmModelDiffer().Diff(model, context.GetModel()).Single();
			return sqlGenerator.Generate(providerManifestToken: (context.InternalContext.ModelProviderInfo != null) ? context.InternalContext.ModelProviderInfo.ProviderManifestToken : DbConfiguration.DependencyResolver.GetService<IManifestTokenResolver>().ResolveManifestToken(connection), migrationOperations: new CreateTableOperation[1] { createTableOperation });
		}
		return new MigrationStatement[1]
		{
			new MigrationStatement
			{
				Sql = ((IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript(),
				SuppressTransaction = true
			}
		};
	}
}
