using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Metadata.Edm;

public class EdmModel : MetadataItem
{
	private readonly List<AssociationType> _associationTypes = new List<AssociationType>();

	private readonly List<ComplexType> _complexTypes = new List<ComplexType>();

	private readonly List<EntityType> _entityTypes = new List<EntityType>();

	private readonly List<EnumType> _enumTypes = new List<EnumType>();

	private readonly List<EdmFunction> _functions = new List<EdmFunction>();

	private readonly EntityContainer _container;

	private double _schemaVersion;

	private DbProviderInfo _providerInfo;

	private DbProviderManifest _providerManifest;

	public override BuiltInTypeKind BuiltInTypeKind => BuiltInTypeKind.MetadataItem;

	internal override string Identity => "EdmModel" + Container.Identity;

	public DataSpace DataSpace => Container.DataSpace;

	public IEnumerable<AssociationType> AssociationTypes => _associationTypes;

	public IEnumerable<ComplexType> ComplexTypes => _complexTypes;

	public IEnumerable<EntityType> EntityTypes => _entityTypes;

	public IEnumerable<EnumType> EnumTypes => _enumTypes;

	public IEnumerable<EdmFunction> Functions => _functions;

	public EntityContainer Container => _container;

	internal double SchemaVersion
	{
		get
		{
			return _schemaVersion;
		}
		set
		{
			_schemaVersion = value;
		}
	}

	internal DbProviderInfo ProviderInfo
	{
		get
		{
			return _providerInfo;
		}
		private set
		{
			_providerInfo = value;
		}
	}

	internal DbProviderManifest ProviderManifest
	{
		get
		{
			return _providerManifest;
		}
		private set
		{
			_providerManifest = value;
		}
	}

	internal virtual IEnumerable<string> NamespaceNames => NamespaceItems.Select((EdmType t) => t.NamespaceName).Distinct();

	internal IEnumerable<EdmType> NamespaceItems => ((IEnumerable<EdmType>)_associationTypes).Concat((IEnumerable<EdmType>)_complexTypes).Concat(_entityTypes).Concat(_enumTypes)
		.Concat(_functions);

	public IEnumerable<GlobalItem> GlobalItems => ((IEnumerable<GlobalItem>)NamespaceItems).Concat((IEnumerable<GlobalItem>)Containers);

	internal virtual IEnumerable<EntityContainer> Containers
	{
		get
		{
			yield return Container;
		}
	}

	private EdmModel(EntityContainer entityContainer, double version = 3.0)
	{
		_container = entityContainer;
		SchemaVersion = version;
	}

	internal EdmModel(DataSpace dataSpace, double schemaVersion = 3.0)
	{
		if (dataSpace != DataSpace.CSpace && dataSpace != DataSpace.SSpace)
		{
			throw new ArgumentException(Strings.MetadataItem_InvalidDataSpace(dataSpace, typeof(EdmModel).Name), "dataSpace");
		}
		_container = new EntityContainer((dataSpace == DataSpace.CSpace) ? "CodeFirstContainer" : "CodeFirstDatabase", dataSpace);
		_schemaVersion = schemaVersion;
	}

	public void AddItem(AssociationType item)
	{
		Check.NotNull(item, "item");
		ValidateSpace(item);
		_associationTypes.Add(item);
	}

	public void AddItem(ComplexType item)
	{
		Check.NotNull(item, "item");
		ValidateSpace(item);
		_complexTypes.Add(item);
	}

	public void AddItem(EntityType item)
	{
		Check.NotNull(item, "item");
		ValidateSpace(item);
		_entityTypes.Add(item);
	}

	public void AddItem(EnumType item)
	{
		Check.NotNull(item, "item");
		ValidateSpace(item);
		_enumTypes.Add(item);
	}

	public void AddItem(EdmFunction item)
	{
		Check.NotNull(item, "item");
		ValidateSpace(item);
		_functions.Add(item);
	}

	public void RemoveItem(AssociationType item)
	{
		Check.NotNull(item, "item");
		_associationTypes.Remove(item);
	}

	public void RemoveItem(ComplexType item)
	{
		Check.NotNull(item, "item");
		_complexTypes.Remove(item);
	}

	public void RemoveItem(EntityType item)
	{
		Check.NotNull(item, "item");
		_entityTypes.Remove(item);
	}

	public void RemoveItem(EnumType item)
	{
		Check.NotNull(item, "item");
		_enumTypes.Remove(item);
	}

	public void RemoveItem(EdmFunction item)
	{
		Check.NotNull(item, "item");
		_functions.Remove(item);
	}

	internal virtual void Validate()
	{
		List<DataModelErrorEventArgs> validationErrors = new List<DataModelErrorEventArgs>();
		DataModelValidator dataModelValidator = new DataModelValidator();
		dataModelValidator.OnError += delegate(object _, DataModelErrorEventArgs e)
		{
			validationErrors.Add(e);
		};
		dataModelValidator.Validate(this, validateSyntax: true);
		if (validationErrors.Count > 0)
		{
			throw new ModelValidationException(validationErrors);
		}
	}

	private void ValidateSpace(EdmType item)
	{
		if (item.DataSpace != DataSpace)
		{
			throw new ArgumentException(Strings.EdmModel_AddItem_NonMatchingNamespace, "item");
		}
	}

	internal static EdmModel CreateStoreModel(DbProviderInfo providerInfo, DbProviderManifest providerManifest, double schemaVersion = 3.0)
	{
		return new EdmModel(DataSpace.SSpace, schemaVersion)
		{
			ProviderInfo = providerInfo,
			ProviderManifest = providerManifest
		};
	}

	internal static EdmModel CreateStoreModel(EntityContainer entityContainer, DbProviderInfo providerInfo, DbProviderManifest providerManifest, double schemaVersion = 3.0)
	{
		EdmModel edmModel = new EdmModel(entityContainer, schemaVersion);
		if (providerInfo != null)
		{
			edmModel.ProviderInfo = providerInfo;
		}
		if (providerManifest != null)
		{
			edmModel.ProviderManifest = providerManifest;
		}
		return edmModel;
	}

	internal static EdmModel CreateConceptualModel(double schemaVersion = 3.0)
	{
		return new EdmModel(DataSpace.CSpace, schemaVersion);
	}

	internal static EdmModel CreateConceptualModel(EntityContainer entityContainer, double schemaVersion = 3.0)
	{
		return new EdmModel(entityContainer, schemaVersion);
	}
}
