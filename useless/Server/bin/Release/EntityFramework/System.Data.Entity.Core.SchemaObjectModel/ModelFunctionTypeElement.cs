using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace System.Data.Entity.Core.SchemaObjectModel;

internal abstract class ModelFunctionTypeElement : FacetEnabledSchemaElement
{
	protected TypeUsage _typeUsage;

	internal ModelFunctionTypeElement(SchemaElement parentElement)
		: base(parentElement)
	{
		_typeUsageBuilder = new TypeUsageBuilder(this);
	}

	internal abstract void WriteIdentity(StringBuilder builder);

	internal abstract TypeUsage GetTypeUsage();

	internal abstract bool ResolveNameAndSetTypeUsage(Converter.ConversionCache convertedItemCache, Dictionary<SchemaElement, GlobalItem> newGlobalItems);
}
