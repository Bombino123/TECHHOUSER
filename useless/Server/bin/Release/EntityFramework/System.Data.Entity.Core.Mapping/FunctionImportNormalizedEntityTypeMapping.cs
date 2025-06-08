using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.Utils;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

internal sealed class FunctionImportNormalizedEntityTypeMapping
{
	internal readonly ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> ColumnConditions;

	internal readonly BitArray ImpliedEntityTypes;

	internal readonly BitArray ComplementImpliedEntityTypes;

	internal FunctionImportNormalizedEntityTypeMapping(FunctionImportStructuralTypeMappingKB parent, List<FunctionImportEntityTypeMappingCondition> columnConditions, BitArray impliedEntityTypes)
	{
		ColumnConditions = new ReadOnlyCollection<FunctionImportEntityTypeMappingCondition>(columnConditions.ToList());
		ImpliedEntityTypes = impliedEntityTypes;
		ComplementImpliedEntityTypes = new BitArray(ImpliedEntityTypes).Not();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Values={0}, Types={1}", new object[2]
		{
			StringUtil.ToCommaSeparatedString(ColumnConditions),
			StringUtil.ToCommaSeparatedString(ImpliedEntityTypes)
		});
	}
}
