using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Internal.Linq;
using System.Data.Entity.ModelConfiguration.Utilities;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Migrations;

public static class DbSetMigrationsExtensions
{
	public static void AddOrUpdate<TEntity>(this IDbSet<TEntity> set, params TEntity[] entities) where TEntity : class
	{
		Check.NotNull(set, "set");
		Check.NotNull(entities, "entities");
		if (set is DbSet<TEntity> dbSet)
		{
			InternalSet<TEntity> internalSet = (InternalSet<TEntity>)((IInternalSetAdapter)dbSet).InternalSet;
			if (internalSet != null)
			{
				dbSet.AddOrUpdate(GetKeyProperties(typeof(TEntity), internalSet), internalSet, entities);
				return;
			}
		}
		Type type = set.GetType();
		MethodInfo declaredMethod = type.GetDeclaredMethod("AddOrUpdate", typeof(TEntity[]));
		if (declaredMethod == null)
		{
			throw Error.UnableToDispatchAddOrUpdate(type);
		}
		object[] parameters = new TEntity[1][] { entities };
		declaredMethod.Invoke(set, parameters);
	}

	public static void AddOrUpdate<TEntity>(this IDbSet<TEntity> set, Expression<Func<TEntity, object>> identifierExpression, params TEntity[] entities) where TEntity : class
	{
		Check.NotNull(set, "set");
		Check.NotNull(identifierExpression, "identifierExpression");
		Check.NotNull(entities, "entities");
		if (set is DbSet<TEntity> dbSet)
		{
			InternalSet<TEntity> internalSet = (InternalSet<TEntity>)((IInternalSetAdapter)dbSet).InternalSet;
			if (internalSet != null)
			{
				IEnumerable<PropertyPath> simplePropertyAccessList = identifierExpression.GetSimplePropertyAccessList();
				dbSet.AddOrUpdate(simplePropertyAccessList, internalSet, entities);
				return;
			}
		}
		Type type = set.GetType();
		MethodInfo declaredMethod = type.GetDeclaredMethod("AddOrUpdate", typeof(Expression<Func<TEntity, object>>), typeof(TEntity[]));
		if (declaredMethod == null)
		{
			throw Error.UnableToDispatchAddOrUpdate(type);
		}
		declaredMethod.Invoke(set, new object[2] { identifierExpression, entities });
	}

	private static void AddOrUpdate<TEntity>(this DbSet<TEntity> set, IEnumerable<PropertyPath> identifyingProperties, InternalSet<TEntity> internalSet, params TEntity[] entities) where TEntity : class
	{
		IEnumerable<PropertyPath> keyProperties = GetKeyProperties(typeof(TEntity), internalSet);
		ParameterExpression parameter = Expression.Parameter(typeof(TEntity));
		foreach (TEntity entity in entities)
		{
			Expression body = identifyingProperties.Select((PropertyPath pi) => Expression.Equal(Expression.Property(parameter, pi.Single()), Expression.Constant(pi.Last().GetValue(entity, null), pi.Last().PropertyType))).Aggregate(null, (Expression current, BinaryExpression predicate) => (current != null) ? Expression.AndAlso(current, predicate) : predicate);
			TEntity val = set.SingleOrDefault(Expression.Lambda<Func<TEntity, bool>>(body, new ParameterExpression[1] { parameter }));
			if (val != null)
			{
				foreach (PropertyPath item in keyProperties)
				{
					item.Single().GetPropertyInfoForSet().SetValue(entity, item.Single().GetValue(val, null), null);
				}
				internalSet.InternalContext.Owner.Entry(val).CurrentValues.SetValues(entity);
			}
			else
			{
				internalSet.Add(entity);
			}
		}
	}

	private static IEnumerable<PropertyPath> GetKeyProperties<TEntity>(Type entityType, InternalSet<TEntity> internalSet) where TEntity : class
	{
		return internalSet.InternalContext.GetEntitySetAndBaseTypeForType(typeof(TEntity)).EntitySet.ElementType.KeyMembers.Select((EdmMember km) => new PropertyPath(entityType.GetAnyProperty(km.Name)));
	}
}
