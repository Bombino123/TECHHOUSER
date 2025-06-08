using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Mapping.Update.Internal;
using System.Data.Entity.Core.Mapping.ViewGeneration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.SchemaObjectModel;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml;

namespace System.Data.Entity.Core.Mapping;

public class StorageMappingItemCollection : MappingItemCollection
{
	internal delegate bool TryGetUserDefinedQueryView(EntitySetBase extent, out GeneratedView generatedView);

	internal delegate bool TryGetUserDefinedQueryViewOfType(Pair<EntitySetBase, Pair<EntityTypeBase, bool>> extent, out GeneratedView generatedView);

	internal class ViewDictionary
	{
		private readonly TryGetUserDefinedQueryView _tryGetUserDefinedQueryView;

		private readonly TryGetUserDefinedQueryViewOfType _tryGetUserDefinedQueryViewOfType;

		private readonly StorageMappingItemCollection _storageMappingItemCollection;

		private static readonly ConfigViewGenerator _config = new ConfigViewGenerator();

		private bool _generatedViewsMode = true;

		private readonly Memoizer<System.Data.Entity.Core.Metadata.Edm.EntityContainer, Dictionary<EntitySetBase, GeneratedView>> _generatedViewsMemoizer;

		private readonly Memoizer<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, GeneratedView> _generatedViewOfTypeMemoizer;

		internal ViewDictionary(StorageMappingItemCollection storageMappingItemCollection, out Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict, out Dictionary<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, GeneratedView> userDefinedQueryViewsOfTypeDict)
		{
			_storageMappingItemCollection = storageMappingItemCollection;
			_generatedViewsMemoizer = new Memoizer<System.Data.Entity.Core.Metadata.Edm.EntityContainer, Dictionary<EntitySetBase, GeneratedView>>(SerializedGetGeneratedViews, null);
			_generatedViewOfTypeMemoizer = new Memoizer<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, GeneratedView>(SerializedGeneratedViewOfType, Pair<EntitySetBase, Pair<EntityTypeBase, bool>>.PairComparer.Instance);
			userDefinedQueryViewsDict = new Dictionary<EntitySetBase, GeneratedView>(EqualityComparer<EntitySetBase>.Default);
			userDefinedQueryViewsOfTypeDict = new Dictionary<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, GeneratedView>(Pair<EntitySetBase, Pair<EntityTypeBase, bool>>.PairComparer.Instance);
			_tryGetUserDefinedQueryView = userDefinedQueryViewsDict.TryGetValue;
			_tryGetUserDefinedQueryViewOfType = userDefinedQueryViewsOfTypeDict.TryGetValue;
		}

		private Dictionary<EntitySetBase, GeneratedView> SerializedGetGeneratedViews(System.Data.Entity.Core.Metadata.Edm.EntityContainer container)
		{
			EntityContainerMapping entityContainerMap = MappingMetadataHelper.GetEntityContainerMap(_storageMappingItemCollection, container);
			System.Data.Entity.Core.Metadata.Edm.EntityContainer arg = ((container.DataSpace == DataSpace.CSpace) ? entityContainerMap.StorageEntityContainer : entityContainerMap.EdmEntityContainer);
			if (_generatedViewsMemoizer.TryGetValue(arg, out var value))
			{
				return value;
			}
			value = new Dictionary<EntitySetBase, GeneratedView>();
			if (!entityContainerMap.HasViews)
			{
				return value;
			}
			if (_generatedViewsMode && _storageMappingItemCollection.MappingViewCacheFactory != null)
			{
				SerializedCollectViewsFromCache(entityContainerMap, value);
			}
			if (value.Count == 0)
			{
				_generatedViewsMode = false;
				SerializedGenerateViews(entityContainerMap, value);
			}
			return value;
		}

		private static void SerializedGenerateViews(EntityContainerMapping entityContainerMap, Dictionary<EntitySetBase, GeneratedView> resultDictionary)
		{
			ViewGenResults viewGenResults = ViewgenGatekeeper.GenerateViewsFromMapping(entityContainerMap, _config);
			KeyToListMap<EntitySetBase, GeneratedView> views = viewGenResults.Views;
			if (viewGenResults.HasErrors)
			{
				throw new MappingException(Helper.CombineErrorMessage(viewGenResults.Errors));
			}
			foreach (KeyValuePair<EntitySetBase, List<GeneratedView>> keyValuePair in views.KeyValuePairs)
			{
				if (!resultDictionary.TryGetValue(keyValuePair.Key, out var value))
				{
					value = keyValuePair.Value[0];
					resultDictionary.Add(keyValuePair.Key, value);
				}
			}
		}

