using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace System.Data.Entity.SqlServer.SqlGen;

internal static class DmlSqlGenerator
{
	internal class ExpressionTranslator : BasicExpressionVisitor
	{
		private readonly SqlStringBuilder _commandText;

		private readonly DbModificationCommandTree _commandTree;

		private readonly List<SqlParameter> _parameters;

		private readonly Dictionary<EdmMember, SqlParameter> _memberValues;

		private readonly SqlGenerator _sqlGenerator;

		private readonly ICollection<EdmProperty> _localVariableBindings;

		private readonly bool _createParameters;

		internal List<SqlParameter> Parameters => _parameters;

		internal Dictionary<EdmMember, SqlParameter> MemberValues => _memberValues;

		internal string PropertyAlias { get; set; }

		internal ExpressionTranslator(SqlStringBuilder commandText, DbModificationCommandTree commandTree, bool preserveMemberValues, SqlGenerator sqlGenerator, ICollection<EdmProperty> localVariableBindings = null, bool createParameters = true)
		{
			_commandText = commandText;
			_commandTree = commandTree;
			_sqlGenerator = sqlGenerator;
			_localVariableBindings = localVariableBindings;
			_parameters = new List<SqlParameter>();
			_memberValues = (preserveMemberValues ? new Dictionary<EdmMember, SqlParameter>() : null);
			_createParameters = createParameters;
		}

		internal SqlParameter CreateParameter(object value, TypeUsage type, string name = null)
		{
			SqlParameter val = SqlProviderServices.CreateSqlParameter(name ?? GetParameterName(_parameters.Count), type, (ParameterMode)0, value, preventTruncation: true, _sqlGenerator.SqlVersion);
			_parameters.Add(val);
			return val;
		}

		internal static string GetParameterName(int index)
		{
			return "@" + index.ToString(CultureInfo.InvariantCulture);
		}

		public override void Visit(DbAndExpression expression)
		{
			Check.NotNull<DbAndExpression>(expression, "expression");
			VisitBinary((DbBinaryExpression)(object)expression, " and ");
		}

		public override void Visit(DbOrExpression expression)
		{
			Check.NotNull<DbOrExpression>(expression, "expression");
			VisitBinary((DbBinaryExpression)(object)expression, " or ");
		}

		public override void Visit(DbComparisonExpression expression)
		{
			Check.NotNull<DbComparisonExpression>(expression, "expression");
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
			Check.NotNull<DbIsNullExpression>(expression, "expression");
			((DbUnaryExpression)expression).Argument.Accept((DbExpressionVisitor)(object)this);
			_commandText.AppendKeyword(" is null");
		}

		public override void Visit(DbNotExpression expression)
		{
			Check.NotNull<DbNotExpression>(expression, "expression");
			_commandText.AppendKeyword("not (");
			((DbUnaryExpression)expression).Argument.Accept((DbExpressionVisitor)(object)this);
			_commandText.Append(")");
		}

