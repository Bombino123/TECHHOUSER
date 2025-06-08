using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Spatial;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer.Resources;
using System.Data.Entity.SqlServer.Utilities;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.Entity.SqlServer.SqlGen;

internal class SqlGenerator : DbExpressionVisitor<ISqlFragment>
{
	internal class KeyFieldExpressionComparer : IEqualityComparer<DbExpression>
	{
		public bool Equals(DbExpression x, DbExpression y)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Invalid comparison between Unknown and I4
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Invalid comparison between Unknown and I4
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Expected O, but got Unknown
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Expected O, but got Unknown
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Invalid comparison between Unknown and I4
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if (x.ExpressionKind != y.ExpressionKind)
			{
				return false;
			}
			DbExpressionKind expressionKind = x.ExpressionKind;
			if ((int)expressionKind <= 43)
			{
				if ((int)expressionKind == 4)
				{
					DbCastExpression val = (DbCastExpression)x;
					DbCastExpression val2 = (DbCastExpression)y;
					if (((DbExpression)val).ResultType == ((DbExpression)val2).ResultType)
					{
						return Equals(((DbUnaryExpression)val).Argument, ((DbUnaryExpression)val2).Argument);
					}
					return false;
				}
				if ((int)expressionKind == 43)
				{
					return ((DbParameterReferenceExpression)x).ParameterName == ((DbParameterReferenceExpression)y).ParameterName;
				}
			}
			else
			{
				if ((int)expressionKind == 46)
				{
					DbPropertyExpression val3 = (DbPropertyExpression)x;
					DbPropertyExpression val4 = (DbPropertyExpression)y;
					if (val3.Property == val4.Property)
					{
						return Equals(val3.Instance, val4.Instance);
					}
					return false;
				}
				if ((int)expressionKind == 56)
				{
					return x == y;
				}
			}
			return false;
		}

