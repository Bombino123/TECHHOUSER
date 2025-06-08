using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal abstract class Shaper
{
	internal abstract class ErrorHandlingValueReader<T>
	{
		private readonly Func<DbDataReader, int, T> getTypedValue;

		private readonly Func<DbDataReader, int, object> getUntypedValue;

		protected ErrorHandlingValueReader(Func<DbDataReader, int, T> typedValueAccessor, Func<DbDataReader, int, object> untypedValueAccessor)
		{
			getTypedValue = typedValueAccessor;
			getUntypedValue = untypedValueAccessor;
		}

		protected ErrorHandlingValueReader()
			: this((Func<DbDataReader, int, T>)GetTypedValueDefault, (Func<DbDataReader, int, object>)GetUntypedValueDefault)
		{
		}

		private static T GetTypedValueDefault(DbDataReader reader, int ordinal)
		{
			Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
			if (underlyingType != null && underlyingType.IsEnum())
			{
				return (T)GetGenericTypedValueDefaultMethod(underlyingType).Invoke(null, new object[2] { reader, ordinal });
			}
			bool isNullable;
			return (T)CodeGenEmitter.GetReaderMethod(typeof(T), out isNullable).Invoke(reader, new object[1] { ordinal });
		}

		public static MethodInfo GetGenericTypedValueDefaultMethod(Type underlyingType)
		{
			return typeof(ErrorHandlingValueReader<>).MakeGenericType(underlyingType).GetOnlyDeclaredMethod("GetTypedValueDefault");
		}

		private static object GetUntypedValueDefault(DbDataReader reader, int ordinal)
		{
			return reader.GetValue(ordinal);
		}

		internal T GetValue(DbDataReader reader, int ordinal)
		{
			if (reader.IsDBNull(ordinal))
			{
				try
				{
					return (T)(object)null;
				}
				catch (NullReferenceException)
				{
					throw CreateNullValueException();
				}
			}
			try
			{
				return getTypedValue(reader, ordinal);
			}
			catch (Exception e)
			{
				if (e.IsCatchableExceptionType())
				{
					Type type = getUntypedValue(reader, ordinal)?.GetType();
					if (!typeof(T).IsAssignableFrom(type))
					{
						throw CreateWrongTypeException(type);
					}
				}
				throw;
			}
		}

		protected abstract Exception CreateNullValueException();

		protected abstract Exception CreateWrongTypeException(Type resultType);
	}

	private class ColumnErrorHandlingValueReader<TColumn> : ErrorHandlingValueReader<TColumn>
	{
		internal ColumnErrorHandlingValueReader()
		{
		}

		internal ColumnErrorHandlingValueReader(Func<DbDataReader, int, TColumn> typedAccessor, Func<DbDataReader, int, object> untypedAccessor)
			: base(typedAccessor, untypedAccessor)
		{
		}

		protected override Exception CreateNullValueException()
		{
			return new InvalidOperationException(Strings.Materializer_NullReferenceCast(typeof(TColumn)));
		}

		protected override Exception CreateWrongTypeException(Type resultType)
		{
			return EntityUtil.ValueInvalidCast(resultType, typeof(TColumn));
		}
	}

	private class PropertyErrorHandlingValueReader<TProperty> : ErrorHandlingValueReader<TProperty>
	{
		private readonly string _propertyName;

		private readonly string _typeName;

		internal PropertyErrorHandlingValueReader(string propertyName, string typeName)
		{
			_propertyName = propertyName;
			_typeName = typeName;
		}

		internal PropertyErrorHandlingValueReader(string propertyName, string typeName, Func<DbDataReader, int, TProperty> typedAccessor, Func<DbDataReader, int, object> untypedAccessor)
			: base(typedAccessor, untypedAccessor)
		{
			_propertyName = propertyName;
			_typeName = typeName;
		}

		protected override Exception CreateNullValueException()
		{
			return new ConstraintException(Strings.Materializer_SetInvalidValue(Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty), _typeName, _propertyName, "null"));
		}

		protected override Exception CreateWrongTypeException(Type resultType)
		{
			return new InvalidOperationException(Strings.Materializer_SetInvalidValue(Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty), _typeName, _propertyName, resultType));
		}
	}

	private IList<IEntityWrapper> _materializedEntities;

	public readonly DbDataReader Reader;

	public readonly object[] State;

	public readonly ObjectContext Context;

	public readonly MetadataWorkspace Workspace;

	public readonly MergeOption MergeOption;

	protected readonly bool Streaming;

	private readonly Lazy<DbSpatialDataReader> _spatialReader;

	internal Shaper(DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption, int stateCount, bool streaming)
	{
		Reader = reader;
		MergeOption = mergeOption;
		State = new object[stateCount];
		Context = context;
		Workspace = workspace;
		_spatialReader = new Lazy<DbSpatialDataReader>(CreateSpatialDataReader);
		Streaming = streaming;
	}

	public TElement Discriminate<TElement>(object[] discriminatorValues, Func<object[], EntityType> discriminate, KeyValuePair<EntityType, Func<Shaper, TElement>>[] elementDelegates)
	{
		EntityType entityType = discriminate(discriminatorValues);
		Func<Shaper, TElement> func = null;
		for (int i = 0; i < elementDelegates.Length; i++)
		{
			KeyValuePair<EntityType, Func<Shaper, TElement>> keyValuePair = elementDelegates[i];
			if (keyValuePair.Key == entityType)
			{
				func = keyValuePair.Value;
			}
		}
		return func(this);
	}

	public IEntityWrapper HandleEntityNoTracking<TEntity>(IEntityWrapper wrappedEntity)
	{
		RegisterMaterializedEntityForEvent(wrappedEntity);
		return wrappedEntity;
	}

	public IEntityWrapper HandleEntity<TEntity>(IEntityWrapper wrappedEntity, EntityKey entityKey, EntitySet entitySet)
	{
		IEntityWrapper entityWrapper = wrappedEntity;
		if ((object)entityKey != null)
		{
			EntityEntry entityEntry = Context.ObjectStateManager.FindEntityEntry(entityKey);
			if (entityEntry != null && !entityEntry.IsKeyEntry)
			{
				UpdateEntry<TEntity>(wrappedEntity, entityEntry);
				entityWrapper = entityEntry.WrappedEntity;
			}
			else
			{
				RegisterMaterializedEntityForEvent(entityWrapper);
				if (entityEntry == null)
				{
					Context.ObjectStateManager.AddEntry(wrappedEntity, entityKey, entitySet, "HandleEntity", isAdded: false);
				}
				else
				{
					Context.ObjectStateManager.PromoteKeyEntry(entityEntry, wrappedEntity, replacingEntry: false, setIsLoaded: true, keyEntryInitialized: false);
				}
			}
		}
		return entityWrapper;
	}

	public IEntityWrapper HandleEntityAppendOnly<TEntity>(Func<Shaper, IEntityWrapper> constructEntityDelegate, EntityKey entityKey, EntitySet entitySet)
	{
		IEntityWrapper entityWrapper;
		if ((object)entityKey == null)
		{
			entityWrapper = constructEntityDelegate(this);
			RegisterMaterializedEntityForEvent(entityWrapper);
		}
		else
		{
			EntityEntry entityEntry = Context.ObjectStateManager.FindEntityEntry(entityKey);
			if (entityEntry != null && !entityEntry.IsKeyEntry)
			{
				if (typeof(TEntity) != entityEntry.WrappedEntity.IdentityType)
				{
					EntityKey entityKey2 = entityEntry.EntityKey;
					throw new NotSupportedException(Strings.Materializer_RecyclingEntity(TypeHelpers.GetFullName(entityKey2.EntityContainerName, entityKey2.EntitySetName), typeof(TEntity).FullName, entityEntry.WrappedEntity.IdentityType.FullName));
				}
				if (EntityState.Added == entityEntry.State)
				{
					throw new InvalidOperationException(Strings.Materializer_AddedEntityAlreadyExists(typeof(TEntity).FullName));
				}
				entityWrapper = entityEntry.WrappedEntity;
			}
			else
			{
				entityWrapper = constructEntityDelegate(this);
				RegisterMaterializedEntityForEvent(entityWrapper);
				if (entityEntry == null)
				{
					Context.ObjectStateManager.AddEntry(entityWrapper, entityKey, entitySet, "HandleEntity", isAdded: false);
				}
				else
				{
					Context.ObjectStateManager.PromoteKeyEntry(entityEntry, entityWrapper, replacingEntry: false, setIsLoaded: true, keyEntryInitialized: false);
				}
			}
		}
		return entityWrapper;
	}

	public IEntityWrapper HandleFullSpanCollection<TTargetEntity>(IEntityWrapper wrappedEntity, Coordinator<TTargetEntity> coordinator, AssociationEndMember targetMember)
	{
		if (wrappedEntity.Entity != null)
		{
			coordinator.RegisterCloseHandler(delegate(Shaper state, List<IEntityWrapper> spannedEntities)
			{
				FullSpanAction(wrappedEntity, spannedEntities, targetMember);
			});
		}
		return wrappedEntity;
	}

	public IEntityWrapper HandleFullSpanElement(IEntityWrapper wrappedSource, IEntityWrapper wrappedSpannedEntity, AssociationEndMember targetMember)
	{
		if (wrappedSource.Entity == null)
		{
			return wrappedSource;
		}
		List<IEntityWrapper> list = null;
		if (wrappedSpannedEntity.Entity != null)
		{
			list = new List<IEntityWrapper>(1);
			list.Add(wrappedSpannedEntity);
		}
		else
		{
			EntityKey entityKey = wrappedSource.EntityKey;
			CheckClearedEntryOnSpan(null, wrappedSource, entityKey, targetMember);
		}
		FullSpanAction(wrappedSource, list, targetMember);
		return wrappedSource;
	}

	public IEntityWrapper HandleRelationshipSpan(IEntityWrapper wrappedEntity, EntityKey targetKey, AssociationEndMember targetMember)
	{
		if (wrappedEntity.Entity == null)
		{
			return wrappedEntity;
		}
		EntityKey entityKey = wrappedEntity.EntityKey;
		AssociationEndMember otherAssociationEnd = MetadataHelper.GetOtherAssociationEnd(targetMember);
		CheckClearedEntryOnSpan(targetKey, wrappedEntity, entityKey, targetMember);
		RelatedEnd relatedEnd;
		if ((object)targetKey != null)
		{
			EntitySet endEntitySet;
			AssociationSet associationSet = Context.MetadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet((AssociationType)targetMember.DeclaringType, targetMember.Name, targetKey.EntitySetName, targetKey.EntityContainerName, out endEntitySet);
			ObjectStateManager objectStateManager = Context.ObjectStateManager;
			ILookup<EntityKey, RelationshipEntry> relationshipLookup = ObjectStateManager.GetRelationshipLookup(Context.ObjectStateManager, associationSet, otherAssociationEnd, entityKey);
			if (!ObjectStateManager.TryUpdateExistingRelationships(Context, MergeOption, associationSet, otherAssociationEnd, relationshipLookup, wrappedEntity, targetMember, targetKey, setIsLoaded: true, out var newEntryState))
			{
				EntityEntry entityEntry = objectStateManager.GetOrAddKeyEntry(targetKey, endEntitySet);
				bool flag = true;
				switch (otherAssociationEnd.RelationshipMultiplicity)
				{
				case RelationshipMultiplicity.ZeroOrOne:
				case RelationshipMultiplicity.One:
				{
					ILookup<EntityKey, RelationshipEntry> relationshipLookup2 = ObjectStateManager.GetRelationshipLookup(Context.ObjectStateManager, associationSet, targetMember, targetKey);
					flag = !ObjectStateManager.TryUpdateExistingRelationships(Context, MergeOption, associationSet, targetMember, relationshipLookup2, entityEntry.WrappedEntity, otherAssociationEnd, entityKey, setIsLoaded: true, out newEntryState);
					if (entityEntry.State == EntityState.Detached)
					{
						entityEntry = objectStateManager.AddKeyEntry(targetKey, endEntitySet);
					}
					break;
				}
				}
				if (flag)
				{
					if (entityEntry.IsKeyEntry || newEntryState == EntityState.Deleted)
					{
						RelationshipWrapper wrapper = new RelationshipWrapper(associationSet, otherAssociationEnd.Name, entityKey, targetMember.Name, targetKey);
						objectStateManager.AddNewRelation(wrapper, newEntryState);
					}
					else if (entityEntry.State != EntityState.Deleted)
					{
						ObjectStateManager.AddEntityToCollectionOrReference(MergeOption, wrappedEntity, otherAssociationEnd, entityEntry.WrappedEntity, targetMember, setIsLoaded: true, relationshipAlreadyExists: false, inKeyEntryPromotion: false);
					}
					else
					{
						RelationshipWrapper wrapper2 = new RelationshipWrapper(associationSet, otherAssociationEnd.Name, entityKey, targetMember.Name, targetKey);
						objectStateManager.AddNewRelation(wrapper2, EntityState.Deleted);
					}
				}
			}
		}
		else if (TryGetRelatedEnd(wrappedEntity, (AssociationType)targetMember.DeclaringType, otherAssociationEnd.Name, targetMember.Name, out relatedEnd))
		{
			SetIsLoadedForSpan(relatedEnd, forceToTrue: false);
		}
		return wrappedEntity;
	}

	private bool TryGetRelatedEnd(IEntityWrapper wrappedEntity, AssociationType associationType, string sourceEndName, string targetEndName, out RelatedEnd relatedEnd)
	{
		AssociationType oSpaceAssociationType = Workspace.MetadataOptimization.GetOSpaceAssociationType(associationType, () => Workspace.GetItemCollection(DataSpace.OSpace).GetItem<AssociationType>(associationType.FullName));
		AssociationEndMember associationEndMember = null;
		AssociationEndMember associationEndMember2 = null;
		foreach (AssociationEndMember associationEndMember3 in oSpaceAssociationType.AssociationEndMembers)
		{
			if (associationEndMember3.Name == sourceEndName)
			{
				associationEndMember = associationEndMember3;
			}
			else if (associationEndMember3.Name == targetEndName)
			{
				associationEndMember2 = associationEndMember3;
			}
		}
		if (associationEndMember != null && associationEndMember2 != null)
		{
			bool flag = false;
			EntitySet endEntitySet;
			if (wrappedEntity.EntityKey == null)
			{
				flag = true;
			}
			else if (Workspace.MetadataOptimization.FindCSpaceAssociationSet(associationType, sourceEndName, wrappedEntity.EntityKey.EntitySetName, wrappedEntity.EntityKey.EntityContainerName, out endEntitySet) != null)
			{
				flag = true;
			}
			if (flag)
			{
				relatedEnd = DelegateFactory.GetRelatedEnd(wrappedEntity.RelationshipManager, associationEndMember, associationEndMember2, null);
				return true;
			}
		}
		relatedEnd = null;
		return false;
	}

	private void SetIsLoadedForSpan(RelatedEnd relatedEnd, bool forceToTrue)
	{
		if (!forceToTrue)
		{
			forceToTrue = relatedEnd.IsEmpty();
			if (relatedEnd is EntityReference entityReference)
			{
				forceToTrue &= entityReference.EntityKey == null;
			}
		}
		if (forceToTrue || MergeOption == MergeOption.OverwriteChanges)
		{
			relatedEnd.IsLoaded = true;
		}
	}

	public IEntityWrapper HandleIEntityWithKey<TEntity>(IEntityWrapper wrappedEntity, EntitySet entitySet)
	{
		return HandleEntity<TEntity>(wrappedEntity, wrappedEntity.EntityKey, entitySet);
	}

	public bool SetColumnValue(int recordStateSlotNumber, int ordinal, object value)
	{
		((RecordState)State[recordStateSlotNumber]).SetColumnValue(ordinal, value);
		return true;
	}

	public bool SetEntityRecordInfo(int recordStateSlotNumber, EntityKey entityKey, EntitySet entitySet)
	{
		((RecordState)State[recordStateSlotNumber]).SetEntityRecordInfo(entityKey, entitySet);
		return true;
	}

	public bool SetState<T>(int ordinal, T value)
	{
		State[ordinal] = value;
		return true;
	}

	public T SetStatePassthrough<T>(int ordinal, T value)
	{
		State[ordinal] = value;
		return value;
	}

	public TProperty GetPropertyValueWithErrorHandling<TProperty>(int ordinal, string propertyName, string typeName)
	{
		return new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName).GetValue(Reader, ordinal);
	}

	public TColumn GetColumnValueWithErrorHandling<TColumn>(int ordinal)
	{
		return new ColumnErrorHandlingValueReader<TColumn>().GetValue(Reader, ordinal);
	}

	public HierarchyId GetHierarchyIdColumnValue(int ordinal)
	{
		return new HierarchyId(Reader.GetValue(ordinal).ToString());
	}

	protected virtual DbSpatialDataReader CreateSpatialDataReader()
	{
		return SpatialHelpers.CreateSpatialDataReader(Workspace, Reader);
	}

	public DbGeography GetGeographyColumnValue(int ordinal)
	{
		if (Streaming)
		{
			return _spatialReader.Value.GetGeography(ordinal);
		}
		return (DbGeography)Reader.GetValue(ordinal);
	}

	public DbGeometry GetGeometryColumnValue(int ordinal)
	{
		if (Streaming)
		{
			return _spatialReader.Value.GetGeometry(ordinal);
		}
		return (DbGeometry)Reader.GetValue(ordinal);
	}

	public TColumn GetSpatialColumnValueWithErrorHandling<TColumn>(int ordinal, PrimitiveTypeKind spatialTypeKind)
	{
		if (spatialTypeKind == PrimitiveTypeKind.Geography)
		{
			if (Streaming)
			{
				return new ColumnErrorHandlingValueReader<TColumn>((DbDataReader reader, int column) => (TColumn)(object)_spatialReader.Value.GetGeography(column), (DbDataReader reader, int column) => _spatialReader.Value.GetGeography(column)).GetValue(Reader, ordinal);
			}
			return new ColumnErrorHandlingValueReader<TColumn>((DbDataReader reader, int column) => (TColumn)Reader.GetValue(column), (DbDataReader reader, int column) => Reader.GetValue(column)).GetValue(Reader, ordinal);
		}
		if (Streaming)
		{
			return new ColumnErrorHandlingValueReader<TColumn>((DbDataReader reader, int column) => (TColumn)(object)_spatialReader.Value.GetGeometry(column), (DbDataReader reader, int column) => _spatialReader.Value.GetGeometry(column)).GetValue(Reader, ordinal);
		}
		return new ColumnErrorHandlingValueReader<TColumn>((DbDataReader reader, int column) => (TColumn)Reader.GetValue(column), (DbDataReader reader, int column) => Reader.GetValue(column)).GetValue(Reader, ordinal);
	}

	public TProperty GetSpatialPropertyValueWithErrorHandling<TProperty>(int ordinal, string propertyName, string typeName, PrimitiveTypeKind spatialTypeKind)
	{
		if (Helper.IsGeographicTypeKind(spatialTypeKind))
		{
			if (Streaming)
			{
				return new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName, (DbDataReader reader, int column) => (TProperty)(object)_spatialReader.Value.GetGeography(column), (DbDataReader reader, int column) => _spatialReader.Value.GetGeography(column)).GetValue(Reader, ordinal);
			}
			return new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName, (DbDataReader reader, int column) => (TProperty)Reader.GetValue(column), (DbDataReader reader, int column) => Reader.GetValue(column)).GetValue(Reader, ordinal);
		}
		if (Streaming)
		{
			return new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName, (DbDataReader reader, int column) => (TProperty)(object)_spatialReader.Value.GetGeometry(column), (DbDataReader reader, int column) => _spatialReader.Value.GetGeometry(column)).GetValue(Reader, ordinal);
		}
		return new PropertyErrorHandlingValueReader<TProperty>(propertyName, typeName, (DbDataReader reader, int column) => (TProperty)Reader.GetValue(column), (DbDataReader reader, int column) => Reader.GetValue(column)).GetValue(Reader, ordinal);
	}

	private void CheckClearedEntryOnSpan(object targetValue, IEntityWrapper wrappedSource, EntityKey sourceKey, AssociationEndMember targetMember)
	{
		if ((object)sourceKey != null && targetValue == null && (MergeOption == MergeOption.PreserveChanges || MergeOption == MergeOption.OverwriteChanges))
		{
			EdmType elementType = ((RefType)MetadataHelper.GetOtherAssociationEnd(targetMember).TypeUsage.EdmType).ElementType;
			if (!Context.Perspective.TryGetType(wrappedSource.IdentityType, out var outTypeUsage) || outTypeUsage.EdmType.EdmEquals(elementType) || TypeSemantics.IsSubTypeOf(outTypeUsage.EdmType, elementType))
			{
				CheckClearedEntryOnSpan(sourceKey, targetMember);
			}
		}
	}

	private void CheckClearedEntryOnSpan(EntityKey sourceKey, AssociationEndMember targetMember)
	{
		AssociationEndMember otherAssociationEnd = MetadataHelper.GetOtherAssociationEnd(targetMember);
		EntitySet endEntitySet;
		AssociationSet associationSet = Context.MetadataWorkspace.MetadataOptimization.FindCSpaceAssociationSet((AssociationType)otherAssociationEnd.DeclaringType, otherAssociationEnd.Name, sourceKey.EntitySetName, sourceKey.EntityContainerName, out endEntitySet);
		if (associationSet != null)
		{
			Context.ObjectStateManager.RemoveRelationships(MergeOption, associationSet, sourceKey, otherAssociationEnd);
		}
	}

	private void FullSpanAction<TTargetEntity>(IEntityWrapper wrappedSource, IList<TTargetEntity> spannedEntities, AssociationEndMember targetMember)
	{
		if (wrappedSource.Entity != null)
		{
			AssociationEndMember otherAssociationEnd = MetadataHelper.GetOtherAssociationEnd(targetMember);
			if (TryGetRelatedEnd(wrappedSource, (AssociationType)targetMember.DeclaringType, otherAssociationEnd.Name, targetMember.Name, out var relatedEnd))
			{
				int num = Context.ObjectStateManager.UpdateRelationships(Context, MergeOption, (AssociationSet)relatedEnd.RelationshipSet, otherAssociationEnd, wrappedSource, targetMember, (List<TTargetEntity>)spannedEntities, setIsLoaded: true);
				SetIsLoadedForSpan(relatedEnd, num > 0);
			}
		}
	}

	private void UpdateEntry<TEntity>(IEntityWrapper wrappedEntity, EntityEntry existingEntry)
	{
		Type typeFromHandle = typeof(TEntity);
		if (typeFromHandle != existingEntry.WrappedEntity.IdentityType)
		{
			EntityKey entityKey = existingEntry.EntityKey;
			throw new NotSupportedException(Strings.Materializer_RecyclingEntity(TypeHelpers.GetFullName(entityKey.EntityContainerName, entityKey.EntitySetName), typeFromHandle.FullName, existingEntry.WrappedEntity.IdentityType.FullName));
		}
		if (EntityState.Added == existingEntry.State)
		{
			throw new InvalidOperationException(Strings.Materializer_AddedEntityAlreadyExists(typeFromHandle.FullName));
		}
		if (MergeOption == MergeOption.AppendOnly)
		{
			return;
		}
		if (MergeOption.OverwriteChanges == MergeOption)
		{
			if (EntityState.Deleted == existingEntry.State)
			{
				existingEntry.RevertDelete();
			}
			existingEntry.UpdateCurrentValueRecord(wrappedEntity.Entity);
			Context.ObjectStateManager.ForgetEntryWithConceptualNull(existingEntry, resetAllKeys: true);
			existingEntry.AcceptChanges();
			Context.ObjectStateManager.FixupReferencesByForeignKeys(existingEntry, replaceAddedRefs: true);
		}
		else if (EntityState.Unchanged == existingEntry.State)
		{
			existingEntry.UpdateCurrentValueRecord(wrappedEntity.Entity);
			Context.ObjectStateManager.ForgetEntryWithConceptualNull(existingEntry, resetAllKeys: true);
			existingEntry.AcceptChanges();
			Context.ObjectStateManager.FixupReferencesByForeignKeys(existingEntry, replaceAddedRefs: true);
		}
		else if (Context.ContextOptions.UseLegacyPreserveChangesBehavior)
		{
			existingEntry.UpdateRecordWithoutSetModified(wrappedEntity.Entity, existingEntry.EditableOriginalValues);
		}
		else
		{
			existingEntry.UpdateRecordWithSetModified(wrappedEntity.Entity, existingEntry.EditableOriginalValues);
		}
	}

	public void RaiseMaterializedEvents()
	{
		if (_materializedEntities == null)
		{
			return;
		}
		foreach (IEntityWrapper materializedEntity in _materializedEntities)
		{
			Context.OnObjectMaterialized(materializedEntity.Entity);
		}
		_materializedEntities.Clear();
	}

	public void InitializeForOnMaterialize()
	{
		if (Context.OnMaterializedHasHandlers)
		{
			if (_materializedEntities == null)
			{
				_materializedEntities = new List<IEntityWrapper>();
			}
		}
		else if (_materializedEntities != null)
		{
			_materializedEntities = null;
		}
	}

	protected void RegisterMaterializedEntityForEvent(IEntityWrapper wrappedEntity)
	{
		if (_materializedEntities != null)
		{
			_materializedEntities.Add(wrappedEntity);
		}
	}
}
internal class Shaper<T> : Shaper
{
	private class SimpleEnumerator : IDbEnumerator<T>, IEnumerator<T>, IDisposable, IEnumerator, IDbAsyncEnumerator<T>, IDbAsyncEnumerator
	{
		private readonly Shaper<T> _shaper;