		private bool TryGenerateQueryViewOfType(System.Data.Entity.Core.Metadata.Edm.EntityContainer entityContainer, EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
		{
			if (type.Abstract)
			{
				generatedView = null;
				return false;
			}
			bool success;
			ViewGenResults viewGenResults = ViewgenGatekeeper.GenerateTypeSpecificQueryView(MappingMetadataHelper.GetEntityContainerMap(_storageMappingItemCollection, entityContainer), _config, entity, type, includeSubtypes, out success);
			if (!success)
			{
				generatedView = null;
				return false;
			}
			KeyToListMap<EntitySetBase, GeneratedView> views = viewGenResults.Views;
			if (viewGenResults.HasErrors)
			{
				throw new MappingException(Helper.CombineErrorMessage(viewGenResults.Errors));
			}
			generatedView = views.AllValues.First();
			return true;
		}

		internal bool TryGetGeneratedViewOfType(EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
		{
			Pair<EntitySetBase, Pair<EntityTypeBase, bool>> arg = new Pair<EntitySetBase, Pair<EntityTypeBase, bool>>(entity, new Pair<EntityTypeBase, bool>(type, includeSubtypes));
			generatedView = _generatedViewOfTypeMemoizer.Evaluate(arg);
			return generatedView != null;
		}

		private GeneratedView SerializedGeneratedViewOfType(Pair<EntitySetBase, Pair<EntityTypeBase, bool>> arg)
		{
			if (_tryGetUserDefinedQueryViewOfType(arg, out var generatedView))
			{
				return generatedView;
			}
			EntitySetBase first = arg.First;
			EntityTypeBase first2 = arg.Second.First;
			bool second = arg.Second.Second;
			if (!TryGenerateQueryViewOfType(first.EntityContainer, first, first2, second, out generatedView))
			{
				return null;
			}
			return generatedView;
		}

		internal GeneratedView GetGeneratedView(EntitySetBase extent, MetadataWorkspace workspace, StorageMappingItemCollection storageMappingItemCollection)
		{
			if (_tryGetUserDefinedQueryView(extent, out var generatedView))
			{
				return generatedView;
			}
			if (extent.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
			{
				AssociationSet aSet = (AssociationSet)extent;
				if (aSet.ElementType.IsForeignKey)
				{
					if (_config.IsViewTracing)
					{
						Helpers.StringTraceLine(string.Empty);
						Helpers.StringTraceLine(string.Empty);
						Helpers.FormatTraceLine("================= Generating FK Query View for: {0} =================", aSet.Name);
						Helpers.StringTraceLine(string.Empty);
						Helpers.StringTraceLine(string.Empty);
					}
					System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint rc = aSet.ElementType.ReferentialConstraints.Single();
					EntitySet dependentSet = aSet.AssociationSetEnds[rc.ToRole.Name].EntitySet;
					EntitySet principalSet = aSet.AssociationSetEnds[rc.FromRole.Name].EntitySet;
					DbExpression dbExpression = dependentSet.Scan();
					EntityType dependentType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)rc.ToRole);
					EntityType principalType = MetadataHelper.GetEntityTypeForEnd((AssociationEndMember)rc.FromRole);
					if (dependentSet.ElementType.IsBaseTypeOf(dependentType))
					{
						dbExpression = dbExpression.OfType(TypeUsage.Create(dependentType));
					}
					if (rc.FromRole.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)
					{
						dbExpression = dbExpression.Where(delegate(DbExpression e)
						{
							DbExpression dbExpression2 = null;
							foreach (EdmProperty toProperty in rc.ToProperties)
							{
								DbExpression dbExpression3 = e.Property(toProperty).IsNull().Not();
								dbExpression2 = ((dbExpression2 == null) ? dbExpression3 : dbExpression2.And(dbExpression3));
							}
							return dbExpression2;
						});
					}
					dbExpression = dbExpression.Select(delegate(DbExpression e)
					{
						List<DbExpression> list = new List<DbExpression>();
						foreach (AssociationEndMember associationEndMember in aSet.ElementType.AssociationEndMembers)
						{
							if (associationEndMember.Name == rc.ToRole.Name)
							{
								List<KeyValuePair<string, DbExpression>> list2 = new List<KeyValuePair<string, DbExpression>>();
								foreach (EdmMember keyMember in dependentSet.ElementType.KeyMembers)
								{
									list2.Add(e.Property((EdmProperty)keyMember));
								}
								list.Add(dependentSet.RefFromKey(DbExpressionBuilder.NewRow(list2), dependentType));
							}
							else
							{
								List<KeyValuePair<string, DbExpression>> list3 = new List<KeyValuePair<string, DbExpression>>();
								foreach (EdmMember keyMember2 in principalSet.ElementType.KeyMembers)
								{
									int index = rc.FromProperties.IndexOf((EdmProperty)keyMember2);
									list3.Add(e.Property(rc.ToProperties[index]));
								}
								list.Add(principalSet.RefFromKey(DbExpressionBuilder.NewRow(list3), principalType));
							}
						}
						return TypeUsage.Create(aSet.ElementType).New(list);
					});
					return GeneratedView.CreateGeneratedViewForFKAssociationSet(aSet, aSet.ElementType, new DbQueryCommandTree(workspace, DataSpace.SSpace, dbExpression), storageMappingItemCollection, _config);
				}
			}
			if (!_generatedViewsMemoizer.Evaluate(extent.EntityContainer).TryGetValue(extent, out generatedView))
			{
				throw new InvalidOperationException(Strings.Mapping_Views_For_Extent_Not_Generated((extent.EntityContainer.DataSpace == DataSpace.SSpace) ? "Table" : "EntitySet", extent.Name));
			}
			return generatedView;
		}

		private void SerializedCollectViewsFromCache(EntityContainerMapping containerMapping, Dictionary<EntitySetBase, GeneratedView> extentMappingViews)
		{
			DbMappingViewCache dbMappingViewCache = _storageMappingItemCollection.MappingViewCacheFactory.Create(containerMapping);
			if (dbMappingViewCache == null)
			{
				return;
			}
			if (MetadataMappingHasherVisitor.GetMappingClosureHash(containerMapping.StorageMappingItemCollection.MappingVersion, containerMapping) != dbMappingViewCache.MappingHashValue)
			{
				throw new MappingException(Strings.ViewGen_HashOnMappingClosure_Not_Matching(dbMappingViewCache.GetType().Name));
			}
			foreach (EntitySetBase item in containerMapping.StorageEntityContainer.BaseEntitySets.Union(containerMapping.EdmEntityContainer.BaseEntitySets))
			{
				if (!extentMappingViews.TryGetValue(item, out var value))
				{
					DbMappingView view = dbMappingViewCache.GetView(item);
					if (view != null)
					{
						value = GeneratedView.CreateGeneratedView(item, null, null, view.EntitySql, _storageMappingItemCollection, new ConfigViewGenerator());
						extentMappingViews.Add(item, value);
					}
				}
			}
		}
	}

