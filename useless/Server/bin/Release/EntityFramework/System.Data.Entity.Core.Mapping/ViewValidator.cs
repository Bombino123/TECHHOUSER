using System.Collections.Generic;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.Utils;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Linq;

namespace System.Data.Entity.Core.Mapping;

internal static class ViewValidator
{
	private sealed class ViewExpressionValidator : BasicExpressionVisitor
	{
		private readonly EntitySetBaseMapping _setMapping;

		private readonly List<EdmSchemaError> _errors;

		private readonly EntityTypeBase _elementType;

		private readonly bool _includeSubtypes;

		private EdmItemCollection EdmItemCollection => _setMapping.EntityContainerMapping.StorageMappingItemCollection.EdmItemCollection;

		private StoreItemCollection StoreItemCollection => _setMapping.EntityContainerMapping.StorageMappingItemCollection.StoreItemCollection;

		internal IEnumerable<EdmSchemaError> Errors => _errors;

		internal ViewExpressionValidator(EntitySetBaseMapping setMapping, EntityTypeBase elementType, bool includeSubtypes)
		{
			_setMapping = setMapping;
			_elementType = elementType;
			_includeSubtypes = includeSubtypes;
			_errors = new List<EdmSchemaError>();
		}

		public override void VisitExpression(DbExpression expression)
		{
			Check.NotNull(expression, "expression");
			ValidateExpressionKind(expression.ExpressionKind);
			base.VisitExpression(expression);
		}

		private void ValidateExpressionKind(DbExpressionKind expressionKind)
		{
			switch (expressionKind)
			{
			case DbExpressionKind.And:
			case DbExpressionKind.Case:
			case DbExpressionKind.Cast:
			case DbExpressionKind.Constant:
			case DbExpressionKind.EntityRef:
			case DbExpressionKind.Equals:
			case DbExpressionKind.Filter:
			case DbExpressionKind.FullOuterJoin:
			case DbExpressionKind.Function:
			case DbExpressionKind.GreaterThan:
			case DbExpressionKind.GreaterThanOrEquals:
			case DbExpressionKind.InnerJoin:
			case DbExpressionKind.IsNull:
			case DbExpressionKind.LeftOuterJoin:
			case DbExpressionKind.LessThan:
			case DbExpressionKind.LessThanOrEquals:
			case DbExpressionKind.NewInstance:
			case DbExpressionKind.Not:
			case DbExpressionKind.NotEquals:
			case DbExpressionKind.Null:
			case DbExpressionKind.Or:
			case DbExpressionKind.Project:
			case DbExpressionKind.Property:
			case DbExpressionKind.Ref:
			case DbExpressionKind.Scan:
			case DbExpressionKind.UnionAll:
			case DbExpressionKind.VariableReference:
				return;
			}
			string p = (_includeSubtypes ? ("IsTypeOf(" + _elementType?.ToString() + ")") : _elementType.ToString());
			_errors.Add(new EdmSchemaError(Strings.Mapping_UnsupportedExpressionKind_QueryView(_setMapping.Set.Name, p, expressionKind), 2071, EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber, _setMapping.StartLinePosition));
		}

		public override void Visit(DbPropertyExpression expression)
		{
			Check.NotNull(expression, "expression");
			base.Visit(expression);
			if (expression.Property.BuiltInTypeKind != BuiltInTypeKind.EdmProperty)
			{
				_errors.Add(new EdmSchemaError(Strings.Mapping_UnsupportedPropertyKind_QueryView(_setMapping.Set.Name, expression.Property.Name, expression.Property.BuiltInTypeKind), 2073, EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber, _setMapping.StartLinePosition));
			}
		}

