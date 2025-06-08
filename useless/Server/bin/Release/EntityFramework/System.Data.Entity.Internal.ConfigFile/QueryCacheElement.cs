using System.Configuration;

namespace System.Data.Entity.Internal.ConfigFile;

internal class QueryCacheElement : ConfigurationElement
{
	private const string SizeKey = "size";

	private const string CleaningIntervalInSecondsKey = "cleaningIntervalInSeconds";

	[ConfigurationProperty("size")]
	[IntegerValidator(MinValue = 0, MaxValue = int.MaxValue)]
	public int Size
	{
		get
		{
			return (int)((ConfigurationElement)this)["size"];
		}
		set
		{
			((ConfigurationElement)this)["size"] = value;
		}
	}

	[ConfigurationProperty("cleaningIntervalInSeconds")]
	[IntegerValidator(MinValue = 0, MaxValue = int.MaxValue)]
	public int CleaningIntervalInSeconds
	{
		get
		{
			return (int)((ConfigurationElement)this)["cleaningIntervalInSeconds"];
		}
		set
		{
			((ConfigurationElement)this)["cleaningIntervalInSeconds"] = value;
		}
	}
}
