using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Text;

namespace System.Data.SQLite.Linq;

internal static class DmlSqlGenerator
{
	private class ExpressionTranslator : DbExpressionVisitor
	{
		private readonly StringBuilder _commandText;

		private readonly DbModificationCommandTree _commandTree;

		private readonly List<DbParameter> _parameters;

		private readonly Dictionary<EdmMember, DbParameter> _memberValues;

		private int parameterNameCount;

		private string _kind;

		internal List<DbParameter> Parameters => _parameters;

		internal Dictionary<EdmMember, DbParameter> MemberValues => _memberValues;

		internal ExpressionTranslator(StringBuilder commandText, DbModificationCommandTree commandTree, bool preserveMemberValues, string kind)
		{
			_kind = kind;
			_commandText = commandText;
			_commandTree = commandTree;
			_parameters = new List<DbParameter>();
			_memberValues = (preserveMemberValues ? new Dictionary<EdmMember, DbParameter>() : null);
		}

		internal SQLiteParameter CreateParameter(object value, TypeUsage type)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			DbType dbType = MetadataHelpers.GetDbType(MetadataHelpers.GetPrimitiveTypeKind(type));
			return CreateParameter(value, dbType);
		}

		internal SQLiteParameter CreateParameter(object value, DbType dbType)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			string text = "@p" + parameterNameCount.ToString(CultureInfo.InvariantCulture);
			parameterNameCount++;
			SQLiteParameter val = new SQLiteParameter(text, value);
			((DbParameter)(object)val).DbType = dbType;
			_parameters.Add((DbParameter)(object)val);
			return val;
		}

		public override void Visit(DbApplyExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionBindingPre(expression.Input);
			if (expression.Apply != null)
			{
				VisitExpression(expression.Apply.Expression);
			}
			VisitExpressionBindingPost(expression.Input);
		}

		public override void Visit(DbArithmeticExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionList(expression.Arguments);
		}

		public override void Visit(DbCaseExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionList(expression.When);
			VisitExpressionList(expression.Then);
			VisitExpression(expression.Else);
		}

		public override void Visit(DbCastExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbCrossJoinExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			foreach (DbExpressionBinding input in expression.Inputs)
			{
				VisitExpressionBindingPre(input);
			}
			foreach (DbExpressionBinding input2 in expression.Inputs)
			{
				VisitExpressionBindingPost(input2);
			}
		}

		public override void Visit(DbDerefExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbDistinctExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbElementExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbEntityRefExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbExceptExpression expression)
		{
			VisitBinary((DbBinaryExpression)(object)expression);
		}

		protected virtual void VisitBinary(DbBinaryExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpression(expression.Left);
			VisitExpression(expression.Right);
		}

		public override void Visit(DbExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			throw new NotSupportedException("DbExpression");
		}

		public override void Visit(DbFilterExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionBindingPre(expression.Input);
			VisitExpression(expression.Predicate);
			VisitExpressionBindingPost(expression.Input);
		}

		public override void Visit(DbFunctionExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionList(expression.Arguments);
		}

		public override void Visit(DbGroupByExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitGroupExpressionBindingPre(expression.Input);
			VisitExpressionList(expression.Keys);
			VisitGroupExpressionBindingMid(expression.Input);
			VisitAggregateList(expression.Aggregates);
			VisitGroupExpressionBindingPost(expression.Input);
		}

		public override void Visit(DbIntersectExpression expression)
		{
			VisitBinary((DbBinaryExpression)(object)expression);
		}

		public override void Visit(DbIsEmptyExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbIsOfExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbJoinExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionBindingPre(expression.Left);
			VisitExpressionBindingPre(expression.Right);
			VisitExpression(expression.JoinCondition);
			VisitExpressionBindingPost(expression.Left);
			VisitExpressionBindingPost(expression.Right);
		}

		public override void Visit(DbLikeExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpression(expression.Argument);
			VisitExpression(expression.Pattern);
			VisitExpression(expression.Escape);
		}

		public override void Visit(DbLimitExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpression(expression.Argument);
			VisitExpression(expression.Limit);
		}

		public override void Visit(DbOfTypeExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbParameterReferenceExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
		}

		public override void Visit(DbProjectExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionBindingPre(expression.Input);
			VisitExpression(expression.Projection);
			VisitExpressionBindingPost(expression.Input);
		}

		public override void Visit(DbQuantifierExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionBindingPre(expression.Input);
			VisitExpression(expression.Predicate);
			VisitExpressionBindingPost(expression.Input);
		}

		public override void Visit(DbRefExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbRefKeyExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbRelationshipNavigationExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpression(expression.NavigationSource);
		}

		public override void Visit(DbSkipExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionBindingPre(expression.Input);
			foreach (DbSortClause item in expression.SortOrder)
			{
				VisitExpression(item.Expression);
			}
			VisitExpressionBindingPost(expression.Input);
			VisitExpression(expression.Count);
		}

		public override void Visit(DbSortExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpressionBindingPre(expression.Input);
			for (int i = 0; i < expression.SortOrder.Count; i++)
			{
				VisitExpression(expression.SortOrder[i].Expression);
			}
			VisitExpressionBindingPost(expression.Input);
		}

		public override void Visit(DbTreatExpression expression)
		{
			VisitUnaryExpression((DbUnaryExpression)(object)expression);
		}

		public override void Visit(DbUnionAllExpression expression)
		{
			VisitBinary((DbBinaryExpression)(object)expression);
		}

		public override void Visit(DbVariableReferenceExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
		}

		public virtual void VisitAggregate(DbAggregate aggregate)
		{
			if (aggregate == null)
			{
				throw new ArgumentException("aggregate");
			}
			VisitExpressionList(aggregate.Arguments);
		}

		public virtual void VisitAggregateList(IList<DbAggregate> aggregates)
		{
			if (aggregates == null)
			{
				throw new ArgumentException("aggregates");
			}
			for (int i = 0; i < aggregates.Count; i++)
			{
				VisitAggregate(aggregates[i]);
			}
		}

		public virtual void VisitExpression(DbExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			expression.Accept((DbExpressionVisitor)(object)this);
		}

		protected virtual void VisitExpressionBindingPost(DbExpressionBinding binding)
		{
		}

		protected virtual void VisitExpressionBindingPre(DbExpressionBinding binding)
		{
			if (binding == null)
			{
				throw new ArgumentException("binding");
			}
			VisitExpression(binding.Expression);
		}

		public virtual void VisitExpressionList(IList<DbExpression> expressionList)
		{
			if (expressionList == null)
			{
				throw new ArgumentException("expressionList");
			}
			for (int i = 0; i < expressionList.Count; i++)
			{
				VisitExpression(expressionList[i]);
			}
		}

		protected virtual void VisitGroupExpressionBindingMid(DbGroupExpressionBinding binding)
		{
		}

		protected virtual void VisitGroupExpressionBindingPost(DbGroupExpressionBinding binding)
		{
		}

		protected virtual void VisitGroupExpressionBindingPre(DbGroupExpressionBinding binding)
		{
			if (binding == null)
			{
				throw new ArgumentException("binding");
			}
			VisitExpression(binding.Expression);
		}

		protected virtual void VisitLambdaFunctionPost(EdmFunction function, DbExpression body)
		{
		}

		protected virtual void VisitLambdaFunctionPre(EdmFunction function, DbExpression body)
		{
			if (function == null)
			{
				throw new ArgumentException("function");
			}
			if (body == null)
			{
				throw new ArgumentException("body");
			}
		}

		protected virtual void VisitUnaryExpression(DbUnaryExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentException("expression");
			}
			VisitExpression(expression.Argument);
		}

		public override void Visit(DbAndExpression expression)
		{
			VisitBinary((DbBinaryExpression)(object)expression, " AND ");
		}

		public override void Visit(DbOrExpression expression)
		{
			VisitBinary((DbBinaryExpression)(object)expression, " OR ");
		}

		public override void Visit(DbComparisonExpression expression)
		{
			VisitBinary((DbBinaryExpression)(object)expression, " = ");
			RegisterMemberValue(((DbBinaryExpression)expression).Left, ((DbBinaryExpression)expression).Right);
		}

		internal void RegisterMemberValue(DbExpression propertyExpression, DbExpression value)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			if (_memberValues != null)
			{
				EdmMember property = ((DbPropertyExpression)propertyExpression).Property;
				if ((int)value.ExpressionKind != 38)
				{
					_memberValues[property] = _parameters[_parameters.Count - 1];
				}
			}
		}

		public override void Visit(DbIsNullExpression expression)
		{
			((DbUnaryExpression)expression).Argument.Accept((DbExpressionVisitor)(object)this);
			_commandText.Append(" IS NULL");
		}

		public override void Visit(DbNotExpression expression)
		{
			_commandText.Append("NOT (");
			((DbExpression)expression).Accept((DbExpressionVisitor)(object)this);
			_commandText.Append(")");
		}

		public override void Visit(DbConstantExpression expression)
		{
			SQLiteParameter val = CreateParameter(expression.Value, ((DbExpression)expression).ResultType);
			_commandText.Append(((DbParameter)(object)val).ParameterName);
		}

		public override void Visit(DbScanExpression expression)
		{
			if (MetadataHelpers.TryGetValueForMetadataProperty<string>((MetadataItem)(object)expression.Target, "DefiningQuery") != null)
			{
				throw new NotSupportedException($"Unable to update the EntitySet '{expression.Target.Name}' because it has a DefiningQuery and no <{_kind}> element exists in the <ModificationFunctionMapping> element to support the current operation.");
			}
			_commandText.Append(SqlGenerator.GetTargetTSql(expression.Target));
		}

		public override void Visit(DbPropertyExpression expression)
		{
			_commandText.Append(GenerateMemberTSql(expression.Property));
		}

		public override void Visit(DbNullExpression expression)
		{
			_commandText.Append("NULL");
		}

		public override void Visit(DbNewInstanceExpression expression)
		{
			bool flag = true;
			foreach (DbExpression argument in expression.Arguments)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					_commandText.Append(", ");
				}
				argument.Accept((DbExpressionVisitor)(object)this);
			}
		}

		private void VisitBinary(DbBinaryExpression expression, string separator)
		{
			_commandText.Append("(");
			expression.Left.Accept((DbExpressionVisitor)(object)this);
			_commandText.Append(separator);
			expression.Right.Accept((DbExpressionVisitor)(object)this);
			_commandText.Append(")");
		}
	}

	private static readonly int s_commandTextBuilderInitialCapacity = 256;

	internal static string GenerateUpdateSql(DbUpdateCommandTree tree, out List<DbParameter> parameters)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = new StringBuilder(s_commandTextBuilderInitialCapacity);
		ExpressionTranslator expressionTranslator = new ExpressionTranslator(stringBuilder, (DbModificationCommandTree)(object)tree, tree.Returning != null, "UpdateFunction");
		stringBuilder.Append("UPDATE ");
		((DbModificationCommandTree)tree).Target.Expression.Accept((DbExpressionVisitor)(object)expressionTranslator);
		stringBuilder.AppendLine();
		bool flag = true;
		stringBuilder.Append("SET ");
		foreach (DbSetClause setClause in tree.SetClauses)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				stringBuilder.Append(", ");
			}
			setClause.Property.Accept((DbExpressionVisitor)(object)expressionTranslator);
			stringBuilder.Append(" = ");
			setClause.Value.Accept((DbExpressionVisitor)(object)expressionTranslator);
		}
		if (flag)
		{
			DbParameter dbParameter = (DbParameter)(object)expressionTranslator.CreateParameter(0, DbType.Int32);
			stringBuilder.Append(dbParameter.ParameterName);
			stringBuilder.Append(" = 0");
		}
		stringBuilder.AppendLine();
		stringBuilder.Append("WHERE ");
		tree.Predicate.Accept((DbExpressionVisitor)(object)expressionTranslator);
		stringBuilder.AppendLine(";");
		GenerateReturningSql(stringBuilder, (DbModificationCommandTree)(object)tree, expressionTranslator, tree.Returning, wasInsert: false);
		parameters = expressionTranslator.Parameters;
		return stringBuilder.ToString();
	}

	internal static string GenerateDeleteSql(DbDeleteCommandTree tree, out List<DbParameter> parameters)
	{
		StringBuilder stringBuilder = new StringBuilder(s_commandTextBuilderInitialCapacity);
		ExpressionTranslator expressionTranslator = new ExpressionTranslator(stringBuilder, (DbModificationCommandTree)(object)tree, preserveMemberValues: false, "DeleteFunction");
		stringBuilder.Append("DELETE FROM ");
		((DbModificationCommandTree)tree).Target.Expression.Accept((DbExpressionVisitor)(object)expressionTranslator);
		stringBuilder.AppendLine();
		stringBuilder.Append("WHERE ");
		tree.Predicate.Accept((DbExpressionVisitor)(object)expressionTranslator);
		parameters = expressionTranslator.Parameters;
		stringBuilder.AppendLine(";");
		return stringBuilder.ToString();
	}

	internal static string GenerateInsertSql(DbInsertCommandTree tree, out List<DbParameter> parameters)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		StringBuilder stringBuilder = new StringBuilder(s_commandTextBuilderInitialCapacity);
		ExpressionTranslator expressionTranslator = new ExpressionTranslator(stringBuilder, (DbModificationCommandTree)(object)tree, tree.Returning != null, "InsertFunction");
		stringBuilder.Append("INSERT INTO ");
		((DbModificationCommandTree)tree).Target.Expression.Accept((DbExpressionVisitor)(object)expressionTranslator);
		if (tree.SetClauses.Count > 0)
		{
			stringBuilder.Append("(");
			bool flag = true;
			foreach (DbSetClause setClause in tree.SetClauses)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(", ");
				}
				setClause.Property.Accept((DbExpressionVisitor)(object)expressionTranslator);
			}
			stringBuilder.AppendLine(")");
			flag = true;
			stringBuilder.Append(" VALUES (");
			foreach (DbSetClause setClause2 in tree.SetClauses)
			{
				DbSetClause val2 = setClause2;
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append(", ");
				}
				val2.Value.Accept((DbExpressionVisitor)(object)expressionTranslator);
				expressionTranslator.RegisterMemberValue(val2.Property, val2.Value);
			}
			stringBuilder.AppendLine(");");
		}
		else
		{
			stringBuilder.AppendLine(" DEFAULT VALUES;");
		}
		GenerateReturningSql(stringBuilder, (DbModificationCommandTree)(object)tree, expressionTranslator, tree.Returning, wasInsert: true);
		parameters = expressionTranslator.Parameters;
		return stringBuilder.ToString();
	}

	private static string GenerateMemberTSql(EdmMember member)
	{
		return SqlGenerator.QuoteIdentifier(member.Name);
	}

	private static bool IsIntegerPrimaryKey(EntitySetBase table, out ReadOnlyMetadataCollection<EdmMember> keyMembers, out EdmMember primaryKeyMember)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		keyMembers = table.ElementType.KeyMembers;
		if (((ReadOnlyCollection<EdmMember>)(object)keyMembers).Count == 1)
		{
			EdmMember val = ((ReadOnlyCollection<EdmMember>)(object)keyMembers)[0];
			if (MetadataHelpers.TryGetPrimitiveTypeKind(val.TypeUsage, out var typeKind) && (int)typeKind == 11)
			{
				primaryKeyMember = val;
				return true;
			}
		}
		primaryKeyMember = null;
		return false;
	}

	private static bool DoAllKeyMembersHaveValues(ExpressionTranslator translator, ReadOnlyMetadataCollection<EdmMember> keyMembers, out EdmMember missingKeyMember)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<EdmMember> enumerator = keyMembers.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				EdmMember current = enumerator.Current;
				if (!translator.MemberValues.ContainsKey(current))
				{
					missingKeyMember = current;
					return false;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		missingKeyMember = null;
		return true;
	}

	private static void GenerateReturningSql(StringBuilder commandText, DbModificationCommandTree tree, ExpressionTranslator translator, DbExpression returning, bool wasInsert)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (returning == null)
		{
			return;
		}
		commandText.Append("SELECT ");
		returning.Accept((DbExpressionVisitor)(object)translator);
		commandText.AppendLine();
		commandText.Append("FROM ");
		tree.Target.Expression.Accept((DbExpressionVisitor)(object)translator);
		commandText.AppendLine();
		commandText.Append("WHERE last_rows_affected() > 0");
		EntitySetBase target = ((DbScanExpression)tree.Target.Expression).Target;
		EdmMember missingKeyMember;
		if (IsIntegerPrimaryKey(target, out var keyMembers, out var primaryKeyMember))
		{
			commandText.Append(" AND ");
			commandText.Append(GenerateMemberTSql(primaryKeyMember));
			commandText.Append(" = ");
			if (translator.MemberValues.TryGetValue(primaryKeyMember, out var value))
			{
				commandText.Append(value.ParameterName);
			}
			else
			{
				if (!wasInsert)
				{
					throw new NotSupportedException(string.Format("Missing value for INSERT key member '{0}' in table '{1}'.", (primaryKeyMember != null) ? primaryKeyMember.Name : "<unknown>", target.Name));
				}
				commandText.AppendLine("last_insert_rowid()");
			}
		}
		else if (DoAllKeyMembersHaveValues(translator, keyMembers, out missingKeyMember))
		{
			Enumerator<EdmMember> enumerator = keyMembers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					EdmMember current = enumerator.Current;
					commandText.Append(" AND ");
					commandText.Append(GenerateMemberTSql(current));
					commandText.Append(" = ");
					if (translator.MemberValues.TryGetValue(current, out var value2))
					{
						commandText.Append(value2.ParameterName);
						continue;
					}
					throw new NotSupportedException(string.Format("Missing value for {0} key member '{1}' in table '{2}' (internal).", wasInsert ? "INSERT" : "UPDATE", (current != null) ? current.Name : "<unknown>", target.Name));
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
		else
		{
			if (!wasInsert)
			{
				throw new NotSupportedException(string.Format("Missing value for UPDATE key member '{0}' in table '{1}'.", (missingKeyMember != null) ? missingKeyMember.Name : "<unknown>", target.Name));
			}
			commandText.Append(" AND ");
			commandText.Append(SqlGenerator.QuoteIdentifier("rowid"));
			commandText.Append(" = ");
			commandText.AppendLine("last_insert_rowid()");
		}
		commandText.AppendLine(";");
	}
}
