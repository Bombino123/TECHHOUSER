using System.Collections.Generic;

namespace System.Data.Entity.Core.Query.InternalTrees;

internal abstract class NestBaseOp : PhysicalOp
{
	private readonly List<SortKey> m_prefixSortKeys;

	private readonly VarVec m_outputs;

	private readonly List<CollectionInfo> m_collectionInfoList;

	internal List<SortKey> PrefixSortKeys => m_prefixSortKeys;

	internal VarVec Outputs => m_outputs;

	internal List<CollectionInfo> CollectionInfo => m_collectionInfoList;

	internal NestBaseOp(OpType opType, List<SortKey> prefixSortKeys, VarVec outputVars, List<CollectionInfo> collectionInfoList)
		: base(opType)
	{
		m_outputs = outputVars;
		m_collectionInfoList = collectionInfoList;
		m_prefixSortKeys = prefixSortKeys;
	}
}
