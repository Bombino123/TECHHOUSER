using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Internal.Validation;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data.Entity.Internal;

internal class InternalEntityEntry
{
	private readonly Type _entityType;

	private readonly InternalContext _internalContext;

	private readonly object _entity;

	private IEntityStateEntry _stateEntry;

	private EntityType _edmEntityType;

	public virtual object Entity => _entity;

	public virtual EntityState State
	{
		get
		{
			if (!IsDetached)
			{
				return _stateEntry.State;
			}
			return EntityState.Detached;
		}
		set
		{
			if (!IsDetached)
			{
				if (_stateEntry.State == EntityState.Modified && value == EntityState.Unchanged)
				{
					CurrentValues.SetValues(OriginalValues);
				}
				_stateEntry.ChangeState(value);
				return;
			}
			switch (value)
			{
			case EntityState.Added:
				_internalContext.Set(_entityType).InternalSet.Add(_entity);
				break;
			case EntityState.Unchanged:
				_internalContext.Set(_entityType).InternalSet.Attach(_entity);
				break;
			case EntityState.Deleted:
			case EntityState.Modified:
				_internalContext.Set(_entityType).InternalSet.Attach(_entity);
				_stateEntry = _internalContext.GetStateEntry(_entity);
				_stateEntry.ChangeState(value);
				break;
			}
		}
	}

	public virtual InternalPropertyValues CurrentValues
	{
		get
		{
			ValidateStateToGetValues("CurrentValues", EntityState.Deleted);
			return new DbDataRecordPropertyValues(_internalContext, _entityType, _stateEntry.CurrentValues, isEntity: true);
		}
	}

	public virtual InternalPropertyValues OriginalValues
	{
		get
		{
			ValidateStateToGetValues("OriginalValues", EntityState.Added);
			return new DbDataRecordPropertyValues(_internalContext, _entityType, _stateEntry.GetUpdatableOriginalValues(), isEntity: true);
		}
	}

