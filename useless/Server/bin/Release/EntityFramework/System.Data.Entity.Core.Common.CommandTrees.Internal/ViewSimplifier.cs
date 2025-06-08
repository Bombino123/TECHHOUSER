using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal class ViewSimplifier
{
	private class ValueSubstituter : DefaultExpressionVisitor
	{
		private readonly string variableName;

		private readonly Dictionary<string, DbExpression> replacements;

		internal static DbExpression Substitute(DbExpression original, string referencedVariable, Dictionary<string, DbExpression> propertyValues)
		{
			return new ValueSubstituter(referencedVariable, propertyValues).VisitExpression(original);
		}

		private ValueSubstituter(string varName, Dictionary<string, DbExpression> replValues)
		{
			variableName = varName;
			replacements = replValues;
		}

		public override DbExpression Visit(DbPropertyExpression expression)
		{
			Check.NotNull(expression, "expression");
			DbExpression dbExpression = null;
			if (expression.Instance.ExpressionKind == DbExpressionKind.VariableReference && ((DbVariableReferenceExpression)expression.Instance).VariableName == variableName && replacements.TryGetValue(expression.Property.Name, out var value))
			{
				return value;
			}
			return base.Visit(expression);
		}
	}

	private class ProjectionCollapser : DefaultExpressionVisitor
	{
		private readonly Dictionary<string, DbExpression> m_varRefMemberBindings;

		private readonly DbExpressionBinding m_outerBinding;

		private bool m_doomed;

		internal bool IsDoomed => m_doomed;

		internal ProjectionCollapser(Dictionary<string, DbExpression> varRefMemberBindings, DbExpressionBinding outerBinding)
		{
			m_varRefMemberBindings = varRefMemberBindings;
			m_outerBinding = outerBinding;
		}

		internal DbExpression CollapseProjection(DbExpression expression)
		{
			return VisitExpression(expression);
		}

		public override DbExpression Visit(DbPropertyExpression property)
		{
			Check.NotNull(property, "property");
			if (property.Instance.ExpressionKind == DbExpressionKind.VariableReference && IsOuterBindingVarRef((DbVariableReferenceExpression)property.Instance))
			{
				return m_varRefMemberBindings[property.Property.Name];
			}
			return base.Visit(property);
		}

		public override DbExpression Visit(DbVariableReferenceExpression varRef)
		{
			Check.NotNull(varRef, "varRef");
			if (IsOuterBindingVarRef(varRef))
			{
				m_doomed = true;
			}
			return base.Visit(varRef);
		}

		private bool IsOuterBindingVarRef(DbVariableReferenceExpression varRef)
		{
			return varRef.VariableName == m_outerBinding.VariableName;
		}
	}

	private readonly EntitySetBase extent;

	private static readonly Func<DbExpression, bool> _patternEntityConstructor = Patterns.MatchProject(Patterns.AnyExpression, Patterns.And(Patterns.MatchEntityType, Patterns.Or(Patterns.MatchNewInstance(), Patterns.MatchCase(Patterns.AnyExpressions, Patterns.MatchForAll(Patterns.MatchNewInstance()), Patterns.MatchNewInstance()))));

	private bool doNotProcess;

	private static readonly Func<DbExpression, bool> _patternNestedTphDiscriminator = Patterns.MatchProject(Patterns.MatchFilter(Patterns.MatchProject(Patterns.MatchFilter(Patterns.AnyExpression, Patterns.Or(Patterns.MatchKind(DbExpressionKind.Equals), Patterns.MatchKind(DbExpressionKind.Or))), Patterns.And(Patterns.MatchRowType, Patterns.MatchNewInstance(Patterns.MatchForAll(Patterns.Or(Patterns.And(Patterns.MatchNewInstance(), Patterns.MatchComplexType), Patterns.MatchKind(DbExpressionKind.Property), Patterns.MatchKind(DbExpressionKind.Case)))))), Patterns.Or(Patterns.MatchKind(DbExpressionKind.Property), Patterns.MatchKind(DbExpressionKind.Or))), Patterns.And(Patterns.MatchEntityType, Patterns.MatchCase(Patterns.MatchForAll(Patterns.MatchKind(DbExpressionKind.Property)), Patterns.MatchForAll(Patterns.MatchKind(DbExpressionKind.NewInstance)), Patterns.MatchKind(DbExpressionKind.NewInstance))));

	private static readonly Func<DbExpression, bool> _patternCase = Patterns.MatchKind(DbExpressionKind.Case);

	private static readonly Func<DbExpression, bool> _patternCollapseNestedProjection = Patterns.MatchProject(Patterns.MatchProject(Patterns.AnyExpression, Patterns.MatchKind(DbExpressionKind.NewInstance)), Patterns.AnyExpression);

	internal static DbQueryCommandTree SimplifyView(EntitySetBase extent, DbQueryCommandTree view)
	{
		view = new ViewSimplifier(extent).Simplify(view);
		return view;
	}

	private ViewSimplifier(EntitySetBase viewTarget)
	{
		extent = viewTarget;
	}

	private DbQueryCommandTree Simplify(DbQueryCommandTree view)
	{
		Func<DbExpression, DbExpression> func = PatternMatchRuleProcessor.Create(PatternMatchRule.Create(_patternCollapseNestedProjection, CollapseNestedProjection), PatternMatchRule.Create(_patternCase, SimplifyCaseStatement), PatternMatchRule.Create(_patternNestedTphDiscriminator, SimplifyNestedTphDiscriminator), PatternMatchRule.Create(_patternEntityConstructor, AddFkRelatedEntityRefs));
		DbExpression query = view.Query;
		query = func(query);
		view = DbQueryCommandTree.FromValidExpression(view.MetadataWorkspace, view.DataSpace, query, view.UseDatabaseNullSemantics, view.DisableFilterOverProjectionSimplificationForCustomFunctions);
		return view;
	}

	private DbExpression AddFkRelatedEntityRefs(DbExpression viewConstructor)
	{
		if (doNotProcess)
		{
			return null;
		}
		if (extent.BuiltInTypeKind != BuiltInTypeKind.EntitySet || extent.EntityContainer.DataSpace != DataSpace.CSpace)
		{
			doNotProcess = true;
			return null;
		}
		EntitySet targetSet = (EntitySet)extent;
		List<AssociationSet> list = (from AssociationSet assocSet in targetSet.EntityContainer.BaseEntitySets.Where((EntitySetBase es) => es.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
			where assocSet.ElementType.IsForeignKey && assocSet.AssociationSetEnds.Any((AssociationSetEnd se) => se.EntitySet == targetSet)
			select assocSet).ToList();
		if (list.Count == 0)
		{
			doNotProcess = true;
			return null;
		}
		HashSet<Tuple<EntityType, AssociationSetEnd, ReferentialConstraint>> hashSet = new HashSet<Tuple<EntityType, AssociationSetEnd, ReferentialConstraint>>();
		foreach (AssociationSet item3 in list)
		{
			ReferentialConstraint referentialConstraint = item3.ElementType.ReferentialConstraints[0];
			AssociationSetEnd associationSetEnd = item3.AssociationSetEnds[referentialConstraint.ToRole.Name];
			if (associationSetEnd.EntitySet == targetSet)
			{
				EntityType item = (EntityType)TypeHelpers.GetEdmType<RefType>(associationSetEnd.CorrespondingAssociationEndMember.TypeUsage).ElementType;
				AssociationSetEnd item2 = item3.AssociationSetEnds[referentialConstraint.FromRole.Name];
				hashSet.Add(Tuple.Create(item, item2, referentialConstraint));
			}
		}
		if (hashSet.Count == 0)
		{
			doNotProcess = true;
			return null;
		}
		DbProjectExpression dbProjectExpression = (DbProjectExpression)viewConstructor;
		List<DbNewInstanceExpression> list2 = new List<DbNewInstanceExpression>();
		List<DbExpression> list3 = null;
		if (dbProjectExpression.Projection.ExpressionKind == DbExpressionKind.Case)
		{
			DbCaseExpression dbCaseExpression = (DbCaseExpression)dbProjectExpression.Projection;
			list3 = new List<DbExpression>(dbCaseExpression.When.Count);
			for (int i = 0; i < dbCaseExpression.When.Count; i++)
			{
				list3.Add(dbCaseExpression.When[i]);
				list2.Add((DbNewInstanceExpression)dbCaseExpression.Then[i]);
			}
			list2.Add((DbNewInstanceExpression)dbCaseExpression.Else);
		}
		else
		{
			list2.Add((DbNewInstanceExpression)dbProjectExpression.Projection);
		}
		bool flag = false;
		for (int j = 0; j < list2.Count; j++)
		{
			DbNewInstanceExpression entityConstructor = list2[j];
			EntityType constructedEntityType = TypeHelpers.GetEdmType<EntityType>(entityConstructor.ResultType);
			List<DbRelatedEntityRef> list4 = (from psdt in hashSet
				where constructedEntityType == psdt.Item1 || constructedEntityType.IsSubtypeOf(psdt.Item1)
				select RelatedEntityRefFromAssociationSetEnd(constructedEntityType, entityConstructor, psdt.Item2, psdt.Item3)).ToList();
			if (list4.Count > 0)
			{
				if (entityConstructor.HasRelatedEntityReferences)
				{
					list4 = entityConstructor.RelatedEntityReferences.Concat(list4).ToList();
				}
				entityConstructor = DbExpressionBuilder.CreateNewEntityWithRelationshipsExpression(constructedEntityType, entityConstructor.Arguments, list4);
				list2[j] = entityConstructor;
				flag = true;
			}
		}
		DbExpression result = null;
		if (flag)
		{
			if (list3 != null)
			{
				List<DbExpression> list5 = new List<DbExpression>(list3.Count);
				List<DbExpression> list6 = new List<DbExpression>(list3.Count);
				for (int k = 0; k < list3.Count; k++)
				{
					list5.Add(list3[k]);
					list6.Add(list2[k]);
				}
				result = dbProjectExpression.Input.Project(DbExpressionBuilder.Case(list5, list6, list2[list3.Count]));
			}
			else
			{
				result = dbProjectExpression.Input.Project(list2[0]);
			}
		}
		doNotProcess = true;
		return result;
	}

	private static DbRelatedEntityRef RelatedEntityRefFromAssociationSetEnd(EntityType constructedEntityType, DbNewInstanceExpression entityConstructor, AssociationSetEnd principalSetEnd, ReferentialConstraint fkConstraint)
	{
		EntityType entityType = (EntityType)TypeHelpers.GetEdmType<RefType>(fkConstraint.FromRole.TypeUsage).ElementType;
		IList<DbExpression> list = null;
		IEnumerable<Tuple<string, DbExpression>> source = from pv in constructedEntityType.Properties.Select((EdmProperty p, int idx) => Tuple.Create(p, entityConstructor.Arguments[idx]))
			join ft in fkConstraint.FromProperties.Select((EdmProperty fp, int idx) => Tuple.Create(fp, fkConstraint.ToProperties[idx])) on pv.Item1 equals ft.Item2
			select Tuple.Create(ft.Item1.Name, pv.Item2);
		if (fkConstraint.FromProperties.Count == 1)
		{
			Tuple<string, DbExpression> tuple = source.Single();
			list = new DbExpression[1] { tuple.Item2 };
		}
		else
		{
			Dictionary<string, DbExpression> keyValueMap = source.ToDictionary<Tuple<string, DbExpression>, string, DbExpression>((Tuple<string, DbExpression> pav) => pav.Item1, (Tuple<string, DbExpression> pav) => pav.Item2, StringComparer.Ordinal);
			list = entityType.KeyMemberNames.Select((string memberName) => keyValueMap[memberName]).ToList();
		}
		DbRefExpression targetEntity = principalSetEnd.EntitySet.CreateRef(entityType, list);
		return DbExpressionBuilder.CreateRelatedEntityRef(fkConstraint.ToRole, fkConstraint.FromRole, targetEntity);
	}

	private static DbExpression SimplifyNestedTphDiscriminator(DbExpression expression)
	{
		DbProjectExpression dbProjectExpression = (DbProjectExpression)expression;
		DbFilterExpression booleanColumnFilter = (DbFilterExpression)dbProjectExpression.Input.Expression;
		DbProjectExpression dbProjectExpression2 = (DbProjectExpression)booleanColumnFilter.Input.Expression;
		DbFilterExpression dbFilterExpression = (DbFilterExpression)dbProjectExpression2.Input.Expression;
		List<DbExpression> list = FlattenOr(booleanColumnFilter.Predicate).ToList();
		List<DbPropertyExpression> list2 = (from px in list.OfType<DbPropertyExpression>()
			where px.Instance.ExpressionKind == DbExpressionKind.VariableReference && ((DbVariableReferenceExpression)px.Instance).VariableName == booleanColumnFilter.Input.VariableName
			select px).ToList();
		if (list.Count != list2.Count)
		{
			return null;
		}
		List<string> list3 = list2.Select((DbPropertyExpression px) => px.Property.Name).ToList();
		Dictionary<object, DbComparisonExpression> discriminatorPredicates = new Dictionary<object, DbComparisonExpression>();
		if (!TypeSemantics.IsEntityType(dbFilterExpression.Input.VariableType) || !TryMatchDiscriminatorPredicate(dbFilterExpression, delegate(DbComparisonExpression compEx, object discValue)
		{
			discriminatorPredicates.Add(discValue, compEx);
		}))
		{
			return null;
		}
		EdmProperty edmProperty = (EdmProperty)((DbPropertyExpression)discriminatorPredicates.First().Value.Left).Property;
		DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)dbProjectExpression2.Projection;
		RowType edmType = TypeHelpers.GetEdmType<RowType>(dbNewInstanceExpression.ResultType);
		Dictionary<string, DbComparisonExpression> dictionary = new Dictionary<string, DbComparisonExpression>();
		Dictionary<string, DbComparisonExpression> dictionary2 = new Dictionary<string, DbComparisonExpression>();
		Dictionary<string, DbExpression> dictionary3 = new Dictionary<string, DbExpression>(dbNewInstanceExpression.Arguments.Count);
		for (int i = 0; i < dbNewInstanceExpression.Arguments.Count; i++)
		{
			string name = edmType.Properties[i].Name;
			DbExpression dbExpression = dbNewInstanceExpression.Arguments[i];
			if (list3.Contains(name))
			{
				if (dbExpression.ExpressionKind != DbExpressionKind.Case)
				{
					return null;
				}
				DbCaseExpression dbCaseExpression = (DbCaseExpression)dbExpression;
				if (dbCaseExpression.When.Count != 1 || !TypeSemantics.IsBooleanType(dbCaseExpression.Then[0].ResultType) || !TypeSemantics.IsBooleanType(dbCaseExpression.Else.ResultType) || dbCaseExpression.Then[0].ExpressionKind != DbExpressionKind.Constant || dbCaseExpression.Else.ExpressionKind != DbExpressionKind.Constant || !(bool)((DbConstantExpression)dbCaseExpression.Then[0]).Value || (bool)((DbConstantExpression)dbCaseExpression.Else).Value)
				{
					return null;
				}
				if (!TryMatchPropertyEqualsValue(dbCaseExpression.When[0], dbProjectExpression2.Input.VariableName, out var property, out var value) || property.Property != edmProperty || !discriminatorPredicates.ContainsKey(value))
				{
					return null;
				}
				dictionary.Add(name, discriminatorPredicates[value]);
				dictionary2.Add(name, (DbComparisonExpression)dbCaseExpression.When[0]);
			}
			else
			{
				dictionary3.Add(name, dbExpression);
			}
		}
		DbExpression predicate = Helpers.BuildBalancedTreeInPlace(new List<DbExpression>(dictionary.Values), (DbExpression left, DbExpression right) => left.Or(right));
		dbFilterExpression = dbFilterExpression.Input.Filter(predicate);
		DbCaseExpression dbCaseExpression2 = (DbCaseExpression)dbProjectExpression.Projection;
		List<DbExpression> list4 = new List<DbExpression>(dbCaseExpression2.When.Count);
		List<DbExpression> list5 = new List<DbExpression>(dbCaseExpression2.Then.Count);
		for (int j = 0; j < dbCaseExpression2.When.Count; j++)
		{
			DbPropertyExpression dbPropertyExpression = (DbPropertyExpression)dbCaseExpression2.When[j];
			DbNewInstanceExpression original = (DbNewInstanceExpression)dbCaseExpression2.Then[j];
			if (!dictionary2.TryGetValue(dbPropertyExpression.Property.Name, out var value2))
			{
				return null;
			}
			list4.Add(value2);
			DbExpression item = ValueSubstituter.Substitute(original, dbProjectExpression.Input.VariableName, dictionary3);
			list5.Add(item);
		}
		DbExpression elseExpression = ValueSubstituter.Substitute(dbCaseExpression2.Else, dbProjectExpression.Input.VariableName, dictionary3);
		DbCaseExpression projection = DbExpressionBuilder.Case(list4, list5, elseExpression);
		return dbFilterExpression.BindAs(dbProjectExpression2.Input.VariableName).Project(projection);
	}

	private static DbExpression SimplifyCaseStatement(DbExpression expression)
	{
		DbCaseExpression dbCaseExpression = (DbCaseExpression)expression;
		bool flag = false;
		List<DbExpression> list = new List<DbExpression>(dbCaseExpression.When.Count);
		foreach (DbExpression item in dbCaseExpression.When)
		{
			if (TrySimplifyPredicate(item, out var simplified))
			{
				list.Add(simplified);
				flag = true;
			}
			else
			{
				list.Add(item);
			}
		}
		if (!flag)
		{
			return null;
		}
		return DbExpressionBuilder.Case(list, dbCaseExpression.Then, dbCaseExpression.Else);
	}

	private static bool TrySimplifyPredicate(DbExpression predicate, out DbExpression simplified)
	{
		simplified = null;
		if (predicate.ExpressionKind != DbExpressionKind.Case)
		{
			return false;
		}
		DbCaseExpression dbCaseExpression = (DbCaseExpression)predicate;
		if (dbCaseExpression.Then.Count != 1 && dbCaseExpression.Then[0].ExpressionKind == DbExpressionKind.Constant)
		{
			return false;
		}
		DbConstantExpression dbConstantExpression = (DbConstantExpression)dbCaseExpression.Then[0];
		if (!true.Equals(dbConstantExpression.Value))
		{
			return false;
		}
		if (dbCaseExpression.Else != null)
		{
			if (dbCaseExpression.Else.ExpressionKind != DbExpressionKind.Constant)
			{
				return false;
			}
			DbConstantExpression dbConstantExpression2 = (DbConstantExpression)dbCaseExpression.Else;
			if (true.Equals(dbConstantExpression2.Value))
			{
				return false;
			}
		}
		simplified = dbCaseExpression.When[0];
		return true;
	}

	private static DbExpression CollapseNestedProjection(DbExpression expression)
	{
		DbProjectExpression dbProjectExpression = (DbProjectExpression)expression;
		DbExpression projection = dbProjectExpression.Projection;
		DbProjectExpression dbProjectExpression2 = (DbProjectExpression)dbProjectExpression.Input.Expression;
		DbNewInstanceExpression dbNewInstanceExpression = (DbNewInstanceExpression)dbProjectExpression2.Projection;
		Dictionary<string, DbExpression> dictionary = new Dictionary<string, DbExpression>(dbNewInstanceExpression.Arguments.Count);
		RowType rowType = (RowType)dbNewInstanceExpression.ResultType.EdmType;
		for (int i = 0; i < rowType.Members.Count; i++)
		{
			dictionary[rowType.Members[i].Name] = dbNewInstanceExpression.Arguments[i];
		}
		ProjectionCollapser projectionCollapser = new ProjectionCollapser(dictionary, dbProjectExpression.Input);
		DbExpression projection2 = projectionCollapser.CollapseProjection(projection);
		if (projectionCollapser.IsDoomed)
		{
			return null;
		}
		return dbProjectExpression2.Input.Project(projection2);
	}

	internal static IEnumerable<DbExpression> FlattenOr(DbExpression expression)
	{
		return Helpers.GetLeafNodes(expression, (DbExpression exp) => exp.ExpressionKind != DbExpressionKind.Or, delegate(DbExpression exp)
		{
			DbOrExpression dbOrExpression = (DbOrExpression)exp;
			return new DbExpression[2] { dbOrExpression.Left, dbOrExpression.Right };
		});
	}

	internal static bool TryMatchDiscriminatorPredicate(DbFilterExpression filter, Action<DbComparisonExpression, object> onMatchedComparison)
	{
		EdmProperty edmProperty = null;
		foreach (DbExpression item in FlattenOr(filter.Predicate))
		{
			if (!TryMatchPropertyEqualsValue(item, filter.Input.VariableName, out var property, out var value))
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
			onMatchedComparison((DbComparisonExpression)item, value);
		}
		return true;
	}

	internal static bool TryMatchPropertyEqualsValue(DbExpression expression, string propertyVariable, out DbPropertyExpression property, out object value)
	{
		property = null;
		value = null;
		if (expression.ExpressionKind != DbExpressionKind.Equals)
		{
			return false;
		}
		DbBinaryExpression dbBinaryExpression = (DbBinaryExpression)expression;
		if (dbBinaryExpression.Left.ExpressionKind != DbExpressionKind.Property)
		{
			return false;
		}
		property = (DbPropertyExpression)dbBinaryExpression.Left;
		if (!TryMatchConstant(dbBinaryExpression.Right, out value))
		{
			return false;
		}
		if (property.Instance.ExpressionKind != DbExpressionKind.VariableReference || ((DbVariableReferenceExpression)property.Instance).VariableName != propertyVariable)
		{
			return false;
		}
		return true;
	}

	private static bool TryMatchConstant(DbExpression expression, out object value)
	{
		if (expression.ExpressionKind == DbExpressionKind.Constant)
		{
			value = ((DbConstantExpression)expression).Value;
			return true;
		}
		if (expression.ExpressionKind == DbExpressionKind.Cast && expression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType && TryMatchConstant(((DbCastExpression)expression).Argument, out value))
		{
			PrimitiveType primitiveType = (PrimitiveType)expression.ResultType.EdmType;
			value = Convert.ChangeType(value, primitiveType.ClrEquivalentType, CultureInfo.InvariantCulture);
			return true;
		}
		value = null;
		return false;
	}
}
