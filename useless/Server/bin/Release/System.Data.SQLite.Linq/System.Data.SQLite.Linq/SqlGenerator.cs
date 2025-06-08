using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Linq;
using System.Text;

namespace System.Data.SQLite.Linq;

internal sealed class SqlGenerator : DbExpressionVisitor<ISqlFragment>
{
	private delegate ISqlFragment FunctionHandler(SqlGenerator sqlgen, DbFunctionExpression functionExpr);

	private class KeyFieldExpressionComparer : IEqualityComparer<DbExpression>
	{
		internal static readonly KeyFieldExpressionComparer Singleton = new KeyFieldExpressionComparer();

		private KeyFieldExpressionComparer()
		{
		}

		public bool Equals(DbExpression x, DbExpression y)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Invalid comparison between Unknown and I4
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Expected O, but got Unknown
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Expected O, but got Unknown
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Invalid comparison between Unknown and I4
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Invalid comparison between Unknown and I4
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			if (x.ExpressionKind == y.ExpressionKind)
			{
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
						DbParameterReferenceExpression val3 = (DbParameterReferenceExpression)x;
						DbParameterReferenceExpression val4 = (DbParameterReferenceExpression)y;
						return val3.ParameterName == val4.ParameterName;
					}
				}
				else
				{
					if ((int)expressionKind == 46)
					{
						DbPropertyExpression val5 = (DbPropertyExpression)x;
						DbPropertyExpression val6 = (DbPropertyExpression)y;
						if (val5.Property == val6.Property)
						{
							return Equals(val5.Instance, val6.Instance);
						}
						goto IL_00bd;
					}
					if ((int)expressionKind == 56)
					{
						return x == y;
					}
				}
				return false;
			}
			goto IL_00bd;
			IL_00bd:
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
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
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

	private SQLiteProviderManifest _manifest;

	private Stack<SqlSelectStatement> selectStatementStack;

	private Stack<bool> isParentAJoinStack;

	private Dictionary<string, int> allExtentNames;

	private Dictionary<string, int> allColumnNames;

	private SymbolTable symbolTable = new SymbolTable();

	private bool isVarRefSingle;

	private static readonly Dictionary<string, FunctionHandler> _builtInFunctionHandlers = InitializeBuiltInFunctionHandlers();

	private static readonly Dictionary<string, FunctionHandler> _canonicalFunctionHandlers = InitializeCanonicalFunctionHandlers();

	private static readonly Dictionary<string, string> _functionNameToOperatorDictionary = InitializeFunctionNameToOperatorDictionary();

	private static readonly Dictionary<string, string> _datepartKeywords = InitializeDatepartKeywords();

	private static readonly char[] hexDigits = new char[16]
	{
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
		'A', 'B', 'C', 'D', 'E', 'F'
	};

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

	private bool HasBuiltMapForIn(DbExpression e, KeyToListMap<DbExpression, DbExpression> values)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		DbExpressionKind expressionKind = e.ExpressionKind;
		if ((int)expressionKind != 13)
		{
			if ((int)expressionKind != 24)
			{
				if ((int)expressionKind != 41)
				{
					return false;
				}
				DbBinaryExpression val = (DbBinaryExpression)(object)((e is DbBinaryExpression) ? e : null);
				if (HasBuiltMapForIn(val.Left, values))
				{
					return HasBuiltMapForIn(val.Right, values);
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

	private static Dictionary<string, FunctionHandler> InitializeBuiltInFunctionHandlers()
	{
		return new Dictionary<string, FunctionHandler>(7, StringComparer.Ordinal)
		{
			{ "CONCAT", HandleConcatFunction },
			{ "DATEPART", HandleDatepartDateFunction },
			{ "DatePart", HandleDatepartDateFunction },
			{ "GETDATE", HandleGetDateFunction },
			{ "GETUTCDATE", HandleGetUtcDateFunction }
		};
	}

	private static Dictionary<string, FunctionHandler> InitializeCanonicalFunctionHandlers()
	{
		return new Dictionary<string, FunctionHandler>(16, StringComparer.Ordinal)
		{
			{ "IndexOf", HandleCanonicalFunctionIndexOf },
			{ "Length", HandleCanonicalFunctionLength },
			{ "NewGuid", HandleCanonicalFunctionNewGuid },
			{ "Round", HandleCanonicalFunctionRound },
			{ "ToLower", HandleCanonicalFunctionToLower },
			{ "ToUpper", HandleCanonicalFunctionToUpper },
			{ "Trim", HandleCanonicalFunctionTrim },
			{ "Left", HandleCanonicalFunctionLeft },
			{ "Right", HandleCanonicalFunctionRight },
			{ "Substring", HandleCanonicalFunctionSubstring },
			{ "CurrentDateTime", HandleGetDateFunction },
			{ "CurrentUtcDateTime", HandleGetUtcDateFunction },
			{ "Year", HandleCanonicalFunctionDatepart },
			{ "Month", HandleCanonicalFunctionDatepart },
			{ "Day", HandleCanonicalFunctionDatepart },
			{ "Hour", HandleCanonicalFunctionDatepart },
			{ "Minute", HandleCanonicalFunctionDatepart },
			{ "Second", HandleCanonicalFunctionDatepart },
			{ "DateAdd", HandleCanonicalFunctionDateAdd },
			{ "DateDiff", HandleCanonicalFunctionDateSubtract },
			{ "DATEADD", HandleCanonicalFunctionDateAdd },
			{ "DATEDIFF", HandleCanonicalFunctionDateSubtract },
			{ "Concat", HandleConcatFunction },
			{ "BitwiseAnd", HandleCanonicalFunctionBitwise },
			{ "BitwiseNot", HandleCanonicalFunctionBitwise },
			{ "BitwiseOr", HandleCanonicalFunctionBitwise },
			{ "BitwiseXor", HandleCanonicalFunctionBitwise }
		};
	}

	private static Dictionary<string, string> InitializeDatepartKeywords()
	{
		return new Dictionary<string, string>(30, StringComparer.OrdinalIgnoreCase)
		{
			{ "d", "%d" },
			{ "day", "%d" },
			{ "dayofyear", "%j" },
			{ "dd", "%d" },
			{ "dw", "%w" },
			{ "dy", "%j" },
			{ "hh", "%H" },
			{ "hour", "%H" },
			{ "m", "%m" },
			{ "mi", "%M" },
			{ "millisecond", "%f" },
			{ "minute", "%M" },
			{ "mm", "%m" },
			{ "month", "%m" },
			{ "ms", "%f" },
			{ "n", "%M" },
			{ "s", "%S" },
			{ "second", "%S" },
			{ "ss", "%S" },
			{ "week", "%W" },
			{ "weekday", "%w" },
			{ "wk", "%W" },
			{ "ww", "%W" },
			{ "y", "%Y" },
			{ "year", "%Y" },
			{ "yy", "%Y" },
			{ "yyyy", "%Y" }
		};
	}

	private static Dictionary<string, string> InitializeFunctionNameToOperatorDictionary()
	{
		return new Dictionary<string, string>(5, StringComparer.Ordinal)
		{
			{ "Concat", "||" },
			{ "CONCAT", "||" },
			{ "BitwiseAnd", "&" },
			{ "BitwiseNot", "~" },
			{ "BitwiseOr", "|" },
			{ "BitwiseXor", "^" }
		};
	}

	private SqlGenerator(SQLiteProviderManifest manifest)
	{
		_manifest = manifest;
	}

	internal static string GenerateSql(SQLiteProviderManifest manifest, DbCommandTree tree, out List<DbParameter> parameters, out CommandType commandType)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		commandType = CommandType.Text;
		if (tree is DbQueryCommandTree)
		{
			SqlGenerator sqlGenerator = new SqlGenerator(manifest);
			parameters = null;
			return sqlGenerator.GenerateSql((DbQueryCommandTree)tree);
		}
		DbFunctionCommandTree val = (DbFunctionCommandTree)(object)((tree is DbFunctionCommandTree) ? tree : null);
		if (val != null)
		{
			SqlGenerator sqlGenerator2 = new SqlGenerator(manifest);
			parameters = null;
			return sqlGenerator2.GenerateFunctionSql(val, out commandType);
		}
		DbInsertCommandTree val2 = (DbInsertCommandTree)(object)((tree is DbInsertCommandTree) ? tree : null);
		if (val2 != null)
		{
			return DmlSqlGenerator.GenerateInsertSql(val2, out parameters);
		}
		DbDeleteCommandTree val3 = (DbDeleteCommandTree)(object)((tree is DbDeleteCommandTree) ? tree : null);
		if (val3 != null)
		{
			return DmlSqlGenerator.GenerateDeleteSql(val3, out parameters);
		}
		DbUpdateCommandTree val4 = (DbUpdateCommandTree)(object)((tree is DbUpdateCommandTree) ? tree : null);
		if (val4 != null)
		{
			return DmlSqlGenerator.GenerateUpdateSql(val4, out parameters);
		}
		throw new NotSupportedException("Unrecognized command tree type");
	}

	private string GenerateSql(DbQueryCommandTree tree)
	{
		selectStatementStack = new Stack<SqlSelectStatement>();
		isParentAJoinStack = new Stack<bool>();
		allExtentNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		allColumnNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		ISqlFragment sqlStatement;
		if (MetadataHelpers.IsCollectionType(tree.Query.ResultType))
		{
			SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(tree.Query);
			sqlSelectStatement.IsTopMost = true;
			sqlStatement = sqlSelectStatement;
		}
		else
		{
			SqlBuilder sqlBuilder = new SqlBuilder();
			sqlBuilder.Append("SELECT ");
			sqlBuilder.Append(tree.Query.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			sqlStatement = sqlBuilder;
		}
		if (isVarRefSingle)
		{
			throw new NotSupportedException();
		}
		return WriteSql(sqlStatement);
	}

	private string GenerateFunctionSql(DbFunctionCommandTree tree, out CommandType commandType)
	{
		EdmFunction edmFunction = tree.EdmFunction;
		string text = (string)((MetadataItem)edmFunction).MetadataProperties["CommandTextAttribute"].Value;
		string text2 = (string)((MetadataItem)edmFunction).MetadataProperties["StoreFunctionNameAttribute"].Value;
		if (string.IsNullOrEmpty(text))
		{
			commandType = CommandType.StoredProcedure;
			return QuoteIdentifier(string.IsNullOrEmpty(text2) ? ((EdmType)edmFunction).Name : text2);
		}
		commandType = CommandType.Text;
		return text;
	}

	private string WriteSql(ISqlFragment sqlStatement)
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		using (SqlWriter writer = new SqlWriter(stringBuilder))
		{
			sqlStatement.WriteSql(writer, this);
		}
		return stringBuilder.ToString();
	}

	private bool TryTranslateIntoIn(DbOrExpression e, out ISqlFragment sqlFragment)
	{
		KeyToListMap<DbExpression, DbExpression> keyToListMap = new KeyToListMap<DbExpression, DbExpression>(KeyFieldExpressionComparer.Singleton);
		if (!HasBuiltMapForIn((DbExpression)(object)e, keyToListMap) || keyToListMap.Keys.Count() <= 0)
		{
			sqlFragment = null;
			return false;
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		bool flag = true;
		foreach (DbExpression key in keyToListMap.Keys)
		{
			ReadOnlyCollection<DbExpression> source = keyToListMap.ListForKey(key);
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
			if (num == 1)
			{
				ParanthesizeExpressionIfNeeded(key, sqlBuilder);
				sqlBuilder.Append(" = ");
				DbExpression value = enumerable.First();
				ParenthesizeExpressionWithoutRedundantConstantCasts(value, sqlBuilder);
			}
			if (num > 1)
			{
				ParanthesizeExpressionIfNeeded(key, sqlBuilder);
				sqlBuilder.Append(" IN (");
				bool flag2 = true;
				foreach (DbExpression item in enumerable)
				{
					if (!flag2)
					{
						sqlBuilder.Append(",");
					}
					else
					{
						flag2 = false;
					}
					ParenthesizeExpressionWithoutRedundantConstantCasts(item, sqlBuilder);
				}
				sqlBuilder.Append(")");
			}
			DbExpression? obj = ((IEnumerable<DbExpression>)source).FirstOrDefault((Func<DbExpression, bool>)((DbExpression v) => (int)v.ExpressionKind == 24));
			DbIsNullExpression val = (DbIsNullExpression)(object)((obj is DbIsNullExpression) ? obj : null);
			if (val != null)
			{
				if (num > 0)
				{
					sqlBuilder.Append(" OR ");
				}
				sqlBuilder.Append(VisitIsNullExpression(val, negate: false));
			}
		}
		sqlFragment = sqlBuilder;
		return true;
	}

	public override ISqlFragment Visit(DbAndExpression e)
	{
		return VisitBinaryExpression(" AND ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
	}

	public override ISqlFragment Visit(DbApplyExpression e)
	{
		throw new NotSupportedException("APPLY joins are not supported");
	}

	public override ISqlFragment Visit(DbArithmeticExpression e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected I4, but got Unknown
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
					goto IL_0092;
				case 2:
					goto IL_00bb;
				default:
					goto IL_013d;
				}
				sqlBuilder = VisitBinaryExpression(" - ", e.Arguments[0], e.Arguments[1]);
			}
			else
			{
				sqlBuilder = VisitBinaryExpression(" / ", e.Arguments[0], e.Arguments[1]);
			}
		}
		else if ((int)expressionKind != 44)
		{
			if ((int)expressionKind != 54)
			{
				goto IL_013d;
			}
			sqlBuilder = new SqlBuilder();
			sqlBuilder.Append(" -(");
			sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			sqlBuilder.Append(")");
		}
		else
		{
			sqlBuilder = VisitBinaryExpression(" + ", e.Arguments[0], e.Arguments[1]);
		}
		goto IL_0143;
		IL_0092:
		sqlBuilder = VisitBinaryExpression(" % ", e.Arguments[0], e.Arguments[1]);
		goto IL_0143;
		IL_0143:
		return sqlBuilder;
		IL_00bb:
		sqlBuilder = VisitBinaryExpression(" * ", e.Arguments[0], e.Arguments[1]);
		goto IL_0143;
		IL_013d:
		throw new InvalidOperationException();
	}

	public override ISqlFragment Visit(DbCaseExpression e)
	{
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
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(((DbUnaryExpression)e).Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbComparisonExpression e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		DbExpressionKind expressionKind = ((DbExpression)e).ExpressionKind;
		if ((int)expressionKind <= 19)
		{
			if ((int)expressionKind == 13)
			{
				return VisitBinaryExpression(" = ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
			if ((int)expressionKind == 18)
			{
				return VisitBinaryExpression(" > ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
			if ((int)expressionKind == 19)
			{
				return VisitBinaryExpression(" >= ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
		}
		else
		{
			if ((int)expressionKind == 28)
			{
				return VisitBinaryExpression(" < ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
			if ((int)expressionKind == 29)
			{
				return VisitBinaryExpression(" <= ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
			if ((int)expressionKind == 37)
			{
				return VisitBinaryExpression(" <> ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
			}
		}
		throw new InvalidOperationException();
	}

	public override ISqlFragment Visit(DbConstantExpression e)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected I4, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (MetadataHelpers.TryGetPrimitiveTypeKind(((DbExpression)e).ResultType, out var typeKind))
		{
			switch ((int)typeKind)
			{
			case 10:
				sqlBuilder.Append(e.Value.ToString());
				break;
			case 0:
				ToBlobLiteral((byte[])e.Value, sqlBuilder);
				break;
			case 1:
				sqlBuilder.Append(((bool)e.Value) ? "1" : "0");
				break;
			case 2:
				sqlBuilder.Append(e.Value.ToString());
				break;
			case 3:
			{
				bool num = NeedSingleQuotes(_manifest._dateTimeFormat);
				string s = SQLiteConvert.ToString((DateTime)e.Value, _manifest._dateTimeFormat, _manifest._dateTimeKind, _manifest._dateTimeFormatString);
				if (num)
				{
					sqlBuilder.Append(EscapeSingleQuote(s, isUnicode: false));
				}
				else
				{
					sqlBuilder.Append(s);
				}
				break;
			}
			case 4:
			{
				string text = ((decimal)e.Value).ToString(CultureInfo.InvariantCulture);
				if (-1 == text.IndexOf('.') && text.TrimStart(new char[1] { '-' }).Length < 20)
				{
					byte val = (byte)text.Length;
					if (MetadataHelpers.TryGetTypeFacetDescriptionByName(((DbExpression)e).ResultType.EdmType, "precision", out var facetDescription) && facetDescription.DefaultValue != null)
					{
						val = Math.Max(val, (byte)facetDescription.DefaultValue);
					}
					sqlBuilder.Append(text);
				}
				else
				{
					sqlBuilder.Append(text);
				}
				break;
			}
			case 5:
				sqlBuilder.Append(((double)e.Value).ToString(CultureInfo.InvariantCulture));
				break;
			case 6:
			{
				object value = e.Value;
				if (_manifest._binaryGuid && value is Guid guid)
				{
					ToBlobLiteral(guid.ToByteArray(), sqlBuilder);
				}
				else
				{
					sqlBuilder.Append(EscapeSingleQuote(e.Value.ToString(), isUnicode: false));
				}
				break;
			}
			case 9:
				sqlBuilder.Append(e.Value.ToString());
				break;
			case 11:
				sqlBuilder.Append(e.Value.ToString());
				break;
			case 7:
				sqlBuilder.Append(((float)e.Value).ToString(CultureInfo.InvariantCulture));
				break;
			case 12:
			{
				bool facetValueOrDefault = MetadataHelpers.GetFacetValueOrDefault(((DbExpression)e).ResultType, MetadataHelpers.UnicodeFacetName, defaultValue: true);
				sqlBuilder.Append(EscapeSingleQuote(e.Value as string, facetValueOrDefault));
				break;
			}
			case 14:
				throw new NotSupportedException("datetimeoffset");
			case 13:
				throw new NotSupportedException("time");
			default:
				throw new NotSupportedException();
			}
			return sqlBuilder;
		}
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbDerefExpression e)
	{
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbDistinctExpression e)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(((DbUnaryExpression)e).Argument);
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			TypeUsage elementTypeUsage = MetadataHelpers.GetElementTypeUsage(((DbUnaryExpression)e).Argument.ResultType);
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, "DISTINCT", elementTypeUsage, out var fromSymbol);
			AddFromSymbol(sqlSelectStatement, "DISTINCT", fromSymbol, addToSymbolTable: false);
		}
		sqlSelectStatement.IsDistinct = true;
		return sqlSelectStatement;
	}

	public override ISqlFragment Visit(DbElementExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("(");
		sqlBuilder.Append(VisitExpressionEnsureSqlStatement(((DbUnaryExpression)e).Argument));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbExceptExpression e)
	{
		return VisitSetOpExpression(((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right, "EXCEPT");
	}

	public override ISqlFragment Visit(DbExpression e)
	{
		throw new InvalidOperationException();
	}

	public override ISqlFragment Visit(DbScanExpression e)
	{
		EntitySetBase target = e.Target;
		if (IsParentAJoin)
		{
			SqlBuilder sqlBuilder = new SqlBuilder();
			sqlBuilder.Append(GetTargetTSql(target));
			return sqlBuilder;
		}
		SqlSelectStatement sqlSelectStatement = new SqlSelectStatement();
		sqlSelectStatement.From.Append(GetTargetTSql(target));
		return sqlSelectStatement;
	}

	internal static string GetTargetTSql(EntitySetBase entitySetBase)
	{
		StringBuilder stringBuilder = new StringBuilder(50);
		string value = MetadataHelpers.TryGetValueForMetadataProperty<string>((MetadataItem)(object)entitySetBase, "DefiningQuery");
		if (!string.IsNullOrEmpty(value))
		{
			stringBuilder.Append("(");
			stringBuilder.Append(value);
			stringBuilder.Append(")");
		}
		else
		{
			string text = MetadataHelpers.TryGetValueForMetadataProperty<string>((MetadataItem)(object)entitySetBase, "Table");
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append(QuoteIdentifier(text));
			}
			else
			{
				stringBuilder.Append(QuoteIdentifier(entitySetBase.Name));
			}
		}
		return stringBuilder.ToString();
	}

	public override ISqlFragment Visit(DbFilterExpression e)
	{
		return VisitFilterExpression(e.Input, e.Predicate, negatePredicate: false);
	}

	public override ISqlFragment Visit(DbFunctionExpression e)
	{
		if (IsSpecialBuiltInFunction(e))
		{
			return HandleSpecialBuiltInFunction(e);
		}
		if (IsSpecialCanonicalFunction(e))
		{
			return HandleSpecialCanonicalFunction(e);
		}
		return HandleFunctionDefault(e);
	}

	public override ISqlFragment Visit(DbEntityRefExpression e)
	{
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbRefKeyExpression e)
	{
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbGroupByExpression e)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
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
		RowType edmType = MetadataHelpers.GetEdmType<RowType>(MetadataHelpers.GetEdmType<CollectionType>(((DbExpression)e).ResultType).TypeUsage);
		bool flag = NeedsInnerQuery(e.Aggregates);
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
		using (IEnumerator<EdmProperty> enumerator = (object)edmType.Properties.GetEnumerator())
		{
			enumerator.MoveNext();
			string s = string.Empty;
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
				string s4 = QuoteIdentifier(((EdmMember)enumerator.Current).Name);
				ISqlFragment sqlFragment = aggregate.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this);
				object aggregateArgument;
				if (flag)
				{
					SqlBuilder sqlBuilder = new SqlBuilder();
					sqlBuilder.Append(fromSymbol);
					sqlBuilder.Append(".");
					sqlBuilder.Append(s4);
					aggregateArgument = sqlBuilder;
					sqlSelectStatement.Select.Append(s);
					sqlSelectStatement.Select.AppendLine();
					sqlSelectStatement.Select.Append(sqlFragment);
					sqlSelectStatement.Select.Append(" AS ");
					sqlSelectStatement.Select.Append(s4);
				}
				else
				{
					aggregateArgument = sqlFragment;
				}
				ISqlFragment s5 = VisitAggregate(aggregate, aggregateArgument);
				sqlSelectStatement2.Select.Append(s);
				sqlSelectStatement2.Select.AppendLine();
				sqlSelectStatement2.Select.Append(s5);
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
		return VisitSetOpExpression(((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right, "INTERSECT");
	}

	public override ISqlFragment Visit(DbIsEmptyExpression e)
	{
		return VisitIsEmptyExpression(e, negate: false);
	}

	public override ISqlFragment Visit(DbIsNullExpression e)
	{
		return VisitIsNullExpression(e, negate: false);
	}

	public override ISqlFragment Visit(DbIsOfExpression e)
	{
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbCrossJoinExpression e)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return VisitJoinExpression(e.Inputs, ((DbExpression)e).ExpressionKind, "CROSS JOIN", null);
	}

	public override ISqlFragment Visit(DbJoinExpression e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		DbExpressionKind expressionKind = ((DbExpression)e).ExpressionKind;
		string joinString = (((int)expressionKind == 16) ? "FULL OUTER JOIN" : (((int)expressionKind == 21) ? "INNER JOIN" : (((int)expressionKind != 27) ? null : "LEFT OUTER JOIN")));
		List<DbExpressionBinding> list = new List<DbExpressionBinding>(2);
		list.Add(e.Left);
		list.Add(e.Right);
		return VisitJoinExpression(list, ((DbExpression)e).ExpressionKind, joinString, e.JoinCondition);
	}

	public override ISqlFragment Visit(DbLikeExpression e)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(e.Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		sqlBuilder.Append(" LIKE ");
		sqlBuilder.Append(e.Pattern.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		if ((int)e.Escape.ExpressionKind != 38)
		{
			sqlBuilder.Append(" ESCAPE ");
			sqlBuilder.Append(e.Escape.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		}
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbLimitExpression e)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(e.Argument, addDefaultColumns: false);
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			TypeUsage elementTypeUsage = MetadataHelpers.GetElementTypeUsage(e.Argument.ResultType);
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, "top", elementTypeUsage, out var fromSymbol);
			AddFromSymbol(sqlSelectStatement, "top", fromSymbol, addToSymbolTable: false);
		}
		ISqlFragment topCount = HandleCountExpression(e.Limit);
		sqlSelectStatement.Top = new TopClause(topCount, e.WithTies);
		return sqlSelectStatement;
	}

	public override ISqlFragment Visit(DbNewInstanceExpression e)
	{
		if (MetadataHelpers.IsCollectionType(((DbExpression)e).ResultType))
		{
			return VisitCollectionConstructor(e);
		}
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbNotExpression e)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Invalid comparison between Unknown and I4
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
			return VisitBinaryExpression(" <> ", ((DbBinaryExpression)val4).Left, ((DbBinaryExpression)val4).Right);
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(" NOT (");
		sqlBuilder.Append(((DbUnaryExpression)e).Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbNullExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("NULL");
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbOfTypeExpression e)
	{
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbOrExpression e)
	{
		ISqlFragment sqlFragment = null;
		if (TryTranslateIntoIn(e, out sqlFragment))
		{
			return sqlFragment;
		}
		return VisitBinaryExpression(" OR ", ((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right);
	}

	public override ISqlFragment Visit(DbParameterReferenceExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("@" + e.ParameterName);
		return sqlBuilder;
	}

	public override ISqlFragment Visit(DbProjectExpression e)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Symbol fromSymbol;
		SqlSelectStatement sqlSelectStatement = VisitInputExpression(e.Input.Expression, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		if (!IsCompatible(sqlSelectStatement, ((DbExpression)e).ExpressionKind))
		{
			sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, e.Input.VariableName, e.Input.VariableType, out fromSymbol);
		}
		selectStatementStack.Push(sqlSelectStatement);
		symbolTable.EnterScope();
		AddFromSymbol(sqlSelectStatement, e.Input.VariableName, fromSymbol);
		DbExpression projection = e.Projection;
		DbNewInstanceExpression val = (DbNewInstanceExpression)(object)((projection is DbNewInstanceExpression) ? projection : null);
		if (val != null)
		{
			sqlSelectStatement.Select.Append(VisitNewInstanceExpression(val));
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
		if (sqlFragment is SymbolPair symbolPair)
		{
			if (symbolPair.Column is JoinSymbol joinSymbol2)
			{
				symbolPair.Column = joinSymbol2.NameToExtent[e.Property.Name];
				return symbolPair;
			}
			if (symbolPair.Column.Columns.ContainsKey(e.Property.Name))
			{
				SqlBuilder sqlBuilder = new SqlBuilder();
				sqlBuilder.Append(symbolPair.Source);
				sqlBuilder.Append(".");
				sqlBuilder.Append(symbolPair.Column.Columns[e.Property.Name]);
				return sqlBuilder;
			}
		}
		SqlBuilder sqlBuilder2 = new SqlBuilder();
		sqlBuilder2.Append(sqlFragment);
		sqlBuilder2.Append(".");
		sqlBuilder2.Append(QuoteIdentifier(e.Property.Name));
		return sqlBuilder2;
	}

	public override ISqlFragment Visit(DbQuantifierExpression e)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
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
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbRelationshipNavigationExpression e)
	{
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbSkipExpression e)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
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
		ISqlFragment skipCount = HandleCountExpression(e.Count);
		sqlSelectStatement.Skip = new SkipClause(skipCount);
		return sqlSelectStatement;
	}

	public override ISqlFragment Visit(DbSortExpression e)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
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
		throw new NotSupportedException();
	}

	public override ISqlFragment Visit(DbUnionAllExpression e)
	{
		return VisitSetOpExpression(((DbBinaryExpression)e).Left, ((DbBinaryExpression)e).Right, "UNION ALL");
	}

	public override ISqlFragment Visit(DbVariableReferenceExpression e)
	{
		if (isVarRefSingle)
		{
			throw new NotSupportedException();
		}
		isVarRefSingle = true;
		Symbol symbol = symbolTable.Lookup(e.VariableName);
		if (!CurrentSelectStatement.FromExtents.Contains(symbol))
		{
			CurrentSelectStatement.OuterExtents[symbol] = true;
		}
		return symbol;
	}

	private SqlBuilder VisitAggregate(DbAggregate aggregate, object aggregateArgument)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		DbFunctionAggregate val = (DbFunctionAggregate)(object)((aggregate is DbFunctionAggregate) ? aggregate : null);
		if (val == null)
		{
			throw new NotSupportedException();
		}
		WriteFunctionName(sqlBuilder, val.Function);
		sqlBuilder.Append("(");
		DbFunctionAggregate val2 = val;
		if (val2 != null && val2.Distinct)
		{
			sqlBuilder.Append("DISTINCT ");
		}
		sqlBuilder.Append(aggregateArgument);
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private SqlBuilder VisitBinaryExpression(string op, DbExpression left, DbExpression right)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (IsComplexExpression(left))
		{
			sqlBuilder.Append("(");
		}
		sqlBuilder.Append(left.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		if (IsComplexExpression(left))
		{
			sqlBuilder.Append(")");
		}
		sqlBuilder.Append(op);
		if (IsComplexExpression(right))
		{
			sqlBuilder.Append("(");
		}
		sqlBuilder.Append(right.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		if (IsComplexExpression(right))
		{
			sqlBuilder.Append(")");
		}
		return sqlBuilder;
	}

	private SqlSelectStatement VisitInputExpression(DbExpression inputExpression, string inputVarName, TypeUsage inputVarType, out Symbol fromSymbol)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		if (e.Arguments.Count == 1 && (int)e.Arguments[0].ExpressionKind == 11)
		{
			DbExpression obj = e.Arguments[0];
			DbElementExpression val = (DbElementExpression)(object)((obj is DbElementExpression) ? obj : null);
			SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(((DbUnaryExpression)val).Argument);
			if (!IsCompatible(sqlSelectStatement, (DbExpressionKind)11))
			{
				TypeUsage elementTypeUsage = MetadataHelpers.GetElementTypeUsage(((DbUnaryExpression)val).Argument.ResultType);
				sqlSelectStatement = CreateNewSelectStatement(sqlSelectStatement, "element", elementTypeUsage, out var fromSymbol);
				AddFromSymbol(sqlSelectStatement, "element", fromSymbol, addToSymbolTable: false);
			}
			sqlSelectStatement.Top = new TopClause(1, withTies: false);
			return sqlSelectStatement;
		}
		bool flag = MetadataHelpers.IsPrimitiveType(MetadataHelpers.GetEdmType<CollectionType>(((DbExpression)e).ResultType).TypeUsage);
		SqlBuilder sqlBuilder = new SqlBuilder();
		string s = string.Empty;
		if (e.Arguments.Count == 0)
		{
			sqlBuilder.Append(" SELECT NULL");
			sqlBuilder.Append(" AS X FROM (SELECT 1) AS Y WHERE 1=0");
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
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(((DbUnaryExpression)e).Argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
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
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Invalid comparison between Unknown and I4
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Invalid comparison between Unknown and I4
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Invalid comparison between Unknown and I4
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Invalid comparison between Unknown and I4
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
		string text = string.Empty;
		bool flag = true;
		int count = inputs.Count;
		for (int i = 0; i < count; i++)
		{
			DbExpressionBinding val = inputs[i];
			if (text != string.Empty)
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
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
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
					else if (sqlSelectStatement.FromExtents[0] is JoinSymbol joinSymbol)
					{
						symbol = new JoinSymbol(input.VariableName, input.VariableType, joinSymbol.ExtentList)
						{
							IsNestedJoin = true,
							ColumnList = columnList,
							FlattenedExtentList = joinSymbol.FlattenedExtentList
						};
					}
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

	private ISqlFragment VisitNewInstanceExpression(DbNewInstanceExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		EdmType edmType = ((DbExpression)e).ResultType.EdmType;
		RowType val = (RowType)(object)((edmType is RowType) ? edmType : null);
		if (val != null)
		{
			ReadOnlyMetadataCollection<EdmProperty> properties = val.Properties;
			string s = string.Empty;
			for (int i = 0; i < e.Arguments.Count; i++)
			{
				DbExpression val2 = e.Arguments[i];
				if (MetadataHelpers.IsRowType(val2.ResultType))
				{
					throw new NotSupportedException();
				}
				EdmProperty val3 = ((ReadOnlyCollection<EdmProperty>)(object)properties)[i];
				sqlBuilder.Append(s);
				sqlBuilder.AppendLine();
				sqlBuilder.Append(val2.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
				sqlBuilder.Append(" AS ");
				sqlBuilder.Append(QuoteIdentifier(((EdmMember)val3).Name));
				s = ", ";
			}
			return sqlBuilder;
		}
		throw new NotSupportedException();
	}

	private ISqlFragment VisitSetOpExpression(DbExpression left, DbExpression right, string separator)
	{
		SqlSelectStatement sqlSelectStatement = VisitExpressionEnsureSqlStatement(left);
		bool num = sqlSelectStatement.HaveOrderByLimitOrOffset();
		SqlSelectStatement sqlSelectStatement2 = VisitExpressionEnsureSqlStatement(right);
		bool flag = sqlSelectStatement2.HaveOrderByLimitOrOffset();
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (num)
		{
			sqlBuilder.Append("SELECT * FROM (");
		}
		sqlBuilder.Append(sqlSelectStatement);
		if (num)
		{
			sqlBuilder.Append(") ");
		}
		sqlBuilder.AppendLine();
		sqlBuilder.Append(separator);
		sqlBuilder.AppendLine();
		if (flag)
		{
			sqlBuilder.Append("SELECT * FROM (");
		}
		sqlBuilder.Append(sqlSelectStatement2);
		if (flag)
		{
			sqlBuilder.Append(") ");
		}
		return sqlBuilder;
	}

	private bool IsSpecialBuiltInFunction(DbFunctionExpression e)
	{
		if (IsBuiltinFunction(e.Function))
		{
			return _builtInFunctionHandlers.ContainsKey(((EdmType)e.Function).Name);
		}
		return false;
	}

	private bool IsSpecialCanonicalFunction(DbFunctionExpression e)
	{
		if (MetadataHelpers.IsCanonicalFunction(e.Function))
		{
			return _canonicalFunctionHandlers.ContainsKey(((EdmType)e.Function).Name);
		}
		return false;
	}

	private ISqlFragment HandleFunctionDefault(DbFunctionExpression e)
	{
		SqlBuilder result = new SqlBuilder();
		WriteFunctionName(result, e.Function);
		HandleFunctionArgumentsDefault(e, result);
		return result;
	}

	private ISqlFragment HandleFunctionDefaultGivenName(DbFunctionExpression e, string functionName)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append(functionName);
		HandleFunctionArgumentsDefault(e, sqlBuilder);
		return sqlBuilder;
	}

	private void HandleFunctionArgumentsDefault(DbFunctionExpression e, SqlBuilder result)
	{
		bool num = MetadataHelpers.TryGetValueForMetadataProperty<bool>((MetadataItem)(object)e.Function, "NiladicFunctionAttribute");
		if (num && e.Arguments.Count > 0)
		{
			throw new InvalidOperationException("Niladic functions cannot have parameters");
		}
		if (num)
		{
			return;
		}
		result.Append("(");
		string s = string.Empty;
		foreach (DbExpression argument in e.Arguments)
		{
			result.Append(s);
			result.Append(argument.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			s = ", ";
		}
		result.Append(")");
	}

	private ISqlFragment HandleSpecialBuiltInFunction(DbFunctionExpression e)
	{
		return HandleSpecialFunction(_builtInFunctionHandlers, e);
	}

	private ISqlFragment HandleSpecialCanonicalFunction(DbFunctionExpression e)
	{
		return HandleSpecialFunction(_canonicalFunctionHandlers, e);
	}

	private ISqlFragment HandleSpecialFunction(Dictionary<string, FunctionHandler> handlers, DbFunctionExpression e)
	{
		if (!handlers.ContainsKey(((EdmType)e.Function).Name))
		{
			throw new InvalidOperationException("Special handling should be called only for functions in the list of special functions");
		}
		return handlers[((EdmType)e.Function).Name](this, e);
	}

	private ISqlFragment HandleSpecialFunctionToOperator(DbFunctionExpression e, bool parenthesiseArguments)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (e.Arguments.Count > 1)
		{
			if (parenthesiseArguments)
			{
				sqlBuilder.Append("(");
			}
			sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
			if (parenthesiseArguments)
			{
				sqlBuilder.Append(")");
			}
		}
		sqlBuilder.Append(" ");
		sqlBuilder.Append(_functionNameToOperatorDictionary[((EdmType)e.Function).Name]);
		sqlBuilder.Append(" ");
		if (parenthesiseArguments)
		{
			sqlBuilder.Append("(");
		}
		sqlBuilder.Append(e.Arguments[e.Arguments.Count - 1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this));
		if (parenthesiseArguments)
		{
			sqlBuilder.Append(")");
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleConcatFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleSpecialFunctionToOperator(e, parenthesiseArguments: false);
	}

	private static ISqlFragment HandleCanonicalFunctionBitwise(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleSpecialFunctionToOperator(e, parenthesiseArguments: true);
	}

	private static ISqlFragment HandleGetDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		SqlBuilder sqlBuilder = new SqlBuilder();
		SQLiteDateFormats dateTimeFormat = sqlgen._manifest._dateTimeFormat;
		if ((int)dateTimeFormat != 0)
		{
			if ((int)dateTimeFormat == 2)
			{
				sqlBuilder.Append("CAST(STRFTIME('%J', 'now') AS double)");
			}
			else
			{
				sqlBuilder.Append("STRFTIME('%Y-%m-%d %H:%M:%S', 'now')");
			}
		}
		else
		{
			sqlBuilder.Append("(STRFTIME('%s', 'now') * 10000000 + 621355968000000000)");
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleGetUtcDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		SqlBuilder sqlBuilder = new SqlBuilder();
		SQLiteDateFormats dateTimeFormat = sqlgen._manifest._dateTimeFormat;
		if ((int)dateTimeFormat != 0)
		{
			if ((int)dateTimeFormat == 2)
			{
				sqlBuilder.Append("CAST(STRFTIME('%J', 'now', 'utc') AS double)");
			}
			else
			{
				sqlBuilder.Append("STRFTIME('%Y-%m-%d %H:%M:%S', 'now', 'utc')");
			}
		}
		else
		{
			sqlBuilder.Append("(STRFTIME('%s', 'now', 'utc') * 10000000 + 621355968000000000)");
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleDatepartDateFunction(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		DbExpression obj = e.Arguments[0];
		DbConstantExpression val = (DbConstantExpression)(object)((obj is DbConstantExpression) ? obj : null);
		if (val == null)
		{
			throw new InvalidOperationException($"DATEPART argument to function '{((EdmType)e.Function).NamespaceName}.{((EdmType)e.Function).Name}' must be a literal string");
		}
		if (!(val.Value is string text))
		{
			throw new InvalidOperationException($"DATEPART argument to function '{((EdmType)e.Function).NamespaceName}.{((EdmType)e.Function).Name}' must be a literal string");
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		if (!_datepartKeywords.TryGetValue(text, out var value))
		{
			throw new InvalidOperationException($"{text}' is not a valid value for DATEPART argument in '{((EdmType)e.Function).NamespaceName}.{((EdmType)e.Function).Name}' function");
		}
		if (value != "%f")
		{
			sqlBuilder.Append("CAST(STRFTIME('");
			sqlBuilder.Append(value);
			sqlBuilder.Append("', ");
			SQLiteDateFormats dateTimeFormat = sqlgen._manifest._dateTimeFormat;
			if ((int)dateTimeFormat == 0)
			{
				sqlBuilder.Append($"(({e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)} - 621355968000000000) / 10000000.0)");
			}
			else
			{
				sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			}
			sqlBuilder.Append(") AS integer)");
		}
		else
		{
			sqlBuilder.Append("CAST(SUBSTR(STRFTIME('%f', ");
			SQLiteDateFormats dateTimeFormat = sqlgen._manifest._dateTimeFormat;
			if ((int)dateTimeFormat == 0)
			{
				sqlBuilder.Append($"(({e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)} - 621355968000000000) / 10000000.0)");
			}
			else
			{
				sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			}
			sqlBuilder.Append("), 4) AS integer)");
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionDateAdd(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		SqlBuilder sqlBuilder = new SqlBuilder();
		SQLiteDateFormats dateTimeFormat = sqlgen._manifest._dateTimeFormat;
		if ((int)dateTimeFormat != 0)
		{
			if ((int)dateTimeFormat == 2)
			{
				sqlBuilder.Append(string.Format("CAST(STRFTIME('%J', JULIANDAY({1}) + ({0} / 86400.0)) AS double)", e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen), e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)));
			}
			else
			{
				sqlBuilder.Append(string.Format("STRFTIME('%Y-%m-%d %H:%M:%S', JULIANDAY({1}) + ({0} / 86400.0))", e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen), e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)));
			}
		}
		else
		{
			sqlBuilder.Append(string.Format("(STRFTIME('%s', JULIANDAY({1}) + ({0} / 86400.0)) * 10000000 + 621355968000000000)", e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen), e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)));
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionDateSubtract(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		SqlBuilder sqlBuilder = new SqlBuilder();
		SQLiteDateFormats dateTimeFormat = sqlgen._manifest._dateTimeFormat;
		if ((int)dateTimeFormat == 0)
		{
			sqlBuilder.Append($"CAST((({e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)} - 621355968000000000) / 10000000.0)  - (({e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)} - 621355968000000000) / 10000000.0) * 86400.0 AS integer)");
		}
		else
		{
			sqlBuilder.Append(string.Format("CAST((JULIANDAY({1}) - JULIANDAY({0})) * 86400.0 AS integer)", e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen), e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)));
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionDatepart(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (!_datepartKeywords.TryGetValue(((EdmType)e.Function).Name, out var value))
		{
			throw new InvalidOperationException($"{((EdmType)e.Function).Name}' is not a valid value for STRFTIME argument");
		}
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("CAST(STRFTIME('");
		sqlBuilder.Append(value);
		sqlBuilder.Append("', ");
		SQLiteDateFormats dateTimeFormat = sqlgen._manifest._dateTimeFormat;
		if ((int)dateTimeFormat == 0)
		{
			sqlBuilder.Append($"(({e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen)} - 621355968000000000) / 10000000.0)");
		}
		else
		{
			sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		}
		sqlBuilder.Append(") AS integer)");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionIndexOf(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "CHARINDEX");
	}

	private static ISqlFragment HandleCanonicalFunctionNewGuid(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("RANDOMBLOB(16)");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionLength(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("LENGTH(");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionRound(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("ROUND(");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		if (e.Arguments.Count == 2)
		{
			sqlBuilder.Append(", ");
			sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
			sqlBuilder.Append(")");
		}
		else
		{
			sqlBuilder.Append(", 0)");
		}
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionTrim(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("TRIM(");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionLeft(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("SUBSTR(");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(", 1, ");
		sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionRight(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("SUBSTR(");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(", -(");
		sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append("), ");
		sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionSubstring(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		SqlBuilder sqlBuilder = new SqlBuilder();
		sqlBuilder.Append("SUBSTR(");
		sqlBuilder.Append(e.Arguments[0].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		sqlBuilder.Append(", ");
		sqlBuilder.Append(e.Arguments[1].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		if (e.Arguments.Count == 3)
		{
			sqlBuilder.Append(", ");
			sqlBuilder.Append(e.Arguments[2].Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)sqlgen));
		}
		sqlBuilder.Append(")");
		return sqlBuilder;
	}

	private static ISqlFragment HandleCanonicalFunctionToLower(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "LOWER");
	}

	private static ISqlFragment HandleCanonicalFunctionToUpper(SqlGenerator sqlgen, DbFunctionExpression e)
	{
		return sqlgen.HandleFunctionDefaultGivenName(e, "UPPER");
	}

	private void AddColumns(SqlSelectStatement selectStatement, Symbol symbol, List<Symbol> columnList, Dictionary<string, Symbol> columnDictionary, ref string separator)
	{
		if (symbol is JoinSymbol joinSymbol)
		{
			if (!joinSymbol.IsNestedJoin)
			{
				foreach (Symbol extent in joinSymbol.ExtentList)
				{
					if (!MetadataHelpers.IsPrimitiveType(extent.Type))
					{
						AddColumns(selectStatement, extent, columnList, columnDictionary, ref separator);
					}
				}
				return;
			}
			{
				foreach (Symbol column in joinSymbol.ColumnList)
				{
					selectStatement.Select.Append(separator);
					selectStatement.Select.Append(symbol);
					selectStatement.Select.Append(".");
					selectStatement.Select.Append(column);
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
					separator = ", ";
				}
				return;
			}
		}
		foreach (EdmProperty property in MetadataHelpers.GetProperties(symbol.Type))
		{
			string name = ((EdmMember)property).Name;
			allColumnNames[name] = 0;
			if (!symbol.Columns.TryGetValue(name, out var value))
			{
				value = new Symbol(name, null);
				symbol.Columns.Add(name, value);
			}
			selectStatement.Select.Append(separator);
			selectStatement.Select.Append(symbol);
			selectStatement.Select.Append(".");
			selectStatement.Select.Append(QuoteIdentifier(name));
			selectStatement.Select.Append(" AS ");
			selectStatement.Select.Append(value);
			if (columnDictionary.ContainsKey(name))
			{
				columnDictionary[name].NeedsRenaming = true;
				value.NeedsRenaming = true;
			}
			else
			{
				columnDictionary[name] = symbol.Columns[name];
			}
			columnList.Add(value);
			separator = ", ";
		}
	}

	private List<Symbol> AddDefaultColumns(SqlSelectStatement selectStatement)
	{
		List<Symbol> list = new List<Symbol>();
		Dictionary<string, Symbol> columnDictionary = new Dictionary<string, Symbol>(StringComparer.OrdinalIgnoreCase);
		string separator = string.Empty;
		if (!selectStatement.Select.IsEmpty)
		{
			separator = ", ";
		}
		foreach (Symbol fromExtent in selectStatement.FromExtents)
		{
			AddColumns(selectStatement, fromExtent, list, columnDictionary, ref separator);
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
		string s = string.Empty;
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
			fromSymbol = new Symbol(inputVarName, inputVarType);
		}
		SqlSelectStatement sqlSelectStatement = new SqlSelectStatement();
		sqlSelectStatement.From.Append("( ");
		sqlSelectStatement.From.Append(oldStatement);
		sqlSelectStatement.From.AppendLine();
		sqlSelectStatement.From.Append(") ");
		return sqlSelectStatement;
	}

	private static bool NeedSingleQuotes(SQLiteDateFormats format)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I4
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		if ((int)format != 0 && (int)format != 2)
		{
			return (int)format != 3;
		}
		return false;
	}

	private static string EscapeSingleQuote(string s, bool isUnicode)
	{
		return "'" + s.Replace("'", "''") + "'";
	}

	private string GetSqlPrimitiveType(TypeUsage type)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected I4, but got Unknown
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		PrimitiveType edmType = MetadataHelpers.GetEdmType<PrimitiveType>(type);
		string name = ((EdmType)edmType).Name;
		bool flag = true;
		bool flag2 = false;
		int num = 0;
		string text = "max";
		byte b = 0;
		byte b2 = 0;
		PrimitiveTypeKind primitiveTypeKind = edmType.PrimitiveTypeKind;
		switch ((int)primitiveTypeKind)
		{
		case 0:
			num = MetadataHelpers.GetFacetValueOrDefault(type, MetadataHelpers.MaxLengthFacetName, MetadataHelpers.BinaryMaxMaxLength);
			return string.Concat(str1: (num != MetadataHelpers.BinaryMaxMaxLength) ? num.ToString(CultureInfo.InvariantCulture) : "max", str0: MetadataHelpers.GetFacetValueOrDefault(type, MetadataHelpers.FixedLengthFacetName, defaultValue: false) ? "binary(" : "varbinary(", str2: ")");
		case 12:
			flag = MetadataHelpers.GetFacetValueOrDefault(type, MetadataHelpers.UnicodeFacetName, defaultValue: true);
			flag2 = MetadataHelpers.GetFacetValueOrDefault(type, MetadataHelpers.FixedLengthFacetName, defaultValue: false);
			num = MetadataHelpers.GetFacetValueOrDefault(type, MetadataHelpers.MaxLengthFacetName, int.MinValue);
			text = ((num != int.MinValue) ? num.ToString(CultureInfo.InvariantCulture) : "max");
			if (flag && !flag2 && num > 4000)
			{
				text = "max";
			}
			if (!flag && !flag2 && num > 8000)
			{
				text = "max";
			}
			if (flag2)
			{
				return (flag ? "nchar(" : "char(") + text + ")";
			}
			return (flag ? "nvarchar(" : "varchar(") + text + ")";
		case 3:
			return MetadataHelpers.GetFacetValueOrDefault(type, MetadataHelpers.PreserveSecondsFacetName, defaultValue: false) ? "datetime" : "smalldatetime";
		case 4:
			b = MetadataHelpers.GetFacetValueOrDefault(type, MetadataHelpers.PrecisionFacetName, (byte)18);
			b2 = MetadataHelpers.GetFacetValueOrDefault(type, MetadataHelpers.ScaleFacetName, (byte)0);
			return name + "(" + b + "," + b2 + ")";
		case 10:
			return "int";
		case 11:
			return "bigint";
		case 9:
			return "smallint";
		case 2:
			return "tinyint";
		case 1:
			return "bit";
		case 7:
			return "real";
		case 5:
			return "float";
		case 6:
			return "uniqueidentifier";
		default:
			throw new NotSupportedException("Unsupported EdmType: " + edmType.PrimitiveTypeKind);
		}
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

	private bool IsApplyExpression(DbExpression e)
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

	private bool IsKeyForIn(DbExpression e)
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

	private bool IsJoinExpression(DbExpression e)
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

	private bool IsComplexExpression(DbExpression e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		DbExpressionKind expressionKind = e.ExpressionKind;
		if ((int)expressionKind == 5 || (int)expressionKind == 43 || (int)expressionKind == 46)
		{
			return false;
		}
		return true;
	}

	private bool IsCompatible(SqlSelectStatement result, DbExpressionKind expressionKind)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
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
					if (result.Top == null)
					{
						return result.OrderBy.IsEmpty;
					}
					return false;
				}
				if ((int)expressionKind == 11)
				{
					goto IL_00d6;
				}
			}
			else
			{
				if ((int)expressionKind == 15)
				{
					if (result.Select.IsEmpty && result.Where.IsEmpty && result.GroupBy.IsEmpty)
					{
						return result.Top == null;
					}
					return false;
				}
				if ((int)expressionKind == 20)
				{
					if (result.Select.IsEmpty && result.GroupBy.IsEmpty && result.OrderBy.IsEmpty)
					{
						return result.Top == null;
					}
					return false;
				}
			}
		}
		else if ((int)expressionKind <= 45)
		{
			if ((int)expressionKind == 31)
			{
				goto IL_00d6;
			}
			if ((int)expressionKind == 45)
			{
				if (result.Select.IsEmpty)
				{
					return result.GroupBy.IsEmpty;
				}
				return false;
			}
		}
		else
		{
			if ((int)expressionKind == 51)
			{
				if (result.Select.IsEmpty && result.GroupBy.IsEmpty && result.OrderBy.IsEmpty)
				{
					return !result.IsDistinct;
				}
				return false;
			}
			if ((int)expressionKind == 52)
			{
				if (result.Select.IsEmpty && result.GroupBy.IsEmpty)
				{
					return result.OrderBy.IsEmpty;
				}
				return false;
			}
		}
		throw new InvalidOperationException();
		IL_00d6:
		return result.Top == null;
	}

	private void ParenthesizeExpressionWithoutRedundantConstantCasts(DbExpression value, SqlBuilder sqlBuilder)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		if ((int)value.ExpressionKind == 5)
		{
			sqlBuilder.Append(((DbExpressionVisitor<ISqlFragment>)this).Visit((DbConstantExpression)value));
		}
		else
		{
			ParanthesizeExpressionIfNeeded(value, sqlBuilder);
		}
	}

	private void ParanthesizeExpressionIfNeeded(DbExpression e, SqlBuilder result)
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

	internal static string QuoteIdentifier(string name)
	{
		return "[" + name.Replace("]", "]]") + "]";
	}

	private bool TryAddExpressionForIn(DbBinaryExpression e, KeyToListMap<DbExpression, DbExpression> values)
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

	private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e)
	{
		return VisitExpressionEnsureSqlStatement(e, addDefaultColumns: true);
	}

	private SqlSelectStatement VisitExpressionEnsureSqlStatement(DbExpression e, bool addDefaultColumns)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Invalid comparison between Unknown and I4
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Invalid comparison between Unknown and I4
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Invalid comparison between Unknown and I4
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		DbExpressionKind expressionKind = e.ExpressionKind;
		if ((int)expressionKind <= 20)
		{
			if ((int)expressionKind == 15 || (int)expressionKind == 20)
			{
				goto IL_0022;
			}
		}
		else if ((int)expressionKind == 45 || (int)expressionKind == 52)
		{
			goto IL_0022;
		}
		string inputVarName = "c";
		symbolTable.EnterScope();
		TypeUsage val = null;
		expressionKind = e.ExpressionKind;
		if ((int)expressionKind <= 16)
		{
			if ((int)expressionKind == 6 || (int)expressionKind == 7 || (int)expressionKind == 16)
			{
				goto IL_007e;
			}
		}
		else if ((int)expressionKind <= 27)
		{
			if ((int)expressionKind == 21 || (int)expressionKind == 27)
			{
				goto IL_007e;
			}
		}
		else if ((int)expressionKind == 42 || (int)expressionKind == 50)
		{
			goto IL_007e;
		}
		val = MetadataHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage;
		goto IL_009f;
		IL_009f:
		Symbol fromSymbol;
		SqlSelectStatement sqlSelectStatement = VisitInputExpression(e, inputVarName, val, out fromSymbol);
		AddFromSymbol(sqlSelectStatement, inputVarName, fromSymbol);
		symbolTable.ExitScope();
		goto IL_00c0;
		IL_00c0:
		if (addDefaultColumns && sqlSelectStatement.Select.IsEmpty)
		{
			AddDefaultColumns(sqlSelectStatement);
		}
		return sqlSelectStatement;
		IL_0022:
		sqlSelectStatement = e.Accept<ISqlFragment>((DbExpressionVisitor<ISqlFragment>)this) as SqlSelectStatement;
		goto IL_00c0;
		IL_007e:
		val = MetadataHelpers.GetElementTypeUsage(e.ResultType);
		goto IL_009f;
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

	private void WrapNonQueryExtent(SqlSelectStatement result, ISqlFragment sqlFragment, DbExpressionKind expressionKind)
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

	private static bool IsBuiltinFunction(EdmFunction function)
	{
		return MetadataHelpers.TryGetValueForMetadataProperty<bool>((MetadataItem)(object)function, "BuiltInAttribute");
	}

	private void WriteFunctionName(SqlBuilder result, EdmFunction function)
	{
		string text = MetadataHelpers.TryGetValueForMetadataProperty<string>((MetadataItem)(object)function, "StoreFunctionNameAttribute");
		if (string.IsNullOrEmpty(text))
		{
			text = ((EdmType)function).Name;
		}
		if (IsBuiltinFunction(function))
		{
			if (((EdmType)function).NamespaceName == "Edm")
			{
				text.ToUpperInvariant();
				result.Append(text);
			}
			else
			{
				result.Append(text);
			}
		}
		else
		{
			result.Append(QuoteIdentifier(text));
		}
	}

	private static void ToBlobLiteral(byte[] bytes, SqlBuilder builder)
	{
		if (builder == null)
		{
			throw new ArgumentNullException("builder");
		}
		if (bytes == null)
		{
			builder.Append("NULL");
			return;
		}
		builder.Append(" X'");
		for (int i = 0; i < bytes.Length; i++)
		{
			builder.Append(hexDigits[(bytes[i] & 0xF0) >> 4]);
			builder.Append(hexDigits[bytes[i] & 0xF]);
		}
		builder.Append("' ");
	}

	private static bool NeedsInnerQuery(IList<DbAggregate> aggregates)
	{
		foreach (DbAggregate aggregate in aggregates)
		{
			if (!IsPropertyOverVarRef(aggregate.Arguments[0]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsPropertyOverVarRef(DbExpression expression)
	{
		DbPropertyExpression val = (DbPropertyExpression)(object)((expression is DbPropertyExpression) ? expression : null);
		if (val == null)
		{
			return false;
		}
		if (!(val.Instance is DbVariableReferenceExpression))
		{
			return false;
		}
		return true;
	}
}