	internal enum InterestingMembersKind
	{
		RequiredOriginalValueMembers,
		FullUpdate,
		PartialUpdate
	}

	private EdmItemCollection _edmCollection;

	private StoreItemCollection _storeItemCollection;

	private ViewDictionary m_viewDictionary;

	private double m_mappingVersion;

	private MetadataWorkspace _workspace;

	private readonly Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>> m_memberMappings = new Dictionary<EdmMember, KeyValuePair<TypeUsage, TypeUsage>>();

	private ViewLoader _viewLoader;

	private readonly ConcurrentDictionary<Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>, ReadOnlyCollection<EdmMember>> _cachedInterestingMembers = new ConcurrentDictionary<Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>, ReadOnlyCollection<EdmMember>>();

	private DbMappingViewCacheFactory _mappingViewCacheFactory;

	public DbMappingViewCacheFactory MappingViewCacheFactory
	{
		get
		{
			return _mappingViewCacheFactory;
		}
		set
		{
			Check.NotNull(value, "value");
			Interlocked.CompareExchange(ref _mappingViewCacheFactory, value, null);
			if (!_mappingViewCacheFactory.Equals(value))
			{
				throw new ArgumentException(Strings.MappingViewCacheFactory_MustNotChange, "value");
			}
		}
	}

	internal MetadataWorkspace Workspace
	{
		get
		{
			if (_workspace == null)
			{
				_workspace = new MetadataWorkspace(() => _edmCollection, () => _storeItemCollection, () => this);
			}
			return _workspace;
		}
	}

	internal EdmItemCollection EdmItemCollection => _edmCollection;

	public double MappingVersion => m_mappingVersion;

	internal StoreItemCollection StoreItemCollection => _storeItemCollection;

	internal StorageMappingItemCollection()
		: base(DataSpace.CSSpace)
	{
	}

