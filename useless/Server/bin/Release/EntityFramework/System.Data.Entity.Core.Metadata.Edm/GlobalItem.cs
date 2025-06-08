namespace System.Data.Entity.Core.Metadata.Edm;

public abstract class GlobalItem : MetadataItem
{
	[MetadataProperty(typeof(DataSpace), false)]
	internal virtual DataSpace DataSpace
	{
		get
		{
			return GetDataSpace();
		}
		set
		{
			SetDataSpace(value);
		}
	}

	internal GlobalItem()
	{
	}

	internal GlobalItem(MetadataFlags flags)
		: base(flags)
	{
	}
}
