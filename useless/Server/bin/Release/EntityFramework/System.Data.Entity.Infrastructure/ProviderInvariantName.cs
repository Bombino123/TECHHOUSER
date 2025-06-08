namespace System.Data.Entity.Infrastructure;

internal class ProviderInvariantName : IProviderInvariantName
{
	public string Name { get; private set; }

	public ProviderInvariantName(string name)
	{
		Name = name;
	}
}
