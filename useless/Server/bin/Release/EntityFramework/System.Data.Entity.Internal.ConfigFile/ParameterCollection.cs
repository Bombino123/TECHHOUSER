using System.Collections;
using System.Configuration;
using System.Linq;

namespace System.Data.Entity.Internal.ConfigFile;

internal class ParameterCollection : ConfigurationElementCollection
{
	private const string ParameterKey = "parameter";

	private int _nextKey;

	public override ConfigurationElementCollectionType CollectionType => (ConfigurationElementCollectionType)0;

	protected override string ElementName => "parameter";

	protected override ConfigurationElement CreateNewElement()
	{
		ParameterElement result = new ParameterElement(_nextKey);
		_nextKey++;
		return (ConfigurationElement)(object)result;
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((ParameterElement)(object)element).Key;
	}

	public virtual object[] GetTypedParameterValues()
	{
		return (from ParameterElement e in (IEnumerable)this
			select e.GetTypedParameterValue()).ToArray();
	}

	internal ParameterElement NewElement()
	{
		ConfigurationElement val = ((ConfigurationElementCollection)this).CreateNewElement();
		((ConfigurationElementCollection)this).BaseAdd(val);
		return (ParameterElement)(object)val;
	}
}
