using System.Collections.Generic;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects;

internal static class DelegateFactory
{
	private static readonly MethodInfo _throwSetInvalidValue = typeof(EntityUtil).GetDeclaredMethod("ThrowSetInvalidValue", typeof(object), typeof(Type), typeof(string), typeof(string));

	internal static Func<object> GetConstructorDelegateForType(ClrComplexType clrType)
	{
		return clrType.Constructor ?? (clrType.Constructor = CreateConstructor(clrType.ClrType));
	}

	internal static Func<object> GetConstructorDelegateForType(ClrEntityType clrType)
	{
		return clrType.Constructor ?? (clrType.Constructor = CreateConstructor(clrType.ClrType));
	}

	internal static object GetValue(EdmProperty property, object target)
	{
		return GetGetterDelegateForProperty(property)(target);
	}

	internal static Func<object, object> GetGetterDelegateForProperty(EdmProperty property)
	{
		return property.ValueGetter ?? (property.ValueGetter = CreatePropertyGetter(property.EntityDeclaringType, property.PropertyInfo));
	}

	internal static void SetValue(EdmProperty property, object target, object value)
	{
		GetSetterDelegateForProperty(property)(target, value);
	}

	internal static Action<object, object> GetSetterDelegateForProperty(EdmProperty property)
	{
		Action<object, object> action = property.ValueSetter;
		if (action == null)
		{
			action = (property.ValueSetter = CreatePropertySetter(property.EntityDeclaringType, property.PropertyInfo, property.Nullable));
		}
		return action;
	}

	internal static RelatedEnd GetRelatedEnd(RelationshipManager sourceRelationshipManager, AssociationEndMember sourceMember, AssociationEndMember targetMember, RelatedEnd existingRelatedEnd)
	{
		Func<RelationshipManager, RelatedEnd, RelatedEnd> func = sourceMember.GetRelatedEnd;
		if (func == null)
		{
			func = (sourceMember.GetRelatedEnd = CreateGetRelatedEndMethod(sourceMember, targetMember));
		}
		return func(sourceRelationshipManager, existingRelatedEnd);
	}

