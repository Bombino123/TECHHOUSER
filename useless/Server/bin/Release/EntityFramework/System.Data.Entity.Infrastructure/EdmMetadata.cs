using System.Data.Entity.Internal;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Infrastructure;

[Obsolete("EdmMetadata is no longer used. The Code First Migrations <see cref=\"EdmModelDiffer\" /> is used instead.")]
public class EdmMetadata
{
	public int Id { get; set; }

	public string ModelHash { get; set; }

	public static string TryGetModelHash(DbContext context)
	{
		Check.NotNull(context, "context");
		DbCompiledModel codeFirstModel = context.InternalContext.CodeFirstModel;
		if (codeFirstModel != null)
		{
			return new ModelHashCalculator().Calculate(codeFirstModel);
		}
		return null;
	}
}
