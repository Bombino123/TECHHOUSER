using System.Data.Entity.Core;
using System.Security;
using System.Threading;

namespace System.Data.Entity.Utilities;

internal static class ExceptionExtensions
{
	public static bool IsCatchableExceptionType(this Exception e)
	{
		Type type = e.GetType();
		if (type != typeof(StackOverflowException) && type != typeof(OutOfMemoryException) && type != typeof(ThreadAbortException) && type != typeof(NullReferenceException) && type != typeof(AccessViolationException))
		{
			return !typeof(SecurityException).IsAssignableFrom(type);
		}
		return false;
	}

	public static bool IsCatchableEntityExceptionType(this Exception e)
	{
		Type type = e.GetType();
		if (e.IsCatchableExceptionType() && type != typeof(EntityCommandExecutionException) && type != typeof(EntityCommandCompilationException))
		{
			return type != typeof(EntitySqlException);
		}
		return false;
	}

	public static bool RequiresContext(this Exception e)
	{
		if (!e.IsCatchableExceptionType())
		{
			return false;
		}
		if (!(e is UpdateException))
		{
			return !(e is ProviderIncompatibleException);
		}
		return false;
	}
}
