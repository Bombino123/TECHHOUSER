using System.Data.Entity.Infrastructure;

namespace System.Data.Entity.SqlServer;

public class SqlAzureExecutionStrategy : DbExecutionStrategy
{
	public SqlAzureExecutionStrategy()
	{
	}

	public SqlAzureExecutionStrategy(int maxRetryCount, TimeSpan maxDelay)
		: base(maxRetryCount, maxDelay)
	{
	}

	protected override bool ShouldRetryOn(Exception exception)
	{
		return SqlAzureRetriableExceptionDetector.ShouldRetryOn(exception);
	}
}