		public override void Visit(DbNewInstanceExpression expression)
		{
			Check.NotNull(expression, "expression");
			base.Visit(expression);
			EdmType edmType = expression.ResultType.EdmType;
			if (edmType.BuiltInTypeKind != BuiltInTypeKind.RowType && edmType != _elementType && (!_includeSubtypes || !_elementType.IsAssignableFrom(edmType)) && (edmType.BuiltInTypeKind != BuiltInTypeKind.ComplexType || !GetComplexTypes().Contains((ComplexType)edmType)))
			{
				_errors.Add(new EdmSchemaError(Strings.Mapping_UnsupportedInitialization_QueryView(_setMapping.Set.Name, edmType.FullName), 2074, EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber, _setMapping.StartLinePosition));
			}
		}

		private IEnumerable<ComplexType> GetComplexTypes()
		{
			IEnumerable<EdmProperty> properties = GetEntityTypes().SelectMany((EntityType entityType) => entityType.Properties).Distinct();
			return GetComplexTypes(properties);
		}

		private IEnumerable<ComplexType> GetComplexTypes(IEnumerable<EdmProperty> properties)
		{
			foreach (ComplexType complexType in properties.Select((EdmProperty p) => p.TypeUsage.EdmType).OfType<ComplexType>())
			{
				yield return complexType;
				foreach (ComplexType complexType2 in GetComplexTypes(complexType.Properties))
				{
					yield return complexType2;
				}
			}
		}

		private IEnumerable<EntityType> GetEntityTypes()
		{
			if (_includeSubtypes)
			{
				return MetadataHelper.GetTypeAndSubtypesOf(_elementType, EdmItemCollection, includeAbstractTypes: true).OfType<EntityType>();
			}
			if (_elementType.BuiltInTypeKind == BuiltInTypeKind.EntityType)
			{
				return Enumerable.Repeat((EntityType)_elementType, 1);
			}
			return Enumerable.Empty<EntityType>();
		}

		public override void Visit(DbFunctionExpression expression)
		{
			Check.NotNull(expression, "expression");
			base.Visit(expression);
			if (!IsStoreSpaceOrCanonicalFunction(StoreItemCollection, expression.Function))
			{
				_errors.Add(new EdmSchemaError(Strings.Mapping_UnsupportedFunctionCall_QueryView(_setMapping.Set.Name, expression.Function.Identity), 2112, EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber, _setMapping.StartLinePosition));
			}
		}

		internal static bool IsStoreSpaceOrCanonicalFunction(StoreItemCollection sSpace, EdmFunction function)
		{
			if (TypeHelpers.IsCanonicalFunction(function))
			{
				return true;
			}
			return sSpace.GetCTypeFunctions(function.FullName, ignoreCase: false).Contains(function);
		}

		public override void Visit(DbScanExpression expression)
		{
			Check.NotNull(expression, "expression");
			base.Visit(expression);
			EntitySetBase target = expression.Target;
			if (target.EntityContainer.DataSpace != DataSpace.SSpace)
			{
				_errors.Add(new EdmSchemaError(Strings.Mapping_UnsupportedScanTarget_QueryView(_setMapping.Set.Name, target.Name), 2072, EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber, _setMapping.StartLinePosition));
			}
		}
	}

	private class AssociationSetViewValidator : DbExpressionVisitor<DbExpressionEntitySetInfo>
	{
		private readonly Stack<KeyValuePair<string, DbExpressionEntitySetInfo>> variableScopes = new Stack<KeyValuePair<string, DbExpressionEntitySetInfo>>();

		private readonly EntitySetBaseMapping _setMapping;

		private readonly List<EdmSchemaError> _errors = new List<EdmSchemaError>();

		internal List<EdmSchemaError> Errors => _errors;

		internal AssociationSetViewValidator(EntitySetBaseMapping setMapping)
		{
			_setMapping = setMapping;
		}

		internal DbExpressionEntitySetInfo VisitExpression(DbExpression expression)
		{
			return expression.Accept(this);
		}

		private DbExpressionEntitySetInfo VisitExpressionBinding(DbExpressionBinding binding)
		{
			if (binding != null)
			{
				return VisitExpression(binding.Expression);
			}
			return null;
		}

