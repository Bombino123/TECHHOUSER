using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Metadata.Edm.Provider;
using System.Data.Entity.Resources;
using System.Data.Entity.Utilities;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.ELinq;

internal sealed class Funcletizer
{
	private enum Mode
	{
		CompiledQueryLockdown,
		CompiledQueryEvaluation,
		ConventionalQuery
	}

	private sealed class FuncletizingVisitor : EntityExpressionVisitor
	{
		private readonly Funcletizer _funcletizer;

		private readonly Func<Expression, bool> _isClientConstant;

		private readonly Func<Expression, bool> _isClientVariable;

		private readonly List<Func<bool>> _recompileRequiredDelegates = new List<Func<bool>>();

		internal FuncletizingVisitor(Funcletizer funcletizer, Func<Expression, bool> isClientConstant, Func<Expression, bool> isClientVariable)
		{
			_funcletizer = funcletizer;
			_isClientConstant = isClientConstant;
			_isClientVariable = isClientVariable;
		}

		internal Func<bool> GetRecompileRequiredFunction()
		{
			ReadOnlyCollection<Func<bool>> recompileRequiredDelegates = new ReadOnlyCollection<Func<bool>>(_recompileRequiredDelegates);
			return () => recompileRequiredDelegates.Any((Func<bool> d) => d());
		}

		internal override Expression Visit(Expression exp)
		{
			if (exp != null)
			{
				if (!_funcletizer._linqExpressionStack.Add(exp))
				{
					throw new InvalidOperationException(Strings.ELinq_CycleDetected);
				}
				try
				{
					if (_isClientConstant(exp))
					{
						return InlineValue(exp, recompileOnChange: false);
					}
					if (_isClientVariable(exp))
					{
						if (_funcletizer.TryGetTypeUsageForTerminal(exp, out var typeUsage))
						{
							return new QueryParameterExpression(typeUsage.Parameter(_funcletizer.GenerateParameterName()), exp, _funcletizer._compiledQueryParameters);
						}
						if (_funcletizer.IsCompiledQuery)
						{
							throw InvalidCompiledQueryParameterException(exp);
						}
						return InlineValue(exp, recompileOnChange: true);
					}
					return base.Visit(exp);
				}
				finally
				{
					_funcletizer._linqExpressionStack.Remove(exp);
				}
			}
			return base.Visit(exp);
		}

		private static NotSupportedException InvalidCompiledQueryParameterException(Expression expression)
		{
			ParameterExpression parameterExpression;
			if (expression.NodeType == ExpressionType.Parameter)
			{
				parameterExpression = (ParameterExpression)expression;
			}
			else
			{
				HashSet<ParameterExpression> parameters = new HashSet<ParameterExpression>();
				EntityExpressionVisitor.Visit(expression, delegate(Expression exp, Func<Expression, Expression> baseVisit)
				{
					if (exp != null && exp.NodeType == ExpressionType.Parameter)
					{
						parameters.Add((ParameterExpression)exp);
					}
					return baseVisit(exp);
				});
				if (parameters.Count != 1)
				{
					return new NotSupportedException(Strings.CompiledELinq_UnsupportedParameterTypes(expression.Type.FullName));
				}
				parameterExpression = parameters.Single();
			}
			if (parameterExpression.Type.Equals(expression.Type))
			{
				return new NotSupportedException(Strings.CompiledELinq_UnsupportedNamedParameterType(parameterExpression.Name, parameterExpression.Type.FullName));
			}
			return new NotSupportedException(Strings.CompiledELinq_UnsupportedNamedParameterUseAsType(parameterExpression.Name, expression.Type.FullName));
		}

		private static Func<object> CompileExpression(Expression expression)
		{
			return Expression.Lambda<Func<object>>(TypeSystem.EnsureType(expression, typeof(object)), new ParameterExpression[0]).Compile();
		}

		private Expression InlineValue(Expression expression, bool recompileOnChange)
		{
			Func<object> func = null;
			object obj = null;
			if (expression.NodeType == ExpressionType.Constant)
			{
				obj = ((ConstantExpression)expression).Value;
			}
			else
			{
				bool flag = false;
				if (expression.NodeType == ExpressionType.Convert)
				{
					UnaryExpression unaryExpression = (UnaryExpression)expression;
					if (!recompileOnChange && unaryExpression.Operand.NodeType == ExpressionType.Constant && typeof(IQueryable).IsAssignableFrom(unaryExpression.Operand.Type))
					{
						obj = ((ConstantExpression)unaryExpression.Operand).Value;
						flag = true;
					}
				}
				if (!flag)
				{
					func = CompileExpression(expression);
					obj = func();
				}
			}
			Expression expression2 = null;
			ObjectQuery objectQuery = (obj as IQueryable).TryGetObjectQuery();
			expression2 = ((objectQuery != null) ? InlineObjectQuery(objectQuery, objectQuery.GetType()) : ((!(obj is LambdaExpression expression3)) ? ((expression.NodeType == ExpressionType.Constant) ? expression : Expression.Constant(obj, expression.Type)) : InlineExpression(Expression.Quote(expression3))));
			if (recompileOnChange)
			{
				AddRecompileRequiredDelegates(func, obj);
			}
			return expression2;
		}

