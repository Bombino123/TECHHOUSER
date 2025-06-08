using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class ForeignKeyIndexConvention : IStoreModelConvention<AssociationType>, IConvention
{
	public virtual void Apply(AssociationType item, DbModel model)
	{
		Check.NotNull(item, "item");
		if (item.Constraint == null)
		{
			return;
		}
		IEnumerable<ConsolidatedIndex> source = ConsolidatedIndex.BuildIndexes(item.Name, item.Constraint.ToProperties.Select((EdmProperty p) => Tuple.Create(p.Name, p)));
		IEnumerable<string> dependentColumnNames = item.Constraint.ToProperties.Select((EdmProperty p) => p.Name);
		if (source.Any((ConsolidatedIndex c) => c.Columns.SequenceEqual(dependentColumnNames)))
		{
			return;
		}
		string name = IndexOperation.BuildDefaultName(dependentColumnNames);
		int num = 0;
		foreach (EdmProperty toProperty in item.Constraint.ToProperties)
		{
			IndexAnnotation indexAnnotation = new IndexAnnotation(new IndexAttribute(name, num++));
			object annotation = toProperty.Annotations.GetAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:Index");
			if (annotation != null)
			{
				indexAnnotation = (IndexAnnotation)((IndexAnnotation)annotation).MergeWith(indexAnnotation);
			}
			toProperty.AddAnnotation("http://schemas.microsoft.com/ado/2013/11/edm/customannotation:Index", indexAnnotation);
		}
	}
}
