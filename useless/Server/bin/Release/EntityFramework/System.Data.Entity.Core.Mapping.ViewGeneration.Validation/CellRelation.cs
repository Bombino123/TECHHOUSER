using System.Data.Entity.Core.Common.Utils;

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation;

internal abstract class CellRelation : InternalBase
{
	internal int m_cellNumber;

	internal int CellNumber => m_cellNumber;

	protected CellRelation(int cellNumber)
	{
		m_cellNumber = cellNumber;
	}

	protected abstract int GetHash();
}
