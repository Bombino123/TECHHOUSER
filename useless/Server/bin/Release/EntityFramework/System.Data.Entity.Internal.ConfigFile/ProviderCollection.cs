using System.Configuration;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Internal.ConfigFile;

internal class ProviderCollection : ConfigurationElementCollection
{
	private const string ProviderKey = "provider";

	public override ConfigurationElementCollectionType CollectionType => (ConfigurationElementCollectionType)0;

	protected override string ElementName => "provider";

	protected override ConfigurationElement CreateNewElement()
	{
		return (ConfigurationElement)(object)new ProviderElement();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((ProviderElement)(object)element).InvariantName;
	}

	protected override void BaseAdd(ConfigurationElement element)
	{
		if (!ValidateProviderElement(element))
		{
			((ConfigurationElementCollection)this).BaseAdd(element);
		}
	}

	protected override void BaseAdd(int index, ConfigurationElement element)
	{
		if (!ValidateProviderElement(element))
		{
			((ConfigurationElementCollection)this).BaseAdd(index, element);
		}
	}

	private bool ValidateProviderElement(ConfigurationElement element)
	{
		object elementKey = ((ConfigurationElementCollection)this).GetElementKey(element);
		ProviderElement providerElement = (ProviderElement)(object)((ConfigurationElementCollection)this).BaseGet(elementKey);
		if (providerElement != null && providerElement.ProviderTypeName != ((ProviderElement)(object)element).ProviderTypeName)
		{
			throw new InvalidOperationException(Strings.ProviderInvariantRepeatedInConfig(elementKey));
		}
		return providerElement != null;
	}

	public ProviderElement AddProvider(string invariantName, string providerTypeName)
	{
		ProviderElement providerElement = (ProviderElement)(object)((ConfigurationElementCollection)this).CreateNewElement();
		((ConfigurationElementCollection)this).BaseAdd((ConfigurationElement)(object)providerElement);
		providerElement.InvariantName = invariantName;
		providerElement.ProviderTypeName = providerTypeName;
		return providerElement;
	}
}
