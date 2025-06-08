using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Migrations.Model;

public class AlterTableOperation : MigrationOperation, IAnnotationTarget
{
	private readonly string _name;

	private readonly List<ColumnModel> _columns = new List<ColumnModel>();

	private readonly IDictionary<string, AnnotationValues> _annotations;

	public virtual string Name => _name;

	public virtual IList<ColumnModel> Columns => _columns;

	public virtual IDictionary<string, AnnotationValues> Annotations => _annotations;

	public override MigrationOperation Inverse
	{
		get
		{
			AlterTableOperation alterTableOperation = new AlterTableOperation(Name, Annotations.ToDictionary((KeyValuePair<string, AnnotationValues> a) => a.Key, (KeyValuePair<string, AnnotationValues> a) => new AnnotationValues(a.Value.NewValue, a.Value.OldValue)));
			alterTableOperation._columns.AddRange(_columns);
			return alterTableOperation;
		}
	}

	public override bool IsDestructiveChange => false;

	bool IAnnotationTarget.HasAnnotations
	{
		get
		{
			if (!Annotations.Any())
			{
				return Columns.SelectMany((ColumnModel c) => c.Annotations).Any();
			}
			return true;
		}
	}

	public AlterTableOperation(string name, IDictionary<string, AnnotationValues> annotations, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		_name = name;
		_annotations = annotations ?? new Dictionary<string, AnnotationValues>();
	}
}
