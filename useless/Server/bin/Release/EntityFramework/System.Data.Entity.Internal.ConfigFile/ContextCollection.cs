using System.Configuration;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Internal.ConfigFile;

internal class ContextCollection : ConfigurationElementCollection
{
	private const string ContextKey = "context";

	public override ConfigurationElementCollectionType CollectionType => (ConfigurationElementCollectionType)0;

	protected override string ElementName => "context";

	protected override ConfigurationElement CreateNewElement()
	{
		return (ConfigurationElement)(object)new ContextElement();
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((ContextElement)(object)element).ContextTypeName;
	}

	protected override void BaseAdd(ConfigurationElement element)
	{
		object elementKey = ((ConfigurationElementCollection)this).GetElementKey(element);
		if (((ConfigurationElementCollection)this).BaseGet(elementKey) != null)
		{
			throw Error.ContextConfiguredMultipleTimes(elementKey);
		}
		((ConfigurationElementCollection)this).BaseAdd(element);
	}

	protected override void BaseAdd(int index, ConfigurationElement element)
	{
		object elementKey = ((ConfigurationElementCollection)this).GetElementKey(element);
		if (((ConfigurationElementCollection)this).BaseGet(elementKey) != null)
		{
			throw Error.ContextConfiguredMultipleTimes(elementKey);
		}
		((ConfigurationElementCollection)this).BaseAdd(index, element);
	}
}
