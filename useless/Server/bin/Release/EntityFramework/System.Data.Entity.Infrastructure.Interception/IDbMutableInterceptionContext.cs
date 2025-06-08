namespace System.Data.Entity.Infrastructure.Interception;

internal interface IDbMutableInterceptionContext
{
	InterceptionContextMutableData MutableData { get; }
}
internal interface IDbMutableInterceptionContext<TResult> : IDbMutableInterceptionContext
{
	new InterceptionContextMutableData<TResult> MutableData { get; }
}
