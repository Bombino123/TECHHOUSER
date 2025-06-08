using System.Data.Entity.ModelConfiguration.Edm;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

internal class CodeFirstOSpaceLoader
{
	private readonly CodeFirstOSpaceTypeFactory _typeFactory;

	public CodeFirstOSpaceLoader(CodeFirstOSpaceTypeFactory typeFactory = null)
	{
		_typeFactory = typeFactory ?? new CodeFirstOSpaceTypeFactory();
	}

	public void LoadTypes(EdmItemCollection edmItemCollection, ObjectItemCollection objectItemCollection)
	{
		foreach (EdmType item in from t in edmItemCollection.OfType<EdmType>()
			where t.BuiltInTypeKind == BuiltInTypeKind.EntityType || t.BuiltInTypeKind == BuiltInTypeKind.EnumType || t.BuiltInTypeKind == BuiltInTypeKind.ComplexType
			select t)
		{
			Type clrType = item.GetClrType();
			if (clrType != null)
			{
				EdmType edmType = _typeFactory.TryCreateType(clrType, item);
				if (edmType != null)
				{
					_typeFactory.CspaceToOspace.Add(item, edmType);
				}
			}
		}
		_typeFactory.CreateRelationships(edmItemCollection);
		foreach (Action referenceResolution in _typeFactory.ReferenceResolutions)
		{
			referenceResolution();
		}
		foreach (EdmType value in _typeFactory.LoadedTypes.Values)
		{
			value.SetReadOnly();
		}
		objectItemCollection.AddLoadedTypes(_typeFactory.LoadedTypes);
		objectItemCollection.OSpaceTypesLoaded = true;
	}
}
