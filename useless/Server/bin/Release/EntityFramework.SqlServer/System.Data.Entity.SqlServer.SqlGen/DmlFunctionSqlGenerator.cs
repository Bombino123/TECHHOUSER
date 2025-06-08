using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class DmlFunctionSqlGenerator
{
	private sealed class ReturningSelectSqlGenerator : BasicExpressionVisitor
	{
		private readonly StringBuilder _select = new StringBuilder();

		private readonly StringBuilder _from = new StringBuilder();

		private readonly StringBuilder _where = new StringBuilder();

		private int _aliasCount;

		private string _currentTableAlias;

		private EntityType _baseTable;

		private string _nextPropertyAlias;

		public string Sql
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(_select.ToString());
				stringBuilder.AppendLine(_from.ToString());
				stringBuilder.Append("WHERE @@ROWCOUNT > 0");
				stringBuilder.Append((object?)_where);
				return stringBuilder.ToString();
			}
		}

		public override void Visit(DbNewInstanceExpression newInstanceExpression)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			ReadOnlyMetadataCollection<EdmProperty> properties = ((RowType)((DbExpression)newInstanceExpression).ResultType.EdmType).Properties;
			for (int i = 0; i < ((ReadOnlyCollection<EdmProperty>)(object)properties).Count; i++)
			{
				_select.Append((_select.Length == 0) ? "SELECT " : ", ");
				_nextPropertyAlias = ((EdmMember)((ReadOnlyCollection<EdmProperty>)(object)properties)[i]).Name;
				newInstanceExpression.Arguments[i].Accept((DbExpressionVisitor)(object)this);
			}
			_nextPropertyAlias = null;
		}

		public override void Visit(DbScanExpression scanExpression)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			string value = SqlGenerator.GetTargetTSql(scanExpression.Target) + " AS " + (_currentTableAlias = "t" + _aliasCount++);
			EntityTypeBase elementType = scanExpression.Target.ElementType;
			if (_from.Length == 0)
			{
				_baseTable = (EntityType)elementType;
				_from.Append("FROM ");
				_from.Append(value);
				return;
			}
			_from.AppendLine();
			_from.Append("JOIN ");
			_from.Append(value);
			_from.Append(" ON ");
			for (int i = 0; i < ((ReadOnlyCollection<EdmMember>)(object)elementType.KeyMembers).Count; i++)
			{
				if (i > 0)
				{
					_from.Append(" AND ");
				}
				_from.Append(_currentTableAlias + ".");
				_from.Append(SqlGenerator.QuoteIdentifier(((ReadOnlyCollection<EdmMember>)(object)elementType.KeyMembers)[i].Name));
				_from.Append(" = t0.");
				_from.Append(SqlGenerator.QuoteIdentifier(((ReadOnlyCollection<EdmMember>)(object)((EntityTypeBase)_baseTable).KeyMembers)[i].Name));
			}
		}

		public override void Visit(DbPropertyExpression propertyExpression)
		{
			_select.Append(_currentTableAlias);
			_select.Append(".");
			_select.Append(SqlGenerator.QuoteIdentifier(propertyExpression.Property.Name));
			if (!string.IsNullOrWhiteSpace(_nextPropertyAlias) && !string.Equals(_nextPropertyAlias, propertyExpression.Property.Name, StringComparison.Ordinal))
			{
				_select.Append(" AS ");
				_select.Append(_nextPropertyAlias);
			}
		}

		public override void Visit(DbParameterReferenceExpression expression)
		{
			_where.Append("@" + expression.ParameterName);
		}

		public override void Visit(DbIsNullExpression expression)
		{
		}

		public override void Visit(DbComparisonExpression comparisonExpression)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			EdmMember property = ((DbPropertyExpression)((DbBinaryExpression)comparisonExpression).Left).Property;
			if (((ReadOnlyCollection<EdmMember>)(object)((EntityTypeBase)_baseTable).KeyMembers).Contains(property))
			{
				_where.Append(" AND t0.");
				_where.Append(SqlGenerator.QuoteIdentifier(property.Name));
				_where.Append(" = ");
				((DbBinaryExpression)comparisonExpression).Right.Accept((DbExpressionVisitor)(object)this);
			}
		}
	}

	private readonly SqlGenerator _sqlGenerator;

	public DmlFunctionSqlGenerator(SqlGenerator sqlGenerator)
	{
		_sqlGenerator = sqlGenerator;
	}

	public string GenerateInsert(ICollection<DbInsertCommandTree> commandTrees)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder();
		DbInsertCommandTree val = commandTrees.First();
		stringBuilder.Append(DmlSqlGenerator.GenerateInsertSql(val, _sqlGenerator, out var parameters, generateReturningSql: false, upperCaseKeywords: true, createParameters: false));
		stringBuilder.AppendLine();
		EntityType val2 = (EntityType)((DbScanExpression)((DbModificationCommandTree)val).Target.Expression).Target.ElementType;
		stringBuilder.Append(IntroduceRequiredLocalVariables(val2, val));
		foreach (DbInsertCommandTree item in commandTrees.Skip(1))
		{
			stringBuilder.Append(DmlSqlGenerator.GenerateInsertSql(item, _sqlGenerator, out parameters, generateReturningSql: false, upperCaseKeywords: true, createParameters: false));
			stringBuilder.AppendLine();
		}
		List<DbInsertCommandTree> list = commandTrees.Where((DbInsertCommandTree ct) => ct.Returning != null).ToList();
		if (list.Any())
		{
			ReturningSelectSqlGenerator returningSelectSqlGenerator = new ReturningSelectSqlGenerator();
			foreach (DbInsertCommandTree item2 in list)
			{
				((DbModificationCommandTree)item2).Target.Expression.Accept((DbExpressionVisitor)(object)returningSelectSqlGenerator);
				item2.Returning.Accept((DbExpressionVisitor)(object)returningSelectSqlGenerator);
			}
			Enumerator<EdmProperty> enumerator3 = ((EntityTypeBase)val2).KeyProperties.GetEnumerator();
			try
			{
				while (enumerator3.MoveNext())
				{
					EdmProperty keyProperty = enumerator3.Current;
					DbExpression val3 = (DbExpression)(((object)(from DbSetClause sc in val.SetClauses
						where ((DbPropertyExpression)sc.Property).Property == keyProperty
						select sc.Value).SingleOrDefault()) ?? ((object)DbExpressionBuilder.Parameter(((EdmMember)keyProperty).TypeUsage, ((EdmMember)keyProperty).Name)));
					((DbExpression)DbExpressionBuilder.Equal((DbExpression)(object)DbExpressionBuilder.Property((DbExpression)(object)((DbModificationCommandTree)val).Target.Variable, keyProperty), val3)).Accept((DbExpressionVisitor)(object)returningSelectSqlGenerator);
				}
			}
			finally
			{
				((IDisposable)enumerator3).Dispose();
			}
			stringBuilder.Append(returningSelectSqlGenerator.Sql);
		}
		return stringBuilder.ToString().TrimEnd(new char[0]);
	}

	private string IntroduceRequiredLocalVariables(EntityType entityType, DbInsertCommandTree commandTree)
	{
		List<EdmProperty> list = ((IEnumerable<EdmProperty>)((EntityTypeBase)entityType).KeyProperties).Where((EdmProperty p) => ((EdmMember)p).IsStoreGeneratedIdentity).ToList();
		SqlStringBuilder sqlStringBuilder = new SqlStringBuilder
		{
			UpperCaseKeywords = true
		};
		if (list.Any())
		{
			foreach (EdmProperty item in list)
			{
				sqlStringBuilder.Append((sqlStringBuilder.Length == 0) ? "DECLARE " : ", ");
				sqlStringBuilder.Append("@");
				sqlStringBuilder.Append(((EdmMember)item).Name);
				sqlStringBuilder.Append(" ");
				sqlStringBuilder.Append(DmlSqlGenerator.GetVariableType(_sqlGenerator, (EdmMember)(object)item));
			}
			sqlStringBuilder.AppendLine();
			DmlSqlGenerator.ExpressionTranslator translator = new DmlSqlGenerator.ExpressionTranslator(sqlStringBuilder, (DbModificationCommandTree)(object)commandTree, preserveMemberValues: true, _sqlGenerator, (ICollection<EdmProperty>)((EntityTypeBase)entityType).KeyProperties);
			DmlSqlGenerator.GenerateReturningSql(sqlStringBuilder, (DbModificationCommandTree)(object)commandTree, entityType, translator, commandTree.Returning, DmlSqlGenerator.UseGeneratedValuesVariable(commandTree, _sqlGenerator.SqlVersion));
			sqlStringBuilder.AppendLine();
			sqlStringBuilder.AppendLine();
		}
		return sqlStringBuilder.ToString();
	}

	public string GenerateUpdate(ICollection<DbUpdateCommandTree> commandTrees, string rowsAffectedParameter)
	{
		if (!commandTrees.Any())
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(DmlSqlGenerator.GenerateUpdateSql(commandTrees.First(), _sqlGenerator, out var parameters, generateReturningSql: false));
		foreach (DbUpdateCommandTree item in commandTrees.Skip(1))
		{
			stringBuilder.Append(DmlSqlGenerator.GenerateUpdateSql(item, _sqlGenerator, out parameters, generateReturningSql: false));
			stringBuilder.AppendLine("AND @@ROWCOUNT > 0");
			stringBuilder.AppendLine();
		}
		List<DbUpdateCommandTree> list = commandTrees.Where((DbUpdateCommandTree ct) => ct.Returning != null).ToList();
		if (list.Any())
		{
			ReturningSelectSqlGenerator returningSelectSqlGenerator = new ReturningSelectSqlGenerator();
			foreach (DbUpdateCommandTree item2 in list)
			{
				((DbModificationCommandTree)item2).Target.Expression.Accept((DbExpressionVisitor)(object)returningSelectSqlGenerator);
				item2.Returning.Accept((DbExpressionVisitor)(object)returningSelectSqlGenerator);
				item2.Predicate.Accept((DbExpressionVisitor)(object)returningSelectSqlGenerator);
			}
			stringBuilder.AppendLine(returningSelectSqlGenerator.Sql);
			stringBuilder.AppendLine();
		}
		AppendSetRowsAffected(stringBuilder, rowsAffectedParameter);
		return stringBuilder.ToString().TrimEnd(new char[0]);
	}

	public string GenerateDelete(ICollection<DbDeleteCommandTree> commandTrees, string rowsAffectedParameter)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(DmlSqlGenerator.GenerateDeleteSql(commandTrees.First(), _sqlGenerator, out var parameters));
		stringBuilder.AppendLine();
		foreach (DbDeleteCommandTree item in commandTrees.Skip(1))
		{
			stringBuilder.AppendLine(DmlSqlGenerator.GenerateDeleteSql(item, _sqlGenerator, out parameters));
			stringBuilder.AppendLine("AND @@ROWCOUNT > 0");
			stringBuilder.AppendLine();
		}
		AppendSetRowsAffected(stringBuilder, rowsAffectedParameter);
		return stringBuilder.ToString().TrimEnd(new char[0]);
	}

	private static void AppendSetRowsAffected(StringBuilder sql, string rowsAffectedParameter)
	{
		if (!string.IsNullOrWhiteSpace(rowsAffectedParameter))
		{
			sql.Append("SET @");
			sql.Append(rowsAffectedParameter);
			sql.AppendLine(" = @@ROWCOUNT");
			sql.AppendLine();
		}
	}
}
