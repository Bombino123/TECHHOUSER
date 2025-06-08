using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Internal;
using System.Data.Entity.ModelConfiguration.Edm;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Reflection;

namespace System.Data.Entity.Migrations.Infrastructure;

internal class ModificationCommandTreeGenerator
{
	private class TempDbContext : DbContext
	{
		public TempDbContext(DbCompiledModel model)
			: base(model)
		{
			InternalContext.InitializerDisabled = true;
		}

		public TempDbContext(DbConnection connection, DbCompiledModel model)
			: base(connection, model, contextOwnsConnection: false)
		{
			InternalContext.InitializerDisabled = true;
		}
	}

	private readonly DbCompiledModel _compiledModel;

	private readonly DbConnection _connection;

	private readonly MetadataWorkspace _metadataWorkspace;

	public ModificationCommandTreeGenerator(DbModel model, DbConnection connection = null)
	{
		_compiledModel = model.Compile();
		_connection = connection;
		using DbContext dbContext = CreateContext();
		_metadataWorkspace = ((IObjectContextAdapter)dbContext).ObjectContext.MetadataWorkspace;
	}

	private DbContext CreateContext()
	{
		if (_connection != null)
		{
			return new TempDbContext(_connection, _compiledModel);
		}
		return new TempDbContext(_compiledModel);
	}

	public IEnumerable<DbInsertCommandTree> GenerateAssociationInsert(string associationIdentity)
	{
		return GenerateAssociation<DbInsertCommandTree>(associationIdentity, EntityState.Added);
	}

	public IEnumerable<DbDeleteCommandTree> GenerateAssociationDelete(string associationIdentity)
	{
		return GenerateAssociation<DbDeleteCommandTree>(associationIdentity, EntityState.Deleted);
	}

	private IEnumerable<TCommandTree> GenerateAssociation<TCommandTree>(string associationIdentity, EntityState state) where TCommandTree : DbCommandTree
	{
		AssociationType item = _metadataWorkspace.GetItem<AssociationType>(associationIdentity, DataSpace.CSpace);
		using DbContext context = CreateContext();
		EntityType entityType = item.SourceEnd.GetEntityType();
		object obj = InstantiateAndAttachEntity(entityType, context);
		EntityType entityType2 = item.TargetEnd.GetEntityType();
		object targetEntity = ((entityType.GetRootType() == entityType2.GetRootType()) ? obj : InstantiateAndAttachEntity(entityType2, context));
		((IObjectContextAdapter)context).ObjectContext.ObjectStateManager.ChangeRelationshipState(obj, targetEntity, item.FullName, item.TargetEnd.Name, (state == EntityState.Deleted) ? state : EntityState.Added);
		using CommandTracer commandTracer = new CommandTracer(context);
		context.SaveChanges();
		foreach (DbCommandTree commandTree in commandTracer.CommandTrees)
		{
			yield return (TCommandTree)commandTree;
		}
	}

	private object InstantiateAndAttachEntity(EntityType entityType, DbContext context)
	{
		Type clrType = entityType.GetClrType();
		DbSet dbSet = context.Set(clrType);
		object obj = InstantiateEntity(entityType, context, clrType, dbSet);
		SetFakeReferenceKeyValues(obj, entityType);
		SetFakeKeyValues(obj, entityType);
		dbSet.Attach(obj);
		return obj;
	}

	private object InstantiateEntity(EntityType entityType, DbContext context, Type clrType, DbSet set)
	{
		object obj;
		if (!clrType.IsAbstract())
		{
			obj = set.Create();
		}
		else
		{
			EntityType entityType2 = _metadataWorkspace.GetItems<EntityType>(DataSpace.CSpace).First((EntityType et) => entityType.IsAncestorOf(et) && !et.Abstract);
			obj = context.Set(entityType2.GetClrType()).Create();
		}
		InstantiateComplexProperties(obj, entityType.Properties);
		return obj;
	}

	public IEnumerable<DbModificationCommandTree> GenerateInsert(string entityIdentity)
	{
		return Generate(entityIdentity, EntityState.Added);
	}

	public IEnumerable<DbModificationCommandTree> GenerateUpdate(string entityIdentity)
	{
		return Generate(entityIdentity, EntityState.Modified);
	}