	internal static Action<object, object> CreateNavigationPropertySetter(Type declaringType, PropertyInfo navigationProperty)
	{
		PropertyInfo propertyInfoForSet = navigationProperty.GetPropertyInfoForSet();
		MethodInfo methodInfo = propertyInfoForSet.Setter();
		if (methodInfo == null)
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyNoSetter);
		}
		if (methodInfo.IsStatic)
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyIsStatic);
		}
		if (methodInfo.DeclaringType.IsValueType())
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyDeclaringTypeIsValueType);
		}
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "entity");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "target");
		return Expression.Lambda<Action<object, object>>(Expression.Assign(Expression.Property(Expression.Convert(parameterExpression, declaringType), propertyInfoForSet), Expression.Convert(parameterExpression2, navigationProperty.PropertyType)), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
	}

	internal static ConstructorInfo GetConstructorForType(Type type)
	{
		ConstructorInfo declaredConstructor = type.GetDeclaredConstructor();
		if (null == declaredConstructor)
		{
			throw new InvalidOperationException(Strings.CodeGen_ConstructorNoParameterless(type.FullName));
		}
		return declaredConstructor;
	}

	internal static NewExpression GetNewExpressionForCollectionType(Type type)
	{
		if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(HashSet<>))
		{
			return Expression.New(type.GetDeclaredConstructor(typeof(IEqualityComparer<>).MakeGenericType(type.GetGenericArguments())), Expression.New(typeof(ObjectReferenceEqualityComparer)));
		}
		return Expression.New(GetConstructorForType(type));
	}

	internal static Func<object> CreateConstructor(Type type)
	{
		GetConstructorForType(type);
		return Expression.Lambda<Func<object>>(Expression.New(type), new ParameterExpression[0]).Compile();
	}

	internal static Func<object, object> CreatePropertyGetter(Type entityDeclaringType, PropertyInfo propertyInfo)
	{
		MethodInfo methodInfo = propertyInfo.Getter();
		if (methodInfo == null)
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyNoGetter);
		}
		if (methodInfo.IsStatic)
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyIsStatic);
		}
		if (propertyInfo.DeclaringType.IsValueType())
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyDeclaringTypeIsValueType);
		}
		if (propertyInfo.GetIndexParameters().Any())
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyIsIndexed);
		}
		Type propertyType = propertyInfo.PropertyType;
		if (propertyType.IsPointer)
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyUnsupportedType);
		}
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "entity");
		Expression expression = Expression.Property(Expression.Convert(parameterExpression, entityDeclaringType), propertyInfo);
		if (propertyType.IsValueType())
		{
			expression = Expression.Convert(expression, typeof(object));
		}
		return Expression.Lambda<Func<object, object>>(expression, new ParameterExpression[1] { parameterExpression }).Compile();
	}

	internal static Action<object, object> CreatePropertySetter(Type entityDeclaringType, PropertyInfo propertyInfo, bool allowNull)
	{
		PropertyInfo property = ValidateSetterProperty(propertyInfo);
		ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "entity");
		ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object), "target");
		Type propertyType = propertyInfo.PropertyType;
		if (propertyType.IsValueType() && Nullable.GetUnderlyingType(propertyType) == null)
		{
			allowNull = false;
		}
		Expression expression = Expression.TypeIs(parameterExpression2, propertyType);
		if (allowNull)
		{
			expression = Expression.Or(Expression.ReferenceEqual(parameterExpression2, Expression.Constant(null)), expression);
		}
		return Expression.Lambda<Action<object, object>>(Expression.IfThenElse(expression, Expression.Assign(Expression.Property(Expression.Convert(parameterExpression, entityDeclaringType), property), Expression.Convert(parameterExpression2, propertyInfo.PropertyType)), Expression.Call(_throwSetInvalidValue, parameterExpression2, Expression.Constant(propertyType), Expression.Constant(entityDeclaringType.Name), Expression.Constant(propertyInfo.Name))), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
	}

	internal static PropertyInfo ValidateSetterProperty(PropertyInfo propertyInfo)
	{
		PropertyInfo propertyInfoForSet = propertyInfo.GetPropertyInfoForSet();
		MethodInfo methodInfo = propertyInfoForSet.Setter();
		if (methodInfo == null)
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyNoSetter);
		}
		if (methodInfo.IsStatic)
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyIsStatic);
		}
		if (propertyInfoForSet.DeclaringType.IsValueType())
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyDeclaringTypeIsValueType);
		}
		if (propertyInfoForSet.GetIndexParameters().Any())
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyIsIndexed);
		}
		if (propertyInfoForSet.PropertyType.IsPointer)
		{
			throw new InvalidOperationException(Strings.CodeGen_PropertyUnsupportedType);
		}
		return propertyInfoForSet;
	}

	private static Func<RelationshipManager, RelatedEnd, RelatedEnd> CreateGetRelatedEndMethod(AssociationEndMember sourceMember, AssociationEndMember targetMember)
	{
		EntityType entityTypeForEnd = MetadataHelper.GetEntityTypeForEnd(sourceMember);
		EntityType entityTypeForEnd2 = MetadataHelper.GetEntityTypeForEnd(targetMember);
		NavigationPropertyAccessor navigationPropertyAccessor = MetadataHelper.GetNavigationPropertyAccessor(entityTypeForEnd2, targetMember, sourceMember);
		NavigationPropertyAccessor navigationPropertyAccessor2 = MetadataHelper.GetNavigationPropertyAccessor(entityTypeForEnd, sourceMember, targetMember);
		return (Func<RelationshipManager, RelatedEnd, RelatedEnd>)typeof(DelegateFactory).GetDeclaredMethod("CreateGetRelatedEndMethod", typeof(AssociationEndMember), typeof(AssociationEndMember), typeof(NavigationPropertyAccessor), typeof(NavigationPropertyAccessor)).MakeGenericMethod(entityTypeForEnd.ClrType, entityTypeForEnd2.ClrType).Invoke(null, new object[4] { sourceMember, targetMember, navigationPropertyAccessor, navigationPropertyAccessor2 });
	}

	private static Func<RelationshipManager, RelatedEnd, RelatedEnd> CreateGetRelatedEndMethod<TSource, TTarget>(AssociationEndMember sourceMember, AssociationEndMember targetMember, NavigationPropertyAccessor sourceAccessor, NavigationPropertyAccessor targetAccessor) where TSource : class where TTarget : class
	{
		switch (targetMember.RelationshipMultiplicity)
		{
		case RelationshipMultiplicity.ZeroOrOne:
		case RelationshipMultiplicity.One:
			return (RelationshipManager manager, RelatedEnd relatedEnd) => manager.GetRelatedReference<TSource, TTarget>(sourceMember, targetMember, sourceAccessor, targetAccessor, relatedEnd);
		case RelationshipMultiplicity.Many:
			return (RelationshipManager manager, RelatedEnd relatedEnd) => manager.GetRelatedCollection<TSource, TTarget>(sourceMember, targetMember, sourceAccessor, targetAccessor, relatedEnd);
		default:
		{
			Type typeFromHandle = typeof(RelationshipMultiplicity);
			throw new ArgumentOutOfRangeException(typeFromHandle.Name, Strings.ADP_InvalidEnumerationValue(typeFromHandle.Name, ((int)targetMember.RelationshipMultiplicity).ToString(CultureInfo.InvariantCulture)));
		}
		}
	}
}
