namespace System.Data.Entity.Core.Common.EntitySql;

internal class ErrorContext
{
	internal int InputPosition = -1;

	internal string ErrorContextInfo;

	internal bool UseContextInfoAsResourceIdentifier = true;

	internal string CommandText;
}