		private void VisitExpressionBindingEnterScope(DbExpressionBinding binding)
		{
			DbExpressionEntitySetInfo value = VisitExpressionBinding(binding);
			variableScopes.Push(new KeyValuePair<string, DbExpressionEntitySetInfo>(binding.VariableName, value));
		}

		private void VisitExpressionBindingExitScope()
		{
			variableScopes.Pop();
		}

		private void ValidateEntitySetsMappedForAssociationSetMapping(DbExpressionStructuralTypeEntitySetInfo setInfos)
		{
			AssociationSet associationSet = _setMapping.Set as AssociationSet;
			int num = 0;
			if (!setInfos.SetInfos.All((KeyValuePair<string, DbExpressionEntitySetInfo> it) => it.Value != null && it.Value is DbExpressionSimpleTypeEntitySetInfo) || setInfos.SetInfos.Count() != 2)
			{
				return;
			}
			foreach (DbExpressionSimpleTypeEntitySetInfo item in setInfos.SetInfos.Select((KeyValuePair<string, DbExpressionEntitySetInfo> it) => it.Value))
			{
				AssociationSetEnd associationSetEnd = associationSet.AssociationSetEnds[num];
				EntitySet entitySet = associationSetEnd.EntitySet;
				if (!entitySet.Equals(item.EntitySet))
				{
					_errors.Add(new EdmSchemaError(Strings.Mapping_EntitySetMismatchOnAssociationSetEnd_QueryView(item.EntitySet.Name, entitySet.Name, associationSetEnd.Name, _setMapping.Set.Name), 2074, EdmSchemaErrorSeverity.Error, _setMapping.EntityContainerMapping.SourceLocation, _setMapping.StartLineNumber, _setMapping.StartLinePosition));
				}
				num++;
			}
		}

		public override DbExpressionEntitySetInfo Visit(DbExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbVariableReferenceExpression expression)
		{
			Check.NotNull(expression, "expression");
			return (from it in variableScopes
				where it.Key == expression.VariableName
				select it.Value).FirstOrDefault();
		}

		public override DbExpressionEntitySetInfo Visit(DbPropertyExpression expression)
		{
			Check.NotNull(expression, "expression");
			if (VisitExpression(expression.Instance) is DbExpressionStructuralTypeEntitySetInfo dbExpressionStructuralTypeEntitySetInfo)
			{
				return dbExpressionStructuralTypeEntitySetInfo.GetEntitySetInfoForMember(expression.Property.Name);
			}
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbProjectExpression expression)
		{
			Check.NotNull(expression, "expression");
			VisitExpressionBindingEnterScope(expression.Input);
			DbExpressionEntitySetInfo result = VisitExpression(expression.Projection);
			VisitExpressionBindingExitScope();
			return result;
		}

		public override DbExpressionEntitySetInfo Visit(DbNewInstanceExpression expression)
		{
			Check.NotNull(expression, "expression");
			DbExpressionMemberCollectionEntitySetInfo dbExpressionMemberCollectionEntitySetInfo = VisitExpressionList(expression.Arguments);
			StructuralType structuralType = expression.ResultType.EdmType as StructuralType;
			if (dbExpressionMemberCollectionEntitySetInfo != null && structuralType != null)
			{
				DbExpressionStructuralTypeEntitySetInfo dbExpressionStructuralTypeEntitySetInfo = new DbExpressionStructuralTypeEntitySetInfo();
				int num = 0;
				foreach (DbExpressionEntitySetInfo entitySetInfo in dbExpressionMemberCollectionEntitySetInfo.entitySetInfos)
				{
					dbExpressionStructuralTypeEntitySetInfo.Add(structuralType.Members[num].Name, entitySetInfo);
					num++;
				}
				if (expression.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.AssociationType)
				{
					ValidateEntitySetsMappedForAssociationSetMapping(dbExpressionStructuralTypeEntitySetInfo);
				}
				return dbExpressionStructuralTypeEntitySetInfo;
			}
			return null;
		}