		public T Current => _shaper.RootCoordinator.Current;

		object IEnumerator.Current => _shaper.RootCoordinator.Current;

		object IDbAsyncEnumerator.Current => _shaper.RootCoordinator.Current;

		internal SimpleEnumerator(Shaper<T> shaper)
		{
			_shaper = shaper;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			_shaper.RootCoordinator.SetCurrentToDefault();
			_shaper.Finally();
		}

		public bool MoveNext()
		{
			if (!_shaper._isActive)
			{
				return false;
			}
			if (_shaper.StoreRead())
			{
				try
				{
					_shaper.StartMaterializingElement();
					_shaper.RootCoordinator.ReadNextElement(_shaper);
				}
				finally
				{
					_shaper.StopMaterializingElement();
				}
				return true;
			}
			Dispose();
			return false;
		}

		public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			if (!_shaper._isActive)
			{
				return false;
			}
			cancellationToken.ThrowIfCancellationRequested();
			if (await _shaper.StoreReadAsync(cancellationToken).WithCurrentCulture())
			{
				try
				{
					_shaper.StartMaterializingElement();
					_shaper.RootCoordinator.ReadNextElement(_shaper);
				}
				finally
				{
					_shaper.StopMaterializingElement();
				}
				return true;
			}
			Dispose();
			return false;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	private class RowNestedResultEnumerator : IDbEnumerator<Coordinator[]>, IEnumerator<Coordinator[]>, IDisposable, IEnumerator, IDbAsyncEnumerator<Coordinator[]>, IDbAsyncEnumerator
	{
		private readonly Shaper<T> _shaper;

		private readonly Coordinator[] _current;

		public Coordinator[] Current => _current;

		object IEnumerator.Current => _current;

		object IDbAsyncEnumerator.Current => _current;

		internal Coordinator<T> RootCoordinator => _shaper.RootCoordinator;

		internal RowNestedResultEnumerator(Shaper<T> shaper)
		{
			_shaper = shaper;
			_current = new Coordinator[_shaper.RootCoordinator.MaxDistanceToLeaf() + 1];
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			_shaper.Finally();
		}

		public bool MoveNext()
		{
			try
			{
				_shaper.StartMaterializingElement();
				if (!_shaper.StoreRead())
				{
					RootCoordinator.ResetCollection(_shaper);
					return false;
				}
				MaterializeRow();
			}
			finally
			{
				_shaper.StopMaterializingElement();
			}
			return true;
		}

		public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			try
			{
				_shaper.StartMaterializingElement();
				if (!(await _shaper.StoreReadAsync(cancellationToken).WithCurrentCulture()))
				{
					RootCoordinator.ResetCollection(_shaper);
					return false;
				}
				MaterializeRow();
			}
			finally
			{
				_shaper.StopMaterializingElement();
			}
			return true;
		}

