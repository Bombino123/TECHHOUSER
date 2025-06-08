using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Internal;

internal class ModelCompatibilityChecker
{
	public virtual bool CompatibleWithModel(InternalContext internalContext, ModelHashCalculator modelHashCalculator, bool throwIfNoMetadata, DatabaseExistenceState existenceState = DatabaseExistenceState.Unknown)
	{
		if (internalContext.CodeFirstModel == null)
		{
			if (throwIfNoMetadata)
			{
				throw Error.Database_NonCodeFirstCompatibilityCheck();
			}
			return true;
		}
		VersionedModel versionedModel = internalContext.QueryForModel(existenceState);
		if (versionedModel != null)
		{
			return internalContext.ModelMatches(versionedModel);
		}
		string text = internalContext.QueryForModelHash();
		if (text == null)
		{
			if (throwIfNoMetadata)
			{
				throw Error.Database_NoDatabaseMetadata();
			}
			return true;
		}
		return string.Equals(text, modelHashCalculator.Calculate(internalContext.CodeFirstModel), StringComparison.Ordinal);
	}
}