		private void AddRecompileRequiredDelegates(Func<object> getValue, object value)
		{
			ObjectQuery originalQuery = (value as IQueryable).TryGetObjectQuery();
			if (originalQuery != null)
			{
				MergeOption? originalMergeOption = originalQuery.QueryState.UserSpecifiedMergeOption;
				if (getValue == null)
				{
					_recompileRequiredDelegates.Add(() => originalQuery.QueryState.UserSpecifiedMergeOption != originalMergeOption);
					return;
				}
				_recompileRequiredDelegates.Add(delegate
				{
					ObjectQuery objectQuery = (getValue() as IQueryable).TryGetObjectQuery();
					return originalQuery != objectQuery || objectQuery.QueryState.UserSpecifiedMergeOption != originalMergeOption;
				});
			}
			else if (getValue != null)
			{
				_recompileRequiredDelegates.Add(() => value != getValue());
			}
		}

		private Expression InlineObjectQuery(ObjectQuery inlineQuery, Type expressionType)
		{
			if (_funcletizer._mode == Mode.CompiledQueryLockdown)
			{
				return Expression.Constant(inlineQuery, expressionType);
			}
			if (_funcletizer._rootContext != inlineQuery.QueryState.ObjectContext)
			{
				throw new NotSupportedException(Strings.ELinq_UnsupportedDifferentContexts);
			}
			Expression expression = inlineQuery.GetExpression();
			if (!(inlineQuery.QueryState is EntitySqlQueryState))
			{
				expression = InlineExpression(expression);
			}
			return TypeSystem.EnsureType(expression, expressionType);
		}

		private Expression InlineExpression(Expression exp)
		{
			exp = _funcletizer.Funcletize(exp, out var recompileRequired);
			if (!_funcletizer.IsCompiledQuery)
			{
				_recompileRequiredDelegates.Add(recompileRequired);
			}
			return exp;
		}
	}

	private readonly ParameterExpression _rootContextParameter;

	private readonly ObjectContext _rootContext;

	private readonly ConstantExpression _rootContextExpression;

	private readonly ReadOnlyCollection<ParameterExpression> _compiledQueryParameters;

	private readonly Mode _mode;

	private readonly HashSet<Expression> _linqExpressionStack = new HashSet<Expression>();

	private const string s_parameterPrefix = "p__linq__";

	private long _parameterNumber;

	internal ObjectContext RootContext => _rootContext;

	internal ParameterExpression RootContextParameter => _rootContextParameter;

	internal ConstantExpression RootContextExpression => _rootContextExpression;

	internal bool IsCompiledQuery
	{
		get
		{
			if (_mode != Mode.CompiledQueryEvaluation)
			{
				return _mode == Mode.CompiledQueryLockdown;
			}
			return true;
		}
	}

	private Funcletizer(Mode mode, ObjectContext rootContext, ParameterExpression rootContextParameter, ReadOnlyCollection<ParameterExpression> compiledQueryParameters)
	{
		_mode = mode;
		_rootContext = rootContext;
		_rootContextParameter = rootContextParameter;
		_compiledQueryParameters = compiledQueryParameters;
		if (_rootContextParameter != null && _rootContext != null)
		{
			_rootContextExpression = Expression.Constant(_rootContext);
		}
	}

	internal static Funcletizer CreateCompiledQueryEvaluationFuncletizer(ObjectContext rootContext, ParameterExpression rootContextParameter, ReadOnlyCollection<ParameterExpression> compiledQueryParameters)
	{
		return new Funcletizer(Mode.CompiledQueryEvaluation, rootContext, rootContextParameter, compiledQueryParameters);
	}

	internal static Funcletizer CreateCompiledQueryLockdownFuncletizer()
	{
		return new Funcletizer(Mode.CompiledQueryLockdown, null, null, null);
	}

	internal static Funcletizer CreateQueryFuncletizer(ObjectContext rootContext)
	{
		return new Funcletizer(Mode.ConventionalQuery, rootContext, null, null);
	}

