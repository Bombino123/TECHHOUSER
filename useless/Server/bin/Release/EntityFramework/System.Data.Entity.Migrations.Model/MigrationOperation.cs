using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Reflection;

namespace System.Data.Entity.Migrations.Model;

public abstract class MigrationOperation
{
	private readonly IDictionary<string, object> _anonymousArguments = new Dictionary<string, object>();

	public IDictionary<string, object> AnonymousArguments => _anonymousArguments;

	public virtual MigrationOperation Inverse => null;

	public abstract bool IsDestructiveChange { get; }

	protected MigrationOperation(object anonymousArguments)
	{
		MigrationOperation migrationOperation = this;
		if (anonymousArguments != null)
		{
			anonymousArguments.GetType().GetNonIndexerProperties().Each(delegate(PropertyInfo p)
			{
				migrationOperation._anonymousArguments.Add(p.Name, p.GetValue(anonymousArguments, null));
			});
		}
	}
}
