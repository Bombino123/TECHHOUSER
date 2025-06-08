namespace System.Data.Entity.Internal;

internal enum DatabaseExistenceState
{
	Unknown,
	DoesNotExist,
	ExistsConsideredEmpty,
	Exists
}
