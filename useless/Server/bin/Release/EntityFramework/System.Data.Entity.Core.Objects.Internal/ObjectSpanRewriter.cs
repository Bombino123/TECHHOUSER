using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;

namespace System.Data.Entity.Core.Objects.Internal;

internal class ObjectSpanRewriter
{
	internal struct SpanTrackingInfo
	{
		public List<KeyValuePair<string, DbExpression>> ColumnDefinitions;

		public AliasGenerator ColumnNames;

		public Dictionary<int, AssociationEndMember> SpannedColumns;

		public Dictionary<AssociationEndMember, bool> FullSpannedEnds;
	}

	private class NavigationInfo
	{
		private readonly DbVariableReferenceExpression _sourceRef;

		private readonly AssociationEndMember _sourceEnd;

		private readonly DbExpression _source;

		public bool InUse;

		public AssociationEndMember SourceEnd => _sourceEnd;

		public DbExpression Source => _source;

		public DbVariableReferenceExpression SourceVariable => _sourceRef;

		public NavigationInfo(DbRelationshipNavigationExpression originalNavigation, DbRelationshipNavigationExpression rewrittenNavigation)
		{
			_sourceEnd = (AssociationEndMember)originalNavigation.NavigateFrom;
			_sourceRef = (DbVariableReferenceExpression)rewrittenNavigation.NavigationSource;
			_source = originalNavigation.NavigationSource;
		}
	}

	private class RelationshipNavigationVisitor : DefaultExpressionVisitor
	{
		private readonly AliasGenerator _aliasGenerator;

		private DbRelationshipNavigationExpression _original;

		private DbRelationshipNavigationExpression _rewritten;

		internal static DbExpression FindNavigationExpression(DbExpression expression, AliasGenerator aliasGenerator, out NavigationInfo navInfo)
		{
			navInfo = null;
			TypeUsage typeUsage = ((CollectionType)expression.ResultType.EdmType).TypeUsage;
			if (!TypeSemantics.IsEntityType(typeUsage) && !TypeSemantics.IsReferenceType(typeUsage))
			{
				return expression;
			}
			RelationshipNavigationVisitor relationshipNavigationVisitor = new RelationshipNavigationVisitor(aliasGenerator);
			DbExpression dbExpression = relationshipNavigationVisitor.Find(expression);
			if (expression != dbExpression)
			{
				navInfo = new NavigationInfo(relationshipNavigationVisitor._original, relationshipNavigationVisitor._rewritten);
				return dbExpression;
			}
			return expression;
		}

		private RelationshipNavigationVisitor(AliasGenerator aliasGenerator)
		{
			_aliasGenerator = aliasGenerator;
		}

		private DbExpression Find(DbExpression expression)
		{
			return VisitExpression(expression);
		}

		protected override DbExpression VisitExpression(DbExpression expression)
		{
			switch (expression.ExpressionKind)
			{
			case DbExpressionKind.Distinct:
			case DbExpressionKind.Filter:
			case DbExpressionKind.Limit:
			case DbExpressionKind.OfType:
			case DbExpressionKind.Project:
			case DbExpressionKind.RelationshipNavigation:
			case DbExpressionKind.Skip:
			case DbExpressionKind.Sort:
				return base.VisitExpression(expression);
			default:
				return expression;
			}
		}

		public override DbExpression Visit(DbRelationshipNavigationExpression expression)
		{
			Check.NotNull(expression, "expression");
			_original = expression;
			string name = _aliasGenerator.Next();
			DbVariableReferenceExpression navigateFrom = new DbVariableReferenceExpression(expression.NavigationSource.ResultType, name);
			_rewritten = navigateFrom.Navigate(expression.NavigateFrom, expression.NavigateTo);
			return _rewritten;
		}

		public override DbExpression Visit(DbFilterExpression expression)
		{
			Check.NotNull(expression, "expression");
			DbExpression dbExpression = Find(expression.Input.Expression);
			if (dbExpression != expression.Input.Expression)
			{
				return dbExpression.BindAs(expression.Input.VariableName).Filter(expression.Predicate);
			}
			return expression;
		}

