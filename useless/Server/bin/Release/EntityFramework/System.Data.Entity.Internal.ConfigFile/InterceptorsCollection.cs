using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;

namespace System.Data.Entity.Internal.ConfigFile;

internal class InterceptorsCollection : ConfigurationElementCollection
{
	private const string ElementKey = "interceptor";

	private int _nextKey;

	public override ConfigurationElementCollectionType CollectionType => (ConfigurationElementCollectionType)0;

	protected override string ElementName => "interceptor";

	public virtual IEnumerable<IDbInterceptor> Interceptors => (from e in ((IEnumerable)this).OfType<InterceptorElement>()
		select e.CreateInterceptor()).ToList();

	protected override ConfigurationElement CreateNewElement()
	{
		return (ConfigurationElement)(object)new InterceptorElement(_nextKey++);
	}

	protected override object GetElementKey(ConfigurationElement element)
	{
		return ((InterceptorElement)(object)element).Key;
	}

	public void AddElement(InterceptorElement element)
	{
		((ConfigurationElementCollection)this).BaseAdd((ConfigurationElement)(object)element);
	}
}
