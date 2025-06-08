namespace System.Data.Entity.Core.Mapping.Update.Internal;

[Flags]
internal enum PropagatorFlags : byte
{
	NoFlags = 0,
	Preserve = 1,
	ConcurrencyValue = 2,
	Unknown = 8,
	Key = 0x10,
	ForeignKey = 0x20
}
