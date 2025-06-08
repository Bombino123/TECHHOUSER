using System.Collections.Generic;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Common.EntitySql;

internal sealed class InlineFunctionGroup : MetadataMember
{
	internal readonly IList<InlineFunctionInfo> FunctionMetadata;

	internal override string MetadataMemberClassName => InlineFunctionGroupClassName;

	internal static string InlineFunctionGroupClassName => Strings.LocalizedInlineFunction;

	internal InlineFunctionGroup(string name, IList<InlineFunctionInfo> functionMetadata)
		: base(MetadataMemberClass.InlineFunctionGroup, name)
	{
		FunctionMetadata = functionMetadata;
	}
}
