using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Interception;
using System.Text;

namespace System.Data.Entity.SqlServer;

internal class SqlTableExistenceChecker : TableExistenceChecker
{
	public override bool AnyModelTableExistsInDatabase(ObjectContext context, DbConnection connection, IEnumerable<EntitySet> modelTables, string edmMetadataContextTableName)
	{
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		foreach (EntitySet modelTable in modelTables)
		{
			stringBuilder.Append("'");
			stringBuilder.Append((string)((MetadataItem)modelTable).MetadataProperties["Schema"].Value);
			stringBuilder.Append(".");
			stringBuilder.Append(((TableExistenceChecker)this).GetTableName(modelTable));
			stringBuilder.Append("',");
		}
		stringBuilder.Remove(stringBuilder.Length - 1, 1);
		DbCommand command = connection.CreateCommand();
		try
		{
			command.CommandText = "\r\nSELECT Count(*)\r\nFROM INFORMATION_SCHEMA.TABLES AS t\r\nWHERE t.TABLE_SCHEMA + '.' + t.TABLE_NAME IN (" + stringBuilder?.ToString() + ")\r\n    OR t.TABLE_NAME = '" + edmMetadataContextTableName + "'";
			bool flag = true;
			if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) == ConnectionState.Open)
			{
				flag = false;
				EntityTransaction currentTransaction = ((EntityConnection)context.Connection).CurrentTransaction;
				if (currentTransaction != null)
				{
					command.Transaction = currentTransaction.StoreTransaction;
				}
			}
			IDbExecutionStrategy executionStrategy = DbProviderServices.GetExecutionStrategy(connection);
			try
			{
				return executionStrategy.Execute<bool>((Func<bool>)delegate
				{
					//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
					//IL_00ab: Expected O, but got Unknown
					if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) == ConnectionState.Broken)
					{
						DbInterception.Dispatch.Connection.Close(connection, context.InterceptionContext);
					}
					if (DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) == ConnectionState.Closed)
					{
						DbInterception.Dispatch.Connection.Open(connection, context.InterceptionContext);
					}
					return (int)DbInterception.Dispatch.Command.Scalar(command, new DbCommandInterceptionContext(context.InterceptionContext)) > 0;
				});
			}
			finally
			{
				if (flag && DbInterception.Dispatch.Connection.GetState(connection, context.InterceptionContext) != 0)
				{
					DbInterception.Dispatch.Connection.Close(connection, context.InterceptionContext);
				}
			}
		}
		finally
		{
			if (command != null)
			{
				((IDisposable)command).Dispose();
			}
		}
	}
}
