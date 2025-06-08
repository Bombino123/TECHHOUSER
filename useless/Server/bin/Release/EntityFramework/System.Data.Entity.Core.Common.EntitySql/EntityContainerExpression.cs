using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class EntityContainerExpression : ExpressionResolution
{
	internal readonly EntityContainer EntityContainer;

	internal override string ExpressionClassName => EntityContainerClassName;

	internal static string EntityContainerClassName => Strings.LocalizedEntityContainerExpression;

	internal EntityContainerExpression(EntityContainer entityContainer)
		: base(ExpressionResolutionClass.EntityContainer)
	{
		EntityContainer = entityContainer;
	}
}
