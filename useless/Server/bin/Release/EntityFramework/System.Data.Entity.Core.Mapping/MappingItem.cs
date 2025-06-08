using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Mapping;

public abstract class MappingItem
{
	private bool _readOnly;

	private readonly List<MetadataProperty> _annotations = new List<MetadataProperty>();

	internal bool IsReadOnly => _readOnly;

	internal IList<MetadataProperty> Annotations => _annotations;

	internal virtual void SetReadOnly()
	{
		_annotations.TrimExcess();
		_readOnly = true;
	}

	internal void ThrowIfReadOnly()
	{
		if (IsReadOnly)
		{
			throw new InvalidOperationException(Strings.OperationOnReadOnlyItem);
		}
	}

	internal static void SetReadOnly(MappingItem item)
	{
		item?.SetReadOnly();
	}

	internal static void SetReadOnly(IEnumerable<MappingItem> items)
	{
		if (items == null)
		{
			return;
		}
		foreach (MappingItem item in items)
		{
			SetReadOnly(item);
		}
	}
}
