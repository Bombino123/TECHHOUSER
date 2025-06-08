namespace System.Data.Entity.Core.Objects;

internal enum ObjectStateValueRecord
{
	OriginalReadonly,
	CurrentUpdatable,
	OriginalUpdatableInternal,
	OriginalUpdatablePublic
}