		public int GetHashCode(DbExpression obj)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Invalid comparison between Unknown and I4
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Invalid comparison between Unknown and I4
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			DbExpressionKind expressionKind = obj.ExpressionKind;
			if ((int)expressionKind <= 43)
			{
				if ((int)expressionKind == 4)
				{
					return GetHashCode(((DbUnaryExpression)(DbCastExpression)obj).Argument);
				}
				if ((int)expressionKind == 43)
				{
					return ((DbParameterReferenceExpression)obj).ParameterName.GetHashCode() ^ 0x7FFFFFFF;
				}
			}
			else
			{
				if ((int)expressionKind == 46)
				{
					return ((object)((DbPropertyExpression)obj).Property).GetHashCode();
				}
				if ((int)expressionKind == 56)
				{
					return ((DbVariableReferenceExpression)obj).VariableName.GetHashCode();
				}
			}
			return ((object)obj).GetHashCode();
		}
	}

	private Stack<SqlSelectStatement> selectStatementStack;

	private Stack<bool> isParentAJoinStack;

	private Dictionary<string, int> allExtentNames;

	private Dictionary<string, int> allColumnNames;

	private readonly SymbolTable symbolTable = new SymbolTable();

	private bool isVarRefSingle;

	private readonly SymbolUsageManager optionalColumnUsageManager = new SymbolUsageManager();

	private readonly Dictionary<string, bool> _candidateParametersToForceNonUnicode = new Dictionary<string, bool>();

	private bool _forceNonUnicode;

	private bool _ignoreForceNonUnicodeFlag;

	private const byte DefaultDecimalPrecision = 18;

	private static readonly char[] _hexDigits = new char[16]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F'
	};

	private List<string> _targets;

	private static readonly ISet<string> _canonicalAndStoreStringFunctionsOneArg = new HashSet<string>(StringComparer.Ordinal)
	{
		"Edm.Trim", "Edm.RTrim", "Edm.LTrim", "Edm.Left", "Edm.Right", "Edm.Substring", "Edm.ToLower", "Edm.ToUpper", "Edm.Reverse", "SqlServer.RTRIM",
		"SqlServer.LTRIM", "SqlServer.LEFT", "SqlServer.RIGHT", "SqlServer.SUBSTRING", "SqlServer.LOWER", "SqlServer.UPPER", "SqlServer.REVERSE"
	};

	private readonly SqlVersion _sqlVersion;

	private TypeUsage _integerType;

	private StoreItemCollection _storeItemCollection;

	private SqlSelectStatement CurrentSelectStatement => selectStatementStack.Peek();

	private bool IsParentAJoin
	{
		get
		{
			if (isParentAJoinStack.Count != 0)
			{
				return isParentAJoinStack.Peek();
			}
			return false;
		}
	}

	internal Dictionary<string, int> AllExtentNames => allExtentNames;

	internal Dictionary<string, int> AllColumnNames => allColumnNames;

	public List<string> Targets => _targets;

	internal SqlVersion SqlVersion => _sqlVersion;

	internal bool IsPreKatmai => SqlVersionUtils.IsPreKatmai(SqlVersion);

	internal TypeUsage IntegerType => _integerType ?? (_integerType = TypeUsage.CreateDefaultTypeUsage((EdmType)(object)StoreItemCollection.GetPrimitiveTypes().First((PrimitiveType t) => (int)t.PrimitiveTypeKind == 11)));

	internal virtual StoreItemCollection StoreItemCollection => _storeItemCollection;

	internal SqlGenerator()
	{
		_sqlVersion = SqlVersion.Sql11;
	}

	internal SqlGenerator(SqlVersion sqlVersion)
	{
		_sqlVersion = sqlVersion;
	}

	internal static string GenerateSql(DbCommandTree tree, SqlVersion sqlVersion, out List<SqlParameter> parameters, out CommandType commandType, out HashSet<string> paramsToForceNonUnicode)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected I4, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		commandType = CommandType.Text;
		parameters = null;
		paramsToForceNonUnicode = null;
		SqlGenerator sqlGenerator = new SqlGenerator(sqlVersion);
		DbCommandTreeKind commandTreeKind = tree.CommandTreeKind;
		return (int)commandTreeKind switch
		{
			0 => sqlGenerator.GenerateSql((DbQueryCommandTree)tree, out paramsToForceNonUnicode), 
			2 => DmlSqlGenerator.GenerateInsertSql((DbInsertCommandTree)tree, sqlGenerator, out parameters), 
			3 => DmlSqlGenerator.GenerateDeleteSql((DbDeleteCommandTree)tree, sqlGenerator, out parameters), 
			1 => DmlSqlGenerator.GenerateUpdateSql((DbUpdateCommandTree)tree, sqlGenerator, out parameters), 
			4 => GenerateFunctionSql((DbFunctionCommandTree)tree, out commandType), 
			_ => null, 
		};
	}

	private static string GenerateFunctionSql(DbFunctionCommandTree tree, out CommandType commandType)
	{
		EdmFunction edmFunction = tree.EdmFunction;
		if (string.IsNullOrEmpty(edmFunction.CommandTextAttribute))
		{
			commandType = CommandType.StoredProcedure;
			string name = (string.IsNullOrEmpty(edmFunction.Schema) ? ((EdmType)edmFunction).NamespaceName : edmFunction.Schema);
			string name2 = (string.IsNullOrEmpty(edmFunction.StoreFunctionNameAttribute) ? ((EdmType)edmFunction).Name : edmFunction.StoreFunctionNameAttribute);
			string text = QuoteIdentifier(name);
			string text2 = QuoteIdentifier(name2);
			return text + "." + text2;
		}
		commandType = CommandType.Text;
		return edmFunction.CommandTextAttribute;
	}

	internal string GenerateSql(DbQueryCommandTree tree, out HashSet<string> paramsToForceNonUnicode)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Invalid comparison between I4 and Unknown
		_targets = new List<string>();
		DbQueryCommandTree val = tree;
		if (SqlVersion == SqlVersion.Sql8 && Sql8ConformanceChecker.NeedsRewrite(tree.Query))
		{
			val = Sql8ExpressionRewriter.Rewrite(tree);
		}
		_storeItemCollection = (StoreItemCollection)((DbCommandTree)val).MetadataWorkspace.GetItemCollection((DataSpace)2);
		selectStatementStack = new Stack<SqlSelectStatement>();
		isParentAJoinStack = new Stack<bool>();
		allExtentNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		allColumnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		ISqlFragment sqlStatement;
		if (6 == (int)((MetadataItem)val.Query.ResultType.EdmType).BuiltInTypeKind)
		{
			SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(val.Query);
			sqlSelectStatement.IsTopMost = true;
			sqlStatement = sqlSelectStatement;
		}
		else
		{
			SqlBuilder sqlBuilder = new SqlBuilder();
			sqlBuilder.Append("SELECT ");
			sqlBuilder.Append(val.Query.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			sqlStatement = sqlBuilder;
		}
		if (isVarRefSingle)
		{
			throw new NotSupportedException();
		}
		paramsToForceNonUnicode = new HashSet<string>(from p in _candidateParametersToForceNonUnicode
			where p.Value
			select p into q
			select q.Key);
		StringBuilder stringBuilder = new StringBuilder(1024);
		SqlWriter sqlWriter = new SqlWriter(stringBuilder);
		try
		{
			WriteSql(sqlWriter, sqlStatement);
		}
		finally
		{
			((IDisposable)sqlWriter)?.Dispose();
		}
		return stringBuilder.ToString();
	}

	internal SqlWriter WriteSql(SqlWriter writer, ISqlFragment sqlStatement)
	{
		sqlStatement.WriteSql(writer, this);
		return writer;
	}

	public override ISqlFragment Visit(DbAndExpression e)
	{
		Check.NotNull<DbAndExpression>(e, "e");
		return VisitBinaryExpression(" AND ", (DbExpressionKind)1, ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
	}

	public override ISqlFragment Visit(DbApplyExpression e)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		Check.NotNull<DbApplyExpression>(e, "e");
		List<DbExpressionBinding> list = new List<DbExpressionBinding>();
		list.Add(e.Input);
		list.Add(e.Apply);
		DbExpressionKind expressionKind = ((DbExpression)e).ExpressionKind;
		string joinString;
		if ((int)expressionKind != 6)
		{
			if ((int)expressionKind != 42)
			{
				throw new InvalidOperationException(string.Empty);
			}
			joinString = "OUTER APPLY";
		}
		else
		{
			joinString = "CROSS APPLY";
		}
		return VisitJoinExpression(list, (DbExpressionKind)7, joinString, null);
	}

	public override ISqlFragment Visit(DbArithmeticExpression e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected I4, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbArithmeticExpression>(e, "e");
		DbExpressionKind expressionKind = ((DbExpression)e).ExpressionKind;
		SqlBuilder sqlBuilder;
		if ((int)expressionKind <= 34)
		{
			if ((int)expressionKind != 10)
			{
				switch (expressionKind - 32)
				{
				case 0:
					break;
				case 1:
					goto IL_00aa;
				case 2:
					goto IL_00d9;
				default:
					goto IL_0167;
				}
				sqlBuilder = VisitBinaryExpression(" - ", ((DbExpression)e).ExpressionKind, e.Arguments[0], e.Arguments[1]);
			}
			else
			{
				sqlBuilder = VisitBinaryExpression(" / ", ((DbExpression)e).ExpressionKind, e.Arguments[0], e.Arguments[1]);
			}
		}
		else if ((int)expressionKind != 44)
		{
			if ((int)expressionKind != 54)
			{
				goto IL_0167;
			}
			sqlBuilder = new SqlBuilder();
			sqlBuilder.Append(" -(");
			sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			sqlBuilder.Append(")");
		}
		else
		{
			sqlBuilder = VisitBinaryExpression(" + ", ((DbExpression)e).ExpressionKind, e.Arguments[0], e.Arguments[1]);
		}
		goto IL_0172;
		IL_00aa:
		sqlBuilder = VisitBinaryExpression(" % ", ((DbExpression)e).ExpressionKind, e.Arguments[0], e.Arguments[1]);
		goto IL_0172;
		IL_0172:
		return sqlBuilder;
		IL_00d9:
		sqlBuilder = VisitBinaryExpression(" * ", ((DbExpression)e).ExpressionKind, e.Arguments[0], e.Arguments[1]);
		goto IL_0172;
		IL_0167:
		throw new InvalidOperationException(string.Empty);
	}

	public override ISqlFragment Visit(DbCaseExpression e)
	{
		Check.NotNull<DbCaseExpression>(e, "e");
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("CASE");
		for (int i = 0; i < e.When.Count; i++)
		{
			sqlBuilder.Append(" WHEN (");
			sqlBuilder.Append(e.When[i].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			sqlBuilder.Append(") THEN ");
			sqlBuilder.Append(e.Then[i].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		}
		if (e.Else != null && !(e.Else is DbNullExpression))
		{
			sqlBuilder.Append(" ELSE ");
			sqlBuilder.Append(e.Else.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		}
		sqlBuilder.Append(" END");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbCastExpression e)
	{
		Check.NotNull<DbCastExpression>(e, "e");
		if (((DbExpression)e).ResultType.IsSpatialType())
		{
			return ((DbUnaryExpression)e).Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(" CAST( ");
		sqlBuilder.Append(((DbUnaryExpression)e).Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		sqlBuilder.Append(" AS ");
		sqlBuilder.Append(GetSqlPrimitiveType(((DbExpression)e).ResultType));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbComparisonExpression e)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Invalid comparison between Unknown and I4
		Check.NotNull<DbComparisonExpression>(e, "e");
		if (((DbBinaryExpression)e).Left.ResultType.IsPrimitiveType((PrimitiveTypeKind)12))
		{
			_forceNonUnicode = CheckIfForceNonUnicodeRequired((DbExpression)(object)e);
		}
		DbExpressionKind expressionKind = ((DbExpression)e).ExpressionKind;
		SqlBuilder result;
		if ((int)expressionKind <= 19)
		{
			if ((int)expressionKind != 13)
			{
				if ((int)expressionKind != 18)
				{
					if ((int)expressionKind != 19)
					{
						goto IL_0106;
					}
					result = VisitComparisonExpression(" >= ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
				}
				else
				{
					result = VisitComparisonExpression(" > ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
				}
			}
			else
			{
				result = VisitComparisonExpression(" = ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
		}
		else if ((int)expressionKind != 28)
		{
			if ((int)expressionKind != 29)
			{
				if ((int)expressionKind != 37)
				{
					goto IL_0106;
				}
				result = VisitComparisonExpression(" <> ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
			else
			{
				result = VisitComparisonExpression(" <= ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
		}
		else
		{
			result = VisitComparisonExpression(" < ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
		}
		_forceNonUnicode = false;
		return result;
		IL_0106:
		throw new InvalidOperationException(string.Empty);
	}

	private bool CheckIfForceNonUnicodeRequired(DbExpression e)
	{
		if (_forceNonUnicode)
		{
			throw new NotSupportedException();
		}
		return MatchPatternForForcingNonUnicode(e);
	}

	private bool MatchPatternForForcingNonUnicode(DbExpression e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		DbExpressionKind expressionKind = e.ExpressionKind;
		if ((int)expressionKind != 30)
		{
			if ((int)expressionKind == 58)
			{
				return MatchSourcePatternForForcingNonUnicode(((DbInExpression)e).Item);
			}
			DbComparisonExpression val = (DbComparisonExpression)e;
			DbExpression left = ((DbBinaryExpression)val).Left;
			DbExpression right = ((DbBinaryExpression)val).Right;
			if (!MatchSourcePatternForForcingNonUnicode(left) || !MatchTargetPatternForForcingNonUnicode(right))
			{
				if (MatchSourcePatternForForcingNonUnicode(right))
				{
					return MatchTargetPatternForForcingNonUnicode(left);
				}
				return false;
			}
			return true;
		}
		DbLikeExpression val2 = (DbLikeExpression)e;
		if (MatchSourcePatternForForcingNonUnicode(val2.Argument) && MatchTargetPatternForForcingNonUnicode(val2.Pattern))
		{
			return MatchTargetPatternForForcingNonUnicode(val2.Escape);
		}
		return false;
	}

	internal bool MatchTargetPatternForForcingNonUnicode(DbExpression expr)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (IsConstParamOrNullExpressionUnicodeNotSpecified(expr))
		{
			return true;
		}
		if ((int)expr.ExpressionKind == 17)
		{
			DbFunctionExpression val = (DbFunctionExpression)expr;
			EdmFunction function = val.Function;
			if (!function.IsCanonicalFunction() && !SqlFunctionCallHandler.IsStoreFunction(function))
			{
				return false;
			}
			string fullName = ((EdmType)function).FullName;
			if (_canonicalAndStoreStringFunctionsOneArg.Contains(fullName))
			{
				return MatchTargetPatternForForcingNonUnicode(val.Arguments[0]);
			}
			if ("Edm.Concat".Equals(fullName, StringComparison.Ordinal))
			{
				if (MatchTargetPatternForForcingNonUnicode(val.Arguments[0]))
				{
					return MatchTargetPatternForForcingNonUnicode(val.Arguments[1]);
				}
				return false;
			}
			if ("Edm.Replace".Equals(fullName, StringComparison.Ordinal) || "SqlServer.REPLACE".Equals(fullName, StringComparison.Ordinal))
			{
				if (MatchTargetPatternForForcingNonUnicode(val.Arguments[0]) && MatchTargetPatternForForcingNonUnicode(val.Arguments[1]))
				{
					return MatchTargetPatternForForcingNonUnicode(val.Arguments[2]);
				}
				return false;
			}
		}
		return false;
	}

	private static bool MatchSourcePatternForForcingNonUnicode(DbExpression argument)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)argument.ExpressionKind == 46 && argument.ResultType.TryGetIsUnicode(out var isUnicode))
		{
			return !isUnicode;
		}
		return false;
	}

	internal static bool IsConstParamOrNullExpressionUnicodeNotSpecified(DbExpression argument)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		DbExpressionKind expressionKind = argument.ExpressionKind;
		TypeUsage resultType = argument.ResultType;
		if (!resultType.IsPrimitiveType((PrimitiveTypeKind)12))
		{
			return false;
		}
		bool value;
		if ((int)expressionKind == 5 || (int)expressionKind == 43 || (int)expressionKind == 38)
		{
			return !resultType.TryGetFacetValue<bool>("Unicode", out value);
		}
		return false;
	}

	private ISqlFragment VisitConstant(DbConstantExpression e, bool isCastOptional)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected I4, but got Unknown
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Expected O, but got Unknown
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Expected O, but got Unknown
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Expected O, but got Unknown
		SqlBuilder sqlBuilder = new SqlBuilder();
		TypeUsage resultType = ((DbExpression)e).ResultType;
		if (resultType.IsPrimitiveType())
		{
			PrimitiveTypeKind primitiveTypeKind = resultType.GetPrimitiveTypeKind();
			switch ((int)primitiveTypeKind)
			{
			case 10:
				sqlBuilder.Append(e.Value.ToString());
				break;
			case 0:
				sqlBuilder.Append(" 0x");
				sqlBuilder.Append(ByteArrayToBinaryString((byte[])e.Value));
				sqlBuilder.Append(" ");
				break;
			case 1:
				WrapWithCastIfNeeded(!isCastOptional, ((bool)e.Value) ? "1" : "0", "bit", sqlBuilder);
				break;
			case 2:
				WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "tinyint", sqlBuilder);
				break;
			case 3:
				sqlBuilder.Append("convert(");
				sqlBuilder.Append(IsPreKatmai ? "datetime" : "datetime2");
				sqlBuilder.Append(", ");
				sqlBuilder.Append(EscapeSingleQuote(((DateTime)e.Value).ToString(IsPreKatmai ? "yyyy-MM-dd HH:mm:ss.fff" : "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture), isUnicode: false));
				sqlBuilder.Append(", 121)");
				break;
			case 13:
				AssertKatmaiOrNewer(primitiveTypeKind);
				sqlBuilder.Append("convert(");
				sqlBuilder.Append(((DbExpression)e).ResultType.EdmType.Name);
				sqlBuilder.Append(", ");
				sqlBuilder.Append(EscapeSingleQuote(e.Value.ToString(), isUnicode: false));
				sqlBuilder.Append(", 121)");
				break;
			case 14:
				AssertKatmaiOrNewer(primitiveTypeKind);
				sqlBuilder.Append("convert(");
				sqlBuilder.Append(((DbExpression)e).ResultType.EdmType.Name);
				sqlBuilder.Append(", ");
				sqlBuilder.Append(EscapeSingleQuote(((DateTimeOffset)e.Value).ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture), isUnicode: false));
				sqlBuilder.Append(", 121)");
				break;
			case 4:
			{
				string text = ((decimal)e.Value).ToString(CultureInfo.InvariantCulture);
				bool cast = -1 == text.IndexOf('.') && text.TrimStart(new char[1] { '-' }).Length < 20;
				string typeName = "decimal(" + Math.Max((byte)text.Length, (byte)18).ToString(CultureInfo.InvariantCulture) + ")";
				WrapWithCastIfNeeded(cast, text, typeName, sqlBuilder);
				break;
			}
			case 5:
			{
				double value2 = (double)e.Value;
				AssertValidDouble(value2);
				WrapWithCastIfNeeded(cast: true, value2.ToString("R", CultureInfo.InvariantCulture), "float(53)", sqlBuilder);
				break;
			}
			case 16:
				AppendSpatialConstant(sqlBuilder, IDbSpatialValueExtensionMethods.AsSpatialValue((DbGeography)e.Value));
				break;
			case 15:
				AppendSpatialConstant(sqlBuilder, IDbSpatialValueExtensionMethods.AsSpatialValue((DbGeometry)e.Value));
				break;
			case 6:
				WrapWithCastIfNeeded(cast: true, EscapeSingleQuote(e.Value.ToString(), isUnicode: false), "uniqueidentifier", sqlBuilder);
				break;
			case 31:
				AppendHierarchyConstant(sqlBuilder, (HierarchyId)e.Value);
				break;
			case 9:
				WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "smallint", sqlBuilder);
				break;
			case 11:
				WrapWithCastIfNeeded(!isCastOptional, e.Value.ToString(), "bigint", sqlBuilder);
				break;
			case 7:
			{
				float value = (float)e.Value;
				AssertValidSingle(value);
				WrapWithCastIfNeeded(cast: true, value.ToString("R", CultureInfo.InvariantCulture), "real", sqlBuilder);
				break;
			}
			case 12:
			{
				if (!((DbExpression)e).ResultType.TryGetIsUnicode(out var isUnicode))
				{
					isUnicode = !_forceNonUnicode;
				}
				sqlBuilder.Append(EscapeSingleQuote(e.Value as string, isUnicode));
				break;
			}
			default:
				throw new NotSupportedException(Strings.NoStoreTypeForEdmType(resultType.EdmType.Name, ((PrimitiveType)resultType.EdmType).PrimitiveTypeKind));
			}
			return sqlBuilder;
		}
		throw new NotSupportedException();
	}

	private static void AppendHierarchyConstant(SqlBuilder result, HierarchyId hierarchyId)
	{
		result.Append("cast(");
		result.Append(EscapeSingleQuote(((object)hierarchyId).ToString(), isUnicode: false));
		result.Append(" as hierarchyid)");
	}

	private void AppendSpatialConstant(SqlBuilder result, IDbSpatialValue spatialValue)
	{
		DbFunctionExpression val = null;
		int? coordinateSystemId = spatialValue.CoordinateSystemId;
		if (coordinateSystemId.HasValue)
		{
			string wellKnownText = spatialValue.WellKnownText;
			if (wellKnownText != null)
			{
				val = (spatialValue.IsGeography ? SpatialEdmFunctions.GeographyFromText(DbExpression.op_Implicit(wellKnownText), DbExpression.op_Implicit((int?)coordinateSystemId.Value)) : SpatialEdmFunctions.GeometryFromText(DbExpression.op_Implicit(wellKnownText), DbExpression.op_Implicit((int?)coordinateSystemId.Value)));
			}
			else
			{
				byte[] wellKnownBinary = spatialValue.WellKnownBinary;
				if (wellKnownBinary != null)
				{
					val = (spatialValue.IsGeography ? SpatialEdmFunctions.GeographyFromBinary(DbExpression.op_Implicit(wellKnownBinary), DbExpression.op_Implicit((int?)coordinateSystemId.Value)) : SpatialEdmFunctions.GeometryFromBinary(DbExpression.op_Implicit(wellKnownBinary), DbExpression.op_Implicit((int?)coordinateSystemId.Value)));
				}
				else
				{
					string gmlString = spatialValue.GmlString;
					if (gmlString != null)
					{
						val = (spatialValue.IsGeography ? SpatialEdmFunctions.GeographyFromGml(DbExpression.op_Implicit(gmlString), DbExpression.op_Implicit((int?)coordinateSystemId.Value)) : SpatialEdmFunctions.GeometryFromGml(DbExpression.op_Implicit(gmlString), DbExpression.op_Implicit((int?)coordinateSystemId.Value)));
					}
				}
			}
		}
		if (val != null)
		{
			result.Append(SqlFunctionCallHandler.GenerateFunctionCallSql(this, val));
			return;
		}
		throw spatialValue.NotSqlCompatible();
	}

	private static void AssertValidDouble(double value)
	{
		if (double.IsNaN(value))
		{
			throw new NotSupportedException(Strings.SqlGen_TypedNaNNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), (object)(PrimitiveTypeKind)5)));
		}
		if (double.IsPositiveInfinity(value))
		{
			throw new NotSupportedException(Strings.SqlGen_TypedPositiveInfinityNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), (object)(PrimitiveTypeKind)5), typeof(double).Name));
		}
		if (double.IsNegativeInfinity(value))
		{
			throw new NotSupportedException(Strings.SqlGen_TypedNegativeInfinityNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), (object)(PrimitiveTypeKind)5), typeof(double).Name));
		}
	}

	private static void AssertValidSingle(float value)
	{
		if (float.IsNaN(value))
		{
			throw new NotSupportedException(Strings.SqlGen_TypedNaNNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), (object)(PrimitiveTypeKind)7)));
		}
		if (float.IsPositiveInfinity(value))
		{
			throw new NotSupportedException(Strings.SqlGen_TypedPositiveInfinityNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), (object)(PrimitiveTypeKind)7), typeof(float).Name));
		}
		if (float.IsNegativeInfinity(value))
		{
			throw new NotSupportedException(Strings.SqlGen_TypedNegativeInfinityNotSupported(Enum.GetName(typeof(PrimitiveTypeKind), (object)(PrimitiveTypeKind)7), typeof(float).Name));
		}
	}

	private static void WrapWithCastIfNeeded(bool cast, string value, string typeName, SqlBuilder result)
	{
		if (!cast)
		{
			result.Append(value);
			return;
		}
		result.Append("cast(");
		result.Append(value);
		result.Append(" as ");
		result.Append(typeName);
		result.Append(")");
	}

	public override ISqlFragment Visit(DbConstantExpression e)
	{
		Check.NotNull<DbConstantExpression>(e, "e");
		return VisitConstant(e, isCastOptional: false);
	}

	public override ISqlFragment Visit(DbDerefExpression e)
	{
		Check.NotNull<DbDerefExpression>(e, "e");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbDistinctExpression e)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbDistinctExpression>(e, "e");
		SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(((DbUnaryExpression)e).Argument);
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			TypeUsage elementTypeUsage = ((DbUnaryExpression)e).Argument.ResultType.GetElementTypeUsage();
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, "distinct", elementTypeUsage, out var fromSymbol);
			AddFromSymbol(sqlSelectStatement, "distinct", fromSymbol, addToSymbolTable: false);
		}
		sqlSelectStatement.Select.IsDistinct = true;
		return sqlSelectStatement;
	}

	public override ISqlFragment Visit(DbElementExpression e)
	{
		Check.NotNull<DbElementExpression>(e, "e");
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("(");
		sqlBuilder.Append(VisitExpressionEnsureSqlStatement(((DbUnaryExpression)e).Argument));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbExceptExpression e)
	{
		Check.NotNull<DbExceptExpression>(e, "e");
		return VisitSetOpExpression((DbBinaryExpression)(object)e, "EXCEPT");
	}

	public override ISqlFragment Visit(DbExpression e)
	{
		Check.NotNull<DbExpression>(e, "e");
		throw new InvalidOperationException(string.Empty);
	}

	public override ISqlFragment Visit(DbScanExpression e)
	{
		Check.NotNull<DbScanExpression>(e, "e");
		string targetTSql = GetTargetTSql(e.Target);
		if (_targets != null)
		{
			_targets.Add(targetTSql);
		}
		if (IsParentAJoin)
		{
			SqlBuilder sqlBuilder = new SqlBuilder();
			sqlBuilder.Append(targetTSql);
			return sqlBuilder;
		}
		SqlSelectStatement sqlSelectStatement = new SqlSelectStatement();
		sqlSelectStatement.From.Append(targetTSql);
		return sqlSelectStatement;
	}

	internal static string GetTargetTSql(EntitySetBase entitySetBase)
	{
		string metadataPropertyValue = ((MetadataItem)(object)entitySetBase).GetMetadataPropertyValue<string>("DefiningQuery");
		if (metadataPropertyValue != null)
		{
			return "(" + metadataPropertyValue + ")";
		}
		StringBuilder stringBuilder = new StringBuilder(50);
		string metadataPropertyValue2 = ((MetadataItem)(object)entitySetBase).GetMetadataPropertyValue<string>("Schema");
		if (!string.IsNullOrEmpty(metadataPropertyValue2))
		{
			stringBuilder.Append(QuoteIdentifier(metadataPropertyValue2));
			stringBuilder.Append(".");
		}
		else
		{
			stringBuilder.Append(QuoteIdentifier(entitySetBase.EntityContainer.Name));
			stringBuilder.Append(".");
		}
		string metadataPropertyValue3 = ((MetadataItem)(object)entitySetBase).GetMetadataPropertyValue<string>("Table");
		stringBuilder.Append(string.IsNullOrEmpty(metadataPropertyValue3) ? QuoteIdentifier(entitySetBase.Name) : QuoteIdentifier(metadataPropertyValue3));
		return stringBuilder.ToString();
	}

	public override ISqlFragment Visit(DbFilterExpression e)
	{
		Check.NotNull<DbFilterExpression>(e, "e");
		return VisitFilterExpression(e.Input, e.Predicate, negatePredicate: false);
	}

	public override ISqlFragment Visit(DbFunctionExpression e)
	{
		Check.NotNull<DbFunctionExpression>(e, "e");
		return SqlFunctionCallHandler.GenerateFunctionCallSql(this, e);
	}

	public override ISqlFragment Visit(DbLambdaExpression expression)
	{
		Check.NotNull<DbLambdaExpression>(expression, "expression");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbEntityRefExpression e)
	{
		Check.NotNull<DbEntityRefExpression>(e, "e");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbRefKeyExpression e)
	{
		Check.NotNull<DbRefKeyExpression>(e, "e");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbGroupByExpression e)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbGroupByExpression>(e, "e");
		Symbol fromSymbol;
		SqlSelectStatement sqlSelectStatement = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		}
		selectStatementStack.Push(sqlSelectStatement);
		symbolTable.EnterScope();
		AddFromSymbol(sqlSelectStatement, e.Input.VariableName, fromSymbol);
		symbolTable.Add(e.Input.GroupVariableName, fromSymbol);
		RowType val = (RowType)((CollectionType)((DbExpression)e).ResultType.EdmType).TypeUsage.EdmType;
		bool flag = GroupByAggregatesNeedInnerQuery(e.Aggregates, e.Input.GroupVariableName) || GroupByKeysNeedInnerQuery(e.Keys, e.Input.VariableName);
		SqlSelectStatement sqlSelectStatement2;
		if (flag)
		{
			sqlSelectStatement2 = CreateNewSelectStatement(sqlSelectStatement, e.Input.VariableName, e.Input.VariableType, finalizeOldStatement: false, out fromSymbol);
			AddFromSymbol(sqlSelectStatement2, e.Input.VariableName, fromSymbol, addToSymbolTable: false);
		}
		else
		{
			sqlSelectStatement2 = sqlSelectStatement;
		}
		using (IEnumerator<EdmProperty> enumerator = (object)val.Properties.GetEnumerator())
		{
			enumerator.MoveNext();
			string s = "";
			foreach (DbExpression key in e.Keys)
			{
				string s2 = QuoteIdentifier(((EdmMember)enumerator.Current).Name);
				sqlSelectStatement2.GroupBy.Append(s);
				ISqlFragment s3 = key.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
				if (!flag)
				{
					sqlSelectStatement2.Select.Append(s);
					sqlSelectStatement2.Select.AppendLine();
					sqlSelectStatement2.Select.Append(s3);
					sqlSelectStatement2.Select.Append(" AS ");
					sqlSelectStatement2.Select.Append(s2);
					sqlSelectStatement2.GroupBy.Append(s3);
				}
				else
				{
					sqlSelectStatement.Select.Append(s);
					sqlSelectStatement.Select.AppendLine();
					sqlSelectStatement.Select.Append(s3);
					sqlSelectStatement.Select.Append(" AS ");
					sqlSelectStatement.Select.Append(s2);
					sqlSelectStatement2.Select.Append(s);
					sqlSelectStatement2.Select.AppendLine();
					sqlSelectStatement2.Select.Append(fromSymbol);
					sqlSelectStatement2.Select.Append(".");
					sqlSelectStatement2.Select.Append(s2);
					sqlSelectStatement2.Select.Append(" AS ");
					sqlSelectStatement2.Select.Append(s2);
					sqlSelectStatement2.GroupBy.Append(s2);
				}
				s = ", ";
				enumerator.MoveNext();
			}
			foreach (DbAggregate aggregate in e.Aggregates)
			{
				EdmProperty current3 = enumerator.Current;
				string s4 = QuoteIdentifier(((EdmMember)current3).Name);
				List<object> list = new List<object>();
				for (int i = 0; i < aggregate.Arguments.Count; i++)
				{
					ISqlFragment sqlFragment = aggregate.Arguments[i].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
					object item;
					if (flag)
					{
						string s5 = QuoteIdentifier(((EdmMember)current3).Name + "_" + i);
						SqlBuilder sqlBuilder = new SqlBuilder();
						sqlBuilder.Append(fromSymbol);
						sqlBuilder.Append(".");
						sqlBuilder.Append(s5);
						item = sqlBuilder;
						sqlSelectStatement.Select.Append(s);
						sqlSelectStatement.Select.AppendLine();
						sqlSelectStatement.Select.Append(sqlFragment);
						sqlSelectStatement.Select.Append(" AS ");
						sqlSelectStatement.Select.Append(s5);
					}
					else
					{
						item = sqlFragment;
					}
					list.Add(item);
				}
				ISqlFragment s6 = VisitAggregate(aggregate, list);
				sqlSelectStatement2.Select.Append(s);
				sqlSelectStatement2.Select.AppendLine();
				sqlSelectStatement2.Select.Append(s6);
				sqlSelectStatement2.Select.Append(" AS ");
				sqlSelectStatement2.Select.Append(s4);
				s = ", ";
				enumerator.MoveNext();
			}
		}
		symbolTable.ExitScope();
		selectStatementStack.Pop();
		return sqlSelectStatement2;
	}

	public override ISqlFragment Visit(DbIntersectExpression e)
	{
		Check.NotNull<DbIntersectExpression>(e, "e");
		return VisitSetOpExpression((DbBinaryExpression)(object)e, "INTERSECT");
	}

	public override ISqlFragment Visit(DbIsEmptyExpression e)
	{
		Check.NotNull<DbIsEmptyExpression>(e, "e");
		return VisitIsEmptyExpression(e, negate: false);
	}

	public override ISqlFragment Visit(DbIsNullExpression e)
	{
		Check.NotNull<DbIsNullExpression>(e, "e");
		return VisitIsNullExpression(e, negate: false);
	}

	public override ISqlFragment Visit(DbIsOfExpression e)
	{
		Check.NotNull<DbIsOfExpression>(e, "e");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbCrossJoinExpression e)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbCrossJoinExpression>(e, "e");
		return VisitJoinExpression(e.Inputs, ((DbExpression)e).ExpressionKind, "CROSS JOIN", null);
	}

	public override ISqlFragment Visit(DbJoinExpression e)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		Check.NotNull<DbJoinExpression>(e, "e");
		DbExpressionKind expressionKind = ((DbExpression)e).ExpressionKind;
		string joinString = (((int)expressionKind == 16) ? "FULL OUTER JOIN" : (((int)expressionKind == 21) ? "INNER JOIN" : (((int)expressionKind != 27) ? null : "LEFT OUTER JOIN")));
		List<DbExpressionBinding> list = new List<DbExpressionBinding>(2);
		list.Add(e.Left);
		list.Add(e.Right);
		return VisitJoinExpression(list, ((DbExpression)e).ExpressionKind, joinString, e.JoinCondition);
	}

	public override ISqlFragment Visit(DbLikeExpression e)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		Check.NotNull<DbLikeExpression>(e, "e");
		_forceNonUnicode = CheckIfForceNonUnicodeRequired((DbExpression)(object)e);
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(e.Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		sqlBuilder.Append(" LIKE ");
		sqlBuilder.Append(e.Pattern.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		if ((int)e.Escape.ExpressionKind != 38)
		{
			sqlBuilder.Append(" ESCAPE ");
			sqlBuilder.Append(e.Escape.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		}
		_forceNonUnicode = false;
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbLimitExpression e)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbLimitExpression>(e, "e");
		SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(e.Argument, addDefaultColumns: false, markAllDefaultColumnsAsUsed: false);
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			TypeUsage elementTypeUsage = e.Argument.ResultType.GetElementTypeUsage();
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, "top", elementTypeUsage, out var fromSymbol);
			AddFromSymbol(sqlSelectStatement, "top", fromSymbol, addToSymbolTable: false);
		}
		ISqlFragment topCount = HandleCountExpression(e.Limit);
		sqlSelectStatement.Select.Top = new TopClause(topCount, e.WithTies);
		return sqlSelectStatement;
	}

	public override ISqlFragment Visit(DbNewInstanceExpression e)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between I4 and Unknown
		Check.NotNull<DbNewInstanceExpression>(e, "e");
		if (6 == (int)((MetadataItem)((DbExpression)e).ResultType.EdmType).BuiltInTypeKind)
		{
			return VisitCollectionConstructor(e);
		}
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbNotExpression e)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		Check.NotNull<DbNotExpression>(e, "e");
		DbExpression argument = ((DbUnaryExpression)e).Argument;
		DbNotExpression val = (DbNotExpression)(object)((argument is DbNotExpression) ? argument : null);
		if (val != null)
		{
			return ((DbUnaryExpression)val).Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
		}
		DbExpression argument2 = ((DbUnaryExpression)e).Argument;
		DbIsEmptyExpression val2 = (DbIsEmptyExpression)(object)((argument2 is DbIsEmptyExpression) ? argument2 : null);
		if (val2 != null)
		{
			return VisitIsEmptyExpression(val2, negate: true);
		}
		DbExpression argument3 = ((DbUnaryExpression)e).Argument;
		DbIsNullExpression val3 = (DbIsNullExpression)(object)((argument3 is DbIsNullExpression) ? argument3 : null);
		if (val3 != null)
		{
			return VisitIsNullExpression(val3, negate: true);
		}
		DbExpression argument4 = ((DbUnaryExpression)e).Argument;
		DbComparisonExpression val4 = (DbComparisonExpression)(object)((argument4 is DbComparisonExpression) ? argument4 : null);
		if (val4 != null && (int)((DbExpression)val4).ExpressionKind == 13)
		{
			bool forceNonUnicode = _forceNonUnicode;
			if (((DbBinaryExpression)val4).Left.ResultType.IsPrimitiveType((PrimitiveTypeKind)12))
			{
				_forceNonUnicode = CheckIfForceNonUnicodeRequired((DbExpression)(object)val4);
			}
			SqlBuilder result = VisitComparisonExpression(" <> ", ((DbBinaryExpression)val4).Left, ((DbBinaryExpression)val4).Right);
			_forceNonUnicode = forceNonUnicode;
			return result;
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(" NOT (");
		sqlBuilder.Append(((DbUnaryExpression)e).Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbNullExpression e)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		Check.NotNull<DbNullExpression>(e, "e");
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("CAST(NULL AS ");
		TypeUsage resultType = ((DbExpression)e).ResultType;
		EdmType edmType = resultType.EdmType;
		PrimitiveTypeKind primitiveTypeKind = ((PrimitiveType)((edmType is PrimitiveType) ? edmType : null)).PrimitiveTypeKind;
		if ((int)primitiveTypeKind != 0)
		{
			if ((int)primitiveTypeKind == 12)
			{
				sqlBuilder.Append("varchar(1)");
			}
			else
			{
				sqlBuilder.Append(GetSqlPrimitiveType(resultType));
			}
		}
		else
		{
			sqlBuilder.Append("varbinary(1)");
		}
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbOfTypeExpression e)
	{
		Check.NotNull<DbOfTypeExpression>(e, "e");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbOrExpression e)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbOrExpression>(e, "e");
		ISqlFragment sqlFragment = null;
		if (TryTranslateIntoIn(e, out sqlFragment))
		{
			return sqlFragment;
		}
		return VisitBinaryExpression(" OR ", ((DbExpression)e).ExpressionKind, ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
	}

	public override ISqlFragment Visit(DbInExpression e)
	{
		Check.NotNull<DbInExpression>(e, "e");
		if (e.List.Count == 0)
		{
			return ((DbExpressionVisitor<ISqlFragment>)this).Visit(DbExpressionBuilder.False);
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (e.Item.ResultType.IsPrimitiveType((PrimitiveTypeKind)12))
		{
			_forceNonUnicode = CheckIfForceNonUnicodeRequired((DbExpression)(object)e);
		}
		sqlBuilder.Append(e.Item.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		sqlBuilder.Append(" IN (");
		bool flag = true;
		foreach (DbExpression item in e.List)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sqlBuilder.Append(", ");
			}
			sqlBuilder.Append(item.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		}
		sqlBuilder.Append(")");
		_forceNonUnicode = false;
		return sqlBuilder;
	}

	internal static IDictionary<DbExpression, IList<DbExpression>> HasBuiltMapForIn(DbOrExpression expression)
	{
		Dictionary<DbExpression, IList<DbExpression>> dictionary = new Dictionary<DbExpression, IList<DbExpression>>(new KeyFieldExpressionComparer());
		if (!HasBuiltMapForIn((DbExpression)(object)expression, dictionary))
		{
			return null;
		}
		return dictionary;
	}

	private bool TryTranslateIntoIn(DbOrExpression e, out ISqlFragment sqlFragment)
	{
		IDictionary<DbExpression, IList<DbExpression>> dictionary = HasBuiltMapForIn(e);
		if (dictionary == null)
		{
			sqlFragment = null;
			return false;
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		bool flag = true;
		foreach (DbExpression key in dictionary.Keys)
		{
			IList<DbExpression> source = dictionary[key];
			if (!flag)
			{
				sqlBuilder.Append(" OR ");
			}
			else
			{
				flag = false;
			}
			IEnumerable<DbExpression> enumerable = source.Where((DbExpression v) => (int)v.ExpressionKind != 24);
			int num = enumerable.Count();
			bool flag2 = false;
			bool forceNonUnicodeOnKey = false;
			if (key.ResultType.IsPrimitiveType((PrimitiveTypeKind)12))
			{
				flag2 = MatchSourcePatternForForcingNonUnicode(key);
				forceNonUnicodeOnKey = !flag2 && MatchTargetPatternForForcingNonUnicode(key) && enumerable.All(MatchSourcePatternForForcingNonUnicode);
			}
			if (num == 1)
			{
				HandleInKey(sqlBuilder, key, forceNonUnicodeOnKey);
				sqlBuilder.Append(" = ");
				DbExpression val = enumerable.First();
				HandleInValue(sqlBuilder, val, key.ResultType.EdmType == val.ResultType.EdmType, flag2);
			}
			if (num > 1)
			{
				HandleInKey(sqlBuilder, key, forceNonUnicodeOnKey);
				sqlBuilder.Append(" IN (");
				bool flag3 = true;
				foreach (DbExpression item in enumerable)
				{
					if (!flag3)
					{
						sqlBuilder.Append(",");
					}
					else
					{
						flag3 = false;
					}
					HandleInValue(sqlBuilder, item, key.ResultType.EdmType == item.ResultType.EdmType, flag2);
				}
				sqlBuilder.Append(")");
			}
			DbExpression? obj = ((IEnumerable<DbExpression>)source).FirstOrDefault((Func<DbExpression, bool>)((DbExpression v) => (int)v.ExpressionKind == 24));
			DbIsNullExpression val2 = (DbIsNullExpression)(object)((obj is DbIsNullExpression) ? obj : null);
			if (val2 != null)
			{
				if (num > 0)
				{
					sqlBuilder.Append(" OR ");
				}
				sqlBuilder.Append(VisitIsNullExpression(val2, negate: false));
			}
		}
		sqlFragment = sqlBuilder;
		return true;
	}

	private void HandleInValue(SqlBuilder sqlBuilder, DbExpression value, bool isSameEdmType, bool forceNonUnicodeOnQualifyingValues)
	{
		ForcingNonUnicode(delegate
		{
			ParenthesizeExpressionWithoutRedundantConstantCasts(value, sqlBuilder, isSameEdmType);
		}, forceNonUnicodeOnQualifyingValues && MatchTargetPatternForForcingNonUnicode(value));
	}

	private void HandleInKey(SqlBuilder sqlBuilder, DbExpression key, bool forceNonUnicodeOnKey)
	{
		ForcingNonUnicode(delegate
		{
			ParenthesizeExpressionIfNeeded(key, sqlBuilder);
		}, forceNonUnicodeOnKey);
	}

	private void ForcingNonUnicode(Action action, bool forceNonUnicode)
	{
		bool flag = false;
		if (forceNonUnicode && !_forceNonUnicode)
		{
			_forceNonUnicode = true;
			flag = true;
		}
		action();
		if (flag)
		{
			_forceNonUnicode = false;
		}
	}

	private void ParenthesizeExpressionWithoutRedundantConstantCasts(DbExpression value, SqlBuilder sqlBuilder, bool isSameEdmType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		if ((int)value.ExpressionKind == 5)
		{
			sqlBuilder.Append(VisitConstant((DbConstantExpression)value, isSameEdmType));
		}
		else
		{
			ParenthesizeExpressionIfNeeded(value, sqlBuilder);
		}
	}

	internal static bool IsKeyForIn(DbExpression e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		if ((int)e.ExpressionKind != 46 && (int)e.ExpressionKind != 56)
		{
			return (int)e.ExpressionKind == 43;
		}
		return true;
	}

	internal static bool TryAddExpressionForIn(DbBinaryExpression e, IDictionary<DbExpression, IList<DbExpression>> values)
	{
		if (IsKeyForIn(e.Left))
		{
			values.Add(e.Left, e.Right);
			return true;
		}
		if (IsKeyForIn(e.Right))
		{
			values.Add(e.Right, e.Left);
			return true;
		}
		return false;
	}

	internal static bool HasBuiltMapForIn(DbExpression e, IDictionary<DbExpression, IList<DbExpression>> values)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		DbExpressionKind expressionKind = e.ExpressionKind;
		if ((int)expressionKind != 13)
		{
			if ((int)expressionKind != 24)
			{
				if ((int)expressionKind == 41)
				{
					DbBinaryExpression val = (DbBinaryExpression)e;
					if (HasBuiltMapForIn(val.Left, values))
					{
						return HasBuiltMapForIn(val.Right, values);
					}
					return false;
				}
				return false;
			}
			DbExpression argument = ((DbUnaryExpression)(DbIsNullExpression)e).Argument;
			if (IsKeyForIn(argument))
			{
				values.Add(argument, e);
				return true;
			}
			return false;
		}
		return TryAddExpressionForIn((DbBinaryExpression)e, values);
	}

	public override ISqlFragment Visit(DbParameterReferenceExpression e)
	{
		Check.NotNull<DbParameterReferenceExpression>(e, "e");
		if (!_ignoreForceNonUnicodeFlag)
		{
			if (!_forceNonUnicode)
			{
				_candidateParametersToForceNonUnicode[e.ParameterName] = false;
			}
			else if (!_candidateParametersToForceNonUnicode.ContainsKey(e.ParameterName))
			{
				_candidateParametersToForceNonUnicode[e.ParameterName] = true;
			}
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("@" + e.ParameterName);
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbProjectExpression e)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbProjectExpression>(e, "e");
		Symbol fromSymbol;
		SqlSelectStatement sqlSelectStatement = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		bool flag = false;
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		}
		else if (SqlVersion == SqlVersion.Sql8 && !sqlSelectStatement.OrderBy.IsEmpty)
		{
			flag = true;
		}
		selectStatementStack.Push(sqlSelectStatement);
		symbolTable.EnterScope();
		AddFromSymbol(sqlSelectStatement, e.Input.VariableName, fromSymbol);
		DbExpression projection = e.Projection;
		DbNewInstanceExpression val = (DbNewInstanceExpression)(object)((projection is DbNewInstanceExpression) ? projection : null);
		if (val != null)
		{
			sqlSelectStatement.Select.Append(VisitNewInstanceExpression(val, flag, out var newColumns));
			if (flag)
			{
				sqlSelectStatement.OutputColumnsRenamed = true;
			}
			sqlSelectStatement.OutputColumns = newColumns;
		}
		else
		{
			sqlSelectStatement.Select.Append(e.Projection.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		}
		symbolTable.ExitScope();
		selectStatementStack.Pop();
		return sqlSelectStatement;
	}

	public override ISqlFragment Visit(DbPropertyExpression e)
	{
		Check.NotNull<DbPropertyExpression>(e, "e");
		ISqlFragment sqlFragment = e.Instance.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
		if (e.Instance is DbVariableReferenceExpression)
		{
			isVarRefSingle = false;
		}
		if (sqlFragment is JoinSymbol joinSymbol)
		{
			if (joinSymbol.IsNestedJoin)
			{
				return new SymbolPair(joinSymbol, joinSymbol.NameToExtent[e.Property.Name]);
			}
			return joinSymbol.NameToExtent[e.Property.Name];
		}
		SqlBuilder sqlBuilder;
		if (sqlFragment is SymbolPair symbolPair)
		{
			if (symbolPair.Column is JoinSymbol joinSymbol2)
			{
				symbolPair.Column = joinSymbol2.NameToExtent[e.Property.Name];
				return symbolPair;
			}
			if (symbolPair.Column.Columns.ContainsKey(e.Property.Name))
			{
				sqlBuilder = new SqlBuilder();
				sqlBuilder.Append(symbolPair.Source);
				sqlBuilder.Append(".");
				Symbol symbol = symbolPair.Column.Columns[e.Property.Name];
				optionalColumnUsageManager.MarkAsUsed(symbol);
				sqlBuilder.Append(symbol);
				return sqlBuilder;
			}
		}
		sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(sqlFragment);
		sqlBuilder.Append(".");
		if (sqlFragment is Symbol symbol2 && symbol2.OutputColumns.TryGetValue(e.Property.Name, out var value))
		{
			optionalColumnUsageManager.MarkAsUsed(value);
			if (symbol2.OutputColumnsRenamed)
			{
				sqlBuilder.Append(value);
			}
			else
			{
				sqlBuilder.Append(QuoteIdentifier(e.Property.Name));
			}
		}
		else
		{
			sqlBuilder.Append(QuoteIdentifier(e.Property.Name));
		}
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbQuantifierExpression e)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		Check.NotNull<DbQuantifierExpression>(e, "e");
		SqlBuilder sqlBuilder = new SqlBuilder();
		bool negatePredicate = (int)((DbExpression)e).ExpressionKind == 0;
		if ((int)((DbExpression)e).ExpressionKind == 2)
		{
			sqlBuilder.Append("EXISTS (");
		}
		else
		{
			sqlBuilder.Append("NOT EXISTS (");
		}
		SqlSelectStatement sqlSelectStatement = VisitFilterExpression(e.Input, e.Predicate, negatePredicate);
		if (sqlSelectStatement.Select.IsEmpty)
		{
			AddDefaultColumns(sqlSelectStatement);
		}
		sqlBuilder.Append(sqlSelectStatement);
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbRefExpression e)
	{
		Check.NotNull<DbRefExpression>(e, "e");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbRelationshipNavigationExpression e)
	{
		Check.NotNull<DbRelationshipNavigationExpression>(e, "e");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbSkipExpression e)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbSkipExpression>(e, "e");
		Symbol fromSymbol;
		SqlSelectStatement sqlSelectStatement = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		}
		selectStatementStack.Push(sqlSelectStatement);
		symbolTable.EnterScope();
		AddFromSymbol(sqlSelectStatement, e.Input.VariableName, fromSymbol);
		if (SqlVersion >= SqlVersion.Sql11)
		{
			sqlSelectStatement.Select.Skip = new SkipClause(HandleCountExpression(e.Count));
			if (SqlProviderServices.UseRowNumberOrderingInOffsetQueries)
			{
				sqlSelectStatement.OrderBy.Append("row_number() OVER (ORDER BY ");
				AddSortKeys(sqlSelectStatement.OrderBy, e.SortOrder);
				sqlSelectStatement.OrderBy.Append(")");
			}
			else
			{
				AddSortKeys(sqlSelectStatement.OrderBy, e.SortOrder);
			}
			symbolTable.ExitScope();
			selectStatementStack.Pop();
			return sqlSelectStatement;
		}
		List<Symbol> list = AddDefaultColumns(sqlSelectStatement);
		sqlSelectStatement.Select.Append("row_number() OVER (ORDER BY ");
		AddSortKeys(sqlSelectStatement.Select, e.SortOrder);
		sqlSelectStatement.Select.Append(") AS ");
		string row_numberName = "row_number";
		Symbol symbol = new Symbol(row_numberName, IntegerType);
		if (list.Any((Symbol c) => string.Equals(c.Name, row_numberName, StringComparison.OrdinalIgnoreCase)))
		{
			symbol.NeedsRenaming = true;
		}
		sqlSelectStatement.Select.Append(symbol);
		symbolTable.ExitScope();
		selectStatementStack.Pop();
		SqlSelectStatement sqlSelectStatement2 = new SqlSelectStatement();
		sqlSelectStatement2.From.Append("( ");
		sqlSelectStatement2.From.Append(sqlSelectStatement);
		sqlSelectStatement2.From.AppendLine();
		sqlSelectStatement2.From.Append(") ");
		Symbol symbol2 = null;
		if (sqlSelectStatement.FromExtents.Count == 1 && sqlSelectStatement.FromExtents[0] is JoinSymbol joinSymbol)
		{
			symbol2 = new JoinSymbol(e.Input.VariableName, e.Input.VariableType, joinSymbol.ExtentList)
			{
				IsNestedJoin = true,
				ColumnList = list,
				FlattenedExtentList = joinSymbol.FlattenedExtentList
			};
		}
		if (symbol2 == null)
		{
			symbol2 = new Symbol(e.Input.VariableName, e.Input.VariableType, sqlSelectStatement.OutputColumns, outputColumnsRenamed: false);
		}
		selectStatementStack.Push(sqlSelectStatement2);
		symbolTable.EnterScope();
		AddFromSymbol(sqlSelectStatement2, e.Input.VariableName, symbol2);
		sqlSelectStatement2.Where.Append(symbol2);
		sqlSelectStatement2.Where.Append(".");
		sqlSelectStatement2.Where.Append(symbol);
		sqlSelectStatement2.Where.Append(" > ");
		sqlSelectStatement2.Where.Append(HandleCountExpression(e.Count));
		AddSortKeys(sqlSelectStatement2.OrderBy, e.SortOrder);
		symbolTable.ExitScope();
		selectStatementStack.Pop();
		return sqlSelectStatement2;
	}

	public override ISqlFragment Visit(DbSortExpression e)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Check.NotNull<DbSortExpression>(e, "e");
		Symbol fromSymbol;
		SqlSelectStatement sqlSelectStatement = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		}
		selectStatementStack.Push(sqlSelectStatement);
		symbolTable.EnterScope();
		AddFromSymbol(sqlSelectStatement, e.Input.VariableName, fromSymbol);
		AddSortKeys(sqlSelectStatement.OrderBy, e.SortOrder);
		symbolTable.ExitScope();
		selectStatementStack.Pop();
		return sqlSelectStatement;
	}

	public override ISqlFragment Visit(DbTreatExpression e)
	{
		Check.NotNull<DbTreatExpression>(e, "e");
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbUnionAllExpression e)
	{
		Check.NotNull<DbUnionAllExpression>(e, "e");
		return VisitSetOpExpression((DbBinaryExpression)(object)e, "UNION ALL");
	}

	public override ISqlFragment Visit(DbVariableReferenceExpression e)
	{
		Check.NotNull<DbVariableReferenceExpression>(e, "e");
		if (isVarRefSingle)
		{
			throw new NotSupportedException();
		}
		isVarRefSingle = true;
		Symbol symbol = symbolTable.Lookup(e.VariableName);
		optionalColumnUsageManager.MarkAsUsed(symbol);
		if (!CurrentSelectStatement.FromExtents.Contains(symbol))
		{
			CurrentSelectStatement.OuterExtents[symbol] = true;
		}
		return symbol;
	}

	private static SqlBuilder VisitAggregate(DbAggregate aggregate, IList<object> aggregateArguments)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		DbFunctionAggregate val = (DbFunctionAggregate)(object)((aggregate is DbFunctionAggregate) ? aggregate : null);
		if (val == null)
		{
			throw new NotSupportedException();
		}
		if (val.Function.IsCanonicalFunction() && string.Equals(((EdmType)val.Function).Name, "BigCount", StringComparison.Ordinal))
		{
			sqlBuilder.Append("COUNT_BIG");
		}
		else
		{
			SqlFunctionCallHandler.WriteFunctionName(sqlBuilder, val.Function);
		}
		sqlBuilder.Append("(");
		DbFunctionAggregate val2 = val;
		if (val2 != null && val2.Distinct)
		{
			sqlBuilder.Append("DISTINCT ");
		}
		string s = string.Empty;
		foreach (object aggregateArgument in aggregateArguments)
		{
			sqlBuilder.Append(s);
			sqlBuilder.Append(aggregateArgument);
			s = ", ";
		}
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	internal void ParenthesizeExpressionIfNeeded(DbExpression e, SqlBuilder result)
	{
		if (IsComplexExpression(e))
		{
			result.Append("(");
			result.Append(e.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			result.Append(")");
		}
		else
		{
			result.Append(e.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		}
	}

	private SqlBuilder VisitBinaryExpression(string op, DbExpressionKind expressionKind, DbExpression left, DbExpression right)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		RemoveUnnecessaryCasts(ref left, ref right);
		SqlBuilder sqlBuilder = new SqlBuilder();
		bool flag = true;
		foreach (DbExpression item in FlattenAssociativeExpression(expressionKind, left, right))
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				sqlBuilder.Append(op);
			}
			ParenthesizeExpressionIfNeeded(item, sqlBuilder);
		}
		return sqlBuilder;
	}

	private static IEnumerable<DbExpression> FlattenAssociativeExpression(DbExpressionKind kind, DbExpression left, DbExpression right)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		if ((int)kind != 41 && (int)kind != 1 && (int)kind != 44 && (int)kind != 34)
		{
			return (IEnumerable<DbExpression>)(object)new DbExpression[2] { left, right };
		}
		List<DbExpression> list = new List<DbExpression>();
		ExtractAssociativeArguments(kind, list, left);
		ExtractAssociativeArguments(kind, list, right);
		return list;
	}

	private static void ExtractAssociativeArguments(DbExpressionKind expressionKind, List<DbExpression> argumentList, DbExpression expression)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<DbExpression> leafNodes = expression.GetLeafNodes(expressionKind, delegate(DbExpression exp)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			DbBinaryExpression val = (DbBinaryExpression)(object)((exp is DbBinaryExpression) ? exp : null);
			return (IEnumerable<DbExpression>)((val != null) ? ((IEnumerable)new DbExpression[2] { val.Left, val.Right }) : ((IEnumerable)((DbArithmeticExpression)exp).Arguments));
		});
		argumentList.AddRange(leafNodes);
	}

	private SqlBuilder VisitComparisonExpression(string op, DbExpression left, DbExpression right)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		RemoveUnnecessaryCasts(ref left, ref right);
		SqlBuilder sqlBuilder = new SqlBuilder();
		bool isCastOptional = left.ResultType.EdmType == right.ResultType.EdmType;
		if ((int)left.ExpressionKind == 5)
		{
			sqlBuilder.Append(VisitConstant((DbConstantExpression)left, isCastOptional));
		}
		else
		{
			ParenthesizeExpressionIfNeeded(left, sqlBuilder);
		}
		sqlBuilder.Append(op);
		if ((int)right.ExpressionKind == 5)
		{
			sqlBuilder.Append(VisitConstant((DbConstantExpression)right, isCastOptional));
		}
		else
		{
			ParenthesizeExpressionIfNeeded(right, sqlBuilder);
		}
		return sqlBuilder;
	}

	private static void RemoveUnnecessaryCasts(ref DbExpression left, ref DbExpression right)
	{
		if (left.ResultType.EdmType == right.ResultType.EdmType)
		{
			DbExpression obj = left;
			DbCastExpression val = (DbCastExpression)(object)((obj is DbCastExpression) ? obj : null);
			if (val != null && ((DbUnaryExpression)val).Argument.ResultType.EdmType == left.ResultType.EdmType)
			{
				left = ((DbUnaryExpression)val).Argument;
			}
			DbExpression obj2 = right;
			DbCastExpression val2 = (DbCastExpression)(object)((obj2 is DbCastExpression) ? obj2 : null);
			if (val2 != null && ((DbUnaryExpression)val2).Argument.ResultType.EdmType == left.ResultType.EdmType)
			{
				right = ((DbUnaryExpression)val2).Argument;
			}
		}
	}

	private SqlSelectStatement VisitInputExpression(DbExpression inputExpression, string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		ISqlFragment sqlFragment = inputExpression.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
		SqlSelectStatement sqlSelectStatement = sqlFragment as SqlSelectStatement;
		if (sqlSelectStatement == null)
		{
			sqlSelectStatement = new SqlSelectStatement();
			WrapNonQueryExtent(sqlSelectStatement, sqlFragment, inputExpression.ExpressionKind);
		}
		if (sqlSelectStatement.FromExtents.Count == 0)
		{
			fromSymbol = new Symbol(inputVarName, inputVarType);
		}
		else if (sqlSelectStatement.FromExtents.Count == 1)
		{
			fromSymbol = sqlSelectStatement.FromExtents[0];
		}
		else
		{
			JoinSymbol joinSymbol = new JoinSymbol(inputVarName, inputVarType, sqlSelectStatement.FromExtents);
			joinSymbol.FlattenedExtentList = sqlSelectStatement.AllJoinExtents;
			fromSymbol = joinSymbol;
			sqlSelectStatement.FromExtents.Clear();
			sqlSelectStatement.FromExtents.Add(fromSymbol);
		}
		return sqlSelectStatement;
	}

	private SqlBuilder VisitIsEmptyExpression(DbIsEmptyExpression e, bool negate)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (!negate)
		{
			sqlBuilder.Append(" NOT");
		}
		sqlBuilder.Append(" EXISTS (");
		sqlBuilder.Append(VisitExpressionEnsureSqlStatement(((DbUnaryExpression)e).Argument));
		sqlBuilder.AppendLine();
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private ISqlFragment VisitCollectionConstructor(DbNewInstanceExpression e)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Invalid comparison between I4 and Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		if (e.Arguments.Count == 1 && (int)e.Arguments[0].ExpressionKind == 11)
		{
			DbExpression obj = e.Arguments[0];
			DbElementExpression val = (DbElementExpression)(object)((obj is DbElementExpression) ? obj : null);
			SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(((DbUnaryExpression)val).Argument);
			if (!IsCompatible(sqlSelectStatement, (DbExpressionKind)11))
			{
				TypeUsage elementTypeUsage = ((DbUnaryExpression)val).Argument.ResultType.GetElementTypeUsage();
				sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, "element", elementTypeUsage, out var fromSymbol);
				AddFromSymbol(sqlSelectStatement, "element", fromSymbol, addToSymbolTable: false);
			}
			sqlSelectStatement.Select.Top = new TopClause(1, withTies: false);
			return sqlSelectStatement;
		}
		CollectionType val2 = (CollectionType)((DbExpression)e).ResultType.EdmType;
		bool flag = 26 == (int)((MetadataItem)val2.TypeUsage.EdmType).BuiltInTypeKind;
		SqlBuilder sqlBuilder = new SqlBuilder();
		string s = "";
		if (e.Arguments.Count == 0)
		{
			sqlBuilder.Append(" SELECT CAST(null as ");
			sqlBuilder.Append(GetSqlPrimitiveType(val2.TypeUsage));
			sqlBuilder.Append(") AS X FROM (SELECT 1) AS Y WHERE 1=0");
		}
		foreach (DbExpression argument in e.Arguments)
		{
			sqlBuilder.Append(s);
			sqlBuilder.Append(" SELECT ");
			sqlBuilder.Append(argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			if (flag)
			{
				sqlBuilder.Append(" AS X ");
			}
			s = " UNION ALL ";
		}
		return sqlBuilder;
	}

	private SqlBuilder VisitIsNullExpression(DbIsNullExpression e, bool negate)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		SqlBuilder sqlBuilder = new SqlBuilder();
		if ((int)((DbUnaryExpression)e).Argument.ExpressionKind == 43)
		{
			_ignoreForceNonUnicodeFlag = true;
		}
		sqlBuilder.Append(((DbUnaryExpression)e).Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		_ignoreForceNonUnicodeFlag = false;
		if (!negate)
		{
			sqlBuilder.Append(" IS NULL");
		}
		else
		{
			sqlBuilder.Append(" IS NOT NULL");
		}
		return sqlBuilder;
	}

	private ISqlFragment VisitJoinExpression(IList<DbExpressionBinding> inputs, DbExpressionKind joinKind, string joinString, DbExpression joinCondition)
	{
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Invalid comparison between Unknown and I4
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Invalid comparison between Unknown and I4
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Invalid comparison between Unknown and I4
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Invalid comparison between Unknown and I4
		SqlSelectStatement sqlSelectStatement;
		if (!IsParentAJoin)
		{
			sqlSelectStatement = new SqlSelectStatement();
			sqlSelectStatement.AllJoinExtents = new List<Symbol>();
			selectStatementStack.Push(sqlSelectStatement);
		}
		else
		{
			sqlSelectStatement = CurrentSelectStatement;
		}
		symbolTable.EnterScope();
		string text = "";
		bool flag = true;
		int count = inputs.Count;
		for (int i = 0; i < count; i++)
		{
			DbExpressionBinding val = inputs[i];
			if (text.Length != 0)
			{
				sqlSelectStatement.From.AppendLine();
			}
			sqlSelectStatement.From.Append(text + " ");
			bool flag2 = (int)val.Expression.ExpressionKind == 50 || (flag && (IsJoinExpression(val.Expression) || IsApplyExpression(val.Expression)));
			isParentAJoinStack.Push(flag2 ? true : false);
			int count2 = sqlSelectStatement.FromExtents.Count;
			ISqlFragment fromExtentFragment = val.Expression.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
			isParentAJoinStack.Pop();
			ProcessJoinInputResult(fromExtentFragment, sqlSelectStatement, val, count2);
			text = joinString;
			flag = false;
		}
		if ((int)joinKind == 16 || (int)joinKind == 21 || (int)joinKind == 27)
		{
			sqlSelectStatement.From.Append(" ON ");
			isParentAJoinStack.Push(item: false);
			sqlSelectStatement.From.Append(joinCondition.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			isParentAJoinStack.Pop();
		}
		symbolTable.ExitScope();
		if (!IsParentAJoin)
		{
			selectStatementStack.Pop();
		}
		return sqlSelectStatement;
	}

	private void ProcessJoinInputResult(ISqlFragment fromExtentFragment, SqlSelectStatement result, DbExpressionBinding input, int fromSymbolStart)
	{
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		Symbol symbol = null;
		if (result != fromExtentFragment)
		{
			if (fromExtentFragment is SqlSelectStatement sqlSelectStatement)
			{
				if (sqlSelectStatement.Select.IsEmpty)
				{
					List<Symbol> columnList = AddDefaultColumns(sqlSelectStatement);
					if (IsJoinExpression(input.Expression) || IsApplyExpression(input.Expression))
					{
						List<Symbol> fromExtents = sqlSelectStatement.FromExtents;
						symbol = new JoinSymbol(input.VariableName, input.VariableType, fromExtents)
						{
							IsNestedJoin = true,
							ColumnList = columnList
						};
					}
					else
					{
						symbol = ((!(sqlSelectStatement.FromExtents[0] is JoinSymbol joinSymbol)) ? new Symbol(input.VariableName, input.VariableType, sqlSelectStatement.OutputColumns, sqlSelectStatement.OutputColumnsRenamed) : new JoinSymbol(input.VariableName, input.VariableType, joinSymbol.ExtentList)
						{
							IsNestedJoin = true,
							ColumnList = columnList,
							FlattenedExtentList = joinSymbol.FlattenedExtentList
						});
					}
				}
				else
				{
					symbol = new Symbol(input.VariableName, input.VariableType, sqlSelectStatement.OutputColumns, sqlSelectStatement.OutputColumnsRenamed);
				}
				result.From.Append(" (");
				result.From.Append(sqlSelectStatement);
				result.From.Append(" )");
			}
			else if (input.Expression is DbScanExpression)
			{
				result.From.Append(fromExtentFragment);
			}
			else
			{
				WrapNonQueryExtent(result, fromExtentFragment, input.Expression.ExpressionKind);
			}
			if (symbol == null)
			{
				symbol = new Symbol(input.VariableName, input.VariableType);
			}
			AddFromSymbol(result, input.VariableName, symbol);
			result.AllJoinExtents.Add(symbol);
		}
		else
		{
			List<Symbol> list = new List<Symbol>();
			for (int i = fromSymbolStart; i < result.FromExtents.Count; i++)
			{
				list.Add(result.FromExtents[i]);
			}
			result.FromExtents.RemoveRange(fromSymbolStart, result.FromExtents.Count - fromSymbolStart);
			symbol = new JoinSymbol(input.VariableName, input.VariableType, list);
			result.FromExtents.Add(symbol);
			symbolTable.Add(input.VariableName, symbol);
		}
	}

	private ISqlFragment VisitNewInstanceExpression(DbNewInstanceExpression e, bool aliasesNeedRenaming, out Dictionary<string, Symbol> newColumns)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between I4 and Unknown
		SqlBuilder sqlBuilder = new SqlBuilder();
		EdmType edmType = ((DbExpression)e).ResultType.EdmType;
		RowType val = (RowType)(object)((edmType is RowType) ? edmType : null);
		if (val != null)
		{
			newColumns = new Dictionary<string, Symbol>(e.Arguments.Count);
			ReadOnlyMetadataCollection<EdmProperty> properties = val.Properties;
			string s = "";
			for (int i = 0; i < e.Arguments.Count; i++)
			{
				DbExpression val2 = e.Arguments[i];
				if (36 == (int)((MetadataItem)val2.ResultType.EdmType).BuiltInTypeKind)
				{
					throw new NotSupportedException();
				}
				EdmProperty val3 = ((ReadOnlyCollection<EdmProperty>)(object)properties)[i];
				sqlBuilder.Append(s);
				sqlBuilder.AppendLine();
				sqlBuilder.Append(val2.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
				sqlBuilder.Append(" AS ");
				if (aliasesNeedRenaming)
				{
					Symbol symbol = new Symbol(((EdmMember)val3).Name, ((EdmMember)val3).TypeUsage);
					symbol.NeedsRenaming = true;
					symbol.NewName = "Internal_" + ((EdmMember)val3).Name;
					sqlBuilder.Append(symbol);
					newColumns.Add(((EdmMember)val3).Name, symbol);
				}
				else
				{
					sqlBuilder.Append(QuoteIdentifier(((EdmMember)val3).Name));
				}
				s = ", ";
			}
			return sqlBuilder;
		}
		throw new NotSupportedException();
	}

	private ISqlFragment VisitSetOpExpression(DbBinaryExpression setOpExpression, string separator)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		List<SqlSelectStatement> list = new List<SqlSelectStatement>();
		VisitAndGatherSetOpLeafExpressions(((DbExpression)setOpExpression).ExpressionKind, setOpExpression.Left, list);
		VisitAndGatherSetOpLeafExpressions(((DbExpression)setOpExpression).ExpressionKind, setOpExpression.Right, list);
		SqlBuilder sqlBuilder = new SqlBuilder();
		for (int i = 0; i < list.Count; i++)
		{
			if (i > 0)
			{
				sqlBuilder.AppendLine();
				sqlBuilder.Append(separator);
				sqlBuilder.AppendLine();
			}
			sqlBuilder.Append(list[i]);
		}
		if (!list[0].OutputColumnsRenamed)
		{
			return sqlBuilder;
		}
		SqlSelectStatement sqlSelectStatement = new SqlSelectStatement();
		sqlSelectStatement.From.Append("( ");
		sqlSelectStatement.From.Append(sqlBuilder);
		sqlSelectStatement.From.AppendLine();
		sqlSelectStatement.From.Append(") ");
		Symbol fromSymbol = new Symbol("X", setOpExpression.Left.ResultType.GetElementTypeUsage(), list[0].OutputColumns, outputColumnsRenamed: true);
		AddFromSymbol(sqlSelectStatement, null, fromSymbol, addToSymbolTable: false);
		return sqlSelectStatement;
	}

	private void VisitAndGatherSetOpLeafExpressions(DbExpressionKind kind, DbExpression expression, List<SqlSelectStatement> leafSelectStatements)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (SqlVersion > SqlVersion.Sql8 && ((int)kind == 55 || (int)kind == 22) && expression.ExpressionKind == kind)
		{
			DbBinaryExpression val = (DbBinaryExpression)expression;
			VisitAndGatherSetOpLeafExpressions(kind, val.Left, leafSelectStatements);
			VisitAndGatherSetOpLeafExpressions(kind, val.Right, leafSelectStatements);
		}
		else
		{
			leafSelectStatements.Add(VisitExpressionEnsureSqlStatement(expression, addDefaultColumns: true, markAllDefaultColumnsAsUsed: true));
		}
	}

	private void AddColumns(SqlSelectStatement selectStatement, Symbol symbol, List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary)
	{
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Invalid comparison between I4 and Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between I4 and Unknown
		if (symbol is JoinSymbol joinSymbol)
		{
			if (!joinSymbol.IsNestedJoin)
			{
				foreach (Symbol extent in joinSymbol.ExtentList)
				{
					if (extent.Type != null && 26 != (int)((MetadataItem)extent.Type.EdmType).BuiltInTypeKind)
					{
						AddColumns(selectStatement, extent, columnList, columnDictionary);
					}
				}
				return;
			}
			{
				foreach (Symbol column in joinSymbol.ColumnList)
				{
					OptionalColumn optionalColumn = CreateOptionalColumn(null, column);
					optionalColumn.Append(symbol);
					optionalColumn.Append(".");
					optionalColumn.Append(column);
					selectStatement.Select.AddOptionalColumn(optionalColumn);
					if (columnDictionary.ContainsKey(column.Name))
					{
						columnDictionary[column.Name].NeedsRenaming = true;
						column.NeedsRenaming = true;
					}
					else
					{
						columnDictionary[column.Name] = column;
					}
					columnList.Add(column);
				}
				return;
			}
		}
		if (symbol.OutputColumnsRenamed)
		{
			selectStatement.OutputColumnsRenamed = true;
		}
		if (selectStatement.OutputColumns == null)
		{
			selectStatement.OutputColumns = new Dictionary<string, Symbol>();
		}
		if (symbol.Type == null || 26 == (int)((MetadataItem)symbol.Type.EdmType).BuiltInTypeKind)
		{
			AddColumn(selectStatement, symbol, columnList, columnDictionary, "X");
			return;
		}
		foreach (EdmProperty property in symbol.Type.GetProperties())
		{
			AddColumn(selectStatement, symbol, columnList, columnDictionary, ((EdmMember)property).Name);
		}
	}

	private OptionalColumn CreateOptionalColumn(Symbol inputColumnSymbol, Symbol column)
	{
		if (!optionalColumnUsageManager.ContainsKey(column))
		{
			optionalColumnUsageManager.Add(inputColumnSymbol, column);
		}
		return new OptionalColumn(optionalColumnUsageManager, column);
	}

	private void AddColumn(SqlSelectStatement selectStatement, Symbol symbol, List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary, string columnName)
	{
		allColumnNames[columnName] = 0;
		Symbol value = null;
		symbol.OutputColumns.TryGetValue(columnName, out value);
		if (!symbol.Columns.TryGetValue(columnName, out var value2))
		{
			value2 = ((value != null && symbol.OutputColumnsRenamed) ? value : new Symbol(columnName, null));
			symbol.Columns.Add(columnName, value2);
		}
		OptionalColumn optionalColumn = CreateOptionalColumn(value, value2);
		optionalColumn.Append(symbol);
		optionalColumn.Append(".");
		if (symbol.OutputColumnsRenamed)
		{
			optionalColumn.Append(value);
		}
		else
		{
			optionalColumn.Append(QuoteIdentifier(columnName));
		}
		optionalColumn.Append(" AS ");
		optionalColumn.Append(value2);
		selectStatement.Select.AddOptionalColumn(optionalColumn);
		if (!selectStatement.OutputColumns.ContainsKey(columnName))
		{
			selectStatement.OutputColumns.Add(columnName, value2);
		}
		if (columnDictionary.ContainsKey(columnName))
		{
			columnDictionary[columnName].NeedsRenaming = true;
			value2.NeedsRenaming = true;
		}
		else
		{
			columnDictionary[columnName] = symbol.Columns[columnName];
		}
		columnList.Add(value2);
	}

	private List<Symbol> AddDefaultColumns(SqlSelectStatement selectStatement)
	{
		List<Symbol> list = new List<Symbol>();
		Dictionary<string, Symbol> columnDictionary = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
		foreach (Symbol fromExtent in selectStatement.FromExtents)
		{
			AddColumns(selectStatement, fromExtent, list, columnDictionary);
		}
		return list;
	}

	private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol)
	{
		AddFromSymbol(selectStatement, inputVarName, fromSymbol, addToSymbolTable: true);
	}

	private void AddFromSymbol(SqlSelectStatement selectStatement, string inputVarName, Symbol fromSymbol, bool addToSymbolTable)
	{
		if (selectStatement.FromExtents.Count == 0 || fromSymbol != selectStatement.FromExtents[0])
		{
			selectStatement.FromExtents.Add(fromSymbol);
			selectStatement.From.Append(" AS ");
			selectStatement.From.Append(fromSymbol);
			allExtentNames[fromSymbol.Name] = 0;
		}
		if (addToSymbolTable)
		{
			symbolTable.Add(inputVarName, fromSymbol);
		}
	}

	private void AddSortKeys(SqlBuilder orderByClause, IList<DbSortClause> sortKeys)
	{
		string s = "";
		foreach (DbSortClause sortKey in sortKeys)
		{
			orderByClause.Append(s);
			orderByClause.Append(sortKey.Expression.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			if (!string.IsNullOrEmpty(sortKey.Collation))
			{
				orderByClause.Append(" COLLATE ");
				orderByClause.Append(sortKey.Collation);
			}
			orderByClause.Append(sortKey.Ascending ? " ASC" : " DESC");
			s = ", ";
		}
	}

	private SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement, string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
	{
		return CreateNewSelectStatement(oldStatement, inputVarName, inputVarType, finalizeOldStatement: true, out fromSymbol);
	}

	private SqlSelectStatement CreateNewSelectStatement(SqlSelectStatement oldStatement, string inputVarName, TypeUsage inputVarType, bool finalizeOldStatement, out Symbol fromSymbol)
	{
		fromSymbol = null;
		if (finalizeOldStatement && oldStatement.Select.IsEmpty)
		{
			List<Symbol> columnList = AddDefaultColumns(oldStatement);
			if (oldStatement.FromExtents[0] is JoinSymbol joinSymbol)
			{
				JoinSymbol joinSymbol2 = new JoinSymbol(inputVarName, inputVarType, joinSymbol.ExtentList);
				joinSymbol2.IsNestedJoin = true;
				joinSymbol2.ColumnList = columnList;
				joinSymbol2.FlattenedExtentList = joinSymbol.FlattenedExtentList;
				fromSymbol = joinSymbol2;
			}
		}
		if (fromSymbol == null)
		{
			fromSymbol = new Symbol(inputVarName, inputVarType, oldStatement.OutputColumns, oldStatement.OutputColumnsRenamed);
		}
		SqlSelectStatement sqlSelectStatement = new SqlSelectStatement();
		sqlSelectStatement.From.Append("( ");
		sqlSelectStatement.From.Append(oldStatement);
		sqlSelectStatement.From.AppendLine();
		sqlSelectStatement.From.Append(") ");
		return sqlSelectStatement;
	}

	private static string EscapeSingleQuote(string s, bool isUnicode)
	{
		return (isUnicode ? "N'" : "'") + s.Replace("'", "''") + "'";
	}

	private string GetSqlPrimitiveType(TypeUsage type)
	{
		TypeUsage storeType = _storeItemCollection.ProviderManifest.GetStoreType(type);
		return GenerateSqlForStoreType(_sqlVersion, storeType);
	}

	internal static string GenerateSqlForStoreType(SqlVersion sqlVersion, TypeUsage storeTypeUsage)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected I4, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected I4, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		string text = storeTypeUsage.EdmType.Name;
		int maxLength = 0;
		byte precision = 0;
		byte scale = 0;
		PrimitiveTypeKind primitiveTypeKind = ((PrimitiveType)storeTypeUsage.EdmType).PrimitiveTypeKind;
		switch ((int)primitiveTypeKind)
		{
		default:
			switch (primitiveTypeKind - 12)
			{
			case 0:
				if (!storeTypeUsage.MustFacetBeConstant("MaxLength"))
				{
					storeTypeUsage.TryGetMaxLength(out maxLength);
					text = text + "(" + maxLength.ToString(CultureInfo.InvariantCulture) + ")";
				}
				break;
			case 1:
				AssertKatmaiOrNewer(sqlVersion, primitiveTypeKind);
				text = "time";
				break;
			case 2:
				AssertKatmaiOrNewer(sqlVersion, primitiveTypeKind);
				text = "datetimeoffset";
				break;
			}
			break;
		case 0:
			if (!storeTypeUsage.MustFacetBeConstant("MaxLength"))
			{
				storeTypeUsage.TryGetMaxLength(out maxLength);
				text = text + "(" + maxLength.ToString(CultureInfo.InvariantCulture) + ")";
			}
			break;
		case 3:
			text = (SqlVersionUtils.IsPreKatmai(sqlVersion) ? "datetime" : "datetime2");
			break;
		case 4:
			if (!storeTypeUsage.MustFacetBeConstant("Precision"))
			{
				storeTypeUsage.TryGetPrecision(out precision);
				storeTypeUsage.TryGetScale(out scale);
				text = text + "(" + precision + "," + scale + ")";
			}
			break;
		case 1:
		case 2:
			break;
		}
		return text;
	}

	private ISqlFragment HandleCountExpression(DbExpression e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if ((int)e.ExpressionKind == 5)
		{
			SqlBuilder sqlBuilder = new SqlBuilder();
			sqlBuilder.Append(((DbConstantExpression)e).Value.ToString());
			return sqlBuilder;
		}
		return e.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
	}

	private static bool IsApplyExpression(DbExpression e)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between I4 and Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between I4 and Unknown
		if (6 != (int)e.ExpressionKind)
		{
			return 42 == (int)e.ExpressionKind;
		}
		return true;
	}

	private static bool IsJoinExpression(DbExpression e)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between I4 and Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between I4 and Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between I4 and Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between I4 and Unknown
		if (7 != (int)e.ExpressionKind && 16 != (int)e.ExpressionKind && 21 != (int)e.ExpressionKind)
		{
			return 27 == (int)e.ExpressionKind;
		}
		return true;
	}

	private static bool IsComplexExpression(DbExpression e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		DbExpressionKind expressionKind = e.ExpressionKind;
		if (expressionKind - 4 <= 1 || (int)expressionKind == 43 || (int)expressionKind == 46)
		{
			return false;
		}
		return true;
	}

	private static bool IsCompatible(SqlSelectStatement result, DbExpressionKind expressionKind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Invalid comparison between Unknown and I4
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Invalid comparison between Unknown and I4
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if ((int)expressionKind <= 20)
		{
			if ((int)expressionKind <= 11)
			{
				if ((int)expressionKind == 9)
				{
					if (result.Select.Top == null && result.Select.Skip == null)
					{
						return result.OrderBy.IsEmpty;
					}
					return false;
				}
				if ((int)expressionKind == 11)
				{
					goto IL_011c;
				}
			}
			else
			{
				if ((int)expressionKind == 15)
				{
					if (result.Select.IsEmpty && result.Where.IsEmpty && result.GroupBy.IsEmpty && result.Select.Top == null)
					{
						return result.Select.Skip == null;
					}
					return false;
				}
				if ((int)expressionKind == 20)
				{
					if (result.Select.IsEmpty && result.GroupBy.IsEmpty && result.OrderBy.IsEmpty && result.Select.Top == null && result.Select.Skip == null)
					{
						return !result.Select.IsDistinct;
					}
					return false;
				}
			}
		}
		else if ((int)expressionKind <= 45)
		{
			if ((int)expressionKind == 31)
			{
				goto IL_011c;
			}
			if ((int)expressionKind == 45)
			{
				if (result.Select.IsEmpty && result.GroupBy.IsEmpty)
				{
					return !result.Select.IsDistinct;
				}
				return false;
			}
		}
		else
		{
			if ((int)expressionKind == 51)
			{
				if (result.Select.IsEmpty && result.Select.Skip == null && result.GroupBy.IsEmpty && result.OrderBy.IsEmpty)
				{
					return !result.Select.IsDistinct;
				}
				return false;
			}
			if ((int)expressionKind == 52)
			{
				if (result.Select.IsEmpty && result.GroupBy.IsEmpty && result.OrderBy.IsEmpty)
				{
					return !result.Select.IsDistinct;
				}
				return false;
			}
		}
		throw new InvalidOperationException(string.Empty);
		IL_011c:
		return result.Select.Top == null;
	}

	internal static string QuoteIdentifier(string name)
	{
		return "[" + name.Replace("]", "]]") + "]";
	}

	private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e)
	{
		return VisitExpressionEnsureSqlStatement(e, addDefaultColumns: true, markAllDefaultColumnsAsUsed: false);
	}

	private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e, bool addDefaultColumns, bool markAllDefaultColumnsAsUsed)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Invalid comparison between Unknown and I4
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		DbExpressionKind expressionKind = e.ExpressionKind;
		if ((int)expressionKind <= 20)
		{
			if ((int)expressionKind == 15 || (int)expressionKind == 20)
			{
				goto IL_0028;
			}
		}
		else if ((int)expressionKind == 45 || (int)expressionKind == 52)
		{
			goto IL_0028;
		}
		string inputVarName = "c";
		symbolTable.EnterScope();
		TypeUsage val = null;
		DbExpressionKind expressionKind2 = e.ExpressionKind;
		if ((int)expressionKind2 <= 21)
		{
			if (expressionKind2 - 6 <= 1 || (int)expressionKind2 == 16 || (int)expressionKind2 == 21)
			{
				goto IL_0082;
			}
		}
		else if ((int)expressionKind2 == 27 || (int)expressionKind2 == 42 || (int)expressionKind2 == 50)
		{
			goto IL_0082;
		}
		val = ((CollectionType)e.ResultType.EdmType).TypeUsage;
		goto IL_00a6;
		IL_0082:
		val = e.ResultType.GetElementTypeUsage();
		goto IL_00a6;
		IL_00c6:
		SqlSelectStatement sqlSelectStatement;
		if (addDefaultColumns && sqlSelectStatement.Select.IsEmpty)
		{
			List<Symbol> list = AddDefaultColumns(sqlSelectStatement);
			if (markAllDefaultColumnsAsUsed)
			{
				foreach (Symbol item in list)
				{
					optionalColumnUsageManager.MarkAsUsed(item);
				}
			}
		}
		return sqlSelectStatement;
		IL_0028:
		sqlSelectStatement = e.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this) as SqlSelectStatement;
		goto IL_00c6;
		IL_00a6:
		sqlSelectStatement = VisitInputExpression(e, inputVarName, val, out var fromSymbol);
		AddFromSymbol(sqlSelectStatement, inputVarName, fromSymbol);
		symbolTable.ExitScope();
		goto IL_00c6;
	}

	private SqlSelectStatement VisitFilterExpression(DbExpressionBinding input, DbExpression predicate, bool negatePredicate)
	{
		Symbol fromSymbol;
		SqlSelectStatement sqlSelectStatement = VisitInputExpression(input.Expression, input.VariableName, input.VariableType, out fromSymbol);
		if (!IsCompatible(sqlSelectStatement, (DbExpressionKind)15))
		{
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, input.VariableName, input.VariableType, out fromSymbol);
		}
		selectStatementStack.Push(sqlSelectStatement);
		symbolTable.EnterScope();
		AddFromSymbol(sqlSelectStatement, input.VariableName, fromSymbol);
		if (negatePredicate)
		{
			sqlSelectStatement.Where.Append("NOT (");
		}
		sqlSelectStatement.Where.Append(predicate.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		if (negatePredicate)
		{
			sqlSelectStatement.Where.Append(")");
		}
		symbolTable.ExitScope();
		selectStatementStack.Pop();
		return sqlSelectStatement;
	}

	private static void WrapNonQueryExtent(SqlSelectStatement result, ISqlFragment sqlFragment, DbExpressionKind expressionKind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		if ((int)expressionKind == 17)
		{
			result.From.Append(sqlFragment);
			return;
		}
		result.From.Append(" (");
		result.From.Append(sqlFragment);
		result.From.Append(")");
	}

	private static string ByteArrayToBinaryString(byte[] binaryArray)
	{
		StringBuilder stringBuilder = new StringBuilder(binaryArray.Length * 2);
		for (int i = 0; i < binaryArray.Length; i++)
		{
			stringBuilder.Append(_hexDigits[(binaryArray[i] & 0xF0) >> 4]).Append(_hexDigits[binaryArray[i] & 0xF]);
		}
		return stringBuilder.ToString();
	}

	private static bool GroupByAggregatesNeedInnerQuery(IList<DbAggregate> aggregates, string inputVarRefName)
	{
		foreach (DbAggregate aggregate in aggregates)
		{
			if (GroupByAggregateNeedsInnerQuery(aggregate.Arguments[0], inputVarRefName))
			{
				return true;
			}
		}
		return false;
	}

	private static bool GroupByAggregateNeedsInnerQuery(DbExpression expression, string inputVarRefName)
	{
		return GroupByExpressionNeedsInnerQuery(expression, inputVarRefName, allowConstants: true);
	}

	private static bool GroupByKeysNeedInnerQuery(IList<DbExpression> keys, string inputVarRefName)
	{
		foreach (DbExpression key in keys)
		{
			if (GroupByKeyNeedsInnerQuery(key, inputVarRefName))
			{
				return true;
			}
		}
		return false;
	}

	private static bool GroupByKeyNeedsInnerQuery(DbExpression expression, string inputVarRefName)
	{
		return GroupByExpressionNeedsInnerQuery(expression, inputVarRefName, allowConstants: false);
	}

	private static bool GroupByExpressionNeedsInnerQuery(DbExpression expression, string inputVarRefName, bool allowConstants)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (allowConstants && (int)expression.ExpressionKind == 5)
		{
			return false;
		}
		if ((int)expression.ExpressionKind == 4)
		{
			return GroupByExpressionNeedsInnerQuery(((DbUnaryExpression)(DbCastExpression)expression).Argument, inputVarRefName, allowConstants);
		}
		if ((int)expression.ExpressionKind == 46)
		{
			return GroupByExpressionNeedsInnerQuery(((DbPropertyExpression)expression).Instance, inputVarRefName, allowConstants);
		}
		if ((int)expression.ExpressionKind == 56)
		{
			return !((DbVariableReferenceExpression)((expression is DbVariableReferenceExpression) ? expression : null)).VariableName.Equals(inputVarRefName);
		}
		return true;
	}

	private void AssertKatmaiOrNewer(PrimitiveTypeKind primitiveTypeKind)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		AssertKatmaiOrNewer(_sqlVersion, primitiveTypeKind);
	}

	private static void AssertKatmaiOrNewer(SqlVersion sqlVersion, PrimitiveTypeKind primitiveTypeKind)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (SqlVersionUtils.IsPreKatmai(sqlVersion))
		{
			throw new NotSupportedException(Strings.SqlGen_PrimitiveTypeNotSupportedPriorSql10(primitiveTypeKind));
		}
	}

	internal void AssertKatmaiOrNewer(DbFunctionExpression e)
	{
		if (IsPreKatmai)
		{
			throw new NotSupportedException(Strings.SqlGen_CanonicalFunctionNotSupportedPriorSql10(((EdmType)e.Function).Name));
		}
	}
}
