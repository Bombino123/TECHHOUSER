using System.Collections.Generic;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Migrations.Model;

public class DropTableOperation : MigrationOperation, IAnnotationTarget
{
	private readonly string _name;

	private readonly CreateTableOperation _inverse;

	private readonly IDictionary<string, IDictionary<string, object>> _removedColumnAnnotations;

	private readonly IDictionary<string, object> _removedAnnotations;

	public virtual string Name => _name;

	public virtual IDictionary<string, object> RemovedAnnotations => _removedAnnotations;

	public IDictionary<string, IDictionary<string, object>> RemovedColumnAnnotations => _removedColumnAnnotations;

	public override MigrationOperation Inverse => _inverse;

	public override bool IsDestructiveChange => true;

	bool IAnnotationTarget.HasAnnotations
	{
		get
		{
			CreateTableOperation createTableOperation = Inverse as CreateTableOperation;
			if (!RemovedAnnotations.Any() && !RemovedColumnAnnotations.Any())
			{
				return ((IAnnotationTarget)createTableOperation)?.HasAnnotations ?? false;
			}
			return true;
		}
	}

	public DropTableOperation(string name, object anonymousArguments = null)
		: this(name, null, null, null, anonymousArguments)
	{
	}

	public DropTableOperation(string name, IDictionary<string, object> removedAnnotations, IDictionary<string, IDictionary<string, object>> removedColumnAnnotations, object anonymousArguments = null)
		: this(name, removedAnnotations, removedColumnAnnotations, null, anonymousArguments)
	{
	}

	public DropTableOperation(string name, CreateTableOperation inverse, object anonymousArguments = null)
		: this(name, null, null, inverse, anonymousArguments)
	{
	}

	public DropTableOperation(string name, IDictionary<string, object> removedAnnotations, IDictionary<string, IDictionary<string, object>> removedColumnAnnotations, CreateTableOperation inverse, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		_name = name;
		_removedAnnotations = removedAnnotations ?? new Dictionary<string, object>();
		_removedColumnAnnotations = removedColumnAnnotations ?? new Dictionary<string, IDictionary<string, object>>();
		_inverse = inverse;
	}
}
