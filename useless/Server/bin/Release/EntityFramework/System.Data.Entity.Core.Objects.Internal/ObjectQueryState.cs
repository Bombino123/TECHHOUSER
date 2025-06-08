using System.Data.Entity.Core.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Utilities;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.Internal;

internal abstract class ObjectQueryState
{
	internal static readonly MergeOption DefaultMergeOption = MergeOption.AppendOnly;

	internal static readonly MethodInfo CreateObjectQueryMethod = typeof(ObjectQueryState).GetOnlyDeclaredMethod("CreateObjectQuery");

	private readonly ObjectContext _context;

	private readonly Type _elementType;

	private ObjectParameterCollection _parameters;

	private readonly Span _span;

	private MergeOption? _userMergeOption;

	private bool _cachingEnabled = true;

	protected ObjectQueryExecutionPlan _cachedPlan;

	internal bool EffectiveStreamingBehavior => UserSpecifiedStreamingBehavior ?? DefaultStreamingBehavior;

	internal bool? UserSpecifiedStreamingBehavior { get; set; }

	internal bool DefaultStreamingBehavior => !(ExecutionStrategy ?? DbProviderServices.GetExecutionStrategy(ObjectContext.Connection, ObjectContext.MetadataWorkspace)).RetriesOnFailure;

	internal IDbExecutionStrategy ExecutionStrategy { get; set; }

	internal Type ElementType => _elementType;

	internal ObjectContext ObjectContext => _context;

	internal ObjectParameterCollection Parameters => _parameters;

	internal Span Span => _span;

	internal MergeOption EffectiveMergeOption
	{
		get
		{
			if (_userMergeOption.HasValue)
			{
				return _userMergeOption.Value;
			}
			return _cachedPlan?.MergeOption ?? DefaultMergeOption;
		}
	}

	internal MergeOption? UserSpecifiedMergeOption
	{
		get
		{
			return _userMergeOption;
		}
		set
		{
			_userMergeOption = value;
		}
	}

	internal bool PlanCachingEnabled
	{
		get
		{
			return _cachingEnabled;
		}
		set
		{
			_cachingEnabled = value;
		}
	}

	internal TypeUsage ResultType
	{
		get
		{
			ObjectQueryExecutionPlan cachedPlan = _cachedPlan;
			if (cachedPlan != null)
			{
				return cachedPlan.ResultType;
			}
			return GetResultType();
		}
	}

	protected ObjectQueryState(Type elementType, ObjectContext context, ObjectParameterCollection parameters, Span span)
	{
		_elementType = elementType;
		_context = context;
		_span = span;
		_parameters = parameters;
	}

	protected ObjectQueryState(Type elementType, ObjectQuery query)
		: this(elementType, query.Context, null, null)
	{
		_cachingEnabled = query.EnablePlanCaching;
		UserSpecifiedStreamingBehavior = query.QueryState.UserSpecifiedStreamingBehavior;
		ExecutionStrategy = query.QueryState.ExecutionStrategy;
	}

	internal ObjectParameterCollection EnsureParameters()
	{
		if (_parameters == null)
		{
			_parameters = new ObjectParameterCollection(ObjectContext.Perspective);
			if (_cachedPlan != null)
			{
				_parameters.SetReadOnly(isReadOnly: true);
			}
		}
		return _parameters;
	}

	internal void ApplySettingsTo(ObjectQueryState other)
	{
		other.PlanCachingEnabled = PlanCachingEnabled;
		other.UserSpecifiedMergeOption = UserSpecifiedMergeOption;
	}

	internal abstract bool TryGetCommandText(out string commandText);

	internal abstract bool TryGetExpression(out Expression expression);

	internal abstract ObjectQueryExecutionPlan GetExecutionPlan(MergeOption? forMergeOption);

	internal abstract ObjectQueryState Include<TElementType>(ObjectQuery<TElementType> sourceQuery, string includePath);

	protected abstract TypeUsage GetResultType();

	protected static MergeOption EnsureMergeOption(params MergeOption?[] preferredMergeOptions)
	{
		for (int i = 0; i < preferredMergeOptions.Length; i++)
		{
			MergeOption? mergeOption = preferredMergeOptions[i];
			if (mergeOption.HasValue)
			{
				return mergeOption.Value;
			}
		}
		return DefaultMergeOption;
	}

	protected static MergeOption? GetMergeOption(params MergeOption?[] preferredMergeOptions)
	{
		for (int i = 0; i < preferredMergeOptions.Length; i++)
		{
			MergeOption? mergeOption = preferredMergeOptions[i];
			if (mergeOption.HasValue)
			{
				return mergeOption.Value;
			}
		}
		return null;
	}

	public ObjectQuery CreateQuery()
	{
		return (ObjectQuery)CreateObjectQueryMethod.MakeGenericMethod(_elementType).Invoke(this, new object[0]);
	}

	public ObjectQuery<TResultType> CreateObjectQuery<TResultType>()
	{
		return new ObjectQuery<TResultType>(this);
	}
}