	public StorageMappingItemCollection(EdmItemCollection edmCollection, StoreItemCollection storeCollection, params string[] filePaths)
		: base(DataSpace.CSSpace)
	{
		Check.NotNull(edmCollection, "edmCollection");
		Check.NotNull(storeCollection, "storeCollection");
		Check.NotNull(filePaths, "filePaths");
		_edmCollection = edmCollection;
		_storeItemCollection = storeCollection;
		MetadataArtifactLoader metadataArtifactLoader = null;
		List<XmlReader> list = null;
		try
		{
			metadataArtifactLoader = MetadataArtifactLoader.CreateCompositeFromFilePaths(filePaths, ".msl");
			list = metadataArtifactLoader.CreateReaders(DataSpace.CSSpace);
			Init(edmCollection, storeCollection, list, metadataArtifactLoader.GetPaths(DataSpace.CSSpace), throwOnError: true);
		}
		finally
		{
			if (list != null)
			{
				Helper.DisposeXmlReaders(list);
			}
		}
	}

	public StorageMappingItemCollection(EdmItemCollection edmCollection, StoreItemCollection storeCollection, IEnumerable<XmlReader> xmlReaders)
		: base(DataSpace.CSSpace)
	{
		Check.NotNull(xmlReaders, "xmlReaders");
		MetadataArtifactLoader metadataArtifactLoader = MetadataArtifactLoader.CreateCompositeFromXmlReaders(xmlReaders);
		Init(edmCollection, storeCollection, metadataArtifactLoader.GetReaders(), metadataArtifactLoader.GetPaths(), throwOnError: true);
	}

	private StorageMappingItemCollection(EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection, IEnumerable<XmlReader> xmlReaders, IList<string> filePaths, out IList<EdmSchemaError> errors)
		: base(DataSpace.CSSpace)
	{
		errors = Init(edmItemCollection, storeItemCollection, xmlReaders, filePaths, throwOnError: false);
	}

	internal StorageMappingItemCollection(EdmItemCollection edmCollection, StoreItemCollection storeCollection, IEnumerable<XmlReader> xmlReaders, IList<string> filePaths)
		: base(DataSpace.CSSpace)
	{
		Init(edmCollection, storeCollection, xmlReaders, filePaths, throwOnError: true);
	}

	private IList<EdmSchemaError> Init(EdmItemCollection edmCollection, StoreItemCollection storeCollection, IEnumerable<XmlReader> xmlReaders, IList<string> filePaths, bool throwOnError)
	{
		_edmCollection = edmCollection;
		_storeItemCollection = storeCollection;
		m_viewDictionary = new ViewDictionary(this, out var userDefinedQueryViewsDict, out var userDefinedQueryViewsOfTypeDict);
		List<EdmSchemaError> list = new List<EdmSchemaError>();
		if (_edmCollection.EdmVersion != 0.0 && _storeItemCollection.StoreSchemaVersion != 0.0 && _edmCollection.EdmVersion != _storeItemCollection.StoreSchemaVersion)
		{
			list.Add(new EdmSchemaError(Strings.Mapping_DifferentEdmStoreVersion, 2102, EdmSchemaErrorSeverity.Error));
		}
		else
		{
			double expectedVersion = ((_edmCollection.EdmVersion != 0.0) ? _edmCollection.EdmVersion : _storeItemCollection.StoreSchemaVersion);
			list.AddRange(LoadItems(xmlReaders, filePaths, userDefinedQueryViewsDict, userDefinedQueryViewsOfTypeDict, expectedVersion));
		}
		if (list.Count > 0 && throwOnError && !MetadataHelper.CheckIfAllErrorsAreWarnings(list))
		{
			throw new MappingException(string.Format(CultureInfo.CurrentCulture, EntityRes.GetString("InvalidSchemaEncountered"), new object[1] { Helper.CombineErrorMessage(list) }));
		}
		return list;
	}

	internal override MappingBase GetMap(string identity, DataSpace typeSpace, bool ignoreCase)
	{
		if (typeSpace != DataSpace.CSpace)
		{
			throw new InvalidOperationException(Strings.Mapping_Storage_InvalidSpace(typeSpace));
		}
		return GetItem<MappingBase>(identity, ignoreCase);
	}

	internal override bool TryGetMap(string identity, DataSpace typeSpace, bool ignoreCase, out MappingBase map)
	{
		if (typeSpace != DataSpace.CSpace)
		{
			throw new InvalidOperationException(Strings.Mapping_Storage_InvalidSpace(typeSpace));
		}
		return TryGetItem<MappingBase>(identity, ignoreCase, out map);
	}

