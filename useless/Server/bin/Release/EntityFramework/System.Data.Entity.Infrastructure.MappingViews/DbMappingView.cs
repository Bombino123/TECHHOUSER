using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure.MappingViews;

public class DbMappingView
{
	private readonly string _entitySql;

	public string EntitySql => _entitySql;

	public DbMappingView(string entitySql)
	{
		Check.NotEmpty(entitySql, "entitySql");
		_entitySql = entitySql;
	}
}
