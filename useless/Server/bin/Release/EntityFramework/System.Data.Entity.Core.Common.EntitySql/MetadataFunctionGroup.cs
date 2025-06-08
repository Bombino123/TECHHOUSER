using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class MetadataFunctionGroup : MetadataMember
{
	internal readonly IList<EdmFunction> FunctionMetadata;

	internal override string MetadataMemberClassName => FunctionGroupClassName;

	internal static string FunctionGroupClassName => Strings.LocalizedFunction;

	internal MetadataFunctionGroup(string name, IList<EdmFunction> functionMetadata)
		: base(MetadataMemberClass.FunctionGroup, name)
	{
		FunctionMetadata = functionMetadata;
	}
}
