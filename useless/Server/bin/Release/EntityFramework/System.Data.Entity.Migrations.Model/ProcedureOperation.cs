using System.Collections.Generic;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Migrations.Model;

public abstract class ProcedureOperation : MigrationOperation
{
	private readonly string _name;

	private readonly string _bodySql;

	private readonly List<ParameterModel> _parameters = new List<ParameterModel>();

	public virtual string Name => _name;

	public string BodySql => _bodySql;

	public virtual IList<ParameterModel> Parameters => _parameters;

	public override bool IsDestructiveChange => false;

	protected ProcedureOperation(string name, string bodySql, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		_name = name;
		_bodySql = bodySql;
	}
}
