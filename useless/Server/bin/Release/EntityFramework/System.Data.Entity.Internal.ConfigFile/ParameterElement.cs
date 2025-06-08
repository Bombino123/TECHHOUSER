using System.Configuration;
using System.Globalization;

namespace System.Data.Entity.Internal.ConfigFile;

internal class ParameterElement : ConfigurationElement
{
	private const string ValueKey = "value";

	private const string TypeKey = "type";

	internal int Key { get; private set; }

	[ConfigurationProperty("value", IsRequired = true)]
	public string ValueString
	{
		get
		{
			return (string)((ConfigurationElement)this)["value"];
		}
		set
		{
			((ConfigurationElement)this)["value"] = value;
		}
	}

	[ConfigurationProperty("type", DefaultValue = "System.String")]
	public string TypeName
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

	public ParameterElement(int key)
	{
		Key = key;
	}

	public object GetTypedParameterValue()
	{
		Type type = Type.GetType(TypeName, throwOnError: true);
		return Convert.ChangeType(ValueString, type, CultureInfo.InvariantCulture);
	}
}
