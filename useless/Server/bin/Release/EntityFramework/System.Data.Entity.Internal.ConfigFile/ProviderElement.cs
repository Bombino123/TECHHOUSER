using System.Configuration;

namespace System.Data.Entity.Internal.ConfigFile;

internal class ProviderElement : ConfigurationElement
{
	private const string InvariantNameKey = "invariantName";

	private const string TypeKey = "type";

	[ConfigurationProperty("invariantName", IsRequired = true)]
	public string InvariantName
	{
		get
		{
			return (string)((ConfigurationElement)this)["invariantName"];
		}
		set
		{
			((ConfigurationElement)this)["invariantName"] = value;
		}
	}

	[ConfigurationProperty("type", IsRequired = true)]
	public string ProviderTypeName
	{
		get
		{
			return (string)((ConfigurationElement)this)["type"];
		}
		set
		{
			((ConfigurationElement)this)["type"] = value;
		}
	}
}