		public override DbExpression Visit(DbProjectExpression expression)
		{
			Check.NotNull(expression, "expression");
			DbExpression dbExpression = expression.Projection;
			if (DbExpressionKind.Deref == dbExpression.ExpressionKind)
			{
				dbExpression = ((DbDerefExpression)dbExpression).Argument;
			}
			if (DbExpressionKind.VariableReference == dbExpression.ExpressionKind && ((DbVariableReferenceExpression)dbExpression).VariableName.Equals(expression.Input.VariableName, StringComparison.Ordinal))
			{
				DbExpression dbExpression2 = Find(expression.Input.Expression);
				if (dbExpression2 != expression.Input.Expression)
				{
					return dbExpression2.BindAs(expression.Input.VariableName).Project(expression.Projection);
				}
			}
			return expression;
		}

		public override DbExpression Visit(DbSortExpression expression)
		{
			Check.NotNull(expression, "expression");
			DbExpression dbExpression = Find(expression.Input.Expression);
			if (dbExpression != expression.Input.Expression)
			{
				return dbExpression.BindAs(expression.Input.VariableName).Sort(expression.SortOrder);
			}
			return expression;
		}

		public override DbExpression Visit(DbSkipExpression expression)
		{
			Check.NotNull(expression, "expression");
			DbExpression dbExpression = Find(expression.Input.Expression);
			if (dbExpression != expression.Input.Expression)
			{
				return dbExpression.BindAs(expression.Input.VariableName).Skip(expression.SortOrder, expression.Count);
			}
			return expression;
		}
	}

	private int _spanCount;

	private SpanIndex _spanIndex;

	private readonly DbExpression _toRewrite;

	private bool _relationshipSpan;

	private readonly DbCommandTree _tree;

	private readonly Stack<NavigationInfo> _navSources = new Stack<NavigationInfo>();

	private readonly AliasGenerator _aliasGenerator;

	internal MetadataWorkspace Metadata => _tree.MetadataWorkspace;

	internal DbExpression Query => _toRewrite;

	internal bool RelationshipSpan
	{
		get
		{
			return _relationshipSpan;
		}
		set
		{
			_relationshipSpan = value;
		}
	}

	internal SpanIndex SpanIndex => _spanIndex;

	internal static bool EntityTypeEquals(EntityTypeBase entityType1, EntityTypeBase entityType2)
	{
		return entityType1 == entityType2;
	}

	internal static bool TryRewrite(DbQueryCommandTree tree, Span span, MergeOption mergeOption, AliasGenerator aliasGenerator, out DbExpression newQuery, out SpanIndex spanInfo)
	{
		newQuery = null;
		spanInfo = null;
		ObjectSpanRewriter objectSpanRewriter = null;
		bool flag = Span.RequiresRelationshipSpan(mergeOption);
		if (span != null && span.SpanList.Count > 0)
		{
			objectSpanRewriter = new ObjectFullSpanRewriter(tree, tree.Query, span, aliasGenerator);
		}
		else if (flag)
		{
			objectSpanRewriter = new ObjectSpanRewriter(tree, tree.Query, aliasGenerator);
		}
		if (objectSpanRewriter != null)
		{
			objectSpanRewriter.RelationshipSpan = flag;
			newQuery = objectSpanRewriter.RewriteQuery();
			if (newQuery != null)
			{
				spanInfo = objectSpanRewriter.SpanIndex;
			}
		}
		return spanInfo != null;
	}

	internal ObjectSpanRewriter(DbCommandTree tree, DbExpression toRewrite, AliasGenerator aliasGenerator)
	{
		_toRewrite = toRewrite;
		_tree = tree;
		_aliasGenerator = aliasGenerator;
	}

	internal DbExpression RewriteQuery()
	{
		DbExpression dbExpression = Rewrite(_toRewrite);
		if (_toRewrite == dbExpression)
		{
			return null;
		}
		return dbExpression;
	}