	internal override MappingBase GetMap(string identity, DataSpace typeSpace)
	{
		return GetMap(identity, typeSpace, ignoreCase: false);
	}

	internal override bool TryGetMap(string identity, DataSpace typeSpace, out MappingBase map)
	{
		return TryGetMap(identity, typeSpace, ignoreCase: false, out map);
	}

	internal override MappingBase GetMap(GlobalItem item)
	{
		DataSpace dataSpace = item.DataSpace;
		if (dataSpace != DataSpace.CSpace)
		{
			throw new InvalidOperationException(Strings.Mapping_Storage_InvalidSpace(dataSpace));
		}
		return GetMap(item.Identity, dataSpace);
	}

	internal override bool TryGetMap(GlobalItem item, out MappingBase map)
	{
		if (item == null)
		{
			map = null;
			return false;
		}
		DataSpace dataSpace = item.DataSpace;
		if (dataSpace != DataSpace.CSpace)
		{
			map = null;
			return false;
		}
		return TryGetMap(item.Identity, dataSpace, out map);
	}

	internal ReadOnlyCollection<EdmMember> GetInterestingMembers(EntitySetBase entitySet, EntityTypeBase entityType, InterestingMembersKind interestingMembersKind)
	{
		Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind> key = new Tuple<EntitySetBase, EntityTypeBase, InterestingMembersKind>(entitySet, entityType, interestingMembersKind);
		return _cachedInterestingMembers.GetOrAdd(key, FindInterestingMembers(entitySet, entityType, interestingMembersKind));
	}

	private ReadOnlyCollection<EdmMember> FindInterestingMembers(EntitySetBase entitySet, EntityTypeBase entityType, InterestingMembersKind interestingMembersKind)
	{
		List<EdmMember> interestingMembers = new List<EdmMember>();
		foreach (TypeMapping mappingsForEntitySetAndSuperType in MappingMetadataHelper.GetMappingsForEntitySetAndSuperTypes(this, entitySet.EntityContainer, entitySet, entityType))
		{
			if (mappingsForEntitySetAndSuperType is AssociationTypeMapping associationTypeMapping)
			{
				FindInterestingAssociationMappingMembers(associationTypeMapping, interestingMembers);
			}
			else
			{
				FindInterestingEntityMappingMembers((EntityTypeMapping)mappingsForEntitySetAndSuperType, interestingMembersKind, interestingMembers);
			}
		}
		if (interestingMembersKind != 0)
		{
			FindForeignKeyProperties(entitySet, entityType, interestingMembers);
		}
		foreach (EntityTypeModificationFunctionMapping item in from functionMappings in MappingMetadataHelper.GetModificationFunctionMappingsForEntitySetAndType(this, entitySet.EntityContainer, entitySet, entityType)
			where functionMappings.UpdateFunctionMapping != null
			select functionMappings)
		{
			FindInterestingFunctionMappingMembers(item, interestingMembersKind, ref interestingMembers);
		}
		return new ReadOnlyCollection<EdmMember>(interestingMembers.Distinct().ToList());
	}

	private static void FindInterestingAssociationMappingMembers(AssociationTypeMapping associationTypeMapping, List<EdmMember> interestingMembers)
	{
		interestingMembers.AddRange(from epm in associationTypeMapping.MappingFragments.SelectMany((MappingFragment m) => m.AllProperties).OfType<EndPropertyMapping>()
			select epm.AssociationEnd);
	}

	private static void FindInterestingEntityMappingMembers(EntityTypeMapping entityTypeMapping, InterestingMembersKind interestingMembersKind, List<EdmMember> interestingMembers)
	{
		foreach (PropertyMapping item in entityTypeMapping.MappingFragments.SelectMany((MappingFragment mf) => mf.AllProperties))
		{
			ScalarPropertyMapping scalarPropertyMapping = item as ScalarPropertyMapping;
			ComplexPropertyMapping complexPropertyMapping = item as ComplexPropertyMapping;
			ConditionPropertyMapping conditionPropertyMapping = item as ConditionPropertyMapping;
			if (scalarPropertyMapping != null && scalarPropertyMapping.Property != null)
			{
				if (MetadataHelper.IsPartOfEntityTypeKey(scalarPropertyMapping.Property))
				{
					if (interestingMembersKind == InterestingMembersKind.RequiredOriginalValueMembers)
					{
						interestingMembers.Add(scalarPropertyMapping.Property);
					}
				}
				else if (MetadataHelper.GetConcurrencyMode(scalarPropertyMapping.Property) == ConcurrencyMode.Fixed)
				{
					interestingMembers.Add(scalarPropertyMapping.Property);
				}
			}
			else if (complexPropertyMapping != null)
			{
				if (interestingMembersKind == InterestingMembersKind.PartialUpdate || MetadataHelper.GetConcurrencyMode(complexPropertyMapping.Property) == ConcurrencyMode.Fixed || HasFixedConcurrencyModeInAnyChildProperty(complexPropertyMapping))
				{
					interestingMembers.Add(complexPropertyMapping.Property);
				}
			}
			else if (conditionPropertyMapping != null && conditionPropertyMapping.Property != null)
			{
				interestingMembers.Add(conditionPropertyMapping.Property);
			}
		}
	}

