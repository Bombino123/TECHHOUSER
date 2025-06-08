using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;

namespace System.Data.Entity.Migrations.Infrastructure;

internal class DynamicToFunctionModificationCommandConverter : DefaultExpressionVisitor
{
	private readonly EntityTypeModificationFunctionMapping _entityTypeModificationFunctionMapping;

	private readonly AssociationSetModificationFunctionMapping _associationSetModificationFunctionMapping;

	private readonly EntityContainerMapping _entityContainerMapping;

	private ModificationFunctionMapping _currentFunctionMapping;

	private EdmProperty _currentProperty;

	private List<EdmProperty> _storeGeneratedKeys;

	private int _nextStoreGeneratedKey;

	private bool _useOriginalValues;

	public DynamicToFunctionModificationCommandConverter(EntityTypeModificationFunctionMapping entityTypeModificationFunctionMapping, EntityContainerMapping entityContainerMapping)
	{
		_entityTypeModificationFunctionMapping = entityTypeModificationFunctionMapping;
		_entityContainerMapping = entityContainerMapping;
	}

	public DynamicToFunctionModificationCommandConverter(AssociationSetModificationFunctionMapping associationSetModificationFunctionMapping, EntityContainerMapping entityContainerMapping)
	{
		_associationSetModificationFunctionMapping = associationSetModificationFunctionMapping;
		_entityContainerMapping = entityContainerMapping;
	}

	public IEnumerable<TCommandTree> Convert<TCommandTree>(IEnumerable<TCommandTree> modificationCommandTrees) where TCommandTree : DbModificationCommandTree
	{
		_currentFunctionMapping = null;
		_currentProperty = null;
		_storeGeneratedKeys = null;
		_nextStoreGeneratedKey = 0;
		return modificationCommandTrees.Select((TCommandTree modificationCommandTree) => ConvertInternal((dynamic)modificationCommandTree)).Cast<TCommandTree>();
	}

	private DbModificationCommandTree ConvertInternal(DbInsertCommandTree commandTree)
	{
		if (_currentFunctionMapping == null)
		{
			_currentFunctionMapping = ((_entityTypeModificationFunctionMapping != null) ? _entityTypeModificationFunctionMapping.InsertFunctionMapping : _associationSetModificationFunctionMapping.InsertFunctionMapping);
			EntityTypeBase elementType = ((DbScanExpression)commandTree.Target.Expression).Target.ElementType;
			_storeGeneratedKeys = elementType.KeyProperties.Where((EdmProperty p) => p.IsStoreGeneratedIdentity).ToList();
		}
		_nextStoreGeneratedKey = 0;
		return new DbInsertCommandTree(commandTree.MetadataWorkspace, commandTree.DataSpace, commandTree.Target, VisitSetClauses(commandTree.SetClauses), (commandTree.Returning != null) ? commandTree.Returning.Accept(this) : null);
	}

	private DbModificationCommandTree ConvertInternal(DbUpdateCommandTree commandTree)
	{
		_currentFunctionMapping = _entityTypeModificationFunctionMapping.UpdateFunctionMapping;
		_useOriginalValues = true;
		DbExpression predicate = commandTree.Predicate.Accept(this);
		_useOriginalValues = false;
		return new DbUpdateCommandTree(commandTree.MetadataWorkspace, commandTree.DataSpace, commandTree.Target, predicate, VisitSetClauses(commandTree.SetClauses), (commandTree.Returning != null) ? commandTree.Returning.Accept(this) : null);
	}

	private DbModificationCommandTree ConvertInternal(DbDeleteCommandTree commandTree)
	{
		_currentFunctionMapping = ((_entityTypeModificationFunctionMapping != null) ? _entityTypeModificationFunctionMapping.DeleteFunctionMapping : _associationSetModificationFunctionMapping.DeleteFunctionMapping);
		return new DbDeleteCommandTree(commandTree.MetadataWorkspace, commandTree.DataSpace, commandTree.Target, commandTree.Predicate.Accept(this));
	}

	private ReadOnlyCollection<DbModificationClause> VisitSetClauses(IList<DbModificationClause> setClauses)
	{
		return new ReadOnlyCollection<DbModificationClause>((from DbSetClause s in setClauses
			select new DbSetClause(s.Property.Accept(this), s.Value.Accept(this))).Cast<DbModificationClause>().ToList());
	}

	public override DbExpression Visit(DbComparisonExpression expression)
	{
		DbComparisonExpression dbComparisonExpression = (DbComparisonExpression)base.Visit(expression);
		DbPropertyExpression dbPropertyExpression = (DbPropertyExpression)dbComparisonExpression.Left;
		if (((EdmProperty)dbPropertyExpression.Property).Nullable)
		{
			DbAndExpression right = dbPropertyExpression.IsNull().And(dbComparisonExpression.Right.IsNull());
			return dbComparisonExpression.Or(right);
		}
		return dbComparisonExpression;
	}

	public override DbExpression Visit(DbPropertyExpression expression)
	{
		_currentProperty = (EdmProperty)expression.Property;
		return base.Visit(expression);
	}

