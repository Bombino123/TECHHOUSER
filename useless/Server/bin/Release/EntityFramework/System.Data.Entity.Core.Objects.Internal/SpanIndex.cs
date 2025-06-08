using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Objects.Internal;

internal sealed class SpanIndex
{
	private sealed class RowTypeEqualityComparer : IEqualityComparer<RowType>
	{
		internal static readonly RowTypeEqualityComparer Instance = new RowTypeEqualityComparer();

		private RowTypeEqualityComparer()
		{
		}

		public bool Equals(RowType x, RowType y)
		{
			if (x == null || y == null)
			{
				return false;
			}
			return x.EdmEquals(y);
		}

		public int GetHashCode(RowType obj)
		{
			return obj.Identity.GetHashCode();
		}
	}

	private Dictionary<RowType, Dictionary<int, AssociationEndMember>> _spanMap;

	private Dictionary<RowType, TypeUsage> _rowMap;

	internal void AddSpannedRowType(RowType spannedRowType, TypeUsage originalRowType)
	{
		if (_rowMap == null)
		{
			_rowMap = new Dictionary<RowType, TypeUsage>(RowTypeEqualityComparer.Instance);
		}
		_rowMap[spannedRowType] = originalRowType;
	}

	internal TypeUsage GetSpannedRowType(RowType spannedRowType)
	{
		if (_rowMap != null && _rowMap.TryGetValue(spannedRowType, out var value))
		{
			return value;
		}
		return null;
	}

	internal bool HasSpanMap(RowType spanRowType)
	{
		if (_spanMap == null)
		{
			return false;
		}
		return _spanMap.ContainsKey(spanRowType);
	}

	internal void AddSpanMap(RowType rowType, Dictionary<int, AssociationEndMember> columnMap)
	{
		if (_spanMap == null)
		{
			_spanMap = new Dictionary<RowType, Dictionary<int, AssociationEndMember>>(RowTypeEqualityComparer.Instance);
		}
		_spanMap[rowType] = columnMap;
	}

	internal Dictionary<int, AssociationEndMember> GetSpanMap(RowType rowType)
	{
		Dictionary<int, AssociationEndMember> value = null;
		if (_spanMap != null && _spanMap.TryGetValue(rowType, out value))
		{
			return value;
		}
		return null;
	}
}