	private static bool HasFixedConcurrencyModeInAnyChildProperty(ComplexPropertyMapping complexMapping)
	{
		foreach (PropertyMapping item in complexMapping.TypeMappings.SelectMany((ComplexTypeMapping m) => m.AllProperties))
		{
			ScalarPropertyMapping scalarPropertyMapping = item as ScalarPropertyMapping;
			ComplexPropertyMapping complexPropertyMapping = item as ComplexPropertyMapping;
			if (scalarPropertyMapping != null && MetadataHelper.GetConcurrencyMode(scalarPropertyMapping.Property) == ConcurrencyMode.Fixed)
			{
				return true;
			}
			if (complexPropertyMapping != null && (MetadataHelper.GetConcurrencyMode(complexPropertyMapping.Property) == ConcurrencyMode.Fixed || HasFixedConcurrencyModeInAnyChildProperty(complexPropertyMapping)))
			{
				return true;
			}
		}
		return false;
	}

	private static void FindForeignKeyProperties(EntitySetBase entitySetBase, EntityTypeBase entityType, List<EdmMember> interestingMembers)
	{
		EntitySet entitySet = entitySetBase as EntitySet;
		if (entitySet == null || !entitySet.HasForeignKeyRelationships)
		{
			return;
		}
		interestingMembers.AddRange(from p in MetadataHelper.GetTypeAndParentTypesOf(entityType, includeAbstractTypes: true).SelectMany((EdmType e) => ((EntityType)e).Properties)
			where entitySet.ForeignKeyDependents.SelectMany((Tuple<AssociationSet, System.Data.Entity.Core.Metadata.Edm.ReferentialConstraint> fk) => fk.Item2.ToProperties).Contains(p)
			select p);
	}

	private static void FindInterestingFunctionMappingMembers(EntityTypeModificationFunctionMapping functionMappings, InterestingMembersKind interestingMembersKind, ref List<EdmMember> interestingMembers)
	{
		if (interestingMembersKind == InterestingMembersKind.PartialUpdate)
		{
			interestingMembers.AddRange(functionMappings.UpdateFunctionMapping.ParameterBindings.Select((ModificationFunctionParameterBinding p) => p.MemberPath.Members.Last()));
			return;
		}
		foreach (ModificationFunctionParameterBinding item in functionMappings.UpdateFunctionMapping.ParameterBindings.Where((ModificationFunctionParameterBinding p) => !p.IsCurrent))
		{
			interestingMembers.Add(item.MemberPath.Members.Last());
		}
	}

	internal GeneratedView GetGeneratedView(EntitySetBase extent, MetadataWorkspace workspace)
	{
		return m_viewDictionary.GetGeneratedView(extent, workspace, this);
	}

	private void AddInternal(MappingBase storageMap)
	{
		storageMap.DataSpace = DataSpace.CSSpace;
		try
		{
			AddInternal((GlobalItem)storageMap);
		}
		catch (ArgumentException innerException)
		{
			throw new MappingException(Strings.Mapping_Duplicate_Type(storageMap.EdmItem.Identity), innerException);
		}
	}

	internal bool ContainsStorageEntityContainer(string storageEntityContainerName)
	{
		return GetItems<EntityContainerMapping>().Any((EntityContainerMapping map) => map.StorageEntityContainer.Name.Equals(storageEntityContainerName, StringComparison.Ordinal));
	}

