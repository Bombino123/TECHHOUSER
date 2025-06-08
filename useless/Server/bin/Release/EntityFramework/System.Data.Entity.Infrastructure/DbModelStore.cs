using System.Xml.Linq;

namespace System.Data.Entity.Infrastructure;

public abstract class DbModelStore
{
	public abstract DbCompiledModel TryLoad(Type contextType);

	public abstract XDocument TryGetEdmx(Type contextType);

	public abstract void Save(Type contextType, DbModel model);

	protected virtual string GetDefaultSchema(Type contextType)
	{
		return "dbo";
	}
}
