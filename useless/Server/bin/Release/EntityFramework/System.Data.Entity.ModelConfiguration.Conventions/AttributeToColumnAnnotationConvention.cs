using System.Collections.Generic;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.ModelConfiguration.Conventions;

public class AttributeToColumnAnnotationConvention<TAttribute, TAnnotation> : Convention where TAttribute : Attribute
{
	public AttributeToColumnAnnotationConvention(string annotationName, Func<PropertyInfo, IList<TAttribute>, TAnnotation> annotationFactory)
	{
		Check.NotEmpty(annotationName, "annotationName");
		Check.NotNull(annotationFactory, "annotationFactory");
		AttributeProvider attributeProvider = DbConfiguration.DependencyResolver.GetService<AttributeProvider>();
		Properties().Having((PropertyInfo pi) => attributeProvider.GetAttributes(pi).OfType<TAttribute>().ToList()).Configure(delegate(ConventionPrimitivePropertyConfiguration c, List<TAttribute> a)
		{
			if (a.Any())
			{
				c.HasColumnAnnotation(annotationName, annotationFactory(c.ClrPropertyInfo, a));
			}
		});
	}
}
