using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;

namespace System.Data.Entity.Core.Metadata.Edm;

internal sealed class ClrPerspective : Perspective
{
	private EntityContainer _defaultContainer;

	internal ClrPerspective(MetadataWorkspace metadataWorkspace)
		: base(metadataWorkspace, DataSpace.CSpace)
	{
	}

	internal bool TryGetType(Type clrType, out TypeUsage outTypeUsage)
	{
		return TryGetTypeByName(clrType.FullNameWithNesting(), ignoreCase: false, out outTypeUsage);
	}

	internal override bool TryGetMember(StructuralType type, string memberName, bool ignoreCase, out EdmMember outMember)
	{
		outMember = null;
		MappingBase map = null;
		if (base.MetadataWorkspace.TryGetMap(type, DataSpace.OCSpace, out map) && map is ObjectTypeMapping objectTypeMapping)
		{
			ObjectMemberMapping memberMapForClrMember = objectTypeMapping.GetMemberMapForClrMember(memberName, ignoreCase);
			if (memberMapForClrMember != null)
			{
				outMember = memberMapForClrMember.EdmMember;
				return true;
			}
		}
		return false;
	}

	internal override bool TryGetTypeByName(string fullName, bool ignoreCase, out TypeUsage typeUsage)
	{
		typeUsage = null;
		MappingBase map = null;
		if (base.MetadataWorkspace.TryGetMap(fullName, DataSpace.OSpace, ignoreCase, DataSpace.OCSpace, out map))
		{
			if (map.EdmItem.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType)
			{
				PrimitiveType mappedPrimitiveType = base.MetadataWorkspace.GetMappedPrimitiveType(((PrimitiveType)map.EdmItem).PrimitiveTypeKind, DataSpace.CSpace);
				if (mappedPrimitiveType != null)
				{
					typeUsage = EdmProviderManifest.Instance.GetCanonicalModelTypeUsage(mappedPrimitiveType.PrimitiveTypeKind);
				}
			}
			else
			{
				typeUsage = GetMappedTypeUsage(map);
			}
		}
		return typeUsage != null;
	}

	internal override EntityContainer GetDefaultContainer()
	{
		return _defaultContainer;
	}

	internal void SetDefaultContainer(string defaultContainerName)
	{
		EntityContainer entityContainer = null;
		if (!string.IsNullOrEmpty(defaultContainerName) && !base.MetadataWorkspace.TryGetEntityContainer(defaultContainerName, DataSpace.CSpace, out entityContainer))
		{
			throw new ArgumentException(Strings.ObjectContext_InvalidDefaultContainerName(defaultContainerName), "defaultContainerName");
		}
		_defaultContainer = entityContainer;
	}

	private static TypeUsage GetMappedTypeUsage(MappingBase map)
	{
		TypeUsage result = null;
		if (map != null)
		{
			MetadataItem edmItem = map.EdmItem;
			EdmType edmType = edmItem as EdmType;
			if (edmItem != null && edmType != null)
			{
				result = TypeUsage.Create(edmType);
			}
		}
		return result;
	}
}