	public IEnumerable<DbModificationCommandTree> GenerateDelete(string entityIdentity)
	{
		return Generate(entityIdentity, EntityState.Deleted);
	}

	private IEnumerable<DbModificationCommandTree> Generate(string entityIdentity, EntityState state)
	{
		EntityType item = _metadataWorkspace.GetItem<EntityType>(entityIdentity, DataSpace.CSpace);
		using DbContext context = CreateContext();
		object entity = InstantiateAndAttachEntity(item, context);
		if (state != EntityState.Deleted)
		{
			context.Entry(entity).State = state;
		}
		ChangeRelationshipStates(context, item, entity, state);
		if (state == EntityState.Deleted)
		{
			context.Entry(entity).State = state;
		}
		HandleTableSplitting(context, item, entity, state);
		using CommandTracer commandTracer = new CommandTracer(context);
		((IObjectContextAdapter)context).ObjectContext.SaveChanges(SaveOptions.None);
		foreach (DbCommandTree commandTree in commandTracer.CommandTrees)
		{
			yield return (DbModificationCommandTree)commandTree;
		}
	}

	private void ChangeRelationshipStates(DbContext context, EntityType entityType, object entity, EntityState state)
	{
		ObjectStateManager objectStateManager = ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager;
		foreach (AssociationType item in from at in _metadataWorkspace.GetItems<AssociationType>(DataSpace.CSpace)
			where !at.IsForeignKey && !at.IsManyToMany() && (at.SourceEnd.GetEntityType().IsAssignableFrom(entityType) || at.TargetEnd.GetEntityType().IsAssignableFrom(entityType))
			select at)
		{
			if (!item.TryGuessPrincipalAndDependentEnds(out var principalEnd, out var dependentEnd))
			{
				principalEnd = item.SourceEnd;
				dependentEnd = item.TargetEnd;
			}
			if (dependentEnd.GetEntityType().IsAssignableFrom(entityType))
			{
				EntityType entityType2 = principalEnd.GetEntityType();
				Type clrType = entityType2.GetClrType();
				DbSet dbSet = context.Set(clrType);
				object obj = dbSet.Local.Cast<object>().SingleOrDefault();
				if (obj == null || (entity == obj && state == EntityState.Added))
				{
					obj = InstantiateEntity(entityType2, context, clrType, dbSet);
					SetFakeReferenceKeyValues(obj, entityType2);
					dbSet.Attach(obj);
				}
				if (principalEnd.IsRequired() && state == EntityState.Modified)
				{
					object obj2 = InstantiateEntity(entityType2, context, clrType, dbSet);
					SetFakeKeyValues(obj2, entityType2);
					dbSet.Attach(obj2);
					objectStateManager.ChangeRelationshipState(entity, obj2, item.FullName, principalEnd.Name, EntityState.Deleted);
				}
				objectStateManager.ChangeRelationshipState(entity, obj, item.FullName, principalEnd.Name, (state == EntityState.Deleted) ? state : EntityState.Added);
			}
		}
	}

	private void HandleTableSplitting(DbContext context, EntityType entityType, object entity, EntityState state)
	{
		foreach (AssociationType item in from at in _metadataWorkspace.GetItems<AssociationType>(DataSpace.CSpace)
			where at.IsForeignKey && at.IsRequiredToRequired() && !at.IsSelfReferencing() && (at.SourceEnd.GetEntityType().IsAssignableFrom(entityType) || at.TargetEnd.GetEntityType().IsAssignableFrom(entityType)) && _metadataWorkspace.GetItems<AssociationType>(DataSpace.SSpace).All((AssociationType fk) => fk.Name != at.Name)
			select at)
		{
			if (!item.TryGuessPrincipalAndDependentEnds(out var principalEnd, out var dependentEnd))
			{
				principalEnd = item.SourceEnd;
				dependentEnd = item.TargetEnd;
			}
			bool flag = false;
			EntityType entityType2;
			if (principalEnd.GetEntityType().GetRootType() == entityType.GetRootType())
			{
				flag = true;
				entityType2 = dependentEnd.GetEntityType();
			}
			else
			{
				entityType2 = principalEnd.GetEntityType();
			}
			object entity2 = InstantiateAndAttachEntity(entityType2, context);
			if (!flag)
			{
				switch (state)
				{
				case EntityState.Added:
					context.Entry(entity).State = EntityState.Modified;
					break;
				case EntityState.Deleted:
					context.Entry(entity).State = EntityState.Unchanged;
					break;
				}
			}
			else if (state != EntityState.Modified)
			{
				context.Entry(entity2).State = state;
			}
		}
	}