	public virtual bool IsDetached
	{
		get
		{
			if (_stateEntry == null || _stateEntry.State == EntityState.Detached)
			{
				_stateEntry = _internalContext.GetStateEntry(_entity);
				if (_stateEntry == null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public virtual Type EntityType => _entityType;

	public virtual EntityType EdmEntityType
	{
		get
		{
			if (_edmEntityType == null)
			{
				MetadataWorkspace metadataWorkspace = _internalContext.ObjectContext.MetadataWorkspace;
				EntityType item = metadataWorkspace.GetItem<EntityType>(_entityType.FullNameWithNesting(), DataSpace.OSpace);
				_edmEntityType = (EntityType)metadataWorkspace.GetEdmSpaceType(item);
			}
			return _edmEntityType;
		}
	}

	public IEntityStateEntry ObjectStateEntry => _stateEntry;

	public InternalContext InternalContext => _internalContext;

	public InternalEntityEntry(InternalContext internalContext, IEntityStateEntry stateEntry)
	{
		_internalContext = internalContext;
		_stateEntry = stateEntry;
		_entity = stateEntry.Entity;
		_entityType = ObjectContextTypeCache.GetObjectType(_entity.GetType());
	}

	public InternalEntityEntry(InternalContext internalContext, object entity)
	{
		_internalContext = internalContext;
		_entity = entity;
		_entityType = ObjectContextTypeCache.GetObjectType(_entity.GetType());
		_stateEntry = _internalContext.GetStateEntry(entity);
		if (_stateEntry == null)
		{
			_internalContext.Set(_entityType).InternalSet.Initialize();
		}
	}

	public virtual InternalPropertyValues GetDatabaseValues()
	{
		ValidateStateToGetValues("GetDatabaseValues", EntityState.Added);
		DbDataRecord dbDataRecord = GetDatabaseValuesQuery().SingleOrDefault();
		if (dbDataRecord != null)
		{
			return new ClonedPropertyValues(OriginalValues, dbDataRecord);
		}
		return null;
	}

	public virtual async Task<InternalPropertyValues> GetDatabaseValuesAsync(CancellationToken cancellationToken)
	{
		ValidateStateToGetValues("GetDatabaseValuesAsync", EntityState.Added);
		cancellationToken.ThrowIfCancellationRequested();
		DbDataRecord dbDataRecord = await GetDatabaseValuesQuery().SingleOrDefaultAsync(cancellationToken).WithCurrentCulture();
		return (dbDataRecord == null) ? null : new ClonedPropertyValues(OriginalValues, dbDataRecord);
	}

	private ObjectQuery<DbDataRecord> GetDatabaseValuesQuery()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("SELECT ");
		AppendEntitySqlRow(stringBuilder, "X", OriginalValues);
		string text = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2]
		{
			DbHelpers.QuoteIdentifier(_stateEntry.EntitySet.EntityContainer.Name),
			DbHelpers.QuoteIdentifier(_stateEntry.EntitySet.Name)
		});
		string text2 = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2]
		{
			DbHelpers.QuoteIdentifier(EntityType.NestingNamespace()),
			DbHelpers.QuoteIdentifier(EntityType.Name)
		});
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " FROM (SELECT VALUE TREAT (Y AS {0}) FROM {1} AS Y) AS X WHERE ", new object[2] { text2, text });
		EntityKeyMember[] entityKeyValues = _stateEntry.EntityKey.EntityKeyValues;
		ObjectParameter[] array = new ObjectParameter[entityKeyValues.Length];
		for (int i = 0; i < entityKeyValues.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" AND ");
			}
			string text3 = string.Format(CultureInfo.InvariantCulture, "p{0}", new object[1] { i.ToString(CultureInfo.InvariantCulture) });
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "X.{0} = @{1}", new object[2]
			{
				DbHelpers.QuoteIdentifier(entityKeyValues[i].Key),
				text3
			});
			array[i] = new ObjectParameter(text3, entityKeyValues[i].Value);
		}
		return _internalContext.ObjectContext.CreateQuery<DbDataRecord>(stringBuilder.ToString(), array);
	}

	private void AppendEntitySqlRow(StringBuilder queryBuilder, string prefix, InternalPropertyValues templateValues)
	{
		bool flag = false;
		foreach (string propertyName in templateValues.PropertyNames)
		{
			if (flag)
			{
				queryBuilder.Append(", ");
			}
			else
			{
				flag = true;
			}
			string text = DbHelpers.QuoteIdentifier(propertyName);
			IPropertyValuesItem item = templateValues.GetItem(propertyName);
			if (item.IsComplex)
			{
				if (!(item.Value is InternalPropertyValues templateValues2))
				{
					throw Error.DbPropertyValues_CannotGetStoreValuesWhenComplexPropertyIsNull(propertyName, EntityType.Name);
				}
				queryBuilder.Append("ROW(");
				AppendEntitySqlRow(queryBuilder, string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[2] { prefix, text }), templateValues2);
				queryBuilder.AppendFormat(CultureInfo.InvariantCulture, ") AS {0}", new object[1] { text });
			}
			else
			{
				queryBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}.{1} ", new object[2] { prefix, text });
			}
		}
	}

	private void ValidateStateToGetValues(string method, EntityState invalidState)
	{
		ValidateNotDetachedAndInitializeRelatedEnd(method);
		if (State == invalidState)
		{
			throw Error.DbPropertyValues_CannotGetValuesForState(method, State);
		}
	}

	public virtual void Reload()
	{
		ValidateStateToGetValues("Reload", EntityState.Added);
		_internalContext.ObjectContext.Refresh(RefreshMode.StoreWins, Entity);
	}

	public virtual Task ReloadAsync(CancellationToken cancellationToken)
	{
		ValidateStateToGetValues("ReloadAsync", EntityState.Added);
		return _internalContext.ObjectContext.RefreshAsync(RefreshMode.StoreWins, Entity, cancellationToken);
	}

	public virtual InternalReferenceEntry Reference(string navigationProperty, Type requestedType = null)
	{
		return (InternalReferenceEntry)ValidateAndGetNavigationMetadata(navigationProperty, requestedType ?? typeof(object), requireCollection: false).CreateMemberEntry(this, null);
	}

	public virtual InternalCollectionEntry Collection(string navigationProperty, Type requestedType = null)
	{
		return (InternalCollectionEntry)ValidateAndGetNavigationMetadata(navigationProperty, requestedType ?? typeof(object), requireCollection: true).CreateMemberEntry(this, null);
	}

	public virtual InternalMemberEntry Member(string propertyName, Type requestedType = null)
	{
		requestedType = requestedType ?? typeof(object);
		IList<string> list = SplitName(propertyName);
		if (list.Count > 1)
		{
			return Property(null, propertyName, list, requestedType, requireComplex: false);
		}
		MemberEntryMetadata memberEntryMetadata = (MemberEntryMetadata)(((object)GetNavigationMetadata(propertyName)) ?? ((object)ValidateAndGetPropertyMetadata(propertyName, EntityType, requestedType)));
		if (memberEntryMetadata == null)
		{
			throw Error.DbEntityEntry_NotAProperty(propertyName, EntityType.Name);
		}
		if (memberEntryMetadata.MemberEntryType != MemberEntryType.CollectionNavigationProperty && !requestedType.IsAssignableFrom(memberEntryMetadata.MemberType))
		{
			throw Error.DbEntityEntry_WrongGenericForNavProp(propertyName, EntityType.Name, requestedType.Name, memberEntryMetadata.MemberType.Name);
		}
		return memberEntryMetadata.CreateMemberEntry(this, null);
	}

	public virtual InternalPropertyEntry Property(string property, Type requestedType = null, bool requireComplex = false)
	{
		return Property(null, property, requestedType ?? typeof(object), requireComplex);
	}

	public InternalPropertyEntry Property(InternalPropertyEntry parentProperty, string propertyName, Type requestedType, bool requireComplex)
	{
		return Property(parentProperty, propertyName, SplitName(propertyName), requestedType, requireComplex);
	}

	private InternalPropertyEntry Property(InternalPropertyEntry parentProperty, string propertyName, IList<string> properties, Type requestedType, bool requireComplex)
	{
		bool flag = properties.Count > 1;
		Type requestedType2 = (flag ? typeof(object) : requestedType);
		Type type = ((parentProperty != null) ? parentProperty.EntryMetadata.ElementType : EntityType);
		PropertyEntryMetadata propertyEntryMetadata = ValidateAndGetPropertyMetadata(properties[0], type, requestedType2);
		if (propertyEntryMetadata == null || ((flag || requireComplex) && !propertyEntryMetadata.IsComplex))
		{
			if (flag)
			{
				throw Error.DbEntityEntry_DottedPartNotComplex(properties[0], propertyName, type.Name);
			}
			throw requireComplex ? Error.DbEntityEntry_NotAComplexProperty(properties[0], type.Name) : Error.DbEntityEntry_NotAScalarProperty(properties[0], type.Name);
		}
		InternalPropertyEntry internalPropertyEntry = (InternalPropertyEntry)propertyEntryMetadata.CreateMemberEntry(this, parentProperty);
		if (!flag)
		{
			return internalPropertyEntry;
		}
		return Property(internalPropertyEntry, propertyName, properties.Skip(1).ToList(), requestedType, requireComplex);
	}

	private NavigationEntryMetadata ValidateAndGetNavigationMetadata(string navigationProperty, Type requestedType, bool requireCollection)
	{
		if (SplitName(navigationProperty).Count != 1)
		{
			throw Error.DbEntityEntry_DottedPathMustBeProperty(navigationProperty);
		}
		NavigationEntryMetadata navigationMetadata = GetNavigationMetadata(navigationProperty);
		if (navigationMetadata == null)
		{
			throw Error.DbEntityEntry_NotANavigationProperty(navigationProperty, EntityType.Name);
		}
		if (requireCollection)
		{
			if (navigationMetadata.MemberEntryType == MemberEntryType.ReferenceNavigationProperty)
			{
				throw Error.DbEntityEntry_UsedCollectionForReferenceProp(navigationProperty, EntityType.Name);
			}
		}
		else if (navigationMetadata.MemberEntryType == MemberEntryType.CollectionNavigationProperty)
		{
			throw Error.DbEntityEntry_UsedReferenceForCollectionProp(navigationProperty, EntityType.Name);
		}
		if (!requestedType.IsAssignableFrom(navigationMetadata.ElementType))
		{
			throw Error.DbEntityEntry_WrongGenericForNavProp(navigationProperty, EntityType.Name, requestedType.Name, navigationMetadata.ElementType.Name);
		}
		return navigationMetadata;
	}

	public virtual NavigationEntryMetadata GetNavigationMetadata(string propertyName)
	{
		EdmEntityType.Members.TryGetValue(propertyName, ignoreCase: false, out var item);
		if (item is NavigationProperty navigationProperty)
		{
			return new NavigationEntryMetadata(EntityType, GetNavigationTargetType(navigationProperty), propertyName, navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many);
		}
		return null;
	}

	private Type GetNavigationTargetType(NavigationProperty navigationProperty)
	{
		MetadataWorkspace metadataWorkspace = _internalContext.ObjectContext.MetadataWorkspace;
		EntityType entityType = navigationProperty.RelationshipType.RelationshipEndMembers.Single((RelationshipEndMember e) => navigationProperty.ToEndMember.Name == e.Name).GetEntityType();
		StructuralType objectSpaceType = metadataWorkspace.GetObjectSpaceType(entityType);
		return ((ObjectItemCollection)metadataWorkspace.GetItemCollection(DataSpace.OSpace)).GetClrType(objectSpaceType);
	}

	public virtual IRelatedEnd GetRelatedEnd(string navigationProperty)
	{
		EdmEntityType.Members.TryGetValue(navigationProperty, ignoreCase: false, out var item);
		NavigationProperty navigationProperty2 = (NavigationProperty)item;
		return _internalContext.ObjectContext.ObjectStateManager.GetRelationshipManager(Entity).GetRelatedEnd(navigationProperty2.RelationshipType.FullName, navigationProperty2.ToEndMember.Name);
	}

	public virtual PropertyEntryMetadata ValidateAndGetPropertyMetadata(string propertyName, Type declaringType, Type requestedType)
	{
		return PropertyEntryMetadata.ValidateNameAndGetMetadata(_internalContext, declaringType, requestedType, propertyName);
	}

	private static IList<string> SplitName(string propertyName)
	{
		return propertyName.Split(new char[1] { '.' });
	}

	private void ValidateNotDetachedAndInitializeRelatedEnd(string method)
	{
		if (IsDetached)
		{
			throw Error.DbEntityEntry_NotSupportedForDetached(method, _entityType.Name);
		}
	}

	public virtual DbEntityValidationResult GetValidationResult(IDictionary<object, object> items)
	{
		EntityValidator entityValidator = InternalContext.ValidationProvider.GetEntityValidator(this);
		bool lazyLoadingEnabled = InternalContext.LazyLoadingEnabled;
		InternalContext.LazyLoadingEnabled = false;
		try
		{
			return (entityValidator != null) ? entityValidator.Validate(InternalContext.ValidationProvider.GetEntityValidationContext(this, items)) : new DbEntityValidationResult(this, Enumerable.Empty<DbValidationError>());
		}
		finally
		{
			InternalContext.LazyLoadingEnabled = lazyLoadingEnabled;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj == null || obj.GetType() != typeof(InternalEntityEntry))
		{
			return false;
		}
		return Equals((InternalEntityEntry)obj);
	}

	public bool Equals(InternalEntityEntry other)
	{
		if (this == other)
		{
			return true;
		}
		if (other != null && _entity == other._entity)
		{
			return _internalContext == other._internalContext;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return RuntimeHelpers.GetHashCode(_entity);
	}
}