	internal SpanTrackingInfo InitializeTrackingInfo(bool createAssociationEndTrackingInfo)
	{
		SpanTrackingInfo result = default(SpanTrackingInfo);
		result.ColumnDefinitions = new List<KeyValuePair<string, DbExpression>>();
		result.ColumnNames = new AliasGenerator(string.Format(CultureInfo.InvariantCulture, "Span{0}_Column", new object[1] { _spanCount }));
		result.SpannedColumns = new Dictionary<int, AssociationEndMember>();
		if (createAssociationEndTrackingInfo)
		{
			result.FullSpannedEnds = new Dictionary<AssociationEndMember, bool>();
		}
		return result;
	}

	internal virtual SpanTrackingInfo CreateEntitySpanTrackingInfo(DbExpression expression, EntityType entityType)
	{
		return default(SpanTrackingInfo);
	}

	protected DbExpression Rewrite(DbExpression expression)
	{
		return expression.ExpressionKind switch
		{
			DbExpressionKind.Element => RewriteElementExpression((DbElementExpression)expression), 
			DbExpressionKind.Limit => RewriteLimitExpression((DbLimitExpression)expression), 
			_ => expression.ResultType.EdmType.BuiltInTypeKind switch
			{
				BuiltInTypeKind.EntityType => RewriteEntity(expression, (EntityType)expression.ResultType.EdmType), 
				BuiltInTypeKind.CollectionType => RewriteCollection(expression), 
				BuiltInTypeKind.RowType => RewriteRow(expression, (RowType)expression.ResultType.EdmType), 
				_ => expression, 
			}, 
		};
	}

	private void AddSpannedRowType(RowType spannedType, TypeUsage originalType)
	{
		if (_spanIndex == null)
		{
			_spanIndex = new SpanIndex();
		}
		_spanIndex.AddSpannedRowType(spannedType, originalType);
	}

	private void AddSpanMap(RowType rowType, Dictionary<int, AssociationEndMember> columnMap)
	{
		if (_spanIndex == null)
		{
			_spanIndex = new SpanIndex();
		}
		_spanIndex.AddSpanMap(rowType, columnMap);
	}

	private DbExpression RewriteEntity(DbExpression expression, EntityType entityType)
	{
		if (DbExpressionKind.NewInstance == expression.ExpressionKind)
		{
			return expression;
		}
		_spanCount++;
		int spanCount = _spanCount;
		SpanTrackingInfo spanTrackingInfo = CreateEntitySpanTrackingInfo(expression, entityType);
		List<KeyValuePair<AssociationEndMember, AssociationEndMember>> list = null;
		list = GetRelationshipSpanEnds(entityType);
		if (list != null)
		{
			if (spanTrackingInfo.ColumnDefinitions == null)
			{
				spanTrackingInfo = InitializeTrackingInfo(createAssociationEndTrackingInfo: false);
			}
			int num = spanTrackingInfo.ColumnDefinitions.Count + 1;
			foreach (KeyValuePair<AssociationEndMember, AssociationEndMember> item in list)
			{
				if (spanTrackingInfo.FullSpannedEnds == null || !spanTrackingInfo.FullSpannedEnds.ContainsKey(item.Value))
				{
					DbExpression source = null;
					if (!TryGetNavigationSource(item.Value, out source))
					{
						source = expression.GetEntityRef().NavigateAllowingAllRelationshipsInSameTypeHierarchy(item.Key, item.Value);
					}
					spanTrackingInfo.ColumnDefinitions.Add(new KeyValuePair<string, DbExpression>(spanTrackingInfo.ColumnNames.Next(), source));
					spanTrackingInfo.SpannedColumns[num] = item.Value;
					num++;
				}
			}
		}
		if (spanTrackingInfo.ColumnDefinitions == null)
		{
			_spanCount--;
			return expression;
		}
		spanTrackingInfo.ColumnDefinitions.Insert(0, new KeyValuePair<string, DbExpression>(string.Format(CultureInfo.InvariantCulture, "Span{0}_SpanRoot", new object[1] { spanCount }), expression));
		DbNewInstanceExpression dbNewInstanceExpression = DbExpressionBuilder.NewRow(spanTrackingInfo.ColumnDefinitions);
		RowType rowType = (RowType)dbNewInstanceExpression.ResultType.EdmType;
		AddSpanMap(rowType, spanTrackingInfo.SpannedColumns);
		return dbNewInstanceExpression;
	}

