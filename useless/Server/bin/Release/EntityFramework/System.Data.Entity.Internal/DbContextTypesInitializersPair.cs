using System.Collections.Generic;

namespace System.Data.Entity.Internal;

internal class DbContextTypesInitializersPair : Tuple<Dictionary<Type, List<string>>, Action<DbContext>>
{
	public Dictionary<Type, List<string>> EntityTypeToPropertyNameMap => base.Item1;

	public Action<DbContext> SetsInitializer => base.Item2;

	public DbContextTypesInitializersPair(Dictionary<Type, List<string>> entityTypeToPropertyNameMap, Action<DbContext> setsInitializer)
		: base(entityTypeToPropertyNameMap, setsInitializer)
	{
	}
}
