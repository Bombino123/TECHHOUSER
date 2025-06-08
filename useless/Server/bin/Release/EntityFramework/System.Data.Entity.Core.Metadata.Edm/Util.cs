using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Diagnostics;

namespace System.Data.Entity.Core.Metadata.Edm;

internal static class Util
{
	internal static void ThrowIfReadOnly(MetadataItem item)
	{
		if (item.IsReadOnly)
		{
			throw new InvalidOperationException(Strings.OperationOnReadOnlyItem);
		}
	}

	[Conditional("DEBUG")]
	internal static void AssertItemHasIdentity(MetadataItem item, string argumentName)
	{
		Check.NotNull(item, argumentName);
	}

	internal static ObjectTypeMapping GetObjectMapping(EdmType type, MetadataWorkspace workspace)
	{
		if (workspace.TryGetItemCollection(DataSpace.CSpace, out var _))
		{
			return (ObjectTypeMapping)workspace.GetMap(type, DataSpace.OCSpace);
		}
		EdmType edmType;
		EdmType cdmType;
		if (type.DataSpace == DataSpace.CSpace)
		{
			edmType = ((!Helper.IsPrimitiveType(type)) ? workspace.GetItem<EdmType>(type.FullName, DataSpace.OSpace) : workspace.GetMappedPrimitiveType(((PrimitiveType)type).PrimitiveTypeKind, DataSpace.OSpace));
			cdmType = type;
		}
		else
		{
			edmType = type;
			cdmType = type;
		}
		if (!Helper.IsPrimitiveType(edmType) && !Helper.IsEntityType(edmType) && !Helper.IsComplexType(edmType))
		{
			throw new NotSupportedException(Strings.Materializer_UnsupportedType);
		}
		if (Helper.IsPrimitiveType(edmType))
		{
			return new ObjectTypeMapping(edmType, cdmType);
		}
		return DefaultObjectMappingItemCollection.LoadObjectMapping(cdmType, edmType, null);
	}
}
