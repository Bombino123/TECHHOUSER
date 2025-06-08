using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Migrations.Design;

public abstract class MigrationCodeGenerator
{
	private readonly IDictionary<string, Func<AnnotationCodeGenerator>> _annotationGenerators = new Dictionary<string, Func<AnnotationCodeGenerator>>();

	public virtual IDictionary<string, Func<AnnotationCodeGenerator>> AnnotationGenerators => _annotationGenerators;

	public abstract ScaffoldedMigration Generate(string migrationId, IEnumerable<MigrationOperation> operations, string sourceModel, string targetModel, string @namespace, string className);

	private static bool AnnotationsExist(MigrationOperation[] operations)
	{
		return operations.OfType<IAnnotationTarget>().Any((IAnnotationTarget o) => o.HasAnnotations);
	}

	protected virtual IEnumerable<string> GetNamespaces(IEnumerable<MigrationOperation> operations)
	{
		Check.NotNull(operations, "operations");
		IEnumerable<string> enumerable = GetDefaultNamespaces();
		MigrationOperation[] array = operations.ToArray();
		if (array.OfType<AddColumnOperation>().Any((AddColumnOperation o) => o.Column.Type == PrimitiveTypeKind.Geography || o.Column.Type == PrimitiveTypeKind.Geometry))
		{
			enumerable = enumerable.Concat(new string[1] { "System.Data.Entity.Spatial" });
		}
		if (array.OfType<AddColumnOperation>().Any((AddColumnOperation o) => o.Column.Type == PrimitiveTypeKind.HierarchyId))
		{
			enumerable = enumerable.Concat(new string[1] { "System.Data.Entity.Hierarchy" });
		}
		if (AnnotationsExist(array))
		{
			enumerable = enumerable.Concat(new string[2] { "System.Collections.Generic", "System.Data.Entity.Infrastructure.Annotations" });
			enumerable = (from a in AnnotationGenerators
				select a.Value into g
				where g != null
				select g).Aggregate(enumerable, (IEnumerable<string> c, Func<AnnotationCodeGenerator> g) => c.Concat(g().GetExtraNamespaces(AnnotationGenerators.Keys)));
		}
		return from n in enumerable.Distinct()
			orderby n
			select n;
	}

	protected virtual IEnumerable<string> GetDefaultNamespaces(bool designer = false)
	{
		List<string> list = new List<string> { "System.Data.Entity.Migrations" };
		if (designer)
		{
			list.Add("System.CodeDom.Compiler");
			list.Add("System.Data.Entity.Migrations.Infrastructure");
			list.Add("System.Resources");
		}
		else
		{
			list.Add("System");
		}
		return list.OrderBy((string n) => n);
	}
}
