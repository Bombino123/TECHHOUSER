using System.Collections.Generic;

namespace System.Data.Entity.Infrastructure.DependencyResolution;

public interface IDbDependencyResolver
{
	object GetService(Type type, object key);

	IEnumerable<object> GetServices(Type type, object key);
}
