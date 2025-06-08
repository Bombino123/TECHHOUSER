namespace System.Data.Entity.Core.Objects;

[Flags]
public enum SaveOptions
{
	None = 0,
	AcceptAllChangesAfterSave = 1,
	DetectChangesBeforeSave = 2
}
