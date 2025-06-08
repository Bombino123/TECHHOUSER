using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Query.InternalTrees;
using System.Linq;

namespace System.Data.Entity.Core.Mapping.ViewGeneration;

internal class DiscriminatorMap
{
	internal readonly DbPropertyExpression Discriminator;

	internal readonly ReadOnlyCollection<KeyValuePair<object, EntityType>> TypeMap;

	internal readonly ReadOnlyCollection<KeyValuePair<EdmProperty, DbExpression>> PropertyMap;

	internal readonly ReadOnlyCollection<KeyValuePair<RelProperty, DbExpression>> RelPropertyMap;

	internal readonly EntitySet EntitySet;

	private DiscriminatorMap(DbPropertyExpression discriminator, List<KeyValuePair<object, EntityType>> typeMap, Dictionary<EdmProperty, DbExpression> propertyMap, Dictionary<RelProperty, DbExpression> relPropertyMap, EntitySet entitySet)
	{
		Discriminator = discriminator;
		TypeMap = new ReadOnlyCollection<KeyValuePair<object, EntityType>>(typeMap);
		PropertyMap = new ReadOnlyCollection<KeyValuePair<EdmProperty, DbExpression>>(propertyMap.ToList());
		RelPropertyMap = new ReadOnlyCollection<KeyValuePair<RelProperty, DbExpression>>(relPropertyMap.ToList());
		EntitySet = entitySet;
	}

	internal static bool TryCreateDiscriminatorMap(EntitySet entitySet, DbExpression queryView, out DiscriminatorMap discriminatorMap)
	{
		discriminatorMap = null;
		if (queryView.ExpressionKind != DbExpressionKind.Project)
		{
			return false;
		}
		DbProjectExpression dbProjectExpression = (DbProjectExpression)queryView;
		if (dbProjectExpression.Projection.ExpressionKind != DbExpressionKind.Case)
		{
			return false;
		}
		DbCaseExpression dbCaseExpression = (DbCaseExpression)dbProjectExpression.Projection;
		if (dbProjectExpression.Projection.ResultType.EdmType.BuiltInTypeKind != BuiltInTypeKind.EntityType)
		{
			return false;
		}
		if (dbProjectExpression.Input.Expression.ExpressionKind != DbExpressionKind.Filter)
		{
			return false;
		}
		DbFilterExpression filter = (DbFilterExpression)dbProjectExpression.Input.Expression;
		HashSet<object> discriminatorDomain = new HashSet<object>();
		if (!ViewSimplifier.TryMatchDiscriminatorPredicate(filter, delegate(DbComparisonExpression equalsExp, object discriminatorValue)
		{
			discriminatorDomain.Add(discriminatorValue);
		}))
		{
			return false;
		}
		List<KeyValuePair<object, EntityType>> list = new List<KeyValuePair<object, EntityType>>();
		Dictionary<EdmProperty, DbExpression> propertyMap = new Dictionary<EdmProperty, DbExpression>();
		Dictionary<RelProperty, DbExpression> relPropertyMap = new Dictionary<RelProperty, DbExpression>();
		Dictionary<EntityType, List<RelProperty>> typeToRelPropertyMap = new Dictionary<EntityType, List<RelProperty>>();
		DbPropertyExpression discriminator = null;
		EdmProperty edmProperty = null;
		for (int i = 0; i < dbCaseExpression.When.Count; i++)
		{
			DbExpression expression = dbCaseExpression.When[i];
			DbExpression then = dbCaseExpression.Then[i];
			string variableName = dbProjectExpression.Input.VariableName;
			if (!ViewSimplifier.TryMatchPropertyEqualsValue(expression, variableName, out var property, out var value))
			{
				return false;
			}
			if (edmProperty == null)
			{
				edmProperty = (EdmProperty)property.Property;
			}
			else if (edmProperty != property.Property)
			{
				return false;
			}
			discriminator = property;
			if (!TryMatchEntityTypeConstructor(then, propertyMap, relPropertyMap, typeToRelPropertyMap, out var entityType))
			{
				return false;
			}
			list.Add(new KeyValuePair<object, EntityType>(value, entityType));
			discriminatorDomain.Remove(value);
		}
		if (1 != discriminatorDomain.Count)
		{
			return false;
		}
		if (dbCaseExpression.Else == null || !TryMatchEntityTypeConstructor(dbCaseExpression.Else, propertyMap, relPropertyMap, typeToRelPropertyMap, out var entityType2))
		{
			return false;
		}
		list.Add(new KeyValuePair<object, EntityType>(discriminatorDomain.Single(), entityType2));
		if (!CheckForMissingRelProperties(relPropertyMap, typeToRelPropertyMap))
		{
			return false;
		}
		int num = list.Select((KeyValuePair<object, EntityType> map) => map.Key).Distinct(TrailingSpaceComparer.Instance).Count();
		int count = list.Count;
		if (num != count)
		{
			return false;
		}
		discriminatorMap = new DiscriminatorMap(discriminator, list, propertyMap, relPropertyMap, entitySet);
		return true;
	}

