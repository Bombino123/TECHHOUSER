using System.Collections;
using System.Data.SqlClient;

namespace System.Data.Entity.SqlServer;

internal static class SqlAzureRetriableExceptionDetector
{
	public static bool ShouldRetryOn(Exception ex)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		SqlException val = (SqlException)(object)((ex is SqlException) ? ex : null);
		if (val != null)
		{
			{
				IEnumerator enumerator = val.Errors.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						switch (((SqlError)enumerator.Current).Number)
						{
						case 20:
						case 64:
						case 121:
						case 233:
						case 1205:
						case 10053:
						case 10054:
						case 10060:
						case 10928:
						case 10929:
						case 40197:
						case 40501:
						case 40613:
						case 41301:
						case 41302:
						case 41305:
						case 41325:
						case 41839:
						case 49918:
						case 49919:
						case 49920:
							return true;
						}
					}
				}
				finally
				{
					IDisposable disposable = enumerator as IDisposable;
					if (disposable != null)
					{
						disposable.Dispose();
					}
				}
			}
			return false;
		}
		if (ex is TimeoutException)
		{
			return true;
		}
		return false;
	}
}
