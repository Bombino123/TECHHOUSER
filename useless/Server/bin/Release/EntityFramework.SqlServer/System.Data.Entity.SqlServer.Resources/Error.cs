using System.CodeDom.Compiler;

namespace System.Data.Entity.SqlServer.Resources;

[GeneratedCode("Resources.SqlServer.tt", "1.0.0.0")]
internal static class Error
{
	internal static Exception InvalidDatabaseName(object p0)
	{
		return new ArgumentException(Strings.InvalidDatabaseName(p0));
	}

	internal static Exception SqlServerMigrationSqlGenerator_UnknownOperation(object p0, object p1)
	{
		return new InvalidOperationException(Strings.SqlServerMigrationSqlGenerator_UnknownOperation(p0, p1));
	}

	internal static Exception ArgumentOutOfRange(string paramName)
	{
		return new ArgumentOutOfRangeException(paramName);
	}

	internal static Exception NotImplemented()
	{
		return new NotImplementedException();
	}

	internal static Exception NotSupported()
	{
		return new NotSupportedException();
	}
}
