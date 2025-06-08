using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

namespace System.Data.Entity.Core.Mapping;

internal struct OutputFromComputeCellGroups
{
	internal List<Cell> Cells;

	internal CqlIdentifiers Identifiers;

	internal List<Set<Cell>> CellGroups;

	internal List<ForeignConstraint> ForeignKeyConstraints;

	internal bool Success;
}
