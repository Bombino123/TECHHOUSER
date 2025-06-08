using System.Collections.Generic;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class AttributeToTableAnnotationConvention<TAttribute, TAnnotation> : Convention where TAttribute : Attribute
{
	public AttributeToTableAnnotationConvention(string annotationName, Func<Type, IList<TAttribute>, TAnnotation> annotationFactory)
	{
		Check.NotEmpty(annotationName, "annotationName");
		Check.NotNull(annotationFactory, "annotationFactory");
		AttributeProvider attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();
		Types().Having((Type t) => attributeProvider.GetAttributes(t).OfType<TAttribute>().ToList()).Configure(delegate(ConventionTypeConfiguration c, List<TAttribute> a)
		{
			if (a.Any())
			{
				c.HasTableAnnotation(annotationName, annotationFactory(c.ClrType, a));
			}
		});
	}
}
