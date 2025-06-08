using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Hierarchy;
using System.Data.Entity.Resources;
using System.Data.Entity.Spatial;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Text;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal;

internal sealed class ExpressionKeyGen : DbExpressionVisitor
{
	private readonly StringBuilder _key = new StringBuilder();

	private static readonly string[] _exprKindNames = InitializeExprKindNames();

	internal string Key => _key.ToString();

	internal static bool TryGenerateKey(DbExpression tree, out string key)
	{
		ExpressionKeyGen expressionKeyGen = new ExpressionKeyGen();
		try
		{
			tree.Accept(expressionKeyGen);
			key = expressionKeyGen._key.ToString();
			return true;
		}
		catch (NotSupportedException)
		{
			key = null;
			return false;
		}
	}

	internal ExpressionKeyGen()
	{
	}

	private static string[] InitializeExprKindNames()
	{
		string[] names = Enum.GetNames(typeof(DbExpressionKind));
		names[10] = "/";
		names[33] = "%";
		names[34] = "*";
		names[44] = "+";
		names[32] = "-";
		names[54] = "-";
		names[13] = "=";
		names[28] = "<";
		names[29] = "<=";
		names[18] = ">";
		names[19] = ">=";
		names[37] = "<>";
		names[46] = ".";
		names[21] = "IJ";
		names[16] = "FOJ";
		names[27] = "LOJ";
		names[6] = "CA";
		names[42] = "OA";
		return names;
	}

	private void VisitVariableName(string varName)
	{
		_key.Append('\'');
		_key.Append(varName.Replace("'", "''"));
		_key.Append('\'');
	}

	private void VisitBinding(DbExpressionBinding binding)
	{
		_key.Append("BV");
		VisitVariableName(binding.VariableName);
		_key.Append("=(");
		binding.Expression.Accept(this);
		_key.Append(')');
	}

	private void VisitGroupBinding(DbGroupExpressionBinding groupBinding)
	{
		_key.Append("GBVV");
		VisitVariableName(groupBinding.VariableName);
		_key.Append(",");
		VisitVariableName(groupBinding.GroupVariableName);
		_key.Append("=(");
		groupBinding.Expression.Accept(this);
		_key.Append(')');
	}

	private void VisitFunction(EdmFunction func, IList<DbExpression> args)
	{
		_key.Append("FUNC<");
		_key.Append(func.Identity);
		_key.Append(">:ARGS(");
		foreach (DbExpression arg in args)
		{
			_key.Append('(');
			arg.Accept(this);
			_key.Append(')');
		}
		_key.Append(')');
	}

	private void VisitExprKind(DbExpressionKind kind)
	{
		_key.Append('[');
		_key.Append(_exprKindNames[(int)kind]);
		_key.Append(']');
	}

	private void VisitUnary(DbUnaryExpression expr)
	{
		VisitExprKind(expr.ExpressionKind);
		_key.Append('(');
		expr.Argument.Accept(this);
		_key.Append(')');
	}

	private void VisitBinary(DbBinaryExpression expr)
	{
		VisitExprKind(expr.ExpressionKind);
		_key.Append('(');
		expr.Left.Accept(this);
		_key.Append(',');
		expr.Right.Accept(this);
		_key.Append(')');
	}

	private void VisitCastOrTreat(DbUnaryExpression e)
	{
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		e.Argument.Accept(this);
		_key.Append(":");
		_key.Append(e.ResultType.Identity);
		_key.Append(')');
	}

