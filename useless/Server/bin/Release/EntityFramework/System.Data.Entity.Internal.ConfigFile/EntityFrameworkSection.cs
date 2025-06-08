using System.Configuration;

namespace System.Data.Entity.Internal.ConfigFile;

internal class EntityFrameworkSection : ConfigurationSection
{
	private const string DefaultConnectionFactoryKey = "defaultConnectionFactory";

	private const string ContextsKey = "contexts";

	private const string ProviderKey = "providers";

	private const string ConfigurationTypeKey = "codeConfigurationType";

	private const string InterceptorsKey = "interceptors";

	private const string QueryCacheKey = "queryCache";

	[ConfigurationProperty("defaultConnectionFactory")]
	public virtual DefaultConnectionFactoryElement DefaultConnectionFactory
	{
		get
		{
			return (DefaultConnectionFactoryElement)((ConfigurationElement)this)["defaultConnectionFactory"];
		}
		set
		{
			((ConfigurationElement)this)["defaultConnectionFactory"] = value;
		}
	}

	[ConfigurationProperty("codeConfigurationType")]
	public virtual string ConfigurationTypeName
	{
		get
		{
			return (string)((ConfigurationElement)this)["codeConfigurationType"];
		}
		set
		{
			((ConfigurationElement)this)["codeConfigurationType"] = value;
		}
	}

	[ConfigurationProperty("providers")]
	public virtual ProviderCollection Providers => (ProviderCollection)((ConfigurationElement)this)["providers"];

	[ConfigurationProperty("contexts")]
	public virtual ContextCollection Contexts => (ContextCollection)((ConfigurationElement)this)["contexts"];

	[ConfigurationProperty("interceptors")]
	public virtual InterceptorsCollection Interceptors => (InterceptorsCollection)((ConfigurationElement)this)["interceptors"];

	[ConfigurationProperty("queryCache")]
	public virtual QueryCacheElement QueryCache
	{
		get
		{
			return (QueryCacheElement)((ConfigurationElement)this)["queryCache"];
		}
		set
		{
			((ConfigurationElement)this)["queryCache"] = value;
		}
	}
}
