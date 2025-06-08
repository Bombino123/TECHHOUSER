using System.Configuration;

namespace System.Data.Entity.Internal.ConfigFile;

internal class DefaultConnectionFactoryElement : ConfigurationElement
{
	private const string TypeKey = "type";

	private const string ParametersKey = "parameters";

	[ConfigurationProperty("type", IsRequired = true)]
	public string FactoryTypeName
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
	public ParameterCollection Parameters => (ParameterCollection)((ConfigurationElement)this)["parameters"];

	public Type GetFactoryType()
	{
		return Type.GetType(FactoryTypeName, throwOnError: true);
	}
}
