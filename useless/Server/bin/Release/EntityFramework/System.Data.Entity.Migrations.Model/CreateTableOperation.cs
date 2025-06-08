using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Migrations.Model;

public class CreateTableOperation : MigrationOperation, IAnnotationTarget
{
	private readonly string _name;

	private readonly List<ColumnModel> _columns = new List<ColumnModel>();

	private AddPrimaryKeyOperation _primaryKey;

	private readonly IDictionary<string, object> _annotations;

	public virtual string Name => _name;

	public virtual IList<ColumnModel> Columns => _columns;

	public AddPrimaryKeyOperation PrimaryKey
	{
		get
		{
			return _primaryKey;
		}
		set
		{
			Check.NotNull(value, "value");
			_primaryKey = value;
			_primaryKey.Table = Name;
		}
	}

	public virtual IDictionary<string, object> Annotations => _annotations;

	public override MigrationOperation Inverse => new DropTableOperation(Name, Annotations, Columns.Where((ColumnModel c) => c.Annotations.Count > 0).ToDictionary((Func<ColumnModel, string>)((ColumnModel c) => c.Name), (Func<ColumnModel, IDictionary<string, object>>)((ColumnModel c) => c.Annotations.ToDictionary((KeyValuePair<string, AnnotationValues> a) => a.Key, (KeyValuePair<string, AnnotationValues> a) => a.Value.NewValue))));

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

	public CreateTableOperation(string name, object anonymousArguments = null)
		: this(name, null, anonymousArguments)
	{
	}

	public CreateTableOperation(string name, IDictionary<string, object> annotations, object anonymousArguments = null)
		: base(anonymousArguments)
	{
		Check.NotEmpty(name, "name");
		_name = name;
		_annotations = annotations ?? new Dictionary<string, object>();
	}
}
