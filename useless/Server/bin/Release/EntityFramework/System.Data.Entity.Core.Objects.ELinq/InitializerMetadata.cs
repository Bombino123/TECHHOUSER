using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.Internal.Materialization;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core.Objects.Internal;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace System.Data.Entity.Core.Objects.ELinq;

internal abstract class InitializerMetadata : IEquatable<InitializerMetadata>
{
	private class Grouping<K, T> : IGrouping<K, T>, IEnumerable<T>, IEnumerable
	{
		private readonly K _key;

		private readonly IEnumerable<T> _group;

		public K Key => _key;

		public IEnumerable<T> Group => _group;

		public Grouping(K key, IEnumerable<T> group)
		{
			_key = key;
			_group = group;
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			if (_group == null)
			{
				yield break;
			}
			foreach (T item in _group)
			{
				yield return item;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	private class GroupingInitializerMetadata : InitializerMetadata
	{
		internal override InitializerMetadataKind Kind => InitializerMetadataKind.Grouping;

		internal GroupingInitializerMetadata(Type type)
			: base(type)
		{
		}

		internal override Expression Emit(List<TranslatorResult> propertyTranslatorResults)
		{
			Type type = ClrType.GetGenericArguments()[0];
			Type type2 = ClrType.GetGenericArguments()[1];
			return Expression.Convert(Expression.New(typeof(Grouping<, >).MakeGenericType(type, type2).GetConstructors().Single(), GetPropertyReaders(propertyTranslatorResults)), ClrType);
		}

		internal override IEnumerable<Type> GetChildTypes()
		{
			Type type = ClrType.GetGenericArguments()[0];
			Type groupElementType = ClrType.GetGenericArguments()[1];
			yield return type;
			yield return typeof(IEnumerable<>).MakeGenericType(groupElementType);
		}
	}

	private class ProjectionNewMetadata : InitializerMetadata
	{
		private readonly NewExpression _newExpression;

		internal override InitializerMetadataKind Kind => InitializerMetadataKind.ProjectionNew;

		internal ProjectionNewMetadata(NewExpression newExpression)
			: base(newExpression.Type)
		{
			_newExpression = newExpression;
		}

		protected override bool IsStructurallyEquivalent(InitializerMetadata other)
		{
			ProjectionNewMetadata projectionNewMetadata = (ProjectionNewMetadata)other;
			if (_newExpression.Members == null && projectionNewMetadata._newExpression.Members == null)
			{
				return true;
			}
			if (_newExpression.Members == null || projectionNewMetadata._newExpression.Members == null)
			{
				return false;
			}
			if (_newExpression.Members.Count != projectionNewMetadata._newExpression.Members.Count)
			{
				return false;
			}
			for (int i = 0; i < _newExpression.Members.Count; i++)
			{
				MemberInfo memberInfo = _newExpression.Members[i];
				MemberInfo obj = projectionNewMetadata._newExpression.Members[i];
				if (!memberInfo.Equals(obj))
				{
					return false;
				}
			}
			return true;
		}

		internal override Expression Emit(List<TranslatorResult> propertyTranslatorResults)
		{
			return Expression.New(_newExpression.Constructor, GetPropertyReaders(propertyTranslatorResults));
		}

		internal override IEnumerable<Type> GetChildTypes()
		{
			return _newExpression.Arguments.Select((Expression arg) => arg.Type);
		}

		internal override void AppendColumnMapKey(ColumnMapKeyBuilder builder)
		{
			base.AppendColumnMapKey(builder);
			builder.Append(_newExpression.Constructor.ToString());
			IEnumerable<MemberInfo> members = _newExpression.Members;
			foreach (MemberInfo item in members ?? Enumerable.Empty<MemberInfo>())
			{
				builder.Append("DT", item.DeclaringType);
				builder.Append("." + item.Name);
			}
		}
	}

	private class EmptyProjectionNewMetadata : ProjectionNewMetadata
	{
		internal EmptyProjectionNewMetadata(NewExpression newExpression)
			: base(newExpression)
		{
		}

		internal override Expression Emit(List<TranslatorResult> propertyReaders)
		{
			return base.Emit(new List<TranslatorResult>());
		}

		internal override IEnumerable<Type> GetChildTypes()
		{
			yield return null;
		}
	}

	private class ProjectionInitializerMetadata : InitializerMetadata
	{
		private readonly MemberInitExpression _initExpression;

		internal override InitializerMetadataKind Kind => InitializerMetadataKind.ProjectionInitializer;

		internal ProjectionInitializerMetadata(MemberInitExpression initExpression)
			: base(initExpression.Type)
		{
			_initExpression = initExpression;
		}

		protected override bool IsStructurallyEquivalent(InitializerMetadata other)
		{
			ProjectionInitializerMetadata projectionInitializerMetadata = (ProjectionInitializerMetadata)other;
			if (_initExpression.Bindings.Count != projectionInitializerMetadata._initExpression.Bindings.Count)
			{
				return false;
			}
			for (int i = 0; i < _initExpression.Bindings.Count; i++)
			{
				MemberBinding memberBinding = _initExpression.Bindings[i];
				MemberBinding memberBinding2 = projectionInitializerMetadata._initExpression.Bindings[i];
				if (!memberBinding.Member.Equals(memberBinding2.Member))
				{
					return false;
				}
			}
			return true;
		}

		internal override Expression Emit(List<TranslatorResult> propertyReaders)
		{
			MemberBinding[] array = new MemberBinding[_initExpression.Bindings.Count];
			MemberBinding[] array2 = new MemberBinding[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				MemberBinding memberBinding = _initExpression.Bindings[i];
				Expression unwrappedExpression = propertyReaders[i].UnwrappedExpression;
				MemberBinding memberBinding2 = Expression.Bind(memberBinding.Member, unwrappedExpression);
				MemberBinding memberBinding3 = Expression.Bind(memberBinding.Member, Expression.Constant(TypeSystem.GetDefaultValue(unwrappedExpression.Type), unwrappedExpression.Type));
				array[i] = memberBinding2;
				array2[i] = memberBinding3;
			}
			return Expression.MemberInit(_initExpression.NewExpression, array);
		}

		internal override IEnumerable<Type> GetChildTypes()
		{
			foreach (MemberBinding binding in _initExpression.Bindings)
			{
				TypeSystem.PropertyOrField(binding.Member, out var _, out var type);
				yield return type;
			}
		}

		internal override void AppendColumnMapKey(ColumnMapKeyBuilder builder)
		{
			base.AppendColumnMapKey(builder);
			foreach (MemberBinding binding in _initExpression.Bindings)
			{
				builder.Append(",", binding.Member.DeclaringType);
				builder.Append("." + binding.Member.Name);
			}
		}
	}

	internal class EntityCollectionInitializerMetadata : InitializerMetadata
	{
		private readonly NavigationProperty _navigationProperty;

		internal static readonly MethodInfo CreateEntityCollectionMethod = typeof(EntityCollectionInitializerMetadata).GetOnlyDeclaredMethod("CreateEntityCollection");

		internal override InitializerMetadataKind Kind => InitializerMetadataKind.EntityCollection;

		internal EntityCollectionInitializerMetadata(Type type, NavigationProperty navigationProperty)
			: base(type)
		{
			_navigationProperty = navigationProperty;
		}

		protected override bool IsStructurallyEquivalent(InitializerMetadata other)
		{
			EntityCollectionInitializerMetadata entityCollectionInitializerMetadata = (EntityCollectionInitializerMetadata)other;
			return _navigationProperty.Equals(entityCollectionInitializerMetadata._navigationProperty);
		}

		internal override Expression Emit(List<TranslatorResult> propertyTranslatorResults)
		{
			Type elementType = GetElementType();
			MethodInfo method = CreateEntityCollectionMethod.MakeGenericMethod(elementType);
			Expression expression = propertyTranslatorResults[0].Expression;
			Expression expressionToGetCoordinator = (propertyTranslatorResults[1] as CollectionTranslatorResult).ExpressionToGetCoordinator;
			return Expression.Call(method, expression, expressionToGetCoordinator, Expression.Constant(_navigationProperty.RelationshipType.FullName), Expression.Constant(_navigationProperty.ToEndMember.Name));
		}

		public static EntityCollection<T> CreateEntityCollection<T>(IEntityWrapper wrappedOwner, Coordinator<T> coordinator, string relationshipName, string targetRoleName) where T : class
		{
			if (wrappedOwner.Entity == null)
			{
				return null;
			}
			EntityCollection<T> result = wrappedOwner.RelationshipManager.GetRelatedCollection<T>(relationshipName, targetRoleName);
			coordinator.RegisterCloseHandler(delegate(Shaper readerState, List<IEntityWrapper> elements)
			{
				result.Load(elements, readerState.MergeOption);
			});
			return result;
		}

		internal override IEnumerable<Type> GetChildTypes()
		{
			Type elementType = GetElementType();
			yield return null;
			yield return typeof(IEnumerable<>).MakeGenericType(elementType);
		}

		internal override void AppendColumnMapKey(ColumnMapKeyBuilder builder)
		{
			base.AppendColumnMapKey(builder);
			builder.Append(",NP" + _navigationProperty.Name);
			builder.Append(",AT", _navigationProperty.DeclaringType);
		}

		private Type GetElementType()
		{
			Type type = ClrType.TryGetElementType(typeof(ICollection<>));
			if (type == null)
			{
				throw new InvalidOperationException(Strings.ELinq_UnexpectedTypeForNavigationProperty(_navigationProperty, typeof(EntityCollection<>), typeof(ICollection<>), ClrType));
			}
			return type;
		}
	}

	internal readonly Type ClrType;

	private static long s_identifier;

	internal readonly string Identity;

	private static readonly string _identifierPrefix = typeof(InitializerMetadata).Name;

	internal abstract InitializerMetadataKind Kind { get; }

	private InitializerMetadata(Type clrType)
	{
		ClrType = clrType;
		Identity = _identifierPrefix + Interlocked.Increment(ref s_identifier).ToString(CultureInfo.InvariantCulture);
	}

	internal static bool TryGetInitializerMetadata(TypeUsage typeUsage, out InitializerMetadata initializerMetadata)
	{
		initializerMetadata = null;
		if (BuiltInTypeKind.RowType == typeUsage.EdmType.BuiltInTypeKind)
		{
			initializerMetadata = ((RowType)typeUsage.EdmType).InitializerMetadata;
		}
		return initializerMetadata != null;
	}

	internal static InitializerMetadata CreateGroupingInitializer(EdmItemCollection itemCollection, Type resultType)
	{
		return itemCollection.GetCanonicalInitializerMetadata(new GroupingInitializerMetadata(resultType));
	}

	internal static InitializerMetadata CreateProjectionInitializer(EdmItemCollection itemCollection, MemberInitExpression initExpression)
	{
		return itemCollection.GetCanonicalInitializerMetadata(new ProjectionInitializerMetadata(initExpression));
	}

	internal static InitializerMetadata CreateProjectionInitializer(EdmItemCollection itemCollection, NewExpression newExpression)
	{
		return itemCollection.GetCanonicalInitializerMetadata(new ProjectionNewMetadata(newExpression));
	}

	internal static InitializerMetadata CreateEmptyProjectionInitializer(EdmItemCollection itemCollection, NewExpression newExpression)
	{
		return itemCollection.GetCanonicalInitializerMetadata(new EmptyProjectionNewMetadata(newExpression));
	}

	internal static InitializerMetadata CreateEntityCollectionInitializer(EdmItemCollection itemCollection, Type type, NavigationProperty navigationProperty)
	{
		return itemCollection.GetCanonicalInitializerMetadata(new EntityCollectionInitializerMetadata(type, navigationProperty));
	}

	internal virtual void AppendColumnMapKey(ColumnMapKeyBuilder builder)
	{
		builder.Append("CLR-", ClrType);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as InitializerMetadata);
	}

	public bool Equals(InitializerMetadata other)
	{
		if (this == other)
		{
			return true;
		}
		if (Kind != other.Kind)
		{
			return false;
		}
		if (!ClrType.Equals(other.ClrType))
		{
			return false;
		}
		return IsStructurallyEquivalent(other);
	}

	public override int GetHashCode()
	{
		return ClrType.GetHashCode();
	}

	protected virtual bool IsStructurallyEquivalent(InitializerMetadata other)
	{
		return true;
	}

	internal abstract Expression Emit(List<TranslatorResult> propertyTranslatorResults);

	internal abstract IEnumerable<Type> GetChildTypes();

	protected static List<Expression> GetPropertyReaders(List<TranslatorResult> propertyTranslatorResults)
	{
		return propertyTranslatorResults.Select((TranslatorResult s) => s.UnwrappedExpression).ToList();
	}
}