	private static void SetFakeReferenceKeyValues(object entity, EntityType entityType)
	{
		foreach (EdmProperty keyProperty in entityType.KeyProperties)
		{
			PropertyInfo clrPropertyInfo = keyProperty.GetClrPropertyInfo();
			object fakeReferenceKeyValue = GetFakeReferenceKeyValue(keyProperty.UnderlyingPrimitiveType.PrimitiveTypeKind);
			if (fakeReferenceKeyValue != null)
			{
				clrPropertyInfo.GetPropertyInfoForSet().SetValue(entity, fakeReferenceKeyValue, null);
			}
		}
	}

	private static object GetFakeReferenceKeyValue(PrimitiveTypeKind primitiveTypeKind)
	{
		return primitiveTypeKind switch
		{
			PrimitiveTypeKind.Binary => new byte[0], 
			PrimitiveTypeKind.String => "42", 
			PrimitiveTypeKind.Geometry => DefaultSpatialServices.Instance.GeometryFromText("POINT (4 2)"), 
			PrimitiveTypeKind.Geography => DefaultSpatialServices.Instance.GeographyFromText("POINT (4 2)"), 
			_ => null, 
		};
	}

	private static void SetFakeKeyValues(object entity, EntityType entityType)
	{
		foreach (EdmProperty keyProperty in entityType.KeyProperties)
		{
			PropertyInfo clrPropertyInfo = keyProperty.GetClrPropertyInfo();
			object fakeKeyValue = GetFakeKeyValue(keyProperty.UnderlyingPrimitiveType.PrimitiveTypeKind);
			clrPropertyInfo.GetPropertyInfoForSet().SetValue(entity, fakeKeyValue, null);
		}
	}

	private static object GetFakeKeyValue(PrimitiveTypeKind primitiveTypeKind)
	{
		return primitiveTypeKind switch
		{
			PrimitiveTypeKind.Binary => new byte[1] { 66 }, 
			PrimitiveTypeKind.Boolean => true, 
			PrimitiveTypeKind.Byte => (byte)66, 
			PrimitiveTypeKind.DateTime => DateTime.Now, 
			PrimitiveTypeKind.Decimal => 42m, 
			PrimitiveTypeKind.Double => 42.0, 
			PrimitiveTypeKind.Guid => Guid.NewGuid(), 
			PrimitiveTypeKind.Single => 42f, 
			PrimitiveTypeKind.SByte => (sbyte)42, 
			PrimitiveTypeKind.Int16 => (short)42, 
			PrimitiveTypeKind.Int32 => 42, 
			PrimitiveTypeKind.Int64 => 42L, 
			PrimitiveTypeKind.String => "42'", 
			PrimitiveTypeKind.Time => TimeSpan.FromMilliseconds(42.0), 
			PrimitiveTypeKind.DateTimeOffset => DateTimeOffset.Now, 
			PrimitiveTypeKind.Geometry => DefaultSpatialServices.Instance.GeometryFromText("POINT (4 3)"), 
			PrimitiveTypeKind.Geography => DefaultSpatialServices.Instance.GeographyFromText("POINT (4 3)"), 
			_ => null, 
		};
	}

	private static void InstantiateComplexProperties(object structuralObject, IEnumerable<EdmProperty> properties)
	{
		foreach (EdmProperty property in properties)
		{
			if (property.IsComplexType)
			{
				PropertyInfo clrPropertyInfo = property.GetClrPropertyInfo();
				object obj = Activator.CreateInstance(clrPropertyInfo.PropertyType);
				InstantiateComplexProperties(obj, property.ComplexType.Properties);
				clrPropertyInfo.GetPropertyInfoForSet().SetValue(structuralObject, obj, null);
			}
		}
	}
}