	private List<EdmSchemaError> LoadItems(IEnumerable<XmlReader> xmlReaders, IList<string> mappingSchemaUris, Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict, Dictionary<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, GeneratedView> userDefinedQueryViewsOfTypeDict, double expectedVersion)
	{
		List<EdmSchemaError> list = new List<EdmSchemaError>();
		int num = -1;
		foreach (XmlReader xmlReader in xmlReaders)
		{
			num++;
			string location = null;
			if (mappingSchemaUris == null)
			{
				SchemaManager.TryGetBaseUri(xmlReader, out location);
			}
			else
			{
				location = mappingSchemaUris[num];
			}
			MappingItemLoader mappingItemLoader = new MappingItemLoader(xmlReader, this, location, m_memberMappings);
			list.AddRange(mappingItemLoader.ParsingErrors);
			CheckIsSameVersion(expectedVersion, mappingItemLoader.MappingVersion, list);
			EntityContainerMapping containerMapping = mappingItemLoader.ContainerMapping;
			if (mappingItemLoader.HasQueryViews && containerMapping != null)
			{
				CompileUserDefinedQueryViews(containerMapping, userDefinedQueryViewsDict, userDefinedQueryViewsOfTypeDict, list);
			}
			if (MetadataHelper.CheckIfAllErrorsAreWarnings(list) && !Contains(containerMapping))
			{
				containerMapping.SetReadOnly();
				AddInternal(containerMapping);
			}
		}
		CheckForDuplicateItems(EdmItemCollection, StoreItemCollection, list);
		return list;
	}

	private static void CompileUserDefinedQueryViews(EntityContainerMapping entityContainerMapping, Dictionary<EntitySetBase, GeneratedView> userDefinedQueryViewsDict, Dictionary<Pair<EntitySetBase, Pair<EntityTypeBase, bool>>, GeneratedView> userDefinedQueryViewsOfTypeDict, IList<EdmSchemaError> errors)
	{
		ConfigViewGenerator config = new ConfigViewGenerator();
		foreach (EntitySetBaseMapping allSetMap in entityContainerMapping.AllSetMaps)
		{
			if (allSetMap.QueryView == null || userDefinedQueryViewsDict.TryGetValue(allSetMap.Set, out var generatedView))
			{
				continue;
			}
			if (GeneratedView.TryParseUserSpecifiedView(allSetMap, allSetMap.Set.ElementType, allSetMap.QueryView, includeSubtypes: true, entityContainerMapping.StorageMappingItemCollection, config, errors, out generatedView))
			{
				userDefinedQueryViewsDict.Add(allSetMap.Set, generatedView);
			}
			foreach (Pair<EntitySetBase, Pair<EntityTypeBase, bool>> typeSpecificQVKey in allSetMap.GetTypeSpecificQVKeys())
			{
				if (GeneratedView.TryParseUserSpecifiedView(allSetMap, typeSpecificQVKey.Second.First, allSetMap.GetTypeSpecificQueryView(typeSpecificQVKey), typeSpecificQVKey.Second.Second, entityContainerMapping.StorageMappingItemCollection, config, errors, out generatedView))
				{
					userDefinedQueryViewsOfTypeDict.Add(typeSpecificQVKey, generatedView);
				}
			}
		}
	}

	private void CheckIsSameVersion(double expectedVersion, double currentLoaderVersion, IList<EdmSchemaError> errors)
	{
		if (m_mappingVersion == 0.0)
		{
			m_mappingVersion = currentLoaderVersion;
		}
		if (expectedVersion != 0.0 && currentLoaderVersion != 0.0 && currentLoaderVersion != expectedVersion)
		{
			errors.Add(new EdmSchemaError(Strings.Mapping_DifferentMappingEdmStoreVersion, 2101, EdmSchemaErrorSeverity.Error));
		}
		if (currentLoaderVersion != m_mappingVersion && currentLoaderVersion != 0.0)
		{
			errors.Add(new EdmSchemaError(Strings.CannotLoadDifferentVersionOfSchemaInTheSameItemCollection, 2100, EdmSchemaErrorSeverity.Error));
		}
	}

	internal ViewLoader GetUpdateViewLoader()
	{
		if (_viewLoader == null)
		{
			_viewLoader = new ViewLoader(this);
		}
		return _viewLoader;
	}

	internal bool TryGetGeneratedViewOfType(EntitySetBase entity, EntityTypeBase type, bool includeSubtypes, out GeneratedView generatedView)
	{
		return m_viewDictionary.TryGetGeneratedViewOfType(entity, type, includeSubtypes, out generatedView);
	}

	private static void CheckForDuplicateItems(EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection, List<EdmSchemaError> errorCollection)
	{
		foreach (GlobalItem item in edmItemCollection)
		{
			if (storeItemCollection.Contains(item.Identity))
			{
				errorCollection.Add(new EdmSchemaError(Strings.Mapping_ItemWithSameNameExistsBothInCSpaceAndSSpace(item.Identity), 2070, EdmSchemaErrorSeverity.Error));
			}
		}
	}