	private DbExpression RewriteElementExpression(DbElementExpression expression)
	{
		DbExpression dbExpression = Rewrite(expression.Argument);
		if (expression.Argument != dbExpression)
		{
			expression = dbExpression.Element();
		}
		return expression;
	}

	private DbExpression RewriteLimitExpression(DbLimitExpression expression)
	{
		DbExpression dbExpression = Rewrite(expression.Argument);
		if (expression.Argument != dbExpression)
		{
			expression = dbExpression.Limit(expression.Limit);
		}
		return expression;
	}

	private DbExpression RewriteRow(DbExpression expression, RowType rowType)
	{
		DbLambdaExpression dbLambdaExpression = expression as DbLambdaExpression;
		DbNewInstanceExpression dbNewInstanceExpression = ((dbLambdaExpression == null) ? (expression as DbNewInstanceExpression) : (dbLambdaExpression.Lambda.Body as DbNewInstanceExpression));
		Dictionary<int, DbExpression> dictionary = null;
		Dictionary<int, DbExpression> dictionary2 = null;
		for (int i = 0; i < rowType.Properties.Count; i++)
		{
			EdmProperty edmProperty = rowType.Properties[i];
			DbExpression dbExpression = null;
			dbExpression = ((dbNewInstanceExpression == null) ? expression.Property(edmProperty.Name) : dbNewInstanceExpression.Arguments[i]);
			DbExpression dbExpression2 = Rewrite(dbExpression);
			if (dbExpression2 != dbExpression)
			{
				if (dictionary2 == null)
				{
					dictionary2 = new Dictionary<int, DbExpression>();
				}
				dictionary2[i] = dbExpression2;
			}
			else
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<int, DbExpression>();
				}
				dictionary[i] = dbExpression;
			}
		}
		if (dictionary2 == null)
		{
			return expression;
		}
		List<DbExpression> list = new List<DbExpression>(rowType.Properties.Count);
		List<EdmProperty> list2 = new List<EdmProperty>(rowType.Properties.Count);
		for (int j = 0; j < rowType.Properties.Count; j++)
		{
			EdmProperty edmProperty2 = rowType.Properties[j];
			DbExpression value = null;
			if (!dictionary2.TryGetValue(j, out value))
			{
				value = dictionary[j];
			}
			list.Add(value);
			list2.Add(new EdmProperty(edmProperty2.Name, value.ResultType));
		}
		RowType rowType2 = new RowType(list2, rowType.InitializerMetadata);
		TypeUsage typeUsage = TypeUsage.Create(rowType2);
		DbExpression dbExpression3 = typeUsage.New(list);
		if (dbNewInstanceExpression == null)
		{
			DbExpression dbExpression4 = expression.IsNull();
			DbExpression dbExpression5 = typeUsage.Null();
			dbExpression3 = DbExpressionBuilder.Case(new List<DbExpression>(new DbExpression[1] { dbExpression4 }), new List<DbExpression>(new DbExpression[1] { dbExpression5 }), dbExpression3);
		}
		AddSpannedRowType(rowType2, expression.ResultType);
		if (dbLambdaExpression != null && dbNewInstanceExpression != null)
		{
			dbExpression3 = DbLambda.Create(dbExpression3, dbLambdaExpression.Lambda.Variables).Invoke(dbLambdaExpression.Arguments);
		}
		return dbExpression3;
	}

	private DbExpression RewriteCollection(DbExpression expression)
	{
		DbExpression dbExpression = expression;
		DbProjectExpression dbProjectExpression = null;
		if (DbExpressionKind.Project == expression.ExpressionKind)
		{
			dbProjectExpression = (DbProjectExpression)expression;
			dbExpression = dbProjectExpression.Input.Expression;
		}
		NavigationInfo navInfo = null;
		if (RelationshipSpan)
		{
			dbExpression = RelationshipNavigationVisitor.FindNavigationExpression(dbExpression, _aliasGenerator, out navInfo);
		}
		if (navInfo != null)
		{
			EnterNavigationCollection(navInfo);
		}
		else
		{
			EnterCollection();
		}
		DbExpression dbExpression2 = expression;
		if (dbProjectExpression != null)
		{
			DbExpression dbExpression3 = Rewrite(dbProjectExpression.Projection);
			if (dbProjectExpression.Projection != dbExpression3)
			{
				dbExpression2 = dbExpression.BindAs(dbProjectExpression.Input.VariableName).Project(dbExpression3);
			}
		}
		else
		{
			DbExpressionBinding dbExpressionBinding = dbExpression.BindAs(_aliasGenerator.Next());
			DbExpression variable = dbExpressionBinding.Variable;
			DbExpression dbExpression4 = Rewrite(variable);
			if (variable != dbExpression4)
			{
				dbExpression2 = dbExpressionBinding.Project(dbExpression4);
			}
		}
		ExitCollection();
		if (navInfo != null && navInfo.InUse)
		{
			List<DbVariableReferenceExpression> list = new List<DbVariableReferenceExpression>(1);
			list.Add(navInfo.SourceVariable);
			List<DbExpression> list2 = new List<DbExpression>(1);
			list2.Add(navInfo.Source);
			dbExpression2 = DbExpressionBuilder.Lambda(dbExpression2, list).Invoke(list2);
		}
		return dbExpression2;
	}

	private void EnterCollection()
	{
		_navSources.Push(null);
	}

	private void EnterNavigationCollection(NavigationInfo info)
	{
		_navSources.Push(info);
	}

	private void ExitCollection()
	{
		_navSources.Pop();
	}

	private bool TryGetNavigationSource(AssociationEndMember wasSourceNowTargetEnd, out DbExpression source)
	{
		source = null;
		NavigationInfo navigationInfo = null;
		if (_navSources.Count > 0)
		{
			navigationInfo = _navSources.Peek();
			if (navigationInfo != null && wasSourceNowTargetEnd != navigationInfo.SourceEnd)
			{
				navigationInfo = null;
			}
		}
		if (navigationInfo != null)
		{
			source = navigationInfo.SourceVariable;
			navigationInfo.InUse = true;
			return true;
		}
		return false;
	}

	private List<KeyValuePair<AssociationEndMember, AssociationEndMember>> GetRelationshipSpanEnds(EntityType entityType)
	{
		List<KeyValuePair<AssociationEndMember, AssociationEndMember>> list = null;
		if (_relationshipSpan)
		{
			foreach (AssociationType item in _tree.MetadataWorkspace.GetItems<AssociationType>(DataSpace.CSpace))
			{
				if (2 != item.AssociationEndMembers.Count)
				{
					continue;
				}
				AssociationEndMember associationEndMember = item.AssociationEndMembers[0];
				AssociationEndMember associationEndMember2 = item.AssociationEndMembers[1];
				if (IsValidRelationshipSpan(entityType, item, associationEndMember, associationEndMember2))
				{
					if (list == null)
					{
						list = new List<KeyValuePair<AssociationEndMember, AssociationEndMember>>();
					}
					list.Add(new KeyValuePair<AssociationEndMember, AssociationEndMember>(associationEndMember, associationEndMember2));
				}
				if (IsValidRelationshipSpan(entityType, item, associationEndMember2, associationEndMember))
				{
					if (list == null)
					{
						list = new List<KeyValuePair<AssociationEndMember, AssociationEndMember>>();
					}
					list.Add(new KeyValuePair<AssociationEndMember, AssociationEndMember>(associationEndMember2, associationEndMember));
				}
			}
		}
		return list;
	}

	private static bool IsValidRelationshipSpan(EntityType compareType, AssociationType associationType, AssociationEndMember fromEnd, AssociationEndMember toEnd)
	{
		if (!associationType.IsForeignKey && (RelationshipMultiplicity.One == toEnd.RelationshipMultiplicity || toEnd.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne))
		{
			EntityType entityType = (EntityType)((RefType)fromEnd.TypeUsage.EdmType).ElementType;
			if (!EntityTypeEquals(compareType, entityType) && !TypeSemantics.IsSubTypeOf(compareType, entityType))
			{
				return TypeSemantics.IsSubTypeOf(entityType, compareType);
			}
			return true;
		}
		return false;
	}
}
