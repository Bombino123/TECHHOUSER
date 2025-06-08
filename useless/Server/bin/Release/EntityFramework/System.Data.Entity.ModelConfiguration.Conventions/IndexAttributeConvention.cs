using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class IndexAttributeConvention : AttributeToColumnAnnotationConvention<IndexAttribute, IndexAnnotation>
{
	public IndexAttributeConvention()
		: base("Index", (Func<PropertyInfo, IList<IndexAttribute>, IndexAnnotation>)((PropertyInfo p, IList<IndexAttribute> a) => new IndexAnnotation(p, a.OrderBy((IndexAttribute i) => i.ToString()))))
	{
	}
}
