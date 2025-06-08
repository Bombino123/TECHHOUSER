using System.Configuration;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Internal.ConfigFile;

internal class InterceptorElement : ConfigurationElement
{
	private const string TypeKey = "type";

	private const string ParametersKey = "parameters";

	internal int Key { get; private set; }

	[ConfigurationProperty("type", IsRequired = true)]
	public virtual string TypeName
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

	public InterceptorElement(int key)
	{
		Key = key;
	}

	public virtual IDbInterceptor CreateInterceptor()
	{
		object obj;
		try
		{
			obj = Activator.CreateInstance(Type.GetType(TypeName, throwOnError: true), Parameters.GetTypedParameterValues());
		}
		catch (Exception innerException)
		{
			throw new InvalidOperationException(Strings.InterceptorTypeNotFound(TypeName), innerException);
		}
		return (obj as IDbInterceptor) ?? throw new InvalidOperationException(Strings.InterceptorTypeNotInterceptor(TypeName));
	}
}
