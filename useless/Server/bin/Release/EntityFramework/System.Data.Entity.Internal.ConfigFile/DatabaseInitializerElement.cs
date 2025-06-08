using System.Configuration;

namespace System.Data.Entity.Internal.ConfigFile;

internal class DatabaseInitializerElement : ConfigurationElement
{
	private const string TypeKey = "type";

	private const string ParametersKey = "parameters";

	[ConfigurationProperty("type", IsRequired = true)]
	public virtual string InitializerTypeName
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

	[ConfigurationProperty("parameters")]
	public virtual ParameterCollection Parameters => (ParameterCollection)((ConfigurationElement)this)["parameters"];
}
