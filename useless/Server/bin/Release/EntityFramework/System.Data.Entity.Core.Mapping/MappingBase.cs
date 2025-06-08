using System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Mapping;

public abstract class MappingBase : GlobalItem
{
	internal abstract MetadataItem EdmItem { get; }

	internal MappingBase()
		: base(MetadataFlags.Readonly)
	{
	}

	internal MappingBase(MetadataFlags flags)
		: base(flags)
	{
	}
}
