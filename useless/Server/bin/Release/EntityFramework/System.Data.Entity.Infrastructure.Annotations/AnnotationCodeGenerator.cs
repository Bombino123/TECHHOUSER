using System.Collections.Generic;
using System.Data.Entity.Migrations.Utilities;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Infrastructure.Annotations;

public abstract class AnnotationCodeGenerator
{
	public virtual IEnumerable<string> GetExtraNamespaces(IEnumerable<string> annotationNames)
	{
		Check.NotNull(annotationNames, "annotationNames");
		return Enumerable.Empty<string>();
	}

	public abstract void Generate(string annotationName, object annotation, IndentedTextWriter writer);
}
