using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal class CoordinatorScratchpad
{
	private class ReplacementExpressionVisitor : EntityExpressionVisitor
	{
		private readonly Dictionary<Expression, Expression> _replacementDictionary;

		private readonly HashSet<LambdaExpression> _inlineDelegates;

		internal ReplacementExpressionVisitor(Dictionary<Expression, Expression> replacementDictionary, HashSet<LambdaExpression> inlineDelegates)
		{
			_replacementDictionary = replacementDictionary;
			_inlineDelegates = inlineDelegates;
		}

		internal override Expression Visit(Expression expression)
		{
			if (expression == null)
			{
				return expression;
			}
			if (_replacementDictionary != null && _replacementDictionary.TryGetValue(expression, out var value))
			{
				return value;
			}
			bool flag = false;
			LambdaExpression lambdaExpression = null;
			if (expression.NodeType == ExpressionType.Lambda && _inlineDelegates != null)
			{
				lambdaExpression = (LambdaExpression)expression;
				flag = _inlineDelegates.Contains(lambdaExpression);
			}
			if (flag)
			{
				Expression expression2 = Visit(lambdaExpression.Body);
				return Expression.Constant(CodeGenEmitter.Compile(expression2.Type, expression2));
			}
			return base.Visit(expression);
		}
	}

	private readonly Type _elementType;

	private CoordinatorScratchpad _parent;

	private readonly List<CoordinatorScratchpad> _nestedCoordinatorScratchpads;

	private readonly Dictionary<Expression, Expression> _expressionWithErrorHandlingMap;

	private readonly HashSet<LambdaExpression> _inlineDelegates;

	private List<RecordStateScratchpad> _recordStateScratchpads;

	internal CoordinatorScratchpad Parent => _parent;

	internal Expression SetKeys { get; set; }

	internal Expression CheckKeys { get; set; }

	internal Expression HasData { get; set; }

	internal Expression Element { get; set; }

	internal Expression InitializeCollection { get; set; }

	internal int StateSlotNumber { get; set; }

	internal int Depth { get; set; }

	internal CoordinatorScratchpad(Type elementType)
	{
		_elementType = elementType;
		_nestedCoordinatorScratchpads = new List<CoordinatorScratchpad>();
		_expressionWithErrorHandlingMap = new Dictionary<Expression, Expression>();
		_inlineDelegates = new HashSet<LambdaExpression>();
	}

	internal void AddExpressionWithErrorHandling(Expression expression, Expression expressionWithErrorHandling)
	{
		_expressionWithErrorHandlingMap[expression] = expressionWithErrorHandling;
	}

	internal void AddInlineDelegate(LambdaExpression expression)
	{
		_inlineDelegates.Add(expression);
	}

	internal void AddNestedCoordinator(CoordinatorScratchpad nested)
	{
		nested._parent = this;
		_nestedCoordinatorScratchpads.Add(nested);
	}

	internal CoordinatorFactory Compile()
	{
		RecordStateFactory[] array;
		if (_recordStateScratchpads != null)
		{
			array = new RecordStateFactory[_recordStateScratchpads.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _recordStateScratchpads[i].Compile();
			}
		}
		else
		{
			array = new RecordStateFactory[0];
		}
		CoordinatorFactory[] array2 = new CoordinatorFactory[_nestedCoordinatorScratchpads.Count];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = _nestedCoordinatorScratchpads[j].Compile();
		}
		Expression expression = new ReplacementExpressionVisitor(null, _inlineDelegates).Visit(Element);
		Expression expression2 = new ReplacementExpressionVisitor(_expressionWithErrorHandlingMap, _inlineDelegates).Visit(Element);
		return (CoordinatorFactory)Activator.CreateInstance(typeof(CoordinatorFactory<>).MakeGenericType(_elementType), Depth, StateSlotNumber, HasData, SetKeys, CheckKeys, array2, expression, expression2, InitializeCollection, array);
	}

	internal RecordStateScratchpad CreateRecordStateScratchpad()
	{
		RecordStateScratchpad recordStateScratchpad = new RecordStateScratchpad();
		if (_recordStateScratchpads == null)
		{
			_recordStateScratchpads = new List<RecordStateScratchpad>();
		}
		_recordStateScratchpads.Add(recordStateScratchpad);
		return recordStateScratchpad;
	}
}
