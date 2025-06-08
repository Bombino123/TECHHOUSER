using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;

namespace System.Data.Entity.Core.Objects.Internal;

internal class ObjectFullSpanRewriter : ObjectSpanRewriter
{
	private class SpanPathInfo
	{
		internal readonly EntityType DeclaringType;

		internal Dictionary<NavigationProperty, SpanPathInfo> Children;

		internal SpanPathInfo(EntityType declaringType)
		{
			DeclaringType = declaringType;
		}
	}

	private readonly Stack<SpanPathInfo> _currentSpanPath = new Stack<SpanPathInfo>();

	internal ObjectFullSpanRewriter(DbCommandTree tree, DbExpression toRewrite, Span span, AliasGenerator aliasGenerator)
		: base(tree, toRewrite, aliasGenerator)
	{
		EntityType entityType = null;
		if (!TryGetEntityType(base.Query.ResultType, out entityType))
		{
			throw new InvalidOperationException(Strings.ObjectQuery_Span_IncludeRequiresEntityOrEntityCollection);
		}
		SpanPathInfo spanPathInfo = new SpanPathInfo(entityType);
		foreach (Span.SpanPath span2 in span.SpanList)
		{
			AddSpanPath(spanPathInfo, span2.Navigations);
		}
		_currentSpanPath.Push(spanPathInfo);
	}

	private void AddSpanPath(SpanPathInfo parentInfo, List<string> navPropNames)
	{
		ConvertSpanPath(parentInfo, navPropNames, 0);
	}

	private void ConvertSpanPath(SpanPathInfo parentInfo, List<string> navPropNames, int pos)
	{
		NavigationProperty item = null;
		if (!parentInfo.DeclaringType.NavigationProperties.TryGetValue(navPropNames[pos], ignoreCase: true, out item))
		{
			throw new InvalidOperationException(Strings.ObjectQuery_Span_NoNavProp(parentInfo.DeclaringType.FullName, navPropNames[pos]));
		}
		if (parentInfo.Children == null)
		{
			parentInfo.Children = new Dictionary<NavigationProperty, SpanPathInfo>();
		}
		SpanPathInfo value = null;
		if (!parentInfo.Children.TryGetValue(item, out value))
		{
			value = new SpanPathInfo(EntityTypeFromResultType(item));
			parentInfo.Children[item] = value;
		}
		if (pos < navPropNames.Count - 1)
		{
			ConvertSpanPath(value, navPropNames, pos + 1);
		}
	}

	private static EntityType EntityTypeFromResultType(NavigationProperty navProp)
	{
		EntityType entityType = null;
		TryGetEntityType(navProp.TypeUsage, out entityType);
		return entityType;
	}

	private static bool TryGetEntityType(TypeUsage resultType, out EntityType entityType)
	{
		if (BuiltInTypeKind.EntityType == resultType.EdmType.BuiltInTypeKind)
		{
			entityType = (EntityType)resultType.EdmType;
			return true;
		}
		if (BuiltInTypeKind.CollectionType == resultType.EdmType.BuiltInTypeKind)
		{
			EdmType edmType = ((CollectionType)resultType.EdmType).TypeUsage.EdmType;
			if (BuiltInTypeKind.EntityType == edmType.BuiltInTypeKind)
			{
				entityType = (EntityType)edmType;
				return true;
			}
		}
		entityType = null;
		return false;
	}

	private AssociationEndMember GetNavigationPropertyTargetEnd(NavigationProperty property)
	{
		return base.Metadata.GetItem<AssociationType>(property.RelationshipType.FullName, DataSpace.CSpace).AssociationEndMembers[property.ToEndMember.Name];
	}

	internal override SpanTrackingInfo CreateEntitySpanTrackingInfo(DbExpression expression, EntityType entityType)
	{
		SpanTrackingInfo result = default(SpanTrackingInfo);
		SpanPathInfo spanPathInfo = _currentSpanPath.Peek();
		if (spanPathInfo.Children != null)
		{
			int num = 1;
			foreach (KeyValuePair<NavigationProperty, SpanPathInfo> child in spanPathInfo.Children)
			{
				if (result.ColumnDefinitions == null)
				{
					result = InitializeTrackingInfo(base.RelationshipSpan);
				}
				DbExpression expression2 = expression.Property(child.Key);
				_currentSpanPath.Push(child.Value);
				expression2 = Rewrite(expression2);
				_currentSpanPath.Pop();
				result.ColumnDefinitions.Add(new KeyValuePair<string, DbExpression>(result.ColumnNames.Next(), expression2));
				AssociationEndMember navigationPropertyTargetEnd = GetNavigationPropertyTargetEnd(child.Key);
				result.SpannedColumns[num] = navigationPropertyTargetEnd;
				if (base.RelationshipSpan)
				{
					result.FullSpannedEnds[navigationPropertyTargetEnd] = true;
				}
				num++;
			}
		}
		return result;
	}
}