	private static bool CheckForMissingRelProperties(Dictionary<RelProperty, DbExpression> relPropertyMap, Dictionary<EntityType, List<RelProperty>> typeToRelPropertyMap)
	{
		foreach (RelProperty key in relPropertyMap.Keys)
		{
			foreach (KeyValuePair<EntityType, List<RelProperty>> item in typeToRelPropertyMap)
			{
				if (item.Key.IsSubtypeOf(key.FromEnd.TypeUsage.EdmType) && !item.Value.Contains(key))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool TryMatchEntityTypeConstructor(DbExpression then, Dictionary<EdmProperty, DbExpression> propertyMap, Dictionary<RelProperty, DbExpression> relPropertyMap, Dictionary<EntityType, List<RelProperty>> typeToRelPropertyMap, out EntityType entityType)
	{
		if (then.ExpressionKind != DbExpressionKind.NewInstance)
		{
			entityType = null;
			return false;
		}
		DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)then;
		entityType = (EntityType)dbNewInstanceExpression.ResultType.EdmType;
		for (int i = 0; i < entityType.Properties.Count; i++)
		{
			EdmProperty key = entityType.Properties[i];
			DbExpression dbExpression = dbNewInstanceExpression.Arguments[i];
			if (propertyMap.TryGetValue(key, out var value))
			{
				if (!ExpressionsCompatible(dbExpression, value))
				{
					return false;
				}
			}
			else
			{
				propertyMap.Add(key, dbExpression);
			}
		}
		if (dbNewInstanceExpression.HasRelatedEntityReferences)
		{
			if (!typeToRelPropertyMap.TryGetValue(entityType, out var value2))
			{
				value2 = new List<RelProperty>();
				typeToRelPropertyMap[entityType] = value2;
			}
			foreach (DbRelatedEntityRef relatedEntityReference in dbNewInstanceExpression.RelatedEntityReferences)
			{
				RelProperty relProperty = new RelProperty((RelationshipType)relatedEntityReference.TargetEnd.DeclaringType, relatedEntityReference.SourceEnd, relatedEntityReference.TargetEnd);
				DbExpression targetEntityReference = relatedEntityReference.TargetEntityReference;
				if (relPropertyMap.TryGetValue(relProperty, out var value3))
				{
					if (!ExpressionsCompatible(targetEntityReference, value3))
					{
						return false;
					}
				}
				else
				{
					relPropertyMap.Add(relProperty, targetEntityReference);
				}
				value2.Add(relProperty);
			}
		}
		return true;
	}

	private static bool ExpressionsCompatible(DbExpression x, DbExpression y)
	{
		if (x.ExpressionKind != y.ExpressionKind)
		{
			return false;
		}
		switch (x.ExpressionKind)
		{
		case DbExpressionKind.Property:
		{
			DbPropertyExpression dbPropertyExpression = (DbPropertyExpression)x;
			DbPropertyExpression dbPropertyExpression2 = (DbPropertyExpression)y;
			if (dbPropertyExpression.Property == dbPropertyExpression2.Property)
			{
				return ExpressionsCompatible(dbPropertyExpression.Instance, dbPropertyExpression2.Instance);
			}
			return false;
		}
		case DbExpressionKind.VariableReference:
			return ((DbVariableReferenceExpression)x).VariableName == ((DbVariableReferenceExpression)y).VariableName;
		case DbExpressionKind.NewInstance:
		{
			DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)x;
			DbNewInstanceExpression dbNewInstanceExpression2 = (DbNewInstanceExpression)y;
			if (!dbNewInstanceExpression.ResultType.EdmType.EdmEquals(dbNewInstanceExpression2.ResultType.EdmType))
			{
				return false;
			}
			for (int i = 0; i < dbNewInstanceExpression.Arguments.Count; i++)
			{
				if (!ExpressionsCompatible(dbNewInstanceExpression.Arguments[i], dbNewInstanceExpression2.Arguments[i]))
				{
					return false;
				}
			}
			return true;
		}
		case DbExpressionKind.Ref:
		{
			DbRefExpression dbRefExpression = (DbRefExpression)x;
			DbRefExpression dbRefExpression2 = (DbRefExpression)y;
			if (dbRefExpression.EntitySet.EdmEquals(dbRefExpression2.EntitySet))
			{
				return ExpressionsCompatible(dbRefExpression.Argument, dbRefExpression2.Argument);
			}
			return false;
		}
		default:
			return false;
		}
	}
}
