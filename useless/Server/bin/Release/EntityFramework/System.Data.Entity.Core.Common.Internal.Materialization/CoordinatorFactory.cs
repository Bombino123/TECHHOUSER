using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Core.Objects.Internal;
using System.Linq.Expressions;
using System.Text;

namespace System.Data.Entity.Core.Common.Internal.Materialization;

internal abstract class CoordinatorFactory
{
	private static readonly Func<Shaper, bool> _alwaysTrue = (Shaper s) => true;

	private static readonly Func<Shaper, bool> _alwaysFalse = (Shaper s) => false;

	internal readonly int Depth;

	internal readonly int StateSlot;

	internal readonly Func<Shaper, bool> HasData;

	internal readonly Func<Shaper, bool> SetKeys;

	internal readonly Func<Shaper, bool> CheckKeys;

	internal readonly ReadOnlyCollection<CoordinatorFactory> NestedCoordinators;

	internal readonly bool IsLeafResult;

	internal readonly bool IsSimple;

	internal readonly ReadOnlyCollection<RecordStateFactory> RecordStateFactories;

	protected CoordinatorFactory(int depth, int stateSlot, Func<Shaper, bool> hasData, Func<Shaper, bool> setKeys, Func<Shaper, bool> checkKeys, CoordinatorFactory[] nestedCoordinators, RecordStateFactory[] recordStateFactories)
	{
		Depth = depth;
		StateSlot = stateSlot;
		IsLeafResult = nestedCoordinators.Length == 0;
		if (hasData == null)
		{
			HasData = _alwaysTrue;
		}
		else
		{
			HasData = hasData;
		}
		if (setKeys == null)
		{
			SetKeys = _alwaysTrue;
		}
		else
		{
			SetKeys = setKeys;
		}
		if (checkKeys == null)
		{
			if (IsLeafResult)
			{
				CheckKeys = _alwaysFalse;
			}
			else
			{
				CheckKeys = _alwaysTrue;
			}
		}
		else
		{
			CheckKeys = checkKeys;
		}
		NestedCoordinators = new ReadOnlyCollection<CoordinatorFactory>(nestedCoordinators);
		RecordStateFactories = new ReadOnlyCollection<RecordStateFactory>(recordStateFactories);
		IsSimple = IsLeafResult && checkKeys == null && hasData == null;
	}

	internal abstract Coordinator CreateCoordinator(Coordinator parent, Coordinator next);
}
internal class CoordinatorFactory<TElement> : CoordinatorFactory
{
	internal readonly Func<Shaper, IEntityWrapper> WrappedElement;

	internal readonly Func<Shaper, TElement> Element;

	internal readonly Func<Shaper, TElement> ElementWithErrorHandling;

	internal readonly Func<Shaper, ICollection<TElement>> InitializeCollection;

	private readonly string Description;

	internal CoordinatorFactory(int depth, int stateSlot, Expression<Func<Shaper, bool>> hasData, Expression<Func<Shaper, bool>> setKeys, Expression<Func<Shaper, bool>> checkKeys, CoordinatorFactory[] nestedCoordinators, Expression<Func<Shaper, TElement>> element, Expression<Func<Shaper, IEntityWrapper>> wrappedElement, Expression<Func<Shaper, TElement>> elementWithErrorHandling, Expression<Func<Shaper, ICollection<TElement>>> initializeCollection, RecordStateFactory[] recordStateFactories)
		: base(depth, stateSlot, CompilePredicate(hasData), CompilePredicate(setKeys), CompilePredicate(checkKeys), nestedCoordinators, recordStateFactories)
	{
		WrappedElement = wrappedElement?.Compile();
		Element = element?.Compile();
		ElementWithErrorHandling = elementWithErrorHandling.Compile();
		InitializeCollection = ((initializeCollection == null) ? ((Func<Shaper, ICollection<TElement>>)((Shaper s) => new List<TElement>())) : initializeCollection.Compile());
		Description = new StringBuilder().Append("HasData: ").AppendLine(DescribeExpression(hasData)).Append("SetKeys: ")
			.AppendLine(DescribeExpression(setKeys))
			.Append("CheckKeys: ")
			.AppendLine(DescribeExpression(checkKeys))
			.Append("Element: ")
			.AppendLine((element == null) ? DescribeExpression(wrappedElement) : DescribeExpression(element))
			.Append("ElementWithExceptionHandling: ")
			.AppendLine(DescribeExpression(elementWithErrorHandling))
			.Append("InitializeCollection: ")
			.AppendLine(DescribeExpression(initializeCollection))
			.ToString();
	}

	public CoordinatorFactory(int depth, int stateSlot, Expression hasData, Expression setKeys, Expression checkKeys, CoordinatorFactory[] nestedCoordinators, Expression element, Expression elementWithErrorHandling, Expression initializeCollection, RecordStateFactory[] recordStateFactories)
		: this(depth, stateSlot, CodeGenEmitter.BuildShaperLambda<bool>(hasData), CodeGenEmitter.BuildShaperLambda<bool>(setKeys), CodeGenEmitter.BuildShaperLambda<bool>(checkKeys), nestedCoordinators, typeof(IEntityWrapper).IsAssignableFrom(element.Type) ? null : CodeGenEmitter.BuildShaperLambda<TElement>(element), typeof(IEntityWrapper).IsAssignableFrom(element.Type) ? CodeGenEmitter.BuildShaperLambda<IEntityWrapper>(element) : null, CodeGenEmitter.BuildShaperLambda<TElement>(typeof(IEntityWrapper).IsAssignableFrom(element.Type) ? CodeGenEmitter.Emit_UnwrapAndEnsureType(elementWithErrorHandling, typeof(TElement)) : elementWithErrorHandling), CodeGenEmitter.BuildShaperLambda<ICollection<TElement>>(initializeCollection), recordStateFactories)
	{
	}

	private static Func<Shaper, bool> CompilePredicate(Expression<Func<Shaper, bool>> predicate)
	{
		return predicate?.Compile();
	}

	private static string DescribeExpression(Expression expression)
	{
		if (expression == null)
		{
			return "undefined";
		}
		return expression.ToString();
	}

	internal override Coordinator CreateCoordinator(Coordinator parent, Coordinator next)
	{
		return new Coordinator<TElement>(this, parent, next);
	}

	internal RecordState GetDefaultRecordState(Shaper<RecordState> shaper)
	{
		RecordState recordState = null;
		if (RecordStateFactories.Count > 0)
		{
			recordState = (RecordState)shaper.State[RecordStateFactories[0].StateSlotNumber];
			recordState.ResetToDefaultState();
		}
		return recordState;
	}

	public override string ToString()
	{
		return Description;
	}
}
