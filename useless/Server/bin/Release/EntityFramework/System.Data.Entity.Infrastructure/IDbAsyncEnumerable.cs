namespace System.Data.Entity.Infrastructure;

public interface IDbAsyncEnumerable
{
	IDbAsyncEnumerator GetAsyncEnumerator();
}
public interface IDbAsyncEnumerable<out T> : IDbAsyncEnumerable
{
	new IDbAsyncEnumerator<T> GetAsyncEnumerator();
}
