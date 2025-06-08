namespace System.Data.Entity.Core.Objects;

public sealed class ObjectContextOptions
{
	public bool EnsureTransactionsForFunctionsAndCommands { get; set; }

	public bool LazyLoadingEnabled { get; set; }

	public bool ProxyCreationEnabled { get; set; }

	public bool UseLegacyPreserveChangesBehavior { get; set; }

	public bool UseConsistentNullReferenceBehavior { get; set; }

	public bool UseCSharpNullComparisonBehavior { get; set; }

	public bool DisableFilterOverProjectionSimplificationForCustomFunctions { get; set; }

	internal ObjectContextOptions()
	{
		ProxyCreationEnabled = true;
		EnsureTransactionsForFunctionsAndCommands = true;
	}
}