	public override void Visit(DbExpression e)
	{
		Check.NotNull(e, "e");
		throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(e.GetType().FullName));
	}

	public override void Visit(DbConstantExpression e)
	{
		Check.NotNull(e, "e");
		switch (((PrimitiveType)TypeHelpers.GetPrimitiveTypeUsageForScalar(e.ResultType).EdmType).PrimitiveTypeKind)
		{
		case PrimitiveTypeKind.Binary:
			if (e.Value is byte[] array)
			{
				_key.Append("'");
				byte[] array2 = array;
				foreach (byte b in array2)
				{
					_key.AppendFormat("{0:X2}", b);
				}
				_key.Append("'");
				break;
			}
			throw new NotSupportedException();
		case PrimitiveTypeKind.String:
			if (e.Value is string text)
			{
				_key.Append("'");
				_key.Append(text.Replace("'", "''"));
				_key.Append("'");
				break;
			}
			throw new NotSupportedException();
		case PrimitiveTypeKind.Boolean:
		case PrimitiveTypeKind.Byte:
		case PrimitiveTypeKind.Decimal:
		case PrimitiveTypeKind.Double:
		case PrimitiveTypeKind.Guid:
		case PrimitiveTypeKind.Single:
		case PrimitiveTypeKind.SByte:
		case PrimitiveTypeKind.Int16:
		case PrimitiveTypeKind.Int32:
		case PrimitiveTypeKind.Int64:
		case PrimitiveTypeKind.Time:
			_key.AppendFormat(CultureInfo.InvariantCulture, "{0}", new object[1] { e.Value });
			break;
		case PrimitiveTypeKind.HierarchyId:
		{
			HierarchyId hierarchyId = e.Value as HierarchyId;
			if (hierarchyId != null)
			{
				_key.Append(hierarchyId);
				break;
			}
			throw new NotSupportedException();
		}
		case PrimitiveTypeKind.DateTime:
			_key.Append(((DateTime)e.Value).ToString("o", CultureInfo.InvariantCulture));
			break;
		case PrimitiveTypeKind.DateTimeOffset:
			_key.Append(((DateTimeOffset)e.Value).ToString("o", CultureInfo.InvariantCulture));
			break;
		case PrimitiveTypeKind.Geometry:
		case PrimitiveTypeKind.GeometryPoint:
		case PrimitiveTypeKind.GeometryLineString:
		case PrimitiveTypeKind.GeometryPolygon:
		case PrimitiveTypeKind.GeometryMultiPoint:
		case PrimitiveTypeKind.GeometryMultiLineString:
		case PrimitiveTypeKind.GeometryMultiPolygon:
		case PrimitiveTypeKind.GeometryCollection:
			if (e.Value is DbGeometry dbGeometry)
			{
				_key.Append(dbGeometry.AsText());
				break;
			}
			throw new NotSupportedException();
		case PrimitiveTypeKind.Geography:
		case PrimitiveTypeKind.GeographyPoint:
		case PrimitiveTypeKind.GeographyLineString:
		case PrimitiveTypeKind.GeographyPolygon:
		case PrimitiveTypeKind.GeographyMultiPoint:
		case PrimitiveTypeKind.GeographyMultiLineString:
		case PrimitiveTypeKind.GeographyMultiPolygon:
		case PrimitiveTypeKind.GeographyCollection:
			if (e.Value is DbGeography dbGeography)
			{
				_key.Append(dbGeography.AsText());
				break;
			}
			throw new NotSupportedException();
		default:
			throw new NotSupportedException();
		}
		_key.Append(":");
		_key.Append(e.ResultType.Identity);
	}

	public override void Visit(DbNullExpression e)
	{
		Check.NotNull(e, "e");
		_key.Append("NULL:");
		_key.Append(e.ResultType.Identity);
	}

	public override void Visit(DbVariableReferenceExpression e)
	{
		Check.NotNull(e, "e");
		_key.Append("Var(");
		VisitVariableName(e.VariableName);
		_key.Append(")");
	}

	public override void Visit(DbParameterReferenceExpression e)
	{
		Check.NotNull(e, "e");
		_key.Append("@");
		_key.Append(e.ParameterName);
		_key.Append(":");
		_key.Append(e.ResultType.Identity);
	}

	public override void Visit(DbFunctionExpression e)
	{
		Check.NotNull(e, "e");
		VisitFunction(e.Function, e.Arguments);
	}

	public override void Visit(DbLambdaExpression expression)
	{
		Check.NotNull(expression, "expression");
		_key.Append("Lambda(");
		foreach (DbVariableReferenceExpression variable in expression.Lambda.Variables)
		{
			_key.Append("(V");
			VisitVariableName(variable.VariableName);
			_key.Append(":");
			_key.Append(variable.ResultType.Identity);
			_key.Append(')');
		}
		_key.Append("=");
		foreach (DbExpression argument in expression.Arguments)
		{
			_key.Append('(');
			argument.Accept(this);
			_key.Append(')');
		}
		_key.Append(")Body(");
		expression.Lambda.Body.Accept(this);
		_key.Append(")");
	}

	public override void Visit(DbPropertyExpression e)
	{
		Check.NotNull(e, "e");
		e.Instance.Accept(this);
		VisitExprKind(e.ExpressionKind);
		_key.Append(e.Property.Name);
	}

	public override void Visit(DbComparisonExpression e)
	{
		Check.NotNull(e, "e");
		VisitBinary(e);
	}

	public override void Visit(DbLikeExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		e.Argument.Accept(this);
		_key.Append(")(");
		e.Pattern.Accept(this);
		_key.Append(")(");
		if (e.Escape != null)
		{
			e.Escape.Accept(this);
		}
		e.Argument.Accept(this);
		_key.Append(')');
	}

	public override void Visit(DbLimitExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		if (e.WithTies)
		{
			_key.Append("WithTies");
		}
		_key.Append('(');
		e.Argument.Accept(this);
		_key.Append(")(");
		e.Limit.Accept(this);
		_key.Append(')');
	}

	public override void Visit(DbIsNullExpression e)
	{
		Check.NotNull(e, "e");
		VisitUnary(e);
	}

	public override void Visit(DbArithmeticExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		foreach (DbExpression argument in e.Arguments)
		{
			_key.Append('(');
			argument.Accept(this);
			_key.Append(')');
		}
	}

	public override void Visit(DbAndExpression e)
	{
		Check.NotNull(e, "e");
		VisitBinary(e);
	}

	public override void Visit(DbOrExpression e)
	{
		Check.NotNull(e, "e");
		VisitBinary(e);
	}

	public override void Visit(DbInExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		e.Item.Accept(this);
		_key.Append(",(");
		bool flag = true;
		foreach (DbExpression item in e.List)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				_key.Append(',');
			}
			item.Accept(this);
		}
		_key.Append("))");
	}

	public override void Visit(DbNotExpression e)
	{
		Check.NotNull(e, "e");
		VisitUnary(e);
	}

	public override void Visit(DbDistinctExpression e)
	{
		Check.NotNull(e, "e");
		VisitUnary(e);
	}

	public override void Visit(DbElementExpression e)
	{
		Check.NotNull(e, "e");
		VisitUnary(e);
	}

	public override void Visit(DbIsEmptyExpression e)
	{
		Check.NotNull(e, "e");
		VisitUnary(e);
	}

	public override void Visit(DbUnionAllExpression e)
	{
		Check.NotNull(e, "e");
		VisitBinary(e);
	}

	public override void Visit(DbIntersectExpression e)
	{
		Check.NotNull(e, "e");
		VisitBinary(e);
	}

	public override void Visit(DbExceptExpression e)
	{
		Check.NotNull(e, "e");
		VisitBinary(e);
	}

	public override void Visit(DbTreatExpression e)
	{
		Check.NotNull(e, "e");
		VisitCastOrTreat(e);
	}

	public override void Visit(DbCastExpression e)
	{
		Check.NotNull(e, "e");
		VisitCastOrTreat(e);
	}

	public override void Visit(DbIsOfExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		e.Argument.Accept(this);
		_key.Append(":");
		_key.Append(e.OfType.EdmType.Identity);
		_key.Append(')');
	}

	public override void Visit(DbOfTypeExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		e.Argument.Accept(this);
		_key.Append(":");
		_key.Append(e.OfType.EdmType.Identity);
		_key.Append(')');
	}

	public override void Visit(DbCaseExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		for (int i = 0; i < e.When.Count; i++)
		{
			_key.Append("WHEN:(");
			e.When[i].Accept(this);
			_key.Append(")THEN:(");
			e.Then[i].Accept(this);
		}
		_key.Append("ELSE:(");
		e.Else.Accept(this);
		_key.Append("))");
	}

	public override void Visit(DbNewInstanceExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append(':');
		_key.Append(e.ResultType.EdmType.Identity);
		_key.Append('(');
		foreach (DbExpression argument in e.Arguments)
		{
			_key.Append('(');
			argument.Accept(this);
			_key.Append(')');
		}
		if (e.HasRelatedEntityReferences)
		{
			foreach (DbRelatedEntityRef relatedEntityReference in e.RelatedEntityReferences)
			{
				_key.Append("RE(A(");
				_key.Append(relatedEntityReference.SourceEnd.DeclaringType.Identity);
				_key.Append(")(");
				_key.Append(relatedEntityReference.SourceEnd.Name);
				_key.Append("->");
				_key.Append(relatedEntityReference.TargetEnd.Name);
				_key.Append(")(");
				relatedEntityReference.TargetEntityReference.Accept(this);
				_key.Append("))");
			}
		}
		_key.Append(')');
	}

	public override void Visit(DbRefExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append("(ESET(");
		_key.Append(e.EntitySet.EntityContainer.Name);
		_key.Append('.');
		_key.Append(e.EntitySet.Name);
		_key.Append(")T(");
		_key.Append(TypeHelpers.GetEdmType<RefType>(e.ResultType).ElementType.FullName);
		_key.Append(")(");
		e.Argument.Accept(this);
		_key.Append(')');
	}

	public override void Visit(DbRelationshipNavigationExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		e.NavigationSource.Accept(this);
		_key.Append(")A(");
		_key.Append(e.NavigateFrom.DeclaringType.Identity);
		_key.Append(")(");
		_key.Append(e.NavigateFrom.Name);
		_key.Append("->");
		_key.Append(e.NavigateTo.Name);
		_key.Append("))");
	}

	public override void Visit(DbDerefExpression e)
	{
		Check.NotNull(e, "e");
		VisitUnary(e);
	}

	public override void Visit(DbRefKeyExpression e)
	{
		Check.NotNull(e, "e");
		VisitUnary(e);
	}

	public override void Visit(DbEntityRefExpression e)
	{
		Check.NotNull(e, "e");
		VisitUnary(e);
	}

	public override void Visit(DbScanExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		_key.Append(e.Target.EntityContainer.Name);
		_key.Append('.');
		_key.Append(e.Target.Name);
		_key.Append(':');
		_key.Append(e.ResultType.EdmType.Identity);
		_key.Append(')');
	}

	public override void Visit(DbFilterExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		VisitBinding(e.Input);
		_key.Append('(');
		e.Predicate.Accept(this);
		_key.Append("))");
	}

	public override void Visit(DbProjectExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		VisitBinding(e.Input);
		_key.Append('(');
		e.Projection.Accept(this);
		_key.Append("))");
	}

	public override void Visit(DbCrossJoinExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		foreach (DbExpressionBinding input in e.Inputs)
		{
			VisitBinding(input);
		}
		_key.Append(')');
	}

	public override void Visit(DbJoinExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		VisitBinding(e.Left);
		VisitBinding(e.Right);
		_key.Append('(');
		e.JoinCondition.Accept(this);
		_key.Append("))");
	}

	public override void Visit(DbApplyExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		VisitBinding(e.Input);
		VisitBinding(e.Apply);
		_key.Append(')');
	}

	public override void Visit(DbGroupByExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		VisitGroupBinding(e.Input);
		foreach (DbExpression key in e.Keys)
		{
			_key.Append("K(");
			key.Accept(this);
			_key.Append(')');
		}
		foreach (DbAggregate aggregate in e.Aggregates)
		{
			if (aggregate is DbGroupAggregate dbGroupAggregate)
			{
				_key.Append("GA(");
				dbGroupAggregate.Arguments[0].Accept(this);
				_key.Append(')');
				continue;
			}
			_key.Append("A:");
			DbFunctionAggregate dbFunctionAggregate = (DbFunctionAggregate)aggregate;
			if (dbFunctionAggregate.Distinct)
			{
				_key.Append("D:");
			}
			VisitFunction(dbFunctionAggregate.Function, dbFunctionAggregate.Arguments);
		}
		_key.Append(')');
	}

	private void VisitSortOrder(IList<DbSortClause> sortOrder)
	{
		_key.Append("SO(");
		foreach (DbSortClause item in sortOrder)
		{
			_key.Append(item.Ascending ? "ASC(" : "DESC(");
			item.Expression.Accept(this);
			_key.Append(')');
			if (!string.IsNullOrEmpty(item.Collation))
			{
				_key.Append(":(");
				_key.Append(item.Collation);
				_key.Append(')');
			}
		}
		_key.Append(')');
	}

	public override void Visit(DbSkipExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		VisitBinding(e.Input);
		VisitSortOrder(e.SortOrder);
		_key.Append('(');
		e.Count.Accept(this);
		_key.Append("))");
	}

	public override void Visit(DbSortExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		VisitBinding(e.Input);
		VisitSortOrder(e.SortOrder);
		_key.Append(')');
	}

	public override void Visit(DbQuantifierExpression e)
	{
		Check.NotNull(e, "e");
		VisitExprKind(e.ExpressionKind);
		_key.Append('(');
		VisitBinding(e.Input);
		_key.Append('(');
		e.Predicate.Accept(this);
		_key.Append("))");
	}
}