		private DbExpressionMemberCollectionEntitySetInfo VisitExpressionList(IList<DbExpression> list)
		{
			return new DbExpressionMemberCollectionEntitySetInfo(list.Select((DbExpression it) => VisitExpression(it)));
		}

		public override DbExpressionEntitySetInfo Visit(DbRefExpression expression)
		{
			Check.NotNull(expression, "expression");
			return new DbExpressionSimpleTypeEntitySetInfo(expression.EntitySet);
		}

		public override DbExpressionEntitySetInfo Visit(DbComparisonExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbLikeExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbLimitExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbIsNullExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbArithmeticExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbAndExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbOrExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbInExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbNotExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbDistinctExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbElementExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbIsEmptyExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbUnionAllExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbIntersectExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbExceptExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbTreatExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbIsOfExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbCastExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbCaseExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbOfTypeExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbRelationshipNavigationExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbDerefExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbRefKeyExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbEntityRefExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbScanExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbFilterExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbConstantExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbNullExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbCrossJoinExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbJoinExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbParameterReferenceExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbFunctionExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbLambdaExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbApplyExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbGroupByExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbSkipExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbSortExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}

		public override DbExpressionEntitySetInfo Visit(DbQuantifierExpression expression)
		{
			Check.NotNull(expression, "expression");
			return null;
		}
	}

	internal abstract class DbExpressionEntitySetInfo
	{
	}

	private class DbExpressionSimpleTypeEntitySetInfo : DbExpressionEntitySetInfo
	{
		private readonly EntitySet m_entitySet;

		internal EntitySet EntitySet => m_entitySet;

		internal DbExpressionSimpleTypeEntitySetInfo(EntitySet entitySet)
		{
			m_entitySet = entitySet;
		}
	}

	private class DbExpressionStructuralTypeEntitySetInfo : DbExpressionEntitySetInfo
	{
		private readonly Dictionary<string, DbExpressionEntitySetInfo> m_entitySetInfos;

		internal IEnumerable<KeyValuePair<string, DbExpressionEntitySetInfo>> SetInfos => m_entitySetInfos;

		internal DbExpressionStructuralTypeEntitySetInfo()
		{
			m_entitySetInfos = new Dictionary<string, DbExpressionEntitySetInfo>();
		}

		internal void Add(string key, DbExpressionEntitySetInfo value)
		{
			m_entitySetInfos.Add(key, value);
		}

		internal DbExpressionEntitySetInfo GetEntitySetInfoForMember(string memberName)
		{
			return m_entitySetInfos[memberName];
		}
	}

	private class DbExpressionMemberCollectionEntitySetInfo : DbExpressionEntitySetInfo
	{
		private readonly IEnumerable<DbExpressionEntitySetInfo> m_entitySets;

		internal IEnumerable<DbExpressionEntitySetInfo> entitySetInfos => m_entitySets;

		internal DbExpressionMemberCollectionEntitySetInfo(IEnumerable<DbExpressionEntitySetInfo> entitySetInfos)
		{
			m_entitySets = entitySetInfos;
		}
	}

	internal static IEnumerable<EdmSchemaError> ValidateQueryView(DbQueryCommandTree view, EntitySetBaseMapping setMapping, EntityTypeBase elementType, bool includeSubtypes)
	{
		ViewExpressionValidator viewExpressionValidator = new ViewExpressionValidator(setMapping, elementType, includeSubtypes);
		viewExpressionValidator.VisitExpression(view.Query);
		if (viewExpressionValidator.Errors.Count() == 0 && setMapping.Set.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)
		{
			AssociationSetViewValidator associationSetViewValidator = new AssociationSetViewValidator(setMapping);
			associationSetViewValidator.VisitExpression(view.Query);
			return associationSetViewValidator.Errors;
		}
		return viewExpressionValidator.Errors;
	}
}