	internal Expression Funcletize(Expression expression, out Func<bool> recompileRequired)
	{
		expression = ReplaceRootContextParameter(expression);
		Func<Expression, bool> isClientConstant;
		Func<Expression, bool> isClientVariable;
		if (_mode == Mode.CompiledQueryEvaluation)
		{
			isClientConstant = Nominate(expression, IsClosureExpression);
			isClientVariable = Nominate(expression, IsCompiledQueryParameterVariable);
		}
		else if (_mode == Mode.CompiledQueryLockdown)
		{
			isClientConstant = Nominate(expression, IsClosureExpression);
			isClientVariable = (Expression exp) => false;
		}
		else
		{
			isClientConstant = Nominate(expression, IsImmutable);
			isClientVariable = Nominate(expression, IsClosureExpression);
		}
		FuncletizingVisitor funcletizingVisitor = new FuncletizingVisitor(this, isClientConstant, isClientVariable);
		Expression result = funcletizingVisitor.Visit(expression);
		recompileRequired = funcletizingVisitor.GetRecompileRequiredFunction();
		return result;
	}

	private Expression ReplaceRootContextParameter(Expression expression)
	{
		if (_rootContextExpression != null)
		{
			return EntityExpressionVisitor.Visit(expression, (Expression exp, Func<Expression, Expression> baseVisit) => (exp != _rootContextParameter) ? baseVisit(exp) : _rootContextExpression);
		}
		return expression;
	}

	private static Func<Expression, bool> Nominate(Expression expression, Func<Expression, bool> localCriterion)
	{
		HashSet<Expression> candidates = new HashSet<Expression>();
		bool cannotBeNominated = false;
		Func<Expression, Func<Expression, Expression>, Expression> visit = delegate(Expression exp, Func<Expression, Expression> baseVisit)
		{
			if (exp != null)
			{
				bool flag = cannotBeNominated;
				cannotBeNominated = false;
				baseVisit(exp);
				if (!cannotBeNominated)
				{
					if (localCriterion(exp))
					{
						candidates.Add(exp);
					}
					else
					{
						cannotBeNominated = true;
					}
				}
				cannotBeNominated |= flag;
			}
			return exp;
		};
		EntityExpressionVisitor.Visit(expression, visit);
		return candidates.Contains;
	}

	private bool IsImmutable(Expression expression)
	{
		if (expression == null)
		{
			return false;
		}
		switch (expression.NodeType)
		{
		case ExpressionType.New:
		{
			if (!ClrProviderManifest.Instance.TryGetPrimitiveType(TypeSystem.GetNonNullableType(expression.Type), out var _))
			{
				return false;
			}
			return true;
		}
		case ExpressionType.Constant:
			return true;
		case ExpressionType.NewArrayInit:
			return typeof(byte[]) == expression.Type;
		case ExpressionType.Convert:
			return true;
		default:
			return false;
		}
	}

	private bool IsClosureExpression(Expression expression)
	{
		if (expression == null)
		{
			return false;
		}
		if (IsImmutable(expression))
		{
			return true;
		}
		if (ExpressionType.MemberAccess == expression.NodeType)
		{
			MemberExpression memberExpression = (MemberExpression)expression;
			if (memberExpression.Member.MemberType == MemberTypes.Property)
			{
				return ExpressionConverter.CanFuncletizePropertyInfo((PropertyInfo)memberExpression.Member);
			}
			return true;
		}
		return false;
	}

	private bool IsCompiledQueryParameterVariable(Expression expression)
	{
		if (expression == null)
		{
			return false;
		}
		if (IsClosureExpression(expression))
		{
			return true;
		}
		if (ExpressionType.Parameter == expression.NodeType)
		{
			ParameterExpression value = (ParameterExpression)expression;
			return _compiledQueryParameters.Contains(value);
		}
		return false;
	}

	private bool TryGetTypeUsageForTerminal(Expression expression, out TypeUsage typeUsage)
	{
		Type type = expression.Type;
		if (_rootContext.Perspective.TryGetTypeByName(TypeSystem.GetNonNullableType(type).FullNameWithNesting(), ignoreCase: false, out typeUsage) && TypeSemantics.IsScalarType(typeUsage))
		{
			if (expression.NodeType == ExpressionType.Convert)
			{
				type = ((UnaryExpression)expression).Operand.Type;
			}
			if (type.IsValueType && Nullable.GetUnderlyingType(type) == null && TypeSemantics.IsNullable(typeUsage))
			{
				typeUsage = typeUsage.ShallowCopy(new FacetValues
				{
					Nullable = false
				});
			}
			return true;
		}
		typeUsage = null;
		return false;
	}

	internal string GenerateParameterName()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[2]
		{
			"p__linq__",
			_parameterNumber++
		});
	}
}