	public string ComputeMappingHashValue(string conceptualModelContainerName, string storeModelContainerName)
	{
		Check.NotEmpty(conceptualModelContainerName, "conceptualModelContainerName");
		Check.NotEmpty(storeModelContainerName, "storeModelContainerName");
		EntityContainerMapping entityContainerMapping = GetItems<EntityContainerMapping>().SingleOrDefault((EntityContainerMapping m) => m.EdmEntityContainer.Name == conceptualModelContainerName && m.StorageEntityContainer.Name == storeModelContainerName);
		if (entityContainerMapping == null)
		{
			throw new InvalidOperationException(Strings.HashCalcContainersNotFound(conceptualModelContainerName, storeModelContainerName));
		}
		return MetadataMappingHasherVisitor.GetMappingClosureHash(MappingVersion, entityContainerMapping);
	}

	public string ComputeMappingHashValue()
	{
		if (GetItems<EntityContainerMapping>().Count != 1)
		{
			throw new InvalidOperationException(Strings.HashCalcMultipleContainers);
		}
		return MetadataMappingHasherVisitor.GetMappingClosureHash(MappingVersion, GetItems<EntityContainerMapping>().Single());
	}

	public Dictionary<EntitySetBase, DbMappingView> GenerateViews(string conceptualModelContainerName, string storeModelContainerName, IList<EdmSchemaError> errors)
	{
		Check.NotEmpty(conceptualModelContainerName, "conceptualModelContainerName");
		Check.NotEmpty(storeModelContainerName, "storeModelContainerName");
		Check.NotNull(errors, "errors");
		return GenerateViews(GetItems<EntityContainerMapping>().SingleOrDefault((EntityContainerMapping m) => m.EdmEntityContainer.Name == conceptualModelContainerName && m.StorageEntityContainer.Name == storeModelContainerName) ?? throw new InvalidOperationException(Strings.ViewGenContainersNotFound(conceptualModelContainerName, storeModelContainerName)), errors);
	}

	public Dictionary<EntitySetBase, DbMappingView> GenerateViews(IList<EdmSchemaError> errors)
	{
		Check.NotNull(errors, "errors");
		if (GetItems<EntityContainerMapping>().Count != 1)
		{
			throw new InvalidOperationException(Strings.ViewGenMultipleContainers);
		}
		return GenerateViews(GetItems<EntityContainerMapping>().Single(), errors);
	}

	internal static Dictionary<EntitySetBase, DbMappingView> GenerateViews(EntityContainerMapping containerMapping, IList<EdmSchemaError> errors)
	{
		Dictionary<EntitySetBase, DbMappingView> dictionary = new Dictionary<EntitySetBase, DbMappingView>();
		if (!containerMapping.HasViews)
		{
			return dictionary;
		}
		if (!containerMapping.HasMappingFragments())
		{
			errors.Add(new EdmSchemaError(Strings.Mapping_AllQueryViewAtCompileTime(containerMapping.Identity), 2088, EdmSchemaErrorSeverity.Warning));
			return dictionary;
		}
		ViewGenResults viewGenResults = ViewgenGatekeeper.GenerateViewsFromMapping(containerMapping, new ConfigViewGenerator
		{
			GenerateEsql = true
		});
		if (viewGenResults.HasErrors)
		{
			viewGenResults.Errors.Each(delegate(EdmSchemaError e)
			{
				errors.Add(e);
			});
		}
		foreach (KeyValuePair<EntitySetBase, List<GeneratedView>> keyValuePair in viewGenResults.Views.KeyValuePairs)
		{
			dictionary.Add(keyValuePair.Key, new DbMappingView(keyValuePair.Value[0].eSQL));
		}
		return dictionary;
	}

	public static StorageMappingItemCollection Create(EdmItemCollection edmItemCollection, StoreItemCollection storeItemCollection, IEnumerable<XmlReader> xmlReaders, IList<string> filePaths, out IList<EdmSchemaError> errors)
	{
		Check.NotNull(edmItemCollection, "edmItemCollection");
		Check.NotNull(storeItemCollection, "storeItemCollection");
		Check.NotNull(xmlReaders, "xmlReaders");
		EntityUtil.CheckArgumentContainsNull(ref xmlReaders, "xmlReaders");
		StorageMappingItemCollection result = new StorageMappingItemCollection(edmItemCollection, storeItemCollection, xmlReaders, filePaths, out errors);
		if (errors == null || errors.Count <= 0)
		{
			return result;
		}
		return null;
	}
}
