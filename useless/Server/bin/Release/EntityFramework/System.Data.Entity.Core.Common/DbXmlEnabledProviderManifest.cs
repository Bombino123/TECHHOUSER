using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Data.Entity.Resources;
using System.Xml;

namespace System.Data.Entity.Core.Common;

public abstract class DbXmlEnabledProviderManifest : DbProviderManifest
{
	private class EmptyItemCollection : ItemCollection
	{
		public EmptyItemCollection()
			: base(DataSpace.SSpace)
		{
		}
	}

	private string _namespaceName;

	private ReadOnlyCollection<PrimitiveType> _primitiveTypes;

	private readonly Dictionary<PrimitiveType, ReadOnlyCollection<FacetDescription>> _facetDescriptions = new Dictionary<PrimitiveType, ReadOnlyCollection<FacetDescription>>();

	private ReadOnlyCollection<EdmFunction> _functions;

	private readonly Dictionary<string, PrimitiveType> _storeTypeNameToEdmPrimitiveType = new Dictionary<string, PrimitiveType>();

	private readonly Dictionary<string, PrimitiveType> _storeTypeNameToStorePrimitiveType = new Dictionary<string, PrimitiveType>();

	public override string NamespaceName => _namespaceName;

	protected Dictionary<string, PrimitiveType> StoreTypeNameToEdmPrimitiveType => _storeTypeNameToEdmPrimitiveType;

	protected Dictionary<string, PrimitiveType> StoreTypeNameToStorePrimitiveType => _storeTypeNameToStorePrimitiveType;

	protected DbXmlEnabledProviderManifest(XmlReader reader)
	{
		if (reader == null)
		{
			throw new ProviderIncompatibleException(Strings.IncorrectProviderManifest, new ArgumentNullException("reader"));
		}
		Load(reader);
	}

	public override ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType edmType)
	{
		return GetReadOnlyCollection(edmType as PrimitiveType, _facetDescriptions, Helper.EmptyFacetDescriptionEnumerable);
	}

	public override ReadOnlyCollection<PrimitiveType> GetStoreTypes()
	{
		return _primitiveTypes;
	}

	public override ReadOnlyCollection<EdmFunction> GetStoreFunctions()
	{
		return _functions;
	}

	private void Load(XmlReader reader)
	{
		Schema schema;
		IList<EdmSchemaError> list = SchemaManager.LoadProviderManifest(reader, (reader.BaseURI.Length > 0) ? reader.BaseURI : null, checkForSystemNamespace: true, out schema);
		if (list.Count != 0)
		{
			throw new ProviderIncompatibleException(Strings.IncorrectProviderManifest + Helper.CombineErrorMessage(list));
		}
		_namespaceName = schema.Namespace;
		List<PrimitiveType> list2 = new List<PrimitiveType>();
		foreach (System.Data.Entity.Core.SchemaObjectModel.SchemaType schemaType in schema.SchemaTypes)
		{
			if (schemaType is TypeElement { PrimitiveType: var primitiveType } typeElement)
			{
				primitiveType.ProviderManifest = this;
				primitiveType.DataSpace = DataSpace.SSpace;
				primitiveType.SetReadOnly();
				list2.Add(primitiveType);
				_storeTypeNameToStorePrimitiveType.Add(primitiveType.Name.ToLowerInvariant(), primitiveType);
				_storeTypeNameToEdmPrimitiveType.Add(primitiveType.Name.ToLowerInvariant(), EdmProviderManifest.Instance.GetPrimitiveType(primitiveType.PrimitiveTypeKind));
				if (EnumerableToReadOnlyCollection(typeElement.FacetDescriptions, out ReadOnlyCollection<FacetDescription> collection))
				{
					_facetDescriptions.Add(primitiveType, collection);
				}
			}
		}
		_primitiveTypes = new ReadOnlyCollection<PrimitiveType>(list2.ToArray());
		ItemCollection itemCollection = new EmptyItemCollection();
		if (!EnumerableToReadOnlyCollection(Converter.ConvertSchema(schema, this, itemCollection), out _functions))
		{
			_functions = Helper.EmptyEdmFunctionReadOnlyCollection;
		}
		foreach (EdmFunction function in _functions)
		{
			function.SetReadOnly();
		}
	}

	private static ReadOnlyCollection<T> GetReadOnlyCollection<T>(PrimitiveType type, Dictionary<PrimitiveType, ReadOnlyCollection<T>> typeDictionary, ReadOnlyCollection<T> useIfEmpty)
	{
		if (typeDictionary.TryGetValue(type, out var value))
		{
			return value;
		}
		return useIfEmpty;
	}

	private static bool EnumerableToReadOnlyCollection<Target, BaseType>(IEnumerable<BaseType> enumerable, out ReadOnlyCollection<Target> collection) where Target : BaseType
	{
		List<Target> list = new List<Target>();
		foreach (BaseType item in enumerable)
		{
			if (typeof(Target) == typeof(BaseType) || item is Target)
			{
				list.Add((Target)(object)item);
			}
		}
		if (list.Count != 0)
		{
			collection = new ReadOnlyCollection<Target>(list);
			return true;
		}
		collection = null;
		return false;
	}
}