		public override void Visit(DbConstantExpression expression)
		{
			Check.NotNull<DbConstantExpression>(expression, "expression");
			SqlParameter val = CreateParameter(expression.Value, ((DbExpression)expression).ResultType);
			if (_createParameters)
			{
				_commandText.Append(((DbParameter)(object)val).ParameterName);
				return;
			}
			SqlWriter sqlWriter = new SqlWriter(_commandText.InnerBuilder);
			try
			{
				_sqlGenerator.WriteSql(sqlWriter, ((DbExpression)expression).Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)_sqlGenerator));
			}
			finally
			{
				((IDisposable)sqlWriter)?.Dispose();
			}
		}

		public override void Visit(DbParameterReferenceExpression expression)
		{
			Check.NotNull<DbParameterReferenceExpression>(expression, "expression");
			SqlParameter val = CreateParameter(DBNull.Value, ((DbExpression)expression).ResultType, "@" + expression.ParameterName);
			_commandText.Append(((DbParameter)(object)val).ParameterName);
		}

		public override void Visit(DbScanExpression expression)
		{
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			Check.NotNull<DbScanExpression>(expression, "expression");
			if (((MetadataItem)(object)expression.Target).GetMetadataPropertyValue<string>("DefiningQuery") != null)
			{
				throw new UpdateException(Strings.Update_SqlEntitySetWithoutDmlFunctions(p1: (_commandTree is DbDeleteCommandTree) ? "DeleteFunction" : ((!(_commandTree is DbInsertCommandTree)) ? "UpdateFunction" : "InsertFunction"), p0: expression.Target.Name, p2: "ModificationFunctionMapping"));
			}
			_commandText.Append(SqlGenerator.GetTargetTSql(expression.Target));
		}

		public override void Visit(DbPropertyExpression expression)
		{
			Check.NotNull<DbPropertyExpression>(expression, "expression");
			if (!string.IsNullOrEmpty(PropertyAlias))
			{
				_commandText.Append(PropertyAlias);
				_commandText.Append(".");
			}
			_commandText.Append(GenerateMemberTSql(expression.Property));
		}

		public override void Visit(DbNullExpression expression)
		{
			Check.NotNull<DbNullExpression>(expression, "expression");
			_commandText.AppendKeyword("null");
		}

		public override void Visit(DbNewInstanceExpression expression)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			Check.NotNull<DbNewInstanceExpression>(expression, "expression");
			bool flag = true;
			foreach (DbExpression argument in expression.Arguments)
			{
				EdmMember property = ((DbPropertyExpression)argument).Property;
				string text = ((_localVariableBindings == null) ? string.Empty : (((IEnumerable<EdmMember>)_localVariableBindings).Contains(property) ? ("@" + property.Name + " = ") : null));
				if (text != null)
				{
					if (flag)
					{
						flag = false;
					}
					else
					{
						_commandText.Append(", ");
					}
					_commandText.Append(text);
					argument.Accept((DbExpressionVisitor)(object)this);
				}
			}
		}

		private void VisitBinary(DbBinaryExpression expression, string separator)
		{
			_commandText.Append("(");
			expression.Left.Accept((DbExpressionVisitor)(object)this);
			_commandText.AppendKeyword(separator);
			expression.Right.Accept((DbExpressionVisitor)(object)this);
			_commandText.Append(")");
		}
	}

	private const int CommandTextBuilderInitialCapacity = 256;

	private const string GeneratedValuesVariableName = "@generated_keys";

	internal static string GenerateUpdateSql(DbUpdateCommandTree tree, SqlGenerator sqlGenerator, out List<SqlParameter> parameters, bool generateReturningSql = true, bool upperCaseKeywords = true)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		SqlStringBuilder sqlStringBuilder = new SqlStringBuilder(256)
		{
			UpperCaseKeywords = upperCaseKeywords
		};
		ExpressionTranslator expressionTranslator = new ExpressionTranslator(sqlStringBuilder, (DbModificationCommandTree)(object)tree, tree.Returning != null, sqlGenerator);
		if (tree.SetClauses.Count == 0)
		{
			sqlStringBuilder.AppendKeyword("declare ");
			sqlStringBuilder.AppendLine("@p int");
		}
		sqlStringBuilder.AppendKeyword("update ");
		((DbModificationCommandTree)tree).Target.Expression.Accept((DbExpressionVisitor)(object)expressionTranslator);
		sqlStringBuilder.AppendLine();
		bool flag = true;
		sqlStringBuilder.AppendKeyword("set ");
		foreach (DbSetClause setClause in tree.SetClauses)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sqlStringBuilder.Append(", ");
			}
			setClause.Property.Accept((DbExpressionVisitor)(object)expressionTranslator);
			sqlStringBuilder.Append(" = ");
			setClause.Value.Accept((DbExpressionVisitor)(object)expressionTranslator);
		}
		if (flag)
		{
			sqlStringBuilder.Append("@p = 0");
		}
		sqlStringBuilder.AppendLine();
		sqlStringBuilder.AppendKeyword("where ");
		tree.Predicate.Accept((DbExpressionVisitor)(object)expressionTranslator);
		sqlStringBuilder.AppendLine();
		if (generateReturningSql)
		{
			GenerateReturningSql(sqlStringBuilder, (DbModificationCommandTree)(object)tree, null, expressionTranslator, tree.Returning, useGeneratedValuesVariable: false);
		}
		parameters = expressionTranslator.Parameters;
		return sqlStringBuilder.ToString();
	}

	internal static string GenerateDeleteSql(DbDeleteCommandTree tree, SqlGenerator sqlGenerator, out List<SqlParameter> parameters, bool upperCaseKeywords = true, bool createParameters = true)
	{
		SqlStringBuilder obj = new SqlStringBuilder(256)
		{
			UpperCaseKeywords = upperCaseKeywords
		};
		ExpressionTranslator expressionTranslator = new ExpressionTranslator(obj, (DbModificationCommandTree)(object)tree, preserveMemberValues: false, sqlGenerator, null, createParameters);
		obj.AppendKeyword("delete ");
		((DbModificationCommandTree)tree).Target.Expression.Accept((DbExpressionVisitor)(object)expressionTranslator);
		obj.AppendLine();
		obj.AppendKeyword("where ");
		tree.Predicate.Accept((DbExpressionVisitor)(object)expressionTranslator);
		parameters = expressionTranslator.Parameters;
		return obj.ToString();
	}

	internal static string GenerateInsertSql(DbInsertCommandTree tree, SqlGenerator sqlGenerator, out List<SqlParameter> parameters, bool generateReturningSql = true, bool upperCaseKeywords = true, bool createParameters = true)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Expected O, but got Unknown
		SqlStringBuilder sqlStringBuilder = new SqlStringBuilder(256)
		{
			UpperCaseKeywords = upperCaseKeywords
		};
		ExpressionTranslator expressionTranslator = new ExpressionTranslator(sqlStringBuilder, (DbModificationCommandTree)(object)tree, tree.Returning != null, sqlGenerator, null, createParameters);
		bool flag = UseGeneratedValuesVariable(tree, sqlGenerator.SqlVersion);
		EntityType val = (EntityType)((DbScanExpression)((DbModificationCommandTree)tree).Target.Expression).Target.ElementType;
		if (flag)
		{
			sqlStringBuilder.AppendKeyword("declare ").Append("@generated_keys").Append(" table(");
			bool flag2 = true;
			Enumerator<EdmMember> enumerator = ((EntityTypeBase)val).KeyMembers.GetEnumerator();
			try
			{
				Facet val2 = default(Facet);
				while (enumerator.MoveNext())
				{
					EdmMember current = enumerator.Current;
					if (flag2)
					{
						flag2 = false;
					}
					else
					{
						sqlStringBuilder.Append(", ");
					}
					sqlStringBuilder.Append(GenerateMemberTSql(current)).Append(" ").Append(GetVariableType(sqlGenerator, current));
					if (current.TypeUsage.Facets.TryGetValue("Collation", false, ref val2))
					{
						string text = val2.Value as string;
						if (!string.IsNullOrEmpty(text))
						{
							sqlStringBuilder.AppendKeyword(" collate ").Append(text);
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			sqlStringBuilder.AppendLine(")");
		}
		sqlStringBuilder.AppendKeyword("insert ");
		((DbModificationCommandTree)tree).Target.Expression.Accept((DbExpressionVisitor)(object)expressionTranslator);
		if (0 < tree.SetClauses.Count)
		{
			sqlStringBuilder.Append("(");
			bool flag3 = true;
			foreach (DbSetClause setClause in tree.SetClauses)
			{
				if (flag3)
				{
					flag3 = false;
				}
				else
				{
					sqlStringBuilder.Append(", ");
				}
				setClause.Property.Accept((DbExpressionVisitor)(object)expressionTranslator);
			}
			sqlStringBuilder.AppendLine(")");
		}
		else
		{
			sqlStringBuilder.AppendLine();
		}
		if (flag)
		{
			sqlStringBuilder.AppendKeyword("output ");
			bool flag4 = true;
			Enumerator<EdmMember> enumerator = ((EntityTypeBase)val).KeyMembers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					EdmMember current2 = enumerator.Current;
					if (flag4)
					{
						flag4 = false;
					}
					else
					{
						sqlStringBuilder.Append(", ");
					}
					sqlStringBuilder.Append("inserted.");
					sqlStringBuilder.Append(GenerateMemberTSql(current2));
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			sqlStringBuilder.AppendKeyword(" into ").AppendLine("@generated_keys");
		}
		if (0 < tree.SetClauses.Count)
		{
			bool flag5 = true;
			sqlStringBuilder.AppendKeyword("values (");
			foreach (DbSetClause setClause2 in tree.SetClauses)
			{
				DbSetClause val4 = setClause2;
				if (flag5)
				{
					flag5 = false;
				}
				else
				{
					sqlStringBuilder.Append(", ");
				}
				val4.Value.Accept((DbExpressionVisitor)(object)expressionTranslator);
				expressionTranslator.RegisterMemberValue(val4.Property, val4.Value);
			}
			sqlStringBuilder.AppendLine(")");
		}
		else
		{
			sqlStringBuilder.AppendKeyword("default values");
			sqlStringBuilder.AppendLine();
		}
		if (generateReturningSql)
		{
			GenerateReturningSql(sqlStringBuilder, (DbModificationCommandTree)(object)tree, val, expressionTranslator, tree.Returning, flag);
		}
		parameters = expressionTranslator.Parameters;
		return sqlStringBuilder.ToString();
	}

	internal static string GetVariableType(SqlGenerator sqlGenerator, EdmMember column)
	{
		string text = SqlGenerator.GenerateSqlForStoreType(sqlGenerator.SqlVersion, column.TypeUsage);
		if (text == "rowversion" || text == "timestamp")
		{
			text = "binary(8)";
		}
		return text;
	}

	internal static bool UseGeneratedValuesVariable(DbInsertCommandTree tree, SqlVersion sqlVersion)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		if (sqlVersion > SqlVersion.Sql8 && tree.Returning != null)
		{
			HashSet<EdmMember> hashSet = new HashSet<EdmMember>(from DbSetClause s in tree.SetClauses
				select ((DbPropertyExpression)s.Property).Property);
			bool flag = false;
			Enumerator<EdmMember> enumerator = ((DbScanExpression)((DbModificationCommandTree)tree).Target.Expression).Target.ElementType.KeyMembers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					EdmMember current = enumerator.Current;
					if (!hashSet.Contains(current))
					{
						if (flag)
						{
							result = true;
							break;
						}
						flag = true;
						if (!IsValidScopeIdentityColumnType(current.TypeUsage))
						{
							result = true;
							break;
						}
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
		return result;
	}

	internal static string GenerateMemberTSql(EdmMember member)
	{
		return SqlGenerator.QuoteIdentifier(member.Name);
	}

	internal static void GenerateReturningSql(SqlStringBuilder commandText, DbModificationCommandTree tree, EntityType tableType, ExpressionTranslator translator, DbExpression returning, bool useGeneratedValuesVariable)
	{
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (returning == null)
		{
			return;
		}
		commandText.AppendKeyword("select ");
		if (useGeneratedValuesVariable)
		{
			translator.PropertyAlias = "t";
		}
		returning.Accept((DbExpressionVisitor)(object)translator);
		if (useGeneratedValuesVariable)
		{
			translator.PropertyAlias = null;
		}
		commandText.AppendLine();
		Enumerator<EdmMember> enumerator;
		if (useGeneratedValuesVariable)
		{
			commandText.AppendKeyword("from ");
			commandText.Append("@generated_keys");
			commandText.AppendKeyword(" as ");
			commandText.Append("g");
			commandText.AppendKeyword(" join ");
			tree.Target.Expression.Accept((DbExpressionVisitor)(object)translator);
			commandText.AppendKeyword(" as ");
			commandText.Append("t");
			commandText.AppendKeyword(" on ");
			string keyword = string.Empty;
			enumerator = ((EntityTypeBase)tableType).KeyMembers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					EdmMember current = enumerator.Current;
					commandText.AppendKeyword(keyword);
					keyword = " and ";
					commandText.Append("g.");
					string s = GenerateMemberTSql(current);
					commandText.Append(s);
					commandText.Append(" = t.");
					commandText.Append(s);
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			commandText.AppendLine();
			commandText.AppendKeyword("where @@ROWCOUNT > 0");
			return;
		}
		commandText.AppendKeyword("from ");
		tree.Target.Expression.Accept((DbExpressionVisitor)(object)translator);
		commandText.AppendLine();
		commandText.AppendKeyword("where @@ROWCOUNT > 0");
		EntitySetBase target = ((DbScanExpression)tree.Target.Expression).Target;
		bool flag = false;
		enumerator = target.ElementType.KeyMembers.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				EdmMember current2 = enumerator.Current;
				commandText.AppendKeyword(" and ");
				commandText.Append(GenerateMemberTSql(current2));
				commandText.Append(" = ");
				if (translator.MemberValues.TryGetValue(current2, out var value))
				{
					commandText.Append(((DbParameter)(object)value).ParameterName);
					continue;
				}
				if (flag)
				{
					throw new NotSupportedException(Strings.Update_NotSupportedServerGenKey(target.Name));
				}
				if (!IsValidScopeIdentityColumnType(current2.TypeUsage))
				{
					throw new InvalidOperationException(Strings.Update_NotSupportedIdentityType(current2.Name, ((object)current2.TypeUsage).ToString()));
				}
				commandText.Append("scope_identity()");
				flag = true;
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
	}

	private static bool IsValidScopeIdentityColumnType(TypeUsage typeUsage)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if (!SqlProviderServices.UseScopeIdentity)
		{
			return false;
		}
		if ((int)((MetadataItem)typeUsage.EdmType).BuiltInTypeKind != 26)
		{
			return false;
		}
		switch (typeUsage.EdmType.Name)
		{
		case "tinyint":
		case "smallint":
		case "int":
		case "bigint":
			return true;
		case "decimal":
		case "numeric":
		{
			Facet val = default(Facet);
			if (typeUsage.Facets.TryGetValue("Scale", false, ref val))
			{
				return Convert.ToInt32(val.Value, CultureInfo.InvariantCulture) == 0;
			}
			return false;
		}
		default:
			return false;
		}
	}
}