	public override DbExpression Visit(DbConstantExpression expression)
	{
		if (_currentProperty != null)
		{
			Tuple<FunctionParameter, bool> parameter = GetParameter(_currentProperty, _useOriginalValues);
			if (parameter != null)
			{
				return new DbParameterReferenceExpression(parameter.Item1.TypeUsage, parameter.Item1.Name);
			}
		}
		return base.Visit(expression);
	}

	public override DbExpression Visit(DbAndExpression expression)
	{
		DbExpression dbExpression = VisitExpression(expression.Left);
		DbExpression dbExpression2 = VisitExpression(expression.Right);
		if (dbExpression != null && dbExpression2 != null)
		{
			return dbExpression.And(dbExpression2);
		}
		return dbExpression ?? dbExpression2;
	}

	public override DbExpression Visit(DbIsNullExpression expression)
	{
		if (expression.Argument is DbPropertyExpression dbPropertyExpression)
		{
			Tuple<FunctionParameter, bool> parameter = GetParameter((EdmProperty)dbPropertyExpression.Property, originalValue: true);
			if (parameter != null)
			{
				if (parameter.Item2)
				{
					return null;
				}
				DbParameterReferenceExpression dbParameterReferenceExpression = new DbParameterReferenceExpression(parameter.Item1.TypeUsage, parameter.Item1.Name);
				DbComparisonExpression left = dbPropertyExpression.Equal(dbParameterReferenceExpression);
				DbAndExpression right = dbPropertyExpression.IsNull().And(dbParameterReferenceExpression.IsNull());
				return left.Or(right);
			}
		}
		return base.Visit(expression);
	}

	public override DbExpression Visit(DbNullExpression expression)
	{
		if (_currentProperty != null)
		{
			Tuple<FunctionParameter, bool> parameter = GetParameter(_currentProperty);
			if (parameter != null)
			{
				return new DbParameterReferenceExpression(parameter.Item1.TypeUsage, parameter.Item1.Name);
			}
		}
		return base.Visit(expression);
	}

	public override DbExpression Visit(DbNewInstanceExpression expression)
	{
		return DbExpressionBuilder.NewRow((from DbPropertyExpression propertyExpression in expression.Arguments
			let resultBinding = _currentFunctionMapping.ResultBindings.Single((ModificationFunctionResultBinding rb) => (from esm in _entityContainerMapping.EntitySetMappings
				from etm in esm.EntityTypeMappings
				from mf in etm.MappingFragments
				from pm in mf.PropertyMappings.OfType<ScalarPropertyMapping>()
				where pm.Column.EdmEquals(propertyExpression.Property) && pm.Column.DeclaringType.EdmEquals(propertyExpression.Property.DeclaringType)
				select pm.Property).Contains(rb.Property))
			select new KeyValuePair<string, DbExpression>(resultBinding.ColumnName, propertyExpression)).ToList());
	}

	private Tuple<FunctionParameter, bool> GetParameter(EdmProperty column, bool originalValue = false)
	{
		List<ColumnMappingBuilder> columnMappings = (from esm in _entityContainerMapping.EntitySetMappings
			from etm in esm.EntityTypeMappings
			from mf in etm.MappingFragments
			from cm in mf.FlattenedProperties
			where cm.ColumnProperty.EdmEquals(column) && cm.ColumnProperty.DeclaringType.EdmEquals(column.DeclaringType)
			select cm).ToList();
		List<ModificationFunctionParameterBinding> list = _currentFunctionMapping.ParameterBindings.Where((ModificationFunctionParameterBinding pb) => columnMappings.Any((ColumnMappingBuilder cm) => pb.MemberPath.Members.Reverse().SequenceEqual(cm.PropertyPath))).ToList();
		if (!list.Any())
		{
			List<EdmMember[]> iaColumnMappings = (from asm in _entityContainerMapping.AssociationSetMappings
				from tm in asm.TypeMappings
				from mf in tm.MappingFragments
				from epm in mf.PropertyMappings.OfType<EndPropertyMapping>()
				from pm in epm.PropertyMappings
				where pm.Column.EdmEquals(column) && pm.Column.DeclaringType.EdmEquals(column.DeclaringType)
				select new EdmMember[2] { pm.Property, epm.AssociationEnd }).ToList();
			list = _currentFunctionMapping.ParameterBindings.Where((ModificationFunctionParameterBinding pb) => iaColumnMappings.Any((EdmMember[] epm) => pb.MemberPath.Members.SequenceEqual(epm))).ToList();
		}
		if (list.Count == 0 && column.IsPrimaryKeyColumn)
		{
			return Tuple.Create(new FunctionParameter(_storeGeneratedKeys[_nextStoreGeneratedKey++].Name, column.TypeUsage, ParameterMode.In), item2: true);
		}
		if (list.Count == 1)
		{
			return Tuple.Create(list[0].Parameter, list[0].IsCurrent);
		}
		if (list.Count == 0)
		{
			return null;
		}
		ModificationFunctionParameterBinding modificationFunctionParameterBinding = (originalValue ? list.Single((ModificationFunctionParameterBinding pb) => !pb.IsCurrent) : list.Single((ModificationFunctionParameterBinding pb) => pb.IsCurrent));
		return Tuple.Create(modificationFunctionParameterBinding.Parameter, modificationFunctionParameterBinding.IsCurrent);
	}
}
