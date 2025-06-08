using System.Configuration;

namespace System.Data.Entity.Internal.ConfigFile;

internal class ContextElement : ConfigurationElement
{
	private const string TypeKey = "type";

	private const string CommandTimeoutKey = "commandTimeout";

	private const string DisableDatabaseInitializationKey = "disableDatabaseInitialization";

	private const string DatabaseInitializerKey = "databaseInitializer";

	[ConfigurationProperty("type", IsRequired = true)]
	public virtual string ContextTypeName
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

	[ConfigurationProperty("commandTimeout")]
	public virtual int? CommandTimeout
	{
		get
		{
			return (int?)((ConfigurationElement)this)["commandTimeout"];
		}
		set
		{
			((ConfigurationElement)this)["commandTimeout"] = value;
		}
	}

	[ConfigurationProperty("disableDatabaseInitialization", DefaultValue = false)]
	public virtual bool IsDatabaseInitializationDisabled
	{
		get
		{
			return (bool)((ConfigurationElement)this)["disableDatabaseInitialization"];
		}
		set
		{
			((ConfigurationElement)this)["disableDatabaseInitialization"] = value;
		}
	}

	[ConfigurationProperty("databaseInitializer")]
	public virtual DatabaseInitializerElement DatabaseInitializer
	{
		get
		{
			return (DatabaseInitializerElement)((ConfigurationElement)this)["databaseInitializer"];
		}
		set
		{
			((ConfigurationElement)this)["databaseInitializer"] = value;
		}
	}
}