		private void MaterializeRow()
		{
			Coordinator coordinator = _shaper.RootCoordinator;
			int i = 0;
			bool flag = false;
			for (; i < _current.Length; i++)
			{
				while (coordinator != null && !coordinator.CoordinatorFactory.HasData(_shaper))
				{
					coordinator = coordinator.Next;
				}
				if (coordinator == null)
				{
					break;
				}
				if (coordinator.HasNextElement(_shaper))
				{
					if (!flag && coordinator.Child != null)
					{
						coordinator.Child.ResetCollection(_shaper);
					}
					flag = true;
					coordinator.ReadNextElement(_shaper);
					_current[i] = coordinator;
				}
				else
				{
					_current[i] = null;
				}
				coordinator = coordinator.Child;
			}
			for (; i < _current.Length; i++)
			{
				_current[i] = null;
			}
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	private class ObjectQueryNestedEnumerator : IDbEnumerator<T>, IEnumerator<T>, IDisposable, IEnumerator, IDbAsyncEnumerator<T>, IDbAsyncEnumerator
	{
		private enum State
		{
			Start,
			Reading,
			NoRowsLastElementPending,
			NoRows
		}

		private readonly RowNestedResultEnumerator _rowEnumerator;

		private T _previousElement;

		private State _state;

		public T Current => _previousElement;

		object IEnumerator.Current => Current;

		object IDbAsyncEnumerator.Current => Current;

		internal ObjectQueryNestedEnumerator(RowNestedResultEnumerator rowEnumerator)
		{
			_rowEnumerator = rowEnumerator;
			_previousElement = default(T);
			_state = State.Start;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			_rowEnumerator.Dispose();
		}

		public bool MoveNext()
		{
			switch (_state)
			{
			case State.Start:
				if (TryReadToNextElement())
				{
					ReadElement();
				}
				else
				{
					_state = State.NoRows;
				}
				break;
			case State.Reading:
				ReadElement();
				break;
			case State.NoRowsLastElementPending:
				_state = State.NoRows;
				break;
			}
			if (_state == State.NoRows)
			{
				_previousElement = default(T);
				return false;
			}
			return true;
		}

		public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			switch (_state)
			{
			case State.Start:
				if (await TryReadToNextElementAsync(cancellationToken).WithCurrentCulture())
				{
					await ReadElementAsync(cancellationToken).WithCurrentCulture();
				}
				else
				{
					_state = State.NoRows;
				}
				break;
			case State.Reading:
				await ReadElementAsync(cancellationToken).WithCurrentCulture();
				break;
			case State.NoRowsLastElementPending:
				_state = State.NoRows;
				break;
			}
			bool result;
			if (_state == State.NoRows)
			{
				_previousElement = default(T);
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		private void ReadElement()
		{
			_previousElement = _rowEnumerator.RootCoordinator.Current;
			if (TryReadToNextElement())
			{
				_state = State.Reading;
			}
			else
			{
				_state = State.NoRowsLastElementPending;
			}
		}

		private async Task ReadElementAsync(CancellationToken cancellationToken)
		{
			_previousElement = _rowEnumerator.RootCoordinator.Current;
			if (await TryReadToNextElementAsync(cancellationToken).WithCurrentCulture())
			{
				_state = State.Reading;
			}
			else
			{
				_state = State.NoRowsLastElementPending;
			}
		}

		private bool TryReadToNextElement()
		{
			while (_rowEnumerator.MoveNext())
			{
				if (_rowEnumerator.Current[0] != null)
				{
					return true;
				}
			}
			return false;
		}

		private async Task<bool> TryReadToNextElementAsync(CancellationToken cancellationToken)
		{
			while (await _rowEnumerator.MoveNextAsync(cancellationToken).WithCurrentCulture())
			{
				if (_rowEnumerator.Current[0] != null)
				{
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			_rowEnumerator.Reset();
		}
	}

	private class RecordStateEnumerator : IDbEnumerator<RecordState>, IEnumerator<RecordState>, IDisposable, IEnumerator, IDbAsyncEnumerator<RecordState>, IDbAsyncEnumerator
	{
		private readonly RowNestedResultEnumerator _rowEnumerator;

		private RecordState _current;

		private int _depth;

		private bool _readerConsumed;

		public RecordState Current => _current;

		object IEnumerator.Current => _current;

		object IDbAsyncEnumerator.Current => _current;

		internal RecordStateEnumerator(RowNestedResultEnumerator rowEnumerator)
		{
			_rowEnumerator = rowEnumerator;
			_current = null;
			_depth = -1;
			_readerConsumed = false;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			_rowEnumerator.Dispose();
		}

		public bool MoveNext()
		{
			if (!_readerConsumed)
			{
				while (true)
				{
					if (-1 == _depth || _rowEnumerator.Current.Length == _depth)
					{
						if (!_rowEnumerator.MoveNext())
						{
							_current = null;
							_readerConsumed = true;
							break;
						}
						_depth = 0;
					}
					Coordinator coordinator = _rowEnumerator.Current[_depth];
					if (coordinator != null)
					{
						_current = ((Coordinator<RecordState>)coordinator).Current;
						_depth++;
						break;
					}
					_depth++;
				}
			}
			return !_readerConsumed;
		}

		public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
		{
			if (!_readerConsumed)
			{
				cancellationToken.ThrowIfCancellationRequested();
				while (true)
				{
					if (-1 == _depth || _rowEnumerator.Current.Length == _depth)
					{
						if (!(await _rowEnumerator.MoveNextAsync(cancellationToken).WithCurrentCulture()))
						{
							_current = null;
							_readerConsumed = true;
							break;
						}
						_depth = 0;
					}
					Coordinator coordinator = _rowEnumerator.Current[_depth];
					if (coordinator != null)
					{
						_current = ((Coordinator<RecordState>)coordinator).Current;
						_depth++;
						break;
					}
					_depth++;
				}
			}
			return !_readerConsumed;
		}

		public void Reset()
		{
			_rowEnumerator.Reset();
		}
	}

	private readonly bool _isObjectQuery;

	private bool _isActive;

	private IDbEnumerator<T> _rootEnumerator;

	private readonly bool _readerOwned;

	internal readonly Coordinator<T> RootCoordinator;

	internal bool DataWaiting { get; set; }

	internal IDbEnumerator<T> RootEnumerator
	{
		get
		{
			if (_rootEnumerator == null)
			{
				InitializeRecordStates(RootCoordinator.CoordinatorFactory);
				_rootEnumerator = GetEnumerator();
			}
			return _rootEnumerator;
		}
	}

	internal event EventHandler OnDone;

	internal Shaper(DbDataReader reader, ObjectContext context, MetadataWorkspace workspace, MergeOption mergeOption, int stateCount, CoordinatorFactory<T> rootCoordinatorFactory, bool readerOwned, bool streaming)
		: base(reader, context, workspace, mergeOption, stateCount, streaming)
	{
		RootCoordinator = (Coordinator<T>)rootCoordinatorFactory.CreateCoordinator(null, null);
		_isObjectQuery = !(typeof(T) == typeof(RecordState));
		_isActive = true;
		RootCoordinator.Initialize(this);
		_readerOwned = readerOwned;
	}

	private void InitializeRecordStates(CoordinatorFactory coordinatorFactory)
	{
		foreach (RecordStateFactory recordStateFactory in coordinatorFactory.RecordStateFactories)
		{
			State[recordStateFactory.StateSlotNumber] = recordStateFactory.Create(coordinatorFactory);
		}
		foreach (CoordinatorFactory nestedCoordinator in coordinatorFactory.NestedCoordinators)
		{
			InitializeRecordStates(nestedCoordinator);
		}
	}

	public virtual IDbEnumerator<T> GetEnumerator()
	{
		if (RootCoordinator.CoordinatorFactory.IsSimple)
		{
			return new SimpleEnumerator(this);
		}
		RowNestedResultEnumerator rowEnumerator = new RowNestedResultEnumerator(this);
		if (_isObjectQuery)
		{
			return new ObjectQueryNestedEnumerator(rowEnumerator);
		}
		return (IDbEnumerator<T>)new RecordStateEnumerator(rowEnumerator);
	}

	private void Finally()
	{
		if (!_isActive)
		{
			return;
		}
		_isActive = false;
		if (_readerOwned)
		{
			if (_isObjectQuery)
			{
				Reader.Dispose();
			}
			if (Context != null && Streaming)
			{
				Context.ReleaseConnection();
			}
		}
		if (this.OnDone != null)
		{
			this.OnDone(this, new EventArgs());
		}
	}

	private bool StoreRead()
	{
		try
		{
			return Reader.Read();
		}
		catch (Exception e)
		{
			HandleReaderException(e);
			throw;
		}
	}

	private async Task<bool> StoreReadAsync(CancellationToken cancellationToken)
	{
		try
		{
			return await Reader.ReadAsync(cancellationToken).WithCurrentCulture();
		}
		catch (Exception e)
		{
			HandleReaderException(e);
			throw;
		}
	}

	private void HandleReaderException(Exception e)
	{
		if (e.IsCatchableEntityExceptionType())
		{
			if (Reader.IsClosed)
			{
				throw new EntityCommandExecutionException(Strings.ADP_DataReaderClosed("Read"), e);
			}
			throw new EntityCommandExecutionException(Strings.EntityClient_StoreReaderFailed, e);
		}
	}

	private void StartMaterializingElement()
	{
		if (Context != null)
		{
			Context.InMaterialization = true;
			InitializeForOnMaterialize();
		}
	}

	private void StopMaterializingElement()
	{
		if (Context != null)
		{
			Context.InMaterialization = false;
			RaiseMaterializedEvents();
		}
	}
}
