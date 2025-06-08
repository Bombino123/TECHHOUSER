using System.Data.Entity.Core.Objects;

namespace System.Data.Entity.Infrastructure;

public interface IObjectContextAdapter
{
	ObjectContext ObjectContext { get; }
}
