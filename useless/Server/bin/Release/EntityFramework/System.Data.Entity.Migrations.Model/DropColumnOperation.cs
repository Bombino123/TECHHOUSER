using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Migrations.Model;

public class DropColumnOperation : MigrationOperation, IAnnotationTarget
{
	private readonly string _table;

	private readonly string _name;

	private readonly AddColumnOperation _inverse;

	private readonly IDictionary<string, object> _removedAnnotations;

	public string Table => _table;

	public string Name => _name;

	public IDictionary<string, object> RemovedAnnotations => _removedAnnotations;

	public override MigrationOperation Inverse => _inverse;

	public override bool IsDestructiveChange => true;

	bool IAnnotationTarget.HasAnnotations
	{
		get
		{
			AddColumnOperation addColumnOperation = Inverse as AddColumnOperation;
			if (!RemovedAnnotations.Any())
			{
				return ((IAnnotationTarget)addColumnOperation)?.HasAnnotations ?? false;
			}
			return true;
		}
	}

	public DropColumnOperation(string table, string name, object anonymousArguments = null)
		: this(table, name, null, null, anonymousArguments)
	{
	}

	public DropColumnOperation(string table, string name, IDictionary<string, object> removedAnnotations, object anonymousArguments = null)
		: this(table, name, removedAnnotations, null, anonymousArguments)
	{
	}

	public DropColumnOperation(string table, string name, AddColumnOperation inverse, object anonymousArguments = null)
		: this(table, name, null, inverse, anonymousArguments)
	{
	}

	public DropColumnOperation(string table, string name, IDictionary<string, object> removedAnnotations, AddColumnOperation inverse, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(table, "table");
		Check.NotEmpty(name, "name");
		_table = table;
		_name = name;
		_removedAnnotations = removedAnnotations ?? new Dictionary<string, object>();
		_inverse = inverse;
	}
}
