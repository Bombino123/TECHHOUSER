namespace System.Data.Entity.Infrastructure.Design;

public abstract class HandlerBase : MarshalByRefObject
{
	public virtual bool ImplementsContract(string interfaceName)
	{
		Type type;
		try
		{
			type = Type.GetType(interfaceName, throwOnError: true);
		}
		catch
		{
			return false;
		}
		return type.IsAssignableFrom(GetType());
	}
}
